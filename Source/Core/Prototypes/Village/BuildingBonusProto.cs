using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Prototypes.Village;

/// <summary>
/// A stat bonus provided by a building.
/// </summary>
public class BuildingBonusProto {
	/// <summary>The stat being modified.</summary>
	public StatProto.ID Stat { get; }

	/// <summary>The value of the modification.</summary>
	public float Value { get; }

	/// <summary>How the value is applied.</summary>
	public ModifierOperation Operation { get; }

	public BuildingBonusProto(StatProto.ID stat, float value, ModifierOperation operation = ModifierOperation.FlatAdd) {
		Stat = stat;
		Value = value;
		Operation = operation;
	}

	/// <summary>Creates a flat bonus.</summary>
	public static BuildingBonusProto Flat(StatProto.ID stat, float value) {
		return new BuildingBonusProto(stat, value, ModifierOperation.FlatAdd);
	}

	/// <summary>Creates a percentage bonus.</summary>
	public static BuildingBonusProto Percent(StatProto.ID stat, float percent) {
		return new BuildingBonusProto(stat, percent, ModifierOperation.PercentIncrease);
	}
}
