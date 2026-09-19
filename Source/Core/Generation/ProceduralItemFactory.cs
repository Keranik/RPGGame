using RPGGame.Core.Combat;
using RPGGame.Core.Effects;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Item.Equipment.Armor;
using RPGGame.Core.Prototypes.Item.Equipment.Weapon;
using RPGGame.Core.Prototypes.Stats;

namespace RPGGame.Core.Generation;

/// <summary>
/// Factory for generating procedural items from base prototypes.
/// Takes an ItemProto and applies creation points to randomize stats and add enchantments.
/// </summary>
public class ProceduralItemFactory {
	private static readonly Random SharedRandom = new();

	private readonly GameDb k_gameDb;
	private readonly Random k_random;

	#region Upgrade Costs

	/// <summary>Cost per point of weapon damage bonus.</summary>
	private const int WEAPON_DAMAGE_COST = 8;

	/// <summary>Cost per point of weapon attack bonus.</summary>
	private const int WEAPON_ATTACK_COST = 10;

	/// <summary>Cost per extra damage die.</summary>
	private const int EXTRA_DAMAGE_DIE_COST = 20;

	/// <summary>Cost to upgrade damage die size.</summary>
	private const int UPGRADE_DIE_SIZE_COST = 15;

	/// <summary>Cost per point of armor AC.</summary>
	private const int ARMOR_AC_COST = 15;

	/// <summary>Cost per point of armor resistance.</summary>
	private const int ARMOR_RESISTANCE_COST = 12;

	/// <summary>Cost per point of stat bonus on equipment.</summary>
	private static readonly Dictionary<StatProto.ID, int> EquipStatCosts = new() {
		{ Ids.Stats.Attributes.Strength, 8 },
		{ Ids.Stats.Attributes.Dexterity, 8 },
		{ Ids.Stats.Attributes.Constitution, 8 },
		{ Ids.Stats.Attributes.Intelligence, 8 },
		{ Ids.Stats.Attributes.Wisdom, 8 },
		{ Ids.Stats.Attributes.Charisma, 8 },
		{ Ids.Stats.Resource.MaxHealth, 3 },
		{ Ids.Stats.Resource.MaxMana, 4 },
		{ Ids.Stats.Combat.ArmorClass, 12 },
		{ Ids.Stats.Combat.AttackBonus, 10 },
		{ Ids.Stats.Combat.DamBonus, 8 },
		{ Ids.Stats.Combat.SpellPower, 10 },
		{ Ids.Stats.Combat.CriticalChance, 6 },
		{ Ids.Stats.Combat.CriticalDamage, 5 },
		{ Ids.Stats.Combat.Initiative, 5 },
		{ Ids.Stats.Movement.MovementSpeed, 4 },
		{ Ids.Stats.Expedition.VisionRange, 4 },
		{ Ids.Stats.Combat.DodgeChance, 4 }
	};

	/// <summary>Valid stats for weapons.</summary>
	private static readonly HashSet<StatProto.ID> ValidWeaponStats = [
		Ids.Stats.Attributes.Strength,
		Ids.Stats.Attributes.Dexterity,
		Ids.Stats.Combat.AttackBonus,
		Ids.Stats.Combat.DamBonus,
		Ids.Stats.Combat.CriticalChance,
		Ids.Stats.Combat.CriticalDamage,
		Ids.Stats.Combat.SpellPower
	];

	/// <summary>Valid stats for armor.</summary>
	private static readonly HashSet<StatProto.ID> ValidArmorStats = [
		Ids.Stats.Attributes.Strength,
		Ids.Stats.Attributes.Dexterity,
		Ids.Stats.Attributes.Constitution,
		Ids.Stats.Resource.MaxHealth,
		Ids.Stats.Combat.ArmorClass,
		Ids.Stats.Resistances.Physical,
		Ids.Stats.Resistances.Magical,
		Ids.Stats.Combat.Initiative,
		Ids.Stats.Movement.MovementSpeed
	];

	/// <summary>Cost to add an enchantment.</summary>
	private const int ENCHANTMENT_COST = 25;

