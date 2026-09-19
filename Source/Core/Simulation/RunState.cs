using RPGGame.Core.Characters;
using RPGGame.Core.Expedition;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;
using UnityEngine;

namespace RPGGame.Core.Simulation;

/// <summary>
/// Contains all data for the current run that will be lost on death/time rewind.
/// This is the "per-run" save state.
/// 
/// Character-related stats alias to Character.BaseStats for single source of truth.
/// </summary>
public class RunState {
	#region Character

	/// <summary>
	/// The current player character (runtime representation with all stats, skills, spells).
	/// </summary>
	public LiveCharacter Character { get; set; } = null!;

	/// <summary>
	/// Convenience accessor for character's stats. Delegates to Character.BaseStats.
	/// </summary>
	public StatValues Stats => Character.BaseStats;

	/// <summary>
	/// Convenience accessor for character's modifiers. Delegates to Character.Modifiers.
	/// </summary>
	public List<StatModifier> Modifiers => Character.Modifiers;

	/// <summary>
	/// The character's class ID.
	/// </summary>
	public string CharacterClassId { get; set; } = "fighter";
	
	/// <summary>
	/// Whether a level-up is pending (waiting for camp).
	/// </summary>
	public bool HasPendingLevelUp { get; set; }

	/// <summary>
	/// Number of pending level-ups (can queue multiple).
	/// </summary>
	public int PendingLevelUps { get; set; }

	/// <summary>
	/// Gets the D&D-style attribute modifier for an attribute stat.
	/// Formula: (attribute - 10) / 2, rounded down.
	/// </summary>
	public int GetAttributeModifier(StatProto.ID attributeStat) {
		float attributeValue = Stats.Get(attributeStat, 10);
		return (int)MathF.Floor((attributeValue - 10f) / 2f);
	}

	#endregion

	#region Inventory 

	/// <summary>
	/// Player's inventory reference data.
	/// Actual items managed by InventoryManager.
	/// </summary>
	public Inventory Inventory { get; set; } = new();

	#endregion

	#region Position & Exploration

	/// <summary>
	/// Current position on the local map (tile coordinates).
	/// </summary>
	public Vector2Int CurrentPosition { get; set; }

	/// <summary>
	/// Current region/area ID.
	/// </summary>
	public string CurrentRegionId { get; set; } = "starting_road";

	/// <summary>
	/// Current location ID (if in a specific location like a building).
	/// </summary>
	public string? CurrentLocationId { get; set; }

	/// <summary>
	/// Distance traveled from village (in tiles).
	/// </summary>
	public float DistanceFromVillage { get; set; }

	/// <summary>
	/// Total distance traveled this run.
	/// </summary>
	public float TotalDistanceTraveled { get; set; }

	/// <summary>
	/// Tiles revealed this run (for fog of war).
	/// Will be transferred to meta-progression on death.
	/// </summary>
	public HashSet<Vector2Int> RevealedTiles { get; set; } = [];

	/// <summary>
	/// Path nodes visited (for tracking progress along roads).
	/// </summary>
	public List<PathNodeId> VisitedPathNodes { get; set; } = [];

	/// <summary>
	/// Events encountered this run (by ID).
	/// </summary>
	public HashSet<string> EncounteredEvents { get; set; } = [];

	#endregion

	#region Time & Status

	/// <summary>
	/// Current in-game day.
	/// </summary>
	[Obsolete("Use GameTime.Instance.Day instead")]
	public int CurrentDay { get; set; } = 1;

	/// <summary>
	/// Current in-game hour (0-23).
	/// </summary>
	[Obsolete("Use GameTime.Hour instead")]
	public int CurrentHour { get; set; } = 8; // Start at 8 AM

	/// <summary>
	/// Current time of day period.
	/// </summary>
	public TimeOfDayPeriod TimeOfDay => GameTime.Instance.Hour switch {
		>= 5 and < 7 => TimeOfDayPeriod.Dawn,
		>= 7 and < 12 => TimeOfDayPeriod.Morning,
		>= 12 and < 14 => TimeOfDayPeriod.Midday,
		>= 14 and < 17 => TimeOfDayPeriod.Afternoon,
		>= 17 and < 19 => TimeOfDayPeriod.Dusk,
		>= 19 and < 22 => TimeOfDayPeriod.Evening,
		_ => TimeOfDayPeriod.Night // >= 22 or < 5
	};

