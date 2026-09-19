using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Offensive generation strategy - prioritizes damage and attack stats.
/// </summary>
public class OffensiveStrategy : BaseGenerationStrategy {
	public OffensiveStrategy(GameDb gameDb, Random random) : base(gameDb, random) { }

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
				// Favor offensive attributes
				if (u.AttributeId == Ids.Stats.Attributes.Strength ||
					u.AttributeId == Ids.Stats.Attributes.Dexterity) {
					weight = 3f;
				} else if (u.AttributeId == classProto.PrimaryAttribute) {
					weight = 2.5f;
				}
			} else if (u.Type == CharacterUpgradeType.Skill) {
				// Favor combat skills
				var skill = GameDb.Get<Prototypes.Skills.SkillProto>(u.SkillId!.Value);
				if (skill?.HasTag(Ids.Tags.Meta.CombatSkill) == true) {
					weight = 2.5f;
				}
			} else if (u.Type == CharacterUpgradeType.Spell) {
				// Favor damage spells
				var spell = GameDb.Get<Prototypes.Spells.SpellProto>(u.SpellId!.Value);
				if (spell?.DamageDice.SidesOfDice > 0) {
					weight = 2f;
				}
			}

			return (u, weight);
		}).ToList();

		return SelectWeighted(weighted);
	}
}