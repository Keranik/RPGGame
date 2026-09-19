using RPGGame.Core.Metrics;
using RPGGame.Core.Simulation;
using UnityEngine;

namespace RPGGame.Core.Lore;

/// <summary>
/// Manages lore discovery and the codex.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class LoreManager {
	#region Fields

	private readonly GameDb k_gameDb;
	private readonly MetricsManager k_metrics;

	private readonly Dictionary<string, LoreEntryState> k_entryStates = [];
	private readonly HashSet<string> k_newlyDiscovered = [];

	#endregion

	#region Properties

	/// <summary>
	/// Total number of lore entries.
	/// </summary>
	public int TotalEntries => k_entryStates.Count;

	/// <summary>
	/// Number of discovered entries.
	/// </summary>
	public int DiscoveredCount => k_entryStates.Values.Count(e => e.IsDiscovered);

	/// <summary>
	/// Number of unread discovered entries.
	/// </summary>
	public int UnreadCount => k_entryStates.Values.Count(e => e.IsDiscovered && !e.IsRead);

	/// <summary>
	/// Completion percentage (0-100).
	/// </summary>
	public float CompletionPercent => TotalEntries > 0 ? (float)DiscoveredCount / TotalEntries * 100 : 0;

	/// <summary>
	/// Whether there are newly discovered entries.
	/// </summary>
	public bool HasNewEntries => k_newlyDiscovered.Count > 0;

	#endregion

	#region Events

	public event Action<LoreEntry>? OnLoreDiscovered;
	public event Action<LoreEntry>? OnLoreRead;
	public event Action<LoreCategory>? OnCategoryUpdated;

	#endregion

	#region Constructor

	public LoreManager(GameDb gameDb, MetricsManager metrics) {
		k_gameDb = gameDb;
		k_metrics = metrics;

		InitializeLoreStates();
		Debug.Log($"LoreManager initialized with {TotalEntries} entries");
	}

	private void InitializeLoreStates() {
		foreach (var entry in k_gameDb.GetAll<LoreEntry>()) {
			k_entryStates[entry.Id] = new LoreEntryState {
				Entry = entry,
				IsDiscovered = entry.StartsDiscovered,
				IsRead = false
			};
		}
	}

	#endregion

	#region Discovery

	/// <summary>
	/// Discovers a lore entry.
	/// </summary>
	public bool DiscoverLore(string loreId, int? runNumber = null) {
		if (!k_entryStates.TryGetValue(loreId, out var state)) {
			Debug.LogWarning($"Lore entry not found: {loreId}");
			return false;
		}

		if (state.IsDiscovered) {
			return false; // Already discovered
		}

		// Check prerequisites
		foreach (var prereq in state.Entry.Prerequisites) {
			if (!IsDiscovered(prereq)) {
				Debug.Log($"Lore {loreId} prerequisites not met");
				return false;
			}
		}

		// Discover the entry
		state.IsDiscovered = true;
		state.DiscoveredAt = DateTime.Now;
		state.DiscoveredInRun = runNumber;

		k_newlyDiscovered.Add(loreId);
		k_metrics.Increment(MetricType.LoreEntriesDiscovered);
		k_metrics.IncrementBreakdown(MetricType.LoreEntriesDiscovered, state.Entry.Category.ToString());

		OnLoreDiscovered?.Invoke(state.Entry);
		OnCategoryUpdated?.Invoke(state.Entry.Category);

		Debug.Log($"Discovered lore: {state.Entry.Title}");

		// Check for related discoveries
		foreach (var relatedId in state.Entry.RelatedEntries) {
			// Related entries might become discoverable, but don't auto-discover
		}

		return true;
	}

	/// <summary>
	/// Discovers multiple lore entries.
	/// </summary>
	public int DiscoverLore(IEnumerable<string> loreIds, int? runNumber = null) {
		int count = 0;
		foreach (var id in loreIds) {
			if (DiscoverLore(id, runNumber)) {
				count++;
			}
		}
		return count;
	}

	/// <summary>
	/// Checks if an entry is discovered.
	/// </summary>
	public bool IsDiscovered(string loreId) {
		return k_entryStates.TryGetValue(loreId, out var state) && state.IsDiscovered;
	}

	/// <summary>
	/// Checks if an entry can be discovered (prerequisites met).
	/// </summary>
	public bool CanDiscover(string loreId) {
		if (!k_entryStates.TryGetValue(loreId, out var state)) {
			return false;
		}
		if (state.IsDiscovered) {
			return false;
		}

		foreach (var prereq in state.Entry.Prerequisites) {
			if (!IsDiscovered(prereq)) {
				return false;
			}
		}

		return true;
	}

	#endregion

	#region Reading

	/// <summary>
	/// Marks an entry as read.
	/// </summary>
	public void MarkAsRead(string loreId) {
		if (!k_entryStates.TryGetValue(loreId, out var state)) {
			return;
		}
		if (!state.IsDiscovered || state.IsRead) {
			return;
		}

		state.IsRead = true;
		k_newlyDiscovered.Remove(loreId);

		OnLoreRead?.Invoke(state.Entry);
	}

	/// <summary>
	/// Marks all discovered entries as read.
	/// </summary>
	public void MarkAllAsRead() {
		foreach (var state in k_entryStates.Values) {
			if (state.IsDiscovered && !state.IsRead) {
				state.IsRead = true;
			}
		}
		k_newlyDiscovered.Clear();
	}

	/// <summary>
	/// Checks if an entry is read.
	/// </summary>
	public bool IsRead(string loreId) {
		return k_entryStates.TryGetValue(loreId, out var state) && state.IsRead;
	}

	/// <summary>
	/// Clears the newly discovered flag for an entry.
	/// </summary>
	public void ClearNewFlag(string loreId) {
		k_newlyDiscovered.Remove(loreId);
	}

	/// <summary>
	/// Clears all newly discovered flags.
	/// </summary>
	public void ClearAllNewFlags() {
		k_newlyDiscovered.Clear();
	}

	#endregion

	#region Queries

	/// <summary>
	/// Gets a lore entry by ID.
	/// </summary>
	public LoreEntry? GetEntry(string loreId) {
		return k_entryStates.TryGetValue(loreId, out var state) ? state.Entry : null;
	}

	/// <summary>
	/// Gets the state for a lore entry.
	/// </summary>
	public LoreEntryState? GetEntryState(string loreId) {
		return k_entryStates.GetValueOrDefault(loreId);
	}

	/// <summary>
	/// Gets all entries in a category.
	/// </summary>
	public IEnumerable<LoreEntryState> GetEntriesByCategory(LoreCategory category) {
		return k_entryStates.Values
			.Where(s => s.Entry.Category == category)
			.OrderBy(s => s.Entry.SortOrder)
			.ThenBy(s => s.Entry.Title);
	}

	/// <summary>
	/// Gets all discovered entries.
	/// </summary>
	public IEnumerable<LoreEntryState> GetDiscoveredEntries() {
		return k_entryStates.Values
			.Where(s => s.IsDiscovered)
			.OrderBy(s => s.Entry.Category)
			.ThenBy(s => s.Entry.SortOrder);
	}

	/// <summary>
	/// Gets all unread entries.
	/// </summary>
	public IEnumerable<LoreEntryState> GetUnreadEntries() {
		return k_entryStates.Values
			.Where(s => s.IsDiscovered && !s.IsRead)
			.OrderByDescending(s => s.DiscoveredAt);
	}

	/// <summary>
	/// Gets newly discovered entry IDs.
	/// </summary>
	public IEnumerable<string> GetNewlyDiscoveredIds() {
		return k_newlyDiscovered;
	}

	/// <summary>
	/// Gets categories with discovered content.
	/// </summary>
	public IEnumerable<LoreCategory> GetAvailableCategories() {
		return k_entryStates.Values
			.Where(s => s.IsDiscovered)
			.Select(s => s.Entry.Category)
			.Distinct()
			.OrderBy(c => c);
	}

	/// <summary>
	/// Gets discovery stats for a category.
	/// </summary>
	public (int discovered, int total) GetCategoryStats(LoreCategory category) {
		var entries = k_entryStates.Values.Where(s => s.Entry.Category == category).ToList();
		int discovered = entries.Count(s => s.IsDiscovered);
		return (discovered, entries.Count);
	}

	/// <summary>
	/// Searches lore entries.
	/// </summary>
	public IEnumerable<LoreEntryState> SearchEntries(string query, bool discoveredOnly = true) {
		var lowerQuery = query.ToLower();

		return k_entryStates.Values
			.Where(s => (!discoveredOnly || s.IsDiscovered) &&
				(s.Entry.Title.ToLower().Contains(lowerQuery) ||
				 s.Entry.Content.ToLower().Contains(lowerQuery) ||
				 s.Entry.Tags.Any(t => t.ToLower().Contains(lowerQuery))))
			.OrderBy(s => s.Entry.Title);
	}

	/// <summary>
	/// Gets codex summary for UI.
	/// </summary>
	public CodexSummary GetCodexSummary() {
		var categories = new Dictionary<LoreCategory, (int discovered, int total, int unread)>();

		foreach (LoreCategory category in Enum.GetValues(typeof(LoreCategory))) {
			var entries = k_entryStates.Values.Where(s => s.Entry.Category == category).ToList();
			if (entries.Count > 0) {
				int discovered = entries.Count(s => s.IsDiscovered);
				int unread = entries.Count(s => s.IsDiscovered && !s.IsRead);
				categories[category] = (discovered, entries.Count, unread);
			}
		}

		return new CodexSummary {
			TotalEntries = TotalEntries,
			DiscoveredEntries = DiscoveredCount,
			UnreadEntries = UnreadCount,
			CompletionPercent = CompletionPercent,
			CategoryStats = categories,
			RecentDiscoveries = k_entryStates.Values
				.Where(s => s.IsDiscovered && s.DiscoveredAt.HasValue)
				.OrderByDescending(s => s.DiscoveredAt)
				.Take(5)
				.Select(s => s.Entry)
				.ToList()
		};
	}

	#endregion

	#region Serialization

	/// <summary>
	/// Gets save data.
	/// </summary>
	public LoreSaveData ToData() {
		return new LoreSaveData {
			DiscoveredEntries = k_entryStates.Values
				.Where(s => s.IsDiscovered)
				.Select(s => new LoreEntrySaveData {
					Id = s.Entry.Id,
					IsRead = s.IsRead,
					DiscoveredAt = s.DiscoveredAt,
					DiscoveredInRun = s.DiscoveredInRun
				})
				.ToList()
		};
	}

	/// <summary>
	/// Loads from save data.
	/// </summary>
	public void FromData(LoreSaveData data) {
		// Reset all to undiscovered
		foreach (var state in k_entryStates.Values) {
			state.IsDiscovered = state.Entry.StartsDiscovered;
			state.IsRead = false;
			state.DiscoveredAt = null;
			state.DiscoveredInRun = null;
		}

		// Apply saved discoveries
		foreach (var savedEntry in data.DiscoveredEntries) {
			if (k_entryStates.TryGetValue(savedEntry.Id, out var state)) {
				state.IsDiscovered = true;
				state.IsRead = savedEntry.IsRead;
				state.DiscoveredAt = savedEntry.DiscoveredAt;
				state.DiscoveredInRun = savedEntry.DiscoveredInRun;
			}
		}

		k_newlyDiscovered.Clear();
	}

	/// <summary>
	/// Applies discovered lore from meta progression.
	/// </summary>
	public void ApplyMetaDiscoveries(HashSet<string> discoveredLore) {
		foreach (var loreId in discoveredLore) {
			if (k_entryStates.TryGetValue(loreId, out var state)) {
				state.IsDiscovered = true;
			}
		}
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// Summary of codex state for UI.
/// </summary>
public class CodexSummary {
	public int TotalEntries { get; set; }
	public int DiscoveredEntries { get; set; }
	public int UnreadEntries { get; set; }
	public float CompletionPercent { get; set; }
	public Dictionary<LoreCategory, (int discovered, int total, int unread)> CategoryStats { get; set; } = [];
	public List<LoreEntry> RecentDiscoveries { get; set; } = [];
}

/// <summary>
/// Save data for lore.
/// </summary>
public class LoreSaveData {
	public List<LoreEntrySaveData> DiscoveredEntries { get; set; } = [];
}

/// <summary>
/// Save data for a single lore entry.
/// </summary>
public class LoreEntrySaveData {
	public string Id { get; set; } = "";
	public bool IsRead { get; set; }
	public DateTime? DiscoveredAt { get; set; }
	public int? DiscoveredInRun { get; set; }
}

#endregion