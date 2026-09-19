using RPGGame.Core.Characters;
using RPGGame.Core.Items;
using RPGGame.Core.Metrics;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Rewards;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Events;

/// <summary>
/// Manages event spawning, execution, and outcomes.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class EventManager {
	#region Fields

	private readonly GameDb k_gameDb;
	private readonly MetricsManager k_metrics;
	private RewardManager? k_rewardManager;

	private EventProto? k_currentEvent;
	private EventChoiceProto? k_selectedChoice;
	private EventOutcomeProto? k_currentOutcome;

	private readonly List<string> k_eventHistory = [];
	private readonly Dictionary<string, int> k_eventCooldowns = [];

	#endregion

	#region Properties

	/// <summary>
	/// Whether an event is currently active.
	/// </summary>
	public bool IsEventActive => k_currentEvent != null;

	/// <summary>
	/// The current active event.
	/// </summary>
	public EventProto? CurrentEvent => k_currentEvent;

	/// <summary>
	/// The selected choice (if any).
	/// </summary>
	public EventChoiceProto? SelectedChoice => k_selectedChoice;

	/// <summary>
	/// The current outcome being displayed.
	/// </summary>
	public EventOutcomeProto? CurrentOutcome => k_currentOutcome;

	/// <summary>
	/// Whether waiting for outcome display.
	/// </summary>
	public bool IsShowingOutcome => k_currentOutcome != null;

	#endregion

	#region Events

	public event Action<EventProto>? OnEventStarted;
	public event Action<EventChoiceProto, EventOutcomeProto>? OnChoiceMade;
	public event Action<EventProto, EventOutcomeProto?>? OnEventEnded;
	public event Action<List<OutcomeEffect>>? OnEffectsApplied;
	public event Action<string>? OnCombatTriggered;
	public event Action<string>? OnEventChained;

	/// <summary>
	/// Fired when a reward needs to be shown (with spinner UI).
	/// The callback should be invoked when the reward is claimed.
	/// </summary>
	public event Action<PendingReward, Action<ItemInstance?>>? OnRewardPending;

	#endregion

	#region Constructor

	public EventManager(GameDb gameDb, MetricsManager metrics) {
		k_gameDb = gameDb;
		k_metrics = metrics;
		UnityEngine.Debug.Log("EventManager initialized");
	}

	#endregion

	#region Dependency Injection

	/// <summary>
	/// Sets the reward manager for item generation.
	/// </summary>
	public void SetRewardManager(RewardManager rewardManager) {
		k_rewardManager = rewardManager;
		UnityEngine.Debug.Log("EventManager: RewardManager set");
	}

	#endregion

	#region Event Spawning

	/// <summary>
	/// Gets a random event appropriate for the current context.
	/// </summary>
	public EventProto? GetRandomEvent(
		RunState runState,
		MetaProgression meta,
		TerrainProto.ID terrain,
		float distance,
		EventType? preferredType = null
	) {
		var candidates = new List<(EventProto evt, float weight)>();

		foreach (var evt in k_gameDb.GetAllProtosOfType<EventProto>()) {
			if (!evt.CanSpawn(runState, meta, terrain, distance)) {
				continue;
			}
			if (IsOnCooldown(evt.Id.Value)) {
				continue;
			}

			float weight = evt.GetEffectiveWeight();

			// Boost preferred type
			if (preferredType.HasValue && evt.Type == preferredType.Value) {
				weight *= 2f;
			}

			// Reduce weight if recently encountered
			if (k_eventHistory.Contains(evt.Id.Value)) {
				weight *= 0.5f;
			}

			candidates.Add((evt, weight));
		}

		if (candidates.Count == 0) {
			return null;
		}

		// Weighted random selection
		float totalWeight = candidates.Sum(c => c.weight);
		float roll = UnityEngine.Random.value * totalWeight;
		float cumulative = 0;

		foreach (var (evt, weight) in candidates) {
			cumulative += weight;
			if (roll <= cumulative) {
				return evt;
			}
		}

		return candidates.Last().evt;
	}

	/// <summary>
	/// Gets a specific event by ID.
	/// </summary>
	public EventProto? GetEvent(string eventId) {
		if (k_gameDb.TryGetProto<EventProto>(new Prototypes.Proto.ID(eventId), out var proto)) {
			return proto;
		}
		return null;
	}

	/// <summary>
	/// Gets a specific event by typed ID.
	/// </summary>
	public EventProto? GetEvent(EventProto.ID eventId) {
		if (k_gameDb.TryGetProto<EventProto>(eventId, out var proto)) {
			return proto;
		}
		return null;
	}

	private bool IsOnCooldown(string eventId) {
		if (!k_eventCooldowns.TryGetValue(eventId, out int cooldown)) {
			return false;
		}
		return cooldown > 0;
	}

	/// <summary>
	/// Decrements all cooldowns (call at end of run or periodically).
	/// </summary>
	public void TickCooldowns() {
		var keys = k_eventCooldowns.Keys.ToList();
		foreach (var key in keys) {
			if (k_eventCooldowns[key] > 0) {
				k_eventCooldowns[key]--;
			}
		}
	}

	#endregion

	#region Event Execution

	/// <summary>
	/// Starts an event by string ID.
	/// </summary>
	public bool StartEvent(string eventId, RunState runState, MetaProgression meta) {
		var evt = GetEvent(eventId);
		if (evt == null) {
			UnityEngine.Debug.LogError($"Event not found: {eventId}");
			return false;
		}

		return StartEvent(evt, runState, meta);
	}

	/// <summary>
	/// Starts an event by typed ID.
	/// </summary>
	public bool StartEvent(EventProto.ID eventId, RunState runState, MetaProgression meta) {
		var evt = GetEvent(eventId);
		if (evt == null) {
			UnityEngine.Debug.LogError($"Event not found: {eventId}");
			return false;
		}

		return StartEvent(evt, runState, meta);
	}

	/// <summary>
	/// Starts an event.
	/// </summary>
	public bool StartEvent(EventProto evt, RunState runState, MetaProgression meta) {
		if (IsEventActive) {
			UnityEngine.Debug.LogWarning("Event already active");
			return false;
		}

		k_currentEvent = evt;
		k_selectedChoice = null;
		k_currentOutcome = null;

		// Track encounter
		runState.EncounteredEvents.Add(evt.Id.Value);
		k_eventHistory.Add(evt.Id.Value);

		// Limit history size
		while (k_eventHistory.Count > 50) {
			k_eventHistory.RemoveAt(0);
		}

		// Apply start effects
		if (evt.OnStartEffects.Count > 0) {
			ApplyEffects(evt.OnStartEffects, runState, meta);
		}

		// Track metrics
		k_metrics.Increment(MetricType.EventsEncountered);

		OnEventStarted?.Invoke(evt);
		UnityEngine.Debug.Log($"Event started: {evt.Title}");

		// Auto-resolve if configured
		if (evt.AutoResolve && evt.Choices.Count == 1) {
			SelectChoice(evt.Choices[0], runState, meta);
		}

		return true;
	}

	/// <summary>
	/// Selects a choice for the current event.
	/// </summary>
	public bool SelectChoice(EventChoiceProto choice, RunState runState, MetaProgression meta) {
		if (!IsEventActive || k_currentEvent == null) {
			return false;
		}

		// Verify choice is valid
		if (choice.Condition != null && !choice.Condition.Evaluate(runState, meta).Passed) {
			UnityEngine.Debug.LogWarning($"Choice condition not met: {choice.Text}");
			return false;
		}

		// Verify cost can be paid
		if (choice.Cost != null && !CanPayCost(choice.Cost, runState)) {
			UnityEngine.Debug.LogWarning($"Cannot afford choice: {choice.Text}");
			return false;
		}

		k_selectedChoice = choice;

		// Pay cost
		if (choice.Cost != null) {
			PayCost(choice.Cost, runState);
		}

		// Determine outcome (success or failure based on condition)
		bool success = true;
		if (choice.Condition != null) {
			var result = choice.Condition.Evaluate(runState, meta);
			success = result.Passed;
		}

		k_currentOutcome = success ? choice.SuccessOutcome : (choice.FailureOutcome ?? choice.SuccessOutcome);

		// Apply outcome effects
		if (k_currentOutcome != null) {
			ApplyEffects(k_currentOutcome.Effects, runState, meta);
		}

		// Track metrics
		k_metrics.Increment(MetricType.ChoicesMade);
		TrackChoiceMetrics(choice, k_currentOutcome, success);

		if (k_currentOutcome != null) {
			OnChoiceMade?.Invoke(choice, k_currentOutcome);
		}

		return true;
	}

	/// <summary>
	/// Completes the current event after outcome is shown.
	/// </summary>
	public void CompleteEvent(RunState runState, MetaProgression meta) {
		if (!IsEventActive || k_currentEvent == null) {
			return;
		}

		var evt = k_currentEvent;
		var outcome = k_currentOutcome;

		// Apply end effects
		if (evt.OnEndEffects.Count > 0) {
			ApplyEffects(evt.OnEndEffects, runState, meta);
		}

		// Set cooldown if needed
		if (evt.RepeatCooldown > 0) {
			k_eventCooldowns[evt.Id.Value] = evt.RepeatCooldown;
		}

		// Clear state
		k_currentEvent = null;
		k_selectedChoice = null;
		k_currentOutcome = null;

		OnEventEnded?.Invoke(evt, outcome);
		
		UnityEngine.Debug.Log($"Event completed: {evt.Title}");
	}

	/// <summary>
	/// Skips the current event (if allowed).
	/// </summary>
	public bool SkipEvent(RunState runState, MetaProgression meta) {
		if (!IsEventActive || k_currentEvent == null) {
			return false;
		}
		if (!k_currentEvent.CanSkip) {
			return false;
		}

		var evt = k_currentEvent;

		k_currentEvent = null;
		k_selectedChoice = null;
		k_currentOutcome = null;

		OnEventEnded?.Invoke(evt, null);
		return true;
	}

	#endregion

	#region Effect Application

	private void ApplyEffects(List<OutcomeEffect> effects, RunState runState, MetaProgression meta) {
		foreach (var effect in effects) {
			ApplyEffect(effect, runState, meta);
		}

		OnEffectsApplied?.Invoke(effects);
	}

	private void ApplyEffect(OutcomeEffect effect, RunState runState, MetaProgression meta) {
		var liveCharacter = runState.Character;
		switch (effect.Type) {
			// Resources
			case OutcomeEffectType.GainGold:
				runState.Character.BaseStats.Add(Ids.Stats.Expedition.GoldOnHand, effect.Value);
				k_metrics.Add(MetricType.GoldEarned, effect.Value);
				break;

			case OutcomeEffectType.LoseGold:
				runState.Character.BaseStats.Subtract(Ids.Stats.Expedition.GoldOnHand, effect.Value);
				k_metrics.Add(MetricType.GoldSpent, effect.Value);
				break;

			case OutcomeEffectType.GainFood:
				runState.Character.BaseStats.Add(Ids.Stats.Expedition.FoodOnHand, effect.Value);
				break;

			case OutcomeEffectType.LoseFood:
				runState.Character.BaseStats.Subtract(Ids.Stats.Expedition.FoodOnHand, effect.Value);
				break;

			case OutcomeEffectType.GainItem:
				// TODO: Add to inventory via InventoryManager
				k_metrics.Increment(MetricType.ItemsPickedUp);
				break;

			case OutcomeEffectType.LoseItem:
				// TODO: Remove from inventory via InventoryManager
				break;

			// Character
			case OutcomeEffectType.Heal:
				float currentHp = runState.Stats.Get(Ids.Stats.Resource.CurrentHealth);
				float maxHp = runState.Stats.Get(Ids.Stats.Resource.MaxHealth);
				runState.Stats.SetBase(Ids.Stats.Resource.CurrentHealth, Math.Min(currentHp + effect.Value, maxHp));
				k_metrics.Add(MetricType.HealingReceived, effect.Value);
				break;

			case OutcomeEffectType.Damage:
				float hp = runState.Stats.Get(Ids.Stats.Resource.CurrentHealth);
				runState.Stats.SetBase(Ids.Stats.Resource.CurrentHealth, Math.Max(0, hp - effect.Value));
				k_metrics.Add(MetricType.DamageReceived, effect.Value);
				break;

			case OutcomeEffectType.RestoreMana:
				float currentMp = runState.Stats.Get(Ids.Stats.Resource.CurrentMana);
				float maxMp = runState.Stats.Get(Ids.Stats.Resource.MaxMana);
				runState.Stats.SetBase(Ids.Stats.Resource.CurrentMana, Math.Min(currentMp + effect.Value, maxMp));
				break;

			case OutcomeEffectType.DrainMana:
				float mp = runState.Stats.Get(Ids.Stats.Resource.CurrentMana);
				runState.Stats.SetBase(Ids.Stats.Resource.CurrentMana, Math.Max(0, mp - effect.Value));
				break;

			case OutcomeEffectType.GainExperience:
				int levels = runState.AddExperience(effect.Value);
				if (levels > 0) {
					k_metrics.Add(MetricType.LevelUpsEarned, levels);
				}
				break;

			case OutcomeEffectType.GainMorale:
				runState.Morale = Math.Min(100, runState.Morale + effect.Value);
				break;

			case OutcomeEffectType.LoseMorale:
				runState.Morale = Math.Max(0, runState.Morale - effect.Value);
				if (runState.Morale == 0) {
					k_metrics.Increment(MetricType.TimesMoraleHitZero);
				}
				break;

			case OutcomeEffectType.AddFatigue:
				runState.Fatigue = Math.Clamp(runState.Fatigue + effect.Value, 0, 100);
				break;

			// Buffs
			case OutcomeEffectType.ApplyBuff:
				// TODO: Apply buff via buff system
				break;

			case OutcomeEffectType.RemoveBuff:
				// TODO: Remove buff via buff system
				break;

			case OutcomeEffectType.ApplyDebuff:
				// TODO: Apply debuff via buff system
				break;

			// Game State
			case OutcomeEffectType.SetFlag:
				runState.ActiveConditions.Add(effect.Parameter);
				break;

			case OutcomeEffectType.ClearFlag:
				runState.ActiveConditions.Remove(effect.Parameter);
				break;

			case OutcomeEffectType.UnlockLore:
				runState.DiscoveredLore.Add(effect.Parameter);
				meta.DiscoveredLore.Add(effect.Parameter);
				k_metrics.Increment(MetricType.LoreEntriesDiscovered);
				break;

			case OutcomeEffectType.StartCombat:
				OnCombatTriggered?.Invoke(effect.Parameter);
				break;

			case OutcomeEffectType.TriggerEvent:
				OnEventChained?.Invoke(effect.Parameter);
				break;

			case OutcomeEffectType.AdvanceTime:
				GameTime.Instance.AdvanceTicks(effect.Value * GameTime.TICKS_PER_HOUR);
				break;

			case OutcomeEffectType.Teleport:
				// TODO: Implement teleportation via ExpeditionManager
				break;

			// Meta
			case OutcomeEffectType.UnlockClass:
				if (!meta.UnlockedClasses.Contains(effect.Parameter)) {
					meta.UnlockedClasses.Add(effect.Parameter);
					k_metrics.Increment(MetricType.ClassesUnlocked);
				}
				break;

			case OutcomeEffectType.UnlockBuilding:
				if (!meta.UnlockedBuildings.Contains(effect.Parameter)) {
					meta.UnlockedBuildings.Add(effect.Parameter);
					k_metrics.Increment(MetricType.VillageBuildingsBuilt);
				}
				break;

			case OutcomeEffectType.GainUpgradePoints:
				meta.VillageUpgradePoints += effect.Value;
				break;

			// ═══════════════════════════════════════════════════════════════
			// ITEM REWARDS (Uses RewardManager + Spinner UI)
			// ═══════════════════════════════════════════════════════════════

			case OutcomeEffectType.GainClassWeapon:
				if (k_rewardManager != null) {
					var weaponReward = k_rewardManager.GenerateClassWeaponReward(
						runState.CharacterClassId,
						effect.Value
					);
					if (weaponReward.IsValid) {
						// Fire event to show reward UI with spinner
						OnRewardPending?.Invoke(weaponReward, instance => {
							if (instance != null) {
								UnityEngine.Debug.Log($"Weapon reward claimed: {instance.DisplayName}");
								k_metrics.Increment(MetricType.ItemsPickedUp);
							}
						});
					} else {
						UnityEngine.Debug.LogWarning($"Failed to generate weapon reward for class {runState.CharacterClassId}");
					}
				} else {
					UnityEngine.Debug.LogWarning("RewardManager not set, cannot generate weapon reward");
				}
				break;

			case OutcomeEffectType.GainClassArmor:
				if (k_rewardManager != null) {
					var armorReward = k_rewardManager.GenerateClassArmorReward(
						runState.CharacterClassId,
						effect.Value
					);
					if (armorReward.IsValid) {
						// Fire event to show reward UI with spinner
						OnRewardPending?.Invoke(armorReward, instance => {
							if (instance != null) {
								UnityEngine.Debug.Log($"Armor reward claimed: {instance.DisplayName}");
								k_metrics.Increment(MetricType.ItemsPickedUp);
							}
						});
					} else {
						UnityEngine.Debug.LogWarning($"Failed to generate armor reward for class {runState.CharacterClassId}");
					}
				} else {
					UnityEngine.Debug.LogWarning("RewardManager not set, cannot generate armor reward");
				}
				break;

			case OutcomeEffectType.GainClassSpell:
				// TODO: Implement spell reward via SpellManager
				UnityEngine.Debug.Log($"Spell reward tier {effect.Value} (not yet implemented)");
				break;

			case OutcomeEffectType.AddRunModifier:
				// TODO: Apply run modifier to RunState
				// The parameter contains the modifier type, FloatValue contains the value
				UnityEngine.Debug.Log($"Run modifier: {effect.Parameter} = {effect.FloatValue}");
				break;
		}
	}

	private bool CanPayCost(ChoiceCost cost, RunState runState) {
		var liveCharacter = runState.Character;
		return cost.Type switch {
			ChoiceCostType.Gold => liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.GoldOnHand) >= cost.Amount,
			ChoiceCostType.Food => liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.FoodOnHand) >= cost.Amount,
			ChoiceCostType.Health => liveCharacter.BaseStats.Get(Ids.Stats.Resource.CurrentHealth) > cost.Amount,
			ChoiceCostType.Mana => liveCharacter.BaseStats.Get(Ids.Stats.Resource.CurrentMana) >= cost.Amount,
			ChoiceCostType.Morale => liveCharacter.BaseStats.Get(Ids.Stats.Resource.Morale) >= cost.Amount,
			ChoiceCostType.Item => true, // TODO: Check inventory
			_ => true
		};
	}

	private void PayCost(ChoiceCost cost, RunState runState) {
		var liveCharacter = runState.Character;
		switch (cost.Type) {
			case ChoiceCostType.Gold:
				liveCharacter.BaseStats.Subtract(Ids.Stats.Expedition.GoldOnHand, cost.Amount);
				k_metrics.Add(MetricType.GoldSpent, cost.Amount);
				break;

			case ChoiceCostType.Food:
				liveCharacter.BaseStats.Subtract(Ids.Stats.Expedition.FoodOnHand, cost.Amount);
				break;

			case ChoiceCostType.Health:
				float hp = liveCharacter.BaseStats.Get(Ids.Stats.Resource.CurrentHealth);
				liveCharacter.BaseStats.Set(Ids.Stats.Resource.CurrentHealth, Math.Max(1, hp - cost.Amount));
				break;

			case ChoiceCostType.Mana:
				float mp = liveCharacter.BaseStats.Get(Ids.Stats.Resource.CurrentMana);
				liveCharacter.BaseStats.Set(Ids.Stats.Resource.CurrentMana, Math.Max(0, mp - cost.Amount));
				break;

			case ChoiceCostType.Morale:
				liveCharacter.BaseStats.Set(Ids.Stats.Resource.Morale, Math.Max(0, runState.Morale - cost.Amount));
				break;

			case ChoiceCostType.Item:
				// TODO: Remove item from inventory
				break;
		}
	}

	#endregion

	#region Metrics

	private void TrackChoiceMetrics(EventChoiceProto choice, EventOutcomeProto? outcome, bool success) {
		// Track specific choice types by ID pattern
		if (choice.ChoiceId.Contains("fight") || choice.ChoiceId.Contains("attack")) {
			k_metrics.Increment(MetricType.AggressiveChoices);
		} else if (choice.ChoiceId.Contains("talk") || choice.ChoiceId.Contains("peace") || choice.ChoiceId.Contains("negotiate")) {
			k_metrics.Increment(MetricType.PeacefulChoices);
		} else if (choice.ChoiceId.Contains("bribe")) {
			k_metrics.Increment(MetricType.BribesAttempted);
			if (success) {
				k_metrics.Increment(MetricType.BribesSuccessful);
			}
		} else if (choice.ChoiceId.Contains("stealth") || choice.ChoiceId.Contains("sneak")) {
			k_metrics.Increment(MetricType.StealthAttempts);
			if (success) {
				k_metrics.Increment(MetricType.StealthSuccesses);
			}
		} else if (choice.ChoiceId.Contains("persuade") || choice.ChoiceId.Contains("convince")) {
			k_metrics.Increment(MetricType.PersuasionAttempts);
			if (success) {
				k_metrics.Increment(MetricType.PersuasionSuccesses);
			}
		} else if (choice.ChoiceId.Contains("intimidate") || choice.ChoiceId.Contains("threaten")) {
			k_metrics.Increment(MetricType.IntimidationAttempts);
			if (success) {
				k_metrics.Increment(MetricType.IntimidationSuccesses);
			}
		}

		// Track skill checks
		if (choice.Condition?.Type == ConditionType.SkillCheck) {
			k_metrics.Increment(MetricType.SkillChecksAttempted);
			if (success) {
				k_metrics.Increment(MetricType.SkillChecksPassed);
			} else {
				k_metrics.Increment(MetricType.SkillChecksFailed);
			}
		}
	}

	#endregion

	#region Queries

	/// <summary>
	/// Gets the current event state for UI.
	/// </summary>
	public EventState GetEventState(RunState runState, MetaProgression meta) {
		if (!IsEventActive || k_currentEvent == null) {
			return new EventState { IsActive = false };
		}

		var availableChoices = new List<ChoiceAvailability>();
		foreach (var choice in k_currentEvent.Choices) {
			bool conditionMet = choice.Condition == null || choice.Condition.Evaluate(runState, meta).Passed;
			bool canAfford = choice.Cost == null || CanPayCost(choice.Cost, runState);

			availableChoices.Add(new ChoiceAvailability {
				Choice = choice,
				IsAvailable = conditionMet && canAfford,
				ConditionMet = conditionMet,
				CanAfford = canAfford,
				LockedReason = !conditionMet ? "Requirements not met" : (!canAfford ? "Cannot afford" : null)
			});
		}

		return new EventState {
			IsActive = true,
			Event = k_currentEvent,
			AvailableChoices = availableChoices,
			SelectedChoice = k_selectedChoice,
			CurrentOutcome = k_currentOutcome,
			IsShowingOutcome = IsShowingOutcome
		};
	}

	#endregion

	#region Reset

	/// <summary>
	/// Resets the event manager state (call when run ends).
	/// </summary>
	public void Reset() {
		k_currentEvent = null;
		k_selectedChoice = null;
		k_currentOutcome = null;
		k_eventHistory.Clear();
		k_eventCooldowns.Clear();
		UnityEngine.Debug.Log("EventManager: Reset");
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// Current state of the event system for UI.
/// </summary>
public class EventState {
	public bool IsActive { get; init; }
	public EventProto? Event { get; init; }
	public List<ChoiceAvailability> AvailableChoices { get; init; } = [];
	public EventChoiceProto? SelectedChoice { get; init; }
	public EventOutcomeProto? CurrentOutcome { get; init; }
	public bool IsShowingOutcome { get; init; }
}

#endregion