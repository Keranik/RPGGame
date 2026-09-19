using RPGGame.Core.Combat;
using RPGGame.Core.Effects;
using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Characters;

/// <summary>
/// Single source of truth for all LiveCharacter stat queries and modifications.
/// All gameplay systems should go through CharacterManager to interact with characters.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class CharacterManager {
	private readonly GameDb k_gameDb;

	public CharacterManager(GameDb gameDb) {
		k_gameDb = gameDb;
	}

	// ═══════════════════════════════════════════════════════════════════════
	// STAT QUERIES
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Gets a stat value with all modifiers applied.</summary>
	public float GetStat(LiveCharacter character, StatProto.ID statId) {
		float baseValue = character.BaseStats.Get(statId);
		var modifiers = character.Modifiers
			.Where(m => m.StatId == statId && m.IsActive && !m.IsExpired)
			.Select(m => m.ToValueModifier())
			.ToList();
		return ModifierCalculator.Calculate(baseValue, modifiers);
	}

	/// <summary>Gets a stat as an integer.</summary>
	public int GetStatInt(LiveCharacter character, StatProto.ID statId) {
		return (int)GetStat(character, statId);
	}

	/// <summary>Gets the D&D-style modifier for an attribute (STR, DEX, etc.).</summary>
	public int GetAttributeModifier(LiveCharacter character, StatProto.ID attributeId) {
		return StatManager.GetAttributeModifier(GetStatInt(character, attributeId));
	}

	/// <summary>Gets resistance to a damage type (0-1).</summary>
	public float GetResistance(LiveCharacter character, DamageType damageType) {
		var statId = MapDamageTypeToResistanceStat(damageType);
		return GetStat(character, statId) / 100f;
	}

	/// <summary>Gets vulnerability to a damage type (0+).</summary>
	public float GetVulnerability(LiveCharacter character, DamageType damageType) {
		var statId = MapDamageTypeToVulnerabilityStat(damageType);
		return GetStat(character, statId) / 100f;
	}

	// ═══════════════════════════════════════════════════════════════════════
	// STAT MODIFICATIONS
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Sets a base stat value directly.</summary>
	public void SetBaseStat(LiveCharacter character, StatProto.ID statId, float value) {
		character.BaseStats.Set(statId, value);
	}

	/// <summary>Adds to a base stat value.</summary>
	public void AddToBaseStat(LiveCharacter character, StatProto.ID statId, float amount) {
		float current = character.BaseStats.Get(statId);
		character.BaseStats.Set(statId, current + amount);
	}

	/// <summary>Adds a permanent modifier.</summary>
	public void AddPermanentModifier(
		LiveCharacter character,
		StatProto.ID statId,
		float value,
		string source,
		ModifierOperation operation = ModifierOperation.FlatAdd
	) {
		var modifier = StatModifier.CreatePermanent(statId, value, source, operation);
		character.Modifiers.Add(modifier);
	}

	/// <summary>Adds a temporary modifier with duration in ticks.</summary>
	public void AddTemporaryModifier(
		LiveCharacter character,
		StatProto.ID statId,
		float value,
		string source,
		Duration inTicks,
		ModifierOperation operation = ModifierOperation.FlatAdd
	) {
		var modifier = StatModifier.CreateTemporary(statId, value, source, inTicks, operation);
		character.Modifiers.Add(modifier);
	}

	/// <summary>Adds an equipment modifier.</summary>
	public void AddEquipmentModifier(
		LiveCharacter character,
		StatProto.ID statId,
		float value,
		string itemId,
		ModifierOperation operation = ModifierOperation.FlatAdd
	) {
		var modifier = StatModifier.CreateEquipment(statId, value, itemId, operation);
		character.Modifiers.Add(modifier);
	}

	/// <summary>Removes all modifiers from a specific source.</summary>
	public int RemoveModifiersBySource(LiveCharacter character, string source) {
		return character.Modifiers.RemoveAll(m => m.Source == source);
	}

	/// <summary>Removes all equipment modifiers for an item.</summary>
	public int RemoveEquipmentModifiers(LiveCharacter character, string itemId) {
		return character.Modifiers.RemoveAll(m =>
			m.Duration == StatModifierDuration.Equipment && m.Source == itemId);
	}

	/// <summary>Ticks all temporary modifiers and removes expired ones.</summary>
	public int TickModifiers(LiveCharacter character) {
		int expiredCount = 0;
		foreach (var modifier in character.Modifiers) {
			if (!modifier.Tick()) {
				expiredCount++;
			}
		}
		character.Modifiers.RemoveAll(m => m.IsExpired);
		return expiredCount;
	}

	/// <summary>Clears all non-permanent modifiers.</summary>
	public void ClearTemporaryModifiers(LiveCharacter character) {
		character.Modifiers.RemoveAll(m => m.Duration == StatModifierDuration.Temporary);
	}

	// ═══════════════════════════════════════════════════════════════════════
	// RESOURCE MANAGEMENT (Health, Mana, Stamina)
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Deals damage to a character, applying resistances and vulnerabilities.</summary>
	public DamageResult DealDamage(
		LiveCharacter target,
		float amount,
		DamageType damageType,
		LiveCharacter? source = null,
		bool ignoreResistance = false,
		bool ignoreVulnerability = false
	) {
		var result = new DamageResult {
			RawDamage = amount,
			DamageType = damageType,
			SourceName = source?.Name,
			TargetName = target.Name
		};

		float modifiedDamage = amount;

		// Apply resistance
		if (!ignoreResistance) {
			float resistance = GetResistance(target, damageType);
			if (resistance > 0) {
				modifiedDamage *= (1f - Math.Min(resistance, 1f));
				result.WasResisted = true;
				result.ResistanceApplied = resistance;
			}
		}

		// Apply vulnerability
		if (!ignoreVulnerability) {
			float vulnerability = GetVulnerability(target, damageType);
			if (vulnerability > 0) {
				modifiedDamage *= (1f + vulnerability);
				result.VulnerabilityApplied = vulnerability;
			}
		}

		// Apply defending bonus
		if (target.IsDefending) {
			modifiedDamage *= 0.5f;
			result.WasDefending = true;
		}

		// Apply shielded condition
		if (target.HasCondition(StatusCondition.Shielded)) {
			modifiedDamage *= 0.75f;
			result.WasShielded = true;
		}

		// Check undying condition
		float currentHealth = GetStat(target, Ids.Stats.Resource.CurrentHealth);
		if (target.HasCondition(StatusCondition.Undying) && currentHealth - modifiedDamage <= 0) {
			modifiedDamage = currentHealth - 1f;
		}

		// Apply damage
		result.FinalDamage = Math.Max(0, modifiedDamage);
		float newHealth = Math.Max(0, currentHealth - result.FinalDamage);
		SetBaseStat(target, Ids.Stats.Resource.CurrentHealth, newHealth);

		result.ResultingHealth = newHealth;
		result.WasLethal = newHealth <= 0;

		return result;
	}

	/// <summary>Heals a character.</summary>
	public float Heal(LiveCharacter character, float amount) {
		float currentHealth = GetStat(character, Ids.Stats.Resource.CurrentHealth);
		float maxHealth = GetStat(character, Ids.Stats.Resource.MaxHealth);
		float newHealth = Math.Min(maxHealth, currentHealth + amount);
		float actualHealed = newHealth - currentHealth;

		SetBaseStat(character, Ids.Stats.Resource.CurrentHealth, newHealth);
		return actualHealed;
	}

	/// <summary>Restores health to full.</summary>
	public float FullHeal(LiveCharacter character) {
		float maxHealth = GetStat(character, Ids.Stats.Resource.MaxHealth);
		float currentHealth = GetStat(character, Ids.Stats.Resource.CurrentHealth);
		SetBaseStat(character, Ids.Stats.Resource.CurrentHealth, maxHealth);
		return maxHealth - currentHealth;
	}

	/// <summary>Spends mana. Returns false if insufficient.</summary>
	public bool SpendMana(LiveCharacter character, float amount) {
		float current = GetStat(character, Ids.Stats.Resource.CurrentMana);
		if (current < amount) return false;

		SetBaseStat(character, Ids.Stats.Resource.CurrentMana, current - amount);
		return true;
	}

	/// <summary>Restores mana.</summary>
	public float RestoreMana(LiveCharacter character, float amount) {
		float current = GetStat(character, Ids.Stats.Resource.CurrentMana);
		float max = GetStat(character, Ids.Stats.Resource.MaxMana);
		float newValue = Math.Min(max, current + amount);
		float actualRestored = newValue - current;

		SetBaseStat(character, Ids.Stats.Resource.CurrentMana, newValue);
		return actualRestored;
	}

	/// <summary>Spends stamina. Returns false if insufficient.</summary>
	public bool SpendStamina(LiveCharacter character, float amount) {
		float current = GetStat(character, Ids.Stats.Resource.CurrentStamina);
		if (current < amount) return false;

		SetBaseStat(character, Ids.Stats.Resource.CurrentStamina, current - amount);
		return true;
	}

	/// <summary>Restores stamina.</summary>
	public float RestoreStamina(LiveCharacter character, float amount) {
		float current = GetStat(character, Ids.Stats.Resource.CurrentStamina);
		float max = GetStat(character, Ids.Stats.Resource.MaxStamina);
		float newValue = Math.Min(max, current + amount);
		float actualRestored = newValue - current;

		SetBaseStat(character, Ids.Stats.Resource.CurrentStamina, newValue);
		return actualRestored;
	}

	/// <summary>Applies regeneration for all resources.</summary>
	public void ApplyRegeneration(LiveCharacter character) {
		float healthRegen = GetStat(character, Ids.Stats.Regeneration.Health);
		float manaRegen = GetStat(character, Ids.Stats.Regeneration.Mana);
		float staminaRegen = GetStat(character, Ids.Stats.Regeneration.Stamina);

		if (healthRegen > 0) Heal(character, healthRegen);
		if (manaRegen > 0) RestoreMana(character, manaRegen);
		if (staminaRegen > 0) RestoreStamina(character, staminaRegen);
	}

	// ═══════════════════════════════════════════════════════════════════════
	// CONDITION MANAGEMENT
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Applies a status condition.</summary>
	public bool ApplyCondition(
		LiveCharacter character,
		StatusCondition condition,
		Duration duration,
		float intensity = 1f,
		string? source = null
	) {
		if (character.IsImmuneToCondition(condition)) {
			return false;
		}

		if (character.Conditions.TryGetValue(condition, out var existing)) {
			existing.RemainingDuration = Duration.Max(existing.RemainingDuration, duration);
			existing.Intensity = Math.Max(existing.Intensity, intensity);
		} else {
			character.Conditions[condition] = new StatusEffect {
				Condition = condition,
				RemainingDuration = duration,
				Intensity = intensity,
				SourceId = source
			};
		}

		return true;
	}

	/// <summary>Removes a status condition.</summary>
	public bool RemoveCondition(LiveCharacter character, StatusCondition condition) {
		return character.Conditions.Remove(condition);
	}

	/// <summary>Clears all debuff conditions.</summary>
	public int ClearDebuffs(LiveCharacter character) {
		var debuffs = character.Conditions.Keys.Where(c => c.IsDebuff()).ToList();
		foreach (var debuff in debuffs) {
			character.Conditions.Remove(debuff);
		}
		return debuffs.Count;
	}

	/// <summary>Ticks all status effects (call at end of turn).</summary>
	public StatusTickResult TickConditions(LiveCharacter character) {
		var result = new StatusTickResult();
		var toRemove = new List<StatusCondition>();

		foreach (var (condition, effect) in character.Conditions) {
			switch (condition) {
				case StatusCondition.Poisoned:
					float poisonDmg = 2 * effect.Intensity;
					DealDamage(character, poisonDmg, DamageType.Poison, ignoreResistance: true);
					result.DamageFromEffects += poisonDmg;
					break;

				case StatusCondition.Burning:
					float burnDmg = 3 * effect.Intensity;
					DealDamage(character, burnDmg, DamageType.Fire, ignoreResistance: true);
					result.DamageFromEffects += burnDmg;
					break;

				case StatusCondition.Bleeding:
					float bleedDmg = 1 * effect.Intensity;
					DealDamage(character, bleedDmg, DamageType.Physical, ignoreResistance: true);
					result.DamageFromEffects += bleedDmg;
					break;

				case StatusCondition.Regenerating:
					float regenAmt = 2 * effect.Intensity;
					Heal(character, regenAmt);
					result.HealingFromEffects += regenAmt;
					break;
			}

			effect.RemainingDuration -= 1.Ticks();
			if (effect.RemainingDuration <= 0) {
				toRemove.Add(condition);
				result.ExpiredConditions.Add(condition);
			}
		}

		foreach (var condition in toRemove) {
			character.Conditions.Remove(condition);
		}

		return result;
	}

	// ═══════════════════════════════════════════════════════════════════════
	// EFFECT APPLICATION (from EffectProto)
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Applies an effect from an EffectProto.</summary>
	public void ApplyEffect(LiveCharacter character, EffectProto.ID effectId, Duration? durationOverride = null) {
		if (!k_gameDb.TryGetProto<EffectProto>(effectId, out var proto)) {
			return;
		}

		if (durationOverride.HasValue) {
			ApplyEffect(character, proto, durationOverride);
		} else {
			ApplyEffect(character, proto);
		}
	}

	/// <summary>Applies an effect from an EffectProto.</summary>
	public void ApplyEffect(LiveCharacter character, EffectProto proto, Duration? durationOverride = null) {
		Duration effectDuration = proto.DefaultDuration;
		if (durationOverride.HasValue) {
			effectDuration = durationOverride.Value;
		}

		string source = $"Effect_{proto.Id.Value}";

		foreach (var mod in proto.StatModifiers) {
			var modifier = StatModifier.CreateTemporary(
				mod.Stat,
				mod.Modifier.Value,
				source,
				effectDuration,
				mod.Modifier.Operation
			);
			character.Modifiers.Add(modifier);
		}

		foreach (var condition in proto.AppliedConditions) {
			ApplyCondition(character, condition, effectDuration, source: source);
		}
	}

	/// <summary>Removes an effect by its source.</summary>
	public void RemoveEffect(LiveCharacter character, EffectProto.ID effectId) {
		string source = $"Effect_{effectId.Value}";
		RemoveModifiersBySource(character, source);
	}

	// ═══════════════════════════════════════════════════════════════════════
	// EXPERIENCE & LEVELING
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Awards experience to a character.</summary>
	public LevelUpResult AwardExperience(LiveCharacter character, int amount) {
		var result = new LevelUpResult { ExperienceAwarded = amount };

		float expGainMod = GetStat(character, Ids.Stats.Economy.ExperienceGain);
		int modifiedAmount = (int)(amount * expGainMod);
		result.ModifiedExperience = modifiedAmount;

		float currentExp = GetStat(character, Ids.Stats.Meta.Experience);
		float newExp = currentExp + modifiedAmount;
		SetBaseStat(character, Ids.Stats.Meta.Experience, newExp);

		int currentLevel = GetStatInt(character, Ids.Stats.Meta.Level);
		float expToNext = GetStat(character, Ids.Stats.Meta.ExperienceToNextLevel);

		while (newExp >= expToNext) {
			newExp -= expToNext;
			currentLevel++;
			result.LevelsGained++;

			SetBaseStat(character, Ids.Stats.Meta.Level, currentLevel);
			SetBaseStat(character, Ids.Stats.Meta.Experience, newExp);
			SetBaseStat(character, Ids.Stats.Meta.ExperienceToNextLevel, CalculateXPToLevel(currentLevel + 1));

			ApplyLevelUpBonuses(character, currentLevel);

			expToNext = GetStat(character, Ids.Stats.Meta.ExperienceToNextLevel);
		}

		result.NewLevel = currentLevel;
		return result;
	}

	private void ApplyLevelUpBonuses(LiveCharacter character, int newLevel) {
		int conMod = GetAttributeModifier(character, Ids.Stats.Attributes.Constitution);
		float healthIncrease = 5 + conMod;
		AddToBaseStat(character, Ids.Stats.Resource.MaxHealth, healthIncrease);

		FullHeal(character);

		float maxMana = GetStat(character, Ids.Stats.Resource.MaxMana);
		SetBaseStat(character, Ids.Stats.Resource.CurrentMana, maxMana);
	}

	private static int CalculateXPToLevel(int level) {
		return (int)(100 * Math.Pow(level, 1.5));
	}

	// ═══════════════════════════════════════════════════════════════════════
	// COMBAT STATE
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Rolls initiative for combat.</summary>
	public int RollInitiative(LiveCharacter character, ref RandomStream rng) {
		int dexMod = GetAttributeModifier(character, Ids.Stats.Attributes.Dexterity);
		int initBonus = GetStatInt(character, Ids.Stats.Combat.Initiative);
		int roll = rng.RollDie(20) + dexMod + initBonus;

		SetBaseStat(character, Ids.Stats.Combat.InitiativeRoll, roll);
		return roll;
	}

	/// <summary>Rolls initiative for combat.</summary>
	public int RollInitiativeNonDeterministic(LiveCharacter character) {
		int dexMod = GetAttributeModifier(character, Ids.Stats.Attributes.Dexterity);
		int initBonus = GetStatInt(character, Ids.Stats.Combat.Initiative);
		int roll = DiceRoller.RollDie(20) + dexMod + initBonus;

		SetBaseStat(character, Ids.Stats.Combat.InitiativeRoll, roll);
		return roll;
	}

	/// <summary>Prepares character for a new combat round.</summary>
	public void StartNewRound(LiveCharacter character) {
		character.StartNewRound();
	}

	/// <summary>Called when character's turn starts.</summary>
	public void StartTurn(LiveCharacter character) {
		character.StartTurn();
	}

	/// <summary>Called when character's turn ends.</summary>
	public void EndTurn(LiveCharacter character) {
		character.EndTurn();
		TickConditions(character);
		TickModifiers(character);
	}

	/// <summary>Resets combat state when exiting combat.</summary>
	public void ExitCombat(LiveCharacter character) {
		character.ExitCombat();
		ClearTemporaryModifiers(character);
	}

	// ═══════════════════════════════════════════════════════════════════════
	// REST & RECOVERY
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Performs a short rest.</summary>
	public RestResult ShortRest(LiveCharacter character) {
		var result = new RestResult { RestType = RestType.Short };

		result.StaminaRestored = RestoreStamina(character, 25);
		result.ManaRestored = RestoreMana(character, GetStat(character, Ids.Stats.Resource.MaxMana) * 0.25f);

		return result;
	}

	/// <summary>Performs a long rest.</summary>
	public RestResult LongRest(LiveCharacter character) {
		var result = new RestResult { RestType = RestType.Long };

		result.HealthRestored = FullHeal(character);

		float maxMana = GetStat(character, Ids.Stats.Resource.MaxMana);
		float currentMana = GetStat(character, Ids.Stats.Resource.CurrentMana);
		SetBaseStat(character, Ids.Stats.Resource.CurrentMana, maxMana);
		result.ManaRestored = maxMana - currentMana;

		float maxStamina = GetStat(character, Ids.Stats.Resource.MaxStamina);
		float currentStamina = GetStat(character, Ids.Stats.Resource.CurrentStamina);
		SetBaseStat(character, Ids.Stats.Resource.CurrentStamina, maxStamina);
		result.StaminaRestored = maxStamina - currentStamina;

		result.ConditionsCleared = character.Conditions.Count;
		character.Conditions.Clear();

		ClearTemporaryModifiers(character);

		return result;
	}

	// ═══════════════════════════════════════════════════════════════════════
	// STAMINA & FATIGUE (Expedition)
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>
	/// Gets the effective maximum stamina after fatigue reduction.
	/// Formula: MaxStamina * (1 - Fatigue/MaxFatigue)
	/// </summary>
	public float GetEffectiveMaxStamina(LiveCharacter character) {
		float maxStamina = GetStat(character, Ids.Stats.Resource.MaxStamina);
		float fatigue = GetStat(character, Ids.Stats.Resource.Fatigue);
		float maxFatigue = GetStat(character, Ids.Stats.Resource.MaxFatigue);
		
		if (maxFatigue <= 0) maxFatigue = 100f;
		float fatigueRatio = Math.Clamp(fatigue / maxFatigue, 0f, 1f);
		
		return maxStamina * (1f - fatigueRatio);
	}

	/// <summary>
	/// Gets current stamina as a percentage of effective max.
	/// </summary>
	public float GetStaminaPercent(LiveCharacter character) {
		float effectiveMax = GetEffectiveMaxStamina(character);
		if (effectiveMax <= 0) return 0f;
		return GetStat(character, Ids.Stats.Resource.CurrentStamina) / effectiveMax;
	}

	/// <summary>
	/// Gets fatigue as a percentage of max fatigue.
	/// </summary>
	public float GetFatiguePercent(LiveCharacter character) {
		float maxFatigue = GetStat(character, Ids.Stats.Resource.MaxFatigue);
		if (maxFatigue <= 0) return 0f;
		return GetStat(character, Ids.Stats.Resource.Fatigue) / maxFatigue;
	}

	/// <summary>
	/// Whether stamina is critically low (below 10).
	/// </summary>
	public bool IsExhausted(LiveCharacter character) {
		return GetStat(character, Ids.Stats.Resource.CurrentStamina) <= 10f;
	}

	/// <summary>
	/// Whether fatigue is critically high (90%+ of max).
	/// </summary>
	public bool IsOverfatigued(LiveCharacter character) {
		float maxFatigue = GetStat(character, Ids.Stats.Resource.MaxFatigue);
		if (maxFatigue <= 0) return false; // No fatigue system or not initialized
		
		float fatigue = GetStat(character, Ids.Stats.Resource.Fatigue);
		return fatigue >= maxFatigue * 0.9f;
	}

	/// <summary>
	/// Checks if character has enough stamina for an action.
	/// </summary>
	public bool HasStamina(LiveCharacter character, float required) {
		return GetStat(character, Ids.Stats.Resource.CurrentStamina) >= required;
	}

	/// <summary>
	/// Consumes stamina. Returns actual amount consumed (may be less if insufficient).
	/// </summary>
	public float ConsumeStamina(LiveCharacter character, float amount) {
		if (amount <= 0) return 0;

		float current = GetStat(character, Ids.Stats.Resource.CurrentStamina);
		float consumed = Math.Min(current, amount);
		SetBaseStat(character, Ids.Stats.Resource.CurrentStamina, current - consumed);

		return consumed;
	}

	/// <summary>
	/// Recovers stamina up to the effective maximum (capped by fatigue).
	/// </summary>
	public float RecoverStamina(LiveCharacter character, float amount) {
		if (amount <= 0) return 0;

		float current = GetStat(character, Ids.Stats.Resource.CurrentStamina);
		float effectiveMax = GetEffectiveMaxStamina(character);
		float maxRecovery = effectiveMax - current;
		float recovered = Math.Min(maxRecovery, amount);
		
		if (recovered > 0) {
			SetBaseStat(character, Ids.Stats.Resource.CurrentStamina, current + recovered);
		}

		return recovered;
	}

	/// <summary>
	/// Adds fatigue. This reduces effective max stamina.
	/// Clamps current stamina if it exceeds new effective max.
	/// </summary>
	public void AddFatigue(LiveCharacter character, float amount) {
		if (amount <= 0) return;

		float currentFatigue = GetStat(character, Ids.Stats.Resource.Fatigue);
		float maxFatigue = GetStat(character, Ids.Stats.Resource.MaxFatigue);
		float newFatigue = Math.Min(maxFatigue, currentFatigue + amount);
		
		SetBaseStat(character, Ids.Stats.Resource.Fatigue, newFatigue);

		// Clamp current stamina to new effective max
		float effectiveMax = GetEffectiveMaxStamina(character);
		float currentStamina = GetStat(character, Ids.Stats.Resource.CurrentStamina);
		if (currentStamina > effectiveMax) {
			SetBaseStat(character, Ids.Stats.Resource.CurrentStamina, effectiveMax);
		}
	}

	/// <summary>
	/// Reduces fatigue (from resting). Returns amount actually reduced.
	/// </summary>
	public float ReduceFatigue(LiveCharacter character, float amount) {
		if (amount <= 0) return 0;

		float current = GetStat(character, Ids.Stats.Resource.Fatigue);
		float reduced = Math.Min(current, amount);
		SetBaseStat(character, Ids.Stats.Resource.Fatigue, current - reduced);

		return reduced;
	}

	/// <summary>
	/// Fully rests - removes all fatigue and restores stamina to max.
	/// </summary>
	public void FullRestStamina(LiveCharacter character) {
		SetBaseStat(character, Ids.Stats.Resource.Fatigue, 0);
		float maxStamina = GetStat(character, Ids.Stats.Resource.MaxStamina);
		SetBaseStat(character, Ids.Stats.Resource.CurrentStamina, maxStamina);
	}

	/// <summary>
	/// Updates stamina based on travel pace. Call each frame during expedition.
	/// </summary>
	public void UpdateStaminaForTravel(
		LiveCharacter character,
		float deltaTime,
		TravelPace pace,
		bool isResting,
		float fatigueRateModifier = 1f
	) {
		float staminaCost = pace.GetStaminaCostPerTile();

		if (staminaCost < 0) {
			// Walking or Waiting - recover stamina
			float recoveryRate = 2f; // Base recovery rate
			if (isResting || pace == TravelPace.Wait) {
				recoveryRate *= 3f; // Resting multiplier
			}
			RecoverStamina(character, Math.Abs(staminaCost) * recoveryRate * deltaTime);
		} else if (staminaCost > 0) {
			// Moving fast - consume stamina
			ConsumeStamina(character, staminaCost * deltaTime);
		}

		// Fatigue handling
		float fatigueRate = pace.GetFatigueRatePerHour() * fatigueRateModifier;
		if (fatigueRate < 0) {
			// Negative rate = fatigue recovery (Wait pace)
			ReduceFatigue(character, Math.Abs(fatigueRate) * deltaTime / 3600f);
		} else if (!isResting) {
			// Positive rate = fatigue accumulation (while moving)
			AddFatigue(character, fatigueRate * deltaTime / 3600f);
		}
	}

	/// <summary>
	/// Gets the fallback pace when current pace requires more stamina than available.
	/// </summary>
	public TravelPace GetFallbackPace(LiveCharacter character, TravelPace currentPace) {
		float currentStamina = GetStat(character, Ids.Stats.Resource.CurrentStamina);
		
		TravelPace[] paces = [TravelPace.Sprint, TravelPace.Run, TravelPace.Jog, TravelPace.Walk, TravelPace.Sneak, TravelPace.Wait];

		foreach (var pace in paces) {
			if (pace > currentPace) continue; // Skip faster paces

			float minRequired = pace.GetMinimumStamina();
			if (currentStamina >= minRequired) {
				return pace;
			}
		}

		// Can always wait
		return TravelPace.Wait;
	}

	// ═══════════════════════════════════════════════════════════════════════
	// UTILITY
	// ═══════════════════════════════════════════════════════════════════════

	public bool IsAlive(LiveCharacter character) => GetStat(character, Ids.Stats.Resource.CurrentHealth) > 0;

	public bool IsIncapacitated(LiveCharacter character) =>
		!IsAlive(character) ||
		character.HasCondition(StatusCondition.Stunned) ||
		character.HasCondition(StatusCondition.Frozen);

	public float GetHealthPercent(LiveCharacter character) {
		float max = GetStat(character, Ids.Stats.Resource.MaxHealth);
		return max > 0 ? GetStat(character, Ids.Stats.Resource.CurrentHealth) / max : 0;
	}

	public float GetManaPercent(LiveCharacter character) {
		float max = GetStat(character, Ids.Stats.Resource.MaxMana);
		return max > 0 ? GetStat(character, Ids.Stats.Resource.CurrentMana) / max : 0;
	}

	// ═══════════════════════════════════════════════════════════════════════
	// DAMAGE TYPE MAPPING
	// ═══════════════════════════════════════════════════════════════════════

	private static StatProto.ID MapDamageTypeToResistanceStat(DamageType type) => type switch {
		DamageType.Physical or DamageType.Slashing or DamageType.Piercing or DamageType.Bludgeoning
			=> Ids.Stats.Resistances.Physical,
		DamageType.Fire => Ids.Stats.Resistances.Fire,
		DamageType.Cold => Ids.Stats.Resistances.Cold,
		DamageType.Lightning => Ids.Stats.Resistances.Lightning,
		DamageType.Poison => Ids.Stats.Resistances.Poison,
		DamageType.Acid => Ids.Stats.Resistances.Acid,
		DamageType.Holy => Ids.Stats.Resistances.Holy,
		DamageType.Necrotic => Ids.Stats.Resistances.Necrotic,
		DamageType.Psychic => Ids.Stats.Resistances.Psychic,
		DamageType.Force => Ids.Stats.Resistances.Force,
		_ => Ids.Stats.Resistances.Magical
	};

	private static StatProto.ID MapDamageTypeToVulnerabilityStat(DamageType type) => type switch {
		DamageType.Physical or DamageType.Slashing or DamageType.Piercing or DamageType.Bludgeoning
			=> Ids.Stats.Vulnerabilities.Physical,
		DamageType.Fire => Ids.Stats.Vulnerabilities.Fire,
		DamageType.Cold => Ids.Stats.Vulnerabilities.Cold,
		DamageType.Lightning => Ids.Stats.Vulnerabilities.Lightning,
		DamageType.Poison => Ids.Stats.Vulnerabilities.Poison,
		DamageType.Acid => Ids.Stats.Vulnerabilities.Acid,
		DamageType.Holy => Ids.Stats.Vulnerabilities.Holy,
		DamageType.Necrotic => Ids.Stats.Vulnerabilities.Necrotic,
		DamageType.Psychic => Ids.Stats.Vulnerabilities.Psychic,
		DamageType.Force => Ids.Stats.Vulnerabilities.Force,
		_ => Ids.Stats.Vulnerabilities.Magical
	};
}

// ═══════════════════════════════════════════════════════════════════════════
// RESULT TYPES
// ═══════════════════════════════════════════════════════════════════════════

public class LevelUpResult {
	public int ExperienceAwarded { get; set; }
	public int ModifiedExperience { get; set; }
	public int LevelsGained { get; set; }
	public int NewLevel { get; set; }
	public bool DidLevelUp => LevelsGained > 0;
}

public enum RestType { Short, Long }

public class RestResult {
	public RestType RestType { get; set; }
	public float HealthRestored { get; set; }
	public float ManaRestored { get; set; }
	public float StaminaRestored { get; set; }
	public int ConditionsCleared { get; set; }
}