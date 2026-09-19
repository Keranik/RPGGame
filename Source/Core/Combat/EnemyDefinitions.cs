using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Combat;

namespace RPGGame.Core.Combat;

/// <summary>
/// Defines all enemy types using the Proto system.
/// </summary>
public class EnemyDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterBeasts(gameDatabase);
		RegisterHumanoids(gameDatabase);
		RegisterUndead(gameDatabase);
		RegisterDemons(gameDatabase);
		RegisterBosses(gameDatabase);
	}

	#region Beasts

	private void RegisterBeasts(GameDb db) {
		db.RegisterProto(new EnemyProto(Ids.Enemies.Beasts.Wolf) {
			Name = "Wolf",
			Description = "A hungry wolf prowling for prey.",
			PortraitName = "enemy_wolf",
			Category = EnemyCategory.Beast,
			MaxHealth = 12,
			ArmorClass = 8,
			AttackBonus = 3,
			DamageBonus = 1,
			InitiativeBonus = 2,
			AttackDamage = HitDice.D4,
			DamageType = DamageType.Piercing,
			ExperienceValue = 25,
			GoldValue = 0,
			LootTable = [
				new LootEntry(Ids.Items.Materials.LeatherScraps, 0.5f),
				new LootEntry(Ids.Items.Materials.WolfPelt, 0.3f)
			],
			AIType = EnemyAIType.Aggressive,
			ChallengeRating = 0.5f,
			SpawnWeight = 20f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Beasts.DireWolf) {
			Name = "Dire Wolf",
			Description = "A massive wolf, alpha of its pack.",
			PortraitName = "enemy_dire_wolf",
			Category = EnemyCategory.Beast,
			MaxHealth = 35,
			ArmorClass = 14,
			AttackBonus = 5,
			DamageBonus = 3,
			InitiativeBonus = 2,
			AttackDamage = new HitDice(2, 6),
			DamageType = DamageType.Piercing,
			ExperienceValue = 75,
			GoldValue = 0,
			LootTable = [
				new LootEntry(Ids.Items.Materials.LeatherScraps, 0.8f, 2, 3),
				new LootEntry(Ids.Items.Materials.DireWolfPelt, 0.5f)
			],
			AIType = EnemyAIType.Aggressive,
			ChallengeRating = 2f,
			SpawnWeight = 5f,
			MinSpawnDistance = 10f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Beasts.Bear) {
			Name = "Bear",
			Description = "A large, territorial bear.",
			PortraitName = "enemy_bear",
			Category = EnemyCategory.Beast,
			MaxHealth = 45,
			ArmorClass = 13,
			AttackBonus = 4,
			DamageBonus = 4,
			InitiativeBonus = 0,
			AttackDamage = new HitDice(2, 6, 2),
			DamageType = DamageType.Slashing,
			ExperienceValue = 100,
			GoldValue = 0,
			LootTable = [
				new LootEntry(Ids.Items.Materials.BearPelt, 0.6f),
				new LootEntry(Ids.Items.Materials.BearClaw, 0.3f)
			],
			AIType = EnemyAIType.Defensive,
			ChallengeRating = 2.5f,
			SpawnWeight = 8f,
			MinSpawnDistance = 5f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest, Ids.Terrains.Plains.Hills]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Beasts.GiantSpider) {
			Name = "Giant Spider",
			Description = "A massive spider with venomous fangs.",
			PortraitName = "enemy_spider",
			Category = EnemyCategory.Beast,
			MaxHealth = 25,
			ArmorClass = 14,
			AttackBonus = 4,
			DamageBonus = 1,
			InitiativeBonus = 3,
			AttackDamage = HitDice.D8,
			DamageType = DamageType.Piercing,
			ExperienceValue = 50,
			GoldValue = 5,
			LootTable = [
				new LootEntry(Ids.Items.Materials.SpiderSilk, 0.6f),
				new LootEntry(Ids.Items.Materials.VenomSac, 0.3f)
			],
			AIType = EnemyAIType.Tactical,
			Abilities = [
				new EnemyAbility {
					Id = "poison_bite",
					Name = "Poison Bite",
					Type = EnemyAbilityType.Attack,
					TargetType = TargetType.SingleEnemy,
					DamageDice = HitDice.D4,
					DamageType = DamageType.Poison,
					Effects = [new AbilityEffect { AppliesCondition = StatusCondition.Poisoned, ConditionDuration = 3.Turns() }]
				}
			],
			ChallengeRating = 1.5f,
			SpawnWeight = 10f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Underground.Cave, Ids.Terrains.Ruins.OpenRuins]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Beasts.GiantRat) {
			Name = "Giant Rat",
			Description = "An oversized rat with diseased fangs.",
			PortraitName = "enemy_rat",
			Category = EnemyCategory.Beast,
			MaxHealth = 8,
			ArmorClass = 11,
			AttackBonus = 2,
			DamageBonus = 0,
			InitiativeBonus = 2,
			AttackDamage = HitDice.D4,
			DamageType = DamageType.Piercing,
			ExperienceValue = 10,
			GoldValue = 0,
			LootTable = [
				new LootEntry(Ids.Items.Materials.RatTail, 0.4f)
			],
			AIType = EnemyAIType.Cowardly,
			ChallengeRating = 0.25f,
			SpawnWeight = 25f,
			ValidTerrains = [Ids.Terrains.Underground.Cave, Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Wetlands.Swamp]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Beasts.Boar) {
			Name = "Wild Boar",
			Description = "A tusked boar with a bad temper.",
			PortraitName = "enemy_boar",
			Category = EnemyCategory.Beast,
			MaxHealth = 20,
			ArmorClass = 11,
			AttackBonus = 3,
			DamageBonus = 2,
			InitiativeBonus = 0,
			AttackDamage = HitDice.D6,
			DamageType = DamageType.Piercing,
			ExperienceValue = 30,
			GoldValue = 0,
			LootTable = [
				new LootEntry(Ids.Items.Consumables.CookedMeat, 0.6f)
			],
			AIType = EnemyAIType.Aggressive,
			ChallengeRating = 0.75f,
			SpawnWeight = 15f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Beasts.GiantBat) {
			Name = "Giant Bat",
			Description = "A massive bat with razor-sharp fangs.",
			PortraitName = "enemy_bat",
			Category = EnemyCategory.Beast,
			MaxHealth = 12,
			ArmorClass = 13,
			AttackBonus = 2,
			DamageBonus = 0,
			InitiativeBonus = 4,
			AttackDamage = HitDice.D4,
			DamageType = DamageType.Piercing,
			ExperienceValue = 15,
			GoldValue = 0,
			LootTable = [],
			AIType = EnemyAIType.Cowardly,
			ChallengeRating = 0.25f,
			SpawnWeight = 12f,
			NightOnly = true,
			ValidTerrains = [Ids.Terrains.Underground.Cave, Ids.Terrains.Ruins.OpenRuins]
		});
	}

	#endregion

	#region Humanoids

	private void RegisterHumanoids(GameDb db) {
		db.RegisterProto(new EnemyProto(Ids.Enemies.Humanoids.Bandit) {
			Name = "Bandit",
			Description = "A common highway robber.",
			PortraitName = "enemy_bandit",
			Category = EnemyCategory.Humanoid,
			MaxHealth = 20,
			ArmorClass = 12,
			AttackBonus = 3,
			DamageBonus = 1,
			InitiativeBonus = 1,
			AttackDamage = HitDice.D6,
			DamageType = DamageType.Slashing,
			ExperienceValue = 30,
			GoldValue = 15,
			LootTable = [
				new LootEntry(Ids.Items.Materials.LeatherScraps, 0.4f),
				new LootEntry(Ids.Weapons.Swords.IronSword, 0.1f),
				new LootEntry(Ids.Items.Consumables.HealthPotionSmall, 0.2f)
			],
			AIType = EnemyAIType.Cowardly,
			ChallengeRating = 0.75f,
			SpawnWeight = 15f,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path, Ids.Terrains.Forests.Forest]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Humanoids.BanditArcher) {
			Name = "Bandit Archer",
			Description = "A bandit skilled with a bow.",
			PortraitName = "enemy_bandit_archer",
			Category = EnemyCategory.Humanoid,
			MaxHealth = 15,
			ArmorClass = 13,
			AttackBonus = 4,
			DamageBonus = 1,
			InitiativeBonus = 2,
			AttackDamage = HitDice.D8,
			DamageType = DamageType.Piercing,
			ExperienceValue = 40,
			GoldValue = 12,
			LootTable = [
				new LootEntry(Ids.Weapons.Bows.HuntingBow, 0.15f)
			],
			AIType = EnemyAIType.Ranged,
			ChallengeRating = 1f,
			SpawnWeight = 10f,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path, Ids.Terrains.Forests.Forest, Ids.Terrains.Ruins.OpenRuins]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Humanoids.BanditChief) {
			Name = "Bandit Chief",
			Description = "The leader of a bandit gang.",
			PortraitName = "enemy_bandit_chief",
			Category = EnemyCategory.Humanoid,
			MaxHealth = 50,
			ArmorClass = 15,
			AttackBonus = 5,
			DamageBonus = 3,
			InitiativeBonus = 2,
			AttackDamage = new HitDice(1, 8, 2),
			DamageType = DamageType.Slashing,
			ExperienceValue = 150,
			GoldValue = 75,
			LootTable = [
				new LootEntry(Ids.Weapons.Swords.SteelSword, 0.25f),
				new LootEntry(Ids.Armor.Light.StuddedLeather, 0.2f),
				new LootEntry(Ids.Items.Consumables.HealthPotionMedium, 0.4f)
			],
			AIType = EnemyAIType.Tactical,
			Abilities = [
				new EnemyAbility {
					Id = "rally",
					Name = "Rally",
					Type = EnemyAbilityType.Buff,
					TargetType = TargetType.AllAllies,
					Cooldown = 3,
					Effects = [new AbilityEffect { BuffId = "inspired" }]
				}
			],
			ChallengeRating = 3f,
			SpawnWeight = 3f,
			MinSpawnDistance = 15f
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Humanoids.Cultist) {
			Name = "Cultist",
			Description = "A follower of dark powers.",
			PortraitName = "enemy_cultist",
			Category = EnemyCategory.Humanoid,
			MaxHealth = 18,
			ArmorClass = 11,
			AttackBonus = 2,
			DamageBonus = 0,
			InitiativeBonus = 1,
			AttackDamage = HitDice.D4,
			DamageType = DamageType.Slashing,
			ExperienceValue = 35,
			GoldValue = 10,
			LootTable = [
				new LootEntry(Ids.Items.Quest.DarkTomePage, 0.2f),
				new LootEntry(Ids.Items.Materials.Herbs, 0.4f)
			],
			AIType = EnemyAIType.Support,
			Abilities = [
				new EnemyAbility {
					Id = "dark_bolt",
					Name = "Dark Bolt",
					Type = EnemyAbilityType.Spell,
					TargetType = TargetType.SingleEnemy,
					ManaCost = 0,
					DamageDice = HitDice.D6,
					DamageType = DamageType.Necrotic
				}
			],
			ChallengeRating = 1f,
			SpawnWeight = 8f,
			MinSpawnDistance = 10f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Underground.Cave]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Humanoids.CultistMage) {
			Name = "Cultist Mage",
			Description = "A cultist who has learned dark magic.",
			PortraitName = "enemy_cultist_mage",
			Category = EnemyCategory.Humanoid,
			MaxHealth = 25,
			ArmorClass = 12,
			AttackBonus = 2,
			DamageBonus = 0,
			InitiativeBonus = 2,
			AttackDamage = HitDice.D4,
			DamageType = DamageType.Necrotic,
			ExperienceValue = 75,
			GoldValue = 25,
			LootTable = [
				new LootEntry(Ids.Items.Quest.DarkTomePage, 0.4f),
				new LootEntry(Ids.Items.Consumables.ManaPotionSmall, 0.3f)
			],
			AIType = EnemyAIType.Support,
			Abilities = [
				new EnemyAbility {
					Id = "shadow_bolt",
					Name = "Shadow Bolt",
					Type = EnemyAbilityType.Spell,
					TargetType = TargetType.SingleEnemy,
					DamageDice = new HitDice(2, 6),
					DamageType = DamageType.Necrotic
				},
				new EnemyAbility {
					Id = "dark_blessing",
					Name = "Dark Blessing",
					Type = EnemyAbilityType.Buff,
					TargetType = TargetType.AllAllies,
					Cooldown = 4,
					Effects = [new AbilityEffect { BuffId = "dark_power" }]
				}
			],
			ChallengeRating = 2f,
			SpawnWeight = 4f,
			MinSpawnDistance = 15f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Underground.Cave]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Humanoids.Goblin) {
			Name = "Goblin",
			Description = "A small, cunning creature.",
			PortraitName = "enemy_goblin",
			Category = EnemyCategory.Humanoid,
			MaxHealth = 10,
			ArmorClass = 13,
			AttackBonus = 3,
			DamageBonus = 0,
			InitiativeBonus = 2,
			AttackDamage = HitDice.D4,
			DamageType = DamageType.Slashing,
			ExperienceValue = 15,
			GoldValue = 5,
			LootTable = [
				new LootEntry(Ids.Weapons.Daggers.SteelDagger, 0.1f)
			],
			AIType = EnemyAIType.Cowardly,
			ChallengeRating = 0.25f,
			SpawnWeight = 18f,
			ValidTerrains = [Ids.Terrains.Underground.Cave, Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Hills]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Humanoids.GoblinShaman) {
			Name = "Goblin Shaman",
			Description = "A goblin who wields primitive magic.",
			PortraitName = "enemy_goblin_shaman",
			Category = EnemyCategory.Humanoid,
			MaxHealth = 15,
			ArmorClass = 12,
			AttackBonus = 2,
			DamageBonus = 0,
			InitiativeBonus = 1,
			AttackDamage = HitDice.D4,
			DamageType = DamageType.Bludgeoning,
			ExperienceValue = 40,
			GoldValue = 15,
			LootTable = [
				new LootEntry(Ids.Items.Materials.Herbs, 0.5f)
			],
			AIType = EnemyAIType.Support,
			Abilities = [
				new EnemyAbility {
					Id = "hex",
					Name = "Hex",
					Type = EnemyAbilityType.Debuff,
					TargetType = TargetType.SingleEnemy,
					Cooldown = 3,
					Effects = [new AbilityEffect { AppliesCondition = StatusCondition.Weakened, ConditionDuration = 2.Turns() }]
				}
			],
			ChallengeRating = 1f,
			SpawnWeight = 8f,
			ValidTerrains = [Ids.Terrains.Underground.Cave, Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Hills]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Humanoids.OrcWarrior) {
			Name = "Orc Warrior",
			Description = "A brutish orc ready for battle.",
			PortraitName = "enemy_orc",
			Category = EnemyCategory.Humanoid,
			MaxHealth = 35,
			ArmorClass = 13,
			AttackBonus = 5,
			DamageBonus = 3,
			InitiativeBonus = 1,
			AttackDamage = HitDice.D12,
			DamageType = DamageType.Slashing,
			ExperienceValue = 75,
			GoldValue = 20,
			LootTable = [
				new LootEntry(Ids.Weapons.Axes.IronAxe, 0.2f),
				new LootEntry(Ids.Armor.Light.LeatherArmor, 0.15f)
			],
			AIType = EnemyAIType.Aggressive,
			ChallengeRating = 2f,
			SpawnWeight = 8f,
			MinSpawnDistance = 12f,
			ValidTerrains = [Ids.Terrains.Plains.Hills, Ids.Terrains.Mountains.Mountain, Ids.Terrains.Desert.Badlands]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Humanoids.OrcBerserker) {
			Name = "Orc Berserker",
			Description = "An orc consumed by battle rage.",
			PortraitName = "enemy_orc_berserker",
			Category = EnemyCategory.Humanoid,
			MaxHealth = 45,
			ArmorClass = 11,
			AttackBonus = 6,
			DamageBonus = 5,
			InitiativeBonus = 2,
			AttackDamage = new HitDice(2, 6),
			DamageType = DamageType.Slashing,
			ExperienceValue = 120,
			GoldValue = 30,
			LootTable = [
				new LootEntry(Ids.Weapons.Axes.GreatAxe, 0.25f)
			],
			AIType = EnemyAIType.Aggressive,
			Abilities = [
				new EnemyAbility {
					Id = "rage",
					Name = "Rage",
					Type = EnemyAbilityType.Buff,
					TargetType = TargetType.Self,
					Cooldown = 5,
					Effects = [new AbilityEffect { BuffId = "enraged" }]
				}
			],
			ChallengeRating = 3f,
			SpawnWeight = 4f,
			MinSpawnDistance = 18f,
			ValidTerrains = [Ids.Terrains.Plains.Hills, Ids.Terrains.Mountains.Mountain, Ids.Terrains.Desert.Badlands]
		});
	}

	#endregion

	#region Undead

	private void RegisterUndead(GameDb db) {
		db.RegisterProto(new EnemyProto(Ids.Enemies.Undead.Skeleton) {
			Name = "Skeleton",
			Description = "Animated bones of the dead.",
			PortraitName = "enemy_skeleton",
			Category = EnemyCategory.Undead,
			MaxHealth = 12,
			ArmorClass = 13,
			AttackBonus = 2,
			DamageBonus = 0,
			InitiativeBonus = 1,
			AttackDamage = HitDice.D6,
			DamageType = DamageType.Slashing,
			ExperienceValue = 25,
			GoldValue = 5,
			LootTable = [
				new LootEntry(Ids.Items.Materials.Bone, 0.6f),
				new LootEntry(Ids.Weapons.Swords.IronSword, 0.05f)
			],
			AIType = EnemyAIType.Aggressive,
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Piercing, 0.5f },
				{ DamageType.Poison, 1f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Bludgeoning, 0.5f },
				{ DamageType.Holy, 0.5f }
			},
			Immunities = [StatusCondition.Poisoned, StatusCondition.Frightened],
			ChallengeRating = 0.5f,
			SpawnWeight = 12f,
			NightOnly = true,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Underground.Cave]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Undead.SkeletonArcher) {
			Name = "Skeleton Archer",
			Description = "A skeleton wielding a bow.",
			PortraitName = "enemy_skeleton_archer",
			Category = EnemyCategory.Undead,
			MaxHealth = 10,
			ArmorClass = 13,
			AttackBonus = 4,
			DamageBonus = 0,
			InitiativeBonus = 2,
			AttackDamage = HitDice.D6,
			DamageType = DamageType.Piercing,
			ExperienceValue = 30,
			GoldValue = 5,
			LootTable = [
				new LootEntry(Ids.Items.Materials.Bone, 0.5f),
				new LootEntry(Ids.Weapons.Bows.HuntingBow, 0.1f)
			],
			AIType = EnemyAIType.Ranged,
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Piercing, 0.5f },
				{ DamageType.Poison, 1f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Bludgeoning, 0.5f },
				{ DamageType.Holy, 0.5f }
			},
			Immunities = [StatusCondition.Poisoned, StatusCondition.Frightened],
			ChallengeRating = 0.75f,
			SpawnWeight = 8f,
			NightOnly = true,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Underground.Cave]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Undead.SkeletonWarrior) {
			Name = "Skeleton Warrior",
			Description = "A heavily armored skeleton.",
			PortraitName = "enemy_skeleton_warrior",
			Category = EnemyCategory.Undead,
			MaxHealth = 25,
			ArmorClass = 16,
			AttackBonus = 4,
			DamageBonus = 2,
			InitiativeBonus = 0,
			AttackDamage = HitDice.D8,
			DamageType = DamageType.Slashing,
			ExperienceValue = 50,
			GoldValue = 15,
			LootTable = [
				new LootEntry(Ids.Items.Materials.Bone, 0.7f),
				new LootEntry(Ids.Armor.Medium.ChainMail, 0.1f)
			],
			AIType = EnemyAIType.Defensive,
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Piercing, 0.5f },
				{ DamageType.Poison, 1f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Bludgeoning, 0.5f },
				{ DamageType.Holy, 0.5f }
			},
			Immunities = [StatusCondition.Poisoned, StatusCondition.Frightened],
			ChallengeRating = 1.5f,
			SpawnWeight = 6f,
			NightOnly = true,
			MinSpawnDistance = 10f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Underground.Cave]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Undead.Zombie) {
			Name = "Zombie",
			Description = "A shambling corpse driven by dark hunger.",
			PortraitName = "enemy_zombie",
			Category = EnemyCategory.Undead,
			MaxHealth = 25,
			ArmorClass = 8,
			AttackBonus = 2,
			DamageBonus = 2,
			InitiativeBonus = -2,
			AttackDamage = HitDice.D6,
			DamageType = DamageType.Bludgeoning,
			ExperienceValue = 30,
			GoldValue = 3,
			LootTable = [],
			AIType = EnemyAIType.Aggressive,
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Poison, 1f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Fire, 0.5f },
				{ DamageType.Holy, 0.5f }
			},
			Immunities = [StatusCondition.Poisoned, StatusCondition.Frightened],
			ChallengeRating = 0.75f,
			SpawnWeight = 10f,
			NightOnly = true,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Wetlands.Swamp]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Undead.ZombieBrute) {
			Name = "Zombie Brute",
			Description = "A massive, bloated zombie.",
			PortraitName = "enemy_zombie_brute",
			Category = EnemyCategory.Undead,
			MaxHealth = 50,
			ArmorClass = 8,
			AttackBonus = 4,
			DamageBonus = 4,
			InitiativeBonus = -3,
			AttackDamage = new HitDice(2, 6),
			DamageType = DamageType.Bludgeoning,
			ExperienceValue = 75,
			GoldValue = 5,
			LootTable = [],
			AIType = EnemyAIType.Aggressive,
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Poison, 1f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Fire, 0.5f },
				{ DamageType.Holy, 0.5f }
			},
			Immunities = [StatusCondition.Poisoned, StatusCondition.Frightened],
			ChallengeRating = 2f,
			SpawnWeight = 5f,
			NightOnly = true,
			MinSpawnDistance = 10f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Wetlands.Swamp]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Undead.Ghost) {
			Name = "Ghost",
			Description = "A restless spirit bound to this world.",
			PortraitName = "enemy_ghost",
			Category = EnemyCategory.Undead,
			MaxHealth = 30,
			ArmorClass = 12,
			AttackBonus = 4,
			DamageBonus = 0,
			InitiativeBonus = 3,
			AttackDamage = new HitDice(2, 6),
			DamageType = DamageType.Necrotic,
			ExperienceValue = 100,
			GoldValue = 0,
			LootTable = [
				new LootEntry(Ids.Items.Materials.Ectoplasm, 0.4f)
			],
			AIType = EnemyAIType.Tactical,
			Abilities = [
				new EnemyAbility {
					Id = "horrifying_visage",
					Name = "Horrifying Visage",
					Type = EnemyAbilityType.Debuff,
					TargetType = TargetType.AllEnemies,
					Cooldown = 4,
					Effects = [new AbilityEffect { AppliesCondition = StatusCondition.Frightened, ConditionDuration = 2.Turns() }]
				}
			],
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Physical, 0.5f },
				{ DamageType.Poison, 1f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Holy, 0.5f }
			},
			Immunities = [StatusCondition.Poisoned, StatusCondition.Frightened, StatusCondition.Stunned],
			ChallengeRating = 3f,
			SpawnWeight = 3f,
			NightOnly = true,
			MinSpawnDistance = 15f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Undead.Wraith) {
			Name = "Wraith",
			Description = "A malevolent spirit of pure hatred.",
			PortraitName = "enemy_wraith",
			Category = EnemyCategory.Undead,
			MaxHealth = 50,
			ArmorClass = 14,
			AttackBonus = 6,
			DamageBonus = 0,
			InitiativeBonus = 4,
			AttackDamage = new HitDice(2, 8),
			DamageType = DamageType.Necrotic,
			ExperienceValue = 200,
			GoldValue = 0,
			LootTable = [
				new LootEntry(Ids.Items.Materials.ShadowEssence, 0.5f),
				new LootEntry(Ids.Items.Materials.SoulFragment, 0.2f)
			],
			AIType = EnemyAIType.Tactical,
			Abilities = [
				new EnemyAbility {
					Id = "life_drain",
					Name = "Life Drain",
					Type = EnemyAbilityType.Attack,
					TargetType = TargetType.SingleEnemy,
					DamageDice = new HitDice(3, 6),
					DamageType = DamageType.Necrotic,
					Cooldown = 2,
					Effects = [new AbilityEffect { HealAmount = 10 }]
				}
			],
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Physical, 0.75f },
				{ DamageType.Poison, 1f },
				{ DamageType.Cold, 0.5f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Holy, 0.5f },
				{ DamageType.Fire, 0.25f }
			},
			Immunities = [StatusCondition.Poisoned, StatusCondition.Frightened, StatusCondition.Stunned, StatusCondition.Frozen],
			ChallengeRating = 5f,
			SpawnWeight = 1f,
			NightOnly = true,
			MinSpawnDistance = 25f
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Undead.Spectre) {
			Name = "Spectre",
			Description = "A spirit consumed by vengeance.",
			PortraitName = "enemy_spectre",
			Category = EnemyCategory.Undead,
			MaxHealth = 40,
			ArmorClass = 13,
			AttackBonus = 5,
			DamageBonus = 0,
			InitiativeBonus = 5,
			AttackDamage = new HitDice(2, 8),
			DamageType = DamageType.Necrotic,
			ExperienceValue = 150,
			GoldValue = 0,
			LootTable = [
				new LootEntry(Ids.Items.Materials.Ectoplasm, 0.6f),
				new LootEntry(Ids.Items.Materials.ShadowEssence, 0.3f)
			],
			AIType = EnemyAIType.Aggressive,
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Physical, 0.75f },
				{ DamageType.Poison, 1f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Holy, 0.5f }
			},
			Immunities = [StatusCondition.Poisoned, StatusCondition.Frightened],
			ChallengeRating = 4f,
			SpawnWeight = 2f,
			NightOnly = true,
			MinSpawnDistance = 20f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins]
		});
	}

	#endregion

	#region Demons

	private void RegisterDemons(GameDb db) {
		db.RegisterProto(new EnemyProto(Ids.Enemies.Demons.Imp) {
			Name = "Imp",
			Description = "A small, mischievous demon.",
			PortraitName = "enemy_imp",
			Category = EnemyCategory.Demon,
			MaxHealth = 15,
			ArmorClass = 13,
			AttackBonus = 4,
			DamageBonus = 1,
			InitiativeBonus = 3,
			AttackDamage = HitDice.D4,
			DamageType = DamageType.Piercing,
			ExperienceValue = 40,
			GoldValue = 10,
			LootTable = [
				new LootEntry(Ids.Items.Materials.DemonHorn, 0.3f)
			],
			AIType = EnemyAIType.Tactical,
			Abilities = [
				new EnemyAbility {
					Id = "fire_bolt",
					Name = "Fire Bolt",
					Type = EnemyAbilityType.Spell,
					TargetType = TargetType.SingleEnemy,
					DamageDice = HitDice.D6,
					DamageType = DamageType.Fire
				}
			],
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Fire, 1f },
				{ DamageType.Poison, 0.5f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Holy, 0.5f },
				{ DamageType.Cold, 0.25f }
			},
			ChallengeRating = 1f,
			SpawnWeight = 5f,
			MinSpawnDistance = 20f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Volcanic.VolcanicPlain]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Demons.Hellhound) {
			Name = "Hellhound",
			Description = "A fiery hound from the infernal planes.",
			PortraitName = "enemy_hellhound",
			Category = EnemyCategory.Demon,
			MaxHealth = 40,
			ArmorClass = 15,
			AttackBonus = 5,
			DamageBonus = 3,
			InitiativeBonus = 2,
			AttackDamage = new HitDice(1, 8, 2),
			DamageType = DamageType.Fire,
			ExperienceValue = 120,
			GoldValue = 0,
			LootTable = [
				new LootEntry(Ids.Items.Materials.HellhoundFang, 0.4f),
				new LootEntry(Ids.Items.Materials.InfernalAsh, 0.3f)
			],
			AIType = EnemyAIType.Aggressive,
			Abilities = [
				new EnemyAbility {
					Id = "fire_breath",
					Name = "Fire Breath",
					Type = EnemyAbilityType.Attack,
					TargetType = TargetType.AllEnemies,
					DamageDice = new HitDice(2, 6),
					DamageType = DamageType.Fire,
					Cooldown = 3
				}
			],
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Fire, 1f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Cold, 0.5f }
			},
			ChallengeRating = 3f,
			SpawnWeight = 3f,
			MinSpawnDistance = 25f,
			ValidTerrains = [Ids.Terrains.Volcanic.VolcanicPlain, Ids.Terrains.Desert.Badlands]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Demons.LesserDemon) {
			Name = "Lesser Demon",
			Description = "A minor demon from the lower planes.",
			PortraitName = "enemy_lesser_demon",
			Category = EnemyCategory.Demon,
			MaxHealth = 55,
			ArmorClass = 14,
			AttackBonus = 5,
			DamageBonus = 3,
			InitiativeBonus = 2,
			AttackDamage = new HitDice(2, 6),
			DamageType = DamageType.Slashing,
			ExperienceValue = 150,
			GoldValue = 25,
			LootTable = [
				new LootEntry(Ids.Items.Materials.DemonHorn, 0.5f),
				new LootEntry(Ids.Items.Materials.InfernalAsh, 0.4f)
			],
			AIType = EnemyAIType.Aggressive,
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Fire, 0.5f },
				{ DamageType.Poison, 0.5f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Holy, 0.5f }
			},
			ChallengeRating = 4f,
			SpawnWeight = 2f,
			MinSpawnDistance = 30f,
			ValidTerrains = [Ids.Terrains.Volcanic.VolcanicPlain, Ids.Terrains.Ruins.OpenRuins]
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Demons.ShadowFiend) {
			Name = "Shadow Fiend",
			Description = "A demon made of living shadow.",
			PortraitName = "enemy_shadow_fiend",
			Category = EnemyCategory.Demon,
			MaxHealth = 45,
			ArmorClass = 15,
			AttackBonus = 6,
			DamageBonus = 2,
			InitiativeBonus = 4,
			AttackDamage = new HitDice(2, 6),
			DamageType = DamageType.Necrotic,
			ExperienceValue = 175,
			GoldValue = 0,
			LootTable = [
				new LootEntry(Ids.Items.Materials.ShadowEssence, 0.6f),
				new LootEntry(Ids.Items.Materials.SoulFragment, 0.3f)
			],
			AIType = EnemyAIType.Tactical,
			Abilities = [
				new EnemyAbility {
					Id = "shadow_step",
					Name = "Shadow Step",
					Description = "Teleports through shadows.",
					Type = EnemyAbilityType.Utility,
					Cooldown = 2
				},
				new EnemyAbility {
					Id = "life_drain",
					Name = "Life Drain",
					Type = EnemyAbilityType.Attack,
					TargetType = TargetType.SingleEnemy,
					DamageDice = new HitDice(2, 8),
					DamageType = DamageType.Necrotic,
					Cooldown = 3,
					Effects = [new AbilityEffect { HealAmount = 8 }]
				}
			],
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Necrotic, 0.75f },
				{ DamageType.Poison, 0.5f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Holy, 0.5f },
				{ DamageType.Fire, 0.25f }
			},
			ChallengeRating = 5f,
			SpawnWeight = 1f,
			NightOnly = true,
			MinSpawnDistance = 30f
		});
	}

	#endregion

	#region Bosses

	private void RegisterBosses(GameDb db) {
		db.RegisterProto(new EnemyProto(Ids.Enemies.Bosses.BanditKing) {
			Name = "The Bandit King",
			Description = "A ruthless warlord who commands all the bandits in the region.",
			PortraitName = "enemy_bandit_king",
			Category = EnemyCategory.Humanoid,
			IsBoss = true,
			MaxHealth = 150,
			ArmorClass = 17,
			AttackBonus = 7,
			DamageBonus = 5,
			InitiativeBonus = 3,
			AttackDamage = new HitDice(2, 8, 3),
			DamageType = DamageType.Slashing,
			ExperienceValue = 500,
			GoldValue = 300,
			LootTable = [
				new LootEntry(Ids.Items.Quest.BanditKingCrown, 1f),
				new LootEntry(Ids.Weapons.Swords.SteelSword, 0.5f),
				new LootEntry(Ids.Items.Consumables.HealthPotionLarge, 0.8f)
			],
			AIType = EnemyAIType.Boss,
			Abilities = [
				new EnemyAbility {
					Id = "commanding_strike",
					Name = "Commanding Strike",
					Type = EnemyAbilityType.Attack,
					TargetType = TargetType.SingleEnemy,
					DamageDice = new HitDice(3, 8),
					DamageType = DamageType.Slashing,
					Cooldown = 3
				},
				new EnemyAbility {
					Id = "rally_troops",
					Name = "Rally Troops",
					Description = "Inspires all allies, granting bonus damage.",
					Type = EnemyAbilityType.Buff,
					TargetType = TargetType.AllAllies,
					Cooldown = 5,
					Effects = [new AbilityEffect { BuffId = "inspired" }]
				}
			],
			ChallengeRating = 8f
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Bosses.Lich) {
			Name = "The Forgotten Lich",
			Description = "An ancient sorcerer who defied death itself.",
			PortraitName = "enemy_lich",
			Category = EnemyCategory.Undead,
			IsBoss = true,
			MaxHealth = 120,
			ArmorClass = 15,
			AttackBonus = 5,
			DamageBonus = 0,
			InitiativeBonus = 4,
			AttackDamage = HitDice.D4,
			DamageType = DamageType.Necrotic,
			ExperienceValue = 750,
			GoldValue = 200,
			LootTable = [
				new LootEntry(Ids.Items.Quest.LichPhylacteryShard, 1f),
				new LootEntry(Ids.Weapons.Staves.ArcaneStaff, 0.4f),
				new LootEntry(Ids.Items.Quest.NecronomiconPage, 0.6f)
			],
			AIType = EnemyAIType.Boss,
			Abilities = [
				new EnemyAbility {
					Id = "soul_rend",
					Name = "Soul Rend",
					Type = EnemyAbilityType.Spell,
					TargetType = TargetType.SingleEnemy,
					DamageDice = new HitDice(4, 6),
					DamageType = DamageType.Necrotic,
					Cooldown = 2
				},
				new EnemyAbility {
					Id = "raise_dead",
					Name = "Raise Dead",
					Description = "Summons skeleton minions.",
					Type = EnemyAbilityType.Utility,
					Cooldown = 4
				},
				new EnemyAbility {
					Id = "dark_shield",
					Name = "Dark Shield",
					Type = EnemyAbilityType.Buff,
					TargetType = TargetType.Self,
					Cooldown = 5,
					Effects = [new AbilityEffect { AppliesCondition = StatusCondition.Shielded, ConditionDuration = 3.Turns() }]
				}
			],
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Necrotic, 1f },
				{ DamageType.Poison, 1f },
				{ DamageType.Cold, 0.5f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Holy, 0.5f },
				{ DamageType.Fire, 0.25f }
			},
			Immunities = [StatusCondition.Poisoned, StatusCondition.Frightened, StatusCondition.Stunned],
			ChallengeRating = 12f
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Bosses.FogHerald) {
			Name = "Herald of the Fog",
			Description = "A being born from the creeping fog itself. Its presence warps time.",
			PortraitName = "enemy_fog_herald",
			Category = EnemyCategory.Aberration,
			IsBoss = true,
			MaxHealth = 200,
			ArmorClass = 18,
			AttackBonus = 8,
			DamageBonus = 4,
			InitiativeBonus = 6,
			AttackDamage = new HitDice(2, 10),
			DamageType = DamageType.Necrotic,
			ExperienceValue = 1500,
			GoldValue = 500,
			LootTable = [
				new LootEntry(Ids.Items.Materials.FogEssence, 1f),
				new LootEntry(Ids.Items.Materials.TimeShard, 0.8f),
				new LootEntry(Ids.Weapons.Swords.TemporalBlade, 0.2f)
			],
			AIType = EnemyAIType.Boss,
			Abilities = [
				new EnemyAbility {
					Id = "temporal_rend",
					Name = "Temporal Rend",
					Description = "Tears at the fabric of time itself.",
					Type = EnemyAbilityType.Attack,
					TargetType = TargetType.SingleEnemy,
					DamageDice = new HitDice(4, 8),
					DamageType = DamageType.Arcane,
					Cooldown = 2
				},
				new EnemyAbility {
					Id = "time_stop",
					Name = "Time Stop",
					Description = "Freezes all enemies in time.",
					Type = EnemyAbilityType.Debuff,
					TargetType = TargetType.AllEnemies,
					Cooldown = 5,
					Effects = [new AbilityEffect { AppliesCondition = StatusCondition.Frozen, ConditionDuration = 1.Turns() }]
				},
				new EnemyAbility {
					Id = "fog_embrace",
					Name = "Fog Embrace",
					Description = "Envelops the battlefield in fog.",
					Type = EnemyAbilityType.Debuff,
					TargetType = TargetType.AllEnemies,
					Cooldown = 4,
					Effects = [new AbilityEffect { AppliesCondition = StatusCondition.Blinded, ConditionDuration = 2.Turns() }]
				}
			],
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Arcane, 0.75f },
				{ DamageType.Cold, 0.5f },
				{ DamageType.Necrotic, 0.5f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Holy, 0.25f }
			},
			Immunities = [StatusCondition.Frozen, StatusCondition.Slowed],
			ChallengeRating = 20f
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Bosses.DemonLord) {
			Name = "Demon Lord Azgoroth",
			Description = "A mighty demon lord who seeks to consume all mortal souls.",
			PortraitName = "enemy_demon_lord",
			Category = EnemyCategory.Demon,
			IsBoss = true,
			MaxHealth = 250,
			ArmorClass = 19,
			AttackBonus = 10,
			DamageBonus = 6,
			InitiativeBonus = 4,
			AttackDamage = new HitDice(3, 8, 4),
			DamageType = DamageType.Fire,
			ExperienceValue = 2000,
			GoldValue = 750,
			LootTable = [
				new LootEntry(Ids.Items.Materials.DemonHorn, 1f, 2, 4),
				new LootEntry(Ids.Items.Materials.InfernalAsh, 1f, 3, 5),
				new LootEntry(Ids.Items.Materials.SoulFragment, 0.6f, 1, 2)
			],
			AIType = EnemyAIType.Boss,
			Abilities = [
				new EnemyAbility {
					Id = "hellfire",
					Name = "Hellfire",
					Description = "Unleashes a wave of infernal fire.",
					Type = EnemyAbilityType.Attack,
					TargetType = TargetType.AllEnemies,
					DamageDice = new HitDice(4, 6),
					DamageType = DamageType.Fire,
					Cooldown = 3
				},
				new EnemyAbility {
					Id = "soul_rip",
					Name = "Soul Rip",
					Description = "Tears at the very essence of a target.",
					Type = EnemyAbilityType.Attack,
					TargetType = TargetType.SingleEnemy,
					DamageDice = new HitDice(5, 8),
					DamageType = DamageType.Necrotic,
					Cooldown = 4,
					Effects = [new AbilityEffect { HealAmount = 20 }]
				},
				new EnemyAbility {
					Id = "summon_imps",
					Name = "Summon Imps",
					Description = "Calls forth imp minions.",
					Type = EnemyAbilityType.Utility,
					Cooldown = 5
				}
			],
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Fire, 1f },
				{ DamageType.Poison, 1f },
				{ DamageType.Necrotic, 0.5f }
			},
			Vulnerabilities = new Dictionary<DamageType, float> {
				{ DamageType.Holy, 0.5f },
				{ DamageType.Cold, 0.25f }
			},
			Immunities = [StatusCondition.Frightened, StatusCondition.Poisoned, StatusCondition.Charmed],
			ChallengeRating = 25f
		});

		db.RegisterProto(new EnemyProto(Ids.Enemies.Bosses.AncientDragon) {
			Name = "Valdros the Ancient",
			Description = "An ancient dragon who has slumbered for centuries beneath the mountains.",
			PortraitName = "enemy_ancient_dragon",
			Category = EnemyCategory.Beast,
			IsBoss = true,
			MaxHealth = 350,
			ArmorClass = 21,
			AttackBonus = 12,
			DamageBonus = 8,
			InitiativeBonus = 2,
			AttackDamage = new HitDice(4, 10, 6),
			DamageType = DamageType.Slashing,
			ExperienceValue = 3000,
			GoldValue = 1500,
			LootTable = [
				new LootEntry(Ids.Items.Materials.TimeShard, 0.5f),
				new LootEntry(Ids.Items.Materials.GemRuby, 1f, 2, 5)
			],
			AIType = EnemyAIType.Boss,
			Abilities = [
				new EnemyAbility {
					Id = "fire_breath",
					Name = "Dragon's Breath",
					Description = "Unleashes a devastating cone of fire.",
					Type = EnemyAbilityType.Attack,
					TargetType = TargetType.AllEnemies,
					DamageDice = new HitDice(6, 10),
					DamageType = DamageType.Fire,
					Cooldown = 4
				},
				new EnemyAbility {
					Id = "tail_sweep",
					Name = "Tail Sweep",
					Description = "Sweeps all enemies with its massive tail.",
					Type = EnemyAbilityType.Attack,
					TargetType = TargetType.AllEnemies,
					DamageDice = new HitDice(3, 8),
					DamageType = DamageType.Bludgeoning,
					Cooldown = 2,
					Effects = [new AbilityEffect { AppliesCondition = StatusCondition.Stunned, ConditionDuration = 1.Turns() }]
				},
				new EnemyAbility {
					Id = "frightful_presence",
					Name = "Frightful Presence",
					Description = "The dragon's terrifying presence shakes all who behold it.",
					Type = EnemyAbilityType.Debuff,
					TargetType = TargetType.AllEnemies,
					Cooldown = 6,
					Effects = [new AbilityEffect { AppliesCondition = StatusCondition.Frightened, ConditionDuration = 3.Turns() }]
				}
			],
			Resistances = new Dictionary<DamageType, float> {
				{ DamageType.Fire, 1f },
				{ DamageType.Physical, 0.25f }
			},
			Immunities = [StatusCondition.Frightened, StatusCondition.Stunned, StatusCondition.Charmed],
			ChallengeRating = 30f
		});
	}

	#endregion
}