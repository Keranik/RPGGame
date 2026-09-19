using RPGGame.Core.Prototypes.Events;

namespace RPGGame.Core.Events;

public partial class ChoiceAvailability {
	public EventChoiceProto Choice { get; init; } = null!;
	public bool IsAvailable { get; init; }
	public bool ConditionMet { get; init; }
	public bool CanAfford { get; init; }
	public string? LockedReason { get; init; }
}
