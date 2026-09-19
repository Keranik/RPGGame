using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Events;

/// <summary>
/// Defines all game events using the Proto system.
/// </summary>
public class EventDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterIntroEvents(gameDatabase);
		RegisterRandomEvents(gameDatabase);
		RegisterLocationEvents(gameDatabase);
		RegisterStoryEvents(gameDatabase);
	}

	#region Intro Events

	private void RegisterIntroEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Intro.TheRoadAhead) {
			Title = "The Road Ahead",
			Description =
				"You leave the safety of the village behind. The road stretches before you, leading into lands slowly being consumed by the creeping fog.\n\nYou feel the weight of your supplies and the warmth of the Anchor's blessing fading with each step.",
			FlavorText = "\"Time moves strangely near the fog. Be careful, Wanderer.\" - Elder's parting words",
			IconName = "icon_event_road",
			Type = EventType.Story,
			Rarity = EventRarity.Unique,
			PausesTravel = true,
			CanSkip = false,
			Choices = [
				EventChoiceProto.Simple("continue", "Press onward",
					EventOutcomeProto.Success("You steel yourself and continue down the road.",
						OutcomeEffect.GainMorale(5)))
			],
			OnEndEffects = [OutcomeEffect.SetFlag("intro_complete")]
		});

		db.RegisterProto(new EventProto(Ids.Events.Intro.FirstCombatComplete) {
			Title = "First Blood",
			Description =
				"You stand victorious over your first foes. The wilderness is dangerous, but you've proven you can handle yourself.",
			Type = EventType.Story,
			Rarity = EventRarity.Unique,
			Choices = [
				EventChoiceProto.Simple("continue", "Continue",
					EventOutcomeProto.Success("You feel more confident in your abilities.",
						OutcomeEffect.GainExperience(10),
						OutcomeEffect.GainMorale(10)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Intro.VillageIntroduction) {
			Title = "Welcome to Haven",
			Description =
				"The village elder greets you at the gates. \"Another Wanderer awakens. The Anchor chose you for a reason. Go forth, push back the fog, and return to us.\"\n\nThe villagers watch with a mixture of hope and sorrow. You're not the first to leave. Many never return.",
			FlavorText = "The Anchor pulses gently, a heartbeat of light against the endless gray.",
			IconName = "icon_event_village",
			Type = EventType.Story,
			Rarity = EventRarity.Unique,
			PausesTravel = true,
			CanSkip = false,
			Choices = [
				EventChoiceProto.Simple("accept", "Accept your duty",
					EventOutcomeProto.Success("You bow to the elder and prepare to depart.",
						OutcomeEffect.GainMorale(10),
						OutcomeEffect.SetFlag("village_introduced")))
			]
		});
	}

	#endregion

	#region Random Events

	private void RegisterRandomEvents(GameDb db) {
		RegisterResourceEvents(db);
		RegisterEncounterEvents(db);
		RegisterEnvironmentalEvents(db);
		RegisterDiscoveryEvents(db);
		RegisterCreatureEvents(db);
		RegisterMysticalEvents(db);
		RegisterHazardEvents(db);
		RegisterStatusEvents(db);
	}

	private void RegisterResourceEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Random.AbandonedCamp) {
			Title = "Abandoned Campsite",
			Description = "You come across an abandoned campsite. The fire is cold, but some supplies remain.",
			IconName = "icon_event_camp",
			Type = EventType.Resource,
			Rarity = EventRarity.Common,
			SpawnWeight = 15f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills],
			Choices = [
				EventChoiceProto.Simple("search", "Search the camp",
					EventOutcomeProto.Success("You find some useful supplies.",
						OutcomeEffect.GainFood(2),
						OutcomeEffect.GainItem(Ids.Items.Consumables.Bandage.Value, 1))),
				EventChoiceProto.Simple("rest", "Rest here briefly",
					EventOutcomeProto.Success("You take a short rest.",
						OutcomeEffect.Heal(10),
						OutcomeEffect.AdvanceTime(1))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.HerbPatch) {
			Title = "Medicinal Herbs",
			Description = "You spot a patch of medicinal herbs growing nearby.",
			IconName = "icon_event_herbs",
			Type = EventType.Resource,
			Rarity = EventRarity.Common,
			SpawnWeight = 12f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Wetlands.Swamp],
			Choices = [
				EventChoiceProto.WithRequirement("gather_skilled", "Gather carefully [Survival 12]",
					EventCondition.SkillCheck(Ids.Skills.Gathering.Herbalism, 12),
					EventOutcomeProto.Success("Your skill yields a bountiful harvest.",
						OutcomeEffect.GainItem(Ids.Items.Materials.RareHerbs.Value, 2),
						OutcomeEffect.GainItem(Ids.Items.Materials.Herbs.Value, 3)),
					EventOutcomeProto.Failure("You manage to gather some herbs.",
						OutcomeEffect.GainItem(Ids.Items.Materials.Herbs.Value, 2))),
				EventChoiceProto.Simple("gather_quick", "Gather what you can",
					EventOutcomeProto.Success("You collect some herbs.",
						OutcomeEffect.GainItem(Ids.Items.Materials.Herbs.Value, 1))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.HiddenCache) {
			Title = "Hidden Cache",
			Description = "You notice disturbed earth near an old tree. Someone buried something here.",
			IconName = "icon_event_treasure",
			Type = EventType.Discovery,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 5f,
			Choices = [
				EventChoiceProto.Simple("dig", "Dig it up",
					EventOutcomeProto.Success("You unearth a buried cache!",
						OutcomeEffect.GainGold(25),
						OutcomeEffect.GainItem(Ids.Items.Consumables.HealthPotionSmall.Value, 1))),
				EventChoiceProto.Leave("Leave it alone")
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.AbandonedWagon) {
			Title = "Abandoned Wagon",
			Description = "An overturned merchant's wagon lies beside the road. Its contents are scattered, but some goods remain intact.",
			IconName = "icon_event_wagon",
			Type = EventType.Resource,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 8f,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path],
			Choices = [
				EventChoiceProto.Simple("search_cargo", "Search the cargo",
					EventOutcomeProto.Success("You salvage what you can.",
						OutcomeEffect.GainGold(15),
						OutcomeEffect.GainFood(2))),
				EventChoiceProto.WithRequirement("investigate", "Investigate what happened [Wisdom 12]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 12),
					EventOutcomeProto.Success("You find tracks leading into the forest. Following them reveals a hidden stash.",
						OutcomeEffect.GainGold(30),
						OutcomeEffect.GainItem(Ids.Items.Consumables.ManaPotionSmall.Value, 1),
						OutcomeEffect.GainExperience(15)),
					EventOutcomeProto.Failure("The tracks are too old to follow.",
						OutcomeEffect.GainGold(10))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.FreshwaterSpring) {
			Title = "Freshwater Spring",
			Description = "Crystal-clear water bubbles up from a natural spring. The water looks refreshing and pure.",
			IconName = "icon_event_water",
			Type = EventType.Resource,
			Rarity = EventRarity.Common,
			SpawnWeight = 10f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Hills, Ids.Terrains.Mountains.Mountain],
			Choices = [
				EventChoiceProto.Simple("drink", "Drink deeply",
					EventOutcomeProto.Success("The cool water refreshes you.",
						OutcomeEffect.Heal(15),
						OutcomeEffect.AddFatigue(-15),
						OutcomeEffect.GainMorale(5))),
				EventChoiceProto.Simple("rest", "Rest by the spring",
					EventOutcomeProto.Success("A peaceful moment in the wilderness.",
						OutcomeEffect.AddFatigue(-25),
						OutcomeEffect.GainMorale(10),
						OutcomeEffect.AdvanceTime(1))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.MushroomRing) {
			Title = "Fairy Ring",
			Description = "A perfect circle of strange mushrooms grows in a clearing. They pulse with a faint inner light.",
			IconName = "icon_event_mushroom",
			Type = EventType.Resource,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 6f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest],
			Choices = [
				EventChoiceProto.WithRequirement("harvest_careful", "Carefully harvest [Intelligence 14]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Intelligence, 14),
					EventOutcomeProto.Success("You harvest the magical mushrooms without disturbing the ring.",
						OutcomeEffect.GainItem(Ids.Items.Materials.GlowingMushroom.Value, 3)),
					EventOutcomeProto.Failure("You take some, but feel a strange curse settle over you.",
						OutcomeEffect.GainItem(Ids.Items.Materials.GlowingMushroom.Value, 1),
						OutcomeEffect.ApplyDebuff("fey_touched", 30))),
				EventChoiceProto.Simple("step_inside", "Step inside the ring",
					EventOutcomeProto.Success("Reality shifts. You find yourself with strange memories and stranger gifts.",
						OutcomeEffect.GainExperience(40),
						OutcomeEffect.RestoreMana(20),
						OutcomeEffect.AdvanceTime(3))),
				EventChoiceProto.Leave("Leave it undisturbed")
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.FallenAdventurer) {
			Title = "Fallen Adventurer",
			Description = "A skeleton in rusted armor lies against a tree. Their journey ended here, but their gear remains.",
			IconName = "icon_event_skeleton",
			Type = EventType.Resource,
			Rarity = EventRarity.Common,
			SpawnWeight = 8f,
			Choices = [
				EventChoiceProto.Simple("loot", "Take their belongings",
					EventOutcomeProto.Success("You salvage what you can from their remains.",
						OutcomeEffect.GainGold(20),
						OutcomeEffect.GainItem(Ids.Items.Consumables.Bandage.Value, 2),
						OutcomeEffect.LoseMorale(5))),
				EventChoiceProto.Simple("bury", "Give them a proper burial",
					EventOutcomeProto.Success("You take time to honor the fallen. Perhaps their spirit will rest easier.",
						OutcomeEffect.GainMorale(10),
						OutcomeEffect.GainExperience(10),
						OutcomeEffect.AdvanceTime(1))),
				EventChoiceProto.Simple("read_journal", "Search for a journal",
					EventOutcomeProto.Success("You find a water-damaged journal with useful notes about the area.",
						OutcomeEffect.GainExperience(20),
						OutcomeEffect.UnlockLore("fallen_adventurer_notes"))),
				EventChoiceProto.Leave()
			]
		});
	}

	private void RegisterEncounterEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Random.WoundedTraveler) {
			Title = "Wounded Traveler",
			Description = "You find a traveler collapsed by the roadside, bleeding from wounds.",
			IconName = "icon_event_person",
			Type = EventType.Encounter,
			Rarity = EventRarity.Common,
			SpawnWeight = 10f,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path],
			Choices = [
				EventChoiceProto.WithRequirement("heal", "Tend to their wounds [Medicine 10]",
					EventCondition.SkillCheck(Ids.Skills.Knowledge.Medicine, 10),
					EventOutcomeProto.Success("You successfully treat their wounds. They're grateful.",
						OutcomeEffect.GainExperience(15),
						OutcomeEffect.GainMorale(10),
						OutcomeEffect.GainGold(10)),
					EventOutcomeProto.Failure("You do your best, but your skills are limited.",
						OutcomeEffect.GainExperience(5),
						OutcomeEffect.LoseMorale(5))),
				EventChoiceProto.WithCost("give_supplies", "Give medical supplies",
					ChoiceCost.Item(Ids.Items.Consumables.Bandage.Value, 1),
					EventOutcomeProto.Success("They thank you profusely.",
						OutcomeEffect.GainMorale(15),
						OutcomeEffect.GainExperience(10))),
				EventChoiceProto.Simple("ignore", "Continue on your way",
					EventOutcomeProto.Success("You leave them behind.",
						OutcomeEffect.LoseMorale(5)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.MerchantRoad) {
			Title = "Traveling Merchant",
			Description = "A merchant with a laden cart waves you down. \"Greetings, traveler! Care to trade?\"",
			IconName = "icon_event_merchant",
			Type = EventType.Trade,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 8f,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path],
			Choices = [
				EventChoiceProto.WithCost("buy_potion", "Buy Health Potion (30g)",
					ChoiceCost.Gold(30),
					EventOutcomeProto.Success("A fair trade.",
						OutcomeEffect.GainItem(Ids.Items.Consumables.HealthPotionSmall.Value, 1))),
				EventChoiceProto.WithCost("buy_food", "Buy Rations (10g)",
					ChoiceCost.Gold(10),
					EventOutcomeProto.Success("You stock up on supplies.",
						OutcomeEffect.GainFood(3))),
				EventChoiceProto.Simple("chat", "Just chat",
					EventOutcomeProto.Success("The merchant shares news of the road ahead.",
						OutcomeEffect.GainExperience(5))),
				EventChoiceProto.Leave("Move along")
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.BanditDemand) {
			Title = "Highway Robbery",
			Description = "Bandits block your path! \"Your gold or your life!\" the leader shouts.",
			IconName = "icon_event_danger",
			Type = EventType.Encounter,
			Rarity = EventRarity.Common,
			SpawnWeight = 12f,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path, Ids.Terrains.Forests.Forest],
			MinDistance = 5f,
			PausesTravel = true,
			CanSkip = false,
			Choices = [
				EventChoiceProto.Fight(Ids.Encounters.Random.BanditsSmall.Value),
				EventChoiceProto.WithCost("pay", "Pay them off (25g)",
					ChoiceCost.Gold(25),
					EventOutcomeProto.Success("They take your gold and let you pass.",
						OutcomeEffect.LoseMorale(10))),
				EventChoiceProto.WithRequirement("intimidate", "Intimidate them [Charisma 14]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Charisma, 14),
					EventOutcomeProto.Success("They back down, muttering curses.",
						OutcomeEffect.GainExperience(20),
						OutcomeEffect.GainMorale(5)),
					EventOutcomeProto.Failure("They laugh at your threats.",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.BanditsSmall.Value))),
				EventChoiceProto.WithRequirement("sneak", "Slip away [Dexterity 12]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Dexterity, 12),
					EventOutcomeProto.Success("You escape into the underbrush.",
						OutcomeEffect.GainExperience(10)),
					EventOutcomeProto.Failure("They spot you!",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.BanditsSmall.Value)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.LostChild) {
			Title = "Lost Child",
			Description = "A frightened child cowers behind a bush. \"I... I got separated from my family. The fog came and...\"",
			IconName = "icon_event_child",
			Type = EventType.Encounter,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 5f,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path, Ids.Terrains.Forests.Forest],
			Choices = [
				EventChoiceProto.Simple("escort", "Help them find their family",
					EventOutcomeProto.Success("After searching, you reunite them with grateful parents.",
						OutcomeEffect.GainMorale(20),
						OutcomeEffect.GainGold(15),
						OutcomeEffect.GainExperience(25),
						OutcomeEffect.AdvanceTime(2))),
				EventChoiceProto.Simple("give_food", "Give them food and directions",
					EventOutcomeProto.Success("You point them toward the village and share your rations.",
						OutcomeEffect.LoseFood(1),
						OutcomeEffect.GainMorale(10),
						OutcomeEffect.GainExperience(10))),
				EventChoiceProto.Simple("ignore", "You can't help everyone",
					EventOutcomeProto.Success("You continue on your way.",
						OutcomeEffect.LoseMorale(15)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.RefugeeCamp) {
			Title = "Refugee Camp",
			Description = "A small group of refugees huddles around a dying fire. They fled a village consumed by the fog.",
			IconName = "icon_event_refugees",
			Type = EventType.Encounter,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 6f,
			MinDistance = 10f,
			Choices = [
				EventChoiceProto.WithCost("share_supplies", "Share your supplies",
					ChoiceCost.Food(2),
					EventOutcomeProto.Success("They bless you for your kindness. One shares valuable information.",
						OutcomeEffect.GainMorale(15),
						OutcomeEffect.GainExperience(20),
						OutcomeEffect.UnlockLore("refugee_testimony"))),
				EventChoiceProto.Simple("trade", "Trade with them",
					EventOutcomeProto.Success("They have little, but offer what they can.",
						OutcomeEffect.GainItem(Ids.Items.Consumables.Bandage.Value, 1),
						OutcomeEffect.GainExperience(5))),
				EventChoiceProto.Simple("stay", "Share their fire for the night",
					EventOutcomeProto.Success("You swap stories through the night.",
						OutcomeEffect.AddFatigue(-30),
						OutcomeEffect.GainMorale(5),
						OutcomeEffect.AdvanceTime(6))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.HermitWise) {
			Title = "The Hermit",
			Description = "You stumble upon a small hut hidden in the wilderness. An elderly hermit greets you with knowing eyes. \"Ah, another Wanderer. I've seen many like you.\"",
			IconName = "icon_event_hermit",
			Type = EventType.Encounter,
			Rarity = EventRarity.Rare,
			SpawnWeight = 4f,
			MinDistance = 15f,
			Choices = [
				EventChoiceProto.Simple("ask_wisdom", "Ask for wisdom about the fog",
					EventOutcomeProto.Success("\"The fog is not alive, but it remembers. Every soul it takes becomes part of its pattern.\"",
						OutcomeEffect.GainExperience(30),
						OutcomeEffect.UnlockLore(Ids.Lore.Fog.Nature01.Value))),
				EventChoiceProto.Simple("ask_training", "Ask for training",
					EventOutcomeProto.Success("The hermit shares ancient combat techniques.",
						OutcomeEffect.GainExperience(50),
						OutcomeEffect.AdvanceTime(3))),
				EventChoiceProto.Simple("ask_healing", "Ask for healing",
					EventOutcomeProto.Success("The hermit tends to your wounds with practiced skill.",
						OutcomeEffect.Heal(50),
						OutcomeEffect.AddFatigue(-20))),
				EventChoiceProto.Leave("Thank them and leave")
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.GhostlyApparition) {
			Title = "Ghostly Apparition",
			Description = "A translucent figure materializes before you. It seems to be trying to communicate, gesturing urgently toward a nearby hill.",
			IconName = "icon_event_ghost",
			Type = EventType.Encounter,
			Rarity = EventRarity.Rare,
			SpawnWeight = 4f,
			MinDistance = 10f,
			Choices = [
				EventChoiceProto.Simple("follow", "Follow the ghost",
					EventOutcomeProto.Success("The ghost leads you to a hidden grave. Buried with the body is a valuable weapon.",
						OutcomeEffect.GainClassWeapon(3),
						OutcomeEffect.GainExperience(30))),
				EventChoiceProto.WithRequirement("speak", "Attempt to communicate [Wisdom 15]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 15),
					EventOutcomeProto.Success("\"Beware the one who walks between... the Harbinger comes...\"",
						OutcomeEffect.GainExperience(40),
						OutcomeEffect.UnlockLore("harbinger_warning")),
					EventOutcomeProto.Failure("The ghost fades before you can understand.",
						OutcomeEffect.GainExperience(10))),
				EventChoiceProto.Leave("Back away slowly")
			]
		});
	}

	private void RegisterEnvironmentalEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Random.StormApproaching) {
			Title = "Storm Clouds",
			Description = "Dark clouds gather on the horizon. A storm is coming.",
			IconName = "icon_event_weather",
			Type = EventType.Environmental,
			Rarity = EventRarity.Common,
			SpawnWeight = 8f,
			Choices = [
				EventChoiceProto.Simple("shelter", "Find shelter and wait",
					EventOutcomeProto.Success("You wait out the storm safely.",
						OutcomeEffect.AdvanceTime(3),
						OutcomeEffect.AddFatigue(-10))),
				EventChoiceProto.Simple("push_on", "Push through the storm",
					EventOutcomeProto.Success("You press on despite the weather.",
						OutcomeEffect.AddFatigue(15),
						OutcomeEffect.LoseMorale(5),
						OutcomeEffect.Damage(5)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.FogThickens) {
			Title = "The Fog Thickens",
			Description = "The air grows cold and visibility drops. The fog is closer here. You feel time slowing around you.",
			FlavorText = "The fog does not merely kill. It unmakes.",
			IconName = "icon_event_fog",
			Type = EventType.Environmental,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 5f,
			MinDistance = 15f,
			Choices = [
				EventChoiceProto.Simple("retreat", "Fall back to safer ground",
					EventOutcomeProto.Success("You retreat from the fog's edge.",
						OutcomeEffect.LoseMorale(5))),
				EventChoiceProto.WithRequirement("push_forward", "Press into the fog [Wisdom 15]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 15),
					EventOutcomeProto.Success("You navigate through the thickest part.",
						OutcomeEffect.GainExperience(30),
						OutcomeEffect.UnlockLore(Ids.Lore.Fog.Nature01.Value)),
					EventOutcomeProto.Failure("The fog disorients you.",
						OutcomeEffect.Damage(15),
						OutcomeEffect.AddFatigue(20),
						OutcomeEffect.LoseMorale(15)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.BeautifulSunset) {
			Title = "Moment of Peace",
			Description = "The clouds part to reveal a stunning sunset. For a moment, you forget the fog and the endless struggle.",
			IconName = "icon_event_sunset",
			Type = EventType.Environmental,
			Rarity = EventRarity.Common,
			SpawnWeight = 8f,
			Choices = [
				EventChoiceProto.Simple("appreciate", "Take a moment to appreciate it",
					EventOutcomeProto.Success("Beauty still exists in this world.",
						OutcomeEffect.GainMorale(15),
						OutcomeEffect.AddFatigue(-10))),
				EventChoiceProto.Simple("continue", "Press on while light remains",
					EventOutcomeProto.Success("You make good time before nightfall.",
						OutcomeEffect.GainExperience(5)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.Earthquake) {
			Title = "Tremors",
			Description = "The ground shakes violently! Rocks tumble from nearby cliffs and trees sway dangerously.",
			IconName = "icon_event_earthquake",
			Type = EventType.Environmental,
			Rarity = EventRarity.Rare,
			SpawnWeight = 3f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Hills, Ids.Terrains.Ruins.OpenRuins],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.WithRequirement("dodge", "Dodge the debris [Dexterity 14]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Dexterity, 14),
					EventOutcomeProto.Success("You avoid the falling rocks.",
						OutcomeEffect.GainExperience(20)),
					EventOutcomeProto.Failure("A rock strikes you!",
						OutcomeEffect.Damage(20))),
				EventChoiceProto.Simple("brace", "Brace yourself and wait",
					EventOutcomeProto.Success("The tremors pass. You escaped the worst.",
						OutcomeEffect.Damage(5)))
			],
			OnEndEffects = [OutcomeEffect.SetFlag("earthquake_occurred")]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.NightfallDanger) {
			Title = "Darkness Falls",
			Description = "Night descends quickly. The sounds of nocturnal predators fill the air.",
			IconName = "icon_event_night",
			Type = EventType.Environmental,
			Rarity = EventRarity.Common,
			SpawnWeight = 10f,
			SpawnConditions = [EventCondition.RequiresTimeOfDay(TimeOfDayPeriod.Night)],
			Choices = [
				EventChoiceProto.Simple("camp", "Make camp immediately",
					EventOutcomeProto.Success("You find a defensible spot to spend the night.",
						OutcomeEffect.AdvanceTime(8),
						OutcomeEffect.AddFatigue(-40))),
				EventChoiceProto.WithRequirement("continue", "Press on in the dark [Wisdom 12]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 12),
					EventOutcomeProto.Success("Your eyes adjust and you make steady progress.",
						OutcomeEffect.GainExperience(15)),
					EventOutcomeProto.Failure("You stumble into a pit!",
						OutcomeEffect.Damage(10),
						OutcomeEffect.AddFatigue(15)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.FloraOvergrowth) {
			Title = "Overgrown Path",
			Description = "The path ahead is choked with thorns and vines. The vegetation seems unnaturally thick.",
			IconName = "icon_event_thorns",
			Type = EventType.Environmental,
			Rarity = EventRarity.Common,
			SpawnWeight = 8f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest, Ids.Terrains.Wetlands.Swamp],
			Choices = [
				EventChoiceProto.Simple("cut_through", "Cut through the brush",
					EventOutcomeProto.Success("You hack your way through, though it takes time.",
						OutcomeEffect.AddFatigue(15),
						OutcomeEffect.AdvanceTime(1))),
				EventChoiceProto.WithRequirement("find_path", "Find a way around [Wisdom 11]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 11),
					EventOutcomeProto.Success("You spot an animal trail that bypasses the worst of it.",
						OutcomeEffect.GainExperience(10)),
					EventOutcomeProto.Failure("The detour takes even longer.",
						OutcomeEffect.AddFatigue(20),
						OutcomeEffect.AdvanceTime(2))),
				EventChoiceProto.Simple("turn_back", "Find another route entirely",
					EventOutcomeProto.Success("You take a different path.",
						OutcomeEffect.AdvanceTime(2)))
			]
		});
	}

	private void RegisterDiscoveryEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Random.AncientShrine) {
			Title = "Ancient Shrine",
			Description = "You discover a weathered shrine to a forgotten deity. Faded offerings still adorn its base.",
			IconName = "icon_event_shrine",
			Type = EventType.Discovery,
			Rarity = EventRarity.Rare,
			SpawnWeight = 3f,
			Choices = [
				EventChoiceProto.Simple("pray", "Offer a prayer",
					EventOutcomeProto.Success("A sense of peace washes over you.",
						OutcomeEffect.Heal(20),
						OutcomeEffect.GainMorale(15),
						OutcomeEffect.AddFatigue(-20))),
				EventChoiceProto.WithCost("offering", "Leave an offering (15g)",
					ChoiceCost.Gold(15),
					EventOutcomeProto.Success("The shrine glows briefly. You feel blessed.",
						OutcomeEffect.Heal(30),
						OutcomeEffect.ApplyBuff("blessed", 100),
						OutcomeEffect.GainExperience(25))),
				EventChoiceProto.Simple("study", "Study the inscriptions",
					EventOutcomeProto.Success("You learn something about the old ways.",
						OutcomeEffect.GainExperience(20),
						OutcomeEffect.UnlockLore("ancient_gods_01"))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.MysteriousStranger) {
			Title = "Mysterious Stranger",
			Description = "A hooded figure stands at a crossroads, watching you approach. Their features are hidden in shadow.",
			FlavorText = "Some say the Wanderers who came before still walk these roads.",
			IconName = "icon_event_mystery",
			Type = EventType.Encounter,
			Rarity = EventRarity.Rare,
			SpawnWeight = 2f,
			MinDistance = 20f,
			SpawnConditions = [EventCondition.MinFogClears(1)],
			Choices = [
				EventChoiceProto.Simple("approach", "Approach and speak",
					EventOutcomeProto.Success("\"Time loops, Wanderer. But you... you remember, don't you?\"",
						OutcomeEffect.GainExperience(50),
						OutcomeEffect.UnlockLore(Ids.Lore.Fog.TimeLoop01.Value),
						OutcomeEffect.SetFlag("met_stranger"))),
				EventChoiceProto.Simple("avoid", "Give them a wide berth",
					EventOutcomeProto.Success("They watch you pass, saying nothing."))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.RuinedLibrary) {
			Title = "Ruined Library",
			Description = "Among crumbling ruins, you find the remains of an ancient library. Most books are ruined, but some pages survive.",
			IconName = "icon_event_library",
			Type = EventType.Discovery,
			Rarity = EventRarity.Rare,
			SpawnWeight = 3f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins],
			Choices = [
				EventChoiceProto.WithRequirement("study", "Carefully study the texts [Intelligence 14]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Intelligence, 14),
					EventOutcomeProto.Success("You piece together fragments of ancient knowledge.",
						OutcomeEffect.GainExperience(40),
						OutcomeEffect.UnlockLore("ancient_magic_theory"),
						OutcomeEffect.RestoreMana(30)),
					EventOutcomeProto.Failure("The texts are too damaged to understand fully.",
						OutcomeEffect.GainExperience(15))),
				EventChoiceProto.Simple("search_treasure", "Search for valuables",
					EventOutcomeProto.Success("You find some coins and a scroll.",
						OutcomeEffect.GainGold(30),
						OutcomeEffect.GainClassSpell(1))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.OldBattlefield) {
			Title = "Old Battlefield",
			Description = "You cross an ancient battlefield. Rusted weapons and scattered bones tell of a great conflict long ago.",
			IconName = "icon_event_battlefield",
			Type = EventType.Discovery,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 5f,
			ValidTerrains = [Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills],
			Choices = [
				EventChoiceProto.Simple("scavenge", "Scavenge equipment",
					EventOutcomeProto.Success("Some gear is still usable.",
						OutcomeEffect.GainClassWeapon(1),
						OutcomeEffect.GainGold(10))),
				EventChoiceProto.Simple("honor_dead", "Pay respects to the fallen",
					EventOutcomeProto.Success("You feel their spirits acknowledge your respect.",
						OutcomeEffect.GainMorale(10),
						OutcomeEffect.ApplyBuff("warriors_blessing", 30))),
				EventChoiceProto.WithRequirement("investigate", "Search for valuable relics [Wisdom 13]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 13),
					EventOutcomeProto.Success("You unearth a commander's badge - a valuable antique.",
						OutcomeEffect.GainItem(Ids.Items.Quest.AncientRelic.Value, 1),
						OutcomeEffect.GainExperience(25)),
					EventOutcomeProto.Failure("Nothing of value remains.",
						OutcomeEffect.GainExperience(5))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.StandingStones) {
			Title = "Standing Stones",
			Description = "A circle of ancient standing stones hums with latent energy. Runes carved into the stones glow faintly in the twilight.",
			IconName = "icon_event_stones",
			Type = EventType.Discovery,
			Rarity = EventRarity.Rare,
			SpawnWeight = 3f,
			Choices = [
				EventChoiceProto.WithRequirement("activate", "Attempt to activate the stones [Intelligence 16]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Intelligence, 16),
					EventOutcomeProto.Success("The stones resonate with power. You feel magic surge through you.",
						OutcomeEffect.RestoreMana(50),
						OutcomeEffect.GainExperience(50),
						OutcomeEffect.ApplyBuff("stone_resonance", 100)),
					EventOutcomeProto.Failure("The magic backlashes!",
						OutcomeEffect.Damage(20),
						OutcomeEffect.DrainMana(20))),
				EventChoiceProto.Simple("rest", "Rest within the circle",
					EventOutcomeProto.Success("The stones provide a sense of protection. You rest well.",
						OutcomeEffect.AddFatigue(-30),
						OutcomeEffect.Heal(20))),
				EventChoiceProto.Simple("study", "Study the runes",
					EventOutcomeProto.Success("You gain insight into ancient magical practices.",
						OutcomeEffect.GainExperience(30),
						OutcomeEffect.UnlockLore("stone_circle_magic"))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.HiddenCave) {
			Title = "Hidden Cave",
			Description = "Behind a curtain of vines, you discover the entrance to a small cave. Tracks suggest something lives within.",
			IconName = "icon_event_cave",
			Type = EventType.Discovery,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 6f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Hills, Ids.Terrains.Forests.Forest],
			Choices = [
				EventChoiceProto.Simple("explore", "Explore the cave",
					EventOutcomeProto.Success("You find it's a bear's den - fortunately empty. Some past victim's belongings remain.",
						OutcomeEffect.GainGold(20),
						OutcomeEffect.GainItem(Ids.Items.Consumables.HealthPotionSmall.Value, 1))),
				EventChoiceProto.WithRequirement("sneak", "Sneak inside quietly [Dexterity 13]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Dexterity, 13),
					EventOutcomeProto.Success("You find a hidden treasure cache, undisturbed by the beast.",
						OutcomeEffect.GainGold(50),
						OutcomeEffect.GainItem(Ids.Items.Materials.GemMoonstone.Value, 2)),
					EventOutcomeProto.Failure("You disturb the occupant!",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.Bear.Value))),
				EventChoiceProto.Leave("Too risky")
			]
		});
	}

	private void RegisterCreatureEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Random.WildAnimal) {
			Title = "Wild Animal",
			Description = "A large predator blocks your path, eyeing you hungrily.",
			IconName = "icon_event_beast",
			Type = EventType.Combat,
			Rarity = EventRarity.Common,
			SpawnWeight = 12f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest, Ids.Terrains.Plains.Hills],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.Fight(Ids.Encounters.Random.WildBeast.Value),
				EventChoiceProto.WithRequirement("calm", "Attempt to calm it [Wisdom 12]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 12),
					EventOutcomeProto.Success("The beast backs down and disappears into the brush.",
						OutcomeEffect.GainExperience(15)),
					EventOutcomeProto.Failure("It attacks!",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.WildBeast.Value))),
				EventChoiceProto.WithRequirement("sneak", "Try to sneak past [Dexterity 11]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Dexterity, 11),
					EventOutcomeProto.Success("You slip by unnoticed.",
						OutcomeEffect.GainExperience(10)),
					EventOutcomeProto.Failure("It spots you!",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.WildBeast.Value)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.WolfAmbush) {
			Title = "Wolf Pack",
			Description = "Eyes gleam in the darkness. A pack of wolves has you surrounded.",
			IconName = "icon_event_wolves",
			Type = EventType.Combat,
			Rarity = EventRarity.Common,
			SpawnWeight = 10f,
			SpawnConditions = [EventCondition.RequiresTimeOfDay(TimeOfDayPeriod.Evening)],
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest],
			PausesTravel = true,
			CanSkip = false,
			Choices = [
				EventChoiceProto.Fight(Ids.Encounters.Random.WolfPack.Value),
				EventChoiceProto.WithRequirement("intimidate", "Appear large and threatening [Strength 13]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Strength, 13),
					EventOutcomeProto.Success("The pack decides you're not worth the risk.",
						OutcomeEffect.GainExperience(20)),
					EventOutcomeProto.Failure("They're not impressed.",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.WolfPack.Value)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.BearAttack) {
			Title = "Bear Territory",
			Description = "A massive bear rears up, roaring a warning. You've wandered into its territory.",
			IconName = "icon_event_bear",
			Type = EventType.Combat,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 6f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest, Ids.Terrains.Mountains.Mountain],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.Fight(Ids.Encounters.Random.Bear.Value),
				EventChoiceProto.Simple("retreat", "Back away slowly",
					EventOutcomeProto.Success("You retreat from its territory.",
						OutcomeEffect.LoseMorale(5))),
				EventChoiceProto.WithCost("offer_food", "Offer food as tribute",
					ChoiceCost.Food(2),
					EventOutcomeProto.Success("The bear takes the food and lets you pass.",
						OutcomeEffect.GainExperience(10)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.SpiderAmbush) {
			Title = "Web Trap",
			Description = "You stumble into massive webs strung between the trees. Clicking sounds come from above...",
			IconName = "icon_event_spider",
			Type = EventType.Combat,
			Rarity = EventRarity.Common,
			SpawnWeight = 8f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest, Ids.Terrains.Wetlands.Swamp],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.Fight(Ids.Encounters.Random.GiantSpiders.Value),
				EventChoiceProto.WithRequirement("burn", "Burn the webs [have torch]",
					EventCondition.HasItem(Ids.Items.Consumables.Torch.Value),
					EventOutcomeProto.Success("Fire drives the spiders back!",
						OutcomeEffect.GainExperience(15)),
					EventOutcomeProto.Failure("",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.GiantSpiders.Value)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.AngryBoar) {
			Title = "Charging Boar",
			Description = "A wild boar snorts and paws the ground. It's about to charge!",
			IconName = "icon_event_boar",
			Type = EventType.Combat,
			Rarity = EventRarity.Common,
			SpawnWeight = 8f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass, Ids.Terrains.Plains.Hills],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.Fight(Ids.Encounters.Random.WildBoar.Value),
				EventChoiceProto.WithRequirement("dodge", "Dodge aside [Dexterity 13]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Dexterity, 13),
					EventOutcomeProto.Success("The boar charges past harmlessly.",
						OutcomeEffect.GainExperience(10)),
					EventOutcomeProto.Failure("It clips you!",
						OutcomeEffect.Damage(10)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.AngryBees) {
			Title = "Disturbed Hive",
			Description = "You've accidentally disturbed a beehive! An angry swarm emerges.",
			IconName = "icon_event_bees",
			Type = EventType.Environmental,
			Rarity = EventRarity.Common,
			SpawnWeight = 6f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.Simple("run", "Run for it!",
					EventOutcomeProto.Success("You escape with only a few stings.",
						OutcomeEffect.Damage(5),
						OutcomeEffect.AddFatigue(10))),
				EventChoiceProto.WithRequirement("smoke", "Use smoke to calm them [Wisdom 12]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 12),
					EventOutcomeProto.Success("The smoke calms the bees. You even harvest some honey!",
						OutcomeEffect.GainFood(1),
						OutcomeEffect.GainItem(Ids.Items.Materials.Honey.Value, 2)),
					EventOutcomeProto.Failure("Too slow! The bees attack.",
						OutcomeEffect.Damage(15))),
				EventChoiceProto.Simple("dive_water", "Dive into nearby water",
					EventOutcomeProto.Success("You escape, but your supplies get wet.",
						OutcomeEffect.LoseFood(1)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.SwampThing) {
			Title = "Something in the Water",
			Description = "The murky water churns. Something large lurks beneath the surface.",
			IconName = "icon_event_swamp",
			Type = EventType.Combat,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 6f,
			ValidTerrains = [Ids.Terrains.Wetlands.Swamp],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.Simple("avoid", "Find another way around",
					EventOutcomeProto.Success("You take a longer but safer path.",
						OutcomeEffect.AdvanceTime(2),
						OutcomeEffect.AddFatigue(10))),
				EventChoiceProto.Simple("wade", "Wade through quickly",
					EventOutcomeProto.Success("You disturb the creature!",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.SwampCreature.Value))),
				EventChoiceProto.WithRequirement("distract", "Throw something to distract it [Intelligence 11]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Intelligence, 11),
					EventOutcomeProto.Success("The creature investigates the splash, and you slip past.",
						OutcomeEffect.GainExperience(15)),
					EventOutcomeProto.Failure("It ignores the distraction!",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.SwampCreature.Value)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.WyvernAttack) {
			Title = "Wyvern Sighting",
			Description = "A screech from above! A wyvern circles overhead, deciding if you're worth hunting.",
			IconName = "icon_event_wyvern",
			Type = EventType.Combat,
			Rarity = EventRarity.Rare,
			SpawnWeight = 2f,
			MinDistance = 25f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Hills],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.Fight(Ids.Encounters.Random.Wyvern.Value),
				EventChoiceProto.Simple("hide", "Hide and wait",
					EventOutcomeProto.Success("The wyvern loses interest and flies away.",
						OutcomeEffect.AdvanceTime(1))),
				EventChoiceProto.WithRequirement("scare", "Appear too dangerous to attack [Intimidate 15]",
					EventCondition.SkillCheck(Ids.Skills.Social.Intimidation, 15),
					EventOutcomeProto.Success("You make yourself look formidable. The wyvern seeks easier prey.",
						OutcomeEffect.GainExperience(30),
						OutcomeEffect.GainMorale(10)),
					EventOutcomeProto.Failure("It dives!",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.Wyvern.Value)))
			]
		});
	}

	private void RegisterMysticalEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Random.ManaWellBlessing) {
			Title = "Mana Well",
			Description = "You discover a natural wellspring of magical energy. The air itself crackles with power.",
			IconName = "icon_event_mana",
			Type = EventType.Discovery,
			Rarity = EventRarity.Rare,
			SpawnWeight = 3f,
			Choices = [
				EventChoiceProto.Simple("drink", "Draw upon its power",
					EventOutcomeProto.Success("Magical energy floods through you.",
						OutcomeEffect.RestoreMana(50),
						OutcomeEffect.ApplyBuff("mana_surge", 50))),
				EventChoiceProto.Simple("meditate", "Meditate at the well",
					EventOutcomeProto.Success("You gain deeper understanding of magic.",
						OutcomeEffect.GainExperience(40),
						OutcomeEffect.RestoreMana(30))),
				EventChoiceProto.WithRequirement("bottle", "Bottle some for later [empty flask]",
					EventCondition.HasItem(Ids.Items.Materials.DeerHide.Value), // TODO: Add bottle item and use that instead
					EventOutcomeProto.Success("You capture some of the essence.",
						OutcomeEffect.GainItem(Ids.Items.Consumables.ManaPotionMedium.Value, 1)),
					EventOutcomeProto.Failure("",
						OutcomeEffect.RestoreMana(20))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.FogWhispers) {
			Title = "Whispers in the Fog",
			Description = "The fog seems alive here. Voices whisper from the mist, speaking of things that were and things that might be.",
			FlavorText = "\"Remember us... we were like you, once...\"",
			IconName = "icon_event_whispers",
			Type = EventType.Environmental,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 5f,
			MinDistance = 20f,
			Choices = [
				EventChoiceProto.Simple("listen", "Listen to the whispers",
					EventOutcomeProto.Success("Fragments of knowledge from lost souls...",
						OutcomeEffect.GainExperience(30),
						OutcomeEffect.UnlockLore(Ids.Lore.Fog.Voices01.Value),
						OutcomeEffect.LoseMorale(10))),
				EventChoiceProto.WithRequirement("communicate", "Try to communicate [Wisdom 16]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 16),
					EventOutcomeProto.Success("A clear voice emerges: \"Seek the Anchor's origin. That is where the fog began.\"",
						OutcomeEffect.GainExperience(50),
						OutcomeEffect.UnlockLore(Ids.Lore.Fog.Origin01.Value)),
					EventOutcomeProto.Failure("The voices descend into cacophony.",
						OutcomeEffect.LoseMorale(15),
						OutcomeEffect.Damage(10))),
				EventChoiceProto.Simple("flee", "Block them out and flee",
					EventOutcomeProto.Success("You escape the fog's reach.",
						OutcomeEffect.AddFatigue(15)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.TimeAnomaly) {
			Title = "Time Anomaly",
			Description = "Reality warps around you. You see yourself walking past - but from a different time.",
			FlavorText = "The fog doesn't just consume space. It consumes time.",
			IconName = "icon_event_time",
			Type = EventType.Environmental,
			Rarity = EventRarity.Rare,
			SpawnWeight = 2f,
			MinDistance = 30f,
			Choices = [
				EventChoiceProto.Simple("observe", "Observe your past/future self",
					EventOutcomeProto.Success("You glimpse events yet to come - or that have already happened.",
						OutcomeEffect.GainExperience(40),
						OutcomeEffect.UnlockLore(Ids.Lore.Fog.TimeLoop01.Value))),
				EventChoiceProto.WithRequirement("interact", "Try to interact [Intelligence 17]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Intelligence, 17),
					EventOutcomeProto.Success("For a moment, you exist in two times at once. Knowledge floods your mind.",
						OutcomeEffect.GainExperience(75),
						OutcomeEffect.GainItem(Ids.Items.Quest.AncientRelic.Value, 1),
						OutcomeEffect.UnlockLore(Ids.Lore.Fog.TimeLoop02.Value)),
					EventOutcomeProto.Failure("Reality snaps back painfully.",
						OutcomeEffect.Damage(25),
						OutcomeEffect.LoseMorale(10))),
				EventChoiceProto.Simple("flee", "Close your eyes and run",
					EventOutcomeProto.Success("You escape the anomaly.",
						OutcomeEffect.AddFatigue(20)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.TimeCrackSealed) {
			Title = "Fractured Time",
			Description = "A crack in reality hangs in the air, edges flickering between moments. Through it, you glimpse the world before the fog.",
			FlavorText = "The world was beautiful once. It can be again.",
			IconName = "icon_event_crack",
			Type = EventType.Discovery,
			Rarity = EventRarity.Rare,
			SpawnWeight = 2f,
			MinDistance = 35f,
			Choices = [
				EventChoiceProto.WithRequirement("seal", "Attempt to seal the crack [Wisdom 18]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 18),
					EventOutcomeProto.Success("You channel your will and the crack slowly closes. The fog weakens slightly in this area.",
						OutcomeEffect.GainExperience(100),
						OutcomeEffect.GainMorale(20),
						OutcomeEffect.SetFlag("time_crack_sealed")),
					EventOutcomeProto.Failure("The crack resists. Energy lashes out.",
						OutcomeEffect.Damage(30),
						OutcomeEffect.DrainMana(30))),
				EventChoiceProto.Simple("reach_through", "Reach through the crack",
					EventOutcomeProto.Success("You pull something from the past - an artifact of the old world.",
						OutcomeEffect.GainItem(Ids.Items.Quest.FogClue.Value, 1),
						OutcomeEffect.Damage(15))),
				EventChoiceProto.Simple("study", "Study the phenomenon",
					EventOutcomeProto.Success("You learn much about the nature of the fog's effect on time.",
						OutcomeEffect.GainExperience(50),
						OutcomeEffect.UnlockLore(Ids.Lore.Fog.TimeLoop03.Value))),
				EventChoiceProto.Leave("Back away carefully")
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.DragonGhost) {
			Title = "Spectral Dragon",
			Description = "The ghostly form of an ancient dragon materializes before you. It regards you with ancient, knowing eyes.",
			FlavorText = "Even in death, the great ones remember.",
			IconName = "icon_event_dragon",
			Type = EventType.Encounter,
			Rarity = EventRarity.Rare,
			SpawnWeight = 1f,
			MinDistance = 40f,
			Choices = [
				EventChoiceProto.Simple("bow", "Bow in respect",
					EventOutcomeProto.Success("The dragon acknowledges your respect. It breathes ghostly flame that doesn't burn - instead, it empowers you.",
						OutcomeEffect.ApplyBuff("dragon_blessing", 150),
						OutcomeEffect.GainExperience(60))),
				EventChoiceProto.WithRequirement("speak", "Attempt to communicate [Charisma 16]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Charisma, 16),
					EventOutcomeProto.Success("\"The fog fears fire, young one. Remember this.\" The dragon shares ancient wisdom.",
						OutcomeEffect.GainExperience(80),
						OutcomeEffect.UnlockLore("dragon_wisdom"),
						OutcomeEffect.GainItem(Ids.Items.Materials.DragonScale.Value, 1)),
					EventOutcomeProto.Failure("The dragon finds you unworthy of conversation and fades away.",
						OutcomeEffect.LoseMorale(10))),
				EventChoiceProto.Simple("attack", "Attack the apparition",
					EventOutcomeProto.Success("Your weapon passes harmlessly through. The dragon roars in anger!",
						OutcomeEffect.Damage(40),
						OutcomeEffect.ApplyDebuff("dragon_curse", 50)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.StarfallWish) {
			Title = "Falling Star",
			Description = "A streak of light crosses the night sky and crashes into the ground nearby. The impact site glows with ethereal light.",
			IconName = "icon_event_star",
			Type = EventType.Discovery,
			Rarity = EventRarity.Rare,
			SpawnWeight = 2f,
			SpawnConditions = [EventCondition.RequiresTimeOfDay(TimeOfDayPeriod.Night)],
			Choices = [
				EventChoiceProto.Simple("investigate", "Investigate the crash site",
					EventOutcomeProto.Success("You find a fragment of star-metal, still warm to the touch.",
						OutcomeEffect.GainItem(Ids.Items.Materials.StarMetal.Value, 1),
						OutcomeEffect.GainExperience(30))),
				EventChoiceProto.Simple("make_wish", "Make a wish upon the fallen star",
					EventOutcomeProto.Success("A warm feeling washes over you. Perhaps wishes do come true.",
						OutcomeEffect.Heal(30),
						OutcomeEffect.GainMorale(20),
						OutcomeEffect.AddFatigue(-20))),
				EventChoiceProto.WithRequirement("extract", "Extract the star's essence [Intelligence 15]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Intelligence, 15),
					EventOutcomeProto.Success("You carefully harvest the magical essence.",
						OutcomeEffect.RestoreMana(40),
						OutcomeEffect.GainItem(Ids.Items.Materials.StarMetal.Value, 2)), // TODO: this should be star essence not star metal
					EventOutcomeProto.Failure("The essence dissipates before you can capture it.",
						OutcomeEffect.RestoreMana(10))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.ShrinePrayer) {
			Title = "Wayside Shrine",
			Description = "A small shrine to the old gods stands by the roadside. Offerings and prayers might still hold power here.",
			IconName = "icon_event_shrine_small",
			Type = EventType.Discovery,
			Rarity = EventRarity.Common,
			SpawnWeight = 7f,
			ValidTerrains = [Ids.Terrains.Roads.Road, Ids.Terrains.Roads.Path],
			Choices = [
				EventChoiceProto.Simple("pray_protection", "Pray for protection",
					EventOutcomeProto.Success("You feel a divine ward settle over you.",
						OutcomeEffect.ApplyBuff("divine_protection", 40))),
				EventChoiceProto.Simple("pray_strength", "Pray for strength",
					EventOutcomeProto.Success("Power flows through your muscles.",
						OutcomeEffect.ApplyBuff("divine_strength", 40))),
				EventChoiceProto.Simple("pray_guidance", "Pray for guidance",
					EventOutcomeProto.Success("Your path forward becomes clearer.",
						OutcomeEffect.ApplyBuff("divine_guidance", 40),
						OutcomeEffect.GainExperience(10))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.DemonWhisper) {
			Title = "Dark Whispers",
			Description = "A voice speaks directly into your mind, offering power in exchange for... something.",
			FlavorText = "\"I can give you the strength to push back the fog. All I ask is a small favor...\"",
			IconName = "icon_event_demon",
			Type = EventType.Encounter,
			Rarity = EventRarity.Rare,
			SpawnWeight = 2f,
			MinDistance = 25f,
			Choices = [
				EventChoiceProto.Simple("accept", "Accept the bargain",
					EventOutcomeProto.Success("Power floods through you, but at what cost?",
						OutcomeEffect.ApplyBuff("demonic_power", 100),
						OutcomeEffect.RestoreMana(50),
						OutcomeEffect.LoseMorale(25),
						OutcomeEffect.SetFlag("made_dark_bargain"))),
				EventChoiceProto.Simple("refuse", "Refuse absolutely",
					EventOutcomeProto.Success("The voice laughs and fades. \"We'll speak again, Wanderer...\"",
						OutcomeEffect.GainMorale(10))),
				EventChoiceProto.WithRequirement("banish", "Attempt to banish it [Wisdom 16]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 16),
					EventOutcomeProto.Success("You drive the presence from your mind with sheer will.",
						OutcomeEffect.GainExperience(40),
						OutcomeEffect.GainMorale(15)),
					EventOutcomeProto.Failure("It resists your attempt, laughing.",
						OutcomeEffect.Damage(15),
						OutcomeEffect.LoseMorale(10)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.TreasureCurse) {
			Title = "Cursed Treasure",
			Description = "You find an ornate chest half-buried in the ground. Gold coins glint from within, but something feels wrong.",
			IconName = "icon_event_curse",
			Type = EventType.Discovery,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 4f,
			Choices = [
				EventChoiceProto.Simple("take_all", "Take everything",
					EventOutcomeProto.Success("Gold! But a curse settles over you...",
						OutcomeEffect.GainGold(75),
						OutcomeEffect.ApplyDebuff("treasure_curse", 60))),
				EventChoiceProto.WithRequirement("dispel", "Attempt to dispel the curse first [Intelligence 14]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Intelligence, 14),
					EventOutcomeProto.Success("You cleanse the treasure before taking it.",
						OutcomeEffect.GainGold(75),
						OutcomeEffect.GainExperience(25)),
					EventOutcomeProto.Failure("The curse activates as you reach for the gold!",
						OutcomeEffect.Damage(20),
						OutcomeEffect.ApplyDebuff("treasure_curse", 30))),
				EventChoiceProto.Simple("take_some", "Take only a handful",
					EventOutcomeProto.Success("You take what you need and leave the rest.",
						OutcomeEffect.GainGold(25))),
				EventChoiceProto.Leave("Leave it alone")
			]
		});
	}

	private void RegisterHazardEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Random.MithrilGuardian) {
			Title = "Ore Guardian",
			Description = "As you approach a vein of precious ore, an elemental guardian rises from the stone to protect it.",
			IconName = "icon_event_elemental",
			Type = EventType.Combat,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 4f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Underground.Cave],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.Fight(Ids.Encounters.Random.OreGuardian.Value),
				EventChoiceProto.Simple("retreat", "Leave the ore alone",
					EventOutcomeProto.Success("You back away from the valuable deposit.",
						OutcomeEffect.LoseMorale(5))),
				EventChoiceProto.WithRequirement("calm", "Attempt to calm the elemental [Wisdom 15]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 15),
					EventOutcomeProto.Success("The guardian allows you to take a small portion.",
						OutcomeEffect.GainItem(Ids.Items.Materials.MithrilOre.Value, 1),
						OutcomeEffect.GainExperience(25)),
					EventOutcomeProto.Failure("It attacks!",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.OreGuardian.Value)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.PoisonPrick) {
			Title = "Poisonous Thorns",
			Description = "You brush against a plant with wickedly sharp thorns. Too late, you notice the purple sheen on them.",
			IconName = "icon_event_poison",
			Type = EventType.Environmental,
			Rarity = EventRarity.Common,
			SpawnWeight = 6f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest, Ids.Terrains.Wetlands.Swamp],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.WithRequirement("treat", "Treat the wound immediately [Medicine 11]",
					EventCondition.SkillCheck(Ids.Skills.Knowledge.Medicine, 11),
					EventOutcomeProto.Success("You extract the poison before it spreads.",
						OutcomeEffect.Damage(5)),
					EventOutcomeProto.Failure("You can't stop the poison.",
						OutcomeEffect.Damage(10),
						OutcomeEffect.ApplyDebuff("poisoned", 20))),
				EventChoiceProto.WithCost("use_antidote", "Use an antidote",
					ChoiceCost.Item(Ids.Items.Consumables.Antidote.Value, 1),
					EventOutcomeProto.Success("The antidote neutralizes the poison.",
						OutcomeEffect.Damage(3))),
				EventChoiceProto.Simple("endure", "Push through the pain",
					EventOutcomeProto.Success("You grit your teeth and continue.",
						OutcomeEffect.Damage(15),
						OutcomeEffect.ApplyDebuff("poisoned", 30)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.MandrakeScream) {
			Title = "Mandrake Root",
			Description = "You spot a valuable mandrake root growing nearby. But pulling it up could be... loud.",
			IconName = "icon_event_mandrake",
			Type = EventType.Resource,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 4f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest],
			Choices = [
				EventChoiceProto.WithRequirement("careful_pull", "Pull it carefully [Dexterity 14]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Dexterity, 14),
					EventOutcomeProto.Success("You extract the mandrake without triggering its scream.",
						OutcomeEffect.GainItem(Ids.Items.Materials.MandrakeRoot.Value, 1),
						OutcomeEffect.GainExperience(20)),
					EventOutcomeProto.Failure("The mandrake shrieks!",
						OutcomeEffect.Damage(20),
						OutcomeEffect.ApplyDebuff("dazed", 10))),
				EventChoiceProto.Simple("quick_pull", "Yank it and run",
					EventOutcomeProto.Success("You grab the mandrake and flee the noise.",
						OutcomeEffect.GainItem(Ids.Items.Materials.MandrakeRoot.Value, 1),
						OutcomeEffect.Damage(10),
						OutcomeEffect.AddFatigue(15))),
				EventChoiceProto.Leave("Not worth the risk")
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.ElderTreeSpirit) {
			Title = "Ancient Tree Spirit",
			Description = "An enormous, ancient tree radiates a presence. A face seems to form in its bark, watching you.",
			IconName = "icon_event_treespirit",
			Type = EventType.Encounter,
			Rarity = EventRarity.Rare,
			SpawnWeight = 3f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Forests.DeepForest],
			Choices = [
				EventChoiceProto.Simple("offer_water", "Offer water to its roots",
					EventOutcomeProto.Success("The tree spirit is pleased. A branch lowers, offering a gift.",
						OutcomeEffect.GainItem(Ids.Items.Materials.ElderWood.Value, 2),
						OutcomeEffect.GainMorale(10))),
				EventChoiceProto.Simple("ask_wisdom", "Ask for forest wisdom",
					EventOutcomeProto.Success("The spirit shares knowledge of the deep woods.",
						OutcomeEffect.GainExperience(35),
						OutcomeEffect.ApplyBuff("forest_knowledge", 50))),
				EventChoiceProto.Simple("rest_shade", "Rest in its shade",
					EventOutcomeProto.Success("The tree protects you while you rest.",
						OutcomeEffect.AddFatigue(-40),
						OutcomeEffect.Heal(25),
						OutcomeEffect.AdvanceTime(2))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.CorruptionSpread) {
			Title = "Spreading Corruption",
			Description = "The ground here is blackened and dead. The fog's corruption has taken root, spreading like a disease.",
			IconName = "icon_event_corruption",
			Type = EventType.Environmental,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 5f,
			MinDistance = 20f,
			ValidTerrains = [Ids.Terrains.Supernatural.Corrupted],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.WithRequirement("cleanse", "Attempt to cleanse the corruption [Wisdom 16]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 16),
					EventOutcomeProto.Success("You channel purifying energy, pushing back the corruption.",
						OutcomeEffect.GainExperience(50),
						OutcomeEffect.GainMorale(15),
						OutcomeEffect.SetFlag("corruption_cleansed")),
					EventOutcomeProto.Failure("The corruption resists and lashes out.",
						OutcomeEffect.Damage(25),
						OutcomeEffect.ApplyDebuff("corruption_touched", 30))),
				EventChoiceProto.Simple("push_through", "Push through quickly",
					EventOutcomeProto.Success("You hurry through the corrupted area.",
						OutcomeEffect.Damage(10),
						OutcomeEffect.LoseMorale(10))),
				EventChoiceProto.Simple("go_around", "Find a way around",
					EventOutcomeProto.Success("You take a longer route to avoid the corruption.",
						OutcomeEffect.AdvanceTime(3),
						OutcomeEffect.AddFatigue(15)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.CartOwner) {
			Title = "Returning Merchant",
			Description = "A merchant approaches - it's the owner of that abandoned wagon! They offer a reward for any goods you might have recovered.",
			IconName = "icon_event_merchant",
			Type = EventType.Encounter,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 3f,
			SpawnConditions = [EventCondition.HasFlag("found_wagon_goods")],
			Choices = [
				EventChoiceProto.Simple("return_goods", "Return what you found",
					EventOutcomeProto.Success("The merchant is overjoyed and rewards you handsomely.",
						OutcomeEffect.GainGold(50),
						OutcomeEffect.GainMorale(20),
						OutcomeEffect.GainExperience(25))),
				EventChoiceProto.Simple("keep_goods", "Pretend ignorance",
					EventOutcomeProto.Success("You say nothing about the wagon.",
						OutcomeEffect.LoseMorale(10))),
				EventChoiceProto.Simple("negotiate", "Negotiate a finder's fee",
					EventOutcomeProto.Success("You agree on fair compensation.",
						OutcomeEffect.GainGold(30),
						OutcomeEffect.GainExperience(15)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.AncientTrap) {
			Title = "Ancient Trap",
			Description = "You hear a click underfoot. You've triggered an ancient trap mechanism!",
			IconName = "icon_event_trap",
			Type = EventType.Environmental,
			Rarity = EventRarity.Common,
			SpawnWeight = 6f,
			ValidTerrains = [Ids.Terrains.Ruins.OpenRuins, Ids.Terrains.Underground.Cave],
			PausesTravel = true,
			CanSkip = false,
			Choices = [
				EventChoiceProto.WithRequirement("disarm", "Try to disarm it [Dexterity 14]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Dexterity, 14),
					EventOutcomeProto.Success("You carefully disable the mechanism.",
						OutcomeEffect.GainExperience(20)),
					EventOutcomeProto.Failure("The trap springs!",
						OutcomeEffect.Damage(25))),
				EventChoiceProto.WithRequirement("dodge", "Leap away [Dexterity 12]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Dexterity, 12),
					EventOutcomeProto.Success("You dive clear as the trap activates.",
						OutcomeEffect.GainExperience(10)),
					EventOutcomeProto.Failure("Not fast enough!",
						OutcomeEffect.Damage(20))),
				EventChoiceProto.Simple("brace", "Brace for impact",
					EventOutcomeProto.Success("You minimize the damage.",
						OutcomeEffect.Damage(15)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.GeyserEruption) {
			Title = "Geyser Field",
			Description = "Steam vents and geysers dot this area. The ground rumbles ominously.",
			IconName = "icon_event_geyser",
			Type = EventType.Environmental,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 4f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Volcanic.VolcanicPlain],
			Choices = [
				EventChoiceProto.WithRequirement("navigate", "Navigate carefully [Wisdom 13]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 13),
					EventOutcomeProto.Success("You time your movements between eruptions.",
						OutcomeEffect.GainExperience(20)),
					EventOutcomeProto.Failure("A geyser erupts beneath you!",
						OutcomeEffect.Damage(20))),
				EventChoiceProto.Simple("run_through", "Run through quickly",
					EventOutcomeProto.Success("You sprint through, getting scalded a bit.",
						OutcomeEffect.Damage(10),
						OutcomeEffect.AddFatigue(15))),
				EventChoiceProto.Simple("go_around", "Find another route",
					EventOutcomeProto.Success("You take the long way around.",
						OutcomeEffect.AdvanceTime(2)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.HotSpringRest) {
			Title = "Natural Hot Spring",
			Description = "Steam rises from a natural hot spring. The mineral-rich water looks incredibly inviting.",
			IconName = "icon_event_hotspring",
			Type = EventType.Rest,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 5f,
			ValidTerrains = [Ids.Terrains.Mountains.Mountain, Ids.Terrains.Plains.Hills],
			Choices = [
				EventChoiceProto.Simple("bathe", "Take a relaxing bath",
					EventOutcomeProto.Success("The hot water soothes your aches and pains.",
						OutcomeEffect.AddFatigue(-40),
						OutcomeEffect.Heal(25),
						OutcomeEffect.GainMorale(15),
						OutcomeEffect.AdvanceTime(2))),
				EventChoiceProto.Simple("quick_soak", "Quick soak and continue",
					EventOutcomeProto.Success("Even a brief rest helps.",
						OutcomeEffect.AddFatigue(-20),
						OutcomeEffect.Heal(10),
						OutcomeEffect.AdvanceTime(1))),
				EventChoiceProto.Leave("No time to rest")
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.GraveDisturbance) {
			Title = "Disturbed Grave",
			Description = "An old grave has been dug up recently. Claw marks score the earth around it.",
			IconName = "icon_event_grave",
			Type = EventType.Discovery,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 5f,
			Choices = [
				EventChoiceProto.Simple("investigate", "Investigate the grave",
					EventOutcomeProto.Success("You find the corpse rising!",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.Undead.Value))),
				EventChoiceProto.Simple("rebury", "Rebury and consecrate the ground",
					EventOutcomeProto.Success("You lay the spirit to rest properly.",
						OutcomeEffect.GainMorale(10),
						OutcomeEffect.GainExperience(15),
						OutcomeEffect.AdvanceTime(1))),
				EventChoiceProto.WithRequirement("detect", "Sense for undead presence [Wisdom 12]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Wisdom, 12),
					EventOutcomeProto.Success("You sense the undead hiding nearby and prepare for combat.",
						OutcomeEffect.ApplyBuff("combat_ready", 20),
						OutcomeEffect.StartCombat(Ids.Encounters.Random.Undead.Value)),
					EventOutcomeProto.Failure("You sense nothing - until it's too late.",
						OutcomeEffect.StartCombat(Ids.Encounters.Random.Undead.Value))),
				EventChoiceProto.Leave()
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.HiddenSpider) {
			Title = "Trapdoor Spider",
			Description = "The ground gives way beneath you! A massive trapdoor spider lunges from its hidden lair.",
			IconName = "icon_event_spider",
			Type = EventType.Combat,
			Rarity = EventRarity.Uncommon,
			SpawnWeight = 5f,
			ValidTerrains = [Ids.Terrains.Forests.Forest, Ids.Terrains.Plains.Grass],
			PausesTravel = true,
			CanSkip = false,
			Choices = [
				EventChoiceProto.Fight(Ids.Encounters.Random.TrapdoorSpider.Value),
				EventChoiceProto.WithRequirement("escape", "Scramble out [Dexterity 15]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Dexterity, 15),
					EventOutcomeProto.Success("You escape the pit before the spider can strike.",
						OutcomeEffect.GainExperience(20)),
					EventOutcomeProto.Failure("It catches you!",
						OutcomeEffect.Damage(10),
						OutcomeEffect.StartCombat(Ids.Encounters.Random.TrapdoorSpider.Value)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.AntSwarm) {
			Title = "Fire Ant Mound",
			Description = "You've disturbed a massive fire ant mound! Thousands of angry ants pour out.",
			IconName = "icon_event_ants",
			Type = EventType.Environmental,
			Rarity = EventRarity.Common,
			SpawnWeight = 5f,
			ValidTerrains = [Ids.Terrains.Plains.Grass, Ids.Terrains.Forests.Forest],
			PausesTravel = true,
			Choices = [
				EventChoiceProto.Simple("run", "Run!",
					EventOutcomeProto.Success("You escape but not before getting bitten.",
						OutcomeEffect.Damage(8),
						OutcomeEffect.AddFatigue(10))),
				EventChoiceProto.WithRequirement("fire", "Use fire to drive them back [have torch]",
					EventCondition.HasItem(Ids.Items.Consumables.Torch.Value),
					EventOutcomeProto.Success("Fire scatters the ants. You also find their queen - valuable alchemical ingredient.",
						OutcomeEffect.GainItem(Ids.Items.Materials.AntEggs.Value, 1)),
					EventOutcomeProto.Failure("",
						OutcomeEffect.Damage(8),
						OutcomeEffect.AddFatigue(10))),
				EventChoiceProto.Simple("stand_still", "Stand absolutely still",
					EventOutcomeProto.Success("The ants lose interest after a tense few minutes.",
						OutcomeEffect.AdvanceTime(1)))
			]
		});
	}

	private void RegisterStatusEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Random.Exhaustion) {
			Title = "Overwhelming Exhaustion",
			Description = "Your body finally gives out. You can barely keep your eyes open, let alone continue traveling.",
			IconName = "icon_event_exhaustion",
			Type = EventType.Environmental,
			Rarity = EventRarity.Common,
			PausesTravel = true,
			CanSkip = false,
			Choices = [
				EventChoiceProto.Simple("rest", "Rest where you are",
					EventOutcomeProto.Success("You collapse and sleep fitfully.",
						OutcomeEffect.AdvanceTime(4),
						OutcomeEffect.AddFatigue(-30),
						OutcomeEffect.LoseMorale(10))),
				EventChoiceProto.WithRequirement("push", "Push through [Constitution 14]",
					EventCondition.SkillCheck(Ids.Stats.Attributes.Constitution, 14),
					EventOutcomeProto.Success("Through sheer will, you continue.",
						OutcomeEffect.LoseMorale(5)),
					EventOutcomeProto.Failure("Your body refuses. You collapse.",
						OutcomeEffect.AdvanceTime(6),
						OutcomeEffect.AddFatigue(-40),
						OutcomeEffect.Damage(10)))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Random.Collapse) {
			Title = "Complete Collapse",
			Description = "Everything goes dark. Your body has reached its absolute limit.",
			FlavorText = "Even the strongest Wanderer must rest eventually.",
			IconName = "icon_event_collapse",
			Type = EventType.Environmental,
			Rarity = EventRarity.Common,
			PausesTravel = true,
			CanSkip = false,
			Choices = [
				EventChoiceProto.Simple("accept", "Accept unconsciousness",
					EventOutcomeProto.Success("You awaken hours later, still exhausted but alive.",
						OutcomeEffect.AdvanceTime(8),
						OutcomeEffect.AddFatigue(-50),
						OutcomeEffect.LoseMorale(20),
						OutcomeEffect.Damage(15)))
			]
		});
	}

	#endregion

	#region Location Events

	private void RegisterLocationEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Story.VillageReturn) {
			Title = "Return to the Village",
			Description = "The familiar walls of the village come into view. The Anchor's warmth welcomes you home.",
			Type = EventType.Story,
			PausesTravel = true,
			Choices = [
				EventChoiceProto.Simple("enter", "Enter the village",
					EventOutcomeProto.Success("You pass through the gates."))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Story.CampsiteFound) {
			Title = "Safe Haven",
			Description = "You've found a sheltered campsite. The area seems defensible.",
			Type = EventType.Rest,
			Choices = [
				EventChoiceProto.Simple("camp", "Set up camp",
					EventOutcomeProto.Success("You establish a comfortable camp.",
						OutcomeEffect.SetFlag("at_campsite"))),
				EventChoiceProto.Simple("continue", "Continue traveling",
					EventOutcomeProto.Success("You press on."))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Story.FogClueDiscovered) {
			Title = "A Clue in the Mist",
			Description = "Among the remains of a fallen Wanderer, you find a journal. The final entries speak of a pattern in the fog - a path to its source.",
			FlavorText = "\"It's not random. The fog follows ley lines. If you map them...\"",
			Type = EventType.Story,
			Rarity = EventRarity.Unique,
			MinDistance = 30f,
			Choices = [
				EventChoiceProto.Simple("study", "Study the journal carefully",
					EventOutcomeProto.Success("The patterns become clear. You now understand how to navigate deeper into the fog.",
						OutcomeEffect.GainExperience(50),
						OutcomeEffect.UnlockLore(Ids.Lore.Fog.LeyLines01.Value),
						OutcomeEffect.SetFlag("fog_clue_found")))
			]
		});
	}

	#endregion

	#region Story Events

	private void RegisterStoryEvents(GameDb db) {
		db.RegisterProto(new EventProto(Ids.Events.Story.BanditCampCleared) {
			Title = "Victory at the Bandit Camp",
			Description = "With the bandits defeated, you search their camp. Among the stolen goods, you find letters hinting at a larger organization.",
			Type = EventType.Story,
			Rarity = EventRarity.Unique,
			Choices = [
				EventChoiceProto.Simple("continue", "Take the evidence",
					EventOutcomeProto.Success("This could be useful information.",
						OutcomeEffect.GainItem(Ids.Items.Quest.BanditLetters.Value, 1),
						OutcomeEffect.UnlockLore("bandit_network_01"),
						OutcomeEffect.SetFlag("bandit_camp_cleared")))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Story.LichDefeated) {
			Title = "The Lich Falls",
			Description = "The ancient lich crumbles to dust, its phylactery shattered. As it fades, it whispers: \"The fog... it comes from beyond time itself...\"",
			Type = EventType.Story,
			Rarity = EventRarity.Unique,
			Choices = [
				EventChoiceProto.Simple("continue", "Ponder its words",
					EventOutcomeProto.Success("Another piece of the puzzle falls into place.",
						OutcomeEffect.UnlockLore(Ids.Lore.Fog.Origin01.Value),
						OutcomeEffect.SetFlag("lich_defeated")))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Story.FogHeraldDefeated) {
			Title = "The Herald Falls",
			Description = "The Herald dissolves into mist. For a moment, the fog recedes. You've done it—you've pushed back the endless night.\n\nBut as you stand victorious, you feel a strange sensation. Time shifts. The world blurs.\n\nYou awaken in the village. It's the first day again. But this time... you remember everything.",
			FlavorText = "The cycle continues. But now you understand.",
			Type = EventType.Story,
			Rarity = EventRarity.Unique,
			Choices = [
				EventChoiceProto.Simple("accept", "Accept your fate",
					EventOutcomeProto.Success("The fog clears... for now. You've bought the world more time.",
						OutcomeEffect.SetFlag("fog_cleared")))
			]
		});

		db.RegisterProto(new EventProto(Ids.Events.Story.CaughtInTime) {
			Title = "Caught in Time",
			Description = "The fog consumes you. As your vision fades, you feel yourself being pulled backward through time.\n\nYou awaken in the village. It's the first day. Again.",
			FlavorText = "Death is not the end. It is merely... a reset.",
			Type = EventType.Story,
			Rarity = EventRarity.Unique,
			Choices = [
				EventChoiceProto.Simple("continue", "Begin again",
					EventOutcomeProto.Success("The cycle continues.",
						OutcomeEffect.SetFlag("died_to_fog")))
			]
		});
	}

	#endregion
}