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
/// Partial class containing  S t a t s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    // ═══════════════════════════════════════════════════════════════════════
// STATS
// ═══════════════════════════════════════════════════════════════════════

public static class Stats {

	// ═══════════════════════════════════════════════════════════════
	// BUILDING / VILLAGE STATS
	// ═══════════════════════════════════════════════════════════════

	public static class BuildingUpgrades {
		// Starting Resources
		public static readonly StatProto.ID StartingGold = n("StartingGold");
		public static readonly StatProto.ID StartingFood = n("StartingFood");
		public static readonly StatProto.ID StartingMedical = n("StartingMedical");
		public static readonly StatProto.ID StartingCamping = n("StartingCamping");

		// Morale & Rest
		public static readonly StatProto.ID MaxMorale = n("MaxMorale");
		public static readonly StatProto.ID RestEffectiveness = n("RestEffectiveness");
		public static readonly StatProto.ID FreeHealing = n("FreeHealing");

		// Construction
		public static readonly StatProto.ID BuildingCostReduction = n("BuildingCostReduction");

		// Consumables
		public static readonly StatProto.ID PotionEffectiveness = n("PotionEffectiveness");

		// Exploration
		public static readonly StatProto.ID FogRevealRadius = n("FogRevealRadius");
		public static readonly StatProto.ID AmbushResistance = n("AmbushResistance");

		// Hunting & Food
		public static readonly StatProto.ID HuntingBonus = n("HuntingBonus");
		public static readonly StatProto.ID FoodFromCombat = n("FoodFromCombat");

		// Divine
		public static readonly StatProto.ID BlessingDuration = n("BlessingDuration");

		// Crafting
		public static readonly StatProto.ID CraftingQuality = n("CraftingQuality");
		public static readonly StatProto.ID SalvageBonus = n("SalvageBonus");

		// Meta Progression
		public static readonly StatProto.ID AchievementBonus = n("AchievementBonus");
		public static readonly StatProto.ID DeathBonus = n("DeathBonus");
		public static readonly StatProto.ID LegacyExperience = n("LegacyExperience");

		// Combat Rewards
		public static readonly StatProto.ID CombatGold = n("CombatGold");

		// Discovery
		public static readonly StatProto.ID EventPreview = n("EventPreview");
		public static readonly StatProto.ID LoreDiscovery = n("LoreDiscovery");

		// Portal
		public static readonly StatProto.ID PortalCharges = n("PortalCharges");
		public static readonly StatProto.ID PortalRange = n("PortalRange");

		// Personal
		public static readonly StatProto.ID PersonalStorage = n("PersonalStorage");

		// Party
		public static readonly StatProto.ID PartySize = n("PartySize");
		public static readonly StatProto.ID CompanionExperience = n("CompanionExperience");

