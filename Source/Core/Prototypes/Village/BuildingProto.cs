using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;
using RPGGame.Core.Village;

namespace RPGGame.Core.Prototypes.Village;

/// <summary>
/// Prototype for building definitions.
/// </summary>
public class BuildingProto : Proto {
	#region ID Type

	new public readonly struct ID(string value) : IEquatable<ID>, IComparable<ID> {
		public readonly string Value = value;

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object? obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(Proto.ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, Proto.ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(Proto.ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, Proto.ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static implicit operator Proto.ID(ID id) => new Proto.ID(id.Value);
	}

	#endregion

	#region Properties

	new public ID Id => new(base.Id.Value);

	/// <summary>
	/// Icon name for UI display.
	/// </summary>
	public string IconName { get; }

	/// <summary>
	/// Building category for organization.
	/// </summary>
	public BuildingCategory Category { get; }

	/// <summary>
	/// Size classification.
	/// </summary>
	public BuildingSize Size { get; }

	/// <summary>
	/// Width in tiles.
	/// </summary>
	public int Width { get; }

	/// <summary>
	/// Height in tiles.
	/// </summary>
	public int Height { get; }

	/// <summary>
	/// Whether this building can be moved after placement.
	/// </summary>
	public bool CanMove { get; }

	/// <summary>
	/// Whether this building can be demolished.
	/// </summary>
	public bool CanDemolish { get; }

	/// <summary>
	/// Cost to unlock this building (in upgrade points).
	/// </summary>
	public int UnlockCost { get; }

	/// <summary>
	/// Base cost to upgrade (multiplied by level).
	/// </summary>
	public int BaseUpgradeCost { get; }

	/// <summary>
	/// Maximum level this building can reach.
	/// </summary>
	public int MaxLevel { get; }

	/// <summary>
	/// Buildings that must be built first.
	/// </summary>
	public IReadOnlyList<BuildingProto.ID> Prerequisites { get; }

	/// <summary>
	/// Minimum fog clears required to unlock.
	/// </summary>
	public int RequiredFogClears { get; }

	/// <summary>
	/// Whether this building is available from the start.
	/// </summary>
	public bool StartsUnlocked { get; }

	/// <summary>
	/// Stat bonuses provided per level.
	/// </summary>
	public IReadOnlyList<BuildingBonusProto> BonusesPerLevel { get; }

	/// <summary>
	/// Services this building provides.
	/// </summary>
	public IReadOnlyList<BuildingServiceProto> Services { get; }

	/// <summary>
	/// Adjacency bonuses when next to specific buildings.
	/// </summary>
	public IReadOnlyList<AdjacencyBonusProto> AdjacencyBonuses { get; }

	/// <summary>
	/// Starting resource bonuses per level.
	/// </summary>
	public StartingResourceBonusProto? StartingBonus { get; }

	/// <summary>
	/// Whether this building has a shop.
	/// </summary>
	public bool HasShop { get; }

	/// <summary>
	/// Whether this building provides rest/healing.
	/// </summary>
	public bool ProvidesRest { get; }

	/// <summary>
	/// Whether this building has NPCs.
	/// </summary>
	public bool HasNPC { get; }

	/// <summary>
	/// NPC ID if this building has an NPC.
	/// </summary>
	public string? NPCId { get; }

	#endregion

	#region Constructor

	public BuildingProto(
		ID id,
		Loc text,
		string iconName = "icon_building",
		BuildingCategory category = BuildingCategory.Utility,
		BuildingSize size = BuildingSize.Small,
		int width = 1,
		int height = 1,
		bool canMove = true,
		bool canDemolish = true,
		int unlockCost = 10,
		int baseUpgradeCost = 5,
		int maxLevel = 5,
		List<BuildingProto.ID>? prerequisites = null,
		int requiredFogClears = 0,
		bool startsUnlocked = false,
		List<BuildingBonusProto>? bonusesPerLevel = null,
		List<BuildingServiceProto>? services = null,
		List<AdjacencyBonusProto>? adjacencyBonuses = null,
		StartingResourceBonusProto? startingBonus = null,
		bool hasShop = false,
		bool providesRest = false,
		bool hasNPC = false,
		string? npcId = null
	) : base(id, text) {
		IconName = iconName;
		Category = category;
		Size = size;
		Width = width;
		Height = height;
		CanMove = canMove;
		CanDemolish = canDemolish;
		UnlockCost = unlockCost;
		BaseUpgradeCost = baseUpgradeCost;
		MaxLevel = maxLevel;
		Prerequisites = prerequisites ?? [];
		RequiredFogClears = requiredFogClears;
		StartsUnlocked = startsUnlocked;
		BonusesPerLevel = bonusesPerLevel ?? [];
		Services = services ?? [];
		AdjacencyBonuses = adjacencyBonuses ?? [];
		StartingBonus = startingBonus;
		HasShop = hasShop;
		ProvidesRest = providesRest;
		HasNPC = hasNPC;
		NPCId = npcId;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets the upgrade cost for a specific level.
	/// </summary>
	public int GetUpgradeCost(int toLevel) => BaseUpgradeCost * toLevel;

	/// <summary>
	/// Gets all bonuses for a specific level (accumulated).
	/// </summary>
	public IEnumerable<BuildingBonusProto> GetBonusesForLevel(int level) {
		foreach (var bonus in BonusesPerLevel) {
			yield return new BuildingBonusProto(bonus.Stat, bonus.Value * level, bonus.Operation);
		}
	}

	#endregion
}

#region Supporting Proto Types

/// <summary>
/// A service provided by a building.
/// </summary>
public class BuildingServiceProto {
	public string Id { get; }
	public string Name { get; }
	public string Description { get; }
	public string IconName { get; }
	public BuildingServiceType Type { get; }
	public int UnlockLevel { get; }

	public BuildingServiceProto(
		string id,
		string name,
		string description,
		BuildingServiceType type,
		int unlockLevel = 1,
		string iconName = "icon_service"
	) {
		Id = id;
		Name = name;
		Description = description;
		Type = type;
		UnlockLevel = unlockLevel;
		IconName = iconName;
	}
}

/// <summary>
/// Types of services buildings can provide.
/// </summary>
public enum BuildingServiceType {
	Shop,
	Craft,
	Repair,
	Rest,
	Heal,
	Train,
	Research,
	Storage,
	Transport,
	Quest,
	Special
}

/// <summary>
/// Bonus for being adjacent to another building.
/// </summary>
public class AdjacencyBonusProto {
	public BuildingProto.ID AdjacentBuildingId { get; }
	public StatProto.ID BonusStat { get; }
	public float BonusValue { get; }
	public string Description { get; }

	public AdjacencyBonusProto(
		BuildingProto.ID adjacentBuildingId,
		StatProto.ID bonusStat,
		float bonusValue,
		string description
	) {
		AdjacentBuildingId = adjacentBuildingId;
		BonusStat = bonusStat;
		BonusValue = bonusValue;
		Description = description;
	}
}

/// <summary>
/// Starting resource bonus from a building.
/// </summary>
public class StartingResourceBonusProto {
	public int GoldPerLevel { get; }
	public int FoodPerLevel { get; }
	public int MedicalSuppliesPerLevel { get; }
	public int CampingSuppliesPerLevel { get; }

	public StartingResourceBonusProto(
		int goldPerLevel = 0,
		int foodPerLevel = 0,
		int medicalSuppliesPerLevel = 0,
		int campingSuppliesPerLevel = 0
	) {
		GoldPerLevel = goldPerLevel;
		FoodPerLevel = foodPerLevel;
		MedicalSuppliesPerLevel = medicalSuppliesPerLevel;
		CampingSuppliesPerLevel = campingSuppliesPerLevel;
	}
}

#endregion