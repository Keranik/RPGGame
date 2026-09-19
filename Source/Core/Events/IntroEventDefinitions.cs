using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Events;

/// <summary>
/// Defines the intro sequence events for each run.
/// Tutorial for new players, run-defining choices for veterans.
/// 
/// Sequence:
/// 1. Gate Guard Boon (Offensive choice - weapon or XP/damage bonus)
/// 2. Defense Boon Choice (Defensive choice - armor or health/healing bonus)
/// 3. First Combat Encounter (Tutorial wolves)
/// </summary>
public class IntroEventDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterGateEvents(gameDatabase);
		RegisterDefenseBoonEvent(gameDatabase);
		RegisterFirstCombatEvents(gameDatabase);
	}

	private void RegisterGateEvents(GameDb db) {
		// Event #1: Gate Guard's Gift - Offensive boon choice
		db.RegisterProto(new EventProto(Ids.Events.Intro.GateGuardBoon) {
			Title = "The Gate Guard's Gift",
			Description = "\"Wait!\" A voice calls from behind. The gate guard hurries toward you, something clutched in his hands.\n\n\"Take this. It was my son's. Before the fog took him. May it serve you better.\"",
			FlavorText = "Every Wanderer who returns weakens the fog.",
			IconName = "icon_event_guard",
			Type = EventType.Story,
			Rarity = EventRarity.Unique,
			PausesTravel = true,
			CanSkip = false,
			MinDistance = 1f,
			MaxDistance = 4f,
			Choices = [
				// Option 1: Tier 2 weapon for your class
				EventChoiceProto.Simple("take_weapon", "Accept the weapon",
					EventOutcomeProto.Success("The weapon feels right in your hand.",
						OutcomeEffect.GainClassWeapon(2),
						OutcomeEffect.GainMorale(10))),

				// Option 2: For veterans - permanent run bonus instead
				EventChoiceProto.WithRequirement("take_blessing", "Accept his blessing instead",
					EventCondition.HasFlag("completed_first_run"),
					EventOutcomeProto.Success("He places his hand on your shoulder. You feel strengthened.",
						OutcomeEffect.AddRunModifier(RunModifierType.ExperienceBonus, 10),
						OutcomeEffect.AddRunModifier(RunModifierType.DamageBonus, 5),
						OutcomeEffect.GainMorale(15)))
			],
			OnEndEffects = [OutcomeEffect.SetFlag("received_gate_gift")]
		});
	}

	private void RegisterDefenseBoonEvent(GameDb db) {
		// Event #2: Defense Boon Choice - Before entering the fog
		db.RegisterProto(new EventProto(Ids.Events.Intro.DefenseBoonChoice) {
			Title = "Preparing for the Journey",
			Description = "Before you lies the edge of safety. Beyond, the fog awaits. An old supply cache sits by the road - left by previous Wanderers who didn't return.",
			FlavorText = "Take what you need. Leave what you can.",
			IconName = "icon_event_loot",
			Type = EventType.Story,
			Rarity = EventRarity.Unique,
			PausesTravel = true,
			CanSkip = false,
			MinDistance = 4f,
			MaxDistance = 6f,
			SpawnConditions = [EventCondition.HasFlag("received_gate_gift")],
			Choices = [
				// Option 1: Tier 2 armor
				EventChoiceProto.Simple("take_armor", "Take the armor",
					EventOutcomeProto.Success("The armor fits well enough. It will serve you in the trials ahead.",
						OutcomeEffect.GainClassArmor(2))),

				// Option 2: Defensive run bonus (for veterans)
				EventChoiceProto.WithRequirement("take_toughness", "Meditate on the path ahead",
					EventCondition.HasFlag("completed_first_run"),
					EventOutcomeProto.Success("You steel yourself for what's to come.",
						OutcomeEffect.AddRunModifier(RunModifierType.MaxHealthBonus, 15),
						OutcomeEffect.AddRunModifier(RunModifierType.HealingBonus, 10)))
			],
			OnEndEffects = [OutcomeEffect.SetFlag("defense_boon_received")]
		});
	}

	private void RegisterFirstCombatEvents(GameDb db) {
		// Event #3: First Combat - Guaranteed tutorial encounter
		db.RegisterProto(new EventProto(Ids.Events.Intro.FirstCombatEncounter) {
			Title = "Movement in the Brush",
			Description = "You hear rustling in the undergrowth ahead. Shapes move between the trees. The fog has awakened something - and it has noticed you.",
			FlavorText = "The fog's creatures grow bolder each day.",
			IconName = "icon_event_danger",
			Type = EventType.Combat,
			Rarity = EventRarity.Unique,
			PausesTravel = true,
			CanSkip = false,
			MinDistance = 8f,
			MaxDistance = 12f,
			SpawnConditions = [EventCondition.HasFlag("defense_boon_received")],
			Choices = [
				EventChoiceProto.Fight(Ids.Encounters.Story.IntroWolves.Value),
				
				EventChoiceProto.WithRequirement("sneak_past", "Attempt to sneak past [Stealth 12]",
					EventCondition.SkillCheck(Ids.Skills.Stealth.Sneaking, 12),
					EventOutcomeProto.Success("You slip past unnoticed.",
						OutcomeEffect.GainExperience(15)),
					EventOutcomeProto.Failure("They spot you!",
						OutcomeEffect.StartCombat(Ids.Encounters.Story.IntroWolves.Value)))
			],
			OnEndEffects = [OutcomeEffect.SetFlag("intro_complete")]
		});
	}
}