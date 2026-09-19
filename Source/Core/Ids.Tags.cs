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
/// Partial class containing  T a g s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    	// ═══════════════════════════════════════════════════════════════════════
// TAGS - Unified tagging system for all game entities
// Used for: Skills, Spells, Enemies, Items, Equipment, Locations, etc.
// ═══════════════════════════════════════════════════════════════════════

public static class Tags {

	// ═══════════════════════════════════════════════════════════════
	// SIZE TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Size {
		public static readonly TagProto.ID Tiny = n("Tiny");
		public static readonly TagProto.ID Small = n("Small");
		public static readonly TagProto.ID Medium = n("Medium");
		public static readonly TagProto.ID Large = n("Large");
		public static readonly TagProto.ID Huge = n("Huge");
		public static readonly TagProto.ID Gargantuan = n("Gargantuan");
		public static readonly TagProto.ID Colossal = n("Colossal");

		private static TagProto.ID n(string name) => new($"Tag_Size_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// TIME OF DAY TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class TimeOfDay {
		public static readonly TagProto.ID Nocturnal = n("Nocturnal");
		public static readonly TagProto.ID Diurnal = n("Diurnal");
		public static readonly TagProto.ID Crepuscular = n("Crepuscular");
		public static readonly TagProto.ID Dawn = n("Dawn");
		public static readonly TagProto.ID Dusk = n("Dusk");
		public static readonly TagProto.ID Midnight = n("Midnight");
		public static readonly TagProto.ID Noon = n("Noon");

		private static TagProto.ID n(string name) => new($"Tag_Time_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// ELEMENTAL TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Element {
		// Core Elements
		public static readonly TagProto.ID Fire = n("Fire");
		public static readonly TagProto.ID Cold = n("Cold");
		public static readonly TagProto.ID Lightning = n("Lightning");
		public static readonly TagProto.ID Poison = n("Poison");
		public static readonly TagProto.ID Acid = n("Acid");
		public static readonly TagProto.ID Thunder = n("Thunder");

		// Magical Elements
		public static readonly TagProto.ID Holy = n("Holy");
		public static readonly TagProto.ID Necrotic = n("Necrotic");
		public static readonly TagProto.ID Radiant = n("Radiant");
		public static readonly TagProto.ID Shadow = n("Shadow");
		public static readonly TagProto.ID Force = n("Force");
		public static readonly TagProto.ID Psychic = n("Psychic");
		public static readonly TagProto.ID Arcane = n("Arcane");

		// Nature Elements
		public static readonly TagProto.ID Nature = n("Nature");
		public static readonly TagProto.ID Water = n("Water");
		public static readonly TagProto.ID Earth = n("Earth");
		public static readonly TagProto.ID Air = n("Air");

		// Special
		public static readonly TagProto.ID Void = n("Void");
		public static readonly TagProto.ID Temporal = n("Temporal");
		public static readonly TagProto.ID Chaos = n("Chaos");
		public static readonly TagProto.ID Order = n("Order");

		private static TagProto.ID n(string name) => new($"Tag_Element_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// SPELL SCHOOLS
	// ═══════════════════════════════════════════════════════════════

	public static class School {
		// Classic D&D Schools
		public static readonly TagProto.ID Evocation = n("Evocation");
		public static readonly TagProto.ID Abjuration = n("Abjuration");
		public static readonly TagProto.ID Conjuration = n("Conjuration");
		public static readonly TagProto.ID Divination = n("Divination");
		public static readonly TagProto.ID Enchantment = n("Enchantment");
		public static readonly TagProto.ID Illusion = n("Illusion");
		public static readonly TagProto.ID Necromancy = n("Necromancy");
		public static readonly TagProto.ID Transmutation = n("Transmutation");

		// Divine/Nature Schools
		public static readonly TagProto.ID Holy = n("Holy");
		public static readonly TagProto.ID Nature = n("Nature");
		public static readonly TagProto.ID Primal = n("Primal");
		public static readonly TagProto.ID Divine = n("Divine");

		// Special Schools
		public static readonly TagProto.ID Temporal = n("Temporal");
		public static readonly TagProto.ID Dark = n("Dark");
		public static readonly TagProto.ID Blood = n("Blood");
		public static readonly TagProto.ID Runic = n("Runic");
		public static readonly TagProto.ID Pact = n("Pact");
		public static readonly TagProto.ID Bardic = n("Bardic");
		public static readonly TagProto.ID Ki = n("Ki");
		public static readonly TagProto.ID Psionics = n("Psionics");

		private static TagProto.ID n(string name) => new($"Tag_School_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// COMBAT TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Combat {
		// General Combat
		public static readonly TagProto.ID Melee = n("Melee");
		public static readonly TagProto.ID Ranged = n("Ranged");
		public static readonly TagProto.ID Defense = n("Defense");
		public static readonly TagProto.ID Offense = n("Offense");
		public static readonly TagProto.ID DualWield = n("DualWield");
		public static readonly TagProto.ID Unarmed = n("Unarmed");
		public static readonly TagProto.ID Tactics = n("Tactics");
		public static readonly TagProto.ID Grappling = n("Grappling");

		// Combat Styles
		public static readonly TagProto.ID Armored = n("Armored");
		public static readonly TagProto.ID Swift = n("Swift");
		public static readonly TagProto.ID Berserking = n("Berserking");
		public static readonly TagProto.ID Defensive = n("Defensive");
		public static readonly TagProto.ID Magical = n("Magical");
		public static readonly TagProto.ID Support = n("Support");
		public static readonly TagProto.ID Assassin = n("Assassin");
		public static readonly TagProto.ID Tank = n("Tank");
		public static readonly TagProto.ID Skirmisher = n("Skirmisher");
		public static readonly TagProto.ID Artillery = n("Artillery");
		public static readonly TagProto.ID Controller = n("Controller");

		private static TagProto.ID n(string name) => new($"Tag_Combat_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// WEAPON TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Weapon {
		// Melee Weapons
		public static readonly TagProto.ID Sword = n("Sword");
		public static readonly TagProto.ID Axe = n("Axe");
		public static readonly TagProto.ID Mace = n("Mace");
		public static readonly TagProto.ID Hammer = n("Hammer");
		public static readonly TagProto.ID Flail = n("Flail");
		public static readonly TagProto.ID Polearm = n("Polearm");
		public static readonly TagProto.ID Spear = n("Spear");
		public static readonly TagProto.ID Dagger = n("Dagger");
		public static readonly TagProto.ID Staff = n("Staff");
		public static readonly TagProto.ID Whip = n("Whip");
		public static readonly TagProto.ID Fist = n("Fist");
		public static readonly TagProto.ID Claw = n("Claw");

		// Ranged Weapons
		public static readonly TagProto.ID Bow = n("Bow");
		public static readonly TagProto.ID Crossbow = n("Crossbow");
		public static readonly TagProto.ID Thrown = n("Thrown");
		public static readonly TagProto.ID Sling = n("Sling");
		public static readonly TagProto.ID Blowgun = n("Blowgun");

		// Special Weapons
		public static readonly TagProto.ID Shield = n("Shield");
		public static readonly TagProto.ID Wand = n("Wand");
		public static readonly TagProto.ID Orb = n("Orb");
		public static readonly TagProto.ID Tome = n("Tome");
		public static readonly TagProto.ID Instrument = n("Instrument");

		// Weapon Properties
		public static readonly TagProto.ID TwoHanded = n("TwoHanded");
		public static readonly TagProto.ID OneHanded = n("OneHanded");
		public static readonly TagProto.ID Light = n("Light");
		public static readonly TagProto.ID Heavy = n("Heavy");
		public static readonly TagProto.ID Finesse = n("Finesse");
		public static readonly TagProto.ID Reach = n("Reach");
		public static readonly TagProto.ID Versatile = n("Versatile");

		private static TagProto.ID n(string name) => new($"Tag_Weapon_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// PHYSICAL / ATHLETICS TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Physical {
		public static readonly TagProto.ID Athletics = n("Athletics");
		public static readonly TagProto.ID Acrobatics = n("Acrobatics");
		public static readonly TagProto.ID Endurance = n("Endurance");
		public static readonly TagProto.ID Swimming = n("Swimming");
		public static readonly TagProto.ID Climbing = n("Climbing");
		public static readonly TagProto.ID Jumping = n("Jumping");
		public static readonly TagProto.ID Running = n("Running");
		public static readonly TagProto.ID Flexibility = n("Flexibility");
		public static readonly TagProto.ID Balance = n("Balance");
		public static readonly TagProto.ID Reflexes = n("Reflexes");

		private static TagProto.ID n(string name) => new($"Tag_Physical_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// STEALTH / SUBTERFUGE TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Stealth {
		public static readonly TagProto.ID Sneaking = n("Sneaking");
		public static readonly TagProto.ID Hiding = n("Hiding");
		public static readonly TagProto.ID Lockpicking = n("Lockpicking");
		public static readonly TagProto.ID TrapHandling = n("TrapHandling");
		public static readonly TagProto.ID TrapSetting = n("TrapSetting");
		public static readonly TagProto.ID Pickpocket = n("Pickpocket");
		public static readonly TagProto.ID SleightOfHand = n("SleightOfHand");
		public static readonly TagProto.ID Disguise = n("Disguise");
		public static readonly TagProto.ID Forgery = n("Forgery");
		public static readonly TagProto.ID Shadowing = n("Shadowing");
		public static readonly TagProto.ID Infiltration = n("Infiltration");
		public static readonly TagProto.ID Escape = n("Escape");

		private static TagProto.ID n(string name) => new($"Tag_Stealth_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// AWARENESS / PERCEPTION TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Awareness {
		public static readonly TagProto.ID Perception = n("Perception");
		public static readonly TagProto.ID Insight = n("Insight");
		public static readonly TagProto.ID Investigation = n("Investigation");
		public static readonly TagProto.ID Tracking = n("Tracking");
		public static readonly TagProto.ID Search = n("Search");
		public static readonly TagProto.ID SenseMotives = n("SenseMotives");
		public static readonly TagProto.ID DetectMagic = n("DetectMagic");
		public static readonly TagProto.ID DetectTraps = n("DetectTraps");
		public static readonly TagProto.ID DetectUndead = n("DetectUndead");
		public static readonly TagProto.ID Alertness = n("Alertness");

		private static TagProto.ID n(string name) => new($"Tag_Awareness_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// SURVIVAL / NATURE TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Survival {
		public static readonly TagProto.ID General = n("General");
		public static readonly TagProto.ID NatureLore = n("NatureLore");
		public static readonly TagProto.ID AnimalHandling = n("AnimalHandling");
		public static readonly TagProto.ID AnimalTraining = n("AnimalTraining");
		public static readonly TagProto.ID Foraging = n("Foraging");
		public static readonly TagProto.ID Hunting = n("Hunting");
		public static readonly TagProto.ID Fishing = n("Fishing");
		public static readonly TagProto.ID Navigation = n("Navigation");
		public static readonly TagProto.ID Camping = n("Camping");
		public static readonly TagProto.ID FireStarting = n("FireStarting");
		public static readonly TagProto.ID WeatherSense = n("WeatherSense");
		public static readonly TagProto.ID Herbalism = n("Herbalism");
		public static readonly TagProto.ID Trapping = n("Trapping");
		public static readonly TagProto.ID Skinning = n("Skinning");

		private static TagProto.ID n(string name) => new($"Tag_Survival_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// CRAFTING / TRADE TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Crafting {
		public static readonly TagProto.ID Alchemy = n("Alchemy");
		public static readonly TagProto.ID Smithing = n("Smithing");
		public static readonly TagProto.ID Weaponsmithing = n("Weaponsmithing");
		public static readonly TagProto.ID Armorsmithing = n("Armorsmithing");
		public static readonly TagProto.ID Leatherworking = n("Leatherworking");
		public static readonly TagProto.ID Tailoring = n("Tailoring");
		public static readonly TagProto.ID Enchanting = n("Enchanting");
		public static readonly TagProto.ID Scribing = n("Scribing");
		public static readonly TagProto.ID Inscription = n("Inscription");
		public static readonly TagProto.ID Runecraft = n("Runecraft");
		public static readonly TagProto.ID Cooking = n("Cooking");
		public static readonly TagProto.ID Brewing = n("Brewing");
		public static readonly TagProto.ID Woodworking = n("Woodworking");
		public static readonly TagProto.ID Carpentry = n("Carpentry");
		public static readonly TagProto.ID Bowmaking = n("Bowmaking");
		public static readonly TagProto.ID Mining = n("Mining");
		public static readonly TagProto.ID Smelting = n("Smelting");
		public static readonly TagProto.ID Jewelcrafting = n("Jewelcrafting");
		public static readonly TagProto.ID Glassblowing = n("Glassblowing");
		public static readonly TagProto.ID Pottery = n("Pottery");
		public static readonly TagProto.ID Tinkering = n("Tinkering");
		public static readonly TagProto.ID Engineering = n("Engineering");
		public static readonly TagProto.ID Trapmaking = n("Trapmaking");

		private static TagProto.ID n(string name) => new($"Tag_Crafting_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// SOCIAL / INTERACTION TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Social {
		public static readonly TagProto.ID Persuasion = n("Persuasion");
		public static readonly TagProto.ID Intimidation = n("Intimidation");
		public static readonly TagProto.ID Deception = n("Deception");
		public static readonly TagProto.ID Diplomacy = n("Diplomacy");
		public static readonly TagProto.ID Performance = n("Performance");
		public static readonly TagProto.ID Mercantile = n("Mercantile");
		public static readonly TagProto.ID Bartering = n("Bartering");
		public static readonly TagProto.ID Etiquette = n("Etiquette");
		public static readonly TagProto.ID Leadership = n("Leadership");
		public static readonly TagProto.ID Inspiration = n("Inspiration");
		public static readonly TagProto.ID Seduction = n("Seduction");
		public static readonly TagProto.ID Bluff = n("Bluff");
		public static readonly TagProto.ID Gather = n("Gather");
		public static readonly TagProto.ID Streetwise = n("Streetwise");
		public static readonly TagProto.ID Nobility = n("Nobility");
		public static readonly TagProto.ID Command = n("Command");

		private static TagProto.ID n(string name) => new($"Tag_Social_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// KNOWLEDGE / LORE TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Knowledge {
		public static readonly TagProto.ID Arcana = n("Arcana");
		public static readonly TagProto.ID History = n("History");
		public static readonly TagProto.ID Religion = n("Religion");
		public static readonly TagProto.ID Medicine = n("Medicine");
		public static readonly TagProto.ID Anatomy = n("Anatomy");
		public static readonly TagProto.ID Planes = n("Planes");
		public static readonly TagProto.ID Undead = n("Undead");
		public static readonly TagProto.ID Demonology = n("Demonology");
		public static readonly TagProto.ID Geography = n("Geography");
		public static readonly TagProto.ID Dungeoneering = n("Dungeoneering");
		public static readonly TagProto.ID Engineering = n("Engineering");
		public static readonly TagProto.ID Law = n("Law");
		public static readonly TagProto.ID Nature = n("Nature");
		public static readonly TagProto.ID Beasts = n("Beasts");
		public static readonly TagProto.ID Dragons = n("Dragons");
		public static readonly TagProto.ID Constructs = n("Constructs");
		public static readonly TagProto.ID Fey = n("Fey");
		public static readonly TagProto.ID Aberrations = n("Aberrations");
		public static readonly TagProto.ID Languages = n("Languages");
		public static readonly TagProto.ID Cryptography = n("Cryptography");
		public static readonly TagProto.ID Alchemy = n("Alchemy");
		public static readonly TagProto.ID Astronomy = n("Astronomy");
		public static readonly TagProto.ID Herbology = n("Herbology");
		public static readonly TagProto.ID Poison = n("Poison");
		public static readonly TagProto.ID Fog = n("Fog");
		public static readonly TagProto.ID Time = n("Time");

		private static TagProto.ID n(string name) => new($"Tag_Knowledge_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// MAGIC MASTERY TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Mastery {
		// School Masteries
		public static readonly TagProto.ID Evocation = n("Evocation");
		public static readonly TagProto.ID Abjuration = n("Abjuration");
		public static readonly TagProto.ID Conjuration = n("Conjuration");
		public static readonly TagProto.ID Divination = n("Divination");
		public static readonly TagProto.ID Enchantment = n("Enchantment");
		public static readonly TagProto.ID Illusion = n("Illusion");
		public static readonly TagProto.ID Necromancy = n("Necromancy");
		public static readonly TagProto.ID Transmutation = n("Transmutation");
		public static readonly TagProto.ID Holy = n("Holy");
		public static readonly TagProto.ID Nature = n("Nature");
		public static readonly TagProto.ID Temporal = n("Temporal");
		public static readonly TagProto.ID Blood = n("Blood");
		public static readonly TagProto.ID Runic = n("Runic");

		// Element Masteries
		public static readonly TagProto.ID Fire = n("Fire");
		public static readonly TagProto.ID Cold = n("Cold");
		public static readonly TagProto.ID Lightning = n("Lightning");
		public static readonly TagProto.ID Poison = n("Poison");
		public static readonly TagProto.ID Shadow = n("Shadow");
		public static readonly TagProto.ID Light = n("Light");

		private static TagProto.ID n(string name) => new($"Tag_Mastery_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// CLASS-SPECIFIC TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Class {
		// Barbarian
		public static readonly TagProto.ID Rage = n("Rage");
		public static readonly TagProto.ID Frenzy = n("Frenzy");
		public static readonly TagProto.ID Totem = n("Totem");

		// Bard
		public static readonly TagProto.ID Bardic = n("Bardic");
		public static readonly TagProto.ID Song = n("Song");
		public static readonly TagProto.ID Inspiration = n("Inspiration");

		// Cleric
		public static readonly TagProto.ID Channel = n("Channel");
		public static readonly TagProto.ID Domain = n("Domain");
		public static readonly TagProto.ID TurnUndead = n("TurnUndead");

		// Druid
		public static readonly TagProto.ID Shapeshifting = n("Shapeshifting");
		public static readonly TagProto.ID WildShape = n("WildShape");
		public static readonly TagProto.ID NaturalMagic = n("NaturalMagic");

		// Fighter
		public static readonly TagProto.ID CombatManeuvers = n("CombatManeuvers");
		public static readonly TagProto.ID Superiority = n("Superiority");
		public static readonly TagProto.ID SecondWind = n("SecondWind");
		public static readonly TagProto.ID ActionSurge = n("ActionSurge");

		// Monk
		public static readonly TagProto.ID Ki = n("Ki");
		public static readonly TagProto.ID FlurryOfBlows = n("FlurryOfBlows");
		public static readonly TagProto.ID StunningStrike = n("StunningStrike");
		public static readonly TagProto.ID Meditation = n("Meditation");

		// Paladin
		public static readonly TagProto.ID DivineSmite = n("DivineSmite");
		public static readonly TagProto.ID LayOnHands = n("LayOnHands");
		public static readonly TagProto.ID Aura = n("Aura");
		public static readonly TagProto.ID Oath = n("Oath");

		// Ranger
		public static readonly TagProto.ID FavoredEnemy = n("FavoredEnemy");
		public static readonly TagProto.ID FavoredTerrain = n("FavoredTerrain");
		public static readonly TagProto.ID HuntersMark = n("HuntersMark");
		public static readonly TagProto.ID Companion = n("Companion");

		// Rogue
		public static readonly TagProto.ID SneakAttack = n("SneakAttack");
		public static readonly TagProto.ID CunningAction = n("CunningAction");
		public static readonly TagProto.ID Evasion = n("Evasion");
		public static readonly TagProto.ID Uncanny = n("Uncanny");

		// Sorcerer
		public static readonly TagProto.ID Metamagic = n("Metamagic");
		public static readonly TagProto.ID Bloodline = n("Bloodline");
		public static readonly TagProto.ID SorceryPoints = n("SorceryPoints");

		// Warlock
		public static readonly TagProto.ID Pact = n("Pact");
		public static readonly TagProto.ID Invocation = n("Invocation");
		public static readonly TagProto.ID EldritchBlast = n("EldritchBlast");
		public static readonly TagProto.ID Patron = n("Patron");

		// Wizard
		public static readonly TagProto.ID Spellbook = n("Spellbook");
		public static readonly TagProto.ID ArcaneRecovery = n("ArcaneRecovery");
		public static readonly TagProto.ID SchoolSpecialization = n("SchoolSpecialization");

		// Game-Specific Classes
		public static readonly TagProto.ID TimeWalker = n("TimeWalker");
		public static readonly TagProto.ID Ascended = n("Ascended");
		public static readonly TagProto.ID FogWalker = n("FogWalker");

		private static TagProto.ID n(string name) => new($"Tag_Class_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// GOVERNING ATTRIBUTES TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Attribute {
		public static readonly TagProto.ID Strength = n("Strength");
		public static readonly TagProto.ID Dexterity = n("Dexterity");
		public static readonly TagProto.ID Constitution = n("Constitution");
		public static readonly TagProto.ID Intelligence = n("Intelligence");
		public static readonly TagProto.ID Wisdom = n("Wisdom");
		public static readonly TagProto.ID Charisma = n("Charisma");

		private static TagProto.ID n(string name) => new($"Tag_Attr_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// CREATURE TYPE TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Creature {
		public static readonly TagProto.ID Humanoid = n("Humanoid");
		public static readonly TagProto.ID Beast = n("Beast");
		public static readonly TagProto.ID Undead = n("Undead");
		public static readonly TagProto.ID Demon = n("Demon");
		public static readonly TagProto.ID Devil = n("Devil");
		public static readonly TagProto.ID Fiend = n("Fiend");
		public static readonly TagProto.ID Dragon = n("Dragon");
		public static readonly TagProto.ID Construct = n("Construct");
		public static readonly TagProto.ID Elemental = n("Elemental");
		public static readonly TagProto.ID Fey = n("Fey");
		public static readonly TagProto.ID Celestial = n("Celestial");
		public static readonly TagProto.ID Aberration = n("Aberration");
		public static readonly TagProto.ID Monstrosity = n("Monstrosity");
		public static readonly TagProto.ID Ooze = n("Ooze");
		public static readonly TagProto.ID Plant = n("Plant");
		public static readonly TagProto.ID Giant = n("Giant");
		public static readonly TagProto.ID Swarm = n("Swarm");
		public static readonly TagProto.ID Shapechanger = n("Shapechanger");
		public static readonly TagProto.ID Ethereal = n("Ethereal");
		public static readonly TagProto.ID Spirit = n("Spirit");
		public static readonly TagProto.ID FogTouched = n("FogTouched");
		public static readonly TagProto.ID TimeLost = n("TimeLost");

		private static TagProto.ID n(string name) => new($"Tag_Creature_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// BEHAVIOR TAGS (AI/Combat Behavior)
	// ═══════════════════════════════════════════════════════════════

	public static class Behavior {
		public static readonly TagProto.ID Aggressive = n("Aggressive");
		public static readonly TagProto.ID Passive = n("Passive");
		public static readonly TagProto.ID Defensive = n("Defensive");
		public static readonly TagProto.ID Cowardly = n("Cowardly");
		public static readonly TagProto.ID Fearless = n("Fearless");
		public static readonly TagProto.ID PackLeader = n("PackLeader");
		public static readonly TagProto.ID PackMember = n("PackMember");
		public static readonly TagProto.ID Solitary = n("Solitary");
		public static readonly TagProto.ID Opportunistic = n("Opportunistic");
		public static readonly TagProto.ID Protector = n("Protector");
		public static readonly TagProto.ID Territorial = n("Territorial");
		public static readonly TagProto.ID Ambusher = n("Ambusher");
		public static readonly TagProto.ID Flanker = n("Flanker");
		public static readonly TagProto.ID Berserk = n("Berserk");
		public static readonly TagProto.ID Tactical = n("Tactical");
		public static readonly TagProto.ID Healer = n("Healer");
		public static readonly TagProto.ID Summoner = n("Summoner");
		public static readonly TagProto.ID Caster = n("Caster");
		public static readonly TagProto.ID MageHunter = n("MageHunter");
		public static readonly TagProto.ID Challenger = n("Challenger");

		private static TagProto.ID n(string name) => new($"Tag_Behavior_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// SPECIAL / ABILITY TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Special {
		// Rarity/Rank
		public static readonly TagProto.ID Elite = n("Elite");
		public static readonly TagProto.ID Champion = n("Champion");
		public static readonly TagProto.ID Boss = n("Boss");
		public static readonly TagProto.ID Minion = n("Minion");
		public static readonly TagProto.ID Legendary = n("Legendary");
		public static readonly TagProto.ID Mythic = n("Mythic");

		// Abilities
		public static readonly TagProto.ID Regenerating = n("Regenerating");
		public static readonly TagProto.ID Splitting = n("Splitting");
		public static readonly TagProto.ID Explosive = n("Explosive");
		public static readonly TagProto.ID Phasing = n("Phasing");
		public static readonly TagProto.ID Burrowing = n("Burrowing");
		public static readonly TagProto.ID Flying = n("Flying");
		public static readonly TagProto.ID Swimming = n("Swimming");
		public static readonly TagProto.ID Climbing = n("Climbing");
		public static readonly TagProto.ID Incorporeal = n("Incorporeal");
		public static readonly TagProto.ID Invisible = n("Invisible");
		public static readonly TagProto.ID Blinking = n("Blinking");
		public static readonly TagProto.ID Teleporting = n("Teleporting");

		// Properties
		public static readonly TagProto.ID Venomous = n("Venomous");
		public static readonly TagProto.ID Diseased = n("Diseased");
		public static readonly TagProto.ID Cursed = n("Cursed");
		public static readonly TagProto.ID Blessed = n("Blessed");
		public static readonly TagProto.ID Armored = n("Armored");
		public static readonly TagProto.ID Shielded = n("Shielded");
		public static readonly TagProto.ID Vampiric = n("Vampiric");
		public static readonly TagProto.ID Draining = n("Draining");
		public static readonly TagProto.ID Reflecting = n("Reflecting");
		public static readonly TagProto.ID Absorbing = n("Absorbing");

		private static TagProto.ID n(string name) => new($"Tag_Special_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// FOG TAGS (Game-Specific)
	// ═══════════════════════════════════════════════════════════════

	public static class Fog {
		public static readonly TagProto.ID Lingering = n("Lingering");
		public static readonly TagProto.ID Hazy = n("Hazy");
		public static readonly TagProto.ID Dense = n("Dense");
		public static readonly TagProto.ID Thick = n("Thick");
		public static readonly TagProto.ID Suffocating = n("Suffocating");
		public static readonly TagProto.ID Touched = n("Touched");
		public static readonly TagProto.ID Corrupted = n("Corrupted");
		public static readonly TagProto.ID Immune = n("Immune");

		private static TagProto.ID n(string name) => new($"Tag_Fog_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// REGIONAL / TERRAIN TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Region {
		public static readonly TagProto.ID Woodland = n("Woodland");
		public static readonly TagProto.ID Forest = n("Forest");
		public static readonly TagProto.ID DeepForest = n("DeepForest");
		public static readonly TagProto.ID Jungle = n("Jungle");
		public static readonly TagProto.ID Mountain = n("Mountain");
		public static readonly TagProto.ID Hills = n("Hills");
		public static readonly TagProto.ID Swamp = n("Swamp");
		public static readonly TagProto.ID Marsh = n("Marsh");
		public static readonly TagProto.ID Desert = n("Desert");
		public static readonly TagProto.ID Tundra = n("Tundra");
		public static readonly TagProto.ID Arctic = n("Arctic");
		public static readonly TagProto.ID Volcanic = n("Volcanic");
		public static readonly TagProto.ID Subterranean = n("Subterranean");
		public static readonly TagProto.ID Cave = n("Cave");
		public static readonly TagProto.ID Aquatic = n("Aquatic");
		public static readonly TagProto.ID Coastal = n("Coastal");
		public static readonly TagProto.ID Plains = n("Plains");
		public static readonly TagProto.ID Grassland = n("Grassland");
		public static readonly TagProto.ID Urban = n("Urban");
		public static readonly TagProto.ID Ruins = n("Ruins");
		public static readonly TagProto.ID Graveyard = n("Graveyard");
		public static readonly TagProto.ID Crypt = n("Crypt");
		public static readonly TagProto.ID Temple = n("Temple");
		public static readonly TagProto.ID Dungeon = n("Dungeon");

		private static TagProto.ID n(string name) => new($"Tag_Region_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// META TAGS (Categorization & Filtering)
	// ═══════════════════════════════════════════════════════════════

	public static class Meta {
		// Action Types
		public static readonly TagProto.ID Passive = n("Passive");
		public static readonly TagProto.ID Active = n("Active");
		public static readonly TagProto.ID Toggle = n("Toggle");
		public static readonly TagProto.ID Reaction = n("Reaction");
		public static readonly TagProto.ID BonusAction = n("BonusAction");
		public static readonly TagProto.ID Concentration = n("Concentration");
		public static readonly TagProto.ID Ritual = n("Ritual");
		public static readonly TagProto.ID Cantrip = n("Cantrip");

		// Effect Types
		public static readonly TagProto.ID Offensive = n("Offensive");
		public static readonly TagProto.ID Defensive = n("Defensive");
		public static readonly TagProto.ID Utility = n("Utility");
		public static readonly TagProto.ID Healing = n("Healing");
		public static readonly TagProto.ID Buff = n("Buff");
		public static readonly TagProto.ID Debuff = n("Debuff");
		public static readonly TagProto.ID DoT = n("DoT");
		public static readonly TagProto.ID HoT = n("HoT");
		public static readonly TagProto.ID Control = n("Control");
		public static readonly TagProto.ID Crowd = n("Crowd");
		public static readonly TagProto.ID Movement = n("Movement");
		public static readonly TagProto.ID Teleport = n("Teleport");
		public static readonly TagProto.ID Summon = n("Summon");
		public static readonly TagProto.ID Transform = n("Transform");

		// Target Types
		public static readonly TagProto.ID Self = n("Self");
		public static readonly TagProto.ID SingleTarget = n("SingleTarget");
		public static readonly TagProto.ID MultiTarget = n("MultiTarget");
		public static readonly TagProto.ID AoE = n("AoE");
		public static readonly TagProto.ID Cone = n("Cone");
		public static readonly TagProto.ID Line = n("Line");
		public static readonly TagProto.ID Sphere = n("Sphere");
		public static readonly TagProto.ID Allies = n("Allies");
		public static readonly TagProto.ID Enemies = n("Enemies");
		public static readonly TagProto.ID All = n("All");

		// Skill Meta Categories
		public static readonly TagProto.ID CombatSkill = n("CombatSkill");
		public static readonly TagProto.ID MagicSkill = n("MagicSkill");
		public static readonly TagProto.ID CraftingSkill = n("CraftingSkill");
		public static readonly TagProto.ID SocialSkill = n("SocialSkill");
		public static readonly TagProto.ID KnowledgeSkill = n("KnowledgeSkill");
		public static readonly TagProto.ID PhysicalSkill = n("PhysicalSkill");
		public static readonly TagProto.ID StealthSkill = n("StealthSkill");
		public static readonly TagProto.ID SurvivalSkill = n("SurvivalSkill");

		// Rarity/Tier
		public static readonly TagProto.ID Common = n("Common");
		public static readonly TagProto.ID Uncommon = n("Uncommon");
		public static readonly TagProto.ID Rare = n("Rare");
		public static readonly TagProto.ID Epic = n("Epic");
		public static readonly TagProto.ID Legendary = n("Legendary");
		public static readonly TagProto.ID Mythic = n("Mythic");
		public static readonly TagProto.ID Unique = n("Unique");

		private static TagProto.ID n(string name) => new($"Tag_Meta_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// ITEM TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Item {
		// Materials
		public static readonly TagProto.ID Wood = n("Wood");
		public static readonly TagProto.ID Stone = n("Stone");
		public static readonly TagProto.ID Iron = n("Iron");
		public static readonly TagProto.ID Steel = n("Steel");
		public static readonly TagProto.ID Mithril = n("Mithril");
		public static readonly TagProto.ID Adamantine = n("Adamantine");
		public static readonly TagProto.ID Silver = n("Silver");
		public static readonly TagProto.ID Gold = n("Gold");
		public static readonly TagProto.ID Leather = n("Leather");
		public static readonly TagProto.ID Cloth = n("Cloth");
		public static readonly TagProto.ID Bone = n("Bone");
		public static readonly TagProto.ID Crystal = n("Crystal");
		public static readonly TagProto.ID Obsidian = n("Obsidian");
		public static readonly TagProto.ID DragonScale = n("DragonScale");
		public static readonly TagProto.ID DemonHide = n("DemonHide");
		public static readonly TagProto.ID Ethereal = n("Ethereal");
		public static readonly TagProto.ID Void = n("Void");
		public static readonly TagProto.ID Temporal = n("Temporal");
		public static readonly TagProto.ID FogMaterial = n("FogMaterial");

		// Item Types
		public static readonly TagProto.ID Consumable = n("Consumable");
		public static readonly TagProto.ID Equipment = n("Equipment");
		public static readonly TagProto.ID Material = n("Material");
		public static readonly TagProto.ID Quest = n("Quest");
		public static readonly TagProto.ID Key = n("Key");
		public static readonly TagProto.ID Treasure = n("Treasure");
		public static readonly TagProto.ID Junk = n("Junk");
		public static readonly TagProto.ID CraftingMat = n("CraftingMat");
		public static readonly TagProto.ID Recipe = n("Recipe");
		public static readonly TagProto.ID Scroll = n("Scroll");
		public static readonly TagProto.ID Potion = n("Potion");
		public static readonly TagProto.ID Food = n("Food");
		public static readonly TagProto.ID Drink = n("Drink");
		public static readonly TagProto.ID Ammo = n("Ammo");

		// Item Properties
		public static readonly TagProto.ID Magical = n("Magical");
		public static readonly TagProto.ID Cursed = n("Cursed");
		public static readonly TagProto.ID Blessed = n("Blessed");
		public static readonly TagProto.ID Enchanted = n("Enchanted");
		public static readonly TagProto.ID SetPiece = n("SetPiece");
		public static readonly TagProto.ID Soulbound = n("Soulbound");
		public static readonly TagProto.ID Stackable = n("Stackable");
		public static readonly TagProto.ID Tradeable = n("Tradeable");
		public static readonly TagProto.ID Destroyable = n("Destroyable");

		private static TagProto.ID n(string name) => new($"Tag_Item_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// ARMOR TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Armor {
		// Types
		public static readonly TagProto.ID Cloth = n("Cloth");
		public static readonly TagProto.ID Light = n("Light");
		public static readonly TagProto.ID Medium = n("Medium");
		public static readonly TagProto.ID Heavy = n("Heavy");
		public static readonly TagProto.ID Shield = n("Shield");
		public static readonly TagProto.ID Robe = n("Robe");

		// Slots
		public static readonly TagProto.ID Head = n("Head");
		public static readonly TagProto.ID Chest = n("Chest");
		public static readonly TagProto.ID Legs = n("Legs");
		public static readonly TagProto.ID Feet = n("Feet");
		public static readonly TagProto.ID Hands = n("Hands");
		public static readonly TagProto.ID Back = n("Back");
		public static readonly TagProto.ID Waist = n("Waist");
		public static readonly TagProto.ID Ring = n("Ring");
		public static readonly TagProto.ID Amulet = n("Amulet");
		public static readonly TagProto.ID Trinket = n("Trinket");

		private static TagProto.ID n(string name) => new($"Tag_Armor_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// RESISTANCE TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Resistance {
		public static readonly TagProto.ID Physical = n("Physical");
		public static readonly TagProto.ID Magical = n("Magical");
		public static readonly TagProto.ID Fire = n("Fire");
		public static readonly TagProto.ID Cold = n("Cold");
		public static readonly TagProto.ID Lightning = n("Lightning");
		public static readonly TagProto.ID Poison = n("Poison");
		public static readonly TagProto.ID Acid = n("Acid");
		public static readonly TagProto.ID Holy = n("Holy");
		public static readonly TagProto.ID Necrotic = n("Necrotic");
		public static readonly TagProto.ID Psychic = n("Psychic");
		public static readonly TagProto.ID Force = n("Force");

		private static TagProto.ID n(string name) => new($"Tag_Resist_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// IMMUNITY TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Immunity {
		public static readonly TagProto.ID Poison = n("Poison");
		public static readonly TagProto.ID Disease = n("Disease");
		public static readonly TagProto.ID Fear = n("Fear");
		public static readonly TagProto.ID Charm = n("Charm");
		public static readonly TagProto.ID Sleep = n("Sleep");
		public static readonly TagProto.ID Stun = n("Stun");
		public static readonly TagProto.ID Paralysis = n("Paralysis");
		public static readonly TagProto.ID Petrification = n("Petrification");
		public static readonly TagProto.ID Death = n("Death");
		public static readonly TagProto.ID Critical = n("Critical");
		public static readonly TagProto.ID Bleed = n("Bleed");

		private static TagProto.ID n(string name) => new($"Tag_Immune_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// VULNERABILITY TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Vulnerability {
		public static readonly TagProto.ID Fire = n("Fire");
		public static readonly TagProto.ID Cold = n("Cold");
		public static readonly TagProto.ID Lightning = n("Lightning");
		public static readonly TagProto.ID Holy = n("Holy");
		public static readonly TagProto.ID Necrotic = n("Necrotic");
		public static readonly TagProto.ID Silver = n("Silver");
		public static readonly TagProto.ID Magic = n("Magic");

		private static TagProto.ID n(string name) => new($"Tag_Vuln_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// CONDITION TAGS
	// ═══════════════════════════════════════════════════════════════

	public static class Condition {
		public static readonly TagProto.ID Blinded = n("Blinded");
		public static readonly TagProto.ID Charmed = n("Charmed");
		public static readonly TagProto.ID Deafened = n("Deafened");
		public static readonly TagProto.ID Exhausted = n("Exhausted");
		public static readonly TagProto.ID Frightened = n("Frightened");
		public static readonly TagProto.ID Grappled = n("Grappled");
		public static readonly TagProto.ID Incapacitated = n("Incapacitated");
		public static readonly TagProto.ID Invisible = n("Invisible");
		public static readonly TagProto.ID Paralyzed = n("Paralyzed");
		public static readonly TagProto.ID Petrified = n("Petrified");
		public static readonly TagProto.ID Poisoned = n("Poisoned");
		public static readonly TagProto.ID Prone = n("Prone");
		public static readonly TagProto.ID Restrained = n("Restrained");
		public static readonly TagProto.ID Stunned = n("Stunned");
		public static readonly TagProto.ID Unconscious = n("Unconscious");
		public static readonly TagProto.ID Bleeding = n("Bleeding");
		public static readonly TagProto.ID Burning = n("Burning");
		public static readonly TagProto.ID Frozen = n("Frozen");
		public static readonly TagProto.ID Silenced = n("Silenced");
		public static readonly TagProto.ID Slowed = n("Slowed");
		public static readonly TagProto.ID Rooted = n("Rooted");
		public static readonly TagProto.ID Weakened = n("Weakened");
		public static readonly TagProto.ID Cursed = n("Cursed");
		public static readonly TagProto.ID Blessed = n("Blessed");
		public static readonly TagProto.ID Empowered = n("Empowered");
		public static readonly TagProto.ID Shielded = n("Shielded");
		public static readonly TagProto.ID Regenerating = n("Regenerating");
		public static readonly TagProto.ID Hasted = n("Hasted");
		public static readonly TagProto.ID Enraged = n("Enraged");
		public static readonly TagProto.ID Confused = n("Confused");
		public static readonly TagProto.ID Dominated = n("Dominated");

		private static TagProto.ID n(string name) => new($"Tag_Condition_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// SOURCE TAGS (Where something comes from)
	// ═══════════════════════════════════════════════════════════════

	public static class Source {
		public static readonly TagProto.ID Player = n("Player");
		public static readonly TagProto.ID Enemy = n("Enemy");
		public static readonly TagProto.ID Ally = n("Ally");
		public static readonly TagProto.ID Environment = n("Environment");
		public static readonly TagProto.ID Trap = n("Trap");
		public static readonly TagProto.ID Spell = n("Spell");
		public static readonly TagProto.ID Skill = n("Skill");
		public static readonly TagProto.ID ItemSource = n("Item");
		public static readonly TagProto.ID Equipment = n("Equipment");
		public static readonly TagProto.ID Consumable = n("Consumable");
		public static readonly TagProto.ID Aura = n("Aura");
		public static readonly TagProto.ID Fog = n("Fog");
		public static readonly TagProto.ID Time = n("Time");

		private static TagProto.ID n(string name) => new($"Tag_Source_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// TRIGGER TAGS (When something activates)
	// ═══════════════════════════════════════════════════════════════

	public static class Trigger {
		public static readonly TagProto.ID OnHit = n("OnHit");
		public static readonly TagProto.ID OnCrit = n("OnCrit");
		public static readonly TagProto.ID OnKill = n("OnKill");
		public static readonly TagProto.ID OnDamage = n("OnDamage");
		public static readonly TagProto.ID OnHeal = n("OnHeal");
		public static readonly TagProto.ID OnDeath = n("OnDeath");
		public static readonly TagProto.ID OnBlock = n("OnBlock");
		public static readonly TagProto.ID OnDodge = n("OnDodge");
		public static readonly TagProto.ID OnSpellCast = n("OnSpellCast");
		public static readonly TagProto.ID OnSkillUse = n("OnSkillUse");
		public static readonly TagProto.ID OnTurnStart = n("OnTurnStart");
		public static readonly TagProto.ID OnTurnEnd = n("OnTurnEnd");
		public static readonly TagProto.ID OnCombatStart = n("OnCombatStart");
		public static readonly TagProto.ID OnCombatEnd = n("OnCombatEnd");
		public static readonly TagProto.ID OnRest = n("OnRest");
		public static readonly TagProto.ID OnLevelUp = n("OnLevelUp");
		public static readonly TagProto.ID OnLowHealth = n("OnLowHealth");
		public static readonly TagProto.ID OnFullHealth = n("OnFullHealth");
		public static readonly TagProto.ID OnStatusApplied = n("OnStatusApplied");
		public static readonly TagProto.ID OnStatusRemoved = n("OnStatusRemoved");

		private static TagProto.ID n(string name) => new($"Tag_Trigger_{name}");
	}
}
}
