using RPGGame.Core.Prototypes.Stats;
using UnityEngine;

namespace RPGGame.Core.Stats;

/// <summary>
/// Simple container for stat values.
/// Stores base values; use StatManager for calculated values with modifiers.
/// </summary>
public class StatValues {
	private readonly Dictionary<StatProto.ID, float> k_values = [];

	#region Basic Access

	/// <summary>Gets a stat value, or default if not set.</summary>
	public float Get(StatProto.ID stat, float defaultValue = 0f) {
		return k_values.GetValueOrDefault(stat, defaultValue);
	}

	/// <summary>Gets a stat value as an integer.</summary>
	public int GetInt(StatProto.ID stat, int defaultValue = 0) {
		return k_values.TryGetValue(stat, out var value) ? (int)Math.Floor(value) : defaultValue;
	}

	/// <summary>Sets a stat value.</summary>
	public void Set(StatProto.ID stat, float value) {
		k_values[stat] = value;
	}

	/// <summary>Sets a base stat value. Alias for Set() for semantic clarity.</summary>
	public void SetBase(StatProto.ID stat, float value) => Set(stat, value);

	public void Add(StatProto.ID stat, float amount)
	{
		if (amount < 0)
		{
			Debug.LogError($"Add called with negative amount ({amount}) on {stat}. Use Subtract instead.");
		}
		k_values[stat] = Get(stat) + amount;
	}

	public void Subtract(StatProto.ID stat, float amount)
	{
		if (amount < 0)
		{
			Debug.LogError($"Subtract called with negative amount ({amount}) on {stat}. Use Add instead.");
		}
		k_values[stat] = Get(stat) - amount;
	}

	/// <summary>Checks if a stat has been set.</summary>
	public bool Has(StatProto.ID stat) => k_values.ContainsKey(stat);

	/// <summary>Removes a stat.</summary>
	public bool Remove(StatProto.ID stat) => k_values.Remove(stat);

	/// <summary>Clears all values.</summary>
	public void Clear() => k_values.Clear();

	/// <summary>Gets all stat IDs that have been set.</summary>
	public IEnumerable<StatProto.ID> GetAllStats() => k_values.Keys;

	#endregion

	#region Convenience Methods

	/// <summary>Gets multiple stats at once.</summary>
	public Dictionary<StatProto.ID, float> GetMultiple(params StatProto.ID[] stats) {
		var result = new Dictionary<StatProto.ID, float>();
		foreach (var stat in stats) {
			result[stat] = Get(stat);
		}
		return result;
	}

	/// <summary>Sets multiple stats at once.</summary>
	public void SetMultiple(params (StatProto.ID stat, float value)[] values) {
		foreach (var (stat, value) in values) {
			Set(stat, value);
		}
	}

	/// <summary>Clamps a stat to min/max bounds.</summary>
	public void Clamp(StatProto.ID stat, float min, float max) {
		float current = Get(stat);
		Set(stat, Math.Clamp(current, min, max));
	}

	/// <summary>Ensures a stat doesn't go below a minimum.</summary>
	public void ClampMin(StatProto.ID stat, float min) {
		float current = Get(stat);
		if (current < min) Set(stat, min);
	}

	/// <summary>Ensures a stat doesn't exceed a maximum.</summary>
	public void ClampMax(StatProto.ID stat, float max) {
		float current = Get(stat);
		if (current > max) Set(stat, max);
	}

	/// <summary>
	/// Clamps current resource to its max (e.g., CurrentHealth to MaxHealth).
	/// </summary>
	public void ClampToMax(StatProto.ID currentStat, StatProto.ID maxStat) {
		float max = Get(maxStat);
		float current = Get(currentStat);
		if (current > max) Set(currentStat, max);
	}

	#endregion

	#region Serialization

	/// <summary>Gets all values as a dictionary (for serialization).</summary>
	public Dictionary<StatProto.ID, float> ToDictionary() => new(k_values);

	/// <summary>Converts to serializable data.</summary>
	public StatValuesData ToData() {
		return new StatValuesData {
			Values = k_values.ToDictionary(
				kvp => kvp.Key.Value,
				kvp => kvp.Value
			)
		};
	}

	/// <summary>Creates from a dictionary.</summary>
	public static StatValues FromDictionary(Dictionary<StatProto.ID, float> values) {
		var result = new StatValues();
		foreach (var (stat, value) in values) {
			result.k_values[stat] = value;
		}
		return result;
	}

	/// <summary>Creates from serialized data.</summary>
	public static StatValues FromData(StatValuesData data) {
		var result = new StatValues();
		foreach (var (idString, value) in data.Values) {
			result.k_values[new StatProto.ID(idString)] = value;
		}
		return result;
	}

	/// <summary>Creates a copy.</summary>
	public StatValues Clone() {
		var result = new StatValues();
		foreach (var (stat, value) in k_values) {
			result.k_values[stat] = value;
		}
		return result;
	}

	#endregion
}

/// <summary>
/// Serializable stat values data.
/// </summary>
public class StatValuesData {
	/// <summary>Stat ID string to value.</summary>
	public Dictionary<string, float> Values { get; set; } = [];
}