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
/// Partial class containing  S t a t C a t e g o r i e s.Trim() IDs.
/// Fill in the corresponding nested static classes here.
/// </summary>
public static partial class Ids
{
    	// ═══════════════════════════════════════════════════════════════════════
	// STAT CATEGORIES
	// ═══════════════════════════════════════════════════════════════════════

	public static class StatCategories {
		public static readonly StatCategoryProto.ID Attributes = n("Attributes");
		public static readonly StatCategoryProto.ID Resource = n("Resource");
		public static readonly StatCategoryProto.ID Combat = n("Combat");
		public static readonly StatCategoryProto.ID Defense = n("Defense");
		public static readonly StatCategoryProto.ID Resistances = n("Resistances");
		public static readonly StatCategoryProto.ID DamageBonuses = n("DamageBonuses");
		public static readonly StatCategoryProto.ID Movement = n("Movement");
		public static readonly StatCategoryProto.ID Regeneration = n("Regeneration");
		public static readonly StatCategoryProto.ID Economy = n("Economy");
		public static readonly StatCategoryProto.ID Expedition = n("Expedition");
		public static readonly StatCategoryProto.ID CostModifiers = n("CostModifiers");
		public static readonly StatCategoryProto.ID Meta = n("Meta");
		public static readonly StatCategoryProto.ID BuildingUpgrades = n("BuildingUpgrades");
		public static readonly StatCategoryProto.ID Vulnerabilities = n("Vulnerabilities");

		private static StatCategoryProto.ID n(string name) => new($"StatCategory_{name}");
	}
}
