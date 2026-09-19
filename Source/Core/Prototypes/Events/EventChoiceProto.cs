using RPGGame.Core.Events;

namespace RPGGame.Core.Prototypes.Events;

public partial class EventChoiceProto {
	public string ChoiceId { get; init; } = "";
	public string Text { get; init; } = "";
	public string? Description { get; init; }
	public string? IconName { get; init; }
	/// <summary>Condition to show/enable this choice.</summary>
	public EventCondition? Condition { get; init; }
	/// <summary>Cost to select this choice.</summary>
	public ChoiceCost? Cost { get; init; }
	/// <summary>Outcome on success.</summary>
	public EventOutcomeProto? SuccessOutcome { get; init; }
	/// <summary>Outcome on failure (for skill checks).</summary>
	public EventOutcomeProto? FailureOutcome { get; init; }
	/// <summary>Whether to show this choice when requirements not met.</summary>
	public bool ShowWhenLocked { get; init; } = true;
	public static EventChoiceProto Simple(string id, string text, EventOutcomeProto outcome) {
		return new EventChoiceProto {
			ChoiceId = id,
			Text = text,
			SuccessOutcome = outcome
		};
	}
	public static EventChoiceProto WithRequirement(
		string id,
		string text,
		EventCondition condition,
		EventOutcomeProto successOutcome,
		EventOutcomeProto? failureOutcome = null
	) {
		return new EventChoiceProto {
			ChoiceId = id,
			Text = text,
			Condition = condition,
			SuccessOutcome = successOutcome,
			FailureOutcome = failureOutcome
		};
	}
	public static EventChoiceProto WithCost(string id, string text, ChoiceCost cost, EventOutcomeProto outcome) {
		return new EventChoiceProto {
			ChoiceId = id,
			Text = text,
			Cost = cost,
			SuccessOutcome = outcome
		};
	}
	public static EventChoiceProto Fight(string encounterId) {
		return new EventChoiceProto {
			ChoiceId = "fight",
			Text = "Fight!",
			IconName = "icon_combat",
			SuccessOutcome = new EventOutcomeProto {
				Message = "You engage in combat!",
				Effects = [OutcomeEffect.StartCombat(encounterId)]
			}
		};
	}
	public static EventChoiceProto Leave(string text = "Leave") {
		return new EventChoiceProto {
			ChoiceId = "leave",
			Text = text,
			SuccessOutcome = new EventOutcomeProto {
				Message = "You continue on your way."
			}
		};
	}
}