	/// <summary>
	/// Current morale (0-100). Aliases to Character.BaseStats.
	/// </summary>
	public float Morale {
		get => Character?.BaseStats.Get(Ids.Stats.Resource.Morale) ?? 100f;
		set => Character?.BaseStats.Set(Ids.Stats.Resource.Morale, value);
	}

	/// <summary>
	/// Current fatigue (0-100, higher = more tired). Aliases to Character.BaseStats.
	/// </summary>
	public float Fatigue {
		get => Character?.BaseStats.Get(Ids.Stats.Resource.Fatigue) ?? 0f;
		set => Character?.BaseStats.Set(Ids.Stats.Resource.Fatigue, value);
	}

	/// <summary>
	/// Hours since last rest.
	/// </summary>
	public int HoursSinceRest { get; set; }

	/// <summary>
	/// Days since last full rest (8+ hours sleep).
	/// </summary>
	public int DaysSinceFullRest { get; set; }

	/// <summary>
	/// Current travel speed multiplier. Aliases to Character.BaseStats.
	/// </summary>
	public float TravelSpeedMultiplier {
		get => Character?.BaseStats.Get(Ids.Stats.Movement.MovementSpeed, 100f) / 100f ?? 1f;
		set => Character?.BaseStats.Set(Ids.Stats.Movement.MovementSpeed, value * 100f);
	}

	/// <summary>
	/// Whether the player is currently traveling (auto-moving).
	/// </summary>
	public bool IsTraveling { get; set; }

	/// <summary>
	/// Whether travel is paused.
	/// </summary>
	public bool IsTravelPaused { get; set; }

	#endregion

	#region Active Effects

	/// <summary>
	/// Active buffs and debuffs with remaining duration.
	/// </summary>
	public List<ActiveEffect> ActiveEffects { get; set; } = [];

	/// <summary>
	/// Active conditions (poisoned, bleeding, blessed, etc.).
	/// </summary>
	public HashSet<string> ActiveConditions { get; set; } = [];

	#endregion

	#region Combat

	/// <summary>
	/// Whether currently in combat.
	/// </summary>
	public bool IsInCombat { get; set; }

	/// <summary>
	/// Current combat state (if in combat).
	/// </summary>
	public CombatStateData? CurrentCombat { get; set; }

	#endregion

	#region Discoveries

	/// <summary>
	/// Lore entries discovered this run.
	/// Will be transferred to meta-progression.
	/// </summary>
	public HashSet<string> DiscoveredLore { get; set; } = [];

	/// <summary>
	/// Secrets found this run.
	/// </summary>
	public HashSet<string> FoundSecrets { get; set; } = [];

	/// <summary>
	/// Landmarks discovered this run.
	/// </summary>
	public HashSet<string> DiscoveredLandmarks { get; set; } = [];

	#endregion

	#region World State

	/// <summary>
	/// Seed used for this run's world generation.
	/// </summary>
	public int WorldSeed { get; set; }

	/// <summary>
	/// Total number of combat encounters this run (used for deterministic combat seeding).
	/// </summary>
	public int TotalEncounters { get; set; }

	/// <summary>
	/// Random state for deterministic generation.
	/// </summary>
	public int RandomState { get; set; }

	/// <summary>
	/// Chunks that have been generated.
	/// </summary>
	public HashSet<Vector2Int> GeneratedChunks { get; set; } = [];

	/// <summary>
	/// Events that have been placed (for persistence).
	/// </summary>
	public Dictionary<Vector2Int, string> PlacedEvents { get; set; } = [];

	#endregion

	#region Computed Properties

	/// <summary>
	/// Checks if the run is over (player died).
	/// </summary>
	public bool IsDead => Character?.IsDead ?? Stats.Get(Ids.Stats.Resource.CurrentHealth) <= 0;

	/// <summary>
	/// Gets the experience required for next level.
	/// </summary>
	public int GetExperienceToNextLevel() => (int)Character.ExperienceToNextLevel;

