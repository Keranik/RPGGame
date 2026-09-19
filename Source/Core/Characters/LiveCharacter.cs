using System.Diagnostics;
using RPGGame.Core.Characters.Creation;
using RPGGame.Core.Combat;
using RPGGame.Core.Generation;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Characters;

/// <summary>
/// The type/role of this character instance.
/// </summary>
public enum CharacterType {
	Player,
	Enemy,
	Companion,
	Summon,
	NPC,
	Boss
}

/// <summary>
/// Runtime representation of any character in the game.
/// All numeric values that can be modified go through BaseStats.
/// Non-numeric data (identities, references, collections) are properties.
/// </summary>
public class LiveCharacter {

	// ═══════════════════════════════════════════════════════════════════════
	// IDENTITY - Not stats, these are references and classifications
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Unique runtime instance ID.</summary>
	public string InstanceId { get; }

	/// <summary>Display name.</summary>
	public string Name { get; set; }

	/// <summary>Portrait/sprite resource name.</summary>
	public string PortraitName { get; set; }

	/// <summary>What type of character this is.</summary>
	public CharacterType Type { get; }

	/// <summary>Class proto ID (player/companions only).</summary>
	public CharacterClassProto.ID? ClassId { get; init; }

	/// <summary>Enemy proto ID (enemies only).</summary>
	public EnemyProto.ID? EnemyProtoId { get; init; }

	/// <summary>Rarity tier (for generated entities).</summary>
	public RarityType Rarity { get; init; } = RarityType.Common;

	// ═══════════════════════════════════════════════════════════════════════
	// TYPE HELPERS - Derived from Type, no storage needed
	// ═══════════════════════════════════════════════════════════════════════

	public bool IsPlayer => Type == CharacterType.Player;
	public bool IsEnemy => Type == CharacterType.Enemy || Type == CharacterType.Boss;
	public bool IsBoss => Type == CharacterType.Boss;
	public bool IsCompanion => Type == CharacterType.Companion;
	public bool IsSummon => Type == CharacterType.Summon;
	public bool IsNPC => Type == CharacterType.NPC;
	public bool IsPlayerSide => Type is CharacterType.Player or CharacterType.Companion or CharacterType.Summon;
	public bool IsEnemySide => Type is CharacterType.Enemy or CharacterType.Boss;
	public bool IsFriendly => IsPlayerSide || Type == CharacterType.NPC;

	// ═══════════════════════════════════════════════════════════════════════
	// STATS - Single source of truth for all numeric values
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>
	/// All stat values. This is the single source of truth for all numeric data.
	/// </summary>
	public StatValues BaseStats { get; } = new();

	/// <summary>
	/// Active modifiers from effects, equipment, conditions, etc.
	/// </summary>
	public List<StatModifier> Modifiers { get; } = [];

	// ═══════════════════════════════════════════════════════════════════════
	// STAT ACCESSORS - Convenience properties that query BaseStats
	// ═══════════════════════════════════════════════════════════════════════

	#region Meta Stats

	public int Level => GetStatInt(Ids.Stats.Meta.Level);
	public float Experience => GetStat(Ids.Stats.Meta.Experience);
	public float ExperienceToNextLevel => GetStat(Ids.Stats.Meta.ExperienceToNextLevel);
	public int Luck => GetStatInt(Ids.Stats.Meta.Luck);
	public int Size => GetStatInt(Ids.Stats.Meta.Size);

	#endregion

	#region Reward Stats (for enemies)

	public int ExperienceReward => GetStatInt(Ids.Stats.Meta.ExperienceAwarded);
	public int GoldReward => GetStatInt(Ids.Stats.Meta.GoldAwarded);

	#endregion

	#region Resource Stats

