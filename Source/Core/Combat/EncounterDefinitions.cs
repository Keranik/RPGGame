using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Prototypes.Events;

namespace RPGGame.Core.Combat;

/// <summary>
/// Defines preset combat encounters using the Proto system.
/// </summary>
public class EncounterDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterRandomEncounters(gameDatabase);
		RegisterStoryEncounters(gameDatabase);
		RegisterBossEncounters(gameDatabase);
	}

	#region Random Encounters

	private void RegisterRandomEncounters(GameDb db) {
		// ═══════════════════════════════════════════════════════════════
		// EARLY GAME
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.Wolves,
			text: Proto.CreateText("Wolf Pack", "A pack of hungry wolves blocks your path."),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Beasts.Wolf, 2, 4)
			],
			bonusExperience: 10,
			musicId: "music_combat_wilderness"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.BanditsSmall,
			text: Proto.CreateText("Bandit Ambush", "Bandits leap from hiding!"),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Humanoids.Bandit, 2, 3)
			],
			bonusExperience: 15,
			bonusGold: 10,
			musicId: "music_combat_bandits"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.Rats,
			text: Proto.CreateText("Rat Infestation", "Giant rats swarm from the shadows."),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Beasts.GiantRat, 3, 6)
			],
			bonusExperience: 5,
			musicId: "music_combat_dungeon"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.Goblins,
			text: Proto.CreateText("Goblin Raiders", "A group of goblins blocks the path."),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Humanoids.Goblin, 3, 5)
			],
			bonusExperience: 10,
			bonusGold: 15,
			musicId: "music_combat_goblins"
		));

		// ═══════════════════════════════════════════════════════════════
		// MID GAME
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.BanditsMixed,
			text: Proto.CreateText("Bandit Gang", "A well-organized bandit gang attacks!"),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Humanoids.Bandit, 2, 3),
				new EncounterEnemy(Ids.Enemies.Humanoids.BanditArcher, 1, 2)
			],
			difficultyModifier: 1.1f,
			bonusExperience: 30,
			bonusGold: 25,
			musicId: "music_combat_bandits"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.SpiderNest,
			text: Proto.CreateText("Spider Nest", "You've stumbled into a giant spider nest!"),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Beasts.GiantSpider, 2, 4)
			],
			bonusExperience: 25,
			guaranteedLoot: [
				new LootEntry(Ids.Items.Materials.SpiderSilk, 1f, 2, 4)
			],
			musicId: "music_combat_dungeon"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.OrcPatrol,
			text: Proto.CreateText("Orc Patrol", "An orc war party spots you!"),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Humanoids.OrcWarrior, 2, 3)
			],
			difficultyModifier: 1.2f,
			bonusExperience: 50,
			bonusGold: 30,
			musicId: "music_combat_orcs"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.UndeadRising,
			text: Proto.CreateText("The Dead Rise", "The ground trembles as the dead claw their way up."),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Undead.Skeleton, 2, 4),
				new EncounterEnemy(Ids.Enemies.Undead.Zombie, 1, 2)
			],
			bonusExperience: 40,
			musicId: "music_combat_undead"
		));

		// ═══════════════════════════════════════════════════════════════
		// LATE GAME
		// ═══════════════════════════════════════════════════════════════
		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.CultistRitual,
			text: Proto.CreateText("Cultist Ritual", "You interrupt a dark ritual!"),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Humanoids.Cultist, 3, 5),
				new EncounterEnemy(Ids.Enemies.Demons.Imp, 1, 2)
			],
			difficultyModifier: 1.3f,
			bonusExperience: 75,
			bonusGold: 50,
			musicId: "music_combat_cult"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.Haunted,
			text: Proto.CreateText("Haunted Grounds", "Spirits materialize around you!"),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Undead.Ghost, 1, 2),
				new EncounterEnemy(Ids.Enemies.Undead.Skeleton, 2, 3)
			],
			canFlee: false,
			bonusExperience: 80,
			musicId: "music_combat_undead"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Random.Hellhounds,
			text: Proto.CreateText("Infernal Hunters", "Hellhounds emerge from fiery portals!"),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Demons.Hellhound, 2, 3)
			],
			difficultyModifier: 1.4f,
			bonusExperience: 100,
			musicId: "music_combat_demon"
		));
	}

	#endregion

	#region Story Encounters

	private void RegisterStoryEncounters(GameDb db) {
		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Story.IntroWolves,
			text: Proto.CreateText("First Blood", "Wolves have been stalking you since you left the village."),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Beasts.Wolf, 2)
			],
			difficultyModifier: 0.1f,
			bonusExperience: 0,
			onVictoryEvent: Ids.Events.Intro.FirstCombatComplete,
			musicId: "music_combat_wilderness"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Story.BanditCamp,
			text: Proto.CreateText("Bandit Camp Assault", "You've found the bandit hideout. Time to clear them out."),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Humanoids.Bandit, 3, 4),
				new EncounterEnemy(Ids.Enemies.Humanoids.BanditArcher, 2),
				new EncounterEnemy(Ids.Enemies.Humanoids.BanditChief, 1)
			],
			canFlee: false,
			bonusExperience: 100,
			bonusGold: 100,
			onVictoryEvent: Ids.Events.Story.BanditCampCleared,
			musicId: "music_combat_bandits"
		));

		db.RegisterProto(new EncounterProto(
	id: Ids.Encounters.Random.Bear,
	text: Proto.CreateText("Angry Bear", "A territorial bear attacks!"),
	enemies: [
		new EncounterEnemy(Ids.Enemies.Beasts.Bear, 1)
	],
	difficultyModifier: 1.2f,
	bonusExperience: 40,
	guaranteedLoot: [
		new LootEntry(Ids.Items.Materials.BearPelt, 0.8f),
		new LootEntry(Ids.Items.Materials.BearMeat, 1f, 1, 2)
	],
	musicId: "music_combat_wilderness"
));

