using RPGGame.Core.Lore;

namespace RPGGame.Core.Prototypes.Lore;
#nullable disable

/// <summary>
/// Prototype for lore entry definitions.
/// </summary>
public class LoreProto : Proto {
	new public readonly struct ID(string value) : IEquatable<ID>, IComparable<ID> {
		public readonly string Value = value;

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(Proto.ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, Proto.ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(Proto.ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, Proto.ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static implicit operator Proto.ID(ID id) => new Proto.ID(id.Value);
	}

	new public ID Id => new(base.Id.Value);

	public LoreCategory Category { get; }
	public string Subcategory { get; }
	public string IconName { get; }
	public string Summary { get; }
	public string Content { get; }
	public string Quote { get; }
	public string QuoteSource { get; }
	public string ImageName { get; }
	public bool StartsDiscovered { get; }
	public string DiscoveryHint { get; }
	public int RequiredFogClears { get; }
	public List<LoreProto.ID> RelatedEntries { get; }
	public List<LoreProto.ID> Prerequisites { get; }
	public int SortOrder { get; }
	public bool IsImportant { get; }
	public bool IsSpoiler { get; }

	public LoreProto(
		ID id,
		Loc text,
		LoreCategory category = LoreCategory.World,
		string subcategory = null,
		string iconName = "icon_lore_default",
		string summary = "",
		string content = "",
		string quote = null,
		string quoteSource = null,
		string imageName = null,
		bool startsDiscovered = false,
		string discoveryHint = "???",
		int requiredFogClears = 0,
		List<LoreProto.ID> relatedEntries = null,
		List<LoreProto.ID> prerequisites = null,
		int sortOrder = 0,
		bool isImportant = false,
		bool isSpoiler = false
	) : base(id, text) {
		Category = category;
		Subcategory = subcategory;
		IconName = iconName;
		Summary = summary;
		Content = content;
		Quote = quote;
		QuoteSource = quoteSource;
		ImageName = imageName;
		StartsDiscovered = startsDiscovered;
		DiscoveryHint = discoveryHint;
		RequiredFogClears = requiredFogClears;
		RelatedEntries = relatedEntries ?? [];
		Prerequisites = prerequisites ?? [];
		SortOrder = sortOrder;
		IsImportant = isImportant;
		IsSpoiler = isSpoiler;
	}

	public string GetFormattedContent() {
		var parts = new List<string>();
		if (!string.IsNullOrEmpty(Quote)) {
			string quoteBlock = $"\"{Quote}\"";
			if (!string.IsNullOrEmpty(QuoteSource)) {
				quoteBlock += $"\n— {QuoteSource}";
			}
			parts.Add(quoteBlock);
			parts.Add("");
		}
		parts.Add(Content);
		return string.Join("\n", parts);
	}
}