	/// <summary>Cost to improve durability.</summary>
	private const int DURABILITY_COST = 5;

	#endregion

	#region Constructor

	public ProceduralItemFactory(GameDb gameDb, Random? random = null) {
		k_gameDb = gameDb;
		k_random = random ?? SharedRandom;
	}

	#endregion

	#region Public Methods - Weapons

	/// <summary>
	/// Generates a procedural weapon from a base prototype.
	/// </summary>
	public GeneratedWeapon? GenerateWeapon(
		WeaponProto.ID protoId,
		int minPoints,
		int maxPoints,
		RarityType maxRarity = RarityType.Rare) {

		var proto = k_gameDb.Get<WeaponProto>(protoId);
		if (proto == null) {
			return null;
		}

		var budget = CreationPointBudget.Create(minPoints, maxPoints, maxRarity, k_random);
		var weapon = new GeneratedWeapon(proto, budget.Rarity);

		SpendWeaponPoints(weapon, budget);
		weapon.FinalizeStats();

		return weapon;
	}

	/// <summary>
	/// Generates a random weapon of the specified type.
	/// </summary>
	public GeneratedWeapon? GenerateRandomWeapon(
		WeaponType? weaponType,
		int minPoints,
		int maxPoints,
		RarityType maxRarity = RarityType.Rare,
		int? maxItemLevel = null) {

		// Get all weapon protos matching criteria
		var candidates = k_gameDb.GetAll<WeaponProto>()
			.Where(w => !weaponType.HasValue || w.WeaponType == weaponType.Value)
			.Where(w => !maxItemLevel.HasValue || w.ItemLevel <= maxItemLevel.Value)
			.ToList();

		if (candidates.Count == 0) {
			return null;
		}

		var proto = candidates[k_random.Next(candidates.Count)];
		var budget = CreationPointBudget.Create(minPoints, maxPoints, maxRarity, k_random);
		var weapon = new GeneratedWeapon(proto, budget.Rarity);

		SpendWeaponPoints(weapon, budget);
		weapon.FinalizeStats();

		return weapon;
	}

	#endregion

	#region Public Methods - Armor

	/// <summary>
	/// Generates a procedural armor from a base prototype.
	/// </summary>
	public GeneratedArmor? GenerateArmor(
		ArmorProto.ID protoId,
		int minPoints,
		int maxPoints,
		RarityType maxRarity = RarityType.Rare) {

		var proto = k_gameDb.Get<ArmorProto>(protoId);
		if (proto == null) {
			return null;
		}

		var budget = CreationPointBudget.Create(minPoints, maxPoints, maxRarity, k_random);
		var armor = new GeneratedArmor(proto, budget.Rarity);

		SpendArmorPoints(armor, budget);
		armor.FinalizeStats();

		return armor;
	}

	/// <summary>
	/// Generates a random armor of the specified type.
	/// </summary>
	public GeneratedArmor? GenerateRandomArmor(
		ArmorType? armorType,
		SlotType? slot,
		int minPoints,
		int maxPoints,
		RarityType maxRarity = RarityType.Rare,
		int? maxItemLevel = null) {

		var candidates = k_gameDb.GetAll<ArmorProto>()
			.Where(a => !armorType.HasValue || a.ArmorType == armorType.Value)
			.Where(a => !slot.HasValue || a.EquipSlot == slot.Value)
			.Where(a => !maxItemLevel.HasValue || a.ItemLevel <= maxItemLevel.Value)
			.ToList();

		if (candidates.Count == 0) {
			return null;
		}

		var proto = candidates[k_random.Next(candidates.Count)];
		var budget = CreationPointBudget.Create(minPoints, maxPoints, maxRarity, k_random);
		var armor = new GeneratedArmor(proto, budget.Rarity);

		SpendArmorPoints(armor, budget);
		armor.FinalizeStats();

		return armor;
	}

	#endregion

	#region Private Methods - Weapon Generation

