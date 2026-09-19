namespace RPGGame.Core.Combat;

/// <summary>
/// Result of dealing damage to a character.
/// </summary>
public class DamageResult {
	/// <summary>Raw damage before any modifiers.</summary>
	public float RawDamage { get; set; }

	/// <summary>Final damage after resistances/vulnerabilities/etc.</summary>
	public float FinalDamage { get; set; }

	/// <summary>Type of damage dealt.</summary>
	public DamageType DamageType { get; set; }

	/// <summary>Name of the source character (if any).</summary>
	public string? SourceName { get; set; }

	/// <summary>Name of the target character.</summary>
	public string? TargetName { get; set; }

	/// <summary>Target's health after damage.</summary>
	public float ResultingHealth { get; set; }

	/// <summary>Whether this damage killed the target.</summary>
	public bool WasLethal { get; set; }

	/// <summary>Whether resistance reduced damage.</summary>
	public bool WasResisted { get; set; }

	/// <summary>Amount of resistance applied (0-1).</summary>
	public float ResistanceApplied { get; set; }

	/// <summary>Amount of vulnerability applied (0+).</summary>
	public float VulnerabilityApplied { get; set; }

	/// <summary>Whether the target was defending.</summary>
	public bool WasDefending { get; set; }

	/// <summary>Whether the target had a shield effect.</summary>
	public bool WasShielded { get; set; }

	/// <summary>Damage reduction percentage from all sources.</summary>
	public float TotalReduction => 1f - (RawDamage > 0 ? FinalDamage / RawDamage : 0);
}
