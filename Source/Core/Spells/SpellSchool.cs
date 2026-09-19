namespace RPGGame.Core.Spells;

/// <summary>
/// Schools of magic.
/// </summary>
public enum SpellSchool {
	/// <summary>No school (non-magical).</summary>
	None,

	// ═══════════════════════════════════════════════════════════════
	// ARCANE SCHOOLS
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Evocation - direct damage and energy manipulation.</summary>
	Evocation,

	/// <summary>Abjuration - protection and wards.</summary>
	Abjuration,

	/// <summary>Conjuration - summoning and creation.</summary>
	Conjuration,

	/// <summary>Transmutation - transformation and alteration.</summary>
	Transmutation,

	/// <summary>Divination - knowledge and foresight.</summary>
	Divination,

	/// <summary>Enchantment - mind-affecting magic.</summary>
	Enchantment,

	/// <summary>Illusion - deception and misdirection.</summary>
	Illusion,

	/// <summary>Necromancy - death and undeath.</summary>
	Necromancy,

	// ═══════════════════════════════════════════════════════════════
	// DIVINE SCHOOLS
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Holy - divine light and healing.</summary>
	Holy,

	/// <summary>Nature - druidic and natural magic.</summary>
	Nature,

	// ═══════════════════════════════════════════════════════════════
	// SPECIAL
	// ═══════════════════════════════════════════════════════════════

	/// <summary>Temporal - time manipulation (rare).</summary>
	Temporal
}
