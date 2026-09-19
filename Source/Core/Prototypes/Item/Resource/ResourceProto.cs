using RPGGame.Core.Expedition;
using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Prototypes.Skills;

namespace RPGGame.Core.Prototypes.Item.Resource;

/// <summary>
/// Prototype for a gatherable resource node in the world.
/// </summary>
public class ResourceProto : Proto {
	#region Strongly-Typed ID

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

	new public ID Id { get; }

	#endregion

	#region Display

	/// <summary>Icon for the resource node.</summary>
	public string IconName { get; init; }

	/// <summary>Sprite/model name for the node in the world.</summary>
	public string SpriteName { get; init; }

	/// <summary>Category of resource (Mining, Herbalism, Woodcutting, etc.).</summary>
	public ResourceCategory Category { get; init; }

	/// <summary>Tags for filtering and bonuses.</summary>
	public List<TagProto.ID> Tags { get; init; } = [];

	#endregion

	#region Gathering

	/// <summary>
	/// Skill used for gathering checks.
	/// References a SkillProto by ID (e.g., Ids.Skills.Gathering.Mining).
	/// </summary>
	public SkillProto.ID GatheringSkill { get; init; }

	/// <summary>Difficulty class for gathering.</summary>
	public int GatheringDC { get; init; }

	/// <summary>Time in seconds per gather attempt.</summary>
	public float GatherTime { get; init; }

	/// <summary>Required tool type (if any).</summary>
	public ToolType? RequiredTool { get; init; }

	#endregion

	#region Yield

	/// <summary>Primary item yielded from this resource.</summary>
	public ItemProto.ID PrimaryYield { get; init; }

	/// <summary>Amount of primary item per successful gather.</summary>
	public int MinYield { get; init; }
	public int MaxYield { get; init; }

	/// <summary>Total number of times this node can be gathered before depleted.</summary>
	public int TotalGathers { get; init; }

	/// <summary>Optional secondary yields with drop chances.</summary>
	public List<ResourceDrop> BonusDrops { get; init; } = [];

	#endregion

	#region Events

	/// <summary>Event triggered when first interacting with the node.</summary>
	public EventProto.ID? OnDiscoverEvent { get; init; }

	/// <summary>Event that can trigger randomly per gather.</summary>
	public EventProto.ID? OnGatherEvent { get; init; }

	/// <summary>Chance (0-1) for OnGatherEvent to trigger.</summary>
	public float GatherEventChance { get; init; }

	/// <summary>Event triggered when node is fully depleted.</summary>
	public EventProto.ID? OnDepletedEvent { get; init; }

	#endregion

	#region Spawning

	/// <summary>Terrain types where this resource can spawn.</summary>
	public List<TerrainProto.ID> ValidTerrains { get; init; } = [];

	/// <summary>Spawn weight (higher = more common).</summary>
	public float SpawnWeight { get; init; }

	/// <summary>Minimum distance from village to spawn.</summary>
	public float MinSpawnDistance { get; init; }

	/// <summary>Time in hours to respawn after depletion (-1 = never).</summary>
	public float RespawnTime { get; init; }

	#endregion

	#region Constructor

	public ResourceProto(
		ID id,
		Loc text,
		string iconName,
		string? spriteName = null,
		ResourceCategory category = ResourceCategory.Gathering,
		SkillProto.ID? gatheringSkill = null,
		int gatheringDC = 10,
		float gatherTime = 5f,
		ToolType? requiredTool = null,
		ItemProto.ID? primaryYield = null,
		int minYield = 1,
		int maxYield = 1,
		int totalGathers = 3,
		List<ResourceDrop>? bonusDrops = null,
		EventProto.ID? onDiscoverEvent = null,
		EventProto.ID? onGatherEvent = null,
		float gatherEventChance = 0f,
		EventProto.ID? onDepletedEvent = null,
		List<TerrainProto.ID>? validTerrains = null,
		float spawnWeight = 10f,
		float minSpawnDistance = 0f,
		float respawnTime = -1f
	) : base(id, text) {
		Id = id;
		IconName = iconName;
		SpriteName = spriteName ?? iconName;
		Category = category;
		GatheringSkill = gatheringSkill ?? default;
		GatheringDC = gatheringDC;
		GatherTime = gatherTime;
		RequiredTool = requiredTool;
		PrimaryYield = primaryYield ?? new ItemProto.ID("");
		MinYield = minYield;
		MaxYield = maxYield;
		TotalGathers = totalGathers;
		BonusDrops = bonusDrops ?? [];
		OnDiscoverEvent = onDiscoverEvent;
		OnGatherEvent = onGatherEvent;
		GatherEventChance = gatherEventChance;
		OnDepletedEvent = onDepletedEvent;
		ValidTerrains = validTerrains ?? [];
		SpawnWeight = spawnWeight;
		MinSpawnDistance = minSpawnDistance;
		RespawnTime = respawnTime;
	}

	#endregion

	#region Methods

	/// <summary>Checks if this resource has a specific tag.</summary>
	public bool HasTag(TagProto.ID tagId) => Tags.Any(t => t == tagId);

	#endregion
}