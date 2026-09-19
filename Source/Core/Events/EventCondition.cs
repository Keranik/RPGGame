using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Events;

/// <summary>
/// Types of conditions that can be checked.
/// </summary>
public enum ConditionType {
	Always,
	Never,
	MinStat,
	MaxStat,
	HasItem,
	MinGold,
	IsClass,
	MinLevel,
	TimeOfDay,
	TerrainType,
	EventCompleted,
	EventNotCompleted,
	FogClears,
	RandomChance,
	SkillCheck,
	HasFlag,
	HasResource,
	MinDistance,
	DayRange
}

/// <summary>
/// A condition that must be met for an event or choice to be available.
/// </summary>
public class EventCondition {
	#region Properties

	/// <summary>
	/// Type of condition.
	/// </summary>
	public ConditionType Type { get; init; } = ConditionType.Always;

	/// <summary>
	/// Primary parameter (stat ID, item ID, etc.).
	/// </summary>
	public string Parameter { get; init; } = "";

	/// <summary>
	/// Numeric value for comparison.
	/// </summary>
	public float Value { get; init; }

	/// <summary>
	/// Secondary value for ranges.
	/// </summary>
	public float Value2 { get; init; }

	/// <summary>
	/// Whether this condition is inverted (NOT).
	/// </summary>
	public bool Inverted { get; init; }

	/// <summary>
	/// Optional display text when condition fails.
	/// </summary>
	public string? FailureText { get; init; }

	/// <summary>
	/// Optional display text when condition succeeds.
	/// </summary>
	public string? SuccessText { get; init; }

	/// <summary>
	/// Whether to show this condition in UI.
	/// </summary>
	public bool ShowInUI { get; init; } = true;

	#endregion

	#region Factory Methods

	public static EventCondition Always() => new() { Type = ConditionType.Always };
	public static EventCondition Never() => new() { Type = ConditionType.Never };

	public static EventCondition MinStat(StatProto.ID stat, float minValue, string? failText = null) => new() {
		Type = ConditionType.MinStat,
		Parameter = stat.Value,
		Value = minValue,
		FailureText = failText ?? $"Requires {((Proto.ID)stat).GetName()} {minValue}+"
	};

	public static EventCondition HasItem(string itemId, int count = 1) => new() {
		Type = ConditionType.HasItem,
		Parameter = itemId,
		Value = count,
		FailureText = $"Requires {itemId}"
	};

	public static EventCondition MinGold(int amount) => new() {
		Type = ConditionType.MinGold,
		Value = amount,
		FailureText = $"Requires {amount} gold"
	};

	public static EventCondition IsClass(string classId) => new() {
		Type = ConditionType.IsClass,
		Parameter = classId,
		FailureText = $"Requires {classId} class"
	};

	public static EventCondition MinLevel(int level) => new() {
		Type = ConditionType.MinLevel,
		Value = level,
		FailureText = $"Requires level {level}+"
	};

	public static EventCondition RequiresTimeOfDay(TimeOfDayPeriod timeOfDay) => new() {
		Type = ConditionType.TimeOfDay,
		Parameter = timeOfDay.ToString(),
		FailureText = $"Only available during {timeOfDay}"
	};

	public static EventCondition RequiresTerrain(TerrainProto.ID terrain) => new() {
		Type = ConditionType.TerrainType,
		Parameter = terrain.ToString(),
		ShowInUI = false
	};

	public static EventCondition EventCompleted(string eventId) => new() {
		Type = ConditionType.EventCompleted,
		Parameter = eventId,
		ShowInUI = false
	};

	public static EventCondition EventNotCompleted(string eventId) => new() {
		Type = ConditionType.EventNotCompleted,
		Parameter = eventId,
		ShowInUI = false
	};

	public static EventCondition MinFogClears(int count) => new() {
		Type = ConditionType.FogClears,
		Value = count,
		FailureText = $"Requires {count} fog clear(s)"
	};

	public static EventCondition RandomChance(float percent) => new() {
		Type = ConditionType.RandomChance,
		Value = percent,
		ShowInUI = false
	};

	/// <summary>
	/// Creates a skill check condition that can test against any stat, skill, or other proto.
	/// </summary>
	public static EventCondition SkillCheck(Proto.ID checkTarget, int dc, string? successText = null, string? failText = null) => new() {
		Type = ConditionType.SkillCheck,
		Parameter = checkTarget.Value,
		Value = dc,
		SuccessText = successText ?? $"[{checkTarget.GetName()} DC {dc}]",
		FailureText = failText
	};

	/// <summary>
	/// Creates a skill check condition using a StatProto.ID.
	/// </summary>
	public static EventCondition SkillCheck(StatProto.ID stat, int dc, string? successText = null, string? failText = null) =>
		SkillCheck((Proto.ID)stat, dc, successText, failText);

	public static EventCondition HasFlag(string flagId) => new() {
		Type = ConditionType.HasFlag,
		Parameter = flagId,
		ShowInUI = false
	};

	public static EventCondition MinDistance(float distance) => new() {
		Type = ConditionType.MinDistance,
		Value = distance,
		ShowInUI = false
	};

	public static EventCondition DayRange(int minDay, int maxDay) => new() {
		Type = ConditionType.DayRange,
		Value = minDay,
		Value2 = maxDay,
		ShowInUI = false
	};

	#endregion

	#region Evaluation

