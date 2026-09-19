using RPGGame.Core.Metrics;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;
using UnityEngine;

namespace RPGGame.Core.Simulation;

/// <summary>
/// Contains all permanent progression that persists across runs.
/// This data survives death/time rewind and is only reset on New Game.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class MetaProgression {
	#region Village

	/// <summary>
	/// The village layout (building positions).
	/// </summary>
	public VillageLayoutData VillageLayout { get; set; } = new();

	/// <summary>
	/// Building levels (BuildingId -> Level).
	/// </summary>
	public Dictionary<string, int> BuildingLevels { get; set; } = [];

	/// <summary>
	/// Buildings that have been unlocked.
	/// </summary>
	public HashSet<string> UnlockedBuildings { get; set; } = [];

	/// <summary>
	/// Village upgrade points available to spend.
	/// </summary>
	public int VillageUpgradePoints { get; set; }

	/// <summary>
	/// Total village upgrade points ever earned.
	/// </summary>
	public int TotalVillageUpgradePointsEarned { get; set; }

	#endregion

	#region Exploration

	/// <summary>
	/// Tiles that are permanently revealed on the map.
	/// Accumulated from all runs.
	/// </summary>
	public HashSet<Vector2Int> PermanentlyRevealedTiles { get; set; } = [];

	/// <summary>
	/// Regions that have been discovered.
	/// </summary>
	public HashSet<string> DiscoveredRegions { get; set; } = [];

	/// <summary>
	/// Landmarks that have been discovered (permanent).
	/// </summary>
	public HashSet<string> DiscoveredLandmarks { get; set; } = [];

	/// <summary>
	/// The furthest distance ever reached from the village.
	/// </summary>
	public float FurthestDistanceReached { get; set; }

	/// <summary>
	/// Fog region weakening data.
	/// Key = region index, Value = current strength (0-1).
	/// Persists across runs so fog gets weaker as player fights through it.
	/// </summary>
	public Dictionary<int, float> FogRegionStrengths { get; set; } = [];

	/// <summary>
	/// Total combat power defeated per fog region.
	/// Used to calculate fog weakening.
	/// </summary>
	public Dictionary<int, int> FogRegionCombatPower { get; set; } = [];

	#endregion

	#region Unlocks

	/// <summary>
	/// Character classes that have been unlocked.
	/// </summary>
	public HashSet<string> UnlockedClasses { get; set; } = ["warrior"]; // Warrior starts unlocked

	/// <summary>
	/// Abilities that have been unlocked (available in future runs).
	/// </summary>
	public HashSet<string> UnlockedAbilities { get; set; } = [];

	/// <summary>
	/// Items that have been unlocked (can appear in shops/drops).
	/// </summary>
	public HashSet<string> UnlockedItems { get; set; } = [];

	/// <summary>
	/// Recipes that have been unlocked for crafting.
	/// </summary>
	public HashSet<string> UnlockedRecipes { get; set; } = [];

	/// <summary>
	/// NPCs that have been permanently rescued/unlocked.
	/// </summary>
	public HashSet<string> UnlockedNPCs { get; set; } = [];

	#endregion

	#region Lore & Knowledge

	/// <summary>
	/// Lore entries that have been discovered.
	/// </summary>
	public HashSet<string> DiscoveredLore { get; set; } = [];

	/// <summary>
	/// Bestiary entries (monsters encountered/studied).
	/// </summary>
	public Dictionary<string, BestiaryEntry> Bestiary { get; set; } = [];

	/// <summary>
	/// Hints/clues discovered about the fog's origin.
	/// </summary>
	public List<string> FogClues { get; set; } = [];

	/// <summary>
	/// Whether the true nature of the fog has been discovered.
	/// </summary>
	public bool FogMysteryRevealed { get; set; }

	#endregion

	#region Meta Stats

	/// <summary>
	/// Permanent stat modifiers that apply to all runs.
	/// These are earned through village upgrades, achievements, etc.
	/// </summary>
	public List<StatModifier> MetaBonusModifiers { get; set; } = [];

	/// <summary>
	/// Starting resource bonuses.
	/// </summary>
	public StartingBonuses StartingBonuses { get; set; } = new();

	#endregion

	#region Achievements

	/// <summary>
	/// Achievements that have been unlocked.
	/// </summary>
	public HashSet<string> UnlockedAchievements { get; set; } = [];

	/// <summary>
	/// Achievement progress for incomplete achievements.
	/// </summary>
	public Dictionary<string, int> AchievementProgress { get; set; } = [];

	#endregion

	#region Run History

	/// <summary>
	/// Total number of runs attempted.
	/// </summary>
	public int TotalRuns { get; set; }

	/// <summary>
	/// Number of successful fog clears.
	/// </summary>
	public int FogClears { get; set; }

	/// <summary>
	/// Whether the true ending has been achieved.
	/// </summary>
	public bool TrueEndingAchieved { get; set; }

	/// <summary>
	/// Number of true endings achieved (for NG+ tracking).
	/// </summary>
	public int TrueEndingCount { get; set; }

	/// <summary>
	/// History of recent runs for display.
	/// </summary>
	public List<RunHistoryEntry> RunHistory { get; set; } = [];

	/// <summary>
	/// Maximum run history entries to keep.
	/// </summary>
	private const int MAX_RUN_HISTORY = 20;

	#endregion

	#region Timestamps

	/// <summary>
	/// When this save was first created.
	/// </summary>
	public DateTime FirstPlayDate { get; set; } = DateTime.Now;

	/// <summary>
	/// Total play time across all sessions.
	/// </summary>
	public TimeSpan TotalPlayTime { get; set; } = TimeSpan.Zero;

	/// <summary>
	/// When this save was last played.
	/// </summary>
	public DateTime LastPlayDate { get; set; } = DateTime.Now;

	#endregion

	#region Constructor

	public MetaProgression() {
		// Initialize with default meta bonuses
		InitializeDefaultBonuses();
		UnityEngine.Debug.Log("MetaProgression initialized");
	}

	private void InitializeDefaultBonuses() {
		// No bonuses at start - these are earned through gameplay
	}

	#endregion

	#region Fog Region Management

	/// <summary>
	/// Gets the saved fog strength for a region, or returns default.
	/// </summary>
	public float GetFogRegionStrength(int regionIndex, float defaultStrength) {
		if (FogRegionStrengths.TryGetValue(regionIndex, out float strength)) {
			return strength;
		}
		return defaultStrength;
	}

	/// <summary>
	/// Records fog weakening from a combat victory.
	/// Called when combat ends in a fog region.
	/// </summary>
	public void RecordFogCombatVictory(int regionIndex, int combatPower, float newStrength) {
		// Track total combat power
		if (!FogRegionCombatPower.TryGetValue(regionIndex, out int existing)) {
			existing = 0;
		}
		FogRegionCombatPower[regionIndex] = existing + combatPower;

		// Save the new fog strength
		FogRegionStrengths[regionIndex] = newStrength;
	}

	/// <summary>
	/// Gets fog region data for creating a region with meta-persisted strength.
	/// </summary>
	public (float strength, int combatPower) GetFogRegionMetaData(int regionIndex, float baseStrength) {
		float strength = GetFogRegionStrength(regionIndex, baseStrength);
		int combatPower = FogRegionCombatPower.GetValueOrDefault(regionIndex, 0);
		return (strength, combatPower);
	}

	#endregion

	#region Run Lifecycle

	/// <summary>
	/// Called when a new run starts. Returns starting bonuses to apply.
	/// </summary>
	public RunStartData OnRunStart() {
		TotalRuns++;
		LastPlayDate = DateTime.Now;

		return new RunStartData {
			MetaStatBonuses = GetAllMetaModifiers(),
			StartingGold = StartingBonuses.BonusGold,
			StartingFood = 3 + StartingBonuses.BonusFood,
			StartingMedicalSupplies = 2 + StartingBonuses.BonusMedicalSupplies,
			StartingCampingSupplies = 1 + StartingBonuses.BonusCampingSupplies,
			UnlockedClasses = [.. UnlockedClasses],
			RevealedTiles = [.. PermanentlyRevealedTiles]
		};
	}

	/// <summary>
	/// Called when a run ends (death/time rewind).
	/// Transfers applicable progress to meta.
	/// </summary>
	public RunEndRewards OnRunEnd(RunState runState, bool isVictory) {
		var rewards = new RunEndRewards();

		// Transfer revealed tiles
		int newTilesRevealed = 0;
		foreach (var tile in runState.RevealedTiles) {
			if (PermanentlyRevealedTiles.Add(tile)) {
				newTilesRevealed++;
			}
		}
		rewards.NewTilesRevealed = newTilesRevealed;

		// Transfer discovered lore
		int newLoreFound = 0;
		foreach (var lore in runState.DiscoveredLore) {
			if (DiscoveredLore.Add(lore)) {
				newLoreFound++;
			}
		}
		rewards.NewLoreDiscovered = newLoreFound;

		// Transfer discovered landmarks
		foreach (var landmark in runState.DiscoveredLandmarks) {
			DiscoveredLandmarks.Add(landmark);
		}

		// Update furthest distance
		if (runState.DistanceFromVillage > FurthestDistanceReached) {
			FurthestDistanceReached = runState.DistanceFromVillage;
			rewards.NewFurthestDistance = true;
		}

		// Calculate village upgrade points earned
		int pointsEarned = CalculateUpgradePointsEarned(runState, isVictory);
		VillageUpgradePoints += pointsEarned;
		TotalVillageUpgradePointsEarned += pointsEarned;
		rewards.UpgradePointsEarned = pointsEarned;

		// Victory bonuses
		if (isVictory) {
			FogClears++;
			rewards.IsFogClear = true;

			// Check for true ending progress
			if (FogClears >= GetRequiredClearsForTrueEnding() && !TrueEndingAchieved) {
				rewards.TrueEndingUnlocked = true;
			}
		}

		// Add to run history
		AddRunToHistory(runState, isVictory);

		return rewards;
	}

	/// <summary>
	/// Called when the true ending is achieved.
	/// </summary>
	public void OnTrueEndingAchieved() {
		TrueEndingAchieved = true;
		TrueEndingCount++;

		// Unlock special rewards for true ending
		UnlockedClasses.Add("ascended"); // Special class for NG+

		// Add permanent meta bonus
		AddMetaBonus(StatModifier.CreateMetaBonus(
			Ids.Stats.Economy.ExperienceGain,
			10,
			"True Ending Bonus"
		));
	}

	private int CalculateUpgradePointsEarned(RunState runState, bool isVictory) {
		int points = 0;

		// Base points for distance traveled
		points += (int)(runState.DistanceFromVillage / 10);

		// Points for days survived
		points += GameTime.Instance.Day;

		// Points for discoveries
		points += runState.DiscoveredLore.Count * 2;
		points += runState.DiscoveredLandmarks.Count * 3;

		// Bonus for victory
		if (isVictory) {
			points += 50;
		}

		// Minimum 1 point per run
		return Math.Max(1, points);
	}

	private int GetRequiredClearsForTrueEnding() {
		// Need to clear the fog 3 times to unlock true ending
		return 3;
	}

	private void AddRunToHistory(RunState runState, bool isVictory) {
		var entry = new RunHistoryEntry {
			RunNumber = TotalRuns,
			CharacterName = runState.Character.Name,
			CharacterClass = runState.CharacterClassId,
			Level = runState.Level,
			DaysSurvived = GameTime.Instance.Day,
			DistanceTraveled = runState.TotalDistanceTraveled,
			IsVictory = isVictory,
			Timestamp = DateTime.Now
		};

		RunHistory.Insert(0, entry);

		// Keep only the most recent runs
		while (RunHistory.Count > MAX_RUN_HISTORY) {
			RunHistory.RemoveAt(RunHistory.Count - 1);
		}
	}

	#endregion

	#region Meta Bonuses

	/// <summary>
	/// Adds a permanent meta bonus modifier.
	/// </summary>
	public void AddMetaBonus(StatModifier modifier) {
		MetaBonusModifiers.Add(modifier);
	}

	/// <summary>
	/// Removes a meta bonus by source.
	/// </summary>
	public int RemoveMetaBonusesBySource(string source) {
		return MetaBonusModifiers.RemoveAll(m => m.Source == source);
	}

	/// <summary>
	/// Gets all meta modifiers to apply to a new run.
	/// </summary>
	public List<StatModifier> GetAllMetaModifiers() {
		var modifiers = new List<StatModifier>();

		// Add all stored meta bonus modifiers
		modifiers.AddRange(MetaBonusModifiers);

		// Add bonuses from building levels
		foreach (var (buildingId, level) in BuildingLevels) {
			var buildingBonuses = GetBuildingBonuses(buildingId, level);
			modifiers.AddRange(buildingBonuses);
		}

		// Add bonuses from achievements
		foreach (var achievementId in UnlockedAchievements) {
			var achievementBonuses = GetAchievementBonuses(achievementId);
			modifiers.AddRange(achievementBonuses);
		}

		return modifiers;
	}

	private List<StatModifier> GetBuildingBonuses(string buildingId, int level) {
		var bonuses = new List<StatModifier>();

		// Building-specific bonuses (will be data-driven later)
		switch (buildingId) {
			case "blacksmith":
				bonuses.Add(StatModifier.CreateMetaBonus(
					Ids.Stats.Combat.AttackBonus,
					level,
					$"Blacksmith Level {level}"
				));
				break;

			case "tavern":
				// Morale is now a run-state property, not a stat
				// Could add a different bonus or skip
				break;

			case "herbalist":
				bonuses.Add(StatModifier.CreateMetaBonus(
					Ids.Stats.Resource.MaxHealth,
					level * 5,
					$"Herbalist Level {level}"
				));
				break;

			case "cartographer":
				bonuses.Add(StatModifier.CreateMetaBonus(
					Ids.Stats.Expedition.VisionRange,
					level,
					$"Cartographer Level {level}"
				));
				break;

			case "training_grounds":
				bonuses.Add(StatModifier.CreateMetaBonus(
					Ids.Stats.Economy.ExperienceGain,
					level * 5,
					$"Training Grounds Level {level}"
				));
				break;
		}

		return bonuses;
	}

	private List<StatModifier> GetAchievementBonuses(string achievementId) {
		var bonuses = new List<StatModifier>();

		// Achievement-specific bonuses (will be data-driven later)
		switch (achievementId) {
			case "first_blood":
				bonuses.Add(StatModifier.CreateMetaBonus(
					Ids.Stats.Combat.DamBonus,
					1,
					"Achievement: First Blood"
				));
				break;

			case "survivor":
				bonuses.Add(StatModifier.CreateMetaBonus(
					Ids.Stats.Resource.MaxHealth,
					10,
					"Achievement: Survivor"
				));
				break;

			case "explorer":
				bonuses.Add(StatModifier.CreateMetaBonus(
					Ids.Stats.Movement.MovementSpeed,
					10, // 10% increase
					"Achievement: Explorer",
					ModifierOperation.PercentIncrease
				));
				break;
		}

		return bonuses;
	}

	#endregion

	#region Village Management

	/// <summary>
	/// Upgrades a building. Returns false if not enough points or max level.
	/// </summary>
	public bool UpgradeBuilding(string buildingId, int cost) {
		if (VillageUpgradePoints < cost) {
			return false;
		}
		if (!UnlockedBuildings.Contains(buildingId)) {
			return false;
		}

		int currentLevel = BuildingLevels.GetValueOrDefault(buildingId, 0);
		int maxLevel = GetBuildingMaxLevel(buildingId);

		if (currentLevel >= maxLevel) {
			return false;
		}

		VillageUpgradePoints -= cost;
		BuildingLevels[buildingId] = currentLevel + 1;

		UnityEngine.Debug.Log($"Upgraded {buildingId} to level {currentLevel + 1}");
		return true;
	}

	/// <summary>
	/// Unlocks a new building.
	/// </summary>
	public bool UnlockBuilding(string buildingId, int cost) {
		if (VillageUpgradePoints < cost) {
			return false;
		}
		if (UnlockedBuildings.Contains(buildingId)) {
			return false;
		}

		VillageUpgradePoints -= cost;
		UnlockedBuildings.Add(buildingId);
		BuildingLevels[buildingId] = 1;

		UnityEngine.Debug.Log($"Unlocked building: {buildingId}");
		return true;
	}

	private int GetBuildingMaxLevel(string buildingId) {
		// Will be data-driven later
		return 5;
	}

	#endregion

	#region Class Unlocks

	/// <summary>
	/// Checks if a class is unlocked.
	/// </summary>
	public bool IsClassUnlocked(string classId) {
		return UnlockedClasses.Contains(classId);
	}

	/// <summary>
	/// Unlocks a new character class.
	/// </summary>
	public void UnlockClass(string classId) {
		if (UnlockedClasses.Add(classId)) {
			UnityEngine.Debug.Log($"Unlocked class: {classId}");
		}
	}

	#endregion

	#region Bestiary

	/// <summary>
	/// Records an encounter with a monster type.
	/// </summary>
	public void RecordMonsterEncounter(string monsterId, bool defeated) {
		if (!Bestiary.TryGetValue(monsterId, out var entry)) {
			entry = new BestiaryEntry { MonsterId = monsterId };
			Bestiary[monsterId] = entry;
		}

		entry.TimesEncountered++;
		if (defeated) {
			entry.TimesDefeated++;
		}

		// Unlock more info based on encounters
		if (entry.TimesEncountered >= 1) {
			entry.BasicInfoUnlocked = true;
		}
		if (entry.TimesDefeated >= 3) {
			entry.StatsUnlocked = true;
		}
		if (entry.TimesDefeated >= 10) {
			entry.WeaknessesUnlocked = true;
		}
	}

	#endregion

	#region Serialization

	/// <summary>
	/// Converts to serializable data.
	/// </summary>
	public MetaProgressionData ToData() {
		return new MetaProgressionData {
			VillageLayout = VillageLayout,
			BuildingLevels = new Dictionary<string, int>(BuildingLevels),
			UnlockedBuildings = [.. UnlockedBuildings],
			VillageUpgradePoints = VillageUpgradePoints,
			TotalVillageUpgradePointsEarned = TotalVillageUpgradePointsEarned,
			PermanentlyRevealedTiles = PermanentlyRevealedTiles
				.Select(t => new TileCoord(t.x, t.y)).ToList(),
			DiscoveredRegions = [.. DiscoveredRegions],
			DiscoveredLandmarks = [.. DiscoveredLandmarks],
			FogRegionStrengths = new Dictionary<int, float>(FogRegionStrengths),
			FogRegionCombatPower = new Dictionary<int, int>(FogRegionCombatPower),
			FurthestDistanceReached = FurthestDistanceReached,
			UnlockedClasses = [.. UnlockedClasses],
			UnlockedAbilities = [.. UnlockedAbilities],
			UnlockedItems = [.. UnlockedItems],
			UnlockedRecipes = [.. UnlockedRecipes],
			UnlockedNPCs = [.. UnlockedNPCs],
			DiscoveredLore = [.. DiscoveredLore],
			Bestiary = new Dictionary<string, BestiaryEntry>(Bestiary),
			FogClues = [.. FogClues],
			FogMysteryRevealed = FogMysteryRevealed,
			MetaBonusModifiers = MetaBonusModifiers.Select(m => m.ToData()).ToList(),
			StartingBonuses = StartingBonuses,
			UnlockedAchievements = [.. UnlockedAchievements],
			AchievementProgress = new Dictionary<string, int>(AchievementProgress),
			TotalRuns = TotalRuns,
			FogClears = FogClears,
			TrueEndingAchieved = TrueEndingAchieved,
			TrueEndingCount = TrueEndingCount,
			RunHistory = [.. RunHistory],
			FirstPlayDate = FirstPlayDate,
			TotalPlayTimeSeconds = (long)TotalPlayTime.TotalSeconds,
			LastPlayDate = LastPlayDate
		};
	}

	/// <summary>
	/// Loads from serialized data.
	/// </summary>
	public void FromData(MetaProgressionData data) {
		VillageLayout = data.VillageLayout;
		BuildingLevels = new Dictionary<string, int>(data.BuildingLevels);
		UnlockedBuildings = [.. data.UnlockedBuildings];
		VillageUpgradePoints = data.VillageUpgradePoints;
		TotalVillageUpgradePointsEarned = data.TotalVillageUpgradePointsEarned;

		PermanentlyRevealedTiles = data.PermanentlyRevealedTiles
			.Select(t => new Vector2Int(t.X, t.Y))
			.ToHashSet();
		DiscoveredRegions = [.. data.DiscoveredRegions];
		DiscoveredLandmarks = [.. data.DiscoveredLandmarks];
		if (data.FogRegionStrengths != null) {
			FogRegionStrengths = new Dictionary<int, float>(data.FogRegionStrengths);
		}
		if (data.FogRegionCombatPower != null) {
			FogRegionCombatPower = new Dictionary<int, int>(data.FogRegionCombatPower);
		}
		FurthestDistanceReached = data.FurthestDistanceReached;

		UnlockedClasses = [.. data.UnlockedClasses];
		UnlockedAbilities = [.. data.UnlockedAbilities];
		UnlockedItems = [.. data.UnlockedItems];
		UnlockedRecipes = [.. data.UnlockedRecipes];
		UnlockedNPCs = [.. data.UnlockedNPCs];

		DiscoveredLore = [.. data.DiscoveredLore];
		Bestiary = new Dictionary<string, BestiaryEntry>(data.Bestiary);
		FogClues = [.. data.FogClues];
		FogMysteryRevealed = data.FogMysteryRevealed;

		MetaBonusModifiers = data.MetaBonusModifiers?.Select(StatModifier.FromData).ToList() ?? [];
		StartingBonuses = data.StartingBonuses;

		UnlockedAchievements = [.. data.UnlockedAchievements];
		AchievementProgress = new Dictionary<string, int>(data.AchievementProgress);

		TotalRuns = data.TotalRuns;
		FogClears = data.FogClears;
		TrueEndingAchieved = data.TrueEndingAchieved;
		TrueEndingCount = data.TrueEndingCount;
		RunHistory = [.. data.RunHistory];

		FirstPlayDate = data.FirstPlayDate;
		TotalPlayTime = TimeSpan.FromSeconds(data.TotalPlayTimeSeconds);
		LastPlayDate = data.LastPlayDate;
	}

	/// <summary>
	/// Resets all meta progression (for New Game).
	/// </summary>
	public void Reset() {
		VillageLayout = new VillageLayoutData();
		BuildingLevels.Clear();
		UnlockedBuildings.Clear();
		VillageUpgradePoints = 0;
		TotalVillageUpgradePointsEarned = 0;

		PermanentlyRevealedTiles.Clear();
		DiscoveredRegions.Clear();
		DiscoveredLandmarks.Clear();
		FurthestDistanceReached = 0;
		FogRegionStrengths.Clear();
		FogRegionCombatPower.Clear();

		UnlockedClasses = ["warrior"];
		UnlockedAbilities.Clear();
		UnlockedItems.Clear();
		UnlockedRecipes.Clear();
		UnlockedNPCs.Clear();

		DiscoveredLore.Clear();
		Bestiary.Clear();
		FogClues.Clear();
		FogMysteryRevealed = false;

		MetaBonusModifiers.Clear();
		StartingBonuses = new StartingBonuses();

		UnlockedAchievements.Clear();
		AchievementProgress.Clear();

		TotalRuns = 0;
		FogClears = 0;
		TrueEndingAchieved = false;
		TrueEndingCount = 0;
		RunHistory.Clear();

		FirstPlayDate = DateTime.Now;
		TotalPlayTime = TimeSpan.Zero;
		LastPlayDate = DateTime.Now;

		UnityEngine.Debug.Log("MetaProgression reset for new game");
	}

	#endregion

	#region Debug

	/// <summary>
	/// Returns a summary of meta progression.
	/// </summary>
	public string GetSummary() {
		var sb = new System.Text.StringBuilder();
		sb.AppendLine("=== META PROGRESSION ===\n");

		sb.AppendLine($"Total Runs: {TotalRuns}");
		sb.AppendLine($"Fog Clears: {FogClears}");
		sb.AppendLine($"True Ending: {(TrueEndingAchieved ? "Yes" : "No")}");
		sb.AppendLine($"Total Play Time: {TotalPlayTime:hh\\:mm\\:ss}");

		sb.AppendLine($"\n-- Village --");
		sb.AppendLine($"Upgrade Points: {VillageUpgradePoints}");
		sb.AppendLine($"Buildings: {UnlockedBuildings.Count}");

		sb.AppendLine($"\n-- Exploration --");
		sb.AppendLine($"Tiles Revealed: {PermanentlyRevealedTiles.Count}");
		sb.AppendLine($"Furthest Distance: {FurthestDistanceReached:F1}");

		sb.AppendLine($"\n-- Unlocks --");
		sb.AppendLine($"Classes: {string.Join(", ", UnlockedClasses)}");
		sb.AppendLine($"Lore Entries: {DiscoveredLore.Count}");
		sb.AppendLine($"Achievements: {UnlockedAchievements.Count}");

		return sb.ToString();
	}

	#endregion
}

