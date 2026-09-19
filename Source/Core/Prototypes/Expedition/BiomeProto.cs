using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Locations;

namespace RPGGame.Core.Prototypes.Expedition;

/// <summary>
/// Defines a biome segment for procedural path generation.
/// Biomes control terrain distribution, encounter rates, and atmospheric progression.
/// </summary>
public class BiomeProto : Proto {
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

	/// <summary>Icon for UI display.</summary>
	public string IconName { get; init; } = "biome_default";

	/// <summary>Color hint for map tinting (hex string).</summary>
	public string ColorHint { get; init; } = "#4a7c4e";

	/// <summary>Ambient description shown when entering the biome.</summary>
	public string? AmbientDescription { get; init; }

	/// <summary>Music track ID for this biome.</summary>
	public string? MusicId { get; init; }

	/// <summary>Ambient sound ID for this biome.</summary>
	public string? AmbientSoundId { get; init; }

	#endregion

	#region Terrain Distribution

	/// <summary>Primary terrain type for this biome (most common).</summary>
	public TerrainProto.ID PrimaryTerrain { get; }

	/// <summary>
	/// Variant terrains that can appear in this biome with weights.
	/// Weight determines relative probability (higher = more common).
	/// </summary>
	public List<WeightedTerrain> VariantTerrains { get; init; } = [];

	/// <summary>
	/// Chance (0-1) that a node uses a variant terrain instead of primary.
	/// </summary>
	public float VariantTerrainChance { get; init; } = 0.3f;

	#endregion

	#region Segment Generation

	/// <summary>Minimum length of this biome segment (in nodes).</summary>
	public int MinLength { get; init; } = 5;

	/// <summary>Maximum length of this biome segment (in nodes).</summary>
	public int MaxLength { get; init; } = 12;

	/// <summary>
	/// Danger level (0-1). Affects encounter difficulty and loot quality.
	/// </summary>
	public float DangerLevel { get; init; } = 0.3f;

	/// <summary>
	/// Order in the biome progression. Lower = earlier in expedition.
	/// Use -1 for biomes that only appear via branching or special placement.
	/// </summary>
	public int ProgressionOrder { get; init; } = 0;

	#endregion

	#region Generation Multipliers

	/// <summary>
	/// Multiplier for branch generation chance in this biome.
	/// Higher = more side paths.
	/// </summary>
	public float BranchChanceMultiplier { get; init; } = 1.0f;

	/// <summary>
	/// Bonus added to dungeon spawn chance at branch ends.
	/// </summary>
	public float DungeonChanceBonus { get; init; } = 0f;

	/// <summary>
	/// Multiplier for landmark spawn chance.
	/// </summary>
	public float LandmarkChanceMultiplier { get; init; } = 1.0f;

	/// <summary>
	/// Multiplier for event spawn density.
	/// </summary>
	public float EventDensityMultiplier { get; init; } = 1.0f;

	/// <summary>
	/// Multiplier for resource node spawn chance.
	/// </summary>
	public float ResourceNodeMultiplier { get; init; } = 1.0f;

	/// <summary>
	/// Multiplier for campsite spacing (higher = campsites further apart).
	/// </summary>
	public float CampsiteSpacingMultiplier { get; init; } = 1.0f;

	#endregion

	#region Allowed Content

	/// <summary>
	/// Dungeon types that can spawn in this biome.
	/// Empty = use defaults based on terrain.
	/// </summary>
	public List<DungeonProto.ID> AllowedDungeons { get; init; } = [];

	/// <summary>
	/// Landmark types that can spawn in this biome.
	/// Empty = use defaults based on terrain.
	/// </summary>
	public List<LandmarkProto.ID> AllowedLandmarks { get; init; } = [];

	/// <summary>
	/// Event types that are preferred in this biome.
	/// Empty = use defaults based on terrain.
	/// </summary>
	public List<EventProto.ID> PreferredEvents { get; init; } = [];

	/// <summary>
	/// Encounter types that are preferred in this biome.
	/// </summary>
	public List<EncounterProto.ID> PreferredEncounters { get; init; } = [];

	/// <summary>
	/// Tags for filtering and bonuses.
	/// </summary>
	public List<TagProto.ID> Tags { get; init; } = [];

	#endregion

	#region Transition

	/// <summary>
	/// Biomes that can naturally follow this one.
	/// Empty = any biome at the next progression level.
	/// </summary>
	public List<ID> ValidNextBiomes { get; init; } = [];

	/// <summary>
	/// Whether this biome can appear in branches (side paths).
	/// </summary>
	public bool CanAppearInBranches { get; init; } = true;

