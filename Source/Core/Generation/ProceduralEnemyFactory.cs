using RPGGame.Core.Combat;
using RPGGame.Core.Expedition;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Generation;

/// <summary>
/// Factory for generating procedural enemies from base prototypes.
/// Takes an EnemyProto and applies creation points to randomize stats and add tags.
/// </summary>
public class ProceduralEnemyFactory {
	private readonly GameDb k_gameDb;
	private readonly Random k_random;

	#region Upgrade Costs

	/// <summary>Cost per point of stat increase.</summary>
	private static readonly Dictionary<StatProto.ID, int> StatCosts = new() {
		{ Ids.Stats.Resource.MaxHealth, 2 },
		{ Ids.Stats.Combat.ArmorClass, 8 },
		{ Ids.Stats.Combat.AttackBonus, 6 },
		{ Ids.Stats.Combat.DamBonus, 5 },
		{ Ids.Stats.Combat.Initiative, 4 },
		{ Ids.Stats.Attributes.Strength, 10 },
		{ Ids.Stats.Attributes.Dexterity, 10 },
		{ Ids.Stats.Attributes.Constitution, 10 },
		{ Ids.Stats.Attributes.Intelligence, 10 },
		{ Ids.Stats.Attributes.Wisdom, 10 },
		{ Ids.Stats.Attributes.Charisma, 10 }
	};

	/// <summary>Cost to add an extra damage die.</summary>
	private const int EXTRA_DAMAGE_DIE_COST = 15;

	/// <summary>Cost to upgrade damage die size.</summary>
	private const int UPGRADE_DIE_SIZE_COST = 10;

	/// <summary>Cost to add a new ability.</summary>
	private const int NEW_ABILITY_COST = 25;

	#endregion

	#region Constructor

	private static readonly Random SharedRandom = new();

