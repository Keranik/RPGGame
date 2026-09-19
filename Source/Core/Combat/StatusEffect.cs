namespace RPGGame.Core.Combat;

public partial class StatusEffect {
	public StatusCondition Condition { get; set; }
	public Duration RemainingDuration { get; set; }
	public float Intensity { get; set; } = 1f;
	public string? SourceId { get; set; }

	/// <summary>Alias for SourceId for compatibility.</summary>
	public string? Source {
		get => SourceId;
		set => SourceId = value;
	}
}
