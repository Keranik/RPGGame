namespace RPGGame.Core.Characters;

/// <summary>
/// Spellcasting type for a class.
/// </summary>
public enum SpellcastingType {
	/// <summary>Not a spellcaster.</summary>
	None,
	/// <summary>Arcane magic (INT-based).</summary>
	Arcane,
	/// <summary>Divine magic (WIS-based).</summary>
	Divine,
	/// <summary>Nature magic (WIS-based).</summary>
	Nature,
	/// <summary>Pact magic (CHA-based).</summary>
	Pact,
	/// <summary>Ki/Inner power (WIS-based).</summary>
	Ki
}
