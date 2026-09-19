using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Skill-heavy generation strategy - prioritizes skill variety.
/// </summary>
public class SkillHeavyStrategy : BaseGenerationStrategy {
	public SkillHeavyStrategy(GameDb gameDb, Random random) : base(gameDb, random) { }

	public override CharacterUpgrade? SelectUpgrade(
		CharacterCreationState state,
		CharacterClassProto classProto,
		CharacterCreationManager manager
	) {
		var affordable = GetAffordableUpgrades(state, classProto, manager);
		if (affordable.Count == 0) return null;

		var weighted = affordable.Select(u => {
			float weight = 1f;

			if (u.Type == CharacterUpgradeType.Skill) {
				// Favor new skills over ranking up existing ones
				bool isNewSkill = !state.SelectedSkills.ContainsKey(u.SkillId!.Value);
				weight = isNewSkill ? 4f : 2f;
			} else if (u.Type == CharacterUpgradeType.Attribute) {
				// Favor attributes that help skills
				if (u.AttributeId == classProto.PrimaryAttribute) {
					weight = 2f;
				} else if (u.AttributeId == classProto.SecondaryAttribute) {
					weight = 1.5f;
				}
			}

			return (u, weight);
		}).ToList();

		return SelectWeighted(weighted);
	}
}