using RPGGame.Core.Prototypes.Activities;
using RPGGame.Core.Prototypes.Item;

namespace RPGGame.Core.Activities.Gathering;
internal class Gathering : ActivityBase , IActivity {
	public Dictionary<ItemProto, int> RewardsList { get; }

	new public ActivityProto Prototype;
	private readonly ActivityId k_activityId;
	new public ActivityId Id => k_activityId;

	public Gathering(ActivityId id, ActivityProto activityType) : base(id, activityType) {
		k_activityId = id;
		Prototype = activityType;
		RewardsList = [];
		RepeatActivity = false;
		UnityEngine.Debug.Log($"Instantiating Gathering instance with id {Id.Value} from prototype {activityType.Id}");
	}

	public override void OnActivityStep() {
		base.OnActivityStep();
		//UnityEngine.Debug.Log($"Gathering step {TotalStepsCompleted} of {TotalSteps}");
	}
}
