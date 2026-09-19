using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Tank generation strategy - prioritizes maximum survivability and threat.
/// </summary>
public class TankStrategy : BaseGenerationStrategy {
	public TankStrategy(GameDb gameDb, Random random) : base(gameDb, random) { }

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
					weight = 4f;
				} else if (u.AttributeId == Ids.Stats.Attributes.Strength) {
					weight = 2f;
				}
			} else if (u.Type == CharacterUpgradeType.BonusHealth) {
				weight = 4f;
			} else if (u.Type == CharacterUpgradeType.Skill) {
				var skill = GameDb.Get<Prototypes.Skills.SkillProto>(u.SkillId!.Value);
				if (skill?.HasTag(Ids.Tags.Combat.Defense) == true ||
					skill?.HasTag(Ids.Tags.Weapon.Shield) == true) {
					weight = 3f;
				}
			}

			return (u, weight);
		}).ToList();

		return SelectWeighted(weighted);
	}
}