#region Supporting Types

// ... (VillageLayoutData, StartingBonuses, BestiaryEntry, RunHistoryEntry, 
//      RunStartData, RunEndRewards remain the same)

#endregion

#region Supporting Types

/// <summary>
/// Village layout data for serialization.
/// </summary>
public class VillageLayoutData {
	public int Width { get; set; } = 10;
	public int Height { get; set; } = 10;
	public Dictionary<string, TileCoord> BuildingPositions { get; set; } = [];
	public TileCoord AnchorPosition { get; set; } = new(5, 5);
}

/// <summary>
/// Starting bonuses that accumulate from meta progression.
/// </summary>
public class StartingBonuses {
	public int BonusGold { get; set; }
	public int BonusFood { get; set; }
	public int BonusMedicalSupplies { get; set; }
	public int BonusCampingSupplies { get; set; }
	public List<string> StartingItems { get; set; } = [];
	public List<string> StartingAbilities { get; set; } = [];
}

/// <summary>
/// Bestiary entry for a monster type.
/// </summary>
public class BestiaryEntry {
	public string MonsterId { get; set; } = "";
	public int TimesEncountered { get; set; }
	public int TimesDefeated { get; set; }
	public bool BasicInfoUnlocked { get; set; }
	public bool StatsUnlocked { get; set; }
	public bool WeaknessesUnlocked { get; set; }
}