	/// <summary>
	/// Evaluates this condition against the current game state.
	/// </summary>
	public ConditionResult Evaluate(RunState runState, MetaProgression meta) {
		bool passed = EvaluateInternal(runState, meta);
		if (Inverted) {
			passed = !passed;
		}

		return new ConditionResult {
			Passed = passed,
			Condition = this,
			DisplayText = passed ? SuccessText : FailureText
		};
	}

	private bool EvaluateInternal(RunState runState, MetaProgression meta) {

		return Type switch {
			ConditionType.Always => true,
			ConditionType.Never => false,

			ConditionType.MinStat => EvaluateMinStat(runState),
			ConditionType.MaxStat => EvaluateMaxStat(runState),
			ConditionType.HasItem => EvaluateHasItem(runState),
			ConditionType.MinGold => runState.Character.BaseStats.GetInt(Ids.Stats.Expedition.GoldOnHand) >= Value,
			ConditionType.IsClass => runState.CharacterClassId == Parameter,
			ConditionType.MinLevel => runState.Level >= Value,
			ConditionType.TimeOfDay => runState.TimeOfDay.ToString() == Parameter,
			ConditionType.TerrainType => true, // Checked by event placer
			ConditionType.EventCompleted => runState.EncounteredEvents.Contains(Parameter),
			ConditionType.EventNotCompleted => !runState.EncounteredEvents.Contains(Parameter),
			ConditionType.FogClears => meta.FogClears >= Value,
			ConditionType.RandomChance => UnityEngine.Random.value * 100 <= Value,
			ConditionType.SkillCheck => EvaluateSkillCheck(runState),
			ConditionType.HasFlag => runState.ActiveConditions.Contains(Parameter),
			ConditionType.HasResource => EvaluateHasResource(runState),
			ConditionType.MinDistance => runState.DistanceFromVillage >= Value,
			ConditionType.DayRange => GameTime.Instance.Day >= Value && GameTime.Instance.Day <= Value2,

			_ => true
		};
	}

	private bool EvaluateMinStat(RunState runState) {
		var statId = new StatProto.ID(Parameter);
		return runState.Stats.Get(statId) >= Value;
	}

	private bool EvaluateMaxStat(RunState runState) {
		var statId = new StatProto.ID(Parameter);
		return runState.Stats.Get(statId) <= Value;
	}

	private bool EvaluateHasItem(RunState runState) {
		// TODO: Implement inventory check
		return false;
	}

	private bool EvaluateHasResource(RunState runState) {
		var liveCharacter = runState.Character;
		return Parameter.ToLower() switch {
			"food" => liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.FoodOnHand) >= Value,
			"medical" => liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.MedicalSupplies) >= Value,
			"camping" => liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.CampingSupplies) >= Value,
			"torches" => liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.Torches) >= Value,
			"water" => liveCharacter.BaseStats.GetInt(Ids.Stats.Expedition.Water) >= Value,
			_ => false
		};
	}

	private bool EvaluateSkillCheck(RunState runState) {
		var protoId = new Proto.ID(Parameter);

		// Determine modifier based on type
		int modifier;
		if (protoId.IsPrimaryAttribute()) {
			// For attributes, use D&D-style modifier
			var statId = new StatProto.ID(Parameter);
			modifier = StatManager.GetAttributeModifier(runState.Stats.Get(statId));
		} else if (protoId.IsStat()) {
			// For other stats, use the raw value
			var statId = new StatProto.ID(Parameter);
			modifier = runState.Stats.GetInt(statId);
		} else if (protoId.IsSkill()) {
			// For skills, get skill rank (would need SkillManager integration)
			// For now, default to 0
			modifier = 0; // TODO: Get skill rank from character
		} else {
			// Unknown type, default to 0
			modifier = 0;
		}

		int roll = UnityEngine.Random.Range(1, 21);
		int total = roll + modifier;

		return total >= Value;
	}

	#endregion

	#region Display

	/// <summary>
	/// Gets display text for this condition.
	/// </summary>
	public string GetDisplayText(RunState runState) {
		return Type switch {
			ConditionType.MinStat => $"{GetTargetDisplayName()} {Value}+",
			ConditionType.HasItem => $"Requires: {Parameter}",
			ConditionType.MinGold => $"{Value} gold",
			ConditionType.IsClass => $"{Parameter} only",
			ConditionType.MinLevel => $"Level {Value}+",
			ConditionType.SkillCheck => GetSkillCheckText(runState),
			_ => SuccessText ?? ""
		};
	}

	private string GetTargetDisplayName() {
		var protoId = new Proto.ID(Parameter);
		return protoId.GetName();
	}

	private string GetSkillCheckText(RunState runState) {
		var protoId = new Proto.ID(Parameter);

		int modifier;
		if (protoId.IsPrimaryAttribute()) {
			var statId = new StatProto.ID(Parameter);
			modifier = StatManager.GetAttributeModifier(runState.Stats.Get(statId));
		} else if (protoId.IsStat()) {
			var statId = new StatProto.ID(Parameter);
			modifier = runState.Stats.GetInt(statId);
		} else {
			modifier = 0;
		}

		string sign = modifier >= 0 ? "+" : "";
		string name = protoId.GetName();
		string abbrev = name.Length >= 3 ? name[..3].ToUpper() : name.ToUpper();
		return $"[{abbrev} DC {Value}] ({sign}{modifier})";
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// Result of evaluating a condition.
/// </summary>
public class ConditionResult {
	public bool Passed { get; set; }
	public EventCondition Condition { get; set; } = null!;
	public string? DisplayText { get; set; }
	public int? RollResult { get; set; } // For skill checks
	public int? RollTotal { get; set; }
}

#endregion