db.RegisterProto(new EncounterProto(
	id: Ids.Encounters.Random.WildBeast,
	text: Proto.CreateText("Wild Beast", "A wild creature charges at you!"),
	enemies: [
		new EncounterEnemy(Ids.Enemies.Beasts.Boar, 1, 2)
	],
	bonusExperience: 15,
	musicId: "music_combat_wilderness"
));

db.RegisterProto(new EncounterProto(
	id: Ids.Encounters.Random.WolfPack,
	text: Proto.CreateText("Wolf Pack", "Wolves surround you in the darkness."),
	enemies: [
		new EncounterEnemy(Ids.Enemies.Beasts.Wolf, 3, 5)
	],
	bonusExperience: 25,
	guaranteedLoot: [
		new LootEntry(Ids.Items.Materials.WolfPelt, 0.5f, 1, 2)
	],
	musicId: "music_combat_wilderness"
));

db.RegisterProto(new EncounterProto(
	id: Ids.Encounters.Random.GiantSpiders,
	text: Proto.CreateText("Giant Spiders", "Spiders descend from the trees!"),
	enemies: [
		new EncounterEnemy(Ids.Enemies.Beasts.GiantSpider, 2, 4)
	],
	bonusExperience: 30,
	guaranteedLoot: [
		new LootEntry(Ids.Items.Materials.SpiderSilk, 0.7f, 1, 3)
	],
	musicId: "music_combat_dungeon"
));

db.RegisterProto(new EncounterProto(
	id: Ids.Encounters.Random.WildBoar,
	text: Proto.CreateText("Charging Boar", "An enraged boar charges!"),
	enemies: [
		new EncounterEnemy(Ids.Enemies.Beasts.Boar, 1)
	],
	bonusExperience: 15,
	guaranteedLoot: [
		new LootEntry(Ids.Items.Materials.BoarMeat, 1f),
		new LootEntry(Ids.Items.Materials.BoarTusk, 0.5f)
	],
	musicId: "music_combat_wilderness"
));

db.RegisterProto(new EncounterProto(
	id: Ids.Encounters.Random.SwampCreature,
	text: Proto.CreateText("Swamp Horror", "Something rises from the murky water!"),
	enemies: [
		new EncounterEnemy(Ids.Enemies.Aberrations.FogCreeper, 1, 2)
	],
	difficultyModifier: 1.3f,
	bonusExperience: 45,
	musicId: "music_combat_fog"
));

db.RegisterProto(new EncounterProto(
	id: Ids.Encounters.Random.Wyvern,
	text: Proto.CreateText("Wyvern Attack", "A wyvern swoops down from above!"),
	enemies: [
		new EncounterEnemy(Ids.Enemies.Beasts.GiantBat, 1) // Using GiantBat as proxy until Wyvern enemy is added
	],
	difficultyModifier: 1.5f,
	bonusExperience: 75,
	musicId: "music_combat_wilderness"
));

