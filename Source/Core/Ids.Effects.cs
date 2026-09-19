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
/// Partial class containing  E f f e c t s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    // ═══════════════════════════════════════════════════════════════════════
	// EFFECTS
	// ═══════════════════════════════════════════════════════════════════════

	public static class Effects {
		// Buff Effects
		public static class Buffs {
			public static readonly EffectProto.ID Blur = newId("Blur");
			public static readonly EffectProto.ID LastStand = newId("LastStand");
			public static readonly EffectProto.ID Empowerment = newId("Empowerment");
			public static readonly EffectProto.ID Haste = newId("Haste");
			public static readonly EffectProto.ID Shield = newId("Shield");
			public static readonly EffectProto.ID Regeneration = newId("Regeneration");
			public static readonly EffectProto.ID Resistance = newId("Resistance");
			public static readonly EffectProto.ID Precision = newId("Precision");
			public static readonly EffectProto.ID Blessed = newId("Blessed");
			public static readonly EffectProto.ID Camouflage = newId("Camouflage");
			public static readonly EffectProto.ID DarkVision = newId("DarkVision");
			public static readonly EffectProto.ID ThermalSight = newId("ThermalSight");
		}

		// Debuff Effects
		public static class Debuffs {
			public static readonly EffectProto.ID Slow = newId("Slow");
			public static readonly EffectProto.ID Weakness = newId("Weakness");
			public static readonly EffectProto.ID Blind = newId("Blind");
			public static readonly EffectProto.ID Vulnerability = newId("Vulnerability");
			public static readonly EffectProto.ID Wounds = newId("Wounds");
			public static readonly EffectProto.ID Silence = newId("Silence");
			public static readonly EffectProto.ID Fear = newId("Fear");
			public static readonly EffectProto.ID Confusion = newId("Confusion");
			public static readonly EffectProto.ID Curse = newId("Curse");
			public static readonly EffectProto.ID Disadvantage = newId("Disadvantage");
		}

		// DoT Effects
		public static class DoT {
			public static readonly EffectProto.ID Poison = newId("Poison");
			public static readonly EffectProto.ID Burning = newId("Burning");
			public static readonly EffectProto.ID Bleeding = newId("Bleeding");
			public static readonly EffectProto.ID Decay = newId("Decay");
			public static readonly EffectProto.ID Frostbite = newId("Frostbite");
			public static readonly EffectProto.ID Shock = newId("Shock");
		}

		// Control Effects
		public static class Control {
			public static readonly EffectProto.ID Root = newId("Root");
			public static readonly EffectProto.ID Stun = newId("Stun");
			public static readonly EffectProto.ID Freeze = newId("Freeze");
			public static readonly EffectProto.ID Charm = newId("Charm");
			public static readonly EffectProto.ID Prone = newId("Prone");
		}

		// Fog Effects
		public static class Fog {
			public static readonly EffectProto.ID Lingering = newId("FogLingering");
			public static readonly EffectProto.ID Hazy = newId("FogHazy");
			public static readonly EffectProto.ID Dense = newId("FogDense");
			public static readonly EffectProto.ID Thick = newId("FogThick");
			public static readonly EffectProto.ID Suffocating = newId("FogSuffocating");
		}

		// Special Effects
		public static class Special {
			public static readonly EffectProto.ID Invisible = newId("Invisible");
			public static readonly EffectProto.ID Undying = newId("Undying");
			public static readonly EffectProto.ID Advantage = newId("Advantage");
		}

		private static EffectProto.ID newId(string name) {
			return new EffectProto.ID($"Effect_{name}");
		}
	}
}
