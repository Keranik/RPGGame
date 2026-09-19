namespace RPGGame.Core.Spells;

/// <summary>
/// Targeting type for spells.
/// </summary>
public enum SpellTargetType {
	/// <summary>Targets self only.</summary>
	Self,
	/// <summary>Targets a single ally.</summary>
	SingleAlly,
	/// <summary>Targets a single enemy.</summary>
	SingleEnemy,
	/// <summary>Targets any single creature.</summary>
	SingleAny,
	/// <summary>Targets all allies.</summary>
	AllAllies,
	/// <summary>Targets all enemies.</summary>
	AllEnemies,
	/// <summary>Targets all creatures.</summary>
	All,
	/// <summary>Area effect at a point.</summary>
	AreaPoint,
	/// <summary>Cone effect.</summary>
	Cone,
	/// <summary>Line effect.</summary>
	Line
}
