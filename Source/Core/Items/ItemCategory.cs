namespace RPGGame.Core.Items;

/// <summary>
/// Categories of items.
/// </summary>
public enum ItemCategory {
	/// <summary>Equippable weapons.</summary>
	Weapon,
	/// <summary>Equippable armor.</summary>
	Armor,
	/// <summary>Equippable accessories.</summary>
	Accessory,
	/// <summary>Consumable items.</summary>
	Consumable,
	/// <summary>Crafting materials.</summary>
	Material,
	/// <summary>Quest items.</summary>
	Quest,
	/// <summary>Key items (special, non-consumable).</summary>
	Key,
	/// <summary>Currency.</summary>
	Currency,
	/// <summary>Ammunition.</summary>
	Ammunition,
	/// <summary>Miscellaneous.</summary>
	Misc
}

/// <summary>
/// Item usage context.
/// </summary>
public enum ItemUseContext {
	/// <summary>Can be used anytime.</summary>
	Anytime,
	/// <summary>Only usable in combat.</summary>
	CombatOnly,
	/// <summary>Only usable outside combat.</summary>
	NonCombat,
	/// <summary>Only usable while camping.</summary>
	CampOnly,
	/// <summary>Cannot be used (passive/equip only).</summary>
	Passive
}