using RPGGame.Core.Activities.Gathering;

namespace RPGGame.Core.Prototypes.Activities;
public class GatheringProto : ActivityProto, IActivityProto
{
	public override Type ActivityType => typeof(Gathering);

	public GatheringProto(ID id, Loc text, double energyPerStep, double totalSteps) : base(id, text, energyPerStep, totalSteps)	{
		EnergyPerStep = energyPerStep;
		TotalSteps = totalSteps;
		UnityEngine.Debug.Log($"Creating new gathering proto with ID {id.Value}");
	}	
}
