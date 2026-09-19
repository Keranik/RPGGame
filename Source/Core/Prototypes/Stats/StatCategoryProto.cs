using RPGGame.Core.Generation;

namespace RPGGame.Core.Prototypes.Stats;

/// <summary>
/// Prototype for stat category definitions.
/// Categories organize stats into logical groups for display and filtering.
/// </summary>
public class StatCategoryProto : Proto {
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

	/// <summary>Icon for this category in UI.</summary>
	public string IconName { get; init; } = "icon_category";

	/// <summary>Color hint for UI (hex string).</summary>
	public string ColorHint { get; init; } = "#FFFFFF";

	/// <summary>Sort order for display (lower = first).</summary>
	public int DisplayOrder { get; init; } = 100;

	/// <summary>Whether this category is expanded by default in UI.</summary>
	public bool DefaultExpanded { get; init; } = true;

	/// <summary>Whether to show this category in character sheets.</summary>
	public bool ShowInCharacterSheet { get; init; } = true;

	/// <summary>Whether to show this category in tooltips.</summary>
	public bool ShowInTooltips { get; init; } = true;

	#endregion

	#region Behavior

	/// <summary>Tags for filtering categories.</summary>
	public List<TagProto.ID> Tags { get; init; } = [];

	/// <summary>Whether stats in this category are typically percentages.</summary>
	public bool StatsArePercentages { get; init; } = false;

	/// <summary>Whether stats in this category can go negative.</summary>
	public bool AllowNegativeStats { get; init; } = false;

	#endregion

	#region Constructor

	public StatCategoryProto(ID id, Loc text) : base(id, text) { }

	public StatCategoryProto(ID id, string name, string description)
		: this(id, Proto.CreateText(name, description)) { }

	#endregion
}