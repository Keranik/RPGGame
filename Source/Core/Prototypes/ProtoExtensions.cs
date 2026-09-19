namespace RPGGame.Core.Prototypes;

/// <summary>
/// Extension methods for Proto.ID to identify what type of proto an ID refers to.
/// IDs follow naming conventions: "Stat_*", "Skill_*", "Tag_*", etc.
/// </summary>
public static class ProtoExtensions {
	#region Type Checks

	/// <summary>Checks if this ID refers to a StatProto.</summary>
	public static bool IsStat(this Proto.ID id) => id.Value.StartsWith("Stat_");

	/// <summary>Checks if this ID refers to a SkillProto.</summary>
	public static bool IsSkill(this Proto.ID id) => id.Value.StartsWith("Skill_");

	/// <summary>Checks if this ID refers to a TagProto.</summary>
	public static bool IsTag(this Proto.ID id) => id.Value.StartsWith("Tag_");

	/// <summary>Checks if this ID refers to an ItemProto.</summary>
	public static bool IsItem(this Proto.ID id) => id.Value.StartsWith("Item_");

	/// <summary>Checks if this ID refers to a SpellProto.</summary>
	public static bool IsSpell(this Proto.ID id) => id.Value.StartsWith("Spell_");

	/// <summary>Checks if this ID refers to an EffectProto.</summary>
	public static bool IsEffect(this Proto.ID id) => id.Value.StartsWith("Effect_");

	/// <summary>Checks if this ID refers to an EnemyProto.</summary>
	public static bool IsEnemy(this Proto.ID id) => id.Value.StartsWith("Enemy_");

	/// <summary>Checks if this ID refers to a LocationProto (Settlement, Dungeon, etc.).</summary>
	public static bool IsLocation(this Proto.ID id) =>
		id.Value.StartsWith("Settlement_") ||
		id.Value.StartsWith("Dungeon_") ||
		id.Value.StartsWith("Landmark_") ||
		id.Value.StartsWith("Camp_") ||
		id.Value.StartsWith("BossArena_");

	/// <summary>Checks if this ID refers to a CharacterClassProto.</summary>
	public static bool IsCharacterClass(this Proto.ID id) => id.Value.StartsWith("Class_");

	/// <summary>Checks if this ID refers to a BuildingProto.</summary>
	public static bool IsBuilding(this Proto.ID id) => id.Value.StartsWith("Building_");

	/// <summary>Checks if this ID refers to an EventProto.</summary>
	public static bool IsEvent(this Proto.ID id) => id.Value.StartsWith("Event_");

	/// <summary>Checks if this ID refers to a ResourceProto.</summary>
	public static bool IsResource(this Proto.ID id) => id.Value.StartsWith("Resource_");

	/// <summary>Checks if this ID refers to an ActivityProto.</summary>
	public static bool IsActivity(this Proto.ID id) => id.Value.StartsWith("Activity_");

	/// <summary>Checks if this ID refers to a LoreProto.</summary>
	public static bool IsLore(this Proto.ID id) => id.Value.StartsWith("Lore_");

	/// <summary>Checks if this ID refers to a StatCategoryProto.</summary>
	public static bool IsStatCategory(this Proto.ID id) => id.Value.StartsWith("StatCategory_");

	#endregion

	#region Category Extraction

	/// <summary>
	/// Extracts the category from an ID.
	/// "Tag_Element_Fire" ? "Element"
	/// "Stat_Attr_Strength" ? "Attr"
	/// </summary>
	public static string GetCategory(this Proto.ID id) {
		var value = id.Value;
		var firstUnderscore = value.IndexOf('_');
		if (firstUnderscore < 0) return "";

		var afterPrefix = value[(firstUnderscore + 1)..];
		var secondUnderscore = afterPrefix.IndexOf('_');
		return secondUnderscore > 0 ? afterPrefix[..secondUnderscore] : afterPrefix;
	}

	/// <summary>
	/// Extracts the name portion from an ID.
	/// "Tag_Element_Fire" ? "Fire"
	/// "Stat_Attr_Strength" ? "Strength"
	/// </summary>
	public static string GetName(this Proto.ID id) {
		var value = id.Value;
		var lastUnderscore = value.LastIndexOf('_');
		return lastUnderscore >= 0 ? value[(lastUnderscore + 1)..] : value;
	}

	/// <summary>
	/// Gets the prefix of an ID.
	/// "Tag_Element_Fire" ? "Tag"
	/// "Stat_Attr_Strength" ? "Stat"
	/// </summary>
	public static string GetPrefix(this Proto.ID id) {
		var value = id.Value;
		var firstUnderscore = value.IndexOf('_');
		return firstUnderscore > 0 ? value[..firstUnderscore] : value;
	}

	#endregion

	#region Stat-Specific Checks

	/// <summary>Checks if this is a primary attribute stat.</summary>
	public static bool IsPrimaryAttribute(this Proto.ID id) {
		return id.IsStat() && id.GetCategory() == "Attr";
	}

	/// <summary>Checks if this is a resistance stat.</summary>
	public static bool IsResistance(this Proto.ID id) {
		return id.IsStat() && id.GetCategory() == "Resist";
	}

	/// <summary>Checks if this is a damage bonus stat.</summary>
	public static bool IsDamageBonus(this Proto.ID id) {
		return id.IsStat() && id.GetCategory() == "DmgBonus";
	}

	/// <summary>Checks if this is a resource stat (health, mana, stamina).</summary>
	public static bool IsResourceStat(this Proto.ID id) {
		return id.IsStat() && id.GetCategory() == "Resource";
	}

	/// <summary>Checks if this is a combat stat.</summary>
	public static bool IsCombatStat(this Proto.ID id) {
		return id.IsStat() && id.GetCategory() == "Combat";
	}

	/// <summary>Checks if this is a defense stat.</summary>
	public static bool IsDefenseStat(this Proto.ID id) {
		return id.IsStat() && id.GetCategory() == "Defense";
	}

	#endregion

	#region Skill-Specific Checks

	/// <summary>Checks if this is a combat skill.</summary>
	public static bool IsCombatSkill(this Proto.ID id) {
		return id.IsSkill() && id.GetCategory() == "Combat";
	}

	/// <summary>Checks if this is a stealth skill.</summary>
	public static bool IsStealthSkill(this Proto.ID id) {
		return id.IsSkill() && id.GetCategory() == "Stealth";
	}

	/// <summary>Checks if this is a crafting skill.</summary>
	public static bool IsCraftingSkill(this Proto.ID id) {
		return id.IsSkill() && id.GetCategory() == "Crafting";
	}

	/// <summary>Checks if this is a gathering skill.</summary>
	public static bool IsGatheringSkill(this Proto.ID id) {
		return id.IsSkill() && id.GetCategory() == "Gathering";
	}

	/// <summary>Checks if this is a magic skill.</summary>
	public static bool IsMagicSkill(this Proto.ID id) {
		return id.IsSkill() && id.GetCategory() == "Magic";
	}

	#endregion

	#region Tag-Specific Checks

	/// <summary>Checks if this is an element tag.</summary>
	public static bool IsElementTag(this Proto.ID id) {
		return id.IsTag() && id.GetCategory() == "Element";
	}

	/// <summary>Checks if this is a creature type tag.</summary>
	public static bool IsCreatureTag(this Proto.ID id) {
		return id.IsTag() && id.GetCategory() == "Creature";
	}

	/// <summary>Checks if this is a region tag.</summary>
	public static bool IsRegionTag(this Proto.ID id) {
		return id.IsTag() && id.GetCategory() == "Region";
	}

	#endregion
}