	public float CurrentHealth => GetStat(Ids.Stats.Resource.CurrentHealth);
	public float MaxHealth => GetStat(Ids.Stats.Resource.MaxHealth);
	public float CurrentMana => GetStat(Ids.Stats.Resource.CurrentMana);
	public float MaxMana => GetStat(Ids.Stats.Resource.MaxMana);
	public float CurrentStamina => GetStat(Ids.Stats.Resource.CurrentStamina);
	public float MaxStamina => GetStat(Ids.Stats.Resource.MaxStamina);
	public float CurrentFatigue => GetStat(Ids.Stats.Resource.Fatigue);
	public float MaxFatigue => GetStat(Ids.Stats.Resource.MaxFatigue);
	public float CurrentMorale => GetStat(Ids.Stats.Resource.Morale);

	public bool IsAlive => CurrentHealth > 0;
	public bool IsDead => CurrentHealth <= 0;
	public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;
	public float ManaPercent => MaxMana > 0 ? CurrentMana / MaxMana : 0;
	public float StaminaPercent => MaxStamina > 0 ? CurrentStamina / MaxStamina : 0;

	#endregion

	#region Attribute Stats

	public int Strength => GetStatInt(Ids.Stats.Attributes.Strength);
	public int Dexterity => GetStatInt(Ids.Stats.Attributes.Dexterity);
	public int Constitution => GetStatInt(Ids.Stats.Attributes.Constitution);
	public int Intelligence => GetStatInt(Ids.Stats.Attributes.Intelligence);
	public int Wisdom => GetStatInt(Ids.Stats.Attributes.Wisdom);
	public int Charisma => GetStatInt(Ids.Stats.Attributes.Charisma);

	public int StrengthMod => StatManager.GetAttributeModifier(Strength);
	public int DexterityMod => StatManager.GetAttributeModifier(Dexterity);
	public int ConstitutionMod => StatManager.GetAttributeModifier(Constitution);
	public int IntelligenceMod => StatManager.GetAttributeModifier(Intelligence);
	public int WisdomMod => StatManager.GetAttributeModifier(Wisdom);
	public int CharismaMod => StatManager.GetAttributeModifier(Charisma);

	#endregion

	#region Combat Stats

	public int ArmorClass => GetStatInt(Ids.Stats.Combat.ArmorClass);
	public int AttackBonus => GetStatInt(Ids.Stats.Combat.AttackBonus);
	public int DamageBonus => GetStatInt(Ids.Stats.Combat.DamBonus);
	public int SpellPower => GetStatInt(Ids.Stats.Combat.SpellPower);
	public int InitiativeBonus => GetStatInt(Ids.Stats.Combat.Initiative);
	public int InitiativeRoll => GetStatInt(Ids.Stats.Combat.InitiativeRoll);
	public float CriticalChance => GetStat(Ids.Stats.Combat.CriticalChance);
	public float CriticalDamage => GetStat(Ids.Stats.Combat.CriticalDamage);
	public float BlockChance => GetStat(Ids.Stats.Combat.BlockChance);
	public float DodgeChance => GetStat(Ids.Stats.Combat.DodgeChance);
	public float ParryChance => GetStat(Ids.Stats.Combat.ParryChance);

	#endregion

	#region Movement Stats

	public float MovementSpeed => GetStat(Ids.Stats.Movement.MovementSpeed);
	public float AttackSpeed => GetStat(Ids.Stats.Movement.AttackSpeed);
	public float CastSpeed => GetStat(Ids.Stats.Movement.CastSpeed);

	#endregion

	#region Regeneration Stats

	public float HealthRegen => GetStat(Ids.Stats.Regeneration.Health);
	public float ManaRegen => GetStat(Ids.Stats.Regeneration.Mana);
	public float StaminaRegen => GetStat(Ids.Stats.Regeneration.Stamina);

	#endregion

	#region Expedition Stats

	public float FoodConsumption => GetStat(Ids.Stats.Expedition.FoodConsumption);
	public float FatigueRate => GetStat(Ids.Stats.Expedition.FatigueRate);
	public float TravelSpeed => GetStat(Ids.Stats.Expedition.TravelSpeed);
	public float CarryCapacity => GetStat(Ids.Stats.Expedition.CarryCapacity);
	public float VisionRange => GetStat(Ids.Stats.Expedition.VisionRange);

