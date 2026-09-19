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
/// Partial class containing  S k i l l s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    // ═══════════════════════════════════════════════════════════════════════
// SKILLS
// ═══════════════════════════════════════════════════════════════════════

public static class Skills {

	// ═══════════════════════════════════════════════════════════════
	// COMBAT SKILLS
	// ═══════════════════════════════════════════════════════════════

	public static class Combat {
		// Melee
		public static readonly SkillProto.ID OneHandedWeapons = n("OneHandedWeapons");
		public static readonly SkillProto.ID TwoHandedWeapons = n("TwoHandedWeapons");
		public static readonly SkillProto.ID DualWield = n("DualWield");
		public static readonly SkillProto.ID Shields = n("Shields");
		public static readonly SkillProto.ID Unarmed = n("Unarmed");

		// Ranged
		public static readonly SkillProto.ID Archery = n("Archery");
		public static readonly SkillProto.ID Throwing = n("Throwing");

		// Defensive
		public static readonly SkillProto.ID ArmorProficiency = n("ArmorProficiency");
		public static readonly SkillProto.ID Dodge = n("Dodge");
		public static readonly SkillProto.ID Parry = n("Parry");

		// Tactics
		public static readonly SkillProto.ID Tactics = n("Tactics");
		public static readonly SkillProto.ID CriticalStrike = n("CriticalStrike");

