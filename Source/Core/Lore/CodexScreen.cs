namespace RPGGame.Core.Lore;

/// <summary>
/// Data for displaying the codex/lore screen.
/// </summary>
public class CodexDisplayData {
	/// <summary>
	/// Currently selected category.
	/// </summary>
	public LoreCategory? SelectedCategory { get; set; }

	/// <summary>
	/// Currently selected entry.
	/// </summary>
	public string? SelectedEntryId { get; set; }

	/// <summary>
	/// Search filter text.
	/// </summary>
	public string SearchFilter { get; set; } = "";

	/// <summary>
	/// Whether to show only unread entries.
	/// </summary>
	public bool ShowUnreadOnly { get; set; } = false;

	/// <summary>
	/// Whether to show locked entries (with hints).
	/// </summary>
	public bool ShowLockedEntries { get; set; } = false;
}

/// <summary>
/// Represents a category node in the codex UI.
/// </summary>
public class CodexCategoryNode {
	public LoreCategory Category { get; set; }
	public string Name { get; set; } = "";
	public string IconName { get; set; } = "";
	public int DiscoveredCount { get; set; }
	public int TotalCount { get; set; }
	public int UnreadCount { get; set; }
	public bool IsExpanded { get; set; }
	public List<CodexEntryNode> Entries { get; set; } = [];

	public float CompletionPercent => TotalCount > 0 ? (float)DiscoveredCount / TotalCount * 100 : 0;
	public bool HasUnread => UnreadCount > 0;
	public bool IsComplete => DiscoveredCount == TotalCount && TotalCount > 0;
}

/// <summary>
/// Represents an entry node in the codex UI.
/// </summary>
public class CodexEntryNode {
	public string Id { get; set; } = "";
	public string Title { get; set; } = "";
	public string Summary { get; set; } = "";
	public string IconName { get; set; } = "";
	public bool IsDiscovered { get; set; }
	public bool IsRead { get; set; }
	public bool IsNew { get; set; }
	public bool IsImportant { get; set; }
	public string? DiscoveryHint { get; set; }
}

/// <summary>
/// Builder for codex UI data.
/// </summary>
public static class CodexBuilder {
	/// <summary>
	/// Builds the category tree for the codex UI.
	/// </summary>
	public static List<CodexCategoryNode> BuildCategoryTree(
		LoreManager loreManager,
		CodexDisplayData displayData
	) {
		var nodes = new List<CodexCategoryNode>();

		foreach (LoreCategory category in Enum.GetValues(typeof(LoreCategory))) {
			var entries = loreManager.GetEntriesByCategory(category).ToList();

			if (entries.Count == 0) {
				continue;
			}

			// Apply filters
			var filteredEntries = entries.Where(e => {
				// Search filter
				if (!string.IsNullOrEmpty(displayData.SearchFilter)) {
					var query = displayData.SearchFilter.ToLower();
					if (!e.Entry.Title.ToLower().Contains(query) &&
						(!e.IsDiscovered || !e.Entry.Content.ToLower().Contains(query))) {
						return false;
					}
				}

				// Unread filter
				if (displayData.ShowUnreadOnly && (!e.IsDiscovered || e.IsRead)) {
					return false;
				}

				// Locked filter
				if (!displayData.ShowLockedEntries && !e.IsDiscovered) {
					return false;
				}

				return true;
			}).ToList();

			if (filteredEntries.Count == 0 && !displayData.ShowLockedEntries) {
				continue;
			}

			var categoryNode = new CodexCategoryNode {
				Category = category,
				Name = category.GetDisplayName(),
				IconName = category.GetIconName(),
				DiscoveredCount = entries.Count(e => e.IsDiscovered),
				TotalCount = entries.Count,
				UnreadCount = entries.Count(e => e.IsDiscovered && !e.IsRead),
				IsExpanded = displayData.SelectedCategory == category
			};

			var newlyDiscovered = loreManager.GetNewlyDiscoveredIds().ToHashSet();

			foreach (var state in filteredEntries) {
				categoryNode.Entries.Add(new CodexEntryNode {
					Id = state.Entry.Id,
					Title = state.IsDiscovered ? state.Entry.Title : "???",
					Summary = state.IsDiscovered ? state.Entry.Summary : state.Entry.DiscoveryHint,
					IconName = state.Entry.IconName,
					IsDiscovered = state.IsDiscovered,
					IsRead = state.IsRead,
					IsNew = newlyDiscovered.Contains(state.Entry.Id),
					IsImportant = state.Entry.IsImportant,
					DiscoveryHint = !state.IsDiscovered ? state.Entry.DiscoveryHint : null
				});
			}

			nodes.Add(categoryNode);
		}

		return nodes;
	}

	/// <summary>
	/// Gets the full display content for a lore entry.
	/// </summary>
	public static LoreEntryDisplay? GetEntryDisplay(LoreManager loreManager, string entryId) {
		var state = loreManager.GetEntryState(entryId);
		if (state == null || !state.IsDiscovered) {
			return null;
		}

		var entry = state.Entry;

		return new LoreEntryDisplay {
			Id = entry.Id,
			Title = entry.Title,
			Category = entry.Category.GetDisplayName(),
			Content = entry.GetFormattedContent(),
			ImageName = entry.ImageName,
			IsImportant = entry.IsImportant,
			DiscoveredAt = state.DiscoveredAt,
			RelatedEntries = entry.RelatedEntries
				.Select(id => loreManager.GetEntry(id))
				.Where(e => e != null && loreManager.IsDiscovered(e!.Id))
				.Select(e => new RelatedEntryInfo { Id = e!.Id, Title = e.Title })
				.ToList()
		};
	}
}

/// <summary>
/// Full display data for a lore entry.
/// </summary>
public class LoreEntryDisplay {
	public string Id { get; set; } = "";
	public string Title { get; set; } = "";
	public string Category { get; set; } = "";
	public string Content { get; set; } = "";
	public string? ImageName { get; set; }
	public bool IsImportant { get; set; }
	public DateTime? DiscoveredAt { get; set; }
	public List<RelatedEntryInfo> RelatedEntries { get; set; } = [];
}

/// <summary>
/// Related entry reference.
/// </summary>
public class RelatedEntryInfo {
	public string Id { get; set; } = "";
	public string Title { get; set; } = "";
}