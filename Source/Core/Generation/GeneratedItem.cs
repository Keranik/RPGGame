using RPGGame.Core.Effects;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Generation;

/// <summary>
/// Base class for procedurally generated items.
/// </summary>
public abstract class GeneratedItem {
	#region Properties

	/// <summary>Rolled rarity for this item.</summary>
	public RarityType Rarity { get; }

	/// <summary>Unique instance ID.</summary>
	public string InstanceId { get; }

	/// <summary>Bonus durability from upgrades.</summary>
	public int BonusDurability { get; set; }

	/// <summary>Stat bonuses from upgrades.</summary>
	public Dictionary<Proto.ID, int> StatBonuses { get; } = [];

	/// <summary>Enchantments applied to this item.</summary>
	public List<EffectProto.ID> Enchantments { get; } = [];

	#endregion

	#region Abstract Properties

	/// <summary>Base prototype ID.</summary>
	public abstract string BaseProtoId { get; }

	/// <summary>Base item name.</summary>
	public abstract string BaseName { get; }

	/// <summary>Base icon name.</summary>
	public abstract string BaseIconName { get; }

	/// <summary>Base item level.</summary>
	public abstract int BaseItemLevel { get; }

	/// <summary>Base buy price.</summary>
	public abstract int BaseBuyPrice { get; }

	/// <summary>Base durability.</summary>
	public abstract int BaseDurability { get; }

	#endregion

	#region Calculated Properties

	/// <summary>Generated name with rarity prefix and suffix.</summary>
	public virtual string DisplayName {
		get {
			string prefix = GetRarityPrefix();
			string suffix = GetEnchantmentSuffix();
			return $"{prefix}{BaseName}{suffix}".Trim();
		}
	}

	/// <summary>Final durability.</summary>
	public int FinalDurability => BaseDurability + BonusDurability;

	/// <summary>Adjusted buy price based on rarity and upgrades.</summary>
	public int FinalBuyPrice => (int)(BaseBuyPrice * GetRarityPriceMultiplier() * GetUpgradeMultiplier());

	/// <summary>Sell price (half of buy).</summary>
	public int FinalSellPrice => FinalBuyPrice / 2;

	#endregion

	#region Constructor

	protected GeneratedItem(RarityType rarity) {
		Rarity = rarity;
		InstanceId = Guid.NewGuid().ToString("N")[..8];
	}

	#endregion

	#region Methods

	public void AddStatBonus(StatProto.ID stat, int amount) {
		StatBonuses[stat] = StatBonuses.GetValueOrDefault(stat) + amount;
	}

	public void AddStatBonus(Proto.ID target, int amount) {
		StatBonuses[target] = StatBonuses.GetValueOrDefault(target) + amount;
	}

	/// <summary>
	/// Gets all equipment stats for this item.
	/// </summary>
	public virtual List<EquipmentStat> GetEquipmentStats() {
		var stats = new List<EquipmentStat>();

		foreach (var (target, value) in StatBonuses) {
			stats.Add(EquipmentStat.Flat(target, value));
		}

		return stats;
	}

	protected string GetRarityPrefix() {
		return Rarity switch {
			RarityType.Uncommon => "Fine ",
			RarityType.Rare => "Superior ",
			RarityType.Epic => "Exquisite ",
			RarityType.Legendary => "Legendary ",
			RarityType.Mythic => "Mythic ",
			_ => ""
		};
	}

	protected string GetEnchantmentSuffix() {
		if (Enchantments.Count == 0) {
			return "";
		}

		// TODO: Generate suffix based on enchantment type
		return Enchantments.Count switch {
			1 => " of Power",
			2 => " of Might",
			>= 3 => " of Legends",
			_ => ""
		};
	}

	protected float GetRarityPriceMultiplier() {
		return Rarity switch {
			RarityType.Common => 1.0f,
			RarityType.Uncommon => 1.5f,
			RarityType.Rare => 2.5f,
			RarityType.Epic => 5.0f,
			RarityType.Legendary => 10.0f,
			RarityType.Mythic => 25.0f,
			_ => 1.0f
		};
	}

	protected float GetUpgradeMultiplier() {
		int totalBonuses = StatBonuses.Values.Sum() + Enchantments.Count * 5;
		return 1.0f + (totalBonuses * 0.1f);
	}

	#endregion
}