using RPGGame.Core.Combat;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Item.Equipment.Armor;

namespace RPGGame.Core.Generation;

/// <summary>
/// A procedurally generated armor instance.
/// </summary>
public class GeneratedArmor : GeneratedItem {
	#region Properties

	/// <summary>Base armor prototype.</summary>
	public ArmorProto BaseProto { get; }

	/// <summary>Bonus AC from upgrades.</summary>
	public int BonusAC { get; set; }

	/// <summary>Damage resistances from upgrades.</summary>
	public Dictionary<DamageType, float> Resistances { get; } = [];

	#endregion

	#region Calculated Properties

	/// <summary>Final armor class bonus.</summary>
	public int FinalArmorBonus { get; private set; }

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
			string enhanceStr = BonusAC > 0 ? $"+{BonusAC} " : "";

			return $"{prefix}{enhanceStr}{BaseName}{suffix}".Trim();
		}
	}

	#endregion

	#region Constructor

	public GeneratedArmor(ArmorProto baseProto, RarityType rarity) : base(rarity) {
		BaseProto = baseProto;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Finalizes all stats after generation.
	/// </summary>
	public void FinalizeStats() {
		FinalArmorBonus = BaseProto.ArmorBonus + BonusAC;
	}

	public override List<EquipmentStat> GetEquipmentStats() {
		var stats = base.GetEquipmentStats();

		if (BonusAC > 0) {
			stats.Add(EquipmentStat.Flat(Ids.Stats.Combat.ArmorClass, BonusAC));
		}

		// Add resistances as stats
		foreach (var (damageType, amount) in Resistances) {
			var resistStat = damageType switch {
				DamageType.Physical => Ids.Stats.Resistances.Physical,
				DamageType.Fire => Ids.Stats.Resistances.Fire,
				DamageType.Cold => Ids.Stats.Resistances.Cold,
				DamageType.Lightning => Ids.Stats.Resistances.Lightning,
				DamageType.Poison => Ids.Stats.Resistances.Poison,
				DamageType.Acid => Ids.Stats.Resistances.Acid,
				DamageType.Holy => Ids.Stats.Resistances.Holy,
				DamageType.Necrotic => Ids.Stats.Resistances.Necrotic,
				DamageType.Psychic => Ids.Stats.Resistances.Psychic,
				DamageType.Force => Ids.Stats.Resistances.Force,
				_ => Ids.Stats.Resistances.Magical
			};
			stats.Add(EquipmentStat.Percent(resistStat, amount * 100));
		}

		return stats;
	}

	/// <summary>
	/// Gets resistance value for a damage type.
	/// </summary>
	public float GetResistance(DamageType type) {
		return Resistances.GetValueOrDefault(type);
	}

	#endregion

	#region Debug

	public override string ToString() {
		var resistStr = Resistances.Count > 0
			? $" Resists:[{string.Join(",", Resistances.Select(r => $"{r.Key}:{r.Value:P0}"))}]"
			: "";

		return $"{DisplayName} ({Rarity}) - AC:+{FinalArmorBonus}{resistStr}";
	}

	#endregion
}