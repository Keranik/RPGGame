using RPGGame.Core.Combat;
using RPGGame.Core.Effects;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Combat;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Generation;

/// <summary>
/// A procedurally generated enemy instance with modified stats and tags.
/// </summary>
public class GeneratedEnemy {
	#region Base Properties

	/// <summary>The base prototype this enemy was generated from.</summary>
	public EnemyProto BaseProto { get; }

	/// <summary>Rolled rarity for this enemy.</summary>
	public RarityType Rarity { get; }

	/// <summary>Unique instance ID.</summary>
	public string InstanceId { get; }

	#endregion

	#region Stat Bonuses

	/// <summary>Bonus HP from upgrades.</summary>
	public int BonusHealth { get; set; }

	/// <summary>Bonus AC from upgrades.</summary>
	public int BonusAC { get; set; }

	/// <summary>Bonus attack from upgrades.</summary>
	public int BonusAttack { get; set; }

	/// <summary>Bonus damage from upgrades.</summary>
	public int BonusDamage { get; set; }

	/// <summary>Bonus initiative from upgrades.</summary>
	public int BonusInitiative { get; set; }

	/// <summary>Attribute bonuses from upgrades.</summary>
	public Dictionary<Proto.ID, int> AttributeBonuses { get; } = [];

	/// <summary>
	/// Tag-based modifiers to apply, grouped by target ID.
	/// Each target can have multiple modifiers from different tags.
	/// </summary>
	public Dictionary<Proto.ID, List<ValueModifier>> TagModifiers { get; } = [];

	#endregion

	#region Damage Dice

	/// <summary>Extra damage dice added.</summary>
	public int ExtraDamageDice { get; private set; }

	/// <summary>Die size upgrades (d6 -> d8 = 1 upgrade).</summary>
	public int DieSizeUpgrades { get; private set; }

	#endregion

	#region Tags and Effects

	/// <summary>Tags applied to this enemy.</summary>
	public HashSet<TagProto.ID> Tags { get; } = [];

	/// <summary>Damage resistances (type -> reduction 0-1).</summary>
	public Dictionary<DamageType, float> Resistances { get; } = [];

	/// <summary>Damage vulnerabilities (type -> extra damage 0-1).</summary>
	public Dictionary<DamageType, float> Vulnerabilities { get; } = [];

	/// <summary>Condition immunities.</summary>
	public List<StatusCondition> Immunities { get; } = [];

	/// <summary>Ability IDs granted by tags.</summary>
	public List<Proto.ID> GrantedAbilityIds { get; } = [];

	/// <summary>Effect IDs granted by tags.</summary>
	public List<EffectProto.ID> GrantedEffectIds { get; } = [];

	/// <summary>Abilities from base proto + granted.</summary>
	public List<EnemyAbility> Abilities { get; } = [];

	#endregion

	#region Final Calculated Stats

	/// <summary>Final max health after all modifiers.</summary>
	public int FinalMaxHealth { get; private set; }

	/// <summary>Final armor class after all modifiers.</summary>
	public int FinalArmorClass { get; private set; }

	/// <summary>Final attack bonus after all modifiers.</summary>
	public int FinalAttackBonus { get; private set; }

	/// <summary>Final damage bonus after all modifiers.</summary>
	public int FinalDamageBonus { get; private set; }

	/// <summary>Final initiative bonus after all modifiers.</summary>
	public int FinalInitiative { get; private set; }

	/// <summary>Final damage dice.</summary>
	public HitDice FinalDamageDice { get; private set; }

	/// <summary>Effective level/CR for this enemy.</summary>
	public float EffectiveLevel => BaseProto.ChallengeRating * GetRarityMultiplier();

	#endregion

	#region Display

	/// <summary>Generated name with rarity prefix.</summary>
	public string DisplayName {
		get {
			string prefix = Rarity switch {
				RarityType.Uncommon => "Strong ",
				RarityType.Rare => "Veteran ",
				RarityType.Epic => "Elite ",
				RarityType.Legendary => "Legendary ",
				RarityType.Mythic => "Mythic ",
				_ => ""
			};
			return prefix + BaseProto.Name;
		}
	}

	/// <summary>Experience value adjusted for rarity.</summary>
	public int ExperienceValue => (int)(BaseProto.ExperienceValue * GetRarityMultiplier());

	/// <summary>Gold value adjusted for rarity.</summary>
	public int GoldValue => (int)(BaseProto.GoldValue * GetRarityMultiplier());

	#endregion

	#region Constructor

	public GeneratedEnemy(EnemyProto baseProto, RarityType rarity) {
		BaseProto = baseProto;
		Rarity = rarity;
		InstanceId = Guid.NewGuid().ToString("N")[..8];

		// Copy base abilities
		if (baseProto.Abilities != null) {
			Abilities.AddRange(baseProto.Abilities);
		}

		// Copy base resistances/vulnerabilities/immunities
		if (baseProto.Resistances != null) {
			foreach (var (type, amount) in baseProto.Resistances) {
				Resistances[type] = amount;
			}
		}
		if (baseProto.Vulnerabilities != null) {
			foreach (var (type, amount) in baseProto.Vulnerabilities) {
				Vulnerabilities[type] = amount;
			}
		}
		if (baseProto.Immunities != null) {
			Immunities.AddRange(baseProto.Immunities);
		}
	}