	#endregion

	#region Economy Stats

	public float GoldFind => GetStat(Ids.Stats.Economy.GoldFind);
	public float ItemFind => GetStat(Ids.Stats.Economy.ItemFind);
	public float ExperienceGain => GetStat(Ids.Stats.Economy.ExperienceGain);

	#endregion

	// ═══════════════════════════════════════════════════════════════════════
	// NON-STAT COLLECTIONS - Complex data that can't be single numeric values
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Known skills with their ranks. Key: SkillID, Value: Rank</summary>
	public Dictionary<SkillProto.ID, int> Skills { get; } = [];

	/// <summary>All known spells.</summary>
	public HashSet<SpellProto.ID> KnownSpells { get; } = [];

	/// <summary>Currently prepared/memorized spells.</summary>
	public HashSet<SpellProto.ID> PreparedSpells { get; } = [];

	/// <summary>Tags applied to this character (creature type, behaviors, etc.).</summary>
	public HashSet<TagProto.ID> Tags { get; } = [];

	/// <summary>Active status conditions with their effect data.</summary>
	public Dictionary<StatusCondition, StatusEffect> Conditions { get; } = [];

	/// <summary>Conditions this character is immune to.</summary>
	public HashSet<StatusCondition> Immunities { get; } = [];

	/// <summary>Combat abilities available to this character.</summary>
	public List<CombatAbility> Abilities { get; } = [];

	/// <summary>Loot table entries (enemies only).</summary>
	public List<LootEntry> LootTable { get; } = [];

	// ═══════════════════════════════════════════════════════════════════════
	// WEAPON DATA - Dice expression + type, not a single stat value
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Unarmed/natural attack damage dice.</summary>
	public HitDice UnarmedDamage { get; set; } = HitDice.D6;

	/// <summary>Unarmed/natural attack damage type.</summary>
	public DamageType UnarmedDamageType { get; set; } = DamageType.Physical;

	// ═══════════════════════════════════════════════════════════════════════
	// TRANSIENT COMBAT STATE - Not stats, just ephemeral flow control
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Position in combat grid/order.</summary>
	public int CombatPosition { get; set; }

	/// <summary>Has this character acted this round?</summary>
	public bool HasActedThisRound { get; set; }

	/// <summary>Is this character currently defending?</summary>
	public bool IsDefending { get; set; }

	// ═══════════════════════════════════════════════════════════════════════
	// DERIVED HELPERS
	// ═══════════════════════════════════════════════════════════════════════

	public bool HasSkill(SkillProto.ID skillId) => Skills.ContainsKey(skillId);
	public int GetSkillRank(SkillProto.ID skillId) => Skills.GetValueOrDefault(skillId, 0);
	public bool HasSpell(SpellProto.ID spellId) => KnownSpells.Contains(spellId);
	public bool IsSpellPrepared(SpellProto.ID spellId) => PreparedSpells.Contains(spellId);
	public bool HasTag(TagProto.ID tagId) => Tags.Contains(tagId);
	public bool HasCondition(StatusCondition condition) => Conditions.ContainsKey(condition);
	public bool IsImmuneToCondition(StatusCondition condition) => Immunities.Contains(condition);

	public bool IsIncapacitated => IsDead ||
		HasCondition(StatusCondition.Stunned) ||
		HasCondition(StatusCondition.Frozen);

	// ═══════════════════════════════════════════════════════════════════════
	// CONSTRUCTOR
	// ═══════════════════════════════════════════════════════════════════════

