using RPGGame.Core.Prototypes.Village;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;
using UnityEngine;

namespace RPGGame.Core.Village;

/// <summary>
/// Manages the village hub - buildings, upgrades, NPCs, and player interactions.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class VillageManager {
	#region Fields

	private readonly GameDb k_gameDb;
	private readonly MetaProgression k_metaProgression;

	private VillageLayout? k_layout;
	private bool k_isInitialized;

	#endregion

	#region Properties

	/// <summary>
	/// The village layout.
	/// </summary>
	public VillageLayout? Layout => k_layout;

	/// <summary>
	/// Whether the village has been initialized.
	/// </summary>
	public bool IsInitialized => k_isInitialized;

	/// <summary>
	/// Available upgrade points.
	/// </summary>
	public int UpgradePoints => k_metaProgression.VillageUpgradePoints;

	/// <summary>
	/// All unlocked building types.
	/// </summary>
	public IEnumerable<string> UnlockedBuildings => k_metaProgression.UnlockedBuildings;

	#endregion

	#region Events

	/// <summary>
	/// Fired when the village is initialized.
	/// </summary>
	public event Action? OnVillageInitialized;

	/// <summary>
	/// Fired when upgrade points change.
	/// </summary>
	public event Action<int>? OnUpgradePointsChanged;

	/// <summary>
	/// Fired when a building is unlocked.
	/// </summary>
	public event Action<string>? OnBuildingUnlocked;

	/// <summary>
	/// Fired when the player interacts with a building.
	/// </summary>
	public event Action<Building, BuildingService>? OnBuildingServiceUsed;

	#endregion

	#region Constructor

	public VillageManager(GameDb gameDb, MetaProgression metaProgression) {
		k_gameDb = gameDb;
		k_metaProgression = metaProgression;
		UnityEngine.Debug.Log("VillageManager initialized");
	}

	#endregion

	#region Initialization

	/// <summary>
	/// Initializes the village for a new game.
	/// </summary>
	public void InitializeNewVillage() {
		// Create 13x13 village layout
		k_layout = new VillageLayout(k_gameDb, VillageLayout.DEFAULT_SIZE);

		// Unlock core buildings
		foreach (var buildingId in Ids.Buildings.CoreBuildings) {
			k_metaProgression.UnlockedBuildings.Add(buildingId.Value);
		}

		// Unlock and register starter buildings
		foreach (var buildingId in Ids.Buildings.StarterBuildings) {
			k_metaProgression.UnlockedBuildings.Add(buildingId.Value);
			k_metaProgression.BuildingLevels[buildingId.Value] = 1;
		}

		// Unlock the Well (used as decoration in corners)
		k_metaProgression.UnlockedBuildings.Add(Ids.Buildings.Well.Value);
		k_metaProgression.BuildingLevels[Ids.Buildings.Well.Value] = 1;

		// Place all starter buildings and decorations
		k_layout.PlaceStarterBuildings();

		k_isInitialized = true;
		OnVillageInitialized?.Invoke();

		UnityEngine.Debug.Log($"New village initialized: {k_layout.Width}x{k_layout.Height}, {k_layout.Buildings.Count} buildings");
	}

	/// <summary>
	/// Loads the village from save data.
	/// </summary>
	public void LoadVillage(VillageLayoutSaveData saveData) {
		k_layout = new VillageLayout(k_gameDb, saveData);
		k_isInitialized = true;
		OnVillageInitialized?.Invoke();

		UnityEngine.Debug.Log("Village loaded from save");
	}

	#endregion

	#region Building Management

	/// <summary>
	/// Gets proto for a building type by typed ID.
	/// </summary>
	public BuildingProto? GetBuildingProto(BuildingProto.ID buildingId) {
		if (k_gameDb.TryGetProto<BuildingProto>(buildingId, out var proto)) {
			return proto;
		}
		return null;
	}

	/// <summary>
	/// Checks if a building type is unlocked.
	/// </summary>
	public bool IsBuildingUnlocked(string buildingId) {
		return k_metaProgression.UnlockedBuildings.Contains(buildingId);
	}

	/// <summary>
	/// Checks if a building type is unlocked (typed ID overload).
	/// </summary>
	public bool IsBuildingUnlocked(BuildingProto.ID buildingId) {
		return k_metaProgression.UnlockedBuildings.Contains(buildingId.Value);
	}

	/// <summary>
	/// Gets all available building protos that are unlocked.
	/// </summary>
	public IEnumerable<BuildingProto> GetAvailableBuildings() {
		foreach (var buildingIdStr in k_metaProgression.UnlockedBuildings) {
			var buildingId = new BuildingProto.ID(buildingIdStr);
			var proto = GetBuildingProto(buildingId);
			if (proto != null) {
				yield return proto;
			}
		}
	}

	/// <summary>
	/// Gets all buildings that can be unlocked.
	/// </summary>
	public IEnumerable<(BuildingProto proto, bool canAfford, string? blockedReason)> GetUnlockableBuildings() {
		foreach (var buildingId in Ids.Buildings.AllBuildings) {
			if (k_metaProgression.UnlockedBuildings.Contains(buildingId.Value)) {
				continue;
			}

			var proto = GetBuildingProto(buildingId);
			if (proto == null) {
				continue;
			}

			string? blockedReason = null;

			// Check prerequisites
			foreach (var prereq in proto.Prerequisites) {
				if (!k_metaProgression.UnlockedBuildings.Contains(prereq.Value)) {
					var prereqProto = GetBuildingProto(prereq);
					blockedReason = $"Requires: {prereqProto?.DisplayText.Name ?? prereq.Value}";
					break;
				}
			}

			// Check fog clears requirement
			if (blockedReason == null && proto.RequiredFogClears > k_metaProgression.FogClears) {
				blockedReason = $"Requires {proto.RequiredFogClears} fog clear(s)";
			}

			bool canAfford = k_metaProgression.VillageUpgradePoints >= proto.UnlockCost;

			yield return (proto, canAfford && blockedReason == null, blockedReason);
		}
	}

	/// <summary>
	/// Unlocks a new building type.
	/// </summary>
	public bool UnlockBuilding(BuildingProto.ID buildingId) {
		var proto = GetBuildingProto(buildingId);
		if (proto == null) {
			return false;
		}

		if (k_metaProgression.UnlockedBuildings.Contains(buildingId.Value)) {
			return false;
		}

		// Check prerequisites
		foreach (var prereq in proto.Prerequisites) {
			if (!k_metaProgression.UnlockedBuildings.Contains(prereq.Value)) {
				return false;
			}
		}

		// Check fog clears
		if (proto.RequiredFogClears > k_metaProgression.FogClears) {
			return false;
		}

		// Check cost
		if (k_metaProgression.VillageUpgradePoints < proto.UnlockCost) {
			return false;
		}

		k_metaProgression.VillageUpgradePoints -= proto.UnlockCost;
		k_metaProgression.UnlockedBuildings.Add(buildingId.Value);
		k_metaProgression.BuildingLevels[buildingId.Value] = 1;

		OnUpgradePointsChanged?.Invoke(k_metaProgression.VillageUpgradePoints);
		OnBuildingUnlocked?.Invoke(buildingId.Value);

		UnityEngine.Debug.Log($"Unlocked building: {proto.DisplayText.Name}");
		return true;
	}

	/// <summary>
	/// Unlocks a new building type (string ID overload for backwards compatibility).
	/// </summary>
	public bool UnlockBuilding(string buildingId) {
		return UnlockBuilding(new BuildingProto.ID(buildingId));
	}

	/// <summary>
	/// Places a building at a position.
	/// </summary>
	public Building? PlaceBuilding(BuildingProto.ID buildingId, Vector2Int position) {
		if (k_layout == null) {
			return null;
		}
		if (!IsBuildingUnlocked(buildingId)) {
			return null;
		}

		return k_layout.PlaceBuilding(buildingId, position);
	}

	/// <summary>
	/// Upgrades a building.
	/// </summary>
	public VillageBuildResult UpgradeBuilding(string instanceId) {
		if (k_layout == null) {
			return new VillageBuildResult { Success = false, Message = "Village not initialized." };
		}

		var building = k_layout.GetBuildingById(instanceId);
		if (building == null) {
			return new VillageBuildResult { Success = false, Message = "Building not found." };
		}

		if (building.Level >= building.Proto.MaxLevel) {
			return new VillageBuildResult { Success = false, Message = "Building is already at max level." };
		}

		int cost = building.UpgradeCost;
		if (k_metaProgression.VillageUpgradePoints < cost) {
			return new VillageBuildResult { Success = false, Message = $"Not enough upgrade points. Need {cost}." };
		}

		if (!k_layout.UpgradeBuilding(instanceId)) {
			return new VillageBuildResult { Success = false, Message = "Failed to upgrade building." };
		}

		k_metaProgression.VillageUpgradePoints -= cost;
		k_metaProgression.BuildingLevels[building.Proto.Id.Value] = building.Level;

		OnUpgradePointsChanged?.Invoke(k_metaProgression.VillageUpgradePoints);

		return new VillageBuildResult {
			Success = true,
			Message = $"Upgraded {building.DisplayName} to level {building.Level}!",
			NewLevel = building.Level
		};
	}

	#endregion

	#region Building Services

	/// <summary>
	/// Uses a service from a building.
	/// </summary>
	public void UseService(Building building, string serviceId) {
		var service = building.GetAvailableServices().FirstOrDefault(s => s.Id == serviceId);
		if (service == null) {
			UnityEngine.Debug.LogWarning($"Service {serviceId} not available on {building.DisplayName}");
			return;
		}

		OnBuildingServiceUsed?.Invoke(building, service);
		UnityEngine.Debug.Log($"Used service {service.Name} at {building.DisplayName}");
	}

	/// <summary>
	/// Gets all buildings with a specific service type.
	/// </summary>
	public IEnumerable<(Building building, BuildingService service)> GetBuildingsWithService(BuildingServiceType serviceType) {
		if (k_layout == null) {
			yield break;
		}

		foreach (var building in k_layout.Buildings) {
			foreach (var service in building.GetAvailableServices()) {
				if (service.Type == serviceType) {
					yield return (building, service);
				}
			}
		}
	}

	#endregion

	#region Meta Bonuses

	/// <summary>
	/// Gets all stat modifiers from village buildings.
	/// </summary>
	public IEnumerable<StatModifier> GetAllBuildingBonuses() {
		if (k_layout == null) {
			yield break;
		}

		foreach (var building in k_layout.Buildings) {
			// Level-based bonuses
			foreach (var bonus in building.Proto.GetBonusesForLevel(building.Level)) {
				yield return StatModifier.CreateMetaBonus(
						bonus.Stat,
						bonus.Value,
						$"{building.DisplayName} Lv{building.Level}",
						bonus.Operation
					);
			}

			// Adjacency bonuses
			var adjacent = k_layout.GetAdjacentBuildings(building);
			foreach (var adjBonus in building.GetActiveAdjacencyBonuses(adjacent)) {
				yield return StatModifier.CreateMetaBonus(
						adjBonus.BonusStat,
						adjBonus.BonusValue,
						adjBonus.Description
					);
			}
		}
	}

	/// <summary>
	/// Gets starting resource bonuses from buildings.
	/// </summary>
	public StartingBonuses GetStartingBonuses() {
		var bonuses = new StartingBonuses();

		if (k_layout == null) {
			return bonuses;
		}

		foreach (var building in k_layout.Buildings) {
			var startingBonus = building.Proto.StartingBonus;
			if (startingBonus == null) {
				continue;
			}

			bonuses.BonusGold += startingBonus.GoldPerLevel * building.Level;
			bonuses.BonusFood += startingBonus.FoodPerLevel * building.Level;
			bonuses.BonusMedicalSupplies += startingBonus.MedicalSuppliesPerLevel * building.Level;
			bonuses.BonusCampingSupplies += startingBonus.CampingSuppliesPerLevel * building.Level;
		}

		return bonuses;
	}

	#endregion

	#region The Anchor

	/// <summary>
	/// Gets The Anchor building.
	/// </summary>
	public Building? GetAnchor() => k_layout?.Anchor;

	/// <summary>
	/// Interacts with The Anchor.
	/// </summary>
	public AnchorInteractionResult InteractWithAnchor() {
		var anchor = GetAnchor();
		if (anchor == null) {
			return new AnchorInteractionResult { Success = false, Message = "The Anchor is not present." };
		}

		return new AnchorInteractionResult {
			Success = true,
			Message = "You feel the Anchor's power flowing through you...",
			TotalRewinds = k_metaProgression.TotalRuns - 1,
			FogClears = k_metaProgression.FogClears,
			LoreHints = GetAnchorLoreHints()
		};
	}

	private List<string> GetAnchorLoreHints() {
		var hints = new List<string>();

		// Progressive hints based on fog clears
		if (k_metaProgression.FogClears >= 1) {
			hints.Add("The fog seems... familiar somehow.");
		}
		if (k_metaProgression.FogClears >= 2) {
			hints.Add("You sense there's more to discover at the fog's source.");
		}
		if (k_metaProgression.FogClears >= 3 && !k_metaProgression.TrueEndingAchieved) {
			hints.Add("The truth awaits. You are ready to face it.");
		}

		// Add discovered fog clues
		hints.AddRange(k_metaProgression.FogClues);

		return hints;
	}

	#endregion

	#region Departure

	/// <summary>
	/// Gets information for the departure screen.
	/// </summary>
	public DepartureInfo GetDepartureInfo(RunState runState) {
		var warnings = new List<string>();
		var suggestions = new List<string>();
		var liveCharacter = runState.Character;
		// Check for issues
		if (liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.FoodOnHand) <= 1) {
			warnings.Add("You're low on food! Visit the General Store.");
		}
		if (liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.MedicalSupplies) == 0) {
			warnings.Add("No medical supplies! Visit the Herbalist.");
		}
		if (runState.HasPendingLevelUp) {
			warnings.Add("You have unspent level-ups! You'll need to camp to apply them.");
		}

		// Suggestions
		if (liveCharacter.BaseStats.Get(Ids.Stats.Resource.CurrentHealth) < runState.Stats.Get(Ids.Stats.Resource.MaxHealth)) {
			suggestions.Add("Rest at the Tavern to restore health.");
		}

		var gate = k_layout?.Gate;

		return new DepartureInfo {
			CanDepart = true,
			Warnings = warnings,
			Suggestions = suggestions,
			GateLevel = gate?.Level ?? 1,
			StartingBonuses = GetStartingBonuses()
		};
	}

	#endregion

	#region Serialization

	/// <summary>
	/// Gets save data for the village.
	/// </summary>
	public VillageLayoutSaveData? GetSaveData() {
		return k_layout?.ToSaveData();
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// Result of interacting with The Anchor.
/// </summary>
public class AnchorInteractionResult {
	public bool Success { get; set; }
	public string Message { get; set; } = "";
	public int TotalRewinds { get; set; }
	public int FogClears { get; set; }
	public List<string> LoreHints { get; set; } = [];
}

/// <summary>
/// Information for the departure screen.
/// </summary>
public class DepartureInfo {
	public bool CanDepart { get; set; }
	public List<string> Warnings { get; set; } = [];
	public List<string> Suggestions { get; set; } = [];
	public int GateLevel { get; set; }
	public StartingBonuses StartingBonuses { get; set; } = new();
}

#endregion