namespace RPGGame.Core.Save;

/// <summary>
/// Metadata about a save slot for display in save/load UI.
/// </summary>
public class SaveSlotInfo {
	#region Properties

	/// <summary>
	/// Slot index (0-based).
	/// </summary>
	public int SlotIndex { get; set; }

	/// <summary>
	/// Whether this slot has save data.
	/// </summary>
	public bool HasData { get; set; }

	/// <summary>
	/// Character name.
	/// </summary>
	public string CharacterName { get; set; } = "";

	/// <summary>
	/// Character class.
	/// </summary>
	public string CharacterClass { get; set; } = "";

	/// <summary>
	/// Character level.
	/// </summary>
	public int Level { get; set; }

	/// <summary>
	/// Current run number.
	/// </summary>
	public int RunNumber { get; set; }

	/// <summary>
	/// Current day in run.
	/// </summary>
	public int CurrentDay { get; set; }

	/// <summary>
	/// Total playtime in seconds.
	/// </summary>
	public double PlaytimeSeconds { get; set; }

	/// <summary>
	/// Playtime as formatted string.
	/// </summary>
	public string PlaytimeFormatted {
		get {
			var ts = TimeSpan.FromSeconds(PlaytimeSeconds);
			return ts.TotalHours >= 1
				? $"{(int)ts.TotalHours}h {ts.Minutes}m"
				: $"{ts.Minutes}m {ts.Seconds}s";
		}
	}

	/// <summary>
	/// When this save was created.
	/// </summary>
	public DateTime SaveTimestamp { get; set; }

	/// <summary>
	/// Save timestamp as relative string.
	/// </summary>
	public string SaveTimestampFormatted {
		get {
			var diff = DateTime.Now - SaveTimestamp;

			if (diff.TotalMinutes < 1) {
				return "Just now";
			}
			if (diff.TotalMinutes < 60) {
				return $"{(int)diff.TotalMinutes}m ago";
			}
			if (diff.TotalHours < 24) {
				return $"{(int)diff.TotalHours}h ago";
			}
			if (diff.TotalDays < 7) {
				return $"{(int)diff.TotalDays}d ago";
			}

			return SaveTimestamp.ToString("MMM d, yyyy");
		}
	}

	/// <summary>
	/// Current game phase.
	/// </summary>
	public Simulation.GamePhase GamePhase { get; set; }

	/// <summary>
	/// Fog clears achieved.
	/// </summary>
	public int FogClears { get; set; }

	/// <summary>
	/// Whether true ending achieved.
	/// </summary>
	public bool TrueEndingAchieved { get; set; }

	/// <summary>
	/// Save version for compatibility checking.
	/// </summary>
	public int SaveVersion { get; set; }

	/// <summary>
	/// Screenshot thumbnail (base64 encoded, optional).
	/// </summary>
	public string? ThumbnailBase64 { get; set; }

	/// <summary>
	/// Whether this is an auto-save slot.
	/// </summary>
	public bool IsAutoSave { get; set; }

	/// <summary>
	/// Custom slot name (user-defined).
	/// </summary>
	public string? CustomName { get; set; }

	/// <summary>
	/// Display name for the slot.
	/// </summary>
	public string DisplayName => CustomName ?? (IsAutoSave ? "Auto-Save" : $"Slot {SlotIndex + 1}");

	#endregion

	#region Methods

	/// <summary>
	/// Creates empty slot info.
	/// </summary>
	public static SaveSlotInfo Empty(int slotIndex) => new() {
		SlotIndex = slotIndex,
		HasData = false
	};

	/// <summary>
	/// Gets a summary string for UI.
	/// </summary>
	public string GetSummary() {
		if (!HasData) {
			return "Empty Slot";
		}

		return $"{CharacterName} - Lv{Level} {CharacterClass}\n" +
			   $"Run {RunNumber}, Day {CurrentDay}\n" +
			   $"{PlaytimeFormatted} | {SaveTimestampFormatted}";
	}

	#endregion
}