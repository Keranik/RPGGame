using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Stats;

namespace RPGGame.Core.Stats;

/// <summary>
/// Manages stat calculations, modifiers, and derived stats.
/// Entities hold StatValues; this manager provides calculation services.
/// </summary>
public class StatManager {
	private readonly GameDb k_gameDb;

	public StatManager(GameDb gameDb) {
		k_gameDb = gameDb;
	}

	#region Attribute Helpers

	/// <summary>
	/// Gets the D&D-style attribute modifier for a value.
	/// (value - 10) / 2, rounded down.
	/// </summary>
	public static int GetAttributeModifier(float attributeValue) {
		return (int)MathF.Floor((attributeValue - 10f) / 2f);
	}

	/// <summary>
	/// Gets the attribute modifier for a stat value.
	/// </summary>
	public int GetAttributeModifier(StatValues values, StatProto.ID attributeId) {
		return GetAttributeModifier(values.Get(attributeId));
	}

	/// <summary>
	/// Checks if a stat is a primary attribute.
	/// </summary>
	public bool IsPrimaryAttribute(StatProto.ID statId) {
		return statId == Ids.Stats.Attributes.Strength ||
		       statId == Ids.Stats.Attributes.Dexterity ||
		       statId == Ids.Stats.Attributes.Constitution ||
		       statId == Ids.Stats.Attributes.Intelligence ||
		       statId == Ids.Stats.Attributes.Wisdom ||
		       statId == Ids.Stats.Attributes.Charisma;
	}

	/// <summary>
	/// Gets all primary attribute IDs.
	/// </summary>
	public static IReadOnlyList<StatProto.ID> PrimaryAttributes => [
		Ids.Stats.Attributes.Strength,
		Ids.Stats.Attributes.Dexterity,
		Ids.Stats.Attributes.Constitution,
		Ids.Stats.Attributes.Intelligence,
		Ids.Stats.Attributes.Wisdom,
		Ids.Stats.Attributes.Charisma
	];

	#endregion

	#region Default Values

	/// <summary>
	/// Gets the default value for a stat from its proto.
	/// </summary>
	public float GetDefaultValue(StatProto.ID statId) {
		return k_gameDb.TryGetProto<StatProto>(statId, out var proto)
			? proto.DefaultValue
			: 0f;
	}

	/// <summary>
	/// Creates a StatValues with all default values populated.
	/// </summary>
	public StatValues CreateDefaultStats() {
		var values = new StatValues();
		foreach (var proto in k_gameDb.GetAll<StatProto>()) {
			values.Set(proto.Id, proto.DefaultValue);
		}
		return values;
	}

	/// <summary>
	/// Creates a StatValues with only specified stats.
	/// </summary>
	public StatValues CreateStats(params (StatProto.ID stat, float value)[] initial) {
		var values = new StatValues();
		foreach (var (stat, value) in initial) {
			values.Set(stat, value);
		}
		return values;
	}

	#endregion

	#region Calculated Values

	/// <summary>
	/// Calculates the final value of a stat with all modifiers applied.
	/// </summary>
	public float Calculate(
		StatProto.ID statId,
		StatValues baseValues,
		IEnumerable<StatModifier> modifiers) {

		float baseValue = baseValues.Get(statId, GetDefaultValue(statId));

		// Get modifiers for this stat
		var statModifiers = modifiers
			.Where(m => m.StatId == statId && m.IsActive)
			.ToList();

		if (statModifiers.Count == 0) {
			return ClampStat(statId, baseValue);
		}

		// Apply using ModifierCalculator
		var valueModifiers = statModifiers
			.Select(m => m.ToValueModifier())
			.ToList();

		float result = ModifierCalculator.Calculate(baseValue, valueModifiers);
		return ClampStat(statId, result);
	}

	/// <summary>
	/// Calculates all stats at once.
	/// </summary>
	public StatValues CalculateAll(
		StatValues baseValues,
		IEnumerable<StatModifier> modifiers) {

		var result = new StatValues();
		var modifierList = modifiers.ToList();

		// Get all stat IDs from base values and modifiers
		var allStatIds = baseValues.GetAllStats()
			.Union(modifierList.Select(m => m.StatId))
			.Distinct();

		foreach (var statId in allStatIds) {
			result.Set(statId, Calculate(statId, baseValues, modifierList));
		}

		return result;
	}

