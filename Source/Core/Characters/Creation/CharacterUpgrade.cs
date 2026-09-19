using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Represents a possible upgrade during character creation.
/// </summary>
public class CharacterUpgrade {
	public CharacterUpgradeType Type { get; init; }
	public int Cost { get; init; }

	// Type-specific data
	public StatProto.ID? AttributeId { get; init; }
	public SkillProto.ID? SkillId { get; init; }
	public SpellProto.ID? SpellId { get; init; }

	public static CharacterUpgrade Attribute(StatProto.ID id, int cost) =>
		new() { Type = CharacterUpgradeType.Attribute, AttributeId = id, Cost = cost };

	public static CharacterUpgrade Skill(SkillProto.ID id, int cost) =>
		new() { Type = CharacterUpgradeType.Skill, SkillId = id, Cost = cost };

	public static CharacterUpgrade Spell(SpellProto.ID id, int cost) =>
		new() { Type = CharacterUpgradeType.Spell, SpellId = id, Cost = cost };

	public static CharacterUpgrade Health(int cost) =>
		new() { Type = CharacterUpgradeType.BonusHealth, Cost = cost };

	public static CharacterUpgrade Mana(int cost) =>
		new() { Type = CharacterUpgradeType.BonusMana, Cost = cost };
}

/// <summary>
/// Types of character upgrades.
/// </summary>
public enum CharacterUpgradeType {
	Attribute,
	Skill,
	Spell,
	BonusHealth,
	BonusMana,
	BonusStamina,
	SkillSchool,
	SpellSchool,
	Tag
}