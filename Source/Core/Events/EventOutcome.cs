namespace RPGGame.Core.Events;

/// <summary>
/// A possible outcome with weight and effects.
/// </summary>
public class EventOutcome {
	public string Id { get; init; } = "";
	public float Weight { get; init; } = 1f;
	public string ResultText { get; init; } = "";
	public string? FlavorText { get; init; }
	public List<OutcomeEffect> Effects { get; init; } = [];
	public bool IsSuccess { get; init; } = true;
	public bool IsCritical { get; init; }
	public List<EventCondition> Conditions { get; init; } = [];

	#region Factory Methods

	public static EventOutcome Success(string text, params OutcomeEffect[] effects) => new() {
		Id = Guid.NewGuid().ToString(),
		ResultText = text,
		IsSuccess = true,
		Effects = [.. effects]
	};

	public static EventOutcome Failure(string text, params OutcomeEffect[] effects) => new() {
		Id = Guid.NewGuid().ToString(),
		ResultText = text,
		IsSuccess = false,
		Effects = [.. effects]
	};

	public static EventOutcome CriticalSuccess(string text, params OutcomeEffect[] effects) => new() {
		Id = Guid.NewGuid().ToString(),
		ResultText = text,
		IsSuccess = true,
		IsCritical = true,
		Effects = [.. effects]
	};

	public static EventOutcome CriticalFailure(string text, params OutcomeEffect[] effects) => new() {
		Id = Guid.NewGuid().ToString(),
		ResultText = text,
		IsSuccess = false,
		IsCritical = true,
		Effects = [.. effects]
	};

	#endregion

	/// <summary>
	/// Gets all display text for effects.
	/// </summary>
	public IEnumerable<string> GetEffectDisplayTexts() {
		return Effects
			.Where(e => e.ShowInUI)
			.Select(e => e.GetDisplayText())
			.Where(t => !string.IsNullOrEmpty(t));
	}
}