namespace RPGGame.Core.Generation;

/// <summary>
/// Centralized creation point costs for procedural generation.
/// All costs are scaled by BaseScale to allow global balancing adjustments.
/// </summary>
/// <remarks>
/// Design Principles:
/// - No magic numbers in generation code
/// - Single point of adjustment for game-wide balance changes
/// - Separate base costs from scaling for easy tuning
/// - Stat costs scale with value (higher stats cost more)
/// </remarks>
public static class CreationPointCosts {
	#region Global Scaling

	/// <summary>
	/// Global scale applied to ALL creation point costs.
	/// Set to 1.0 for default. Increase to make everything more expensive.
	/// </summary>
	public static float BaseScale { get; set; } = 1.0f;

	/// <summary>
	/// Applies the global scale to a cost value.
	/// </summary>
	public static int Scale(int baseCost) => (int)Math.Ceiling(baseCost * BaseScale);

	/// <summary>
	/// Applies the global scale to a cost value.
	/// </summary>
	public static float Scale(float baseCost) => baseCost * BaseScale;

	#endregion

	#region Character Creation

	/// <summary>
	/// Base creation points for new characters.
	/// </summary>
	public const int BASE_CHARACTER_POINTS = 25;

	/// <summary>
	/// Bonus multiplier for random reroll characters (hidden bonus).
	/// </summary>
	public const float RANDOM_REROLL_BONUS_MULTIPLIER = 1.20f;

	/// <summary>
	/// Gets the actual character creation points (scaled).
	/// </summary>
	public static int CharacterCreationPoints => Scale(BASE_CHARACTER_POINTS);

	/// <summary>
	/// Gets the random reroll character creation points (with hidden bonus).
	/// </summary>
	public static int RandomRerollCreationPoints => (int)(CharacterCreationPoints * RANDOM_REROLL_BONUS_MULTIPLIER);

	#endregion

	#region Attribute Costs (Scaling)

	/// <summary>
	/// Base costs for attribute increases by current value.
	/// Higher stats cost progressively more.
	/// Index = current stat value (8-20+), Value = cost to increase by 1.
	/// </summary>
	private static readonly Dictionary<int, int> AttributeCostByValue = new() {
		{ 8, 2 },   // 8 → 9 costs 2
		{ 9, 2 },   // 9 → 10 costs 2
		{ 10, 3 },  // 10 → 11 costs 3
		{ 11, 3 },  // 11 → 12 costs 3
		{ 12, 4 },  // 12 → 13 costs 4
		{ 13, 4 },  // 13 → 14 costs 4
		{ 14, 6 },  // 14 → 15 costs 6
		{ 15, 6 },  // 15 → 16 costs 6
		{ 16, 9 },  // 16 → 17 costs 9
		{ 17, 9 },  // 17 → 18 costs 9
		{ 18, 12 }, // 18 → 19 costs 12
		{ 19, 12 }, // 19 → 20 costs 12
		{ 20, 15 }  // 20+ costs 15 each
	};

	/// <summary>
	/// Gets the cost to increase an attribute from its current value.
	/// </summary>
	public static int GetAttributeIncreaseCost(int currentValue) {
		if (currentValue < 8) return Scale(1);
		if (AttributeCostByValue.TryGetValue(currentValue, out int cost)) {
			return Scale(cost);
		}
		// Above 20, use the max cost
		return Scale(15);
	}

	/// <summary>
	/// Gets the total cost to raise an attribute from one value to another.
	/// </summary>
	public static int GetAttributeTotalCost(int fromValue, int toValue) {
		if (toValue <= fromValue) return 0;

		int total = 0;
		for (int v = fromValue; v < toValue; v++) {
			total += GetAttributeIncreaseCost(v);
		}
		return total;
	}

	#endregion

	#region Derived Stat Costs

	/// <summary>Base cost per point of bonus HP.</summary>
	public const int BASE_HEALTH_COST = 1;

	/// <summary>Base cost per point of bonus Mana.</summary>
	public const int BASE_MANA_COST = 2;

	/// <summary>Base cost per point of bonus Stamina.</summary>
	public const int BASE_STAMINA_COST = 1;

	/// <summary>Base cost per point of Armor Class.</summary>
	public const int BASE_ARMOR_CLASS_COST = 8;

	/// <summary>Base cost per point of Attack Bonus.</summary>
	public const int BASE_ATTACK_BONUS_COST = 6;

	/// <summary>Base cost per point of Damage Bonus.</summary>
	public const int BASE_DAMAGE_BONUS_COST = 5;

	/// <summary>Base cost per point of Spell Power.</summary>
	public const int BASE_SPELL_POWER_COST = 5;

	/// <summary>Base cost per point of Initiative.</summary>
	public const int BASE_INITIATIVE_COST = 4;

	// Scaled accessors
	public static int HealthCost => Scale(BASE_HEALTH_COST);
	public static int ManaCost => Scale(BASE_MANA_COST);
	public static int StaminaCost => Scale(BASE_STAMINA_COST);
	public static int ArmorClassCost => Scale(BASE_ARMOR_CLASS_COST);
	public static int AttackBonusCost => Scale(BASE_ATTACK_BONUS_COST);
	public static int DamageBonusCost => Scale(BASE_DAMAGE_BONUS_COST);
	public static int SpellPowerCost => Scale(BASE_SPELL_POWER_COST);
	public static int InitiativeCost => Scale(BASE_INITIATIVE_COST);

	#endregion

