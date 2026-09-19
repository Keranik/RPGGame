using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Item.Equipment.Weapon;

namespace RPGGame.Core.Generation;

/// <summary>
/// A procedurally generated weapon instance.
/// </summary>
public class GeneratedWeapon : GeneratedItem {
	#region Properties

	/// <summary>Base weapon prototype.</summary>
	public WeaponProto BaseProto { get; }

	/// <summary>Bonus attack from upgrades.</summary>
	public int BonusAttack { get; set; }

	/// <summary>Bonus damage from upgrades.</summary>
	public int BonusDamage { get; set; }

	/// <summary>Extra damage dice added.</summary>
	public int ExtraDamageDice { get; set; }

	/// <summary>Die size upgrades.</summary>
	public int DieSizeUpgrades { get; set; }

	#endregion

	#region Calculated Properties

	/// <summary>Final damage dice after upgrades.</summary>
	public HitDice FinalDamageDice { get; private set; }

	/// <summary>Final attack bonus.</summary>
	public int FinalAttackBonus { get; private set; }

	/// <summary>Final damage bonus.</summary>
	public int FinalDamageBonus { get; private set; }

	#endregion

	#region Overrides

	public override string BaseProtoId => BaseProto.Id.Value;
	public override string BaseName => BaseProto.DisplayText.Name;
	public override string BaseIconName => BaseProto.IconName;
	public override int BaseItemLevel => BaseProto.ItemLevel;
	public override int BaseBuyPrice => BaseProto.BuyPrice;
	public override int BaseDurability => BaseProto.Durability;

	public override string DisplayName {
		get {
			string prefix = GetRarityPrefix();
			string suffix = GetEnchantmentSuffix();

			// Add enhancement bonus to name if applicable
			int enhancement = Math.Max(BonusAttack, BonusDamage);
			string enhanceStr = enhancement > 0 ? $"+{enhancement} " : "";

			return $"{prefix}{enhanceStr}{BaseName}{suffix}".Trim();
		}
	}

	#endregion

	#region Constructor

	public GeneratedWeapon(WeaponProto baseProto, RarityType rarity) : base(rarity) {
		BaseProto = baseProto;
	}

	#endregion

	#region Methods

	public bool CanUpgradeDieSize() {
		int currentSize = GetCurrentDieSize();
		return currentSize < 12;
	}

	private int GetCurrentDieSize() {
		int baseSize = BaseProto.DamageDice.SidesOfDice;
		int[] sizes = [4, 6, 8, 10, 12];
		int currentIndex = Array.IndexOf(sizes, baseSize);
		if (currentIndex < 0) currentIndex = 1;

		int upgradedIndex = Math.Min(currentIndex + DieSizeUpgrades, sizes.Length - 1);
		return sizes[upgradedIndex];
	}

	/// <summary>
	/// Finalizes all stats after generation.
	/// </summary>
	public void FinalizeStats() {
		// Calculate damage dice
		int diceCount = BaseProto.DamageDice.NumberOfDice + ExtraDamageDice;
		int dieSize = GetCurrentDieSize();
		int modifier = BaseProto.DamageDice.Bonus + BonusDamage;
		FinalDamageDice = new HitDice(diceCount, dieSize, modifier);

		// Attack and damage bonuses
		FinalAttackBonus = BonusAttack;
		FinalDamageBonus = BonusDamage;
	}

	public override List<EquipmentStat> GetEquipmentStats() {
		var stats = base.GetEquipmentStats();

		if (BonusAttack > 0) {
			stats.Add(EquipmentStat.Flat(Ids.Stats.Combat.AttackBonus, BonusAttack));
		}
		if (BonusDamage > 0) {
			stats.Add(EquipmentStat.Flat(Ids.Stats.Combat.DamBonus, BonusDamage));
		}

		return stats;
	}

	#endregion

	#region Debug

	public override string ToString() {
		return $"{DisplayName} ({Rarity}) - {FinalDamageDice} {BaseProto.DamageType} " +
			   $"ATK:+{FinalAttackBonus} DMG:+{FinalDamageBonus}";
	}

	#endregion
}