	/// <summary>
	/// Gets the current level.
	/// </summary>
	public int Level => Character?.Level ?? Stats.GetInt(Ids.Stats.Meta.Level, 1);

	/// <summary>
	/// Gets current health.
	/// </summary>
	public float CurrentHealth => Character?.CurrentHealth ?? Stats.Get(Ids.Stats.Resource.CurrentHealth);

	/// <summary>
	/// Gets max health.
	/// </summary>
	public float MaxHealth => Character?.MaxHealth ?? Stats.Get(Ids.Stats.Resource.MaxHealth);

	/// <summary>
	/// Gets current mana.
	/// </summary>
	public float CurrentMana => Character?.CurrentMana ?? Stats.Get(Ids.Stats.Resource.CurrentMana);

	/// <summary>
	/// Gets max mana.
	/// </summary>
	public float MaxMana => Character?.MaxMana ?? Stats.Get(Ids.Stats.Resource.MaxMana);

	/// <summary>
	/// Gets current stamina.
	/// </summary>
	public float CurrentStamina => Character?.CurrentStamina ?? Stats.Get(Ids.Stats.Resource.CurrentStamina);

	/// <summary>
	/// Gets max stamina.
	/// </summary>
	public float MaxStamina => Character?.MaxStamina ?? Stats.Get(Ids.Stats.Resource.MaxStamina);

	#endregion

	#region Factory Methods

	/// <summary>
	/// Creates a new run state for a fresh run.
	/// Character should already have default stats initialized.
	/// </summary>
	public static RunState CreateNew(LiveCharacter character, string classId, int worldSeed) {
		var runState = new RunState {
			Character = character,
			CharacterClassId = classId,
			Inventory = new Inventory(),
			CurrentPosition = Vector2Int.zero,
			WorldSeed = worldSeed,
			RandomState = worldSeed
		};

		runState.Morale = 100.Percent();
		runState.Fatigue = 0f;
		runState.TravelSpeedMultiplier = 100.Percent();
		
		return runState;
	}

	/// <summary>
	/// Calculates XP required for a given level.
	/// </summary>
	public static int CalculateXPToLevel(int level) {
		// Standard RPG curve: 100 * level^1.5
		return (int)(100 * Math.Pow(level, 1.5));
	}

	#endregion

	#region Methods

	/// <summary>
	/// Adds experience and checks for level up.
	/// Returns number of levels gained.
	/// </summary>
	public int AddExperience(int amount) {
		Character.BaseStats.Add(Ids.Stats.Meta.Experience, amount);
		int levelsGained = 0;

		while (Character.Experience >= Character.ExperienceToNextLevel) {
			Character.BaseStats.Subtract(Ids.Stats.Meta.ExperienceAwarded, Character.ExperienceToNextLevel);
			levelsGained++;
			PendingLevelUps++;
			HasPendingLevelUp = true;

			int newLevel = Level + levelsGained;
			Character.BaseStats.Set(Ids.Stats.Meta.ExperienceToNextLevel, CalculateXPToLevel(newLevel + 1));
		}

		return levelsGained;
	}

	/// <summary>
	/// Applies a pending level up.
	/// Returns false if no level ups pending.
	/// </summary>
	public bool ApplyLevelUp(StatManager statManager) {
		if (PendingLevelUps <= 0) {
			return false;
		}

		PendingLevelUps--;
		HasPendingLevelUp = PendingLevelUps > 0;

		// Increase level
		Stats.Add(Ids.Stats.Meta.Level, 1);

		// Recalculate derived stats
		statManager.CalculateDerivedStats(Stats);

		// Heal to full on level up
		Stats.Set(Ids.Stats.Resource.CurrentHealth, MaxHealth);
		Stats.Set(Ids.Stats.Resource.CurrentMana, MaxMana);

		return true;
	}

	/// <summary>
	/// Advances time by the specified hours.
	/// </summary>
	[Obsolete("Use GameLoop to advance time")]
	public void AdvanceTime(int hours) {
		CurrentHour += hours;

		while (CurrentHour >= 24) {
			CurrentHour -= 24;
			CurrentDay++;
		}

		HoursSinceRest += hours;

		// Fatigue increases over time
		Fatigue = Math.Min(100, Fatigue + (hours * 2.5f));

		// Morale decreases slightly over time
		Morale = Math.Max(0, Morale - (hours * 0.5f));
	}

