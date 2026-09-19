using RPGGame.Core.Generation;

namespace RPGGame.Core.Prototypes.Expedition;

/// <summary>
/// Data-driven terrain definition that replaces hardcoded TerrainType enum behavior.
/// Contains all gameplay-relevant properties for terrain traversal.
/// </summary>
public class TerrainProto : Proto {
	#region Strongly-Typed ID

	new public readonly struct ID(string value) : IEquatable<ID>, IComparable<ID> {
		public readonly string Value = value;

		public bool Equals(ID other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object? obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(ID lhs, ID rhs) => lhs.Equals(rhs);
		public static bool operator !=(ID lhs, ID rhs) => !lhs.Equals(rhs);
		public static bool operator ==(Proto.ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, Proto.ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(Proto.ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, Proto.ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);

		public static implicit operator Proto.ID(ID id) => new(id.Value);
		public static explicit operator ID(Proto.ID id) => new(id.Value);
	}

	new public ID Id => new(base.Id.Value);

	#endregion

	#region Display

	/// <summary>Icon identifier for UI.</summary>
	public string IconName { get; init; } = "terrain_default";

	/// <summary>Color tint for map rendering (hex).</summary>
	public string MapColor { get; init; } = "#4a7c4e";

	/// <summary>Sort order for display (lower = first).</summary>
	public int DisplayOrder { get; init; } = 100;

	#endregion

	#region Movement & Travel

	/// <summary>
	/// Movement speed multiplier (1.0 = normal, 0.5 = half speed, 1.5 = fast).
	/// Roads are fast, swamps are slow.
	/// </summary>
	public Percent MovementSpeedMultiplier { get; init; }

	/// <summary>
	/// Stamina drain per unit of distance traveled.
	/// Higher values = more exhausting terrain.
	/// </summary>
	public Percent StaminaDrainPerDistance { get; init; }

	/// <summary>
	/// Fatigue accumulation rate multiplier.
	/// Mountains and swamps cause more fatigue.
	/// </summary>
	public Percent FatigueRateMultiplier { get; init; }

	/// <summary>
	/// Whether this terrain requires special equipment to traverse.
	/// </summary>
	public bool RequiresSpecialEquipment { get; init; } = false;

	/// <summary>
	/// Equipment type required (e.g., "climbing_gear", "cold_weather", "boat").
	/// </summary>
	public string? RequiredEquipmentType { get; init; }

	#endregion

	#region Environmental Effects

	/// <summary>
	/// Damage per hour of travel (heat, cold, poison, etc.).
	/// 0 = no environmental damage.
	/// </summary>
	public float EnvironmentalDamagePerHour { get; init; } = 0f;

	/// <summary>
	/// Type of environmental damage dealt.
	/// </summary>
	public DamageType EnvironmentalDamageType { get; init; } = DamageType.Physical;

	/// <summary>
	/// Morale modifier per hour of travel.
	/// Negative for depressing terrain, positive for beautiful areas.
	/// </summary>
	public float MoraleModifierPerHour { get; init; } = 0f;

	/// <summary>
	/// Vision range multiplier (fog, dense forest reduce this).
	/// </summary>
	public Percent VisionRangeMultiplier { get; init; } = Percent.Hundred;

	/// <summary>
	/// Whether this terrain provides natural shelter for camping.
	/// </summary>
	public bool ProvidesNaturalShelter { get; init; } = false;

	/// <summary>
	/// Rest effectiveness multiplier when camping here.
	/// </summary>
	public Percent RestEffectivenessMultiplier { get; init; } = Percent.Hundred;

	#endregion

	#region Encounters & Events

	/// <summary>
	/// Base encounter chance modifier.
	/// Higher = more random encounters.
	/// </summary>
	public Percent EncounterChanceMultiplier { get; init; } = Percent.Hundred;

	/// <summary>
	/// Ambush chance modifier (enemies get surprise).
	/// Dense terrain increases this.
	/// </summary>
	public Percent AmbushChanceMultiplier { get; init; } = Percent.Hundred;

	/// <summary>
	/// Tags that influence what events/encounters can spawn here.
	/// </summary>
	public List<TagProto.ID> Tags { get; init; } = [];

	#endregion

	#region Resources

	/// <summary>
	/// Resource categories that can spawn in this terrain.
	/// </summary>
	public List<TerrainResourceCategory> ResourceCategories { get; init; } = [];

	/// <summary>
	/// Foraging chance multiplier.
	/// </summary>
	public Percent ForagingChanceMultiplier { get; init; } = 100.Percent();

	/// <summary>
	/// Hunting success multiplier.
	/// </summary>
	public Percent HuntingSuccessMultiplier { get; init; } = 100.Percent();

	/// <summary>
	/// Water availability (0-1) for survival mechanics.
	/// </summary>
	public Percent WaterAvailability { get; init; } = 50.Percent();

	#endregion

	#region Traversability

	/// <summary>
	/// Whether this terrain is passable at all.
	/// </summary>
	public bool IsPassable { get; init; } = true;

	/// <summary>
	/// Whether this terrain blocks line of sight.
	/// </summary>
	public bool BlocksLineOfSight { get; init; } = false;

	/// <summary>
	/// Terrain category for grouping (natural, urban, underground, magical).
	/// </summary>
	public TerrainCategory Category { get; init; } = TerrainCategory.Natural;

	/// <summary>
	/// Base difficulty rating (0-1) for path generation weighting.
	/// </summary>
	public Percent BaseDifficulty { get; init; } = 30.Percent();

	#endregion

	#region Constructor

	public TerrainProto(
		ID id,
		Loc text,
		Percent movementSpeed,
		Percent staminaDrain,
		Percent fatigueRate
	) : base(id, text) {
		MovementSpeedMultiplier = movementSpeed;
		StaminaDrainPerDistance = staminaDrain;
		FatigueRateMultiplier = fatigueRate;
	}

	public TerrainProto(
		ID id,
		string name,
		string description,
		Percent movementSpeed,
		Percent staminaDrain,
		Percent fatigueRate
	) : this(id, Proto.CreateText(name, description), movementSpeed, staminaDrain, fatigueRate) { }

	#endregion

	#region Methods

	/// <summary>
	/// Checks if this terrain has a specific tag.
	/// </summary>
	public bool HasTag(TagProto.ID tagId) => Tags.Any(t => t == tagId);

	/// <summary>
	/// Checks if this terrain supports a specific resource category.
	/// </summary>
	public bool SupportsResource(TerrainResourceCategory category) => ResourceCategories.Contains(category);

	/// <summary>
	/// Gets the effective movement time for a given base distance.
	/// </summary>
	public float GetMovementTime(float baseDistance) =>
		MovementSpeedMultiplier > 0 ? baseDistance / MovementSpeedMultiplier : float.MaxValue;

	/// <summary>
	/// Gets the stamina cost for traveling a given distance.
	/// </summary>
	public float GetStaminaCost(float distance) => distance * StaminaDrainPerDistance;

	/// <summary>
	/// Gets the fatigue accumulated over a given duration.
	/// </summary>
	public float GetFatigueAccumulation(float hours) => hours * FatigueRateMultiplier;

	/// <summary>
	/// Gets environmental damage for a given duration.
	/// </summary>
	public float GetEnvironmentalDamage(float hours) => hours * EnvironmentalDamagePerHour;

	#endregion
}

/// <summary>
/// High-level terrain category for grouping and filtering.
/// </summary>
public enum TerrainCategory {
	/// <summary>Roads, paths, bridges.</summary>
	Constructed,

	/// <summary>Forests, plains, hills, mountains.</summary>
	Natural,

	/// <summary>Rivers, lakes, ocean.</summary>
	Aquatic,

	/// <summary>Caves, tunnels, mines.</summary>
	Underground,

	/// <summary>Villages, towns, ruins.</summary>
	Settlement,

	/// <summary>Corrupted, void-touched, magical.</summary>
	Supernatural,

	/// <summary>Desert, volcanic, frozen.</summary>
	Extreme,

	Road,
	Grassland,
	Forest,
	Mountain,
	Water,
	Coastal,
	Wetland,
	Desert,
	Frozen,
	Volcanic,
	Ruins,
	Magical,
	Special
}

/// <summary>
/// Resource categories available in terrain for spawning.
/// </summary>
public enum TerrainResourceCategory {
	Herbs,
	Ore,
	Wood,
	Game,
	Fish,
	Forage,
	Magical,
	Salvage
}