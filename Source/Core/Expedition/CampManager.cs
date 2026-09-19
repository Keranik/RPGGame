using RPGGame.Core.Characters;
using RPGGame.Core.Metrics;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;
using UnityEngine;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Manages camping, resting, and recovery during expeditions.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class CampManager {
	#region Fields

	private readonly MetricsManager k_metrics;
	private readonly GameDb k_gameDb;
	private readonly CharacterManager k_characterManager;

	private CampQuality k_quality = CampQuality.Basic;
	private int k_hoursRested;

	#endregion

	#region Properties

	/// <summary>
	/// Whether the player is currently camped.
	/// </summary>
	public bool IsCamped { get; private set; }

	/// <summary>
	/// Current camp quality (affects recovery rates).
	/// </summary>
	public CampQuality Quality => k_quality;

	/// <summary>
	/// Hours spent in current camp.
	/// </summary>
	public int HoursCamped { get; private set; }

	/// <summary>
	/// Whether a campfire is lit.
	/// </summary>
	public bool HasCampfire { get; private set; }

	/// <summary>
	/// Available camp activities.
	/// </summary>
	public List<CampActivity> AvailableActivities { get; } = [];

	#endregion

	#region Events

	/// <summary>
	/// Fired when camp is set up.
	/// </summary>
	public event Action<CampQuality>? OnCampSetUp;

	/// <summary>
	/// Fired when camp is broken down.
	/// </summary>
	public event Action? OnCampBroken;

	/// <summary>
	/// Fired when an activity is completed.
	/// </summary>
	public event Action<CampActivity, CampActivityResult>? OnActivityCompleted;

	/// <summary>
	/// Fired when rest is complete.
	/// </summary>
	public event Action<RestResult>? OnRestComplete;

	/// <summary>
	/// Fired when a level up is applied.
	/// </summary>
	public event Action<int>? OnLevelUpApplied;

	/// <summary>
	/// Fired when a camp event occurs.
	/// </summary>
	public event Action<CampEvent>? OnCampEvent;

	#endregion

	#region Constructor

	public CampManager(MetricsManager metrics, GameDb gameDb, CharacterManager characterManager) {
		k_metrics = metrics;
		k_gameDb = gameDb;
		k_characterManager = characterManager;
		InitializeActivities();
	}

	private void InitializeActivities() {
		AvailableActivities.AddRange([
			new CampActivity {
				Id = "rest_short",
				Name = "Short Rest",
				Description = "Rest for 1 hour. Recover some health and reduce fatigue.",
				Duration = 1,
				Type = CampActivityType.Rest
			},
			new CampActivity {
				Id = "rest_long",
				Name = "Long Rest",
				Description = "Sleep for 8 hours. Full recovery.",
				Duration = 8,
				Type = CampActivityType.Rest
			},
			new CampActivity {
				Id = "cook",
				Name = "Cook Meal",
				Description = "Prepare a hot meal. Bonus morale if you have ingredients.",
				Duration = 1,
				Type = CampActivityType.Cooking,
				RequiresCampfire = true
			},
			new CampActivity {
				Id = "craft",
				Name = "Craft Items",
				Description = "Create items from gathered materials.",
				Duration = 2,
				Type = CampActivityType.Crafting
			},
			new CampActivity {
				Id = "repair",
				Name = "Repair Equipment",
				Description = "Restore durability to your gear.",
				Duration = 1,
				Type = CampActivityType.Repair
			},
			new CampActivity {
				Id = "forage",
				Name = "Forage",
				Description = "Search the area for food and herbs.",
				Duration = 2,
				Type = CampActivityType.Foraging
			},
			new CampActivity {
				Id = "hunt",
				Name = "Hunt",
				Description = "Hunt for food. Requires hunting skill.",
				Duration = 3,
				Type = CampActivityType.Hunting
			},
			new CampActivity {
				Id = "fish",
				Name = "Fish",
				Description = "Try to catch fish if near water.",
				Duration = 2,
				Type = CampActivityType.Fishing
			},
			new CampActivity {
				Id = "study",
				Name = "Study Lore",
				Description = "Review discovered lore and notes.",
				Duration = 1,
				Type = CampActivityType.Study
			},
			new CampActivity {
				Id = "meditate",
				Name = "Meditate",
				Description = "Clear your mind. Recover mana faster.",
				Duration = 1,
				Type = CampActivityType.Rest
			},
			new CampActivity {
				Id = "level_up",
				Name = "Level Up",
				Description = "Apply pending level ups.",
				Duration = 1,
				Type = CampActivityType.LevelUp,
				RequiresPendingLevelUp = true
			}
		]);
	}

	#endregion

	#region Camp Setup

	/// <summary>
	/// Sets up camp at the current location.
	/// </summary>
	public bool SetupCamp(PathNodeType locationType) {
		if (IsCamped) {
			return false;
		}

		// Determine camp quality based on location
		k_quality = locationType switch {
			PathNodeType.CampSite => CampQuality.Good,
			PathNodeType.Settlement => CampQuality.Excellent,
			PathNodeType.Village => CampQuality.Excellent,
			_ => CampQuality.Basic
		};

		IsCamped = true;
		k_hoursRested = 0;

		OnCampSetUp?.Invoke(k_quality);
		Debug.Log($"Camp set up: {k_quality} quality");

		return true;
	}

	/// <summary>
	/// Lights a campfire (uses torch from RunState).
	/// </summary>
	// TODO: This needs to actually have camp/torch item and supplies logic
	public bool LightCampfire(RunState runState) {
		if (!IsCamped) {
			return false;
		}
		if (HasCampfire) {
			return true;
		}

		return false;
	}

	/// <summary>
	/// Breaks down camp and prepares to leave.
	/// </summary>
	public void BreakCamp() {
		if (!IsCamped) {
			return;
		}

		IsCamped = false;
		HasCampfire = false;
		HoursCamped = 0;
		k_quality = CampQuality.Basic;

		OnCampBroken?.Invoke();
		Debug.Log("Camp broken down");
	}

	/// <summary>
	/// Determines camp quality based on location and conditions.
	/// </summary>
	private CampQuality DetermineCampQuality(PathNodeType locationType, RunState runState) {
		// Base quality from location
		int qualityScore = locationType switch {
			PathNodeType.Settlement => 4,
			PathNodeType.CampSite => 3,
			PathNodeType.Village => 5,
			_ => 1
		};

		// Bonus for campfire
		if (HasCampfire) {
			qualityScore++;
		}

		// Bonus for high morale
		if (runState.Morale >= 80) {
			qualityScore++;
		}

		return qualityScore switch {
			>= 5 => CampQuality.Excellent,
			4 => CampQuality.Good,
			3 => CampQuality.Comfortable,
			2 => CampQuality.Basic,
			_ => CampQuality.Poor
		};
	}

	#endregion

	#region Activities

	/// <summary>
	/// Gets activities available at the current camp.
	/// </summary>
	public IEnumerable<CampActivity> GetAvailableActivities(RunState runState, bool hasCampfire) {
		foreach (var activity in AvailableActivities) {
			if (activity.RequiresCampfire && !hasCampfire) {
				continue;
			}
			if (activity.RequiresPendingLevelUp && !runState.HasPendingLevelUp) {
				continue;
			}

			yield return activity;
		}
	}

	/// <summary>
	/// Performs a camp activity.
	/// </summary>
	public CampActivityResult PerformActivity(CampActivity activity, RunState runState) {
		if (!IsCamped) {
			return new CampActivityResult {
				Success = false,
				Message = "Must set up camp first!"
			};
		}

		var result = new CampActivityResult {
			Activity = activity,
			HoursSpent = activity.Duration
		};

		switch (activity.Type) {
			case CampActivityType.Rest:
				result = PerformRest(activity, runState);
				break;

			case CampActivityType.LevelUp:
				result = PerformLevelUp(runState);
				break;

			case CampActivityType.Cooking:
				result = PerformCooking(runState);
				break;

			case CampActivityType.Foraging:
				result = PerformForaging(runState);
				break;

			case CampActivityType.Hunting:
				result = PerformHunting(runState);
				break;

			case CampActivityType.Fishing:
				result = PerformFishing(runState);
				break;

			default:
				result.Success = true;
				result.Message = $"Spent {activity.Duration} hours on {activity.Name}.";
				break;
		}

		HoursCamped += activity.Duration;
		k_metrics.Add(MetricType.HoursRested, activity.Duration);

		OnActivityCompleted?.Invoke(activity, result);
		return result;
	}

	#endregion

	#region Skill Checks

	/// <summary>
	/// Gets the skill check modifier for a skill.
	/// </summary>
	private int GetSkillCheckModifier(RunState runState, SkillProto.ID skillId) {
		if (!k_gameDb.TryGetProto<SkillProto>(skillId, out var skillProto)) {
			return 0;
		}

		var governingAttribute = GetGoverningAttribute(skillProto);
		if (governingAttribute == null) {
			return 0;
		}

		float attributeValue = runState.Stats.Get(governingAttribute.Value);
		int attributeMod = StatManager.GetAttributeModifier(attributeValue);

		return attributeMod;
	}

	/// <summary>
	/// Gets the governing attribute for a skill based on its attribute tag.
	/// </summary>
	private Prototypes.Stats.StatProto.ID? GetGoverningAttribute(SkillProto skillProto) {
		if (skillProto.HasTag(Ids.Tags.Attribute.Strength)) {
			return Ids.Stats.Attributes.Strength;
		}
		if (skillProto.HasTag(Ids.Tags.Attribute.Dexterity)) {
			return Ids.Stats.Attributes.Dexterity;
		}
		if (skillProto.HasTag(Ids.Tags.Attribute.Constitution)) {
			return Ids.Stats.Attributes.Constitution;
		}
		if (skillProto.HasTag(Ids.Tags.Attribute.Intelligence)) {
			return Ids.Stats.Attributes.Intelligence;
		}
		if (skillProto.HasTag(Ids.Tags.Attribute.Wisdom)) {
			return Ids.Stats.Attributes.Wisdom;
		}
		if (skillProto.HasTag(Ids.Tags.Attribute.Charisma)) {
			return Ids.Stats.Attributes.Charisma;
		}

		return null;
	}

	#endregion

	#region Rest

	/// <summary>
	/// Performs rest activity.
	/// </summary>
	public CampActivityResult PerformRest(CampActivity activity, RunState runState) {
		int hours = activity.Duration;
		float qualityMod = k_quality.GetRecoveryMultiplier();

		// Health recovery
		float healthRecovery = hours * 2 * qualityMod;
		int conMod = StatManager.GetAttributeModifier(runState.Stats.Get(Ids.Stats.Attributes.Constitution));
		healthRecovery *= (1 + conMod * 0.1f);

		float currentHealth = runState.Stats.Get(Ids.Stats.Resource.CurrentHealth);
		float maxHealth = runState.Stats.Get(Ids.Stats.Resource.MaxHealth);
		float newHealth = Math.Min(maxHealth, currentHealth + healthRecovery);
		float actualHealthRecovered = newHealth - currentHealth;
		runState.Stats.Set(Ids.Stats.Resource.CurrentHealth, newHealth);

		// Mana recovery
		float manaRecovery = hours * 3 * qualityMod;
		float currentMana = runState.Stats.Get(Ids.Stats.Resource.CurrentMana);
		float maxMana = runState.Stats.Get(Ids.Stats.Resource.MaxMana);
		runState.Stats.Set(Ids.Stats.Resource.CurrentMana, Math.Min(maxMana, currentMana + manaRecovery));

		// Fatigue reduction - use CharacterManager
		float fatigueReduction = hours * 12 * qualityMod;
		if (runState.Character != null) {
			k_characterManager.ReduceFatigue(runState.Character, fatigueReduction);
		}

		// Morale recovery
		float moraleRecovery = hours * 2 * qualityMod;
		if (HasCampfire) {
			moraleRecovery += 5;
		}
		runState.Morale = Math.Clamp(runState.Morale + moraleRecovery, 0, 100);

		// Update run state
		runState.Rest(hours);

		var restResult = new RestResult {
			HoursRested = hours,
			HealthRecovered = actualHealthRecovered,
			ManaRecovered = manaRecovery,
			FatigueReduced = fatigueReduction,
			MoraleRecovered = moraleRecovery,
			WasFullRest = hours >= 8
		};

		OnRestComplete?.Invoke(restResult);

		return new CampActivityResult {
			Success = true,
			Activity = activity,
			HoursSpent = hours,
			Message = $"Rested for {hours} hours. Recovered {actualHealthRecovered:F0} HP.",
			HealthChange = actualHealthRecovered,
			MoraleChange = moraleRecovery,
			FatigueChange = -fatigueReduction
		};
	}

	#endregion

	#region Level Up

	/// <summary>
	/// Performs level up activity.
	/// </summary>
	public CampActivityResult PerformLevelUp(RunState runState) {
		if (!runState.HasPendingLevelUp) {
			return new CampActivityResult {
				Success = false,
				Message = "No level ups pending."
			};
		}

		int levelsApplied = 0;
		while (runState.PendingLevelUps > 0) {
			runState.PendingLevelUps--;
			runState.HasPendingLevelUp = runState.PendingLevelUps > 0;

			runState.Stats.Add(Ids.Stats.Meta.Level, 1);
			runState.Stats.Set(Ids.Stats.Resource.CurrentHealth, runState.MaxHealth);
			runState.Stats.Set(Ids.Stats.Resource.CurrentMana, runState.MaxMana);

			levelsApplied++;
		}

		k_metrics.Add(MetricType.LevelUpsEarned, levelsApplied);
		OnLevelUpApplied?.Invoke(levelsApplied);

		int newLevel = runState.Stats.GetInt(Ids.Stats.Meta.Level);

		return new CampActivityResult {
			Success = true,
			HoursSpent = 1,
			Message = $"Applied {levelsApplied} level up(s)! Now level {newLevel}.",
			LevelsGained = levelsApplied
		};
	}

	#endregion

	#region Gathering Activities

	private CampActivityResult PerformCooking(RunState runState) {
		var liveCharacter = runState.Character;
		bool hasIngredients = liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.FoodOnHand) >= 1;

		if (!hasIngredients) {
			return new CampActivityResult {
				Success = false,
				Message = "No ingredients to cook with."
			};
		}

		float moraleBonus = 10f;
		if (HasCampfire) {
			moraleBonus += 5f;
		}

		runState.Morale = Math.Clamp(runState.Morale + moraleBonus, 0, 100);

		return new CampActivityResult {
			Success = true,
			HoursSpent = 1,
			Message = $"Cooked a warm meal. +{moraleBonus:F0} morale.",
			MoraleChange = moraleBonus
		};
	}

	private CampActivityResult PerformForaging(RunState runState) {
		int survivalMod = GetSkillCheckModifier(runState, Ids.Skills.Survival.General);
		int roll = UnityEngine.Random.Range(1, 21) + survivalMod;

		int foodFound = 0;
		int herbsFound = 0;

		if (roll >= 15) {
			foodFound = 2;
			herbsFound = 1;
		} else if (roll >= 10) {
			foodFound = 1;
		} else if (roll >= 5) {
			herbsFound = 1;
		}

		runState.Character.BaseStats.Add(Ids.Stats.Expedition.FoodOnHand, foodFound);
		k_metrics.RecordGathering("herbs", herbsFound);

		string message = foodFound > 0 || herbsFound > 0
			? $"Found {foodFound} food and {herbsFound} herbs."
			: "Found nothing useful.";

		return new CampActivityResult {
			Success = foodFound > 0 || herbsFound > 0,
			HoursSpent = 2,
			Message = message,
			FoodChange = foodFound,
			ItemsGained = herbsFound > 0 ? [("herbs", herbsFound)] : []
		};
	}

	private CampActivityResult PerformHunting(RunState runState) {
		int huntingMod = GetSkillCheckModifier(runState, Ids.Skills.Gathering.Hunting);
		int roll = UnityEngine.Random.Range(1, 21) + huntingMod;

		int foodFound = 0;

		if (roll >= 18) {
			foodFound = 4;
		} else if (roll >= 14) {
			foodFound = 2;
		} else if (roll >= 10) {
			foodFound = 1;
		}

		runState.Character.BaseStats.Add(Ids.Stats.Expedition.FoodOnHand, foodFound);
		if (foodFound > 0) {
			k_metrics.Add(MetricType.AnimalsHunted, 1);
		}

		return new CampActivityResult {
			Success = foodFound > 0,
			HoursSpent = 3,
			Message = foodFound > 0 ? $"Successful hunt! +{foodFound} food." : "The hunt was unsuccessful.",
			FoodChange = foodFound
		};
	}

	private CampActivityResult PerformFishing(RunState runState) {
		int fishingMod = GetSkillCheckModifier(runState, Ids.Skills.Gathering.Fishing);
		int roll = UnityEngine.Random.Range(1, 21) + fishingMod;

		int fishCaught = 0;

		if (roll >= 15) {
			fishCaught = 3;
		} else if (roll >= 10) {
			fishCaught = 2;
		} else if (roll >= 5) {
			fishCaught = 1;
		}

		runState.Character.BaseStats.Add(Ids.Stats.Expedition.FoodOnHand, fishCaught);
		k_metrics.Add(MetricType.FishCaught, fishCaught);

		return new CampActivityResult {
			Success = fishCaught > 0,
			HoursSpent = 2,
			Message = fishCaught > 0 ? $"Caught {fishCaught} fish!" : "No luck fishing today.",
			FoodChange = fishCaught
		};
	}

	#endregion

	#region Camp Events

	/// <summary>
	/// Checks for random camp events (called periodically while camped).
	/// </summary>
	public CampEvent? CheckForCampEvent() {
		if (!IsCamped) {
			return null;
		}

		if (UnityEngine.Random.value > 0.10f) {
			return null;
		}

		var possibleEvents = new List<(CampEvent evt, float weight)> {
			(new CampEvent { Type = CampEventType.NightSound, Message = "You hear strange sounds in the night...", MoraleChange = -5 }, 3f),
			(new CampEvent { Type = CampEventType.Wildlife, Message = "A curious animal approaches the camp.", MoraleChange = 2 }, 2f),
			(new CampEvent { Type = CampEventType.Weather, Message = "The weather takes a turn.", MoraleChange = -3 }, 2f),
			(new CampEvent { Type = CampEventType.Dream, Message = "You have a strange dream...", IsLoreRelated = true }, 1f),
		};

		if (HasCampfire) {
			possibleEvents = possibleEvents.Where(e => e.evt.MoraleChange >= 0).ToList();
		}

		if (possibleEvents.Count == 0) {
			return null;
		}

		float totalWeight = possibleEvents.Sum(e => e.weight);
		float roll = UnityEngine.Random.value * totalWeight;
		float cumulative = 0;

		foreach (var (evt, weight) in possibleEvents) {
			cumulative += weight;
			if (roll <= cumulative) {
				OnCampEvent?.Invoke(evt);
				return evt;
			}
		}

		return null;
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// Quality of the camp affects recovery rates.
/// </summary>
public enum CampQuality {
	Poor,
	Basic,
	Comfortable,
	Good,
	Excellent
}

public static class CampQualityExtensions {
	public static float GetRecoveryMultiplier(this CampQuality quality) {
		return quality switch {
			CampQuality.Poor => 0.5f,
			CampQuality.Basic => 1.0f,
			CampQuality.Comfortable => 1.25f,
			CampQuality.Good => 1.5f,
			CampQuality.Excellent => 2.0f,
			_ => 1.0f
		};
	}

	public static string GetDisplayName(this CampQuality quality) {
		return quality switch {
			CampQuality.Poor => "Poor Camp",
			CampQuality.Basic => "Basic Camp",
			CampQuality.Comfortable => "Comfortable Camp",
			CampQuality.Good => "Good Camp",
			CampQuality.Excellent => "Excellent Camp",
			_ => "Camp"
		};
	}
}

/// <summary>
/// Types of camp activities.
/// </summary>
public enum CampActivityType {
	Rest,
	Cooking,
	Crafting,
	Repair,
	Foraging,
	Hunting,
	Fishing,
	Study,
	LevelUp
}

/// <summary>
/// An activity that can be performed while camped.
/// </summary>
public class CampActivity {
	public string Id { get; init; } = "";
	public string Name { get; init; } = "";
	public string Description { get; init; } = "";
	public int Duration { get; init; } = 1;
	public CampActivityType Type { get; init; }
	public bool RequiresCampfire { get; init; }
	public bool RequiresPendingLevelUp { get; init; }
	public string IconName { get; init; } = "icon_activity";
}

/// <summary>
/// Result of performing a camp activity.
/// </summary>
public class CampActivityResult {
	public bool Success { get; set; }
	public CampActivity? Activity { get; set; }
	public int HoursSpent { get; set; }
	public string Message { get; set; } = "";
	public float HealthChange { get; set; }
	public float ManaChange { get; set; }
	public float MoraleChange { get; set; }
	public float FatigueChange { get; set; }
	public int FoodChange { get; set; }
	public int LevelsGained { get; set; }
	public List<(string itemId, int count)> ItemsGained { get; set; } = [];
}

/// <summary>
/// Result of resting.
/// </summary>
public class RestResult {
	public int HoursRested { get; set; }
	public float HealthRecovered { get; set; }
	public float ManaRecovered { get; set; }
	public float FatigueReduced { get; set; }
	public float MoraleRecovered { get; set; }
	public bool WasFullRest { get; set; }
}

/// <summary>
/// A random event that can occur while camping.
/// </summary>
public class CampEvent {
	public CampEventType Type { get; set; }
	public string Message { get; set; } = "";
	public float MoraleChange { get; set; }
	public float HealthChange { get; set; }
	public bool IsLoreRelated { get; set; }
	public string? EventId { get; set; }
}

/// <summary>
/// Types of camp events.
/// </summary>
public enum CampEventType {
	NightSound,
	Wildlife,
	Weather,
	Dream,
	Visitor,
	Discovery,
	Ambush
}

#endregion