	/// <summary>
	/// Rest for the specified hours.
	/// </summary>
	public void Rest(int hours) {
		GameTime.Instance.AdvanceTicks(hours*GameTime.TICKS_PER_HOUR);

		// Reduce fatigue
		float fatigueReduction = hours * 10f;
		Fatigue = Math.Max(0, Fatigue - fatigueReduction);

		// Recover morale
		float moraleRecovery = hours * 2f;
		Morale = Math.Min(100, Morale + moraleRecovery);

		// Natural healing based on Constitution
		float conValue = Stats.Get(Ids.Stats.Attributes.Constitution);
		float healthRecovery = hours * (conValue / 10f);
		float newHealth = Math.Min(MaxHealth, CurrentHealth + healthRecovery);
		Stats.Set(Ids.Stats.Resource.CurrentHealth, newHealth);

		// Track rest
		HoursSinceRest = 0;
		if (hours >= 8) {
			DaysSinceFullRest = 0;
		}
	}

	/// <summary>
	/// Consumes food for a day. Returns false if out of food.
	/// </summary>
	public bool ConsumeFood() {
		if (Stats.GetInt(Ids.Stats.Expedition.FoodOnHand) <= 0) {
			// Starving - major morale and health penalty
			Morale = Math.Max(0, Morale - 20);
			float damage = MaxHealth * 0.1f;
			Stats.Add(Ids.Stats.Resource.CurrentHealth, -damage);
			Stats.ClampMin(Ids.Stats.Resource.CurrentHealth, 0);
			return false;
		}

		Character.BaseStats.Subtract(Ids.Stats.Expedition.FoodOnHand, 1);
		return true;
	}

	/// <summary>
	/// Adds a stat modifier.
	/// </summary>
	public void AddModifier(StatModifier modifier) {
		Modifiers.Add(modifier);
	}

	/// <summary>
	/// Removes a stat modifier by ID.
	/// </summary>
	public bool RemoveModifier(string modifierId) {
		var modifier = Modifiers.FirstOrDefault(m => m.Id == modifierId);
		if (modifier == null) return false;
		Modifiers.Remove(modifier);
		return true;
	}

	/// <summary>
	/// Removes all modifiers from a source.
	/// </summary>
	public int RemoveModifiersFromSource(string source) {
		int removed = Modifiers.RemoveAll(m => m.Source == source);
		return removed;
	}

	#endregion

	#region Serialization

	/// <summary>
	/// Converts to serializable data.
	/// Note: All aliased properties are serialized via Character.ToData().
	/// </summary>
	public RunStateData ToData() {
		return new RunStateData {
			Character = Character?.ToData() ?? new LiveCharacterData(),
			CharacterClassId = CharacterClassId,
			HasPendingLevelUp = HasPendingLevelUp,
			PendingLevelUps = PendingLevelUps,
			Inventory = Inventory.ToData(),
			CurrentPositionX = CurrentPosition.x,
			CurrentPositionY = CurrentPosition.y,
			CurrentRegionId = CurrentRegionId,
			CurrentLocationId = CurrentLocationId,
			DistanceFromVillage = DistanceFromVillage,
			TotalDistanceTraveled = TotalDistanceTraveled,
			RevealedTiles = RevealedTiles.Select(t => new TileCoord(t.x, t.y)).ToList(),
			VisitedPathNodes = VisitedPathNodes.Select(id => id.Value).ToList(),  // Convert PathNodeId to int
			EncounteredEvents = [.. EncounteredEvents],
			HoursSinceRest = HoursSinceRest,
			DaysSinceFullRest = DaysSinceFullRest,
			IsTraveling = IsTraveling,
			IsTravelPaused = IsTravelPaused,
			ActiveConditions = [.. ActiveConditions],
			DiscoveredLore = [.. DiscoveredLore],
			FoundSecrets = [.. FoundSecrets],
			DiscoveredLandmarks = [.. DiscoveredLandmarks],
			WorldSeed = WorldSeed,
			RandomState = RandomState
		};
	}

