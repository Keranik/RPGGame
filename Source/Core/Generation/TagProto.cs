using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Generation;

/// <summary>
/// Universal prototype for tag definitions.
/// Tags can be applied to any Proto (Skills, Spells, Items, Enemies, Characters, etc.)
/// 
/// Tags are purely descriptive + a collection of modifiers.
/// The interpretation of those modifiers is handled by the systems that use them.
/// </summary>
public class TagProto : Proto {
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
	public string IconName { get; init; } = "icon_tag";

	/// <summary>Color hint for UI (hex string, e.g., "#FF5500").</summary>
	public string ColorHint { get; init; } = "";

	/// <summary>Whether this tag should be displayed in UI.</summary>
	public bool ShowInUI { get; init; } = true;

	/// <summary>Sort order for display (lower = first).</summary>
	public int DisplayOrder { get; init; } = 100;

	/// <summary>
	/// The category of this tag for organization/filtering.
	/// Derived from the tag ID prefix (e.g., "Tag_Element_Fire" → "Element").
	/// </summary>
	public string Category => GetCategoryFromId();

	#endregion

	#region Relationships

	/// <summary>
	/// Parent tags that this tag inherits modifiers from.
	/// </summary>
	public List<ID> ParentTags { get; init; } = [];

	/// <summary>
	/// Tags that conflict with this one (cannot have both).
	/// </summary>
	public List<ID> ConflictingTags { get; init; } = [];

	/// <summary>
	/// Tags that are required to have this tag.
	/// </summary>
	public List<ID> RequiredTags { get; init; } = [];

	/// <summary>
	/// Tags that this tag implies (automatically added when this tag is added).
	/// </summary>
	public List<ID> ImpliedTags { get; init; } = [];

	#endregion

	#region Generation

	/// <summary>Creation point cost for this tag (0 = free/flavor tag).</summary>
	public int CreationPointCost { get; init; } = 0;

	/// <summary>Whether this tag can be randomly applied by generators.</summary>
	public bool CanBeRandomlyApplied { get; init; } = true;

	/// <summary>Minimum level for this tag to appear.</summary>
	public int MinLevel { get; init; } = 0;

	/// <summary>Maximum level for this tag (-1 = no max).</summary>
	public int MaxLevel { get; init; } = -1;

	/// <summary>Weight multiplier for random selection (1.0 = normal).</summary>
	public float SelectionWeight { get; init; } = 1f;

	#endregion

	#region Modifiers

	/// <summary>
	/// Modifiers granted by this tag.
	/// Key = Proto.ID of what's being modified (StatProto.ID, SkillProto.ID, etc.)
	/// Value = how it's modified.
	/// 
	/// Examples:
	///   { Ids.Stats.Attributes.Strength, 2.Flat() }           // +2 Strength
	///   { Ids.Stats.Resistances.Cold, 50.Flat() }             // +50% Cold Resistance
	///   { Ids.Stats.DamageBonus.Fire, 25.PercentMore() }      // 25% more Fire Damage
	///   { Ids.Skills.Stealth.Sneaking, 20.PercentIncrease() } // +20% to Sneaking skill
	///   { Ids.Skills.Combat.Archery, 2.Flat() }               // +2 to Archery skill
	/// </summary>
	public Dictionary<Proto.ID, ValueModifier> Modifiers { get; init; } = [];

	#endregion

	#region Constructor

	public TagProto(ID id, Loc text) : base(id, text) { }

	public TagProto(ID id, string name, string description)
		: this(id, Proto.CreateText(name, description)) { }

	#endregion

	#region Factory Methods

	/// <summary>
	/// Creates a simple categorical tag with no modifiers.
	/// </summary>
	public static TagProto Simple(ID id, string name, string description, string iconName = "icon_tag") {
		return new TagProto(id, name, description) {
			IconName = iconName
		};
	}

	/// <summary>
	/// Creates a tag with a single modifier.
	/// </summary>
	public static TagProto WithModifier(ID id, string name, string description, Proto.ID target, ValueModifier modifier, string iconName = "icon_tag") {
		return new TagProto(id, name, description) {
			IconName = iconName,
			Modifiers = new() { { target, modifier } }
		};
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets the modifier for a specific target, or null if none.
	/// </summary>
	public ValueModifier? GetModifier(Proto.ID target) {
		return Modifiers.TryGetValue(target, out var mod) ? mod : null;
	}

	/// <summary>
	/// Checks if this tag modifies a specific target.
	/// </summary>
	public bool Modifies(Proto.ID target) => Modifiers.ContainsKey(target);

	/// <summary>
	/// Checks if this tag has any modifiers.
	/// </summary>
	public bool HasModifiers => Modifiers.Count > 0;

	/// <summary>
	/// Checks if this is a purely categorical tag (no modifiers).
	/// </summary>
	public bool IsCategoricalOnly => Modifiers.Count == 0;

	/// <summary>
	/// Gets all stat modifiers (filters to only StatProto targets).
	/// </summary>
	public IEnumerable<KeyValuePair<Proto.ID, ValueModifier>> GetStatModifiers() {
		return Modifiers.Where(m => m.Key.Value.StartsWith("Stat_"));
	}

	/// <summary>
	/// Gets all skill modifiers (filters to only SkillProto targets).
	/// </summary>
	public IEnumerable<KeyValuePair<Proto.ID, ValueModifier>> GetSkillModifiers() {
		return Modifiers.Where(m => m.Key.Value.StartsWith("Skill_"));
	}

	/// <summary>
	/// Extracts category from tag ID.
	/// "Tag_Element_Fire" → "Element"
	/// </summary>
	private string GetCategoryFromId() {
		var value = base.Id.Value;
		if (!value.StartsWith("Tag_")) return "";

		var withoutPrefix = value[4..];
		var underscoreIndex = withoutPrefix.IndexOf('_');
		return underscoreIndex > 0 ? withoutPrefix[..underscoreIndex] : withoutPrefix;
	}

	#endregion
}