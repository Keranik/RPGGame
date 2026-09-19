using RPGGame.Core.Characters;

namespace RPGGame.Core.Combat;

/// <summary>
/// Represents an action taken in combat.
/// </summary>
public class CombatAction {
	#region Properties

	public CombatActionType Type { get; init; }
	public LiveCharacter Actor { get; init; } = null!;
	public LiveCharacter? Target { get; init; }
	public List<LiveCharacter> Targets { get; init; } = [];
	public CombatAbility? Ability { get; init; }
	public string? ItemId { get; init; }
	public int? TargetPosition { get; init; }

	#endregion

	#region Factory Methods

	public static CombatAction Attack(LiveCharacter actor, LiveCharacter target) => new() {
		Type = CombatActionType.Attack,
		Actor = actor,
		Target = target
	};

	public static CombatAction HeavyAttack(LiveCharacter actor, LiveCharacter target) => new() {
		Type = CombatActionType.HeavyAttack,
		Actor = actor,
		Target = target
	};

	public static CombatAction QuickAttack(LiveCharacter actor, LiveCharacter target) => new() {
		Type = CombatActionType.QuickAttack,
		Actor = actor,
		Target = target
	};

	public static CombatAction Defend(LiveCharacter actor) => new() {
		Type = CombatActionType.Defend,
		Actor = actor
	};

	public static CombatAction UseAbility(LiveCharacter actor, CombatAbility ability, LiveCharacter? target = null, List<LiveCharacter>? targets = null) => new() {
		Type = CombatActionType.UseAbility,
		Actor = actor,
		Ability = ability,
		Target = target,
		Targets = targets ?? []
	};

	public static CombatAction CastSpell(LiveCharacter actor, CombatAbility spell, LiveCharacter? target = null, List<LiveCharacter>? targets = null) => new() {
		Type = CombatActionType.CastSpell,
		Actor = actor,
		Ability = spell,
		Target = target,
		Targets = targets ?? []
	};

	public static CombatAction UseItem(LiveCharacter actor, string itemId, LiveCharacter? target = null) => new() {
		Type = CombatActionType.UseItem,
		Actor = actor,
		ItemId = itemId,
		Target = target
	};

	public static CombatAction Flee(LiveCharacter actor) => new() {
		Type = CombatActionType.Flee,
		Actor = actor
	};

	public static CombatAction Wait(LiveCharacter actor) => new() {
		Type = CombatActionType.Wait,
		Actor = actor
	};

	#endregion

	#region Validation

	/// <summary>Validates using CharacterManager for stat checks.</summary>
	public bool IsValid(CharacterManager charManager) {
		if (Actor == null || !charManager.IsAlive(Actor)) return false;
		if (charManager.IsIncapacitated(Actor)) return false;

		switch (Type) {
			case CombatActionType.Attack:
			case CombatActionType.HeavyAttack:
			case CombatActionType.QuickAttack:
				return Target != null && charManager.IsAlive(Target);

			case CombatActionType.UseAbility:
			case CombatActionType.CastSpell:
				if (Ability == null) return false;
				if (Ability.ManaCost > charManager.GetStat(Actor, Ids.Stats.Resource.CurrentMana)) return false;
				if (Ability.CurrentCooldown > 0) return false;
				return true;

			case CombatActionType.UseItem:
				return !string.IsNullOrEmpty(ItemId);

			case CombatActionType.Defend:
			case CombatActionType.Wait:
			case CombatActionType.Flee:
				return true;

			default:
				return true;
		}
	}

	#endregion

	#region Display

	public string GetDescription() {
		return Type switch {
			CombatActionType.Attack => $"{Actor.Name} attacks {Target?.Name}",
			CombatActionType.HeavyAttack => $"{Actor.Name} heavy attacks {Target?.Name}",
			CombatActionType.QuickAttack => $"{Actor.Name} quick attacks {Target?.Name}",
			CombatActionType.Defend => $"{Actor.Name} defends",
			CombatActionType.UseAbility => $"{Actor.Name} uses {Ability?.Name}",
			CombatActionType.CastSpell => $"{Actor.Name} casts {Ability?.Name}",
			CombatActionType.UseItem => $"{Actor.Name} uses item",
			CombatActionType.Flee => $"{Actor.Name} attempts to flee",
			CombatActionType.Wait => $"{Actor.Name} waits",
			_ => $"{Actor.Name} acts"
		};
	}

	#endregion
}

/// <summary>
/// Result of executing a combat action.
/// </summary>
public class CombatActionResult {
	public CombatAction Action { get; set; } = null!;
	public bool Success { get; set; }
	public AttackRollResult? AttackRoll { get; set; }
	public List<DamageResult> DamageDealt { get; set; } = [];
	public float HealingDone { get; set; }
	public List<(LiveCharacter target, StatusCondition condition)> EffectsApplied { get; set; } = [];
	public string Message { get; set; } = "";
	public List<string> AdditionalMessages { get; set; } = [];
	public List<LiveCharacter> Defeated { get; set; } = [];
	public bool FleeSuccessful { get; set; }
}

/// <summary>
/// Result of an attack roll.
/// </summary>
public class AttackRollResult {
	public int NaturalRoll { get; set; }
	public int Modifier { get; set; }
	public int Total { get; set; }
	public int TargetAC { get; set; }
	public AttackResult Result { get; set; }

	public bool IsHit => Result is AttackResult.Hit or AttackResult.CriticalHit;
	public bool IsCritical => Result == AttackResult.CriticalHit;
	public bool IsCriticalMiss => Result == AttackResult.CriticalMiss;

	public string GetDisplayString() {
		string resultStr = Result switch {
			AttackResult.CriticalHit => "CRITICAL HIT!",
			AttackResult.CriticalMiss => "CRITICAL MISS!",
			AttackResult.Hit => "Hit!",
			AttackResult.Miss => "Miss",
			AttackResult.Blocked => "Blocked",
			AttackResult.Dodged => "Dodged",
			AttackResult.Parried => "Parried",
			_ => Result.ToString()
		};
		return $"Roll: {NaturalRoll} + {Modifier} = {Total} vs AC {TargetAC} - {resultStr}";
	}
}