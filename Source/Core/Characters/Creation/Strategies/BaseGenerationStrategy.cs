using RPGGame.Core.Prototypes.Characters;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Base class for character generation strategies.
/// Strategies define how random reroll allocates creation points.
/// </summary>
public abstract class BaseGenerationStrategy {
	protected readonly GameDb GameDb;
	protected readonly Random Random;

	protected BaseGenerationStrategy(GameDb gameDb, Random random) {
		GameDb = gameDb;
		Random = random;
	}

	/// <summary>
	/// Selects the next upgrade to apply.
	/// Returns null if no valid upgrades available.
	/// </summary>
	public abstract CharacterUpgrade? SelectUpgrade(
		CharacterCreationState state,
		CharacterClassProto classProto,
		CharacterCreationManager manager
	);

	/// <summary>
	/// Gets all affordable upgrades.
	/// </summary>
	protected List<CharacterUpgrade> GetAffordableUpgrades(
		CharacterCreationState state,
		CharacterClassProto classProto,
		CharacterCreationManager manager
	) {
		var upgrades = new List<CharacterUpgrade>();

		// Attributes
		AddAttributeUpgrades(upgrades, state, manager);

		// Skills
		AddSkillUpgrades(upgrades, state, manager);

		// Spells (if caster)
		if (classProto.CanCastSpells) {
			AddSpellUpgrades(upgrades, state, manager);
		}

		// Bonus HP/Mana
		AddDerivedStatUpgrades(upgrades, state, classProto);

		return upgrades.Where(u => u.Cost <= state.PointsRemaining).ToList();
	}

	private void AddAttributeUpgrades(
		List<CharacterUpgrade> upgrades,
		CharacterCreationState state,
		CharacterCreationManager manager
	) {
		var attributes = new[] {
			Ids.Stats.Attributes.Strength,
			Ids.Stats.Attributes.Dexterity,
			Ids.Stats.Attributes.Constitution,
			Ids.Stats.Attributes.Intelligence,
			Ids.Stats.Attributes.Wisdom,
			Ids.Stats.Attributes.Charisma
		};

		foreach (var attr in attributes) {
			if (manager.CanIncreaseAttribute(attr)) {
				int cost = manager.GetAttributeIncreaseCost(attr);
				upgrades.Add(CharacterUpgrade.Attribute(attr, cost));
			}
		}
	}

	private void AddSkillUpgrades(
		List<CharacterUpgrade> upgrades,
		CharacterCreationState state,
		CharacterCreationManager manager
	) {
		foreach (var skill in manager.GetAvailableSkills()) {
			if (manager.CanAddSkill(skill.Id)) {
				int cost = manager.GetSkillCost(skill.Id);
				upgrades.Add(CharacterUpgrade.Skill(skill.Id, cost));
			}
		}
	}

	private void AddSpellUpgrades(
		List<CharacterUpgrade> upgrades,
		CharacterCreationState state,
		CharacterCreationManager manager
	) {
		foreach (var spell in manager.GetAvailableSpells()) {
			if (manager.CanAddSpell(spell.Id)) {
				int cost = manager.GetSpellCost(spell.Id);
				upgrades.Add(CharacterUpgrade.Spell(spell.Id, cost));
			}
		}
	}

	private void AddDerivedStatUpgrades(
		List<CharacterUpgrade> upgrades,
		CharacterCreationState state,
		CharacterClassProto classProto
	) {
		upgrades.Add(CharacterUpgrade.Health(Generation.CreationPointCosts.HealthCost));

		if (classProto.CanCastSpells) {
			upgrades.Add(CharacterUpgrade.Mana(Generation.CreationPointCosts.ManaCost));
		}
	}

	/// <summary>
	/// Selects randomly from a weighted list.
	/// </summary>
	protected CharacterUpgrade? SelectWeighted(List<(CharacterUpgrade upgrade, float weight)> weightedUpgrades) {
		if (weightedUpgrades.Count == 0) return null;

		float totalWeight = weightedUpgrades.Sum(u => u.weight);
		float roll = (float)Random.NextDouble() * totalWeight;
		float cumulative = 0;

		foreach (var (upgrade, weight) in weightedUpgrades) {
			cumulative += weight;
			if (roll <= cumulative) {
				return upgrade;
			}
		}

		return weightedUpgrades[^1].upgrade;
	}
}