	private void SpendWeaponPoints(GeneratedWeapon weapon, CreationPointBudget budget) {
		int iterations = 0;
		const int maxIterations = 100;

		while (budget.RemainingPoints > 0 && iterations < maxIterations) {
			iterations++;

			var upgrades = GetAffordableWeaponUpgrades(weapon, budget);
			if (upgrades.Count == 0) {
				break;
			}

			var upgrade = upgrades[k_random.Next(upgrades.Count)];
			ApplyWeaponUpgrade(weapon, upgrade, budget);
		}
	}

	private List<ItemUpgrade> GetAffordableWeaponUpgrades(GeneratedWeapon weapon, CreationPointBudget budget) {
		var upgrades = new List<ItemUpgrade>();
		int remaining = budget.RemainingPoints;

		// Damage bonus
		if (WEAPON_DAMAGE_COST <= remaining) {
			upgrades.Add(new ItemUpgrade(ItemUpgradeType.DamageBonus, WEAPON_DAMAGE_COST));
		}

		// Attack bonus
		if (WEAPON_ATTACK_COST <= remaining) {
			upgrades.Add(new ItemUpgrade(ItemUpgradeType.AttackBonus, WEAPON_ATTACK_COST));
		}

		// Extra damage die
		if (EXTRA_DAMAGE_DIE_COST <= remaining && weapon.ExtraDamageDice < 2) {
			upgrades.Add(new ItemUpgrade(ItemUpgradeType.ExtraDamageDie, EXTRA_DAMAGE_DIE_COST));
		}

		// Upgrade die size
		if (UPGRADE_DIE_SIZE_COST <= remaining && weapon.CanUpgradeDieSize()) {
			upgrades.Add(new ItemUpgrade(ItemUpgradeType.UpgradeDieSize, UPGRADE_DIE_SIZE_COST));
		}

		// Stat bonuses
		foreach (var (statId, cost) in EquipStatCosts) {
			if (cost <= remaining && ValidWeaponStats.Contains(statId)) {
				upgrades.Add(new ItemUpgrade(ItemUpgradeType.StatBonus, cost, statId: statId));
			}
		}

		// Enchantment
		if (ENCHANTMENT_COST <= remaining && weapon.Enchantments.Count < 3) {
			upgrades.Add(new ItemUpgrade(ItemUpgradeType.Enchantment, ENCHANTMENT_COST));
		}

		// Durability
		if (DURABILITY_COST <= remaining) {
			upgrades.Add(new ItemUpgrade(ItemUpgradeType.Durability, DURABILITY_COST));
		}

		return upgrades;
	}

	private void ApplyWeaponUpgrade(GeneratedWeapon weapon, ItemUpgrade upgrade, CreationPointBudget budget) {
		budget.Spend(upgrade.Cost);

		switch (upgrade.Type) {
			case ItemUpgradeType.DamageBonus:
				weapon.BonusDamage++;
				break;

			case ItemUpgradeType.AttackBonus:
				weapon.BonusAttack++;
				break;

			case ItemUpgradeType.ExtraDamageDie:
				weapon.ExtraDamageDice++;
				break;

			case ItemUpgradeType.UpgradeDieSize:
				weapon.DieSizeUpgrades++;
				break;

			case ItemUpgradeType.StatBonus:
				weapon.AddStatBonus(upgrade.StatId!.Value, 1);
				break;

			case ItemUpgradeType.Enchantment:
				AddRandomEnchantment(weapon);
				break;

			case ItemUpgradeType.Durability:
				weapon.BonusDurability += 10;
				break;
		}
	}

	#endregion

	#region Private Methods - Armor Generation

	private void SpendArmorPoints(GeneratedArmor armor, CreationPointBudget budget) {
		int iterations = 0;
		const int maxIterations = 100;

		while (budget.RemainingPoints > 0 && iterations < maxIterations) {
			iterations++;

			var upgrades = GetAffordableArmorUpgrades(armor, budget);
			if (upgrades.Count == 0) {
				break;
			}

			var upgrade = upgrades[k_random.Next(upgrades.Count)];
			ApplyArmorUpgrade(armor, upgrade, budget);
		}
	}