		private static StatProto.ID n(string name) => new($"Stat_Building_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// PRIMARY ATTRIBUTES
	// ═══════════════════════════════════════════════════════════════

	public static class Attributes {
		public static readonly StatProto.ID Strength = n("Strength");
		public static readonly StatProto.ID Dexterity = n("Dexterity");
		public static readonly StatProto.ID Constitution = n("Constitution");
		public static readonly StatProto.ID Intelligence = n("Intelligence");
		public static readonly StatProto.ID Wisdom = n("Wisdom");
		public static readonly StatProto.ID Charisma = n("Charisma");

		private static StatProto.ID n(string name) => new($"Stat_Attr_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// RESOURCES
	// ═══════════════════════════════════════════════════════════════

	public static class Resource {
		public static readonly StatProto.ID MaxHealth = n("MaxHealth");
		public static readonly StatProto.ID CurrentHealth = n("CurrentHealth");
		public static readonly StatProto.ID MaxMana = n("MaxMana");
		public static readonly StatProto.ID CurrentMana = n("CurrentMana");
		public static readonly StatProto.ID MaxStamina = n("MaxStamina");
		public static readonly StatProto.ID CurrentStamina = n("CurrentStamina");
		public static readonly StatProto.ID Fatigue = n("Fatigue");
		public static readonly StatProto.ID MaxFatigue = n("MaxFatigue");
		public static readonly StatProto.ID Morale = n("Morale");

		private static StatProto.ID n(string name) => new($"Stat_Resource_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// COMBAT
	// ═══════════════════════════════════════════════════════════════

	public static class Combat {
		public static readonly StatProto.ID ArmorClass = n("ArmorClass");
		public static readonly StatProto.ID AttackBonus = n("AttackBonus");
		public static readonly StatProto.ID DamBonus = n("DamageBonus");
		public static readonly StatProto.ID SpellPower = n("SpellPower");
		public static readonly StatProto.ID Initiative = n("Initiative");
		public static readonly StatProto.ID InitiativeRoll = n("InitiativeRoll");
		public static readonly StatProto.ID CriticalChance = n("CriticalChance");
		public static readonly StatProto.ID CriticalDamage = n("CriticalDamage");
		public static readonly StatProto.ID BlockChance = n("BlockChance");
		public static readonly StatProto.ID DodgeChance = n("DodgeChance");
		public static readonly StatProto.ID ParryChance = n("ParryChance");

		private static StatProto.ID n(string name) => new($"Stat_Combat_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// MOVEMENT & SPEED
	// ═══════════════════════════════════════════════════════════════

	public static class Movement {
		public static readonly StatProto.ID MovementSpeed = n("MovementSpeed");
		public static readonly StatProto.ID AttackSpeed = n("AttackSpeed");
		public static readonly StatProto.ID CastSpeed = n("CastSpeed");

		private static StatProto.ID n(string name) => new($"Stat_Movement_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// RESISTANCES
	// ═══════════════════════════════════════════════════════════════

	public static class Resistances {
		public static readonly StatProto.ID Physical = n("Physical");
		public static readonly StatProto.ID Magical = n("Magical");
		public static readonly StatProto.ID Fire = n("Fire");
		public static readonly StatProto.ID Cold = n("Cold");
		public static readonly StatProto.ID Lightning = n("Lightning");
		public static readonly StatProto.ID Poison = n("Poison");
		public static readonly StatProto.ID Acid = n("Acid");
		public static readonly StatProto.ID Holy = n("Holy");
		public static readonly StatProto.ID Necrotic = n("Necrotic");
		public static readonly StatProto.ID Psychic = n("Psychic");
		public static readonly StatProto.ID Force = n("Force");

		private static StatProto.ID n(string name) => new($"Stat_Resist_{name}");
	}

	public static class Vulnerabilities {
		public static readonly StatProto.ID Physical = n("Physical");
		public static readonly StatProto.ID Magical = n("Magical");
		public static readonly StatProto.ID Fire = n("Fire");
		public static readonly StatProto.ID Cold = n("Cold");
		public static readonly StatProto.ID Lightning = n("Lightning");
		public static readonly StatProto.ID Poison = n("Poison");
		public static readonly StatProto.ID Acid = n("Acid");
		public static readonly StatProto.ID Holy = n("Holy");
		public static readonly StatProto.ID Necrotic = n("Necrotic");
		public static readonly StatProto.ID Psychic = n("Psychic");
		public static readonly StatProto.ID Force = n("Force");
		public static readonly StatProto.ID Silver = n("Silver"); // For werewolves etc

		private static StatProto.ID n(string name) => new($"Stat_Vuln_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// DAMAGE BONUSES
	// ═══════════════════════════════════════════════════════════════

	public static class DamageBonus {
		public static readonly StatProto.ID Physical = n("Physical");
		public static readonly StatProto.ID Fire = n("Fire");
		public static readonly StatProto.ID Cold = n("Cold");
		public static readonly StatProto.ID Lightning = n("Lightning");
		public static readonly StatProto.ID Poison = n("Poison");
		public static readonly StatProto.ID Acid = n("Acid");
		public static readonly StatProto.ID Holy = n("Holy");
		public static readonly StatProto.ID Necrotic = n("Necrotic");
		public static readonly StatProto.ID Psychic = n("Psychic");
		public static readonly StatProto.ID Force = n("Force");
		public static readonly StatProto.ID Arcane = n("Arcane");

		private static StatProto.ID n(string name) => new($"Stat_DmgBonus_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// REGENERATION
	// ═══════════════════════════════════════════════════════════════

	public static class Regeneration {
		public static readonly StatProto.ID Health = n("Health");
		public static readonly StatProto.ID Mana = n("Mana");
		public static readonly StatProto.ID Stamina = n("Stamina");
		public static readonly StatProto.ID Exhaustion = n("Exhaustion");
		public static readonly StatProto.ID Morale = n("Morale");

		private static StatProto.ID n(string name) => new($"Stat_Regen_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// ECONOMY
	// ═══════════════════════════════════════════════════════════════

	public static class Economy {
		public static readonly StatProto.ID GoldFind = n("GoldFind");
		public static readonly StatProto.ID ItemFind = n("ItemFind");
		public static readonly StatProto.ID ExperienceGain = n("ExperienceGain");
		public static readonly StatProto.ID CraftingSpeed = n("CraftingSpeed");
		public static readonly StatProto.ID GatheringSpeed = n("GatheringSpeed");

		private static StatProto.ID n(string name) => new($"Stat_Economy_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// EXPEDITION
	// ═══════════════════════════════════════════════════════════════

	public static class Expedition {
		public static readonly StatProto.ID FoodConsumption = n("FoodConsumption");
		public static readonly StatProto.ID FatigueRate = n("FatigueRate");
		public static readonly StatProto.ID TravelSpeed = n("TravelSpeed");
		public static readonly StatProto.ID CarryCapacity = n("CarryCapacity");
		public static readonly StatProto.ID VisionRange = n("VisionRange");

		public static readonly StatProto.ID GoldOnHand = n("GoldOnHand");
		public static readonly StatProto.ID FoodOnHand = n("FoodOnHand");
		public static readonly StatProto.ID MedicalSupplies = n("MedicalSupplies");
		public static readonly StatProto.ID CampingSupplies = n("CampingSupplies");
		public static readonly StatProto.ID Torches = n("Torches");
		public static readonly StatProto.ID Water = n("Water");

		private static StatProto.ID n(string name) => new($"Stat_Expedition_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// META
	// ═══════════════════════════════════════════════════════════════

	public static class Meta {
		public static readonly StatProto.ID Level = n("Level");
		public static readonly StatProto.ID GoldAwarded = n("GoldAwarded");
		public static readonly StatProto.ID Experience = n("Experience");
		public static readonly StatProto.ID ExperienceAwarded = n("ExperienceAwarded");
		public static readonly StatProto.ID ExperienceToNextLevel = n("ExperienceToNextLevel");
		public static readonly StatProto.ID Luck = n("Luck");
		public static readonly StatProto.ID Size = n("Size");
		public static readonly StatProto.ID CreationPointsPerLevel = n("CreationPointsPerLevel");
		public static readonly StatProto.ID CreationPointsUnspent = n("CreationPointsUnspent");

		private static StatProto.ID n(string name) => new($"Stat_Meta_{name}");
	}

	// ═══════════════════════════════════════════════════════════════
	// COST MODIFIERS
	// ═══════════════════════════════════════════════════════════════

	public static class CostMod {
		public static readonly StatProto.ID ManaCost = n("ManaCost");
		public static readonly StatProto.ID StaminaCost = n("StaminaCost");
		public static readonly StatProto.ID Cooldown = n("Cooldown");

		private static StatProto.ID n(string name) => new($"Stat_CostMod_{name}");
	}
}
}
