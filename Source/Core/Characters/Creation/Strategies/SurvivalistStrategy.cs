using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Survivalist strategy - prioritizes utility and survival skills.
/// </summary>
public class SurvivalistStrategy : BaseGenerationStrategy {
	public SurvivalistStrategy(GameDb gameDb, Random random) : base(gameDb, random) { }

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
				if (u.AttributeId == Ids.Stats.Attributes.Wisdom) {
					weight = 3f;
				} else if (u.AttributeId == Ids.Stats.Attributes.Constitution) {
					weight = 2.5f;
				}
			} else if (u.Type == CharacterUpgradeType.Skill) {
				var skill = GameDb.Get<Prototypes.Skills.SkillProto>(u.SkillId!.Value);
				if (skill?.HasTag(Ids.Tags.Meta.SurvivalSkill) == true) {
					weight = 4f;
				} else if (skill?.HasTag(Ids.Tags.Meta.CraftingSkill) == true) {
					weight = 2f;
				} else if (skill?.HasTag(Ids.Tags.Meta.StealthSkill) == true) {
					weight = 2f;
				}
			} else if (u.Type == CharacterUpgradeType.BonusHealth) {
				weight = 2f;
			}

			return (u, weight);
		}).ToList();

		return SelectWeighted(weighted);
	}
}