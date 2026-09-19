namespace RPGGame.UI.Components;

/// <summary>
/// Controls how much detail is shown in entity/item detail panels.
/// </summary>
public enum EntityViewMode {
	/// <summary>Minimal info for HUD, tooltips, quick glances.</summary>
	Compact,

	/// <summary>Medium detail for sidebars, quick inspection.</summary>
	Summary,

	/// <summary>Complete info for dedicated windows/screens.</summary>
	Full
}

/// <summary>
/// Controls whether and how detail panels can be expanded.
/// </summary>
public enum ExpandMode {
	/// <summary>No expansion allowed (fixed display, tooltips).</summary>
	None,

	/// <summary>Expands within the same panel with a down arrow indicator.</summary>
	InPlace,

	/// <summary>Shows "Open Details" button that opens a separate window.</summary>
	NewWindow
}