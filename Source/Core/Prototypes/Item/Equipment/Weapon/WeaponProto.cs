using RPGGame.Core.Characters;
using RPGGame.Core.Combat;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Item.Equipment;

namespace RPGGame.Core.Prototypes.Item.Equipment.Weapon;

/// <summary>
/// Prototype for weapon items.
/// </summary>
public class WeaponProto : EquipmentProto {
	// Define the prefix specific to weapons
	private const string WEAPON_PREFIX = "Weapon_";

	// Override the ID with weapon-specific logic
	new public ID Id { get; }

	// WeaponProto-specific ID struct
	new public readonly struct ID : IEquatable<ID>, IComparable<ID> {
		public readonly string Value;

		public ID(string value) {
			Value = value;
		}

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(Proto.ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, Proto.ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(Proto.ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, Proto.ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);

		public static implicit operator EquipmentProto.ID(ID weaponId) => new EquipmentProto.ID(weaponId.Value);
		public static implicit operator ItemProto.ID(ID weaponId) => new ItemProto.ID(weaponId.Value);
		public static implicit operator Proto.ID(ID weaponId) => new Proto.ID(weaponId.Value);
	}

	// ═══════════════════════════════════════════════════════════════
	// Weapon Properties
	// ═══════════════════════════════════════════════════════════════
	public WeaponType WeaponType { get; }
	public HitDice DamageDice { get; }
	public DamageType DamageType { get; }

	// ═══════════════════════════════════════════════════════════════
	// Ranged Properties
	// ═══════════════════════════════════════════════════════════════
	public int Range { get; }
	public AmmoType? RequiredAmmo { get; }

	/// <summary>
	/// Creates a new weapon prototype.
	/// </summary>
	public WeaponProto(
		ID id,
		Loc text,
		string iconName,
		RarityType rarity,
		int buyPrice,
		int itemLevel,
		SlotType equipSlot,
		WeaponType weaponType,
		HitDice damageDice,
		DamageType damageType,
		int requiredLevel = 0,
		int? sellPrice = null,
		IReadOnlyList<EquipmentStat>? equipStats = null,
		float weight = 1f,
		int durability = 100,
		bool isTwoHanded = false,
		int range = 1,
		AmmoType? requiredAmmo = null,
		IReadOnlyList<CharacterClassProto.ID>? classRestrictions = null,
		string? flavorText = null
	) : base(
		id: new EquipmentProto.ID(id.Value),
		text: text,
		iconName: iconName,
		category: ItemCategory.Weapon,
		type: ItemType.Weapon,
		rarity: rarity,
		buyPrice: buyPrice,
		equipSlot: equipSlot,
		itemLevel: itemLevel,
		requiredLevel: requiredLevel,
		sellPrice: sellPrice,
		equipStats: equipStats,
		weight: weight,
		durability: durability,
		isTwoHanded: isTwoHanded,
		classRestrictions: classRestrictions,
		flavorText: flavorText
	) {
		Id = id;
		WeaponType = weaponType;
		DamageDice = damageDice;
		DamageType = damageType;
		Range = range;
		RequiredAmmo = requiredAmmo;
	}
}