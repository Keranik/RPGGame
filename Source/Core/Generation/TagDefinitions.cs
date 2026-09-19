using RPGGame.Core.Stats;

namespace RPGGame.Core.Generation;

/// <summary>
/// Defines all tags using the Proto system.
/// Tags can be applied to any entity (Skills, Spells, Items, Enemies, Characters, etc.)
/// </summary>
public class TagDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterSizeTags(gameDatabase);
		RegisterTimeOfDayTags(gameDatabase);
		RegisterElementTags(gameDatabase);
		RegisterCombatTags(gameDatabase);
		RegisterBehaviorTags(gameDatabase);
		RegisterCreatureTags(gameDatabase);
		RegisterSpecialTags(gameDatabase);
		RegisterFogTags(gameDatabase);
		RegisterRegionTags(gameDatabase);
		RegisterWeaponTags(gameDatabase);
		RegisterArmorTags(gameDatabase);
		RegisterSchoolTags(gameDatabase);
		RegisterMasteryTags(gameDatabase);
		RegisterAttributeTags(gameDatabase);
		RegisterMetaTags(gameDatabase);
		RegisterPhysicalTags(gameDatabase);
		RegisterStealthTags(gameDatabase);
		RegisterAwarenessTags(gameDatabase);
		RegisterSurvivalTags(gameDatabase);
		RegisterCraftingTags(gameDatabase);
		RegisterSocialTags(gameDatabase);
		RegisterKnowledgeTags(gameDatabase);
		RegisterClassTags(gameDatabase);
		RegisterResistanceTags(gameDatabase);
		RegisterImmunityTags(gameDatabase);
		RegisterVulnerabilityTags(gameDatabase);
		RegisterConditionTags(gameDatabase);
		RegisterItemTags(gameDatabase);
		RegisterSourceTags(gameDatabase);
		RegisterTriggerTags(gameDatabase);
	}

	#region Size Tags

	private void RegisterSizeTags(GameDb db) {
		db.RegisterProto(new TagProto(Ids.Tags.Size.Tiny, "Tiny", "A diminutive creature, hard to hit but fragile.") {
			IconName = "icon_tag_tiny",
			CreationPointCost = 15,
			ConflictingTags = [Ids.Tags.Size.Small, Ids.Tags.Size.Medium, Ids.Tags.Size.Large, Ids.Tags.Size.Huge, Ids.Tags.Size.Gargantuan, Ids.Tags.Size.Colossal],
			SelectionWeight = 0.3f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Strength, 4.FlatSubtract() },
				{ Ids.Stats.Attributes.Dexterity, 4.Flat() },
				{ Ids.Stats.Attributes.Constitution, 2.FlatSubtract() },
				{ Ids.Stats.Combat.ArmorClass, 2.Flat() },
				{ Ids.Stats.Meta.Size, 1.FlatSubtract() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Size.Small, "Small", "A smaller than average creature.") {
			IconName = "icon_tag_small",
			CreationPointCost = 10,
			ConflictingTags = [Ids.Tags.Size.Tiny, Ids.Tags.Size.Medium, Ids.Tags.Size.Large, Ids.Tags.Size.Huge, Ids.Tags.Size.Gargantuan, Ids.Tags.Size.Colossal],
			SelectionWeight = 0.8f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Strength, 1.FlatSubtract() },
				{ Ids.Stats.Attributes.Dexterity, 2.Flat() },
				{ Ids.Stats.Attributes.Constitution, 1.FlatSubtract() },
				{ Ids.Stats.Combat.ArmorClass, 1.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Size.Medium, "Medium", "An average-sized creature.") {
			IconName = "icon_tag_medium",
			ConflictingTags = [Ids.Tags.Size.Tiny, Ids.Tags.Size.Small, Ids.Tags.Size.Large, Ids.Tags.Size.Huge, Ids.Tags.Size.Gargantuan, Ids.Tags.Size.Colossal]
		});

		db.RegisterProto(new TagProto(Ids.Tags.Size.Large, "Large", "A larger than average creature with great strength.") {
			IconName = "icon_tag_large",
			CreationPointCost = 25,
			MinLevel = 3,
			ConflictingTags = [Ids.Tags.Size.Tiny, Ids.Tags.Size.Small, Ids.Tags.Size.Medium, Ids.Tags.Size.Huge, Ids.Tags.Size.Gargantuan, Ids.Tags.Size.Colossal],
			SelectionWeight = 0.5f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Strength, 2.Flat() },
				{ Ids.Stats.Attributes.Dexterity, 1.FlatSubtract() },
				{ Ids.Stats.Attributes.Constitution, 2.Flat() },
				{ Ids.Stats.Resource.MaxHealth, 10.Flat() },
				{ Ids.Stats.Meta.Size, 1.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Size.Huge, "Huge", "A massive creature of tremendous power.") {
			IconName = "icon_tag_huge",
			CreationPointCost = 50,
			MinLevel = 8,
			ConflictingTags = [Ids.Tags.Size.Tiny, Ids.Tags.Size.Small, Ids.Tags.Size.Medium, Ids.Tags.Size.Large, Ids.Tags.Size.Gargantuan, Ids.Tags.Size.Colossal],
			SelectionWeight = 0.2f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Strength, 4.Flat() },
				{ Ids.Stats.Attributes.Dexterity, 2.FlatSubtract() },
				{ Ids.Stats.Attributes.Constitution, 4.Flat() },
				{ Ids.Stats.Resource.MaxHealth, 25.Flat() },
				{ Ids.Stats.Combat.ArmorClass, 1.FlatSubtract() },
				{ Ids.Stats.Meta.Size, 2.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Size.Gargantuan, "Gargantuan", "An enormously large creature.") {
			IconName = "icon_tag_gargantuan",
			CreationPointCost = 75,
			MinLevel = 12,
			ConflictingTags = [Ids.Tags.Size.Tiny, Ids.Tags.Size.Small, Ids.Tags.Size.Medium, Ids.Tags.Size.Large, Ids.Tags.Size.Huge, Ids.Tags.Size.Colossal],
			SelectionWeight = 0.1f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Strength, 6.Flat() },
				{ Ids.Stats.Attributes.Dexterity, 3.FlatSubtract() },
				{ Ids.Stats.Attributes.Constitution, 6.Flat() },
				{ Ids.Stats.Resource.MaxHealth, 50.Flat() },
				{ Ids.Stats.Combat.ArmorClass, 2.FlatSubtract() },
				{ Ids.Stats.Meta.Size, 3.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Size.Colossal, "Colossal", "A creature of legendary proportions.") {
			IconName = "icon_tag_colossal",
			CreationPointCost = 100,
			MinLevel = 16,
			ConflictingTags = [Ids.Tags.Size.Tiny, Ids.Tags.Size.Small, Ids.Tags.Size.Medium, Ids.Tags.Size.Large, Ids.Tags.Size.Huge, Ids.Tags.Size.Gargantuan],
			SelectionWeight = 0.05f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Strength, 8.Flat() },
				{ Ids.Stats.Attributes.Dexterity, 4.FlatSubtract() },
				{ Ids.Stats.Attributes.Constitution, 8.Flat() },
				{ Ids.Stats.Resource.MaxHealth, 100.Flat() },
				{ Ids.Stats.Combat.ArmorClass, 3.FlatSubtract() },
				{ Ids.Stats.Meta.Size, 4.Flat() }
			}
		});
	}

	#endregion

	#region Time of Day Tags

	private void RegisterTimeOfDayTags(GameDb db) {
		db.RegisterProto(new TagProto(Ids.Tags.TimeOfDay.Nocturnal, "Nocturnal", "Active and empowered at night.") {
			IconName = "icon_tag_nocturnal",
			CanBeRandomlyApplied = false,
			ConflictingTags = [Ids.Tags.TimeOfDay.Diurnal],
			Modifiers = new() {
				{ Ids.Stats.Combat.Initiative, 2.Flat() },
				{ Ids.Skills.Stealth.Sneaking, 5.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.TimeOfDay.Diurnal, "Diurnal", "Empowered by daylight.") {
			IconName = "icon_tag_diurnal",
			CanBeRandomlyApplied = false,
			ConflictingTags = [Ids.Tags.TimeOfDay.Nocturnal],
			Modifiers = new() {
				{ Ids.Stats.Combat.DamBonus, 2.Flat() },
				{ Ids.Skills.Awareness.Perception, 25.PercentIncrease() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.TimeOfDay.Crepuscular, "Crepuscular", "Most active during dawn and dusk.") {
			IconName = "icon_tag_crepuscular",
			CanBeRandomlyApplied = false,
			Modifiers = new() {
				{ Ids.Stats.Combat.Initiative, 3.Flat() }
			}
		});

		db.RegisterProto(TagProto.Simple(Ids.Tags.TimeOfDay.Dawn, "Dawn", "Associated with dawn.", "icon_tag_dawn"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.TimeOfDay.Dusk, "Dusk", "Associated with dusk.", "icon_tag_dusk"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.TimeOfDay.Midnight, "Midnight", "Associated with midnight.", "icon_tag_midnight"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.TimeOfDay.Noon, "Noon", "Associated with noon.", "icon_tag_noon"));
	}

	#endregion

	#region Element Tags

	private void RegisterElementTags(GameDb db) {
		// Core Elements
		db.RegisterProto(new TagProto(Ids.Tags.Element.Fire, "Fire", "Infused with the power of fire.") {
			IconName = "icon_element_fire",
			ColorHint = "#FF4400",
			CreationPointCost = 30,
			MinLevel = 5,
			ConflictingTags = [Ids.Tags.Element.Cold],
			SelectionWeight = 0.4f,
			Modifiers = new() {
				{ Ids.Stats.Resistances.Fire, 50.Flat() },
				{ Ids.Stats.Resistances.Cold, 25.FlatSubtract() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Cold, "Cold", "Infused with the power of ice.") {
			IconName = "icon_element_cold",
			ColorHint = "#00CCFF",
			CreationPointCost = 30,
			MinLevel = 5,
			ConflictingTags = [Ids.Tags.Element.Fire],
			SelectionWeight = 0.4f,
			Modifiers = new() {
				{ Ids.Stats.Resistances.Cold, 50.Flat() },
				{ Ids.Stats.Resistances.Fire, 25.FlatSubtract() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Lightning, "Lightning", "Crackles with electrical energy.") {
			IconName = "icon_element_lightning",
			ColorHint = "#FFFF00",
			CreationPointCost = 30,
			MinLevel = 5,
			SelectionWeight = 0.3f,
			Modifiers = new() {
				{ Ids.Stats.Resistances.Lightning, 50.Flat() },
				{ Ids.Stats.Combat.Initiative, 3.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Poison, "Poison", "Carries deadly poison.") {
			IconName = "icon_element_poison",
			ColorHint = "#00FF00",
			CreationPointCost = 20,
			MinLevel = 2,
			SelectionWeight = 0.6f,
			Modifiers = new() {
				{ Ids.Stats.Resistances.Poison, 75.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Acid, "Acid", "Corrosive and dissolving.") {
			IconName = "icon_element_acid",
			ColorHint = "#AAFF00",
			CreationPointCost = 25,
			MinLevel = 4,
			SelectionWeight = 0.4f,
			Modifiers = new() {
				{ Ids.Stats.Resistances.Acid, 50.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Thunder, "Thunder", "Associated with thunder and sonic force.") {
			IconName = "icon_element_thunder",
			ColorHint = "#8866FF"
		});

		// Magical Elements
		db.RegisterProto(new TagProto(Ids.Tags.Element.Holy, "Holy", "Touched by divine power.") {
			IconName = "icon_element_holy",
			ColorHint = "#FFFFCC",
			CreationPointCost = 35,
			MinLevel = 6,
			ConflictingTags = [Ids.Tags.Element.Necrotic, Ids.Tags.Element.Shadow],
			SelectionWeight = 0.2f,
			Modifiers = new() {
				{ Ids.Stats.Resistances.Holy, 50.Flat() },
				{ Ids.Stats.Resistances.Necrotic, 25.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Necrotic, "Necrotic", "Touched by death energy.") {
			IconName = "icon_element_necrotic",
			ColorHint = "#330033",
			CreationPointCost = 35,
			MinLevel = 6,
			ConflictingTags = [Ids.Tags.Element.Holy, Ids.Tags.Element.Radiant],
			SelectionWeight = 0.3f,
			Modifiers = new() {
				{ Ids.Stats.Resistances.Necrotic, 50.Flat() },
				{ Ids.Stats.Resistances.Holy, 25.FlatSubtract() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Shadow, "Shadow", "Touched by darkness.") {
			IconName = "icon_element_shadow",
			ColorHint = "#220022",
			CreationPointCost = 35,
			MinLevel = 6,
			ConflictingTags = [Ids.Tags.Element.Holy, Ids.Tags.Element.Radiant],
			SelectionWeight = 0.3f,
			Modifiers = new() {
				{ Ids.Skills.Stealth.Sneaking, 20.PercentIncrease() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Radiant, "Radiant", "Infused with pure light.") {
			IconName = "icon_element_radiant",
			ColorHint = "#FFFFFF",
			ConflictingTags = [Ids.Tags.Element.Shadow, Ids.Tags.Element.Necrotic]
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Force, "Force", "Pure magical force.") {
			IconName = "icon_element_force",
			ColorHint = "#CC88FF"
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Psychic, "Psychic", "Mental and psionic energy.") {
			IconName = "icon_element_psychic",
			ColorHint = "#FF88FF"
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Arcane, "Arcane", "Raw magical energy.") {
			IconName = "icon_element_arcane",
			ColorHint = "#8888FF"
		});

		// Nature Elements
		db.RegisterProto(new TagProto(Ids.Tags.Element.Nature, "Nature", "Connected to nature.") {
			IconName = "icon_element_nature",
			ColorHint = "#228822"
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Water, "Water", "Associated with water.") {
			IconName = "icon_element_water",
			ColorHint = "#0066CC"
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Earth, "Earth", "Associated with earth and stone.") {
			IconName = "icon_element_earth",
			ColorHint = "#886644"
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Air, "Air", "Associated with air and wind.") {
			IconName = "icon_element_air",
			ColorHint = "#CCFFFF"
		});

		// Special Elements
		db.RegisterProto(new TagProto(Ids.Tags.Element.Void, "Void", "Connected to the void.") {
			IconName = "icon_element_void",
			ColorHint = "#000000"
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Temporal, "Temporal", "Associated with time.") {
			IconName = "icon_element_temporal",
			ColorHint = "#CCAA88"
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Chaos, "Chaos", "Connected to chaos.") {
			IconName = "icon_element_chaos",
			ColorHint = "#FF00FF"
		});

		db.RegisterProto(new TagProto(Ids.Tags.Element.Order, "Order", "Connected to order.") {
			IconName = "icon_element_order",
			ColorHint = "#AAAAFF"
		});
	}

	#endregion

	#region Combat Tags

	private void RegisterCombatTags(GameDb db) {
		// General Combat
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Melee, "Melee", "Melee combat oriented.", "icon_combat_melee"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Ranged, "Ranged", "Ranged combat oriented.", "icon_combat_ranged"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Defense, "Defense", "Defensive combat style.", "icon_combat_defense"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Offense, "Offense", "Offensive combat style.", "icon_combat_offense"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.DualWield, "Dual Wield", "Fights with two weapons.", "icon_combat_dualwield"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Unarmed, "Unarmed", "Fights without weapons.", "icon_combat_unarmed"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Tactics, "Tactics", "Tactical combat style.", "icon_combat_tactics"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Grappling, "Grappling", "Grappling combat style.", "icon_combat_grappling"));

		// Combat Styles with modifiers
		db.RegisterProto(new TagProto(Ids.Tags.Combat.Armored, "Armored", "Heavily protected but slower.") {
			IconName = "icon_combat_armored",
			CreationPointCost = 25,
			MinLevel = 3,
			SelectionWeight = 0.5f,
			Modifiers = new() {
				{ Ids.Stats.Combat.ArmorClass, 3.Flat() },
				{ Ids.Stats.Movement.MovementSpeed, 20.PercentReduce() },
				{ Ids.Stats.Combat.Initiative, 1.FlatSubtract() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Combat.Swift, "Swift", "Exceptionally fast and agile.") {
			IconName = "icon_combat_swift",
			CreationPointCost = 20,
			ConflictingTags = [Ids.Tags.Combat.Armored],
			SelectionWeight = 0.6f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Dexterity, 2.Flat() },
				{ Ids.Stats.Combat.Initiative, 3.Flat() },
				{ Ids.Stats.Movement.MovementSpeed, 30.PercentIncrease() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Combat.Berserking, "Berserking", "Fights with reckless fury.") {
			IconName = "icon_combat_berserking",
			CreationPointCost = 30,
			MinLevel = 4,
			ConflictingTags = [Ids.Tags.Combat.Defensive, Ids.Tags.Behavior.Cowardly],
			SelectionWeight = 0.4f,
			Modifiers = new() {
				{ Ids.Stats.Combat.DamBonus, 3.Flat() },
				{ Ids.Stats.Combat.ArmorClass, 2.FlatSubtract() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Combat.Defensive, "Defensive", "Focuses on protection and countering.") {
			IconName = "icon_combat_defensive",
			CreationPointCost = 20,
			ConflictingTags = [Ids.Tags.Combat.Berserking],
			SelectionWeight = 0.5f,
			Modifiers = new() {
				{ Ids.Stats.Combat.ArmorClass, 2.Flat() },
				{ Ids.Stats.Combat.BlockChance, 15.PercentIncrease() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Combat.Magical, "Magical", "Wields arcane power.") {
			IconName = "icon_combat_magical",
			CreationPointCost = 35,
			MinLevel = 3,
			SelectionWeight = 0.3f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Intelligence, 2.Flat() },
				{ Ids.Stats.Combat.SpellPower, 3.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Combat.Support, "Support", "Buffs allies and debuffs enemies.") {
			IconName = "icon_combat_support",
			CreationPointCost = 25,
			SelectionWeight = 0.3f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Wisdom, 2.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Combat.Assassin, "Assassin", "Strikes from shadows with deadly precision.") {
			IconName = "icon_combat_assassin",
			CreationPointCost = 40,
			MinLevel = 5,
			SelectionWeight = 0.2f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Dexterity, 2.Flat() },
				{ Ids.Stats.Combat.CriticalChance, 15.PercentIncrease() },
				{ Ids.Stats.Combat.CriticalDamage, 50.PercentMore() },
				{ Ids.Skills.Stealth.Sneaking, 25.PercentIncrease() }
			}
		});

		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Tank, "Tank", "Absorbs damage for allies.", "icon_combat_tank"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Skirmisher, "Skirmisher", "Hit and run tactics.", "icon_combat_skirmisher"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Artillery, "Artillery", "Long range damage dealer.", "icon_combat_artillery"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Combat.Controller, "Controller", "Controls the battlefield.", "icon_combat_controller"));
	}

	#endregion

	#region Behavior Tags

	private void RegisterBehaviorTags(GameDb db) {
		db.RegisterProto(new TagProto(Ids.Tags.Behavior.Cowardly, "Cowardly", "Flees when wounded.") {
			IconName = "icon_behavior_cowardly",
			ConflictingTags = [Ids.Tags.Behavior.Fearless, Ids.Tags.Combat.Berserking],
			SelectionWeight = 0.6f
		});

		db.RegisterProto(new TagProto(Ids.Tags.Behavior.Fearless, "Fearless", "Never retreats, fights to the death.") {
			IconName = "icon_behavior_fearless",
			CreationPointCost = 10,
			ConflictingTags = [Ids.Tags.Behavior.Cowardly],
			SelectionWeight = 0.4f
		});

		db.RegisterProto(new TagProto(Ids.Tags.Behavior.PackLeader, "Pack Leader", "Commands nearby allies.") {
			IconName = "icon_behavior_leader",
			CreationPointCost = 20,
			MinLevel = 3,
			ConflictingTags = [Ids.Tags.Behavior.PackMember, Ids.Tags.Behavior.Solitary],
			SelectionWeight = 0.2f,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Charisma, 2.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Behavior.PackMember, "Pack Member", "Follows the pack leader.") {
			IconName = "icon_behavior_pack",
			ConflictingTags = [Ids.Tags.Behavior.PackLeader, Ids.Tags.Behavior.Solitary],
			SelectionWeight = 0.8f
		});

		db.RegisterProto(new TagProto(Ids.Tags.Behavior.Solitary, "Solitary", "Prefers to fight alone.") {
			IconName = "icon_behavior_solitary",
			ConflictingTags = [Ids.Tags.Behavior.PackLeader, Ids.Tags.Behavior.PackMember],
			SelectionWeight = 0.4f
		});

		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Aggressive, "Aggressive", "Attacks on sight.", "icon_behavior_aggressive"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Passive, "Passive", "Does not attack unless provoked.", "icon_behavior_passive"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Defensive, "Defensive", "Defends territory.", "icon_behavior_defensive"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Opportunistic, "Opportunistic", "Targets the weakest enemy.", "icon_behavior_opportunistic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Protector, "Protector", "Defends weaker allies.", "icon_behavior_protector"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Territorial, "Territorial", "Defends its territory.", "icon_behavior_territorial"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Ambusher, "Ambusher", "Attacks from hiding.", "icon_behavior_ambusher"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Flanker, "Flanker", "Tries to flank enemies.", "icon_behavior_flanker"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Berserk, "Berserk", "Goes berserk when wounded.", "icon_behavior_berserk"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Tactical, "Tactical", "Uses advanced tactics.", "icon_behavior_tactical"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Healer, "Healer", "Prioritizes healing allies.", "icon_behavior_healer"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Summoner, "Summoner", "Summons allies.", "icon_behavior_summoner"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Caster, "Caster", "Prefers casting spells.", "icon_behavior_caster"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.MageHunter, "Mage Hunter", "Targets spellcasters.", "icon_behavior_magehunter"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Behavior.Challenger, "Challenger", "Targets the strongest enemy.", "icon_behavior_challenger"));
	}

	#endregion

	#region Creature Tags

	private void RegisterCreatureTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Humanoid, "Humanoid", "A humanoid creature.", "icon_creature_humanoid"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Beast, "Beast", "A natural beast.", "icon_creature_beast"));

		db.RegisterProto(new TagProto(Ids.Tags.Creature.Undead, "Undead", "An unliving creature.") {
			IconName = "icon_creature_undead",
			CanBeRandomlyApplied = false,
			ConflictingTags = [Ids.Tags.Creature.Construct, Ids.Tags.Element.Holy],
			Modifiers = new() {
				{ Ids.Stats.Resistances.Poison, 100.Flat() },
				{ Ids.Stats.Resistances.Necrotic, 50.Flat() },
				{ Ids.Stats.Resistances.Holy, 50.FlatSubtract() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Creature.Demon, "Demon", "A demonic creature.") {
			IconName = "icon_creature_demon",
			CanBeRandomlyApplied = false,
			ConflictingTags = [Ids.Tags.Element.Holy, Ids.Tags.Creature.Celestial],
			Modifiers = new() {
				{ Ids.Stats.Resistances.Fire, 50.Flat() },
				{ Ids.Stats.Resistances.Poison, 50.Flat() },
				{ Ids.Stats.Resistances.Holy, 50.FlatSubtract() }
			}
		});

		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Devil, "Devil", "A devil creature.", "icon_creature_devil"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Fiend, "Fiend", "A fiendish creature.", "icon_creature_fiend"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Dragon, "Dragon", "A dragon or dragon-kin.", "icon_creature_dragon"));

		db.RegisterProto(new TagProto(Ids.Tags.Creature.Construct, "Construct", "An artificial creature.") {
			IconName = "icon_creature_construct",
			CanBeRandomlyApplied = false,
			ConflictingTags = [Ids.Tags.Creature.Undead],
			Modifiers = new() {
				{ Ids.Stats.Resistances.Poison, 100.Flat() },
				{ Ids.Stats.Resistances.Psychic, 100.Flat() }
			}
		});

		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Elemental, "Elemental", "An elemental being.", "icon_creature_elemental"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Fey, "Fey", "A fey creature.", "icon_creature_fey"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Celestial, "Celestial", "A celestial being.", "icon_creature_celestial"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Aberration, "Aberration", "An aberrant creature.", "icon_creature_aberration"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Monstrosity, "Monstrosity", "A monstrous creature.", "icon_creature_monstrosity"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Ooze, "Ooze", "An ooze creature.", "icon_creature_ooze"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Plant, "Plant", "A plant creature.", "icon_creature_plant"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Giant, "Giant", "A giant creature.", "icon_creature_giant"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Swarm, "Swarm", "A swarm of creatures.", "icon_creature_swarm"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Shapechanger, "Shapechanger", "Can change shape.", "icon_creature_shapechanger"));

		db.RegisterProto(new TagProto(Ids.Tags.Creature.Ethereal, "Ethereal", "Partially phased out of reality.") {
			IconName = "icon_creature_ethereal",
			CreationPointCost = 50,
			MinLevel = 8,
			SelectionWeight = 0.1f,
			Modifiers = new() {
				{ Ids.Stats.Resistances.Physical, 50.Flat() }
			}
		});

		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.Spirit, "Spirit", "A spiritual entity.", "icon_creature_spirit"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.FogTouched, "Fog-Touched", "Touched by the fog.", "icon_creature_fogtouched"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Creature.TimeLost, "Time-Lost", "Lost in time.", "icon_creature_timelost"));
	}

	#endregion

	#region Special Tags

	private void RegisterSpecialTags(GameDb db) {
		// Rarity/Rank
		db.RegisterProto(new TagProto(Ids.Tags.Special.Elite, "Elite", "A significantly stronger variant.") {
			IconName = "icon_special_elite",
			CanBeRandomlyApplied = false,
			ConflictingTags = [Ids.Tags.Special.Champion, Ids.Tags.Special.Boss, Ids.Tags.Special.Minion],
			Modifiers = new() {
				{ Ids.Stats.Resource.MaxHealth, 50.PercentMore() },
				{ Ids.Stats.Combat.DamBonus, 2.Flat() },
				{ Ids.Stats.Combat.ArmorClass, 2.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Special.Champion, "Champion", "A boss-level variant.") {
			IconName = "icon_special_champion",
			CanBeRandomlyApplied = false,
			ConflictingTags = [Ids.Tags.Special.Elite, Ids.Tags.Special.Boss, Ids.Tags.Special.Minion],
			Modifiers = new() {
				{ Ids.Stats.Resource.MaxHealth, 100.PercentMore() },
				{ Ids.Stats.Combat.DamBonus, 4.Flat() },
				{ Ids.Stats.Combat.ArmorClass, 3.Flat() },
				{ Ids.Stats.Combat.Initiative, 2.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Special.Boss, "Boss", "A major boss creature.") {
			IconName = "icon_special_boss",
			CanBeRandomlyApplied = false,
			ConflictingTags = [Ids.Tags.Special.Elite, Ids.Tags.Special.Champion, Ids.Tags.Special.Minion],
			Modifiers = new() {
				{ Ids.Stats.Resource.MaxHealth, 200.PercentMore() },
				{ Ids.Stats.Combat.DamBonus, 6.Flat() },
				{ Ids.Stats.Combat.ArmorClass, 4.Flat() },
				{ Ids.Stats.Combat.Initiative, 4.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Special.Minion, "Minion", "A weak minion creature.") {
			IconName = "icon_special_minion",
			CanBeRandomlyApplied = false,
			ConflictingTags = [Ids.Tags.Special.Elite, Ids.Tags.Special.Champion, Ids.Tags.Special.Boss],
			Modifiers = new() {
				{ Ids.Stats.Resource.MaxHealth, 50.PercentLess() }
			}
		});

		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Legendary, "Legendary", "A legendary creature.", "icon_special_legendary"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Mythic, "Mythic", "A mythic creature.", "icon_special_mythic"));

		// Abilities
		db.RegisterProto(new TagProto(Ids.Tags.Special.Regenerating, "Regenerating", "Heals over time.") {
			IconName = "icon_special_regenerating",
			CreationPointCost = 30,
			MinLevel = 4,
			SelectionWeight = 0.3f,
			Modifiers = new() {
				{ Ids.Stats.Regeneration.Health, 5.Flat() }
			}
		});

		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Splitting, "Splitting", "Splits when killed.", "icon_special_splitting"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Explosive, "Explosive", "Explodes on death.", "icon_special_explosive"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Phasing, "Phasing", "Can phase through matter.", "icon_special_phasing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Burrowing, "Burrowing", "Can burrow underground.", "icon_special_burrowing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Flying, "Flying", "Can fly.", "icon_special_flying"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Swimming, "Swimming", "Excellent swimmer.", "icon_special_swimming"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Climbing, "Climbing", "Excellent climber.", "icon_special_climbing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Incorporeal, "Incorporeal", "Has no physical form.", "icon_special_incorporeal"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Invisible, "Invisible", "Cannot be seen.", "icon_special_invisible"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Blinking, "Blinking", "Randomly teleports.", "icon_special_blinking"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Teleporting, "Teleporting", "Can teleport.", "icon_special_teleporting"));

		// Properties
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Venomous, "Venomous", "Attacks with venom.", "icon_special_venomous"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Diseased, "Diseased", "Carries disease.", "icon_special_diseased"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Cursed, "Cursed", "Carries a curse.", "icon_special_cursed"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Blessed, "Blessed", "Divinely blessed.", "icon_special_blessed"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Armored, "Armored", "Naturally armored.", "icon_special_armored"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Shielded, "Shielded", "Has a magical shield.", "icon_special_shielded"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Vampiric, "Vampiric", "Drains life.", "icon_special_vampiric"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Draining, "Draining", "Drains energy.", "icon_special_draining"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Reflecting, "Reflecting", "Reflects attacks.", "icon_special_reflecting"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Special.Absorbing, "Absorbing", "Absorbs damage.", "icon_special_absorbing"));
	}

	#endregion

	#region Fog Tags

	private void RegisterFogTags(GameDb db) {
		db.RegisterProto(new TagProto(Ids.Tags.Fog.Lingering, "Fog-Touched (Lingering)", "Empowered by lingering fog.") {
			IconName = "icon_fog_lingering",
			CanBeRandomlyApplied = false
		});

		db.RegisterProto(new TagProto(Ids.Tags.Fog.Hazy, "Fog-Touched (Hazy)", "Obscured by hazy fog.") {
			IconName = "icon_fog_hazy",
			CanBeRandomlyApplied = false,
			Modifiers = new() {
				{ Ids.Stats.Combat.ArmorClass, 2.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Fog.Dense, "Fog-Touched (Dense)", "Empowered by dense fog.") {
			IconName = "icon_fog_dense",
			CanBeRandomlyApplied = false
		});

		db.RegisterProto(new TagProto(Ids.Tags.Fog.Thick, "Fog-Touched (Thick)", "Empowered by thick fog.") {
			IconName = "icon_fog_thick",
			CanBeRandomlyApplied = false
		});

		db.RegisterProto(new TagProto(Ids.Tags.Fog.Suffocating, "Fog-Touched (Suffocating)", "Born of the deepest fog.") {
			IconName = "icon_fog_suffocating",
			CanBeRandomlyApplied = false
		});

		db.RegisterProto(TagProto.Simple(Ids.Tags.Fog.Touched, "Fog-Touched", "Touched by the fog.", "icon_fog_touched"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Fog.Corrupted, "Fog-Corrupted", "Corrupted by the fog.", "icon_fog_corrupted"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Fog.Immune, "Fog-Immune", "Immune to fog effects.", "icon_fog_immune"));
	}

	#endregion

	#region Region Tags

	private void RegisterRegionTags(GameDb db) {
		db.RegisterProto(new TagProto(Ids.Tags.Region.Woodland, "Woodland", "Adapted to forest environments.") {
			IconName = "icon_region_woodland",
			CanBeRandomlyApplied = false,
			Modifiers = new() {
				{ Ids.Skills.Stealth.Sneaking, 20.PercentIncrease() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Region.Mountain, "Mountain", "Adapted to mountainous terrain.") {
			IconName = "icon_region_mountain",
			CanBeRandomlyApplied = false,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Constitution, 1.Flat() },
				{ Ids.Stats.Resistances.Physical, 10.PercentIncrease() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Region.Swamp, "Swamp", "Adapted to swampy conditions.") {
			IconName = "icon_region_swamp",
			CanBeRandomlyApplied = false,
			Modifiers = new() {
				{ Ids.Stats.Resistances.Poison, 25.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Region.Desert, "Desert", "Adapted to arid environments.") {
			IconName = "icon_region_desert",
			CanBeRandomlyApplied = false,
			Modifiers = new() {
				{ Ids.Stats.Resistances.Fire, 25.Flat() },
				{ Ids.Stats.Attributes.Constitution, 1.Flat() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Region.Subterranean, "Subterranean", "Dwells underground.") {
			IconName = "icon_region_subterranean",
			CanBeRandomlyApplied = false,
			Modifiers = new() {
				{ Ids.Skills.Stealth.Sneaking, 15.PercentIncrease() }
			}
		});

		db.RegisterProto(new TagProto(Ids.Tags.Region.Aquatic, "Aquatic", "Native to water environments.") {
			IconName = "icon_region_aquatic",
			CanBeRandomlyApplied = false,
			Modifiers = new() {
				{ Ids.Stats.Attributes.Dexterity, 2.Flat() }
			}
		});

		// Simple region tags
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Forest, "Forest", "Found in forests.", "icon_region_forest"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.DeepForest, "Deep Forest", "Found in deep forests.", "icon_region_deepforest"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Jungle, "Jungle", "Found in jungles.", "icon_region_jungle"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Hills, "Hills", "Found in hilly terrain.", "icon_region_hills"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Marsh, "Marsh", "Found in marshes.", "icon_region_marsh"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Tundra, "Tundra", "Found in tundra.", "icon_region_tundra"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Arctic, "Arctic", "Found in arctic regions.", "icon_region_arctic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Volcanic, "Volcanic", "Found in volcanic areas.", "icon_region_volcanic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Cave, "Cave", "Found in caves.", "icon_region_cave"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Coastal, "Coastal", "Found on coasts.", "icon_region_coastal"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Plains, "Plains", "Found on plains.", "icon_region_plains"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Grassland, "Grassland", "Found in grasslands.", "icon_region_grassland"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Urban, "Urban", "Found in urban areas.", "icon_region_urban"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Ruins, "Ruins", "Found in ruins.", "icon_region_ruins"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Graveyard, "Graveyard", "Found in graveyards.", "icon_region_graveyard"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Crypt, "Crypt", "Found in crypts.", "icon_region_crypt"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Temple, "Temple", "Found in temples.", "icon_region_temple"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Region.Dungeon, "Dungeon", "Found in dungeons.", "icon_region_dungeon"));
	}

	#endregion

	#region School Tags

	private void RegisterSchoolTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Evocation, "Evocation", "The school of evocation.", "icon_school_evocation"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Abjuration, "Abjuration", "The school of abjuration.", "icon_school_abjuration"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Conjuration, "Conjuration", "The school of conjuration.", "icon_school_conjuration"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Divination, "Divination", "The school of divination.", "icon_school_divination"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Enchantment, "Enchantment", "The school of enchantment.", "icon_school_enchantment"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Illusion, "Illusion", "The school of illusion.", "icon_school_illusion"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Necromancy, "Necromancy", "The school of necromancy.", "icon_school_necromancy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Transmutation, "Transmutation", "The school of transmutation.", "icon_school_transmutation"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Holy, "Holy", "Divine holy magic.", "icon_school_holy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Nature, "Nature", "Nature magic.", "icon_school_nature"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Primal, "Primal", "Primal magic.", "icon_school_primal"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Divine, "Divine", "Divine magic.", "icon_school_divine"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Temporal, "Temporal", "Time magic.", "icon_school_temporal"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Dark, "Dark", "Dark magic.", "icon_school_dark"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Blood, "Blood", "Blood magic.", "icon_school_blood"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Runic, "Runic", "Runic magic.", "icon_school_runic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Pact, "Pact", "Pact magic.", "icon_school_pact"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Bardic, "Bardic", "Bardic magic.", "icon_school_bardic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Ki, "Ki", "Ki-based abilities.", "icon_school_ki"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.School.Psionics, "Psionics", "Psionic abilities.", "icon_school_psionics"));
	}

	#endregion

	#region Weapon Tags

	private void RegisterWeaponTags(GameDb db) {
		// Weapon types
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Sword, "Sword", "A sword weapon.", "icon_weapon_sword"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Axe, "Axe", "An axe weapon.", "icon_weapon_axe"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Mace, "Mace", "A mace weapon.", "icon_weapon_mace"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Hammer, "Hammer", "A hammer weapon.", "icon_weapon_hammer"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Flail, "Flail", "A flail weapon.", "icon_weapon_flail"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Polearm, "Polearm", "A polearm weapon.", "icon_weapon_polearm"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Spear, "Spear", "A spear weapon.", "icon_weapon_spear"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Dagger, "Dagger", "A dagger weapon.", "icon_weapon_dagger"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Staff, "Staff", "A staff weapon.", "icon_weapon_staff"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Whip, "Whip", "A whip weapon.", "icon_weapon_whip"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Fist, "Fist", "Unarmed/fist weapons.", "icon_weapon_fist"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Claw, "Claw", "Claw weapons.", "icon_weapon_claw"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Bow, "Bow", "A bow weapon.", "icon_weapon_bow"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Crossbow, "Crossbow", "A crossbow weapon.", "icon_weapon_crossbow"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Thrown, "Thrown", "A thrown weapon.", "icon_weapon_thrown"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Sling, "Sling", "A sling weapon.", "icon_weapon_sling"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Blowgun, "Blowgun", "A blowgun weapon.", "icon_weapon_blowgun"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Shield, "Shield", "A shield.", "icon_weapon_shield"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Wand, "Wand", "A wand.", "icon_weapon_wand"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Orb, "Orb", "An orb focus.", "icon_weapon_orb"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Tome, "Tome", "A tome.", "icon_weapon_tome"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Instrument, "Instrument", "A musical instrument.", "icon_weapon_instrument"));

		// Weapon properties
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.TwoHanded, "Two-Handed", "Requires two hands.", "icon_weapon_twohanded"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.OneHanded, "One-Handed", "Uses one hand.", "icon_weapon_onehanded"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Light, "Light", "A light weapon.", "icon_weapon_light"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Heavy, "Heavy", "A heavy weapon.", "icon_weapon_heavy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Finesse, "Finesse", "Can use DEX for attacks.", "icon_weapon_finesse"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Reach, "Reach", "Has extended reach.", "icon_weapon_reach"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Weapon.Versatile, "Versatile", "Can be used one or two-handed.", "icon_weapon_versatile"));
	}

	#endregion

	#region Armor Tags

	private void RegisterArmorTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Cloth, "Cloth", "Cloth armor.", "icon_armor_cloth"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Light, "Light", "Light armor.", "icon_armor_light"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Medium, "Medium", "Medium armor.", "icon_armor_medium"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Heavy, "Heavy", "Heavy armor.", "icon_armor_heavy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Shield, "Shield", "A shield.", "icon_armor_shield"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Robe, "Robe", "A robe.", "icon_armor_robe"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Head, "Head", "Head slot.", "icon_armor_head"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Chest, "Chest", "Chest slot.", "icon_armor_chest"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Legs, "Legs", "Legs slot.", "icon_armor_legs"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Feet, "Feet", "Feet slot.", "icon_armor_feet"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Hands, "Hands", "Hands slot.", "icon_armor_hands"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Back, "Back", "Back slot.", "icon_armor_back"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Waist, "Waist", "Waist slot.", "icon_armor_waist"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Ring, "Ring", "Ring slot.", "icon_armor_ring"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Amulet, "Amulet", "Amulet slot.", "icon_armor_amulet"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Armor.Trinket, "Trinket", "Trinket slot.", "icon_armor_trinket"));
	}

	#endregion

	#region Mastery Tags

	private void RegisterMasteryTags(GameDb db) {
		// School masteries
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Evocation, "Evocation Mastery", "Mastery of evocation magic.", "icon_mastery_evocation"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Abjuration, "Abjuration Mastery", "Mastery of abjuration magic.", "icon_mastery_abjuration"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Conjuration, "Conjuration Mastery", "Mastery of conjuration magic.", "icon_mastery_conjuration"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Divination, "Divination Mastery", "Mastery of divination magic.", "icon_mastery_divination"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Enchantment, "Enchantment Mastery", "Mastery of enchantment magic.", "icon_mastery_enchantment"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Illusion, "Illusion Mastery", "Mastery of illusion magic.", "icon_mastery_illusion"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Necromancy, "Necromancy Mastery", "Mastery of necromancy magic.", "icon_mastery_necromancy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Transmutation, "Transmutation Mastery", "Mastery of transmutation magic.", "icon_mastery_transmutation"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Holy, "Holy Mastery", "Mastery of holy magic.", "icon_mastery_holy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Nature, "Nature Mastery", "Mastery of nature magic.", "icon_mastery_nature"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Temporal, "Temporal Mastery", "Mastery of time magic.", "icon_mastery_temporal"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Blood, "Blood Mastery", "Mastery of blood magic.", "icon_mastery_blood"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Runic, "Runic Mastery", "Mastery of runic magic.", "icon_mastery_runic"));

		// Element masteries
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Fire, "Fire Mastery", "Mastery of fire.", "icon_mastery_fire"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Cold, "Cold Mastery", "Mastery of cold.", "icon_mastery_cold"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Lightning, "Lightning Mastery", "Mastery of lightning.", "icon_mastery_lightning"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Poison, "Poison Mastery", "Mastery of poison.", "icon_mastery_poison"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Shadow, "Shadow Mastery", "Mastery of shadow.", "icon_mastery_shadow"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Mastery.Light, "Light Mastery", "Mastery of light.", "icon_mastery_light"));
	}

	#endregion

	#region Attribute Tags

	private void RegisterAttributeTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Attribute.Strength, "Strength", "Governed by Strength.", "icon_attr_strength"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Attribute.Dexterity, "Dexterity", "Governed by Dexterity.", "icon_attr_dexterity"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Attribute.Constitution, "Constitution", "Governed by Constitution.", "icon_attr_constitution"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Attribute.Intelligence, "Intelligence", "Governed by Intelligence.", "icon_attr_intelligence"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Attribute.Wisdom, "Wisdom", "Governed by Wisdom.", "icon_attr_wisdom"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Attribute.Charisma, "Charisma", "Governed by Charisma.", "icon_attr_charisma"));
	}

	#endregion

	#region Meta Tags

	private void RegisterMetaTags(GameDb db) {
		// Action Types
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Passive, "Passive", "Always active.", "icon_meta_passive"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Active, "Active", "Must be activated.", "icon_meta_active"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Toggle, "Toggle", "Can be toggled on/off.", "icon_meta_toggle"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Reaction, "Reaction", "Used as a reaction.", "icon_meta_reaction"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.BonusAction, "Bonus Action", "Uses a bonus action.", "icon_meta_bonusaction"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Concentration, "Concentration", "Requires concentration.", "icon_meta_concentration"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Ritual, "Ritual", "Can be cast as a ritual.", "icon_meta_ritual"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Cantrip, "Cantrip", "A cantrip spell.", "icon_meta_cantrip"));

		// Effect Types
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Offensive, "Offensive", "Deals damage or debuffs.", "icon_meta_offensive"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Defensive, "Defensive", "Provides protection.", "icon_meta_defensive"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Utility, "Utility", "Provides utility.", "icon_meta_utility"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Healing, "Healing", "Restores health.", "icon_meta_healing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Buff, "Buff", "Provides a beneficial effect.", "icon_meta_buff"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Debuff, "Debuff", "Applies a harmful effect.", "icon_meta_debuff"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.DoT, "DoT", "Damage over time.", "icon_meta_dot"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.HoT, "HoT", "Healing over time.", "icon_meta_hot"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Control, "Control", "Controls enemies.", "icon_meta_control"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Crowd, "Crowd Control", "Affects multiple enemies.", "icon_meta_crowd"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Movement, "Movement", "Affects movement.", "icon_meta_movement"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Teleport, "Teleport", "Teleportation effect.", "icon_meta_teleport"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Summon, "Summon", "Summons allies.", "icon_meta_summon"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Transform, "Transform", "Transformation effect.", "icon_meta_transform"));

		// Target Types
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Self, "Self", "Targets self.", "icon_meta_self"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.SingleTarget, "Single Target", "Targets one entity.", "icon_meta_singletarget"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.MultiTarget, "Multi-Target", "Targets multiple entities.", "icon_meta_multitarget"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.AoE, "AoE", "Area of effect.", "icon_meta_aoe"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Cone, "Cone", "Cone-shaped area.", "icon_meta_cone"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Line, "Line", "Line-shaped area.", "icon_meta_line"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Sphere, "Sphere", "Spherical area.", "icon_meta_sphere"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Allies, "Allies", "Targets allies.", "icon_meta_allies"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Enemies, "Enemies", "Targets enemies.", "icon_meta_enemies"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.All, "All", "Targets everyone.", "icon_meta_all"));

		// Skill Meta Categories
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.CombatSkill, "Combat Skill", "A combat-related skill.", "icon_meta_combatskill"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.MagicSkill, "Magic Skill", "A magic-related skill.", "icon_meta_magicskill"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.CraftingSkill, "Crafting Skill", "A crafting-related skill.", "icon_meta_craftingskill"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.SocialSkill, "Social Skill", "A social-related skill.", "icon_meta_socialskill"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.KnowledgeSkill, "Knowledge Skill", "A knowledge-related skill.", "icon_meta_knowledgeskill"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.PhysicalSkill, "Physical Skill", "A physical skill.", "icon_meta_physicalskill"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.StealthSkill, "Stealth Skill", "A stealth-related skill.", "icon_meta_stealthskill"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.SurvivalSkill, "Survival Skill", "A survival-related skill.", "icon_meta_survivalskill"));

		// Rarity
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Common, "Common", "Common rarity.", "icon_meta_common"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Uncommon, "Uncommon", "Uncommon rarity.", "icon_meta_uncommon"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Rare, "Rare", "Rare rarity.", "icon_meta_rare"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Epic, "Epic", "Epic rarity.", "icon_meta_epic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Legendary, "Legendary", "Legendary rarity.", "icon_meta_legendary"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Mythic, "Mythic", "Mythic rarity.", "icon_meta_mythic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Meta.Unique, "Unique", "Unique item.", "icon_meta_unique"));
	}

	#endregion

	#region Physical Tags

	private void RegisterPhysicalTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Physical.Athletics, "Athletics", "Athletic ability.", "icon_physical_athletics"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Physical.Acrobatics, "Acrobatics", "Acrobatic ability.", "icon_physical_acrobatics"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Physical.Endurance, "Endurance", "Physical endurance.", "icon_physical_endurance"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Physical.Swimming, "Swimming", "Swimming ability.", "icon_physical_swimming"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Physical.Climbing, "Climbing", "Climbing ability.", "icon_physical_climbing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Physical.Jumping, "Jumping", "Jumping ability.", "icon_physical_jumping"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Physical.Running, "Running", "Running ability.", "icon_physical_running"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Physical.Flexibility, "Flexibility", "Physical flexibility.", "icon_physical_flexibility"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Physical.Balance, "Balance", "Balance ability.", "icon_physical_balance"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Physical.Reflexes, "Reflexes", "Quick reflexes.", "icon_physical_reflexes"));
	}

	#endregion

	#region Stealth Tags

	private void RegisterStealthTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.Sneaking, "Sneaking", "Moving silently.", "icon_stealth_sneaking"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.Hiding, "Hiding", "Staying hidden.", "icon_stealth_hiding"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.Lockpicking, "Lockpicking", "Picking locks.", "icon_stealth_lockpicking"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.TrapHandling, "Trap Handling", "Disarming traps.", "icon_stealth_traphandling"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.TrapSetting, "Trap Setting", "Setting traps.", "icon_stealth_trapsetting"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.Pickpocket, "Pickpocket", "Stealing from pockets.", "icon_stealth_pickpocket"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.SleightOfHand, "Sleight of Hand", "Manual dexterity tricks.", "icon_stealth_sleightofhand"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.Disguise, "Disguise", "Creating disguises.", "icon_stealth_disguise"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.Forgery, "Forgery", "Creating forgeries.", "icon_stealth_forgery"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.Shadowing, "Shadowing", "Following undetected.", "icon_stealth_shadowing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.Infiltration, "Infiltration", "Infiltrating locations.", "icon_stealth_infiltration"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Stealth.Escape, "Escape", "Escaping restraints.", "icon_stealth_escape"));
	}

	#endregion

	#region Awareness Tags

	private void RegisterAwarenessTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Awareness.Perception, "Perception", "Noticing things.", "icon_awareness_perception"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Awareness.Insight, "Insight", "Reading people.", "icon_awareness_insight"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Awareness.Investigation, "Investigation", "Finding clues.", "icon_awareness_investigation"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Awareness.Tracking, "Tracking", "Following trails.", "icon_awareness_tracking"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Awareness.Search, "Search", "Searching areas.", "icon_awareness_search"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Awareness.SenseMotives, "Sense Motives", "Detecting lies.", "icon_awareness_sensemotives"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Awareness.DetectMagic, "Detect Magic", "Sensing magic.", "icon_awareness_detectmagic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Awareness.DetectTraps, "Detect Traps", "Finding traps.", "icon_awareness_detecttraps"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Awareness.DetectUndead, "Detect Undead", "Sensing undead.", "icon_awareness_detectundead"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Awareness.Alertness, "Alertness", "Staying alert.", "icon_awareness_alertness"));
	}

	#endregion

	#region Survival Tags

	private void RegisterSurvivalTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.General, "Survival", "General survival.", "icon_survival_general"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.NatureLore, "Nature Lore", "Knowledge of nature.", "icon_survival_naturelore"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.AnimalHandling, "Animal Handling", "Working with animals.", "icon_survival_animalhandling"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.AnimalTraining, "Animal Training", "Training animals.", "icon_survival_animaltraining"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.Foraging, "Foraging", "Finding food.", "icon_survival_foraging"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.Hunting, "Hunting", "Hunting game.", "icon_survival_hunting"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.Fishing, "Fishing", "Catching fish.", "icon_survival_fishing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.Navigation, "Navigation", "Finding your way.", "icon_survival_navigation"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.Camping, "Camping", "Setting up camp.", "icon_survival_camping"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.FireStarting, "Fire Starting", "Starting fires.", "icon_survival_firestarting"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.WeatherSense, "Weather Sense", "Predicting weather.", "icon_survival_weathersense"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.Herbalism, "Herbalism", "Gathering herbs.", "icon_survival_herbalism"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.Trapping, "Trapping", "Setting animal traps.", "icon_survival_trapping"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Survival.Skinning, "Skinning", "Skinning animals.", "icon_survival_skinning"));
	}

	#endregion

	#region Crafting Tags

	private void RegisterCraftingTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Alchemy, "Alchemy", "Creating potions.", "icon_crafting_alchemy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Smithing, "Smithing", "Working metal.", "icon_crafting_smithing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Weaponsmithing, "Weaponsmithing", "Crafting weapons.", "icon_crafting_weaponsmithing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Armorsmithing, "Armorsmithing", "Crafting armor.", "icon_crafting_armorsmithing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Leatherworking, "Leatherworking", "Working leather.", "icon_crafting_leatherworking"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Tailoring, "Tailoring", "Making clothes.", "icon_crafting_tailoring"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Enchanting, "Enchanting", "Enchanting items.", "icon_crafting_enchanting"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Scribing, "Scribing", "Writing scrolls.", "icon_crafting_scribing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Inscription, "Inscription", "Inscribing runes.", "icon_crafting_inscription"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Runecraft, "Runecraft", "Crafting runes.", "icon_crafting_runecraft"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Cooking, "Cooking", "Preparing food.", "icon_crafting_cooking"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Brewing, "Brewing", "Brewing drinks.", "icon_crafting_brewing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Woodworking, "Woodworking", "Working wood.", "icon_crafting_woodworking"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Carpentry, "Carpentry", "Building with wood.", "icon_crafting_carpentry"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Bowmaking, "Bowmaking", "Crafting bows.", "icon_crafting_bowmaking"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Mining, "Mining", "Extracting ore.", "icon_crafting_mining"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Smelting, "Smelting", "Smelting ore.", "icon_crafting_smelting"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Jewelcrafting, "Jewelcrafting", "Crafting jewelry.", "icon_crafting_jewelcrafting"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Glassblowing, "Glassblowing", "Working glass.", "icon_crafting_glassblowing"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Pottery, "Pottery", "Making pottery.", "icon_crafting_pottery"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Tinkering, "Tinkering", "Building gadgets.", "icon_crafting_tinkering"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Engineering, "Engineering", "Engineering devices.", "icon_crafting_engineering"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Crafting.Trapmaking, "Trapmaking", "Making traps.", "icon_crafting_trapmaking"));
	}

	#endregion

	#region Social Tags

	private void RegisterSocialTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Persuasion, "Persuasion", "Convincing others.", "icon_social_persuasion"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Intimidation, "Intimidation", "Threatening others.", "icon_social_intimidation"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Deception, "Deception", "Lying to others.", "icon_social_deception"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Diplomacy, "Diplomacy", "Negotiating peacefully.", "icon_social_diplomacy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Performance, "Performance", "Entertaining others.", "icon_social_performance"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Mercantile, "Mercantile", "Trading goods.", "icon_social_mercantile"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Bartering, "Bartering", "Haggling prices.", "icon_social_bartering"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Etiquette, "Etiquette", "Proper manners.", "icon_social_etiquette"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Leadership, "Leadership", "Leading others.", "icon_social_leadership"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Inspiration, "Inspiration", "Inspiring others.", "icon_social_inspiration"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Seduction, "Seduction", "Charming others.", "icon_social_seduction"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Bluff, "Bluff", "Bluffing others.", "icon_social_bluff"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Gather, "Gather Information", "Gathering intel.", "icon_social_gather"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Streetwise, "Streetwise", "Street smarts.", "icon_social_streetwise"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Nobility, "Nobility", "Noble knowledge.", "icon_social_nobility"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Social.Command, "Command", "Commanding others.", "icon_social_command"));
	}

	#endregion

	#region Knowledge Tags

	private void RegisterKnowledgeTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Arcana, "Arcana", "Magical knowledge.", "icon_knowledge_arcana"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.History, "History", "Historical knowledge.", "icon_knowledge_history"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Religion, "Religion", "Religious knowledge.", "icon_knowledge_religion"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Medicine, "Medicine", "Medical knowledge.", "icon_knowledge_medicine"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Anatomy, "Anatomy", "Anatomical knowledge.", "icon_knowledge_anatomy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Planes, "Planes", "Planar knowledge.", "icon_knowledge_planes"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Undead, "Undead Lore", "Undead knowledge.", "icon_knowledge_undead"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Demonology, "Demonology", "Demon knowledge.", "icon_knowledge_demonology"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Geography, "Geography", "Geographical knowledge.", "icon_knowledge_geography"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Dungeoneering, "Dungeoneering", "Dungeon knowledge.", "icon_knowledge_dungeoneering"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Engineering, "Engineering", "Engineering knowledge.", "icon_knowledge_engineering"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Law, "Law", "Legal knowledge.", "icon_knowledge_law"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Nature, "Nature", "Nature knowledge.", "icon_knowledge_nature"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Beasts, "Beasts", "Beast knowledge.", "icon_knowledge_beasts"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Dragons, "Dragons", "Dragon knowledge.", "icon_knowledge_dragons"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Constructs, "Constructs", "Construct knowledge.", "icon_knowledge_constructs"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Fey, "Fey", "Fey knowledge.", "icon_knowledge_fey"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Aberrations, "Aberrations", "Aberration knowledge.", "icon_knowledge_aberrations"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Languages, "Languages", "Language knowledge.", "icon_knowledge_languages"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Cryptography, "Cryptography", "Code knowledge.", "icon_knowledge_cryptography"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Alchemy, "Alchemy Lore", "Alchemy knowledge.", "icon_knowledge_alchemy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Astronomy, "Astronomy", "Star knowledge.", "icon_knowledge_astronomy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Herbology, "Herbology", "Herb knowledge.", "icon_knowledge_herbology"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Poison, "Poison Lore", "Poison knowledge.", "icon_knowledge_poison"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Fog, "Fog Lore", "Fog knowledge.", "icon_knowledge_fog"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Knowledge.Time, "Time Lore", "Time knowledge.", "icon_knowledge_time"));
	}

	#endregion

	#region Class Tags

	private void RegisterClassTags(GameDb db) {
		// Barbarian
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Rage, "Rage", "Barbarian rage ability.", "icon_class_rage"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Frenzy, "Frenzy", "Frenzied rage.", "icon_class_frenzy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Totem, "Totem", "Totem warrior ability.", "icon_class_totem"));

		// Bard
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Bardic, "Bardic", "Bardic ability.", "icon_class_bardic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Song, "Song", "Bardic song.", "icon_class_song"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Inspiration, "Inspiration", "Bardic inspiration.", "icon_class_inspiration"));

		// Cleric
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Channel, "Channel Divinity", "Channel divinity ability.", "icon_class_channel"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Domain, "Domain", "Cleric domain.", "icon_class_domain"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.TurnUndead, "Turn Undead", "Turn undead ability.", "icon_class_turnundead"));

		// Druid
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Shapeshifting, "Shapeshifting", "Shapeshifting ability.", "icon_class_shapeshifting"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.WildShape, "Wild Shape", "Wild shape ability.", "icon_class_wildshape"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.NaturalMagic, "Natural Magic", "Nature magic.", "icon_class_naturalmagic"));

		// Fighter
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.CombatManeuvers, "Combat Maneuvers", "Combat maneuver ability.", "icon_class_combatmaneuvers"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Superiority, "Superiority", "Battle master ability.", "icon_class_superiority"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.SecondWind, "Second Wind", "Second wind ability.", "icon_class_secondwind"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.ActionSurge, "Action Surge", "Action surge ability.", "icon_class_actionsurge"));

		// Monk
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Ki, "Ki", "Ki ability.", "icon_class_ki"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.FlurryOfBlows, "Flurry of Blows", "Flurry of blows ability.", "icon_class_flurryofblows"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.StunningStrike, "Stunning Strike", "Stunning strike ability.", "icon_class_stunningstrike"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Meditation, "Meditation", "Meditation ability.", "icon_class_meditation"));

		// Paladin
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.DivineSmite, "Divine Smite", "Divine smite ability.", "icon_class_divinesmite"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.LayOnHands, "Lay on Hands", "Lay on hands ability.", "icon_class_layonhands"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Aura, "Aura", "Paladin aura.", "icon_class_aura"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Oath, "Oath", "Sacred oath.", "icon_class_oath"));

		// Ranger
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.FavoredEnemy, "Favored Enemy", "Favored enemy ability.", "icon_class_favoredenemy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.FavoredTerrain, "Favored Terrain", "Favored terrain ability.", "icon_class_favoredterrain"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.HuntersMark, "Hunter's Mark", "Hunter's mark ability.", "icon_class_huntersmark"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Companion, "Companion", "Animal companion.", "icon_class_companion"));

		// Rogue
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.SneakAttack, "Sneak Attack", "Sneak attack ability.", "icon_class_sneakattack"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.CunningAction, "Cunning Action", "Cunning action ability.", "icon_class_cunningaction"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Evasion, "Evasion", "Evasion ability.", "icon_class_evasion"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Uncanny, "Uncanny Dodge", "Uncanny dodge ability.", "icon_class_uncanny"));

		// Sorcerer
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Metamagic, "Metamagic", "Metamagic ability.", "icon_class_metamagic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Bloodline, "Bloodline", "Sorcerer bloodline.", "icon_class_bloodline"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.SorceryPoints, "Sorcery Points", "Sorcery points.", "icon_class_sorcerypoints"));

		// Warlock
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Pact, "Pact", "Warlock pact.", "icon_class_pact"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Invocation, "Invocation", "Eldritch invocation.", "icon_class_invocation"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.EldritchBlast, "Eldritch Blast", "Eldritch blast.", "icon_class_eldritchblast"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Patron, "Patron", "Warlock patron.", "icon_class_patron"));

		// Wizard
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Spellbook, "Spellbook", "Wizard spellbook.", "icon_class_spellbook"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.ArcaneRecovery, "Arcane Recovery", "Arcane recovery ability.", "icon_class_arcanerecovery"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.SchoolSpecialization, "School Specialization", "School specialization.", "icon_class_schoolspecialization"));

		// Game-Specific
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.TimeWalker, "Time Walker", "Time walker ability.", "icon_class_timewalker"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.Ascended, "Ascended", "Ascended ability.", "icon_class_ascended"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Class.FogWalker, "Fog Walker", "Fog walker ability.", "icon_class_fogwalker"));
	}

	#endregion

	#region Resistance Tags

	private void RegisterResistanceTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Physical, "Physical Resistance", "Resistant to physical damage.", "icon_resist_physical"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Magical, "Magical Resistance", "Resistant to magical damage.", "icon_resist_magical"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Fire, "Fire Resistance", "Resistant to fire damage.", "icon_resist_fire"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Cold, "Cold Resistance", "Resistant to cold damage.", "icon_resist_cold"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Lightning, "Lightning Resistance", "Resistant to lightning damage.", "icon_resist_lightning"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Poison, "Poison Resistance", "Resistant to poison damage.", "icon_resist_poison"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Acid, "Acid Resistance", "Resistant to acid damage.", "icon_resist_acid"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Holy, "Holy Resistance", "Resistant to holy damage.", "icon_resist_holy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Necrotic, "Necrotic Resistance", "Resistant to necrotic damage.", "icon_resist_necrotic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Psychic, "Psychic Resistance", "Resistant to psychic damage.", "icon_resist_psychic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Resistance.Force, "Force Resistance", "Resistant to force damage.", "icon_resist_force"));
	}

	#endregion

	#region Immunity Tags

	private void RegisterImmunityTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Poison, "Poison Immunity", "Immune to poison.", "icon_immune_poison"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Disease, "Disease Immunity", "Immune to disease.", "icon_immune_disease"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Fear, "Fear Immunity", "Immune to fear.", "icon_immune_fear"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Charm, "Charm Immunity", "Immune to charm.", "icon_immune_charm"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Sleep, "Sleep Immunity", "Immune to sleep.", "icon_immune_sleep"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Stun, "Stun Immunity", "Immune to stun.", "icon_immune_stun"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Paralysis, "Paralysis Immunity", "Immune to paralysis.", "icon_immune_paralysis"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Petrification, "Petrification Immunity", "Immune to petrification.", "icon_immune_petrification"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Death, "Death Immunity", "Immune to death effects.", "icon_immune_death"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Critical, "Critical Immunity", "Immune to critical hits.", "icon_immune_critical"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Immunity.Bleed, "Bleed Immunity", "Immune to bleeding.", "icon_immune_bleed"));
	}

	#endregion

	#region Vulnerability Tags

	private void RegisterVulnerabilityTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Vulnerability.Fire, "Fire Vulnerability", "Vulnerable to fire.", "icon_vuln_fire"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Vulnerability.Cold, "Cold Vulnerability", "Vulnerable to cold.", "icon_vuln_cold"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Vulnerability.Lightning, "Lightning Vulnerability", "Vulnerable to lightning.", "icon_vuln_lightning"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Vulnerability.Holy, "Holy Vulnerability", "Vulnerable to holy.", "icon_vuln_holy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Vulnerability.Necrotic, "Necrotic Vulnerability", "Vulnerable to necrotic.", "icon_vuln_necrotic"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Vulnerability.Silver, "Silver Vulnerability", "Vulnerable to silver.", "icon_vuln_silver"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Vulnerability.Magic, "Magic Vulnerability", "Vulnerable to magic.", "icon_vuln_magic"));
	}

	#endregion

	#region Condition Tags

	private void RegisterConditionTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Blinded, "Blinded", "Cannot see.", "icon_condition_blinded"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Charmed, "Charmed", "Charmed by another.", "icon_condition_charmed"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Deafened, "Deafened", "Cannot hear.", "icon_condition_deafened"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Exhausted, "Exhausted", "Suffering exhaustion.", "icon_condition_exhausted"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Frightened, "Frightened", "Afraid.", "icon_condition_frightened"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Grappled, "Grappled", "Being grappled.", "icon_condition_grappled"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Incapacitated, "Incapacitated", "Cannot act.", "icon_condition_incapacitated"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Invisible, "Invisible", "Cannot be seen.", "icon_condition_invisible"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Paralyzed, "Paralyzed", "Cannot move.", "icon_condition_paralyzed"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Petrified, "Petrified", "Turned to stone.", "icon_condition_petrified"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Poisoned, "Poisoned", "Suffering poison.", "icon_condition_poisoned"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Prone, "Prone", "Lying down.", "icon_condition_prone"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Restrained, "Restrained", "Movement restricted.", "icon_condition_restrained"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Stunned, "Stunned", "Stunned.", "icon_condition_stunned"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Unconscious, "Unconscious", "Knocked out.", "icon_condition_unconscious"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Bleeding, "Bleeding", "Losing blood.", "icon_condition_bleeding"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Burning, "Burning", "On fire.", "icon_condition_burning"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Frozen, "Frozen", "Frozen solid.", "icon_condition_frozen"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Silenced, "Silenced", "Cannot speak.", "icon_condition_silenced"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Slowed, "Slowed", "Movement slowed.", "icon_condition_slowed"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Rooted, "Rooted", "Cannot move.", "icon_condition_rooted"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Weakened, "Weakened", "Strength reduced.", "icon_condition_weakened"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Cursed, "Cursed", "Under a curse.", "icon_condition_cursed"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Blessed, "Blessed", "Divinely blessed.", "icon_condition_blessed"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Empowered, "Empowered", "Power increased.", "icon_condition_empowered"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Shielded, "Shielded", "Protected by shield.", "icon_condition_shielded"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Regenerating, "Regenerating", "Health regenerating.", "icon_condition_regenerating"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Hasted, "Hasted", "Speed increased.", "icon_condition_hasted"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Enraged, "Enraged", "In a rage.", "icon_condition_enraged"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Confused, "Confused", "Confused.", "icon_condition_confused"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Condition.Dominated, "Dominated", "Mind controlled.", "icon_condition_dominated"));
	}

	#endregion

	#region Item Tags

	private void RegisterItemTags(GameDb db) {
		// Materials
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Wood, "Wood", "Made of wood.", "icon_item_wood"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Stone, "Stone", "Made of stone.", "icon_item_stone"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Iron, "Iron", "Made of iron.", "icon_item_iron"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Steel, "Steel", "Made of steel.", "icon_item_steel"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Mithril, "Mithril", "Made of mithril.", "icon_item_mithril"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Adamantine, "Adamantine", "Made of adamantine.", "icon_item_adamantine"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Silver, "Silver", "Made of silver.", "icon_item_silver"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Gold, "Gold", "Made of gold.", "icon_item_gold"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Leather, "Leather", "Made of leather.", "icon_item_leather"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Cloth, "Cloth", "Made of cloth.", "icon_item_cloth"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Bone, "Bone", "Made of bone.", "icon_item_bone"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Crystal, "Crystal", "Made of crystal.", "icon_item_crystal"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Obsidian, "Obsidian", "Made of obsidian.", "icon_item_obsidian"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.DragonScale, "Dragon Scale", "Made of dragon scale.", "icon_item_dragonscale"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.DemonHide, "Demon Hide", "Made of demon hide.", "icon_item_demonhide"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Ethereal, "Ethereal", "Made of ethereal material.", "icon_item_ethereal"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Void, "Void", "Made of void material.", "icon_item_void"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Temporal, "Temporal", "Made of temporal material.", "icon_item_temporal"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.FogMaterial, "Fog Material", "Made of fog material.", "icon_item_fogmaterial"));

		// Item Types
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Consumable, "Consumable", "Can be consumed.", "icon_item_consumable"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Equipment, "Equipment", "Can be equipped.", "icon_item_equipment"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Material, "Material", "A crafting material.", "icon_item_material"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Quest, "Quest", "A quest item.", "icon_item_quest"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Key, "Key", "A key item.", "icon_item_key"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Treasure, "Treasure", "A treasure.", "icon_item_treasure"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Junk, "Junk", "Junk item.", "icon_item_junk"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.CraftingMat, "Crafting Material", "Used in crafting.", "icon_item_craftingmat"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Recipe, "Recipe", "A crafting recipe.", "icon_item_recipe"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Scroll, "Scroll", "A magic scroll.", "icon_item_scroll"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Potion, "Potion", "A potion.", "icon_item_potion"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Food, "Food", "Food item.", "icon_item_food"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Drink, "Drink", "Drink item.", "icon_item_drink"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Ammo, "Ammo", "Ammunition.", "icon_item_ammo"));

		// Item Properties
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Magical, "Magical", "A magical item.", "icon_item_magical"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Cursed, "Cursed", "A cursed item.", "icon_item_cursed"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Blessed, "Blessed", "A blessed item.", "icon_item_blessed"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Enchanted, "Enchanted", "An enchanted item.", "icon_item_enchanted"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.SetPiece, "Set Piece", "Part of a set.", "icon_item_setpiece"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Soulbound, "Soulbound", "Bound to soul.", "icon_item_soulbound"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Stackable, "Stackable", "Can be stacked.", "icon_item_stackable"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Tradeable, "Tradeable", "Can be traded.", "icon_item_tradeable"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Item.Destroyable, "Destroyable", "Can be destroyed.", "icon_item_destroyable"));
	}

	#endregion

	#region Source Tags

	private void RegisterSourceTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Player, "Player", "From player.", "icon_source_player"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Enemy, "Enemy", "From enemy.", "icon_source_enemy"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Ally, "Ally", "From ally.", "icon_source_ally"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Environment, "Environment", "From environment.", "icon_source_environment"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Trap, "Trap", "From trap.", "icon_source_trap"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Spell, "Spell", "From spell.", "icon_source_spell"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Skill, "Skill", "From skill.", "icon_source_skill"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.ItemSource, "Item", "From item.", "icon_source_item"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Equipment, "Equipment", "From equipment.", "icon_source_equipment"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Consumable, "Consumable", "From consumable.", "icon_source_consumable"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Aura, "Aura", "From aura.", "icon_source_aura"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Fog, "Fog", "From fog.", "icon_source_fog"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Source.Time, "Time", "From time.", "icon_source_time"));
	}

	#endregion

	#region Trigger Tags

	private void RegisterTriggerTags(GameDb db) {
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnHit, "On Hit", "Triggers when hitting an enemy.", "icon_trigger_onhit"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnCrit, "On Crit", "Triggers on critical hit.", "icon_trigger_oncrit"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnKill, "On Kill", "Triggers when killing an enemy.", "icon_trigger_onkill"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnDamage, "On Damage", "Triggers when taking damage.", "icon_trigger_ondamage"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnHeal, "On Heal", "Triggers when healed.", "icon_trigger_onheal"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnDeath, "On Death", "Triggers on death.", "icon_trigger_ondeath"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnBlock, "On Block", "Triggers when blocking.", "icon_trigger_onblock"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnDodge, "On Dodge", "Triggers when dodging.", "icon_trigger_ondodge"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnSpellCast, "On Spell Cast", "Triggers when casting a spell.", "icon_trigger_onspellcast"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnSkillUse, "On Skill Use", "Triggers when using a skill.", "icon_trigger_onskilluse"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnTurnStart, "On Turn Start", "Triggers at turn start.", "icon_trigger_onturnstart"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnTurnEnd, "On Turn End", "Triggers at turn end.", "icon_trigger_onturnend"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnCombatStart, "On Combat Start", "Triggers at combat start.", "icon_trigger_oncombatstart"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnCombatEnd, "On Combat End", "Triggers at combat end.", "icon_trigger_oncombatend"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnRest, "On Rest", "Triggers when resting.", "icon_trigger_onrest"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnLevelUp, "On Level Up", "Triggers on level up.", "icon_trigger_onlevelup"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnLowHealth, "On Low Health", "Triggers at low health.", "icon_trigger_onlowhealth"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnFullHealth, "On Full Health", "Triggers at full health.", "icon_trigger_onfullhealth"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnStatusApplied, "On Status Applied", "Triggers when status applied.", "icon_trigger_onstatusapplied"));
		db.RegisterProto(TagProto.Simple(Ids.Tags.Trigger.OnStatusRemoved, "On Status Removed", "Triggers when status removed.", "icon_trigger_onstatusremoved"));
	}

	#endregion
}