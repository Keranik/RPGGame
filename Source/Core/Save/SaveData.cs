using RPGGame.Core.Items;
using RPGGame.Core.Metrics;
using RPGGame.Core.Simulation;
using RPGGame.Core.Village;

namespace RPGGame.Core.Save;

/// <summary>
/// Complete save data structure containing all game state.
/// Uses the serialization data types from each subsystem.
/// </summary>
public class SaveData {
	#region Metadata

	/// <summary>
	/// Save format version for compatibility.
	/// </summary>
	public int Version { get; set; } = SaveManager.CURRENT_SAVE_VERSION;

	/// <summary>
	/// When this save was created.
	/// </summary>
	public DateTime Timestamp { get; set; } = DateTime.Now;

	/// <summary>
	/// Total playtime in seconds.
	/// </summary>
	public double PlaytimeSeconds { get; set; }

	/// <summary>
	/// Checksum for integrity verification.
	/// </summary>
	public string? Checksum { get; set; }

	#endregion

	#region Core State

	/// <summary>
	/// Current game phase.
	/// </summary>
	public GamePhase GamePhase { get; set; }

	/// <summary>
	/// Meta-progression data (persists across runs).
	/// Uses MetaProgression.MetaProgressionData structure.
	/// </summary>
	public MetaProgressionData? MetaProgression { get; set; }

	/// <summary>
	/// Current run state (null if not in a run).
	/// Uses RunState.RunStateData structure.
	/// </summary>
	public RunStateData? RunState { get; set; }

	#endregion

	#region Character

	/// <summary>
	/// Character data for save slot display.
	/// </summary>
	public CharacterSaveData? Character { get; set; }

	/// <summary>
	/// Inventory data.
	/// Uses InventoryManager.InventorySaveData structure.
	/// </summary>
	public InventorySaveData? Inventory { get; set; }

	#endregion

	#region Village

	/// <summary>
	/// Village layout data.
	/// Uses VillageLayout.VillageLayoutSaveData structure.
	/// </summary>
	public VillageLayoutSaveData? Village { get; set; }

	#endregion

	#region Metrics

	/// <summary>
	/// All-time metrics.
	/// Uses MetricsManager.MetricsData structure.
	/// </summary>
	public MetricsData? Metrics { get; set; }

	#endregion
}

#region Character Save Data

/// <summary>
/// Lightweight character data for save slot display.
/// </summary>
public class CharacterSaveData {
	public string Name { get; set; } = "";
	public string ClassId { get; set; } = "";
	public int Level { get; set; }
	public string PortraitName { get; set; } = "";
}

#endregion