	/// <summary>
	/// Flavor text shown when transitioning into this biome.
	/// </summary>
	public string? TransitionText { get; init; }

	#endregion

	#region Constructor

	public BiomeProto(
		ID id,
		Loc text,
		TerrainProto.ID primaryTerrain,
		int progressionOrder = 0,
		float dangerLevel = 0.3f
	) : base(id, text) {
		PrimaryTerrain = primaryTerrain;
		ProgressionOrder = progressionOrder;
		DangerLevel = dangerLevel;
	}

	public BiomeProto(
		ID id,
		string name,
		string description,
		TerrainProto.ID primaryTerrain,
		int progressionOrder = 0,
		float dangerLevel = 0.3f
	) : this(id, Proto.CreateText(name, description), primaryTerrain, progressionOrder, dangerLevel) { }

	#endregion

	#region Methods

	/// <summary>
	/// Checks if this biome has a specific tag.
	/// </summary>
	public bool HasTag(TagProto.ID tagId) => Tags.Any(t => t == tagId);

	/// <summary>
	/// Gets the effective branch chance for this biome.
	/// </summary>
	public float GetEffectiveBranchChance(float baseChance) => baseChance * BranchChanceMultiplier;

	/// <summary>
	/// Gets the effective dungeon chance for this biome.
	/// </summary>
	public float GetEffectiveDungeonChance(float baseChance) => baseChance + DungeonChanceBonus;

	/// <summary>
	/// Gets the effective landmark chance for this biome.
	/// </summary>
	public float GetEffectiveLandmarkChance(float baseChance) => baseChance * LandmarkChanceMultiplier;

	/// <summary>
	/// Gets the effective event chance for this biome.
	/// </summary>
	public float GetEffectiveEventChance(float baseChance) => baseChance * EventDensityMultiplier;

	/// <summary>
	/// Gets the effective resource node chance for this biome.
	/// </summary>
	public float GetEffectiveResourceChance(float baseChance) => baseChance * ResourceNodeMultiplier;

	/// <summary>
	/// Gets the effective campsite spacing for this biome.
	/// </summary>
	public int GetEffectiveCampsiteSpacing(int baseSpacing) =>
		(int)Math.Round(baseSpacing * CampsiteSpacingMultiplier);

	/// <summary>
	/// Checks if this biome is part of the main progression path.
	/// </summary>
	public bool IsMainPathBiome => ProgressionOrder >= 0;

	/// <summary>
	/// Checks if a specific dungeon is allowed in this biome.
	/// </summary>
	public bool AllowsDungeon(DungeonProto.ID dungeonId) =>
		AllowedDungeons.Count == 0 || AllowedDungeons.Contains(dungeonId);

	/// <summary>
	/// Checks if a specific landmark is allowed in this biome.
	/// </summary>
	public bool AllowsLandmark(LandmarkProto.ID landmarkId) =>
		AllowedLandmarks.Count == 0 || AllowedLandmarks.Contains(landmarkId);

	/// <summary>
	/// Selects a terrain for a node in this biome using the provided random stream.
	/// </summary>
	public TerrainProto.ID SelectTerrain(ref RandomStream rng) {
		// Check if we should use a variant
		if (VariantTerrains.Count > 0 && rng.NextFloat() < VariantTerrainChance) {
			return SelectWeightedVariant(ref rng);
		}
		return PrimaryTerrain;
	}

	/// <summary>
	/// Selects a weighted variant terrain.
	/// </summary>
	private TerrainProto.ID SelectWeightedVariant(ref RandomStream rng) {
		float totalWeight = VariantTerrains.Sum(v => v.Weight);
		float roll = rng.NextFloat() * totalWeight;
		float cumulative = 0f;

		foreach (var variant in VariantTerrains) {
			cumulative += variant.Weight;
			if (roll <= cumulative) {
				return variant.Terrain;
			}
		}

		// Fallback to last variant or primary
		return VariantTerrains.Count > 0 ? VariantTerrains[^1].Terrain : PrimaryTerrain;
	}

	/// <summary>
	/// Gets a random length for this biome segment.
	/// </summary>
	public int GetRandomLength(ref RandomStream rng) => rng.NextInt(MinLength, MaxLength + 1);

	#endregion
}

/// <summary>
/// Terrain type with spawn weight for biome variant selection.
/// </summary>
public class WeightedTerrain(TerrainProto.ID terrain, float weight = 1.0f) {
	public TerrainProto.ID Terrain { get; init; } = terrain;
	public float Weight { get; init; } = weight;
}