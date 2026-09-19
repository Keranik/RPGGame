using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Spell-heavy generation strategy - prioritizes spells and mana.
/// </summary>
public class SpellHeavyStrategy : BaseGenerationStrategy {
	public SpellHeavyStrategy(GameDb gameDb, Random random) : base(gameDb, random) { }

	public override CharacterUpgrade? SelectUpgrade(
		CharacterCreationState state,
		CharacterClassProto classProto,
		CharacterCreationManager manager
	) {
		var affordable = GetAffordableUpgrades(state, classProto, manager);
		if (affordable.Count == 0) return null;

		var weighted = affordable.Select(u => {
			float weight = 1f;

			if (u.Type == CharacterUpgradeType.Spell) {
				weight = 4f; // Heavily favor spells
			} else if (u.Type == CharacterUpgradeType.BonusMana) {
				weight = 3f;
			} else if (u.Type == CharacterUpgradeType.Attribute) {
				if (u.AttributeId == Ids.Stats.Attributes.Intelligence) {
					weight = 3f;
				} else if (u.AttributeId == Ids.Stats.Attributes.Wisdom) {
					weight = 2f;
				}
			} else if (u.Type == CharacterUpgradeType.Skill) {
				var skill = GameDb.Get<Prototypes.Skills.SkillProto>(u.SkillId!.Value);
				if (skill?.HasTag(Ids.Tags.Meta.MagicSkill) == true) {
					weight = 2.5f;
				}
			}

			return (u, weight);
		}).ToList();

		return SelectWeighted(weighted);
	}
}