	#endregion

	#region Methods

	/// <summary>
	/// Checks if this enemy has a specific tag.
	/// </summary>
	public bool HasTag(TagProto.ID tagId) => Tags.Contains(tagId);

	/// <summary>
	/// Adds a modifier for a target (stat or skill).
	/// </summary>
	public void AddTagModifier(Proto.ID target, ValueModifier modifier) {
		if (!TagModifiers.TryGetValue(target, out var modifiers)) {
			modifiers = [];
			TagModifiers[target] = modifiers;
		}
		modifiers.Add(modifier);
	}

	/// <summary>
	/// Adds all modifiers from a tag's modifier dictionary.
	/// </summary>
	public void AddTagModifiers(Dictionary<Proto.ID, ValueModifier> modifiers) {
		foreach (var (target, modifier) in modifiers) {
			AddTagModifier(target, modifier);
		}
	}

	public void AddDamageDie() {
		ExtraDamageDice++;
	}

	public bool CanUpgradeDieSize() {
		int currentSize = GetCurrentDieSize();
		return currentSize < 12;
	}

	public void UpgradeDieSize() {
		if (CanUpgradeDieSize()) {
			DieSizeUpgrades++;
		}
	}

	private int GetCurrentDieSize() {
		int baseSize = BaseProto.AttackDamage.SidesOfDice;
		int[] sizes = [4, 6, 8, 10, 12];
		int currentIndex = Array.IndexOf(sizes, baseSize);
		if (currentIndex < 0) currentIndex = 1;

		int upgradedIndex = Math.Min(currentIndex + DieSizeUpgrades, sizes.Length - 1);
		return sizes[upgradedIndex];
	}

	/// <summary>
	/// Gets all modifiers for a specific stat.
	/// </summary>
	public List<ValueModifier> GetModifiersForStat(StatProto.ID statId) {
		var result = new List<ValueModifier>();

		// Add flat bonuses from point spending
		if (statId == Ids.Stats.Resource.MaxHealth && BonusHealth > 0) {
			result.Add(BonusHealth.Flat());
		} else if (statId == Ids.Stats.Combat.ArmorClass && BonusAC > 0) {
			result.Add(BonusAC.Flat());
		} else if (statId == Ids.Stats.Combat.AttackBonus && BonusAttack > 0) {
			result.Add(BonusAttack.Flat());
		} else if (statId == Ids.Stats.Combat.DamBonus && BonusDamage > 0) {
			result.Add(BonusDamage.Flat());
		} else if (statId == Ids.Stats.Combat.Initiative && BonusInitiative > 0) {
			result.Add(BonusInitiative.Flat());
		}

		// Add tag modifiers
		if (TagModifiers.TryGetValue(statId, out var tagMods)) {
			result.AddRange(tagMods);
		}

		return result;
	}

	/// <summary>
	/// Calculates final stats after all modifiers.
	/// </summary>
	public void FinalizeStats() {
		// Calculate finals using ModifierCalculator
		FinalMaxHealth = ModifierCalculator.CalculateInt(
			BaseProto.MaxHealth,
			GetModifiersForStat(Ids.Stats.Resource.MaxHealth)
		);

		FinalArmorClass = ModifierCalculator.CalculateInt(
			BaseProto.ArmorClass,
			GetModifiersForStat(Ids.Stats.Combat.ArmorClass)
		);

		FinalAttackBonus = ModifierCalculator.CalculateInt(
			BaseProto.AttackBonus,
			GetModifiersForStat(Ids.Stats.Combat.AttackBonus)
		);

		FinalDamageBonus = ModifierCalculator.CalculateInt(
			BaseProto.DamageBonus,
			GetModifiersForStat(Ids.Stats.Combat.DamBonus)
		);

		FinalInitiative = ModifierCalculator.CalculateInt(
			BaseProto.InitiativeBonus,
			GetModifiersForStat(Ids.Stats.Combat.Initiative)
		);

		// Calculate damage dice
		int diceCount = BaseProto.AttackDamage.NumberOfDice + ExtraDamageDice;
		int dieSize = GetCurrentDieSize();
		int modifier = BaseProto.AttackDamage.Bonus + FinalDamageBonus;
		FinalDamageDice = new HitDice(diceCount, dieSize, modifier);
	}

	private float GetRarityMultiplier() {
		return Rarity switch {
			RarityType.Common => 1.0f,
			RarityType.Uncommon => 1.25f,
			RarityType.Rare => 1.5f,
			RarityType.Epic => 2.0f,
			RarityType.Legendary => 3.0f,
			RarityType.Mythic => 5.0f,
			_ => 1.0f
		};
	}

	#endregion

	#region Debug

	public override string ToString() {
		return $"{DisplayName} (CR {EffectiveLevel:F1}) - HP:{FinalMaxHealth} AC:{FinalArmorClass} " +
			   $"ATK:+{FinalAttackBonus} DMG:{FinalDamageDice} Tags:[{string.Join(",", Tags.Select(t => t.Value))}]";
	}

	#endregion
}