	private LiveCharacter(CharacterType type) {
		InstanceId = Guid.NewGuid().ToString("N")[..8];
		Type = type;
		Name = "Unknown";
		PortraitName = "default";

		// Set reasonable defaults for stats
		BaseStats.Set(Ids.Stats.Meta.Level, 1);
		BaseStats.Set(Ids.Stats.Meta.Size, 1); // Medium
		BaseStats.Set(Ids.Stats.Movement.MovementSpeed, 100.Percent());
		BaseStats.Set(Ids.Stats.Expedition.FatigueRate, 100.Percent()); 
		BaseStats.Set(Ids.Stats.Resource.MaxFatigue, 100f);
		BaseStats.Set(Ids.Stats.Resource.Fatigue, 0f);

		if (type == CharacterType.Player) {
			BaseStats.Set(Ids.Stats.Expedition.GoldOnHand, 15);
			BaseStats.Set(Ids.Stats.Expedition.FoodOnHand, 5);
			BaseStats.Set(Ids.Stats.Expedition.MedicalSupplies, 3);
			BaseStats.Set(Ids.Stats.Expedition.CampingSupplies, 2);
			BaseStats.Set(Ids.Stats.Economy.GoldFind, 100.Percent());
			BaseStats.Set(Ids.Stats.Economy.ItemFind, 100.Percent());
			BaseStats.Set(Ids.Stats.Economy.ExperienceGain, 100.Percent());
			BaseStats.Set(Ids.Stats.Expedition.FoodConsumption, 100.Percent());
			BaseStats.Set(Ids.Stats.Expedition.TravelSpeed, 100.Percent());
		}
		
	}

	// ═══════════════════════════════════════════════════════════════════════
	// STAT QUERIES
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>Gets a stat value with all modifiers applied.</summary>
	public float GetStat(StatProto.ID statId) {
		float baseValue = BaseStats.Get(statId);
		var valueModifiers = Modifiers
			.Where(m => m.StatId == statId && m.IsActive && !m.IsExpired)
			.Select(m => m.ToValueModifier());
		return ModifierCalculator.Calculate(baseValue, valueModifiers);
	}

	/// <summary>Gets a stat as an integer.</summary>
	public int GetStatInt(StatProto.ID statId) => (int)GetStat(statId);

	/// <summary>Gets the D&D-style attribute modifier for an attribute.</summary>
	public int GetAttributeModifier(StatProto.ID attributeId) {
		return StatManager.GetAttributeModifier(GetStatInt(attributeId));
	}

	/// <summary>Gets damage resistance for a type (0-1 range, from Resistances stats).</summary>
	public float GetResistance(DamageType type) {
		var statId = MapDamageTypeToResistanceStat(type);
		return GetStat(statId) / 100f; // Stored as 0-100, return as 0-1
	}

	/// <summary>Gets damage vulnerability for a type (0+ range, from Vulnerabilities stats).</summary>
	public float GetVulnerability(DamageType type) {
		var statId = MapDamageTypeToVulnerabilityStat(type);
		return GetStat(statId) / 100f; // Stored as 0-100, return as 0-1
	}

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

	// ═══════════════════════════════════════════════════════════════════════
	// COMBAT ROUND MANAGEMENT
	// ═══════════════════════════════════════════════════════════════════════

	public void StartNewRound() {
		HasActedThisRound = false;
		IsDefending = false;
	}

	public void StartTurn() {
		IsDefending = false;
	}

	public void EndTurn() {
		HasActedThisRound = true;
	}

	public void ExitCombat() {
		BaseStats.Set(Ids.Stats.Combat.InitiativeRoll, 0);
		CombatPosition = 0;
		HasActedThisRound = false;
		IsDefending = false;
	}

	// ═══════════════════════════════════════════════════════════════════════
	// FACTORY METHODS
	// ═══════════════════════════════════════════════════════════════════════

	/// <summary>
	/// Creates a LiveCharacter from RunState for combat.
	/// This creates a copy that can be used in combat without modifying the original.
	/// </summary>
	public static LiveCharacter CreateFromRunState(RunState runState) {
		// For combat, we use the existing character directly
		// The RunState.Character IS the LiveCharacter
		return runState.Character;
	}

