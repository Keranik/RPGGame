using RPGGame.Core.Prototypes.Activities;

namespace RPGGame.Core.Activities;
public interface IActivity {
	ActivityId Id { get; }

	ActivityProto Prototype { get; }

	bool RepeatActivity { get; set; }

	void OnActivityStart();
	void OnActivityEnd();
	void OnActivityStep();
	void OnAwardReward();

	public double TotalSteps { get; }
	public double TotalStepsCompleted { get; }
}
