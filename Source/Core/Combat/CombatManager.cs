using System.Linq;
using RPGGame.Core.Characters;
using RPGGame.Core.Metrics;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Combat;

/// <summary>
/// Manages turn-based combat encounters using LiveCharacter and CharacterManager.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class CombatManager {
	#region Fields

	private readonly GameDb k_gameDb;
	private readonly CharacterManager k_charManager;
	private readonly MetricsManager k_metrics;

	private CombatEncounter? k_currentEncounter;
	private readonly List<LiveCharacter> k_combatants = [];
	private readonly List<LiveCharacter> k_turnOrder = [];
	private int k_currentTurnIndex;
	private int k_roundNumber;

	// Deterministic random stream for this combat encounter
	private RandomStream k_combatRng;
	private int k_encounterId;

	// Store reference to player's LiveCharacter for syncing back to RunState
	private LiveCharacter? k_playerCombatant;
	private RunState? k_currentRunState;

	#endregion

	#region Properties

	public bool IsInCombat => k_currentEncounter != null;
	public CombatPhase Phase { get; private set; } = CombatPhase.None;
	public CombatEncounter? CurrentEncounter => k_currentEncounter;
	public IReadOnlyList<LiveCharacter> Combatants => k_combatants;
	public IReadOnlyList<LiveCharacter> TurnOrder => k_turnOrder;
	public int CurrentTurnIndex => k_currentTurnIndex;
	public int RoundNumber => k_roundNumber;

	public LiveCharacter? CurrentCombatant =>
		k_currentTurnIndex >= 0 && k_currentTurnIndex < k_turnOrder.Count
			? k_turnOrder[k_currentTurnIndex]
			: null;

	public LiveCharacter? Player => k_combatants.FirstOrDefault(c => c.IsPlayer);
	public IEnumerable<LiveCharacter> LivingEnemies => k_combatants.Where(c => c.IsEnemySide && c.IsAlive);
	public IEnumerable<LiveCharacter> LivingAllies => k_combatants.Where(c => c.IsPlayerSide && c.IsAlive);

	#endregion

	#region Events

	public event Action<CombatEncounter>? OnCombatStarted;
	public event Action<CombatPhase>? OnPhaseChanged;
	public event Action<LiveCharacter>? OnTurnStarted;
	public event Action<LiveCharacter>? OnTurnEnded;
	public event Action<CombatActionResult>? OnActionResolved;
	public event Action<LiveCharacter>? OnCombatantDefeated;
	public event Action<CombatResult>? OnCombatEnded;
	public event Action<int>? OnNewRound;

	#endregion

	#region Constructor

	public CombatManager(GameDb gameDb, CharacterManager charManager, MetricsManager metrics) {
		k_gameDb = gameDb;
		k_charManager = charManager;
		k_metrics = metrics;
	}

	#endregion

	#region Combat Lifecycle

	public bool StartCombat(EncounterProto.ID encounterId, RunState runState) {
		if (!k_gameDb.TryGetProto<EncounterProto>(encounterId, out var proto)) {
			UnityEngine.Debug.LogError($"Encounter proto not found: {encounterId}");
			return false;
		}
		return StartCombat(CombatEncounter.FromProto(proto), runState);
	}

	public bool StartCombat(EncounterProto proto, RunState runState) {
		return StartCombat(CombatEncounter.FromProto(proto), runState);
	}

	public bool StartCombat(CombatEncounter encounter, RunState runState) {
		if (IsInCombat) {
			UnityEngine.Debug.LogWarning("Already in combat");
			return false;
		}

		k_currentEncounter = encounter;
		k_currentRunState = runState;
		k_combatants.Clear();
		k_turnOrder.Clear();
		k_roundNumber = 0;

		// Initialize deterministic random for this combat
		// Use encounter count from run state to ensure unique but reproducible streams
		k_encounterId = runState.TotalEncounters;
		k_combatRng = GameRandom.For("Combat", k_encounterId);

		// Create player LiveCharacter from RunState
		k_playerCombatant = CreatePlayerFromRunState(runState);
		k_combatants.Add(k_playerCombatant);

		// Create enemy LiveCharacters from encounter
		var enemies = CreateEnemiesFromEncounter(encounter);
		k_combatants.AddRange(enemies);

		SetPhase(CombatPhase.Initiative);
		OnCombatStarted?.Invoke(encounter);

		RollInitiative();
		StartNewRound();

		k_metrics.Increment(MetricType.CombatTurnsTaken);
		UnityEngine.Debug.Log($"Combat started: {encounter.Name}");

		return true;
	}

	private LiveCharacter CreatePlayerFromRunState(RunState runState) {
		return LiveCharacter.CreateFromRunState(runState);
	}

	private List<LiveCharacter> CreateEnemiesFromEncounter(CombatEncounter encounter) {
		var enemies = new List<LiveCharacter>();
		var spawnRng = k_combatRng.Fork("Spawns");

		foreach (var spawn in encounter.EnemySpawns) {
			if (!k_gameDb.TryGetProto<EnemyProto>(spawn.EnemyId, out var proto)) {
				UnityEngine.Debug.LogWarning($"Enemy proto not found: {spawn.EnemyId}");
				continue;
			}

			int count = spawnRng.NextInt(spawn.MinCount, spawn.MaxCount + 1);

			for (int i = 0; i < count; i++) {
				var enemy = LiveCharacter.CreateEnemy(proto);

				if (encounter.DifficultyModifier != 1f) {
					float maxHealth = k_charManager.GetStat(enemy, Ids.Stats.Resource.MaxHealth);
					k_charManager.SetBaseStat(enemy, Ids.Stats.Resource.MaxHealth, maxHealth * encounter.DifficultyModifier);
					k_charManager.SetBaseStat(enemy, Ids.Stats.Resource.CurrentHealth, maxHealth * encounter.DifficultyModifier);
				}

				if (count > 1) {
					enemy.Name = $"{enemy.Name} {i + 1}";
				}

				enemies.Add(enemy);
			}
		}

		return enemies;
	}

	public void EndCombat(CombatEndReason reason) {
		if (!IsInCombat) return;

		var result = new CombatResult {
			Encounter = k_currentEncounter!,
			Reason = reason,
			RoundsElapsed = k_roundNumber,
			EnemiesDefeated = k_combatants.Count(c => c.IsEnemySide && !c.IsAlive)
		};

		if (reason == CombatEndReason.Victory) {
			result.ExperienceGained = CalculateExperienceReward();
			result.GoldGained = CalculateGoldReward();
			result.LootDropped = GenerateLoot();

			SetPhase(CombatPhase.Victory);
			k_metrics.Increment(MetricType.CombatsWon);
			k_metrics.Add(MetricType.EnemiesDefeated, result.EnemiesDefeated);
		} else if (reason == CombatEndReason.Defeat) {
			SetPhase(CombatPhase.Defeat);
		} else if (reason == CombatEndReason.Fled) {
			SetPhase(CombatPhase.Fled);
			k_metrics.Increment(MetricType.CombatsFled);
		}

		k_metrics.Add(MetricType.CombatTurnsTaken, k_roundNumber);
		k_metrics.SetMin(MetricType.QuickestCombatTurns, k_roundNumber);
		k_metrics.SetMax(MetricType.LongestCombatTurns, k_roundNumber);

		if (k_playerCombatant != null && k_currentRunState != null) {
			SyncPlayerToRunState(k_playerCombatant, k_currentRunState);
		}

		foreach (var combatant in k_combatants) {
			k_charManager.ExitCombat(combatant);
		}

		k_currentEncounter = null;
		k_currentRunState = null;
		k_playerCombatant = null;
		k_combatants.Clear();
		k_turnOrder.Clear();
		k_currentTurnIndex = -1;

		SetPhase(CombatPhase.None);
		OnCombatEnded?.Invoke(result);
	}

	private void SyncPlayerToRunState(LiveCharacter player, RunState runState) {
		if (player != runState.Character) {
			runState.Character.BaseStats.Set(Ids.Stats.Resource.CurrentHealth, player.CurrentHealth);
			runState.Character.BaseStats.Set(Ids.Stats.Resource.CurrentMana, player.CurrentMana);
			runState.Character.BaseStats.Set(Ids.Stats.Resource.CurrentStamina, player.CurrentStamina);
		}
	}

	private void SetPhase(CombatPhase phase) {
		if (Phase != phase) {
			Phase = phase;
			OnPhaseChanged?.Invoke(phase);
		}
	}

	#endregion

	#region Initiative & Turns

	private void RollInitiative() {
		foreach (var combatant in k_combatants) {
			int roll = k_charManager.RollInitiative(combatant, ref k_combatRng);
			k_metrics.RecordDiceRoll(roll, 20);
		}

		k_turnOrder.Clear();
		k_turnOrder.AddRange(k_combatants
			.Where(c => c.IsAlive)
			.OrderByDescending(c => c.InitiativeRoll)
			.ThenByDescending(c => k_charManager.GetAttributeModifier(c, Ids.Stats.Attributes.Dexterity)));

		UnityEngine.Debug.Log($"Initiative order: {string.Join(", ", k_turnOrder.Select(c => $"{c.Name}({c.InitiativeRoll})"))}");
	}

	private void StartNewRound() {
		k_roundNumber++;
		k_currentTurnIndex = -1;

		foreach (var combatant in k_combatants) {
			k_charManager.StartNewRound(combatant);
		}

		foreach (var combatant in k_combatants) {
			foreach (var ability in combatant.Abilities) {
				if (ability.CurrentCooldown > 0) {
					ability.CurrentCooldown--;
				}
			}
		}

		k_turnOrder.Clear();
		k_turnOrder.AddRange(k_combatants
			.Where(c => c.IsAlive)
			.OrderByDescending(c => c.InitiativeRoll));

		OnNewRound?.Invoke(k_roundNumber);
		NextTurn();
	}

	public void NextTurn() {
		if (CurrentCombatant != null) {
			k_charManager.EndTurn(CurrentCombatant);
			OnTurnEnded?.Invoke(CurrentCombatant);

			GameTime.Instance.FastForward(Duration.OneCombatTurn);

			if (!k_charManager.IsAlive(CurrentCombatant)) {
				HandleCombatantDefeated(CurrentCombatant);
			}
		}

		if (CheckCombatEnd()) return;

		k_currentTurnIndex++;
		while (k_currentTurnIndex < k_turnOrder.Count && !k_turnOrder[k_currentTurnIndex].IsAlive) {
			k_currentTurnIndex++;
		}

		if (k_currentTurnIndex >= k_turnOrder.Count) {
			StartNewRound();
			return;
		}

		var current = CurrentCombatant!;
		k_charManager.StartTurn(current);

		if (current.IsPlayer) {
			SetPhase(CombatPhase.PlayerTurn);
		} else if (current.IsPlayerSide) {
			SetPhase(CombatPhase.AllyTurn);
		} else {
			SetPhase(CombatPhase.EnemyTurn);
		}

		OnTurnStarted?.Invoke(current);

		if (!current.IsPlayer && !k_charManager.IsIncapacitated(current)) {
			ExecuteAITurn(current);
		}
	}

	private bool CheckCombatEnd() {
		if (!LivingEnemies.Any()) {
			EndCombat(CombatEndReason.Victory);
			return true;
		}
		if (!LivingAllies.Any()) {
			EndCombat(CombatEndReason.Defeat);
			return true;
		}
		return false;
	}

	private void HandleCombatantDefeated(LiveCharacter combatant) {
		OnCombatantDefeated?.Invoke(combatant);

		if (combatant.IsEnemySide) {
			k_metrics.IncrementBreakdown(MetricType.EnemiesDefeated, combatant.EnemyProtoId?.Value ?? "unknown");
			if (combatant.IsBoss) {
				k_metrics.Increment(MetricType.BossesDefeated);
			}
		}

		UnityEngine.Debug.Log($"{combatant.Name} was defeated!");
	}

	#endregion

	#region Action Execution

	public CombatActionResult? ExecuteAction(CombatAction action) {
		if (!IsInCombat || Phase != CombatPhase.PlayerTurn) return null;
		if (!action.IsValid(k_charManager)) return null;

		SetPhase(CombatPhase.ResolvingAction);

		var result = ResolveAction(action);

		SetPhase(CombatPhase.ShowingResults);
		OnActionResolved?.Invoke(result);

		return result;
	}

	public void ContinueAfterAction() {
		if (Phase == CombatPhase.ShowingResults) {
			NextTurn();
		}
	}

	private CombatActionResult ResolveAction(CombatAction action) {
		var result = new CombatActionResult { Action = action };

		switch (action.Type) {
			case CombatActionType.Attack:
			case CombatActionType.HeavyAttack:
			case CombatActionType.QuickAttack:
				result = ResolveAttack(action);
				break;

			case CombatActionType.Defend:
				action.Actor.IsDefending = true;
				result.Success = true;
				result.Message = $"{action.Actor.Name} takes a defensive stance.";
				break;

			case CombatActionType.UseAbility:
			case CombatActionType.CastSpell:
				result = ResolveAbility(action);
				break;

			case CombatActionType.UseItem:
				result = ResolveUseItem(action);
				break;

			case CombatActionType.Flee:
				result = ResolveFlee(action);
				break;

			case CombatActionType.Wait:
				result.Success = true;
				result.Message = $"{action.Actor.Name} waits.";
				break;
		}

		foreach (var combatant in k_combatants.Where(c => !c.IsAlive)) {
			if (!result.Defeated.Contains(combatant)) {
				result.Defeated.Add(combatant);
				HandleCombatantDefeated(combatant);
			}
		}

		return result;
	}

	private CombatActionResult ResolveAttack(CombatAction action) {
		var result = new CombatActionResult { Action = action };
		var target = action.Target!;

		int attackMod = k_charManager.GetStatInt(action.Actor, Ids.Stats.Combat.AttackBonus);
		float damageMultiplier = 1f;

		switch (action.Type) {
			case CombatActionType.HeavyAttack:
				attackMod -= 2;
				damageMultiplier = 1.5f;
				break;
			case CombatActionType.QuickAttack:
				attackMod += 2;
				damageMultiplier = 0.75f;
				break;
		}

		if (action.Actor.HasCondition(StatusCondition.Blinded)) attackMod -= 4;
		if (action.Actor.HasCondition(StatusCondition.Weakened)) {
			attackMod -= 2;
			damageMultiplier *= 0.8f;
		}
		if (action.Actor.HasCondition(StatusCondition.Empowered)) damageMultiplier *= 1.25f;

		// Roll attack using deterministic stream
		int roll = k_combatRng.RollDie(20);
		k_metrics.RecordDiceRoll(roll, 20);

		int targetAC = k_charManager.GetStatInt(target, Ids.Stats.Combat.ArmorClass);

		result.AttackRoll = new AttackRollResult {
			NaturalRoll = roll,
			Modifier = attackMod,
			Total = roll + attackMod,
			TargetAC = targetAC
		};

		if (roll == 1) {
			result.AttackRoll.Result = AttackResult.CriticalMiss;
			result.Success = false;
			result.Message = $"{action.Actor.Name} critically misses!";
			k_metrics.Increment(MetricType.AttacksMissed);
		} else if (roll == 20 || result.AttackRoll.Total >= targetAC + 10) {
			result.AttackRoll.Result = AttackResult.CriticalHit;
			result.Success = true;
			damageMultiplier *= 2f;
			k_metrics.Increment(MetricType.CriticalHits);
		} else if (result.AttackRoll.Total >= targetAC) {
			result.AttackRoll.Result = AttackResult.Hit;
			result.Success = true;
			k_metrics.Increment(MetricType.AttacksHit);
		} else {
			result.AttackRoll.Result = AttackResult.Miss;
			result.Success = false;
			result.Message = $"{action.Actor.Name} misses {target.Name}.";
			k_metrics.Increment(MetricType.AttacksMissed);
		}

		if (result.Success) {
			// Roll damage using deterministic stream
			int rawDamage = k_combatRng.Roll(action.Actor.UnarmedDamage);
			rawDamage += k_charManager.GetStatInt(action.Actor, Ids.Stats.Combat.DamBonus);
			rawDamage = (int)(rawDamage * damageMultiplier);
			rawDamage = Math.Max(1, rawDamage);

			var damageResult = k_charManager.DealDamage(target, rawDamage, action.Actor.UnarmedDamageType, action.Actor);
			result.DamageDealt.Add(damageResult);

			k_metrics.RecordAttack(true, result.AttackRoll.IsCritical, (int)damageResult.FinalDamage);
			if (action.Actor.IsPlayer) {
				k_metrics.Add(MetricType.DamageDealt, (long)damageResult.FinalDamage);
			} else {
				k_metrics.Add(MetricType.DamageReceived, (long)damageResult.FinalDamage);
			}

			string critText = result.AttackRoll.IsCritical ? " CRITICAL HIT!" : "";
			result.Message = $"{action.Actor.Name} hits {target.Name} for {damageResult.FinalDamage:F0} damage!{critText}";

			if (damageResult.WasLethal) {
				result.Message += $" {target.Name} is defeated!";
			}
		}

		k_metrics.Increment(MetricType.AttacksAttempted);
		return result;
	}

	private CombatActionResult ResolveAbility(CombatAction action) {
		var result = new CombatActionResult { Action = action };
		var ability = action.Ability!;

		k_charManager.SpendMana(action.Actor, ability.ManaCost);
		ability.CurrentCooldown = ability.Cooldown;

		result.Success = true;
		result.Message = $"{action.Actor.Name} uses {ability.Name}!";

		if (action.Type == CombatActionType.CastSpell) {
			k_metrics.Increment(MetricType.SpellsCast);
		} else {
			k_metrics.Increment(MetricType.AbilitiesUsed);
		}

		var targets = GetAbilityTargets(action);

		foreach (var target in targets) {
			if (ability.DamageDice.IsValid) {
				// Roll ability damage using deterministic stream
				int damage = k_combatRng.Roll(ability.DamageDice);
				var damageResult = k_charManager.DealDamage(target, damage, ability.DamageType, action.Actor);
				result.DamageDealt.Add(damageResult);
				result.AdditionalMessages.Add($"{target.Name} takes {damageResult.FinalDamage:F0} {ability.DamageType} damage.");

				if (action.Actor.IsPlayer) {
					k_metrics.Add(MetricType.DamageDealt, (long)damageResult.FinalDamage);
				}

				if (damageResult.WasLethal) {
					result.AdditionalMessages.Add($"{target.Name} is defeated!");
				}
			}

			foreach (var effect in ability.Effects) {
				if (effect.AppliesCondition.HasValue) {
					k_charManager.ApplyCondition(target, effect.AppliesCondition.Value, effect.ConditionDuration);
					result.EffectsApplied.Add((target, effect.AppliesCondition.Value));
					result.AdditionalMessages.Add($"{target.Name} is now {effect.AppliesCondition.Value}!");
				}

				if (effect.HealAmount > 0) {
					float healed = k_charManager.Heal(target, effect.HealAmount);
					result.HealingDone += healed;
					result.AdditionalMessages.Add($"{target.Name} is healed for {healed:F0} HP.");

					if (action.Actor.IsPlayer) {
						k_metrics.Add(MetricType.HealingDone, (long)healed);
					}
				}
			}
		}

		return result;
	}

	private CombatActionResult ResolveUseItem(CombatAction action) {
		var result = new CombatActionResult { Action = action };
		result.Success = true;
		result.Message = $"{action.Actor.Name} uses an item.";
		k_metrics.Increment(MetricType.ItemsUsed);
		return result;
	}

	private CombatActionResult ResolveFlee(CombatAction action) {
		var result = new CombatActionResult { Action = action };

		if (k_currentEncounter != null && !k_currentEncounter.CanFlee) {
			result.Success = false;
			result.Message = "Cannot flee from this battle!";
			return result;
		}

		// Roll flee check using deterministic stream
		int roll = k_combatRng.RollDie(20);
		int dexMod = k_charManager.GetAttributeModifier(action.Actor, Ids.Stats.Attributes.Dexterity);
		int dc = 10 + LivingEnemies.Count();

		k_metrics.RecordDiceRoll(roll, 20);

		if (roll + dexMod >= dc) {
			result.Success = true;
			result.FleeSuccessful = true;
			result.Message = $"{action.Actor.Name} successfully flees!";
			EndCombat(CombatEndReason.Fled);
		} else {
			result.Success = false;
			result.Message = $"{action.Actor.Name} fails to flee!";
		}

		return result;
	}

	private List<LiveCharacter> GetAbilityTargets(CombatAction action) {
		var targets = new List<LiveCharacter>();
		var ability = action.Ability!;

		switch (ability.TargetType) {
			case TargetType.Self:
				targets.Add(action.Actor);
				break;
			case TargetType.SingleEnemy:
				if (action.Target != null) targets.Add(action.Target);
				break;
			case TargetType.SingleAlly:
				targets.Add(action.Target ?? action.Actor);
				break;
			case TargetType.AllEnemies:
				targets.AddRange(action.Actor.IsPlayerSide ? LivingEnemies : LivingAllies);
				break;
			case TargetType.AllAllies:
				targets.AddRange(action.Actor.IsPlayerSide ? LivingAllies : LivingEnemies);
				break;
			case TargetType.All:
				targets.AddRange(k_combatants.Where(c => c.IsAlive));
				break;
		}

		return targets;
	}

	#endregion

	#region AI

	private void ExecuteAITurn(LiveCharacter combatant) {
		var action = DecideAIAction(combatant);
		if (action != null) {
			var result = ResolveAction(action);
			OnActionResolved?.Invoke(result);
		}
		NextTurn();
	}

	private CombatAction? DecideAIAction(LiveCharacter combatant) {
		EnemyAIType aiType = EnemyAIType.Aggressive;

		if (combatant.EnemyProtoId != null) {
			if (k_gameDb.TryGetProto<EnemyProto>(combatant.EnemyProtoId.Value, out var proto)) {
				aiType = proto.AIType;
			}
		}

		return aiType switch {
			EnemyAIType.Aggressive => DecideAggressiveAction(combatant),
			EnemyAIType.Defensive => DecideDefensiveAction(combatant),
			EnemyAIType.Support => DecideSupportAction(combatant),
			EnemyAIType.Cowardly => DecideCowardlyAction(combatant),
			EnemyAIType.Tactical => DecideTacticalAction(combatant),
			EnemyAIType.Ranged => DecideRangedAction(combatant),
			EnemyAIType.Boss => DecideBossAction(combatant),
			_ => DecideAggressiveAction(combatant)
		};
	}

	private CombatAction DecideAggressiveAction(LiveCharacter combatant) {
		var target = LivingAllies.OrderBy(c => c.CurrentHealth).FirstOrDefault();
		return target != null ? CombatAction.Attack(combatant, target) : CombatAction.Wait(combatant);
	}

	private CombatAction DecideDefensiveAction(LiveCharacter combatant) {
		if (combatant.HealthPercent < 0.3f) return CombatAction.Defend(combatant);
		return DecideAggressiveAction(combatant);
	}

	private CombatAction DecideSupportAction(LiveCharacter combatant) {
		var healAbility = combatant.Abilities
			.FirstOrDefault(a => a.Type == CombatAbilityType.Heal &&
				a.CurrentCooldown == 0 &&
				a.ManaCost <= k_charManager.GetStat(combatant, Ids.Stats.Resource.CurrentMana));

		if (healAbility != null) {
			var woundedAlly = LivingEnemies
				.Where(c => k_charManager.GetHealthPercent(c) < 0.5f)
				.OrderBy(c => k_charManager.GetHealthPercent(c))
				.FirstOrDefault();

			if (woundedAlly != null) {
				return CombatAction.UseAbility(combatant, healAbility, woundedAlly);
			}
		}

		return DecideAggressiveAction(combatant);
	}

	private CombatAction DecideCowardlyAction(LiveCharacter combatant) {
		if (combatant.HealthPercent < 0.25f && k_currentEncounter?.CanFlee == true) {
			return CombatAction.Flee(combatant);
		}
		if (combatant.HealthPercent < 0.5f) {
			return CombatAction.Defend(combatant);
		}
		return DecideAggressiveAction(combatant);
	}

	private CombatAction DecideTacticalAction(LiveCharacter combatant) {
		float currentMana = k_charManager.GetStat(combatant, Ids.Stats.Resource.CurrentMana);
		var ability = combatant.Abilities
			.Where(a => a.CurrentCooldown == 0 && a.ManaCost <= currentMana)
			.OrderByDescending(a => a.Type == CombatAbilityType.Attack ? 2 : 1)
			.FirstOrDefault();

		// Use deterministic random for AI decisions
		if (ability != null && k_combatRng.NextFloat() > 0.3f) {
			var target = ability.TargetType == TargetType.SingleEnemy
				? LivingAllies.OrderBy(c => c.CurrentHealth).FirstOrDefault()
				: null;
			return CombatAction.UseAbility(combatant, ability, target);
		}

		return DecideAggressiveAction(combatant);
	}

	private CombatAction DecideRangedAction(LiveCharacter combatant) {
		var player = Player;
		if (player != null && player.IsAlive) {
			return CombatAction.Attack(combatant, player);
		}
		return DecideAggressiveAction(combatant);
	}

	private CombatAction DecideBossAction(LiveCharacter combatant) {
		float currentMana = k_charManager.GetStat(combatant, Ids.Stats.Resource.CurrentMana);
		var ability = combatant.Abilities
			.Where(a => a.CurrentCooldown == 0 && a.ManaCost <= currentMana)
			.OrderByDescending(a => a.Type switch {
				CombatAbilityType.Attack => 3,
				CombatAbilityType.Debuff => 2,
				CombatAbilityType.Buff => 1,
				_ => 0
			})
			.FirstOrDefault();

		// Use deterministic random for AI decisions
		if (ability != null && k_combatRng.NextFloat() > 0.2f) {
			LiveCharacter? target = ability.TargetType switch {
				TargetType.SingleEnemy => LivingAllies.OrderBy(c => c.CurrentHealth).FirstOrDefault(),
				TargetType.SingleAlly => LivingEnemies.OrderBy(c => c.CurrentHealth).FirstOrDefault(),
				_ => null
			};
			return CombatAction.UseAbility(combatant, ability, target);
		}

		var weakTarget = LivingAllies.OrderBy(c => c.CurrentHealth).FirstOrDefault();
		return weakTarget != null ? CombatAction.Attack(combatant, weakTarget) : CombatAction.Wait(combatant);
	}

	#endregion

	#region Rewards

	private int CalculateExperienceReward() {
		int total = k_currentEncounter?.BonusExperience ?? 0;
		foreach (var combatant in k_combatants.Where(c => c.IsEnemySide)) {
			total += combatant.ExperienceReward;
		}
		return total;
	}

	private int CalculateGoldReward() {
		var rewardRng = k_combatRng.Fork("Rewards");
		int total = k_currentEncounter?.BonusGold ?? 0;
		foreach (var combatant in k_combatants.Where(c => c.IsEnemySide)) {
			// Use deterministic random for gold variance
			int goldDrop = (int)(combatant.GoldReward * (0.8f + rewardRng.NextFloat() * 0.4f));
			total += goldDrop;
		}
		return total;
	}

	private List<LootEntry> GenerateLoot() {
		var loot = new List<LootEntry>();
		var lootRng = k_combatRng.Fork("Loot");

		if (k_currentEncounter?.GuaranteedLoot != null) {
			loot.AddRange(k_currentEncounter.GuaranteedLoot);
		}

		foreach (var combatant in k_combatants.Where(c => c.IsEnemySide)) {
			foreach (var drop in combatant.LootTable) {
				// Use deterministic random for loot drops
				if (lootRng.Check(drop.DropChance)) {
					int count = lootRng.NextInt(drop.MinQuantity, drop.MaxQuantity + 1);
					loot.Add(new LootEntry(drop.ItemId, count));
				}
			}
		}

		return loot;
	}

	#endregion

	#region Queries

	public IEnumerable<LiveCharacter> GetValidTargets(LiveCharacter actor, CombatActionType actionType) {
		return actionType switch {
			CombatActionType.Attack or CombatActionType.HeavyAttack or CombatActionType.QuickAttack
				=> actor.IsPlayerSide ? LivingEnemies : LivingAllies,
			_ => k_combatants.Where(c => c.IsAlive)
		};
	}

	public List<CombatActionType> GetAvailableActions() {
		var actions = new List<CombatActionType>();
		var current = CurrentCombatant;

		if (current == null || !current.IsAlive) return actions;

		actions.Add(CombatActionType.Attack);
		actions.Add(CombatActionType.Defend);
		actions.Add(CombatActionType.Wait);
		actions.Add(CombatActionType.HeavyAttack);
		actions.Add(CombatActionType.QuickAttack);

		float currentMana = k_charManager.GetStat(current, Ids.Stats.Resource.CurrentMana);
		if (current.Abilities.Any(a => a.CurrentCooldown == 0 && a.ManaCost <= currentMana)) {
			actions.Add(CombatActionType.UseAbility);
		}

		if (k_currentEncounter?.CanFlee == true) {
			actions.Add(CombatActionType.Flee);
		}

		actions.Add(CombatActionType.UseItem);

		return actions;
	}

	public CombatState GetCombatState() {
		return new CombatState {
			IsInCombat = IsInCombat,
			Phase = Phase,
			RoundNumber = k_roundNumber,
			CurrentTurnIndex = k_currentTurnIndex,
			CurrentCombatant = CurrentCombatant,
			Combatants = [.. k_combatants],
			TurnOrder = [.. k_turnOrder],
			CanFlee = k_currentEncounter?.CanFlee ?? false,
			EncounterName = k_currentEncounter?.Name ?? ""
		};
	}

	#endregion
}

#region Supporting Types

public enum CombatEndReason {
	Victory,
	Defeat,
	Fled,
	Interrupted
}

public class CombatResult {
	public CombatEncounter Encounter { get; set; } = null!;
	public CombatEndReason Reason { get; set; }
	public int RoundsElapsed { get; set; }
	public int EnemiesDefeated { get; set; }
	public int ExperienceGained { get; set; }
	public int GoldGained { get; set; }
	public List<LootEntry> LootDropped { get; set; } = [];
	public int TotalDamageDealt { get; set; }
	public int TotalDamageReceived { get; set; }

	public bool IsVictory => Reason == CombatEndReason.Victory;
}

public class CombatState {
	public bool IsInCombat { get; set; }
	public CombatPhase Phase { get; set; }
	public int RoundNumber { get; set; }
	public int CurrentTurnIndex { get; set; }
	public LiveCharacter? CurrentCombatant { get; set; }
	public List<LiveCharacter> Combatants { get; set; } = [];
	public List<LiveCharacter> TurnOrder { get; set; } = [];
	public bool CanFlee { get; set; }
	public string EncounterName { get; set; } = "";
}

#endregion