	#region Skill Costs

	/// <summary>Base cost to learn a new skill (rank 1).</summary>
	public const int BASE_SKILL_LEARN_COST = 5;

	/// <summary>Base cost to increase a skill by one rank.</summary>
	public const int BASE_SKILL_RANK_COST = 3;

	/// <summary>Cost multiplier for mastery-tier skills (rank 4+).</summary>
	public const float SKILL_MASTERY_MULTIPLIER = 1.5f;

	/// <summary>Cost multiplier for grandmastery-tier skills (rank 7+).</summary>
	public const float SKILL_GRANDMASTERY_MULTIPLIER = 2.0f;

	/// <summary>
	/// Gets the cost to increase a skill from one rank to the next.
	/// </summary>
	public static int GetSkillRankCost(int currentRank) {
		int baseCost = currentRank == 0 ? BASE_SKILL_LEARN_COST : BASE_SKILL_RANK_COST;

		if (currentRank >= 7) {
			baseCost = (int)(baseCost * SKILL_GRANDMASTERY_MULTIPLIER);
		} else if (currentRank >= 4) {
			baseCost = (int)(baseCost * SKILL_MASTERY_MULTIPLIER);
		}

		return Scale(baseCost);
	}

	#endregion

	#region Spell Costs

	/// <summary>Base cost to learn a cantrip (level 0 spell).</summary>
	public const int BASE_CANTRIP_COST = 3;

	/// <summary>Base cost multiplier per spell level.</summary>
	public const int BASE_SPELL_LEVEL_COST = 5;

	/// <summary>
	/// Gets the cost to learn a spell of the given level.
	/// </summary>
	public static int GetSpellLearnCost(int spellLevel) {
		if (spellLevel <= 0) return Scale(BASE_CANTRIP_COST);
		return Scale(BASE_CANTRIP_COST + (spellLevel * BASE_SPELL_LEVEL_COST));
	}

	#endregion

	#region Spell School / Skill School Costs

	/// <summary>Base cost to unlock access to a new spell school.</summary>
	public const int BASE_SPELL_SCHOOL_COST = 15;

	/// <summary>Base cost to unlock access to a new skill category.</summary>
	public const int BASE_SKILL_CATEGORY_COST = 10;

	public static int SpellSchoolCost => Scale(BASE_SPELL_SCHOOL_COST);
	public static int SkillCategoryCost => Scale(BASE_SKILL_CATEGORY_COST);

	#endregion

	#region Proficiency Costs

	/// <summary>Base cost to gain proficiency with a weapon type.</summary>
	public const int BASE_WEAPON_PROFICIENCY_COST = 8;

	/// <summary>Base cost to gain proficiency with an armor type.</summary>
	public const int BASE_ARMOR_PROFICIENCY_COST = 10;

	public static int WeaponProficiencyCost => Scale(BASE_WEAPON_PROFICIENCY_COST);
	public static int ArmorProficiencyCost => Scale(BASE_ARMOR_PROFICIENCY_COST);

	#endregion

	#region Tag/Package Costs

	/// <summary>
	/// Gets the creation point cost for an entity tag.
	/// Tags define their own costs in TagProto.
	/// This provides a fallback/default.
	/// </summary>
	public const int DEFAULT_TAG_COST = 10;

	public static int DefaultTagCost => Scale(DEFAULT_TAG_COST);

	#endregion

	#region Enemy/Item Generation (Preserved from existing system)

	/// <summary>Cost to add an extra damage die to enemy/weapon.</summary>
	public const int EXTRA_DAMAGE_DIE_COST = 15;

	/// <summary>Cost to upgrade damage die size.</summary>
	public const int UPGRADE_DIE_SIZE_COST = 10;

	/// <summary>Cost to add a new ability to enemy.</summary>
	public const int NEW_ABILITY_COST = 25;

	/// <summary>Cost to add an enchantment to item.</summary>
	public const int ENCHANTMENT_COST = 25;

	/// <summary>Cost per point of durability.</summary>
	public const int DURABILITY_COST = 5;

	// Scaled versions
	public static int ExtraDamageDieCost => Scale(EXTRA_DAMAGE_DIE_COST);
	public static int UpgradeDieSizeCost => Scale(UPGRADE_DIE_SIZE_COST);
	public static int NewAbilityCost => Scale(NEW_ABILITY_COST);
	public static int EnchantmentCost => Scale(ENCHANTMENT_COST);
	public static int DurabilityCost => Scale(DURABILITY_COST);

	#endregion

	#region Minimum / Maximum Bounds

	/// <summary>Minimum attribute value (cannot go below).</summary>
	public const int MIN_ATTRIBUTE_VALUE = 3;

	/// <summary>Maximum attribute value during character creation.</summary>
	public const int MAX_CREATION_ATTRIBUTE_VALUE = 18;

	/// <summary>Absolute maximum attribute value (with all bonuses).</summary>
	public const int MAX_ATTRIBUTE_VALUE = 30;

	/// <summary>Maximum skill rank during character creation.</summary>
	public const int MAX_CREATION_SKILL_RANK = 3;

	/// <summary>Absolute maximum skill rank.</summary>
	public const int MAX_SKILL_RANK = 10;

	/// <summary>Maximum number of spells learnable during character creation.</summary>
	public const int MAX_CREATION_SPELLS = 6;

	/// <summary>Maximum spell level learnable during character creation.</summary>
	public const int MAX_CREATION_SPELL_LEVEL = 2;

	#endregion
}