	private List<ItemUpgrade> GetAffordableArmorUpgrades(GeneratedArmor armor, CreationPointBudget budget) {
		var upgrades = new List<ItemUpgrade>();
		int remaining = budget.RemainingPoints;

		// AC bonus
		if (ARMOR_AC_COST <= remaining && armor.BonusAC < 3) {
			upgrades.Add(new ItemUpgrade(ItemUpgradeType.ArmorBonus, ARMOR_AC_COST));
		}

		// Resistance bonus
		if (ARMOR_RESISTANCE_COST <= remaining) {
			upgrades.Add(new ItemUpgrade(ItemUpgradeType.Resistance, ARMOR_RESISTANCE_COST));
		}

		// Stat bonuses
		foreach (var (statId, cost) in EquipStatCosts) {
			if (cost <= remaining && ValidArmorStats.Contains(statId)) {
				upgrades.Add(new ItemUpgrade(ItemUpgradeType.StatBonus, cost, statId: statId));
			}
		}

		// Enchantment
		if (ENCHANTMENT_COST <= remaining && armor.Enchantments.Count < 3) {
			upgrades.Add(new ItemUpgrade(ItemUpgradeType.Enchantment, ENCHANTMENT_COST));
		}

		// Durability
		if (DURABILITY_COST <= remaining) {
			upgrades.Add(new ItemUpgrade(ItemUpgradeType.Durability, DURABILITY_COST));
		}

		return upgrades;
	}

	private void ApplyArmorUpgrade(GeneratedArmor armor, ItemUpgrade upgrade, CreationPointBudget budget) {
		budget.Spend(upgrade.Cost);

		switch (upgrade.Type) {
			case ItemUpgradeType.ArmorBonus:
				armor.BonusAC++;
				break;

			case ItemUpgradeType.Resistance:
				AddRandomResistance(armor);
				break;

			case ItemUpgradeType.StatBonus:
				armor.AddStatBonus(upgrade.StatId!.Value, 1);
				break;

			case ItemUpgradeType.Enchantment:
				AddRandomEnchantment(armor);
				break;

			case ItemUpgradeType.Durability:
				armor.BonusDurability += 10;
				break;
		}
	}

	#endregion

	#region Private Methods - Enchantments

	private void AddRandomEnchantment(GeneratedItem item) {
		// TODO: Pull from enchantment pool based on item type
		var enchantments = new List<EffectProto.ID> {
			Ids.Effects.Buffs.Empowerment,
			Ids.Effects.Buffs.Precision,
			Ids.Effects.Buffs.Haste,
			Ids.Effects.Buffs.Resistance,
			Ids.Effects.Buffs.Regeneration
		};

		// Filter out already applied
		enchantments = enchantments.Where(e => !item.Enchantments.Contains(e)).ToList();

		if (enchantments.Count > 0) {
			item.Enchantments.Add(enchantments[k_random.Next(enchantments.Count)]);
		}
	}

	private void AddRandomResistance(GeneratedArmor armor) {
		var resistanceTypes = new[] {
			DamageType.Fire,
			DamageType.Cold,
			DamageType.Lightning,
			DamageType.Poison,
			DamageType.Necrotic
		};

		var type = resistanceTypes[k_random.Next(resistanceTypes.Length)];
		armor.Resistances[type] = armor.Resistances.GetValueOrDefault(type) + 0.1f;
	}

	#endregion
}

/// <summary>
/// Types of upgrades that can be applied to items.
/// </summary>
public enum ItemUpgradeType {
	DamageBonus,
	AttackBonus,
	ExtraDamageDie,
	UpgradeDieSize,
	ArmorBonus,
	Resistance,
	StatBonus,
	Enchantment,
	Durability
}

/// <summary>
/// Represents an upgrade option for an item.
/// </summary>
public readonly struct ItemUpgrade {
	public ItemUpgradeType Type { get; }
	public int Cost { get; }
	public StatProto.ID? StatId { get; }

	public ItemUpgrade(ItemUpgradeType type, int cost, StatProto.ID? statId = null) {
		Type = type;
		Cost = cost;
		StatId = statId;
	}
}