	/// <summary>
	/// Creates from serialized data.
	/// Note: All aliased properties are restored via Character.FromData().
	/// </summary>
	public static RunState FromData(RunStateData data) {
		return new RunState {
			Character = LiveCharacter.FromData(data.Character),
			CharacterClassId = data.CharacterClassId,
			HasPendingLevelUp = data.HasPendingLevelUp,
			PendingLevelUps = data.PendingLevelUps,
			Inventory = Inventory.FromData(data.Inventory),
			CurrentPosition = new Vector2Int(data.CurrentPositionX, data.CurrentPositionY),
			CurrentRegionId = data.CurrentRegionId,
			CurrentLocationId = data.CurrentLocationId,
			DistanceFromVillage = data.DistanceFromVillage,
			TotalDistanceTraveled = data.TotalDistanceTraveled,
			RevealedTiles = data.RevealedTiles.Select(t => new Vector2Int(t.X, t.Y)).ToHashSet(),
			VisitedPathNodes = data.VisitedPathNodes.Select(id => new PathNodeId(id)).ToList(),  // Convert int to PathNodeId
			EncounteredEvents = [.. data.EncounteredEvents],
			HoursSinceRest = data.HoursSinceRest,
			DaysSinceFullRest = data.DaysSinceFullRest,
			IsTraveling = data.IsTraveling,
			IsTravelPaused = data.IsTravelPaused,
			ActiveConditions = [.. data.ActiveConditions],
			DiscoveredLore = [.. data.DiscoveredLore],
			FoundSecrets = [.. data.FoundSecrets],
			DiscoveredLandmarks = [.. data.DiscoveredLandmarks],
			WorldSeed = data.WorldSeed,
			RandomState = data.RandomState
		};
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// An active effect/buff/debuff on the player.
/// </summary>
public class ActiveEffect {
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public string Description { get; set; } = "";
	public int RemainingTicks { get; set; }
	public int TotalTicks { get; set; }
	public bool IsBuff { get; set; }
	public string IconName { get; set; } = "";
	public List<string> ModifierIds { get; set; } = [];
}

/// <summary>
/// Combat state for serialization.
/// </summary>
public class CombatStateData {
	public List<string> EnemyIds { get; set; } = [];
	public int CurrentTurnIndex { get; set; }
	public int RoundNumber { get; set; }
	public bool IsPlayerTurn { get; set; }
}

#endregion

#region Serialization Data

/// <summary>
/// Serializable run state data.
/// Note: Character stats are stored in Character.BaseStats and serialized via LiveCharacterData.
/// </summary>
public class RunStateData {
	public LiveCharacterData Character { get; set; } = new();
	public string CharacterClassId { get; set; } = "";
	public bool HasPendingLevelUp { get; set; }
	public int PendingLevelUps { get; set; }
	public InventoryData Inventory { get; set; } = new();
	public int CurrentPositionX { get; set; }
	public int CurrentPositionY { get; set; }
	public string CurrentRegionId { get; set; } = "";
	public string? CurrentLocationId { get; set; }
	public float DistanceFromVillage { get; set; }
	public float TotalDistanceTraveled { get; set; }
	public List<TileCoord> RevealedTiles { get; set; } = [];
	public List<int> VisitedPathNodes { get; set; } = [];
	public List<string> EncounteredEvents { get; set; } = [];
	public int CurrentDay { get; set; }
	public int CurrentHour { get; set; }
	public int HoursSinceRest { get; set; }
	public int DaysSinceFullRest { get; set; }
	public bool IsTraveling { get; set; }
	public bool IsTravelPaused { get; set; }
	public List<string> ActiveConditions { get; set; } = [];
	public List<string> DiscoveredLore { get; set; } = [];
	public List<string> FoundSecrets { get; set; } = [];
	public List<string> DiscoveredLandmarks { get; set; } = [];
	public int WorldSeed { get; set; }
	public int RandomState { get; set; }
}

/// <summary>
/// Simple tile coordinate for serialization.
/// </summary>
public record TileCoord(int X, int Y);

#endregion