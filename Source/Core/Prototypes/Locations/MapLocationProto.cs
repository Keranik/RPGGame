using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Expedition;

namespace RPGGame.Core.Prototypes.Locations;

/// <summary>
/// Base prototype for all map locations.
/// Locations are places on the world map that can be visited.
/// </summary>
public abstract class MapLocationProto : Proto {
	/// <summary>Prefix for all location IDs.</summary>
	public const string LOCATION_PREFIX = "Location_";

	/// <summary>Location-specific ID.</summary>
	new public ID Id { get; }

	/// <summary>Location-specific ID struct.</summary>
	new public readonly struct ID : IEquatable<ID>, IComparable<ID> {
		public readonly string Value;

		public ID(string value) => Value = value;

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object? obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(ID lhs, ID rhs) => lhs.Equals(rhs);
		public static bool operator !=(ID lhs, ID rhs) => !lhs.Equals(rhs);

		public static implicit operator Proto.ID(ID id) => new Proto.ID(id.Value);
	}

	#region Properties

	/// <summary>Icon displayed on the map.</summary>
	public string IconName { get; init; } = "icon_location_default";

	/// <summary>Tilemap/scene to load when entering this location.</summary>
	public string? SceneName { get; init; }

	/// <summary>Music to play at this location.</summary>
	public string? MusicId { get; init; }

	/// <summary>Ambient sound loop.</summary>
	public string? AmbientId { get; init; }

	/// <summary>Category of this location.</summary>
	public abstract LocationCategory Category { get; }

	/// <summary>Default terrain type for this location.</summary>
	public TerrainProto.ID DefaultTerrain { get; init; } = Ids.Terrains.Plains.Grass;

	/// <summary>Whether this location is safe (no random encounters).</summary>
	public bool IsSafe { get; init; }

	/// <summary>Whether the player can rest here.</summary>
	public bool CanRest { get; init; }

	/// <summary>Whether this location has a shop.</summary>
	public bool HasShop { get; init; }

	/// <summary>Event triggered when first visiting.</summary>
	public EventProto.ID? FirstVisitEventId { get; init; }

	/// <summary>Event triggered on every visit.</summary>
	public EventProto.ID? OnEnterEventId { get; init; }

	/// <summary>Minimum distance from village to spawn (procedural).</summary>
	public int MinSpawnDistance { get; init; }

	/// <summary>Maximum distance from village to spawn (-1 = no limit).</summary>
	public int MaxSpawnDistance { get; init; } = -1;

	/// <summary>Weight for random selection during generation.</summary>
	public float SpawnWeight { get; init; } = 1f;

	/// <summary>Whether this can be spawned procedurally.</summary>
	public bool CanSpawnProcedurally { get; init; } = true;

	/// <summary>Required flag to spawn (meta progression).</summary>
	public string? RequiredFlag { get; init; }

	/// <summary>Terrains where this location can spawn.</summary>
	public List<TerrainProto.ID> ValidTerrains { get; init; } = [];

	#endregion

	#region Constructor

	protected MapLocationProto(ID id, Loc text) : base(id, text) {
		Id = id;
	}

	protected MapLocationProto(ID id, string name, string description)
		: this(id, CreateText(name, description)) { }

	#endregion
}

/// <summary>
/// Categories of map locations.
/// </summary>
public enum LocationCategory {
	/// <summary>Safe settlement.</summary>
	Settlement,

	/// <summary>Natural landmark.</summary>
	Landmark,

	/// <summary>Point of interest with activities.</summary>
	PointOfInterest,

	/// <summary>Resource gathering location.</summary>
	ResourceNode,

	/// <summary>Enterable dungeon/area.</summary>
	Dungeon,

	/// <summary>Camp/rest location.</summary>
	Camp,

	/// <summary>Boss encounter location.</summary>
	BossArena,

	/// <summary>Story-critical location.</summary>
	Story,

	/// <summary>Hidden/secret location.</summary>
	Secret
}