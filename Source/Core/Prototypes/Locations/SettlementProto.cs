using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes.Village;

namespace RPGGame.Core.Prototypes.Locations;

/// <summary>
/// Prototype for settlement locations - villages, towns, cities.
/// </summary>
public class SettlementProto : MapLocationProto {
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

	public override LocationCategory Category => LocationCategory.Settlement;

	/// <summary>Size of the settlement.</summary>
	public SettlementSize Size { get; init; }

	/// <summary>Population (for flavor/scaling).</summary>
	public int Population { get; init; } = 100;

	/// <summary>Buildings available in this settlement.</summary>
	public List<BuildingProto.ID> AvailableBuildings { get; init; } = [];

	/// <summary>NPCs that can be found here.</summary>
	public List<Proto.ID> ResidentNPCs { get; init; } = [];

	/// <summary>Faction controlling this settlement.</summary>
	public Proto.ID? FactionId { get; init; }

	/// <summary>Whether this is the player's home village.</summary>
	public bool IsHomeVillage { get; init; }

	/// <summary>Whether player can fast travel here.</summary>
	public bool CanFastTravel { get; init; } = true;

	/// <summary>Tax/price modifier for shops (1.0 = normal).</summary>
	public float PriceModifier { get; init; } = 1f;

	/// <summary>Quest board availability.</summary>
	public bool HasQuestBoard { get; init; }

	/// <summary>Guild hall presence.</summary>
	public bool HasGuildHall { get; init; }

	#endregion

	#region Constructor

	public SettlementProto(ID id, Loc text, SettlementSize size) : base(id, text) {
		Id = id;
		Size = size;
		IsSafe = true;
		CanRest = true;
		HasShop = size >= SettlementSize.Village;
	}

	public SettlementProto(ID id, string name, string description, SettlementSize size)
		: this(id, CreateText(name, description), size) { }

	#endregion
}

/// <summary>
/// Size tiers for settlements.
/// </summary>
public enum SettlementSize {
	Hamlet,      // Tiny, few services
	Village,     // Small, basic services
	Town,        // Medium, full services
	City,        // Large, many services
	Capital      // Huge, all services
}