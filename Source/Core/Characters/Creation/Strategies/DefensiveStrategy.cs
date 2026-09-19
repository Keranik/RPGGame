using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Defensive generation strategy - prioritizes survivability.
/// </summary>
public class DefensiveStrategy : BaseGenerationStrategy {
	public DefensiveStrategy(GameDb gameDb, Random random) : base(gameDb, random) { }

	public override CharacterUpgrade? SelectUpgrade(
		CharacterCreationState state,
		CharacterClassProto classProto,
		CharacterCreationManager manager
	) {
		var affordable = GetAffordableUpgrades(state, classProto, manager);
		if (affordable.Count == 0) return null;

		var weighted = affordable.Select(u => {
			float weight = 1f;

			if (u.Type == CharacterUpgradeType.Attribute) {
				if (u.AttributeId == Ids.Stats.Attributes.Constitution) {
					weight = 3f;
				} else if (u.AttributeId == Ids.Stats.Attributes.Wisdom) {
					weight = 2f;
				}
			} else if (u.Type == CharacterUpgradeType.BonusHealth) {
				weight = 3f;
			} else if (u.Type == CharacterUpgradeType.Skill) {
				var skill = GameDb.Get<Prototypes.Skills.SkillProto>(u.SkillId!.Value);
				if (skill?.HasTag(Ids.Tags.Combat.Defense) == true) {
					weight = 2.5f;
				}
			} else if (u.Type == CharacterUpgradeType.Spell) {
				var spell = GameDb.Get<Prototypes.Spells.SpellProto>(u.SpellId!.Value);
				if (spell?.School == Spells.SpellSchool.Abjuration ||
					spell?.HealingDice.SidesOfDice > 0) {
					weight = 2.5f;
				}
			}

			return (u, weight);
		}).ToList();

		return SelectWeighted(weighted);
	}
}