	public ProceduralEnemyFactory(GameDb gameDb, Random? random = null) {
		k_gameDb = gameDb;
		k_random = random ?? SharedRandom;
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Generates a procedural enemy from a base prototype.
	/// </summary>
	public GeneratedEnemy? Generate(
		EnemyProto.ID protoId,
		int minPoints,
		int maxPoints,
		RarityType maxRarity = RarityType.Rare,
		List<TagProto.ID>? forcedTags = null,
		TerrainProto.ID? terrain = null,
		bool isNight = false) {

		var proto = k_gameDb.Get<EnemyProto>(protoId);
		if (proto == null) {
			return null;
		}

		// Create budget
		var budget = CreationPointBudget.Create(minPoints, maxPoints, maxRarity, k_random);

		// Start with base stats from proto
		var enemy = new GeneratedEnemy(proto, budget.Rarity);

		// Apply forced tags first (free)
		if (forcedTags != null) {
			foreach (var tag in forcedTags) {
				ApplyTag(enemy, tag);
			}
		}

		// Apply time-of-day tag (free)
		if (isNight && !enemy.HasTag(Ids.Tags.TimeOfDay.Diurnal)) {
			ApplyTag(enemy, Ids.Tags.TimeOfDay.Nocturnal);
		}

		// Apply regional tag if applicable (free)
		if (terrain.HasValue) {
			var terrainProto = k_gameDb.Get<TerrainProto>(terrain.Value);
			ApplyRegionalTag(enemy, terrainProto);
			ApplyTerrainTags(enemy, terrainProto);
		}

		// Spend points on upgrades
		SpendCreationPoints(enemy, budget);

		// Apply elite/champion based on rarity
		ApplyRarityTags(enemy, budget.Rarity);

		// Calculate final stats
		enemy.FinalizeStats();

		return enemy;
	}

	/// <summary>
	/// Generates multiple enemies from the same prototype with varying stats.
	/// </summary>
	public List<GeneratedEnemy> GenerateGroup(
		EnemyProto.ID protoId,
		int count,
		int minPoints,
		int maxPoints,
		RarityType maxRarity = RarityType.Uncommon,
		List<TagProto.ID>? forcedTags = null) {

		var enemies = new List<GeneratedEnemy>();

		for (int i = 0; i < count; i++) {
			var enemy = Generate(protoId, minPoints, maxPoints, maxRarity, forcedTags);
			if (enemy != null) {
				enemies.Add(enemy);
			}
		}

		return enemies;
	}

	#endregion

	#region Private Methods

	private void SpendCreationPoints(GeneratedEnemy enemy, CreationPointBudget budget) {
		int iterations = 0;
		const int maxIterations = 100;

		while (budget.RemainingPoints > 0 && iterations < maxIterations) {
			iterations++;

			var affordableUpgrades = GetAffordableUpgrades(enemy, budget);
			if (affordableUpgrades.Count == 0) {
				break;
			}

			var upgrade = affordableUpgrades[k_random.Next(affordableUpgrades.Count)];
			ApplyUpgrade(enemy, upgrade, budget);
		}
	}

	private List<EnemyUpgrade> GetAffordableUpgrades(GeneratedEnemy enemy, CreationPointBudget budget) {
		var upgrades = new List<EnemyUpgrade>();
		int remaining = budget.RemainingPoints;

		// Stat upgrades
		foreach (var (statId, cost) in StatCosts) {
			if (cost <= remaining) {
				upgrades.Add(new EnemyUpgrade(EnemyUpgradeType.Stat, cost, statId: statId));
			}
		}

		// Damage die upgrades
		if (EXTRA_DAMAGE_DIE_COST <= remaining) {
			upgrades.Add(new EnemyUpgrade(EnemyUpgradeType.ExtraDamageDie, EXTRA_DAMAGE_DIE_COST));
		}
		if (UPGRADE_DIE_SIZE_COST <= remaining && enemy.CanUpgradeDieSize()) {
			upgrades.Add(new EnemyUpgrade(EnemyUpgradeType.UpgradeDieSize, UPGRADE_DIE_SIZE_COST));
		}

		// Tag upgrades
		var availableTags = GetAvailableTags(enemy, remaining);
		foreach (var (tagId, cost) in availableTags) {
			upgrades.Add(new EnemyUpgrade(EnemyUpgradeType.Tag, cost, tagId: tagId));
		}

		// Ability upgrade
		if (NEW_ABILITY_COST <= remaining && enemy.Abilities.Count < 3) {
			upgrades.Add(new EnemyUpgrade(EnemyUpgradeType.Ability, NEW_ABILITY_COST));
		}

		return upgrades;
	}

	private List<(TagProto.ID tagId, int cost)> GetAvailableTags(GeneratedEnemy enemy, int maxCost) {
		var result = new List<(TagProto.ID, int)>();

		foreach (var tagProto in k_gameDb.GetAll<TagProto>()) {
			// Skip if already has tag
			if (enemy.HasTag(tagProto.Id)) {
				continue;
			}

			// Skip if can't be randomly applied
			if (!tagProto.CanBeRandomlyApplied) {
				continue;
			}

			// Skip if too expensive
			if (tagProto.CreationPointCost > maxCost || tagProto.CreationPointCost <= 0) {
				continue;
			}

			// Skip if conflicts with existing tags
			if (HasConflictingTag(enemy, tagProto)) {
				continue;
			}

			// Skip if level requirement not met
			if (tagProto.MinLevel > enemy.EffectiveLevel) {
				continue;
			}
			if (tagProto.MaxLevel >= 0 && tagProto.MaxLevel < enemy.EffectiveLevel) {
				continue;
			}

			result.Add((tagProto.Id, tagProto.CreationPointCost));
		}

		return result;
	}

	private bool HasConflictingTag(GeneratedEnemy enemy, TagProto tagProto) {
		foreach (var conflictId in tagProto.ConflictingTags) {
			if (enemy.HasTag(conflictId)) {
				return true;
			}
		}
		return false;
	}

	private void ApplyUpgrade(GeneratedEnemy enemy, EnemyUpgrade upgrade, CreationPointBudget budget) {
		budget.Spend(upgrade.Cost);

		switch (upgrade.Type) {
			case EnemyUpgradeType.Stat:
				ApplyStatUpgrade(enemy, upgrade.StatId!.Value);
				break;

			case EnemyUpgradeType.ExtraDamageDie:
				enemy.AddDamageDie();
				break;

			case EnemyUpgradeType.UpgradeDieSize:
				enemy.UpgradeDieSize();
				break;

			case EnemyUpgradeType.Tag:
				ApplyTag(enemy, upgrade.TagId!.Value);
				break;

			case EnemyUpgradeType.Ability:
				// TODO: Add random ability from pool
				break;
		}
	}

	private void ApplyStatUpgrade(GeneratedEnemy enemy, StatProto.ID statId) {
		if (statId == Ids.Stats.Resource.MaxHealth) {
			enemy.BonusHealth += 5;
		} else if (statId == Ids.Stats.Combat.ArmorClass) {
			enemy.BonusAC += 1;
		} else if (statId == Ids.Stats.Combat.AttackBonus) {
			enemy.BonusAttack += 1;
		} else if (statId == Ids.Stats.Combat.DamBonus) {
			enemy.BonusDamage += 1;
		} else if (statId == Ids.Stats.Combat.Initiative) {
			enemy.BonusInitiative += 1;
		} else {
			// Attribute upgrades
			enemy.AttributeBonuses[statId] = enemy.AttributeBonuses.GetValueOrDefault(statId) + 1;
		}
	}

	private void ApplyTag(GeneratedEnemy enemy, TagProto.ID tagId) {
		if (enemy.HasTag(tagId)) {
			return;
		}

		// Add the tag ID to the enemy
		enemy.Tags.Add(tagId);

		// Get tag proto to apply its modifiers
		if (!k_gameDb.TryGetProto<TagProto>(tagId, out var tagProto)) {
			return;
		}

		// Apply modifiers from the tag
		foreach (var (target, modifier) in tagProto.Modifiers) {
			enemy.AddTagModifier(target, modifier);
		}

		// Apply implied tags
		foreach (var impliedTagId in tagProto.ImpliedTags) {
			ApplyTag(enemy, impliedTagId);
		}
	}

	/// <summary>
	/// Applies regional tags from the terrain to the enemy.
	/// Uses data-driven tags from TerrainProto instead of hardcoded switch.
	/// </summary>
	private void ApplyRegionalTag(GeneratedEnemy enemy, TerrainProto terrain) {
		// Apply all regional tags from the terrain
		foreach (var tag in terrain.Tags) {
			// Only apply tags that are regional tags (Tag_Region_*)
			if (tag.Value.StartsWith("Tag_Region_")) {
				ApplyTag(enemy, tag);
			}
		}
	}

	/// <summary>
	/// Applies terrain-related tags to the enemy.
	/// Includes regional, elemental, and creature type tags from the terrain.
	/// </summary>
	private void ApplyTerrainTags(GeneratedEnemy enemy, TerrainProto terrain) {
		foreach (var tag in terrain.Tags) {
			// Apply regional, element, and creature tags from terrain
			if (tag.Value.StartsWith("Tag_Element_") ||
				tag.Value.StartsWith("Tag_Creature_")) {
				ApplyTag(enemy, tag);
			}
		}
	}

	private void ApplyRarityTags(GeneratedEnemy enemy, RarityType rarity) {
		switch (rarity) {
			case RarityType.Epic:
			case RarityType.Legendary:
				ApplyTag(enemy, Ids.Tags.Special.Elite);
				break;

			case RarityType.Mythic:
				ApplyTag(enemy, Ids.Tags.Special.Champion);
				break;
		}
	}

	#endregion
}

/// <summary>
/// Types of upgrades that can be applied to enemies.
/// </summary>
public enum EnemyUpgradeType {
	Stat,
	ExtraDamageDie,
	UpgradeDieSize,
	Tag,
	Ability
}

/// <summary>
/// Represents an upgrade option for an enemy.
/// </summary>
public readonly struct EnemyUpgrade {
	public EnemyUpgradeType Type { get; }
	public int Cost { get; }
	public StatProto.ID? StatId { get; }
	public TagProto.ID? TagId { get; }

	public EnemyUpgrade(
		EnemyUpgradeType type,
		int cost,
		StatProto.ID? statId = null,
		TagProto.ID? tagId = null) {

		Type = type;
		Cost = cost;
		StatId = statId;
		TagId = tagId;
	}
}