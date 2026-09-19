using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Generation;

/// <summary>
/// Represents a modification to a stat or skill from a tag.
/// Pairs a Proto.ID (StatProto.ID or SkillProto.ID) with a ValueModifier.
/// </summary>
public readonly struct TagStatModifier {
	/// <summary>The target being modified (stat or skill).</summary>
	public Proto.ID Target { get; }

	/// <summary>How the target is modified.</summary>
	public ValueModifier Modifier { get; }

	public TagStatModifier(Proto.ID target, ValueModifier modifier) {
		Target = target;
		Modifier = modifier;
	}

	#region Factory Methods - Stats

	/// <summary>Creates a stat modifier with the given ValueModifier.</summary>
	public static TagStatModifier Create(StatProto.ID stat, ValueModifier modifier) {
		return new TagStatModifier(stat, modifier);
	}

	/// <summary>Creates a flat stat bonus.</summary>
	public static TagStatModifier Flat(StatProto.ID stat, int value) {
		return new TagStatModifier(stat, value.Flat());
	}

	/// <summary>Creates a flat stat penalty.</summary>
	public static TagStatModifier Penalty(StatProto.ID stat, int value) {
		return new TagStatModifier(stat, value.FlatSubtract());
	}

	/// <summary>Creates an additive percentage increase to a stat.</summary>
	public static TagStatModifier PercentIncrease(StatProto.ID stat, float percent) {
		return new TagStatModifier(stat, percent.PercentIncrease());
	}

	/// <summary>Creates an additive percentage reduction to a stat.</summary>
	public static TagStatModifier PercentReduce(StatProto.ID stat, float percent) {
		return new TagStatModifier(stat, percent.PercentReduce());
	}

	/// <summary>Creates a multiplicative percentage increase to a stat.</summary>
	public static TagStatModifier PercentMore(StatProto.ID stat, float percent) {
		return new TagStatModifier(stat, percent.PercentMore());
	}

	/// <summary>Creates a multiplicative percentage decrease to a stat.</summary>
	public static TagStatModifier PercentLess(StatProto.ID stat, float percent) {
		return new TagStatModifier(stat, percent.PercentLess());
	}

	#endregion

	#region Factory Methods - Skills

	/// <summary>Creates a skill modifier with the given ValueModifier.</summary>
	public static TagStatModifier Create(SkillProto.ID skill, ValueModifier modifier) {
		return new TagStatModifier(skill, modifier);
	}

	/// <summary>Creates a flat skill bonus.</summary>
	public static TagStatModifier SkillFlat(SkillProto.ID skill, int value) {
		return new TagStatModifier(skill, value.Flat());
	}

	/// <summary>Creates a flat skill penalty.</summary>
	public static TagStatModifier SkillPenalty(SkillProto.ID skill, int value) {
		return new TagStatModifier(skill, value.FlatSubtract());
	}

	/// <summary>Creates an additive percentage increase to a skill.</summary>
	public static TagStatModifier SkillPercentIncrease(SkillProto.ID skill, float percent) {
		return new TagStatModifier(skill, percent.PercentIncrease());
	}

	/// <summary>Creates an additive percentage reduction to a skill.</summary>
	public static TagStatModifier SkillPercentReduce(SkillProto.ID skill, float percent) {
		return new TagStatModifier(skill, percent.PercentReduce());
	}

	#endregion

	public override string ToString() => $"{Target}: {Modifier}";
}