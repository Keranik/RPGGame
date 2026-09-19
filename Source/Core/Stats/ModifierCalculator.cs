namespace RPGGame.Core.Stats;

/// <summary>
/// Calculates final values from a collection of ValueModifiers.
/// Follows order: Base → Flat Add/Sub → Additive % → Multiplicative %
/// </summary>
public static class ModifierCalculator {
	/// <summary>
	/// Calculates the final value after applying all modifiers.
	/// Order: Base + Flat → × (1 + Sum of Increased%) → × Each More multiplier
	/// </summary>
	public static float Calculate(float baseValue, IEnumerable<ValueModifier> modifiers) {
		float flatTotal = 0f;
		float additivePercentTotal = 0f;
		float multiplicativeProduct = 1f;

		foreach (var mod in modifiers) {
			switch (mod.Operation) {
				case ModifierOperation.FlatAdd:
					flatTotal += mod.Value;
					break;

				case ModifierOperation.FlatSubtract:
					flatTotal -= mod.Value;
					break;

				case ModifierOperation.PercentIncrease:
					additivePercentTotal += mod.Value;
					break;

				case ModifierOperation.PercentReduce:
					additivePercentTotal -= mod.Value;
					break;

				case ModifierOperation.PercentMore:
					multiplicativeProduct *= (1f + mod.Value / 100f);
					break;

				case ModifierOperation.PercentLess:
					multiplicativeProduct *= (1f - mod.Value / 100f);
					break;
			}
		}

		// Apply in order: Base + Flat → Additive % → Multiplicative %
		float afterFlat = baseValue + flatTotal;
		float afterAdditive = afterFlat * (1f + additivePercentTotal / 100f);
		float final = afterAdditive * multiplicativeProduct;

		return final;
	}

	/// <summary>
	/// Calculates the final value, returning breakdown info.
	/// </summary>
	public static ModifierBreakdown CalculateWithBreakdown(float baseValue, IEnumerable<ValueModifier> modifiers) {
		var breakdown = new ModifierBreakdown { BaseValue = baseValue };

		foreach (var mod in modifiers) {
			switch (mod.Operation) {
				case ModifierOperation.FlatAdd:
					breakdown.FlatTotal += mod.Value;
					break;
				case ModifierOperation.FlatSubtract:
					breakdown.FlatTotal -= mod.Value;
					break;
				case ModifierOperation.PercentIncrease:
					breakdown.AdditivePercentTotal += mod.Value;
					break;
				case ModifierOperation.PercentReduce:
					breakdown.AdditivePercentTotal -= mod.Value;
					break;
				case ModifierOperation.PercentMore:
					breakdown.MultiplicativeFactors.Add(1f + mod.Value / 100f);
					break;
				case ModifierOperation.PercentLess:
					breakdown.MultiplicativeFactors.Add(1f - mod.Value / 100f);
					break;
			}
		}

		breakdown.Calculate();
		return breakdown;
	}

	/// <summary>
	/// Calculates the final value for integer stats (rounds at the end).
	/// </summary>
	public static int CalculateInt(int baseValue, IEnumerable<ValueModifier> modifiers) {
		return (int)Math.Round(Calculate(baseValue, modifiers));
	}
}

/// <summary>
/// Breakdown of how modifiers were applied to reach a final value.
/// </summary>
public class ModifierBreakdown {
	public float BaseValue { get; init; }
	public float FlatTotal { get; set; }
	public float AdditivePercentTotal { get; set; }
	public List<float> MultiplicativeFactors { get; } = [];

	public float AfterFlat { get; private set; }
	public float AfterAdditive { get; private set; }
	public float FinalValue { get; private set; }

	public float MultiplicativeProduct => MultiplicativeFactors.Count > 0
		? MultiplicativeFactors.Aggregate(1f, (a, b) => a * b)
		: 1f;

	public void Calculate() {
		AfterFlat = BaseValue + FlatTotal;
		AfterAdditive = AfterFlat * (1f + AdditivePercentTotal / 100f);
		FinalValue = AfterAdditive * MultiplicativeProduct;
	}

	public override string ToString() {
		var sb = new System.Text.StringBuilder();
		sb.AppendLine($"Base: {BaseValue:F1}");

		if (Math.Abs(FlatTotal) > 0.001f) {
			sb.AppendLine($"  + Flat: {FlatTotal:+0.#;-0.#}");
			sb.AppendLine($"  = {AfterFlat:F1}");
		}

		if (Math.Abs(AdditivePercentTotal) > 0.001f) {
			sb.AppendLine($"  × (1 + {AdditivePercentTotal:F0}%)");
			sb.AppendLine($"  = {AfterAdditive:F1}");
		}

		if (MultiplicativeFactors.Count > 0) {
			foreach (var factor in MultiplicativeFactors) {
				sb.AppendLine($"  × {factor:F2}");
			}
			sb.AppendLine($"  = {FinalValue:F1}");
		}

		return sb.ToString();
	}
}