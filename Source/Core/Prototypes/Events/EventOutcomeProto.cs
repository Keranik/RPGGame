using RPGGame.Core.Events;

namespace RPGGame.Core.Prototypes.Events;

public partial class EventOutcomeProto {
	public string Message { get; init; } = "";
	public string? FlavorText { get; init; }
	public List<OutcomeEffect> Effects { get; init; } = [];
	public static EventOutcomeProto Success(string message, params OutcomeEffect[] effects) {
		return new EventOutcomeProto {
			Message = message,
			Effects = [.. effects]
		};
	}
	public static EventOutcomeProto Failure(string message, params OutcomeEffect[] effects) {
		return new EventOutcomeProto {
			Message = message,
			Effects = [.. effects]
		};
	}
}
