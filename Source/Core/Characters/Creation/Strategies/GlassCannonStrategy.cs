using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Glass cannon strategy - maximum offense, minimal defense.
/// </summary>
public class GlassCannonStrategy : BaseGenerationStrategy {
	public GlassCannonStrategy(GameDb gameDb, Random random) : base(gameDb, random) { }

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
				// Max out primary offensive stat
				if (u.AttributeId == classProto.PrimaryAttribute) {
					weight = 5f;
				} else if (u.AttributeId == Ids.Stats.Attributes.Dexterity ||
						   u.AttributeId == Ids.Stats.Attributes.Intelligence) {
					weight = 3f;
				} else if (u.AttributeId == Ids.Stats.Attributes.Constitution) {
					weight = 0.5f; // Actively avoid CON
				}
			} else if (u.Type == CharacterUpgradeType.Skill) {
				var skill = GameDb.Get<Prototypes.Skills.SkillProto>(u.SkillId!.Value);
				if (skill?.HasTag(Ids.Tags.Combat.Offense) == true) {
					weight = 3f;
				} else if (skill?.HasTag(Ids.Tags.Combat.Defense) == true) {
					weight = 0.3f; // Avoid defensive skills
				}
			} else if (u.Type == CharacterUpgradeType.Spell) {
				var spell = GameDb.Get<Prototypes.Spells.SpellProto>(u.SpellId!.Value);
				if (spell?.DamageDice.SidesOfDice > 0) {
					weight = 4f;
				}
			} else if (u.Type == CharacterUpgradeType.BonusHealth) {
				weight = 0.3f; // Avoid HP
			} else if (u.Type == CharacterUpgradeType.BonusMana && classProto.CanCastSpells) {
				weight = 2f;
			}

			return (u, weight);
		}).ToList();

		return SelectWeighted(weighted);
	}
}