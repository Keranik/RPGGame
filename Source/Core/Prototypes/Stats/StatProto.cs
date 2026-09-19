using RPGGame.Core.Generation;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Prototypes.Stats;

/// <summary>
/// Prototype for stat definitions.
/// Stats are numeric values that can be modified (attributes, resources, resistances, etc.)
/// </summary>
public class StatProto : Proto {
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
	public string IconName { get; init; } = "icon_stat";

	/// <summary>Color hint for UI (hex string).</summary>
	public string ColorHint { get; init; } = "#FFFFFF";

	/// <summary>Sort order within category (lower = first).</summary>
	public int DisplayOrder { get; init; } = 100;

	/// <summary>Whether to display as integer.</summary>
	public bool DisplayAsInt { get; init; } = true;

	/// <summary>Whether to display as percentage.</summary>
	public bool DisplayAsPercent { get; init; } = false;

	/// <summary>Format string for display (e.g., "+{0}", "{0}%").</summary>
	public string DisplayFormat { get; init; } = "{0}";

	/// <summary>Whether higher values are better (for coloring).</summary>
	public bool HigherIsBetter { get; init; } = true;

	/// <summary>Abbreviated name for compact displays.</summary>
	public string? Abbreviation { get; init; } = null;

	#endregion

	#region Categorization

	/// <summary>Category this stat belongs to.</summary>
	public StatCategoryProto.ID Category { get; init; }

	/// <summary>Tags for filtering and bonuses.</summary>
	public List<TagProto.ID> Tags { get; init; } = [];

	#endregion

	#region Value Constraints

	/// <summary>Default/base value for this stat.</summary>
	public float DefaultValue { get; init; } = 0f;

	/// <summary>Minimum possible value (null = no minimum).</summary>
	public float? MinValue { get; init; } = null;

	/// <summary>Maximum possible value (null = no maximum).</summary>
	public float? MaxValue { get; init; } = null;

	#endregion

	#region Derivation

	/// <summary>
	/// Stats this stat derives from.
	/// E.g., MaxHealth derives from Constitution.
	/// </summary>
	public List<StatDerivation> DerivedFrom { get; init; } = [];

	/// <summary>Whether this stat is automatically derived and shouldn't be set directly.</summary>
	public bool IsDerived => DerivedFrom.Count > 0;

	#endregion

	#region Constructor

	public StatProto(ID id, Loc text, StatCategoryProto.ID category) : base(id, text) {
		Category = category;
	}

	public StatProto(ID id, string name, string description, StatCategoryProto.ID category)
		: this(id, Proto.CreateText(name, description), category) { }

	#endregion

	#region Methods

	/// <summary>Clamps a value to this stat's constraints.</summary>
	public float Clamp(float value) {
		if (MinValue.HasValue && value < MinValue.Value) {
			return MinValue.Value;
		}
		if (MaxValue.HasValue && value > MaxValue.Value) {
			return MaxValue.Value;
		}
		return value;
	}

	/// <summary>Formats a value for display.</summary>
	public string FormatValue(float value) {
		float displayValue = DisplayAsInt ? MathF.Floor(value) : value;
		if (DisplayAsPercent) {
			displayValue *= 100f;
		}
		return string.Format(DisplayFormat, displayValue);
	}

	/// <summary>Checks if this stat has a specific tag.</summary>
	public bool HasTag(TagProto.ID tagId) => Tags.Any(t => t == tagId);

	#endregion
}

/// <summary>
/// Defines how one stat derives from another.
/// </summary>
public readonly record struct StatDerivation(
	StatProto.ID SourceStat,
	float Multiplier = 1f,
	float Offset = 0f,
	bool UseModifier = false // If true, uses D&D-style attribute modifier
);