using RPGGame.Core;
using RPGGame.Core.Activities;
using RPGGame.Core.Prototypes.Activities;

// Removed [Dependency] - not used yet, will be manually wired up later
public class ActivityFactoryBase : IFactory<ActivityProto, ActivityBase>
{
	private readonly ActivityId.Factory k_activityIdFactory;
	private Func<Type, object[], object>? k_createInstance;

	public ActivityFactoryBase(ActivityId.Factory idFactory) {
		k_activityIdFactory = idFactory;
		UnityEngine.Debug.Log("Activity Factory Initialized");
	}

	public void SetInstanceCreator(Func<Type, object[], object> creator) {
		k_createInstance = creator ?? throw new ArgumentNullException(nameof(creator));
	}

	public ActivityBase Create(ActivityProto proto) {
		if (k_createInstance == null) {
			throw new InvalidOperationException(
				$"ActivityFactoryBase.Create called before SetInstanceCreator. " +
				$"Ensure GameManager calls SetInstanceCreator after building the resolver.");
		}

		ActivityId activityId = k_activityIdFactory.GetNextId();
		object activity = k_createInstance(proto.ActivityType, new object[] { activityId, proto });
		return (ActivityBase)activity;
	}
}