db.RegisterProto(new EncounterProto(
	id: Ids.Encounters.Random.OreGuardian,
	text: Proto.CreateText("Ore Guardian", "An elemental guardian protects the ore!"),
	enemies: [
		new EncounterEnemy(Ids.Enemies.Aberrations.FogCreeper, 1) // Using FogCreeper as proxy
	],
	difficultyModifier: 1.4f,
	bonusExperience: 50,
	guaranteedLoot: [
		new LootEntry(Ids.Items.Materials.MithrilOre, 0.5f, 1, 2),
		new LootEntry(Ids.Items.Materials.IronOre, 1f, 2, 4)
	],
	musicId: "music_combat_dungeon"
));

db.RegisterProto(new EncounterProto(
	id: Ids.Encounters.Random.Undead,
	text: Proto.CreateText("Risen Dead", "The dead rise to attack!"),
	enemies: [
		new EncounterEnemy(Ids.Enemies.Undead.Skeleton, 1, 3),
		new EncounterEnemy(Ids.Enemies.Undead.Zombie, 0, 2)
	],
	bonusExperience: 35,
	guaranteedLoot: [
		new LootEntry(Ids.Items.Materials.Bone, 0.8f, 1, 3)
	],
	musicId: "music_combat_undead"
));

db.RegisterProto(new EncounterProto(
	id: Ids.Encounters.Random.TrapdoorSpider,
	text: Proto.CreateText("Trapdoor Spider", "A massive spider lunges from its hidden lair!"),
	enemies: [
		new EncounterEnemy(Ids.Enemies.Beasts.GiantSpider, 1)
	],
	difficultyModifier: 1.1f,
	bonusExperience: 25,
	guaranteedLoot: [
		new LootEntry(Ids.Items.Materials.SpiderSilk, 1f, 1, 2),
		new LootEntry(Ids.Items.Materials.VenomSac, 0.4f)
	],
	musicId: "music_combat_dungeon"
));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Story.FogCreatures,
			text: Proto.CreateText("Children of the Fog", "Twisted creatures emerge from the encroaching fog."),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Undead.Wraith, 1, 2),
				new EncounterEnemy(Ids.Enemies.Undead.Ghost, 2, 3)
			],
			difficultyModifier: 1.5f,
			canFlee: false,
			bonusExperience: 150,
			musicId: "music_combat_fog"
		));
	}

	#endregion

	#region Boss Encounters

	private void RegisterBossEncounters(GameDb db) {
		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Bosses.BanditKing,
			text: Proto.CreateText("The Bandit King's Throne", "The Bandit King sits upon his ill-gotten throne, surrounded by his elite guard."),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Bosses.BanditKing, 1),
				new EncounterEnemy(Ids.Enemies.Humanoids.BanditChief, 1, 2)
			],
			isBoss: true,
			canFlee: false,
			bonusExperience: 300,
			bonusGold: 200,
			guaranteedLoot: [
				new LootEntry(Ids.Items.Quest.BanditKingKey, 1f)
			],
			musicId: "music_boss_bandit"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Bosses.Lich,
			text: Proto.CreateText("The Forgotten Tomb", "In the depths of the ancient tomb, the Lich awaits."),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Bosses.Lich, 1),
				new EncounterEnemy(Ids.Enemies.Undead.Skeleton, 2, 4)
			],
			isBoss: true,
			canFlee: false,
			bonusExperience: 500,
			bonusGold: 150,
			onVictoryEvent: Ids.Events.Story.LichDefeated,
			musicId: "music_boss_lich"
		));

		db.RegisterProto(new EncounterProto(
			id: Ids.Encounters.Bosses.FogHerald,
			text: Proto.CreateText("The Herald Awakens", "At the source of the fog, a terrible being manifests. This is what you came for."),
			enemies: [
				new EncounterEnemy(Ids.Enemies.Bosses.FogHerald, 1)
			],
			isBoss: true,
			canFlee: false,
			bonusExperience: 1000,
			bonusGold: 500,
			onVictoryEvent: Ids.Events.Story.FogHeraldDefeated,
			onDefeatEvent: Ids.Events.Story.CaughtInTime,
			musicId: "music_boss_final",
			backgroundId: "bg_fog_source"
		));
	}

	#endregion
}