/// <summary>
/// History entry for a completed run.
/// </summary>
public class RunHistoryEntry {
	public int RunNumber { get; set; }
	public string CharacterName { get; set; } = "";
	public string CharacterClass { get; set; } = "";
	public int Level { get; set; }
	public int DaysSurvived { get; set; }
	public float DistanceTraveled { get; set; }
	public bool IsVictory { get; set; }
	public DateTime Timestamp { get; set; }
}

/// <summary>
/// Data returned when starting a new run.
/// </summary>
public class RunStartData {
	public List<StatModifier> MetaStatBonuses { get; set; } = [];
	public int StartingGold { get; set; }
	public int StartingFood { get; set; }
	public int StartingMedicalSupplies { get; set; }
	public int StartingCampingSupplies { get; set; }
	public List<string> UnlockedClasses { get; set; } = [];
	public HashSet<Vector2Int> RevealedTiles { get; set; } = [];
}

/// <summary>
/// Rewards returned when a run ends.
/// </summary>
public class RunEndRewards {
	public int NewTilesRevealed { get; set; }
	public int NewLoreDiscovered { get; set; }
	public bool NewFurthestDistance { get; set; }
	public int UpgradePointsEarned { get; set; }
	public bool IsFogClear { get; set; }
	public bool TrueEndingUnlocked { get; set; }
	public List<string> NewAchievements { get; set; } = [];
	public List<string> NewUnlocks { get; set; } = [];
}