	/// <summary>
	/// Clamps a value to a stat's constraints.
	/// </summary>
	public float ClampStat(StatProto.ID statId, float value) {
		if (k_gameDb.TryGetProto<StatProto>(statId, out var proto)) {
			return proto.Clamp(value);
		}
		return value;
	}

	#endregion

	#region Derived Stats

	/// <summary>
	/// Calculates derived stats based on base stats.
	/// </summary>
	public void CalculateDerivedStats(StatValues values) {
		foreach (var proto in k_gameDb.GetAll<StatProto>()) {
			if (!proto.IsDerived) continue;

			float derivedValue = proto.DefaultValue;

			foreach (var derivation in proto.DerivedFrom) {
				float sourceValue = values.Get(derivation.SourceStat);

				if (derivation.UseModifier) {
					// D&D-style attribute modifier: (value - 10) / 2
					sourceValue = MathF.Floor((sourceValue - 10f) / 2f);
				}

				derivedValue += (sourceValue * derivation.Multiplier) + derivation.Offset;
			}

			values.Set(proto.Id, derivedValue);
		}
	}

	#endregion

	#region Queries

	/// <summary>
	/// Gets a stat proto by ID.
	/// </summary>
	public StatProto? GetStatProto(StatProto.ID statId) {
		return k_gameDb.TryGetProto<StatProto>(statId, out var proto) ? proto : null;
	}

	/// <summary>
	/// Gets all stats in a category.
	/// </summary>
	public IEnumerable<StatProto> GetStatsByCategory(StatCategoryProto.ID categoryId) {
		return k_gameDb.GetAll<StatProto>().Where(p => p.Category == categoryId);
	}

	/// <summary>
	/// Gets all stats with a specific tag.
	/// </summary>
	public IEnumerable<StatProto> GetStatsByTag(TagProto.ID tagId) {
		return k_gameDb.GetAll<StatProto>().Where(p => p.HasTag(tagId));
	}

	/// <summary>
	/// Gets all stat categories.
	/// </summary>
	public IEnumerable<StatCategoryProto> GetAllCategories() {
		return k_gameDb.GetAll<StatCategoryProto>().OrderBy(c => c.DisplayOrder);
	}

	/// <summary>
	/// Gets all resistance stats.
	/// </summary>
	public IEnumerable<StatProto> GetResistances() {
		return GetStatsByCategory(Ids.StatCategories.Resistances);
	}

	/// <summary>
	/// Gets all damage bonus stats.
	/// </summary>
	public IEnumerable<StatProto> GetDamageBonuses() {
		return GetStatsByCategory(Ids.StatCategories.DamageBonuses);
	}

	#endregion

	#region Display Helpers

	/// <summary>
	/// Formats a stat value for display using the stat's format settings.
	/// </summary>
	public string FormatStatValue(StatProto.ID statId, float value) {
		if (k_gameDb.TryGetProto<StatProto>(statId, out var proto)) {
			return proto.FormatValue(value);
		}
		return value.ToString("F0");
	}

	/// <summary>
	/// Gets the display name for a stat.
	/// </summary>
	public string GetStatDisplayName(StatProto.ID statId) {
		if (k_gameDb.TryGetProto<StatProto>(statId, out var proto)) {
			return proto.DisplayText.Name;
		}
		return statId.Value;
	}

	/// <summary>
	/// Gets the abbreviation for a stat.
	/// </summary>
	public string GetStatAbbreviation(StatProto.ID statId) {
		if (k_gameDb.TryGetProto<StatProto>(statId, out var proto)) {
			return proto.Abbreviation ?? proto.DisplayText.Name[..Math.Min(3, proto.DisplayText.Name.Length)].ToUpper();
		}
		return statId.Value[..Math.Min(3, statId.Value.Length)].ToUpper();
	}

	#endregion
}