	public static LiveCharacter CreatePlayer(
		string name,
		CharacterClassProto classProto,
		CharacterCreationState creationState
	) {
		var character = new LiveCharacter(CharacterType.Player) {
			Name = name,
			PortraitName = classProto.PortraitName,
			ClassId = classProto.Id
		};

		var stats = character.BaseStats;
		UnityEngine.Debug.Log($"Creating new player character from {classProto}");
		// Attributes from creation
		foreach (var (attrId, value) in creationState.Attributes) {
			stats.Set(attrId, value);
		}

		// Calculate derived stats
		int conMod = StatManager.GetAttributeModifier(
			creationState.Attributes.GetValueOrDefault(Ids.Stats.Attributes.Constitution, 10));
		int intMod = StatManager.GetAttributeModifier(
			creationState.Attributes.GetValueOrDefault(Ids.Stats.Attributes.Intelligence, 10));
		int dexMod = StatManager.GetAttributeModifier(
			creationState.Attributes.GetValueOrDefault(Ids.Stats.Attributes.Dexterity, 10));

		int maxHealth = classProto.BaseHealth + creationState.BonusHealth + conMod;
		int maxMana = classProto.BaseMana + creationState.BonusMana +
			(classProto.CanCastSpells ? intMod : 0);

		// Resources
		stats.Set(Ids.Stats.Resource.MaxHealth, maxHealth);
		stats.Set(Ids.Stats.Resource.CurrentHealth, maxHealth);
		stats.Set(Ids.Stats.Resource.MaxMana, maxMana);
		stats.Set(Ids.Stats.Resource.CurrentMana, maxMana);
		stats.Set(Ids.Stats.Resource.MaxStamina, 100);
		stats.Set(Ids.Stats.Resource.CurrentStamina, 100);
		stats.Set(Ids.Stats.Resource.Fatigue, 0);
		stats.Set(Ids.Stats.Resource.MaxFatigue, 100);

		// Combat
		stats.Set(Ids.Stats.Combat.ArmorClass, classProto.BaseArmorClass + dexMod);
		stats.Set(Ids.Stats.Combat.Initiative, dexMod);

		// Meta
		stats.Set(Ids.Stats.Meta.Level, 1);
		stats.Set(Ids.Stats.Meta.Experience, 0);
		stats.Set(Ids.Stats.Meta.ExperienceToNextLevel, CalculateXPToLevel(2));

		// Skills
		foreach (var (skillId, rank) in creationState.SelectedSkills) {
			character.Skills[skillId] = rank;
		}

		// Spells
		foreach (var spellId in creationState.SelectedSpells) {
			character.KnownSpells.Add(spellId);
			character.PreparedSpells.Add(spellId);
		}

		return character;
	}

	public static LiveCharacter CreateEnemy(GeneratedEnemy generated) {
		var type = generated.BaseProto.IsBoss ? CharacterType.Boss : CharacterType.Enemy;

		var character = new LiveCharacter(type) {
			Name = generated.DisplayName,
			PortraitName = generated.BaseProto.PortraitName,
			EnemyProtoId = generated.BaseProto.Id,
			Rarity = generated.Rarity,
			UnarmedDamage = generated.FinalDamageDice,
			UnarmedDamageType = generated.BaseProto.DamageType
		};

		var stats = character.BaseStats;

		// Resources
		stats.Set(Ids.Stats.Resource.MaxHealth, generated.FinalMaxHealth);
		stats.Set(Ids.Stats.Resource.CurrentHealth, generated.FinalMaxHealth);

		// Combat
		stats.Set(Ids.Stats.Combat.ArmorClass, generated.FinalArmorClass);
		stats.Set(Ids.Stats.Combat.AttackBonus, generated.FinalAttackBonus);
		stats.Set(Ids.Stats.Combat.DamBonus, generated.FinalDamageBonus);
		stats.Set(Ids.Stats.Combat.Initiative, generated.FinalInitiative);

		// Meta
		stats.Set(Ids.Stats.Meta.Level, (int)generated.EffectiveLevel);

		// Rewards
		stats.Set(Ids.Stats.Meta.ExperienceAwarded, generated.ExperienceValue);
		stats.Set(Ids.Stats.Meta.GoldAwarded, generated.GoldValue);

		// Tags
		foreach (var tag in generated.Tags) {
			character.Tags.Add(tag);
		}

		// Resistances (stored as 0-100 percentages)
		foreach (var (damageType, amount) in generated.Resistances) {
			var statId = MapDamageTypeToResistanceStat(damageType);
			stats.Set(statId, amount * 100f);
		}

		// Vulnerabilities (stored as 0-100 percentages)
		foreach (var (damageType, amount) in generated.Vulnerabilities) {
			var statId = MapDamageTypeToVulnerabilityStat(damageType);
			stats.Set(statId, amount * 100f);
		}

		// Immunities (these are binary, not stats)
		foreach (var immunity in generated.Immunities) {
			character.Immunities.Add(immunity);
		}

		// Abilities
		foreach (var ability in generated.Abilities) {
			character.Abilities.Add(new CombatAbility {
				Id = ability.Id,
				Name = ability.Name,
				Description = ability.Description,
				Type = MapAbilityType(ability.Type),
				TargetType = ability.TargetType,
				ManaCost = ability.ManaCost,
				Cooldown = ability.Cooldown,
				DamageDice = ability.DamageDice,
				DamageType = ability.DamageType
			});
		}

		// Loot
		character.LootTable.AddRange(generated.BaseProto.LootTable);

		return character;
	}

