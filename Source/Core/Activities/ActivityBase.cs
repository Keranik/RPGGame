using RPGGame.Core.Prototypes.Activities;
using Unity.Properties;

namespace RPGGame.Core.Activities;
public abstract class ActivityBase : IActivity {
	private bool k_repeatActivity;

	public bool RepeatActivity {
		get => k_repeatActivity;
		set => k_repeatActivity = value;
	}

	private bool k_isActive;
	public bool IsActive {
		get => k_isActive;
		set => k_isActive = value;
	}

	public ActivityProto Prototype { get; }
	public string Name;
	public string Description;	
	public double EnergyPerStep { get; private set; }

	[CreateProperty]
	public double TotalSteps { get; private set; }

	[CreateProperty]
	public double TotalStepsCompleted { get; private set; }

	[CreateProperty]
	public string FormattedProgress => $"{TotalStepsCompleted}/{TotalSteps}";

	private readonly ActivityId k_activityId;
	public ActivityId Id => k_activityId;

	protected ActivityBase(ActivityId id, ActivityProto activityType) {
		k_activityId = id;
		RepeatActivity = false;
		IsActive = false;
		Prototype = activityType;		
		Name = activityType.DisplayText.Name;
		Description = activityType.DisplayText.Description;
		EnergyPerStep = activityType.EnergyPerStep;
		TotalSteps = activityType.TotalSteps;
	}

	public void SetEnergyPerStep(double energy) {
		EnergyPerStep = energy;
	}

	public void SetTotalSteps(double steps) {
		TotalSteps = steps;
	}

	public virtual void OnActivityStart() {
		TotalStepsCompleted = 0;
		IsActive = true;
	}

	public virtual void OnActivityEnd() {
		OnAwardReward();
		TotalStepsCompleted = 0;
		IsActive = false;

		if (RepeatActivity)
        {
            OnActivityStart();
		}
    }

	public virtual void OnActivityStep() {
		if (!IsActive) {
			return;
		}

		TotalStepsCompleted++;

		if (TotalStepsCompleted > TotalSteps) {
			OnActivityEnd();
		}
	}

	public virtual void OnAwardReward() {
		var numOfReward = DiceRoller.RollDisadvantage(HitDice.D4);
		var rewardType = DiceRoller.RollDisadvantage(HitDice.D4);
		UnityEngine.Debug.Log($"Rolled {numOfReward} of rewardType {rewardType}");
		// Placeholder for future implementation
	}
}
