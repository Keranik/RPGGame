namespace RPGGame.Core.Village;

/// <summary>
/// Categories for organizing buildings.
/// </summary>
public enum BuildingCategory {
	/// <summary>Core structures that cannot be moved or demolished (Anchor, Gate).</summary>
	Core,

	/// <summary>Commerce and trading buildings.</summary>
	Commerce,

	/// <summary>Combat and training buildings.</summary>
	Combat,

	/// <summary>Training and skill development buildings.</summary>
	Training,

	/// <summary>Defensive structures.</summary>
	Defense,

	/// <summary>Magic and knowledge buildings.</summary>
	Magic,

	/// <summary>Crafting and production buildings.</summary>
	Production,

	/// <summary>Survival and resource buildings.</summary>
	Survival,

	/// <summary>Utility and support buildings.</summary>
	Utility,

	/// <summary>Special unlockable buildings.</summary>
	Special
}
