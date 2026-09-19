namespace RPGGame.Core.Combat;

/// <summary>
/// Types of combatants in battle.
/// </summary>
public enum CombatantType {
	/// <summary>The player character.</summary>
	Player,
	/// <summary>Player's ally/companion.</summary>
	Ally,
	/// <summary>Neutral party (can be swayed).</summary>
	Neutral,
	/// <summary>Enemy combatant.</summary>
	Enemy,
	/// <summary>Boss enemy.</summary>
	Boss,
	/// <summary>Summon/pet controlled by player.</summary>
	Summon
}

/// <summary>
/// Combat action types.
/// </summary>
public enum CombatActionType {
	/// <summary>Basic weapon attack.</summary>
	Attack,
	/// <summary>Heavy attack (higher damage, lower accuracy).</summary>
	HeavyAttack,
	/// <summary>Quick attack (lower damage, higher accuracy).</summary>
	QuickAttack,
	/// <summary>Defensive stance.</summary>
	Defend,
	/// <summary>Use an ability/skill.</summary>
	UseAbility,
	/// <summary>Cast a spell.</summary>
	CastSpell,
	/// <summary>Use an item.</summary>
	UseItem,
	/// <summary>Attempt to flee.</summary>
	Flee,
	/// <summary>Skip turn / wait.</summary>
	Wait,
	/// <summary>Move position.</summary>
	Move,
	/// <summary>Interact with environment.</summary>
	Interact
}

/// <summary>
/// Result of an attack roll.
/// </summary>
public enum AttackResult {
	/// <summary>Attack missed.</summary>
	Miss,
	/// <summary>Attack hit normally.</summary>
	Hit,
	/// <summary>Critical hit (nat 20 or crit range).</summary>
	CriticalHit,
	/// <summary>Critical miss (nat 1).</summary>
	CriticalMiss,
	/// <summary>Attack was blocked.</summary>
	Blocked,
	/// <summary>Attack was dodged.</summary>
	Dodged,
	/// <summary>Attack was parried.</summary>
	Parried
}

/// <summary>
/// Current phase of combat.
/// </summary>
public enum CombatPhase {
	/// <summary>Not in combat.</summary>
	None,
	/// <summary>Combat is starting, roll initiative.</summary>
	Initiative,
	/// <summary>Player's turn to act.</summary>
	PlayerTurn,
	/// <summary>Enemy's turn to act.</summary>
	EnemyTurn,
	/// <summary>Ally's turn to act.</summary>
	AllyTurn,
	/// <summary>Resolving an action.</summary>
	ResolvingAction,
	/// <summary>Showing action results.</summary>
	ShowingResults,
	/// <summary>Combat ended in victory.</summary>
	Victory,
	/// <summary>Combat ended in defeat.</summary>
	Defeat,
	/// <summary>Combat ended by fleeing.</summary>
	Fled
}


public static class StatusConditionExtensions {
	/// <summary>
	/// Gets whether this is a negative condition.
	/// </summary>
	public static bool IsDebuff(this StatusCondition condition) {
		return condition switch {
			StatusCondition.None => false,
			StatusCondition.Shielded => false,
			StatusCondition.Regenerating => false,
			StatusCondition.Empowered => false,
			StatusCondition.Hasted => false,
			StatusCondition.Invisible => false,
			StatusCondition.Undying => false,
			_ => true
		};
	}

	/// <summary>
	/// Gets the icon name for this condition.
	/// </summary>
	public static string GetIconName(this StatusCondition condition) {
		return $"icon_status_{condition.ToString().ToLower()}";
	}

	/// <summary>
	/// Gets display color for this condition.
	/// </summary>
	public static UnityEngine.Color GetColor(this StatusCondition condition) {
		return condition switch {
			StatusCondition.Poisoned => new UnityEngine.Color(0.5f, 0.8f, 0.2f),
			StatusCondition.Burning => new UnityEngine.Color(1f, 0.5f, 0.2f),
			StatusCondition.Bleeding => new UnityEngine.Color(0.8f, 0.2f, 0.2f),
			StatusCondition.Frozen => new UnityEngine.Color(0.5f, 0.8f, 1f),
			StatusCondition.Stunned => new UnityEngine.Color(1f, 1f, 0.3f),
			StatusCondition.Shielded => new UnityEngine.Color(0.3f, 0.6f, 1f),
			StatusCondition.Regenerating => new UnityEngine.Color(0.3f, 1f, 0.5f),
			StatusCondition.Empowered => new UnityEngine.Color(1f, 0.8f, 0.3f),
			StatusCondition.Hasted => new UnityEngine.Color(0.8f, 0.8f, 1f),
			_ => UnityEngine.Color.white
		};
	}
}