	public static LiveCharacter CreateEnemy(EnemyProto proto, RarityType rarity = RarityType.Common) {
		var type = proto.IsBoss ? CharacterType.Boss : CharacterType.Enemy;

		var character = new LiveCharacter(type) {
			Name = proto.Name,
			PortraitName = proto.PortraitName,
			EnemyProtoId = proto.Id,
			Rarity = rarity,
			UnarmedDamage = proto.AttackDamage,
			UnarmedDamageType = proto.DamageType
		};

		
		var stats = character.BaseStats;

		// Resources
		stats.Set(Ids.Stats.Resource.MaxHealth, proto.MaxHealth);
		stats.Set(Ids.Stats.Resource.CurrentHealth, proto.MaxHealth);

		// Combat
		stats.Set(Ids.Stats.Combat.ArmorClass, proto.ArmorClass);
		stats.Set(Ids.Stats.Combat.AttackBonus, proto.AttackBonus);
		stats.Set(Ids.Stats.Combat.DamBonus, proto.DamageBonus);
		stats.Set(Ids.Stats.Combat.Initiative, proto.InitiativeBonus);

		// Meta
		stats.Set(Ids.Stats.Meta.Level, (int)proto.ChallengeRating);

		// Rewards
		stats.Set(Ids.Stats.Meta.ExperienceAwarded, proto.ExperienceValue);
		stats.Set(Ids.Stats.Meta.GoldAwarded, proto.GoldValue);

		// Resistances
		foreach (var (damageType, amount) in proto.Resistances) {
			var statId = MapDamageTypeToResistanceStat(damageType);
			stats.Set(statId, amount * 100f);
		}

		// Vulnerabilities
		foreach (var (damageType, amount) in proto.Vulnerabilities) {
			var statId = MapDamageTypeToVulnerabilityStat(damageType);
			stats.Set(statId, amount * 100f);
		}

		// Immunities
		foreach (var immunity in proto.Immunities) {
			character.Immunities.Add(immunity);
		}

		// Abilities
		foreach (var ability in proto.Abilities) {
			character.Abilities.Add(new CombatAbility {
				Id = ability.Id,
				Name = ability.Name,
				Description = ability.Description,
				Type = MapAbilityType(ability.Type),
				TargetType = ability.TargetType,
				ManaCost = ability.ManaCost,
				Cooldown = ability.Cooldown,
				DamageDice = ability.DamageDice,
				DamageType = ability.DamageType
			});
		}

		// Loot
		character.LootTable.AddRange(proto.LootTable);

		return character;
	}

