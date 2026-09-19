using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Prototypes.Village;
using RPGGame.Core.Stats;
using UnityEngine;

namespace RPGGame.Core.Village;

/// <summary>
/// Represents a placed building instance in the village.
/// </summary>
public class Building {
	#region Properties

	/// <summary>
	/// The building prototype definition.
	/// </summary>
	public BuildingProto Proto { get; }

	/// <summary>
	/// Current level of this building.
	/// </summary>
	public int Level { get; private set; }

	/// <summary>
	/// Position in the village grid.
	/// </summary>
	public Vector2Int Position { get; private set; }

	/// <summary>
	/// Whether this building is currently active/functional.
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// Unique instance ID for this building.
	/// </summary>
	public string InstanceId { get; }

	/// <summary>
	/// Custom name set by player (if allowed).
	/// </summary>
	public string? CustomName { get; set; }

	/// <summary>
	/// Display name (custom or default).
	/// </summary>
	public string DisplayName => CustomName ?? Proto.DisplayText.Name;

	/// <summary>
	/// Building description.
	/// </summary>
	public string Description => Proto.DisplayText.Description;

	#endregion

	#region Computed Properties

	/// <summary>
	/// All tiles occupied by this building.
	/// </summary>
	public IEnumerable<Vector2Int> OccupiedTiles {
		get {
			for (int x = 0; x < Proto.Width; x++) {
				for (int y = 0; y < Proto.Height; y++) {
					yield return new Vector2Int(Position.x + x, Position.y + y);
				}
			}
		}
	}

	/// <summary>
	/// Center position of this building.
	/// </summary>
	public Vector2 CenterPosition => new(
		Position.x + Proto.Width / 2f,
		Position.y + Proto.Height / 2f
	);

	/// <summary>
	/// Whether this building can be upgraded.
	/// </summary>
	public bool CanUpgrade => Level < Proto.MaxLevel;

	/// <summary>
	/// Cost to upgrade to the next level.
	/// </summary>
	public int UpgradeCost => Proto.GetUpgradeCost(Level + 1);

	/// <summary>
	/// Whether this building can be moved.
	/// </summary>
	public bool CanMove => Proto.CanMove;

	/// <summary>
	/// Whether this building can be demolished.
	/// </summary>
	public bool CanDemolish => Proto.CanDemolish;

	/// <summary>
	/// Building category.
	/// </summary>
	public BuildingCategory Category => Proto.Category;

	/// <summary>
	/// Building icon name.
	/// </summary>
	public string IconName => Proto.IconName;

	#endregion

	#region Constructor

	public Building(BuildingProto proto, Vector2Int position, int level = 1) {
		Proto = proto;
		Position = position;
		Level = Math.Clamp(level, 1, proto.MaxLevel);
		InstanceId = Guid.NewGuid().ToString();
	}

	/// <summary>
	/// Constructor for loading from save data.
	/// </summary>
	public Building(BuildingProto proto, BuildingSaveData saveData) {
		Proto = proto;
		Position = new Vector2Int(saveData.PositionX, saveData.PositionY);
		Level = saveData.Level;
		InstanceId = saveData.InstanceId;
		CustomName = saveData.CustomName;
		IsActive = saveData.IsActive;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Upgrades this building by one level.
	/// </summary>
	public bool Upgrade() {
		if (!CanUpgrade) {
			return false;
		}
		Level++;
		return true;
	}

	/// <summary>
	/// Moves this building to a new position.
	/// </summary>
	public bool MoveTo(Vector2Int newPosition) {
		if (!CanMove) {
			return false;
		}
		Position = newPosition;
		return true;
	}

	/// <summary>
	/// Gets services available at the current level.
	/// </summary>
	public IEnumerable<BuildingService> GetAvailableServices() {
		return Proto.Services
			.Where(s => s.UnlockLevel <= Level)
			.Select(s => new BuildingService {
				Id = s.Id,
				Name = s.Name,
				Description = s.Description,
				Type = (BuildingServiceType)s.Type,
				UnlockLevel = s.UnlockLevel
			});
	}

	/// <summary>
	/// Gets stat bonuses for the current level.
	/// </summary>
	public IEnumerable<StatModifier> GetCurrentBonuses() {
		foreach (var bonus in Proto.BonusesPerLevel) {
			float value = bonus.Value * Level;
			yield return StatModifier.CreateMetaBonus(
					bonus.Stat,
					value,
					$"{DisplayName} Lv{Level}",
					bonus.Operation
				);
		}
	}

	/// <summary>
	/// Checks if this building occupies a specific tile.
	/// </summary>
	public bool OccupiesTile(Vector2Int tile) {
		return tile.x >= Position.x && tile.x < Position.x + Proto.Width &&
			   tile.y >= Position.y && tile.y < Position.y + Proto.Height;
	}

	/// <summary>
	/// Checks if this building is adjacent to another building.
	/// </summary>
	public bool IsAdjacentTo(Building other) {
		foreach (var myTile in OccupiedTiles) {
			foreach (var otherTile in other.OccupiedTiles) {
				int dx = Math.Abs(myTile.x - otherTile.x);
				int dy = Math.Abs(myTile.y - otherTile.y);

				// Adjacent if sharing an edge (not diagonal)
				if ((dx == 1 && dy == 0) || (dx == 0 && dy == 1)) {
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Gets adjacency bonuses from nearby buildings.
	/// </summary>
	public IEnumerable<AdjacencyBonus> GetActiveAdjacencyBonuses(IEnumerable<Building> nearbyBuildings) {
		foreach (var bonus in Proto.AdjacencyBonuses) {
			if (nearbyBuildings.Any(b => b.Proto.Id == bonus.AdjacentBuildingId && IsAdjacentTo(b))) {
				yield return new AdjacencyBonus {
					AdjacentBuildingId = bonus.AdjacentBuildingId.Value,
					BonusStat = bonus.BonusStat,
					BonusValue = bonus.BonusValue,
					Description = bonus.Description
				};
			}
		}
	}

	#endregion

	#region Serialization

	/// <summary>
	/// Converts to save data.
	/// </summary>
	public BuildingSaveData ToSaveData() {
		return new BuildingSaveData {
			BuildingId = Proto.Id.Value,
			InstanceId = InstanceId,
			PositionX = Position.x,
			PositionY = Position.y,
			Level = Level,
			CustomName = CustomName,
			IsActive = IsActive
		};
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// A service provided by a building.
/// </summary>
public class BuildingService {
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public string Description { get; set; } = "";
	public BuildingServiceType Type { get; set; }
	public int UnlockLevel { get; set; } = 1;
}

/// <summary>
/// Adjacency bonus from nearby buildings.
/// </summary>
public class AdjacencyBonus {
	public string AdjacentBuildingId { get; set; } = "";
	public StatProto.ID BonusStat { get; set; }
	public float BonusValue { get; set; }
	public string Description { get; set; } = "";
}

#endregion

#region Save Data

/// <summary>
/// Serializable save data for a building instance.
/// </summary>
public class BuildingSaveData {
	public string BuildingId { get; set; } = "";
	public string InstanceId { get; set; } = "";
	public int PositionX { get; set; }
	public int PositionY { get; set; }
	public int Level { get; set; }
	public string? CustomName { get; set; }
	public bool IsActive { get; set; } = true;
}

#endregion