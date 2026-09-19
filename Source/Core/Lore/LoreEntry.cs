namespace RPGGame.Core.Lore;

/// <summary>
/// A single lore entry in the codex.
/// </summary>
public class LoreEntry {
	#region Identity

	/// <summary>
	/// Unique identifier.
	/// </summary>
	public string Id { get; init; } = "";

	/// <summary>
	/// Display title.
	/// </summary>
	public string Title { get; init; } = "";

	/// <summary>
	/// Category for organization.
	/// </summary>
	public LoreCategory Category { get; init; } = LoreCategory.World;

	/// <summary>
	/// Subcategory for further organization.
	/// </summary>
	public string? Subcategory { get; init; }

	/// <summary>
	/// Icon name for display.
	/// </summary>
	public string IconName { get; init; } = "icon_lore_default";

	#endregion

	#region Content

	/// <summary>
	/// Short summary/preview text.
	/// </summary>
	public string Summary { get; init; } = "";

	/// <summary>
	/// Full lore text content.
	/// </summary>
	public string Content { get; init; } = "";

	/// <summary>
	/// Optional flavor quote.
	/// </summary>
	public string? Quote { get; init; }

	/// <summary>
	/// Quote attribution.
	/// </summary>
	public string? QuoteSource { get; init; }

	/// <summary>
	/// Image/illustration name (if any).
	/// </summary>
	public string? ImageName { get; init; }

	#endregion

	#region Discovery

	/// <summary>
	/// Whether this entry starts discovered.
	/// </summary>
	public bool StartsDiscovered { get; init; } = false;

	/// <summary>
	/// Hint shown when entry is locked.
	/// </summary>
	public string DiscoveryHint { get; init; } = "???";

	/// <summary>
	/// Minimum fog clears required to discover.
	/// </summary>
	public int RequiredFogClears { get; init; } = 0;

	/// <summary>
	/// Related entries that may be revealed together.
	/// </summary>
	public List<string> RelatedEntries { get; init; } = [];

	/// <summary>
	/// Entries that must be discovered first.
	/// </summary>
	public List<string> Prerequisites { get; init; } = [];

	#endregion

	#region Metadata

	/// <summary>
	/// Sort order within category.
	/// </summary>
	public int SortOrder { get; init; } = 0;

	/// <summary>
	/// Tags for searching/filtering.
	/// </summary>
	public List<string> Tags { get; init; } = [];

	/// <summary>
	/// Whether this is a major/important entry.
	/// </summary>
	public bool IsImportant { get; init; } = false;

	/// <summary>
	/// Whether this entry contains spoilers.
	/// </summary>
	public bool IsSpoiler { get; init; } = false;

	/// <summary>
	/// Associated achievement ID (if discovering grants achievement).
	/// </summary>
	public string? AchievementId { get; init; }

	#endregion

	#region Methods

	/// <summary>
	/// Gets the formatted content with quote.
	/// </summary>
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

	#endregion
}

/// <summary>
/// Runtime state for a lore entry.
/// </summary>
public class LoreEntryState {
	/// <summary>
	/// The entry data.
	/// </summary>
	public LoreEntry Entry { get; set; } = null!;

	/// <summary>
	/// Whether this entry has been discovered.
	/// </summary>
	public bool IsDiscovered { get; set; }

	/// <summary>
	/// Whether this entry has been read.
	/// </summary>
	public bool IsRead { get; set; }

	/// <summary>
	/// When this entry was discovered.
	/// </summary>
	public DateTime? DiscoveredAt { get; set; }

	/// <summary>
	/// Which run this was discovered in.
	/// </summary>
	public int? DiscoveredInRun { get; set; }
}