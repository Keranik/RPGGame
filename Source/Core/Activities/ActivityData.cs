using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Activities;

namespace RPGGame.Core.Activities;

public class ActivityData : ICoreData {

	public void GameData(GameDb gameDatabase) {
		//ActivityProto fishing = new ActivityProto(
		//	id: Ids.Activities.Fishing,
		//	text: Proto.CreateText(
		//		name: "Fishing",
		//		description: "Try to catch fish."),
		//	energyPerStep: 0.1,
		//	totalSteps: 20);
		//gameDatabase.RegisterProto(fishing);

		GatheringProto gathering = new GatheringProto(
			id: Ids.Activities.Gathering,
			text: Proto.CreateText(
				name: "Gathering",
				description: "Gather resources."),
			energyPerStep: 0.1,
			totalSteps: 20);
		UnityEngine.Debug.Log($"Created base game prototype {gathering} with id {gathering.Id}");
		gameDatabase.RegisterProto(gathering);

		//ActivityProto cooking = new ActivityProto(
		//	id: Ids.Activities.Cooking,
		//	text: Proto.CreateText(
		//		name: "Cooking",
		//		description: "Cook food."),
		//	energyPerStep: 0.1,
		//	totalSteps: 20);
		//gameDatabase.RegisterProto(cooking);

		//ActivityProto mining = new ActivityProto(
		//	id: Ids.Activities.Mining,
		//	text: Proto.CreateText(
		//		name: "Mining",
		//		description: "Mine ore."),
		//	energyPerStep: 0.1,
		//	totalSteps: 20);
		//gameDatabase.RegisterProto(mining);
	}
}
