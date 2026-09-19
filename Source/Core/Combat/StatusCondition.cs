
namespace RPGGame.Core.Combat;

/// <summary>
/// Status conditions that can affect combatants.
/// </summary>
public enum StatusCondition {
	/// <summary>No special condition.</summary>
	None,
	/// <summary>Taking damage over time.</summary>
	Poisoned,
	/// <summary>Taking damage over time, can spread.</summary>
	Burning,
	/// <summary>Taking damage over time from wounds.</summary>
	Bleeding,
	/// <summary>Cannot move.</summary>
	Rooted,
	/// <summary>Cannot act.</summary>
	Stunned,
	/// <summary>Miss chance increased.</summary>
	Blinded,
	/// <summary>Auto-attacks target this unit.</summary>
	Charmed,
	/// <summary>Reduced accuracy and damage.</summary>
	Weakened,
	/// <summary>Movement speed reduced.</summary>
	Slowed,
	/// <summary>Cannot use abilities.</summary>
	Silenced,
	/// <summary>Attacks have disadvantage.</summary>
	Frightened,
	/// <summary>Must attack nearest target.</summary>
	Confused,
	/// <summary>Cannot be targeted.</summary>
	Invisible,
	/// <summary>Taking reduced damage.</summary>
	Shielded,
	/// <summary>Regenerating health.</summary>
	Regenerating,
	/// <summary>Increased damage.</summary>
	Empowered,
	/// <summary>Increased speed.</summary>
	Hasted,
	/// <summary>Skips next turn.</summary>
	Frozen,
	/// <summary>Cannot die (1 HP minimum).</summary>
	Undying,
	/// <summary>Negative effects amplified.</summary>
	Cursed,
	/// <summary>Positive effects amplified.</summary>
	Blessed
}