#endregion

#region Serialization Data

/// <summary>
/// Serializable meta progression data.
/// </summary>
public class MetaProgressionData {
	public VillageLayoutData VillageLayout { get; set; } = new();
	public Dictionary<string, int> BuildingLevels { get; set; } = [];
	public List<string> UnlockedBuildings { get; set; } = [];
	public int VillageUpgradePoints { get; set; }
	public int TotalVillageUpgradePointsEarned { get; set; }
	public List<TileCoord> PermanentlyRevealedTiles { get; set; } = [];
	public List<string> DiscoveredRegions { get; set; } = [];
	public List<string> DiscoveredLandmarks { get; set; } = [];
	public float FurthestDistanceReached { get; set; }
	public Dictionary<int, float>? FogRegionStrengths { get; set; } = [];
	public Dictionary<int, int>? FogRegionCombatPower { get; set; } = [];
	public List<string> UnlockedClasses { get; set; } = [];
	public List<string> UnlockedAbilities { get; set; } = [];
	public List<string> UnlockedItems { get; set; } = [];
	public List<string> UnlockedRecipes { get; set; } = [];
	public List<string> UnlockedNPCs { get; set; } = [];
	public List<string> DiscoveredLore { get; set; } = [];
	public Dictionary<string, BestiaryEntry> Bestiary { get; set; } = [];
	public List<string> FogClues { get; set; } = [];
	public bool FogMysteryRevealed { get; set; }
	public List<StatModifierData> MetaBonusModifiers { get; set; } = [];
	public StartingBonuses StartingBonuses { get; set; } = new();
	public List<string> UnlockedAchievements { get; set; } = [];
	public Dictionary<string, int> AchievementProgress { get; set; } = [];
	public int TotalRuns { get; set; }
	public int FogClears { get; set; }
	public bool TrueEndingAchieved { get; set; }
	public int TrueEndingCount { get; set; }
	public List<RunHistoryEntry> RunHistory { get; set; } = [];
	public DateTime FirstPlayDate { get; set; }
	public long TotalPlayTimeSeconds { get; set; }
	public DateTime LastPlayDate { get; set; }
}

#endregion