	private static CombatAbilityType MapAbilityType(EnemyAbilityType type) => type switch {
		EnemyAbilityType.Attack => CombatAbilityType.Attack,
		EnemyAbilityType.Spell => CombatAbilityType.Spell,
		EnemyAbilityType.Buff => CombatAbilityType.Buff,
		EnemyAbilityType.Debuff => CombatAbilityType.Debuff,
		EnemyAbilityType.Heal => CombatAbilityType.Heal,
		EnemyAbilityType.Utility => CombatAbilityType.Utility,
		_ => CombatAbilityType.Attack
	};

	private static int CalculateXPToLevel(int level) {
		return (int)(100 * Math.Pow(level, 1.5));
	}

	// ═══════════════════════════════════════════════════════════════════════
	// SERIALIZATION
	// ═══════════════════════════════════════════════════════════════════════

	public LiveCharacterData ToData() {
		return new LiveCharacterData {
			InstanceId = InstanceId,
			Name = Name,
			PortraitName = PortraitName,
			Type = Type,
			ClassId = ClassId?.Value,
			EnemyProtoId = EnemyProtoId?.Value,
			Rarity = Rarity,
			BaseStats = BaseStats.ToData(),
			UnarmedDamage = UnarmedDamage,
			UnarmedDamageType = UnarmedDamageType,
			Skills = Skills.ToDictionary(kv => kv.Key.Value, kv => kv.Value),
			KnownSpells = [.. KnownSpells.Select(s => s.Value)],
			PreparedSpells = [.. PreparedSpells.Select(s => s.Value)],
			Tags = [.. Tags.Select(t => t.Value)],
			Conditions = Conditions.ToDictionary(kv => kv.Key, kv => kv.Value),
			Immunities = [.. Immunities]
		};
	}

	public static LiveCharacter FromData(LiveCharacterData data) {
		var character = new LiveCharacter(data.Type) {
			Name = data.Name,
			PortraitName = data.PortraitName,
			Rarity = data.Rarity,
			UnarmedDamage = data.UnarmedDamage,
			UnarmedDamageType = data.UnarmedDamageType
		};

		// Load stats from data
		var loadedStats = StatValues.FromData(data.BaseStats);
		foreach (var statId in loadedStats.GetAllStats()) {
			character.BaseStats.Set(statId, loadedStats.Get(statId));
		}

		foreach (var (skillId, rank) in data.Skills) {
			character.Skills[new SkillProto.ID(skillId)] = rank;
		}
		foreach (var spellId in data.KnownSpells) {
			character.KnownSpells.Add(new SpellProto.ID(spellId));
		}
		foreach (var spellId in data.PreparedSpells) {
			character.PreparedSpells.Add(new SpellProto.ID(spellId));
		}
		foreach (var tagId in data.Tags) {
			character.Tags.Add(new TagProto.ID(tagId));
		}
		foreach (var (condition, effect) in data.Conditions) {
			character.Conditions[condition] = effect;
		}
		foreach (var immunity in data.Immunities) {
			character.Immunities.Add(immunity);
		}

		return character;
	}
}

// ═══════════════════════════════════════════════════════════════════════════
// SERIALIZATION DATA
// ═══════════════════════════════════════════════════════════════════════════

public class LiveCharacterData {
	public string InstanceId { get; set; } = "";
	public string Name { get; set; } = "";
	public string PortraitName { get; set; } = "";
	public CharacterType Type { get; set; }
	public string? ClassId { get; set; }
	public string? EnemyProtoId { get; set; }
	public RarityType Rarity { get; set; }
	public StatValuesData BaseStats { get; set; } = new();
	public HitDice UnarmedDamage { get; set; } = HitDice.D6;
	public DamageType UnarmedDamageType { get; set; }
	public Dictionary<string, int> Skills { get; set; } = [];
	public List<string> KnownSpells { get; set; } = [];
	public List<string> PreparedSpells { get; set; } = [];
	public List<string> Tags { get; set; } = [];
	public Dictionary<StatusCondition, StatusEffect> Conditions { get; set; } = [];
	public List<StatusCondition> Immunities { get; set; } = [];
}