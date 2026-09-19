using RPGGame.Core.Effects;
using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Activities;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Item;
using RPGGame.Core.Prototypes.Item.Equipment;
using RPGGame.Core.Prototypes.Item.Equipment.Armor;
using RPGGame.Core.Prototypes.Item.Equipment.Weapon;
using RPGGame.Core.Prototypes.Item.Resource;
using RPGGame.Core.Prototypes.Locations;
using RPGGame.Core.Prototypes.Lore;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Prototypes.Village;

namespace RPGGame.Core;

/// <summary>
/// Partial class containing  E n e m i e s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    
	// ═══════════════════════════════════════════════════════════════════════
	// ENEMIES
	// ═══════════════════════════════════════════════════════════════════════

	public static class Enemies {
		public static class Beasts {
			public static readonly EnemyProto.ID Wolf = newId("Wolf");
			public static readonly EnemyProto.ID DireWolf = newId("DireWolf");
			public static readonly EnemyProto.ID Bear = newId("Bear");
			public static readonly EnemyProto.ID GiantSpider = newId("GiantSpider");
			public static readonly EnemyProto.ID GiantRat = newId("GiantRat");
			public static readonly EnemyProto.ID Boar = newId("Boar");
			public static readonly EnemyProto.ID GiantBat = newId("GiantBat");
		}

		public static class Humanoids {
			public static readonly EnemyProto.ID Bandit = newId("Bandit");
			public static readonly EnemyProto.ID BanditArcher = newId("BanditArcher");
			public static readonly EnemyProto.ID BanditChief = newId("BanditChief");
			public static readonly EnemyProto.ID Cultist = newId("Cultist");
			public static readonly EnemyProto.ID CultistMage = newId("CultistMage");
			public static readonly EnemyProto.ID Goblin = newId("Goblin");
			public static readonly EnemyProto.ID GoblinShaman = newId("GoblinShaman");
			public static readonly EnemyProto.ID OrcWarrior = newId("OrcWarrior");
			public static readonly EnemyProto.ID OrcBerserker = newId("OrcBerserker");
		}

		public static class Undead {
			public static readonly EnemyProto.ID Skeleton = newId("Skeleton");
			public static readonly EnemyProto.ID SkeletonArcher = newId("SkeletonArcher");
			public static readonly EnemyProto.ID SkeletonWarrior = newId("SkeletonWarrior");
			public static readonly EnemyProto.ID Zombie = newId("Zombie");
			public static readonly EnemyProto.ID ZombieBrute = newId("ZombieBrute");
			public static readonly EnemyProto.ID Ghost = newId("Ghost");
			public static readonly EnemyProto.ID Wraith = newId("Wraith");
			public static readonly EnemyProto.ID Spectre = newId("Spectre");
		}

		public static class Demons {
			public static readonly EnemyProto.ID Imp = newId("Imp");
			public static readonly EnemyProto.ID Hellhound = newId("Hellhound");
			public static readonly EnemyProto.ID LesserDemon = newId("LesserDemon");
			public static readonly EnemyProto.ID ShadowFiend = newId("ShadowFiend");
		}

		public static class Aberrations {
			public static readonly EnemyProto.ID FogCreeper = newId("FogCreeper");
			public static readonly EnemyProto.ID FogWraith = newId("FogWraith");
			public static readonly EnemyProto.ID TimeLost = newId("TimeLost");
		}

		public static class Bosses {
			public static readonly EnemyProto.ID BanditKing = newId("Boss_BanditKing");
			public static readonly EnemyProto.ID Lich = newId("Boss_Lich");
			public static readonly EnemyProto.ID FogHerald = newId("Boss_FogHerald");
			public static readonly EnemyProto.ID DemonLord = newId("Boss_DemonLord");
			public static readonly EnemyProto.ID AncientDragon = newId("Boss_AncientDragon");
		}

		private static EnemyProto.ID newId(string name) {
			return new EnemyProto.ID($"Enemy_{name}");
		}
	}
}
