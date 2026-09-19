using RPGGame.Core.Expedition;

namespace RPGGame.Core.Prototypes.Locations;

/// <summary>
/// Prototype for camp/rest locations.
/// </summary>
public class CampProto : MapLocationProto {
	new public ID Id { get; }

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

		public static implicit operator MapLocationProto.ID(ID id) => new MapLocationProto.ID(id.Value);
		public static implicit operator Proto.ID(ID id) => new Proto.ID(id.Value);
	}

	#region Properties

	public override LocationCategory Category => LocationCategory.Camp;

	/// <summary>Type of camp.</summary>
	public CampType CampType { get; init; }

	/// <summary>Rest effectiveness multiplier (1.0 = normal).</summary>
	public float RestEffectiveness { get; init; } = 1f;

	/// <summary>Safety level - affects ambush chance.</summary>
	public float SafetyLevel { get; init; } = 0.5f;

	/// <summary>Whether fire can be lit here.</summary>
	public bool CanLightFire { get; init; } = true;

	/// <summary>Whether camp provides shelter from weather.</summary>
	public bool HasShelter { get; init; }

	/// <summary>Number of party members that can rest.</summary>
	public int Capacity { get; init; } = 4;

	/// <summary>Whether this camp persists between expeditions.</summary>
	public bool IsPermanent { get; init; }

	/// <summary>Resources available at this camp.</summary>
	public List<Proto.ID> AvailableResources { get; init; } = [];

	#endregion

	#region Constructor

	public CampProto(ID id, Loc text, CampType type) : base(id, text) {
		Id = id;
		CampType = type;
		CanRest = true; // Camps always allow rest
	}

	public CampProto(ID id, string name, string description, CampType type)
		: this(id, CreateText(name, description), type) { }

	#endregion
}

/// <summary>
/// Types of camps.
/// </summary>
public enum CampType {
	Clearing,        // Basic campsite in open area
	Cave,            // Sheltered cave
	Ruins,           // Ruined structure
	Waystation,      // Maintained rest stop
	HuntersCamp,     // Abandoned hunter camp
	BanditCamp,      // Cleared bandit camp
	Shrine,          // Sacred rest area
	Inn,             // Roadside inn
	Outpost          // Military outpost
}