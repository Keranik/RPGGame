using System;
using System.Collections.Generic;
using System.Text;

namespace RPGGame.Core.Combat;

/// <summary>
/// AI behavior types.
/// </summary>
public enum EnemyAIType {
	/// <summary>Attacks nearest target.</summary>
	Aggressive,
	/// <summary>Attacks weakest target.</summary>
	Opportunistic,
	/// <summary>Prioritizes healing/buffing allies.</summary>
	Support,
	/// <summary>Stays at range, uses abilities.</summary>
	Ranged,
	/// <summary>Defends until provoked.</summary>
	Defensive,
	/// <summary>Uses abilities strategically.</summary>
	Tactical,
	/// <summary>Flees when low health.</summary>
	Cowardly,
	/// <summary>Boss behavior - uses phases.</summary>
	Boss
}
