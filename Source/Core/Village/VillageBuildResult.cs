namespace RPGGame.Core.Village;

/// <summary>
/// Result of a building upgrade operation.
/// </summary>
public class VillageBuildResult {
	/// <summary>
	/// Whether the upgrade succeeded.
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// Result message.
	/// </summary>
	public string Message { get; set; } = "";

	/// <summary>
	/// New level after upgrade.
	/// </summary>
	public int NewLevel { get; set; }

	/// <summary>
	/// Cost paid for the upgrade.
	/// </summary>
	public int CostPaid { get; set; }

	/// <summary>
	/// The building that was upgraded.
	/// </summary>
	public Building? Building { get; set; }
}