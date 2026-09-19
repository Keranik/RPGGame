using RPGGame.Core.Expedition;

namespace RPGGame.Core.Prototypes.Locations;

/// <summary>
/// Prototype for landmark locations - points of interest on the map.
/// Landmarks are distinctive features like ancient trees, stone circles, etc.
/// </summary>
public class LandmarkProto : MapLocationProto {
	/// <summary>Landmark-specific ID.</summary>
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

	public override LocationCategory Category => LocationCategory.Landmark;

	/// <summary>Type of landmark for visual/behavior.</summary>
	public LandmarkType LandmarkType { get; init; }

	/// <summary>Whether this landmark is interactive (can be examined).</summary>
	public bool IsInteractive { get; init; } = true;

	/// <summary>Whether this landmark provides a buff when visited.</summary>
	public bool ProvidesBuff { get; init; }

	/// <summary>Effect granted when visiting (if ProvidesBuff).</summary>
	public Effects.EffectProto.ID? VisitEffect { get; init; }

	/// <summary>Duration of visit effect in hours.</summary>
	public int VisitEffectDuration { get; init; } = 24;

	/// <summary>Cooldown before effect can be gained again (hours).</summary>
	public int EffectCooldown { get; init; } = 48;

	/// <summary>Lore entry unlocked when discovered.</summary>
	public Lore.LoreProto.ID? UnlocksLore { get; init; }

	/// <summary>Whether this landmark reveals nearby fog of war.</summary>
	public int VisionBonus { get; init; }

	#endregion

	#region Constructor

	public LandmarkProto(ID id, Loc text, LandmarkType type) : base(id, text) {
		Id = id;
		LandmarkType = type;
	}

	public LandmarkProto(ID id, string name, string description, LandmarkType type)
		: this(id, CreateText(name, description), type) { }

	#endregion
}

/// <summary>
/// Types of landmarks.
/// </summary>
public enum LandmarkType {
	// Natural
	AncientTree,
	StoneCircle,
	CrystalFormation,
	Waterfall,
	HotSpring,
	GiantMushroom,
	FogMonument,
	TwistedTree,
	FloatingRocks,

	// Ruins
	RuinedShrine,
	AncientStatue,
	FallenPillar,
	BrokenArch,
	OldBattlefield,
	GraveSite,
	AbandonedAltar,

	// Man-made
	Signpost,
	Crossroads,
	OldWell,
	WayShrine,
	WatchTower,
	AbandonedCart,
	HermitHut,
	BanditSign,

	// Magical
	ManaWell,
	LeyLineNode,
	TemporalRift,
	ShadowTear,
	FeyCircle,
	DemonScar,

	// Special
	VillageGate,
	FogBoundary,
	FogHeart
}