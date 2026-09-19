using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Items;

/// <summary>
/// Represents a stat modifier from equipment.
/// </summary>
public class EquipmentStat {
	/// <summary>The stat or skill being modified.</summary>
	public Proto.ID Target { get; init; }

	/// <summary>The modification value (always positive).</summary>
	public float Value { get; init; }

	/// <summary>How the value is applied.</summary>
	public ModifierOperation Operation { get; init; } = ModifierOperation.FlatAdd;

	#region Factory Methods - Stats (Bonuses)

	/// <summary>Adds a flat value to a stat. Example: +5 Strength</summary>
	public static EquipmentStat Flat(StatProto.ID stat, float value) => new() {
		Target = stat, Value = value, Operation = ModifierOperation.FlatAdd
	};

	/// <summary>Increases a stat by a percentage. Example: +10% Health</summary>
	public static EquipmentStat Percent(StatProto.ID stat, float value) => new() {
		Target = stat, Value = value, Operation = ModifierOperation.PercentIncrease
	};

	/// <summary>Multiplicative increase. Example: 20% more damage</summary>
	public static EquipmentStat More(StatProto.ID stat, float value) => new() {
		Target = stat, Value = value, Operation = ModifierOperation.PercentMore
	};

	#endregion

	#region Factory Methods - Stats (Penalties)

	/// <summary>Subtracts a flat value from a stat. Example: -2 Speed (cursed item)</summary>
	public static EquipmentStat FlatPenalty(StatProto.ID stat, float value) => new() {
		Target = stat, Value = value, Operation = ModifierOperation.FlatSubtract
	};

	/// <summary>Reduces a stat by a percentage. Example: -15% Movement Speed</summary>
	public static EquipmentStat PercentPenalty(StatProto.ID stat, float value) => new() {
		Target = stat, Value = value, Operation = ModifierOperation.PercentReduce
	};

	/// <summary>Multiplicative decrease. Example: 10% less damage taken</summary>
	public static EquipmentStat Less(StatProto.ID stat, float value) => new() {
		Target = stat, Value = value, Operation = ModifierOperation.PercentLess
	};

	#endregion

	#region Factory Methods - Skills

	public static EquipmentStat SkillFlat(SkillProto.ID skill, float value) => new() {
		Target = skill, Value = value, Operation = ModifierOperation.FlatAdd
	};

	public static EquipmentStat SkillPercent(SkillProto.ID skill, float value) => new() {
		Target = skill, Value = value, Operation = ModifierOperation.PercentIncrease
	};

	#endregion

	#region Factory Methods - Generic

	public static EquipmentStat Flat(Proto.ID target, float value) => new() {
		Target = target, Value = value, Operation = ModifierOperation.FlatAdd
	};

	public static EquipmentStat Percent(Proto.ID target, float value) => new() {
		Target = target, Value = value, Operation = ModifierOperation.PercentIncrease
	};

	#endregion

	#region Display

	public string GetDisplayString() {
		return Operation switch {
			ModifierOperation.FlatAdd => $"+{Value:0}",
			ModifierOperation.FlatSubtract => $"-{Value:0}",
			ModifierOperation.PercentIncrease => $"+{Value:0}%",
			ModifierOperation.PercentReduce => $"-{Value:0}%",
			ModifierOperation.PercentMore => $"{Value:0}% more",
			ModifierOperation.PercentLess => $"{Value:0}% less",
			_ => $"{Value:0}"
		};
	}

	#endregion
}