		private static SkillProto.ID n(string name) => new($"Skill_Combat_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// PHYSICAL SKILLS
	// ═══════════════════════════════════════════════════════════════

	public static class Physical {
		public static readonly SkillProto.ID Athletics = n("Athletics");
		public static readonly SkillProto.ID Acrobatics = n("Acrobatics");
		public static readonly SkillProto.ID Endurance = n("Endurance");
		public static readonly SkillProto.ID Swimming = n("Swimming");
		public static readonly SkillProto.ID Climbing = n("Climbing");

		private static SkillProto.ID n(string name) => new($"Skill_Physical_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// STEALTH SKILLS
	// ═══════════════════════════════════════════════════════════════

	public static class Stealth {
		public static readonly SkillProto.ID Sneaking = n("Sneaking");
		public static readonly SkillProto.ID Hiding = n("Hiding");
		public static readonly SkillProto.ID Lockpicking = n("Lockpicking");
		public static readonly SkillProto.ID Pickpocket = n("Pickpocket");
		public static readonly SkillProto.ID TrapHandling = n("TrapHandling");
		public static readonly SkillProto.ID SleightOfHand = n("SleightOfHand");
		public static readonly SkillProto.ID Disguise = n("Disguise");

		private static SkillProto.ID n(string name) => new($"Skill_Stealth_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// AWARENESS SKILLS
	// ═══════════════════════════════════════════════════════════════

	public static class Awareness {
		public static readonly SkillProto.ID Perception = n("Perception");
		public static readonly SkillProto.ID Insight = n("Insight");
		public static readonly SkillProto.ID Investigation = n("Investigation");
		public static readonly SkillProto.ID Tracking = n("Tracking");

		private static SkillProto.ID n(string name) => new($"Skill_Awareness_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// SURVIVAL SKILLS
	// ═══════════════════════════════════════════════════════════════

	public static class Survival {
		public static readonly SkillProto.ID General = n("General");
		public static readonly SkillProto.ID AnimalHandling = n("AnimalHandling");
		public static readonly SkillProto.ID Navigation = n("Navigation");
		public static readonly SkillProto.ID Camping = n("Camping");
		public static readonly SkillProto.ID WeatherSense = n("WeatherSense");

		private static SkillProto.ID n(string name) => new($"Skill_Survival_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// GATHERING SKILLS
	// ═══════════════════════════════════════════════════════════════

	public static class Gathering {
		public static readonly SkillProto.ID Mining = n("Mining");
		public static readonly SkillProto.ID Herbalism = n("Herbalism");
		public static readonly SkillProto.ID Woodcutting = n("Woodcutting");
		public static readonly SkillProto.ID Fishing = n("Fishing");
		public static readonly SkillProto.ID Foraging = n("Foraging");
		public static readonly SkillProto.ID Skinning = n("Skinning");
		public static readonly SkillProto.ID Hunting = n("Hunting");

		private static SkillProto.ID n(string name) => new($"Skill_Gathering_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// CRAFTING SKILLS
	// ═══════════════════════════════════════════════════════════════

	public static class Crafting {
		public static readonly SkillProto.ID Smithing = n("Smithing");
		public static readonly SkillProto.ID Alchemy = n("Alchemy");
		public static readonly SkillProto.ID Enchanting = n("Enchanting");
		public static readonly SkillProto.ID Cooking = n("Cooking");
		public static readonly SkillProto.ID Leatherworking = n("Leatherworking");
		public static readonly SkillProto.ID Tailoring = n("Tailoring");
		public static readonly SkillProto.ID Woodworking = n("Woodworking");
		public static readonly SkillProto.ID Jewelcrafting = n("Jewelcrafting");
		public static readonly SkillProto.ID Scribing = n("Scribing");
		public static readonly SkillProto.ID Runecraft = n("Runecraft");

		private static SkillProto.ID n(string name) => new($"Skill_Crafting_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// SOCIAL SKILLS
	// ═══════════════════════════════════════════════════════════════

	public static class Social {
		public static readonly SkillProto.ID Persuasion = n("Persuasion");
		public static readonly SkillProto.ID Intimidation = n("Intimidation");
		public static readonly SkillProto.ID Deception = n("Deception");
		public static readonly SkillProto.ID Performance = n("Performance");
		public static readonly SkillProto.ID Mercantile = n("Mercantile");
		public static readonly SkillProto.ID Leadership = n("Leadership");

		private static SkillProto.ID n(string name) => new($"Skill_Social_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// KNOWLEDGE SKILLS
	// ═══════════════════════════════════════════════════════════════

	public static class Knowledge {
		public static readonly SkillProto.ID Arcana = n("Arcana");
		public static readonly SkillProto.ID History = n("History");
		public static readonly SkillProto.ID Religion = n("Religion");
		public static readonly SkillProto.ID Nature = n("Nature");
		public static readonly SkillProto.ID Medicine = n("Medicine");
		public static readonly SkillProto.ID Dungeoneering = n("Dungeoneering");
		public static readonly SkillProto.ID Planes = n("Planes");
		public static readonly SkillProto.ID FogLore = n("FogLore");
		public static readonly SkillProto.ID TimeLore = n("TimeLore");

		private static SkillProto.ID n(string name) => new($"Skill_Knowledge_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// MAGIC SKILLS
	// ═══════════════════════════════════════════════════════════════

	public static class Magic {
		// Spell Schools
		public static readonly SkillProto.ID Evocation = n("Evocation");
		public static readonly SkillProto.ID Abjuration = n("Abjuration");
		public static readonly SkillProto.ID Conjuration = n("Conjuration");
		public static readonly SkillProto.ID Divination = n("Divination");
		public static readonly SkillProto.ID Enchantment = n("Enchantment");
		public static readonly SkillProto.ID Illusion = n("Illusion");
		public static readonly SkillProto.ID Necromancy = n("Necromancy");
		public static readonly SkillProto.ID Transmutation = n("Transmutation");

		// Divine/Nature
		public static readonly SkillProto.ID HolyMagic = n("HolyMagic");
		public static readonly SkillProto.ID NatureMagic = n("NatureMagic");

		// Special
		public static readonly SkillProto.ID Temporal = n("Temporal");
		public static readonly SkillProto.ID BloodMagic = n("BloodMagic");

		// General
		public static readonly SkillProto.ID SpellFocus = n("SpellFocus");
		public static readonly SkillProto.ID ManaPool = n("ManaPool");

		private static SkillProto.ID n(string name) => new($"Skill_Magic_{name}");
	}
}
}
