using RPGGame.Core.Characters;
using RPGGame.Core.Generation;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Prototypes.Characters;

/// <summary>
/// Prototype for character class definitions.
/// </summary>
public class CharacterClassProto : Proto {
	#region Strongly-Typed ID

	new public readonly struct ID(string value) : IEquatable<ID>, IComparable<ID> {
		public readonly string Value = value;

		public bool Equals(ID other) => Value == other.Value;
		public int CompareTo(ID other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
		public override string ToString() => Value;
		public override bool Equals(object? obj) => obj is ID other && Equals(other);
		public override int GetHashCode() => Value?.GetHashCode() ?? 0;

		public static bool operator ==(Proto.ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, Proto.ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(Proto.ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, Proto.ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator ==(ID lhs, ID rhs) => string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static bool operator !=(ID lhs, ID rhs) => !string.Equals(lhs.Value, rhs.Value, StringComparison.Ordinal);
		public static implicit operator Proto.ID(ID id) => new(id.Value);
	}

	new public ID Id => new(base.Id.Value);

	#endregion

	#region Core Properties

	public string Tagline { get; }
	public string IconName { get; }
	public string PortraitName { get; }
	public HitDice HitDice { get; }
	public int BaseHealth { get; }
	public int BaseMana { get; }
	public int BaseArmorClass { get; }
	public ClassAttributes BaseAttributes { get; }
	public int HealthPerLevel { get; }
	public int ManaPerLevel { get; }

	/// <summary>The primary attribute for this class.</summary>
	public StatProto.ID PrimaryAttribute { get; }

	/// <summary>The secondary attribute for this class.</summary>
	public StatProto.ID SecondaryAttribute { get; }

	public SpellcastingType SpellcastingType { get; }
	public ClassRole Role { get; }
	public int Difficulty { get; }

	#endregion

	#region Proficiencies

	public List<WeaponType> WeaponProficiencies { get; }
	public List<ArmorType> ArmorProficiencies { get; }

	#endregion

	#region Starting Skills & Spells

	/// <summary>
	/// Skill school tags this class starts with access to.
	/// Players can pick skills from these schools without purchasing school access.
	/// </summary>
	public List<TagProto.ID> StartingSkillSchools { get; }

	/// <summary>
	/// Spell school tags this class starts with access to.
	/// Spellcasters can pick spells from these schools without purchasing school access.
	/// </summary>
	public List<TagProto.ID> StartingSpellSchools { get; }

	/// <summary>
	/// Skills automatically granted at character creation (free, no cost).
	/// </summary>
	public List<SkillProto.ID> StartingSkills { get; }

	/// <summary>
	/// Spells automatically granted at character creation (free, no cost).
	/// </summary>
	public List<SpellProto.ID> StartingSpells { get; }

	/// <summary>
	/// Bonus creation points for this class (positive = easier, negative = harder).
	/// </summary>
	public int BonusCreationPoints { get; }

	#endregion

	#region Starting Equipment & Resources

	public List<Proto.ID> StartingEquipment { get; }
	public int StartingGold { get; }

	#endregion

	#region Unlock Requirements

	public bool StartsUnlocked { get; }
	public string UnlockHint { get; }
	public int RequiredFogClears { get; }

	#endregion

	#region Constructor

	public CharacterClassProto(
		ID id,
		Loc text,
		string tagline = "",
		string iconName = "icon_class",
		string portraitName = "portrait_default",
		HitDice hitDice = default,
		int baseHealth = 10,
		int baseMana = 0,
		int baseArmorClass = 10,
		ClassAttributes? baseAttributes = null,
		int healthPerLevel = 5,
		int manaPerLevel = 0,
		StatProto.ID? primaryAttribute = null,
		StatProto.ID? secondaryAttribute = null,
		SpellcastingType spellcastingType = SpellcastingType.None,
		ClassRole role = ClassRole.Melee,
		int difficulty = 2,
		List<WeaponType>? weaponProficiencies = null,
		List<ArmorType>? armorProficiencies = null,
		List<TagProto.ID>? startingSkillSchools = null,
		List<TagProto.ID>? startingSpellSchools = null,
		List<SkillProto.ID>? startingSkills = null,
		List<SpellProto.ID>? startingSpells = null,
		int bonusCreationPoints = 0,
		List<Proto.ID>? startingEquipment = null,
		int startingGold = 10,
		bool startsUnlocked = false,
		string unlockHint = "",
		int requiredFogClears = 0
	) : base(id, text) {
		Tagline = tagline;
		IconName = iconName;
		PortraitName = portraitName;
		HitDice = hitDice.SidesOfDice == 0 ? Core.HitDice.D8 : hitDice;
		BaseHealth = baseHealth;
		BaseMana = baseMana;
		BaseArmorClass = baseArmorClass;
		BaseAttributes = baseAttributes ?? new ClassAttributes();
		HealthPerLevel = healthPerLevel;
		ManaPerLevel = manaPerLevel;
		PrimaryAttribute = primaryAttribute ?? Ids.Stats.Attributes.Strength;
		SecondaryAttribute = secondaryAttribute ?? Ids.Stats.Attributes.Constitution;
		SpellcastingType = spellcastingType;
		Role = role;
		Difficulty = difficulty;
		WeaponProficiencies = weaponProficiencies ?? [];
		ArmorProficiencies = armorProficiencies ?? [];
		StartingSkillSchools = startingSkillSchools ?? [];
		StartingSpellSchools = startingSpellSchools ?? [];
		StartingSkills = startingSkills ?? [];
		StartingSpells = startingSpells ?? [];
		BonusCreationPoints = bonusCreationPoints;
		StartingEquipment = startingEquipment ?? [];
		StartingGold = startingGold;
		StartsUnlocked = startsUnlocked;
		UnlockHint = unlockHint;
		RequiredFogClears = requiredFogClears;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Checks if this class has access to a skill school.
	/// </summary>
	public bool HasSkillSchoolAccess(TagProto.ID schoolTag) {
		return StartingSkillSchools.Any(s => s == schoolTag);
	}

	/// <summary>
	/// Checks if this class has access to a spell school.
	/// </summary>
	public bool HasSpellSchoolAccess(TagProto.ID schoolTag) {
		return StartingSpellSchools.Any(s => s == schoolTag);
	}

	/// <summary>
	/// Gets the total creation points available for this class.
	/// </summary>
	public int GetCreationPoints(bool isRandomReroll = false) {
		int basePoints = isRandomReroll
			? CreationPointCosts.RandomRerollCreationPoints
			: CreationPointCosts.CharacterCreationPoints;

		return basePoints + BonusCreationPoints;
	}

	/// <summary>
	/// Checks if this class can cast spells.
	/// </summary>
	public bool CanCastSpells => SpellcastingType != SpellcastingType.None;

	#endregion
}