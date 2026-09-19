using RPGGame.Core.Prototypes.Activities;
using RPGGame.Core.Simulation;
using UnityEngine;
#nullable disable

namespace RPGGame.Core.Activities;

public class ActivityManager
{
	public List<IActivity> AllActivities { get; private set; }
	public IActivity CurrentActivity { get; set; }

	private readonly ActivityFactoryBase k_activityFactory;

	public ActivityManager(ActivityFactoryBase activityFactory, GameLoop gameLoop)
	{
		AllActivities = [];
		k_activityFactory = activityFactory;
		Debug.Log("Activity Manager Initialized");
		gameLoop.OnUpdate += OnSimUpdate;
	}

	public void TryAddActivity<T>(ActivityProto proto) where T : ActivityBase, IActivity {
		UnityEngine.Debug.Log($"Attempting to add activity with proto ID: {proto.Id.Value}");
		if (AllActivities.Any(activity => activity.Prototype.Id == proto.Id)) {
			Debug.LogWarning($"Activity {proto.Id} already exists");
			return;
		}

		createAndAddActivity<T>(proto);		
	}

	private void createAndAddActivity<T>(ActivityProto proto) where T : ActivityBase, IActivity
	{
		ActivityBase activity = k_activityFactory.Create(proto);
		addActivity(activity);
	}

	private void addActivity(IActivity activity)
	{
		UnityEngine.Debug.Log($"Adding activity {activity.Id} to manager");
		AllActivities.Add(activity);
		CurrentActivity = activity;
	}

	public void RemoveActivity(IActivity activity)
	{
		AllActivities.Remove(activity);
	}

	public IActivity GetActivityById(ActivityId id)
	{
		return AllActivities.FirstOrDefault(a => a.Id == id);
	}

	public IActivity GetActivityByProto(ActivityProto proto) {
		return AllActivities.FirstOrDefault(a => a.Prototype == proto);
	}

	public void OnSimUpdate()
	{
		CurrentActivity?.OnActivityStep();
	}
}
