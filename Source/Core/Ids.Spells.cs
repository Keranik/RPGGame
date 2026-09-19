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
/// Partial class containing  S p e l l s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    	// ═══════════════════════════════════════════════════════════════════════
	// SPELLS
	// ═══════════════════════════════════════════════════════════════════════

	public static class Spells {
		public static class Cantrips {
			public static readonly SpellProto.ID MagicMissile = newId("MagicMissile");
			public static readonly SpellProto.ID FireBolt = newId("FireBolt");
			public static readonly SpellProto.ID RayOfFrost = newId("RayOfFrost");
			public static readonly SpellProto.ID SacredFlame = newId("SacredFlame");
			public static readonly SpellProto.ID MinorHeal = newId("MinorHeal");
		}

		public static class Evocation {
			public static readonly SpellProto.ID Fireball = newId("Fireball");
			public static readonly SpellProto.ID LightningBolt = newId("LightningBolt");
			public static readonly SpellProto.ID BurningHands = newId("BurningHands");
			public static readonly SpellProto.ID IceStorm = newId("IceStorm");
			public static readonly SpellProto.ID ChainLightning = newId("ChainLightning");
		}

		public static class Abjuration {
			public static readonly SpellProto.ID ArcaneShield = newId("ArcaneShield");
			public static readonly SpellProto.ID MageArmor = newId("MageArmor");
			public static readonly SpellProto.ID DispelMagic = newId("DispelMagic");
			public static readonly SpellProto.ID ProtectionFromEvil = newId("ProtectionFromEvil");
			public static readonly SpellProto.ID CounterSpell = newId("CounterSpell");
		}

		public static class Holy {
			public static readonly SpellProto.ID Heal = newId("Heal");
			public static readonly SpellProto.ID CureWounds = newId("CureWounds");
			public static readonly SpellProto.ID MassHeal = newId("MassHeal");
			public static readonly SpellProto.ID DivineLight = newId("DivineLight");
			public static readonly SpellProto.ID HolySmite = newId("HolySmite");
			public static readonly SpellProto.ID Resurrection = newId("Resurrection");
			public static readonly SpellProto.ID Bless = newId("Bless");
		}

		public static class Necromancy {
			public static readonly SpellProto.ID LifeDrain = newId("LifeDrain");
			public static readonly SpellProto.ID SoulRend = newId("SoulRend");
			public static readonly SpellProto.ID AnimateDead = newId("AnimateDead");
			public static readonly SpellProto.ID Fear = newId("Fear");
			public static readonly SpellProto.ID Curse = newId("Curse");
		}

		public static class Temporal {
			public static readonly SpellProto.ID TimeEcho = newId("TimeEcho");
			public static readonly SpellProto.ID FateSight = newId("FateSight");
			public static readonly SpellProto.ID RewindWound = newId("RewindWound");
			public static readonly SpellProto.ID TemporalShift = newId("TemporalShift");
			public static readonly SpellProto.ID UnravelFate = newId("UnravelFate");
			public static readonly SpellProto.ID TimeStop = newId("TimeStop");
		}

		private static SpellProto.ID newId(string name) {
			return new SpellProto.ID($"Spell_{name}");
		}
	}
}
