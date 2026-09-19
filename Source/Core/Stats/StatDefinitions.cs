using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Stats;

namespace RPGGame.Core.Stats;

/// <summary>
/// Defines all stat categories and stats using the Proto system.
/// </summary>
public class StatDefinitions : ICoreData {
	public void GameData(GameDb gameDatabase) {
		RegisterCategories(gameDatabase);
		RegisterAttributes(gameDatabase);
		RegisterResources(gameDatabase);
		RegisterCombat(gameDatabase);
		RegisterDefense(gameDatabase);
		RegisterResistances(gameDatabase);
		RegisterVulnerabilities(gameDatabase);
		RegisterDamageBonuses(gameDatabase);
		RegisterMovement(gameDatabase);
		RegisterRegeneration(gameDatabase);
		RegisterEconomy(gameDatabase);
		RegisterExpedition(gameDatabase);
		RegisterCostModifiers(gameDatabase);
		RegisterMeta(gameDatabase);
		RegisterBuildingUpgrades(gameDatabase);
	}

	#region Categories

	private void RegisterCategories(GameDb db) {
		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.Attributes,
			name: "Attributes",
			description: "Core character attributes that determine base capabilities."
		) {
			IconName = "icon_category_attributes",
			ColorHint = "#FFD700",
			DisplayOrder = 10,
			DefaultExpanded = true
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.Resource,
			name: "Resources",
			description: "Vital resources like health, mana, and stamina."
		) {
			IconName = "icon_category_resources",
			ColorHint = "#FF4444",
			DisplayOrder = 20,
			DefaultExpanded = true
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.Combat,
			name: "Combat",
			description: "Offensive combat statistics."
		) {
			IconName = "icon_category_combat",
			ColorHint = "#FF6600",
			DisplayOrder = 30
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.Defense,
			name: "Defense",
			description: "Defensive combat statistics."
		) {
			IconName = "icon_category_defense",
			ColorHint = "#4488FF",
			DisplayOrder = 40
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.Resistances,
			name: "Resistances",
			description: "Damage type resistances (percentage reduction)."
		) {
			IconName = "icon_category_resistances",
			ColorHint = "#88FF88",
			DisplayOrder = 50,
			StatsArePercentages = true,
			AllowNegativeStats = true // Vulnerabilities
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.DamageBonuses,
			name: "Damage Bonuses",
			description: "Bonus damage by element type."
		) {
			IconName = "icon_category_damage",
			ColorHint = "#FF4444",
			DisplayOrder = 60,
			StatsArePercentages = true,
			ShowInCharacterSheet = false // Only show if non-zero
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.Movement,
			name: "Movement & Speed",
			description: "Speed-related statistics."
		) {
			IconName = "icon_category_movement",
			ColorHint = "#44FFFF",
			DisplayOrder = 70
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.Regeneration,
			name: "Regeneration",
			description: "Resource regeneration rates."
		) {
			IconName = "icon_category_regen",
			ColorHint = "#44FF44",
			DisplayOrder = 80
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.Economy,
			name: "Economy",
			description: "Loot, experience, and crafting bonuses."
		) {
			IconName = "icon_category_economy",
			ColorHint = "#FFFF44",
			DisplayOrder = 90,
			StatsArePercentages = true
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.Expedition,
			name: "Expedition",
			description: "Exploration and survival statistics."
		) {
			IconName = "icon_category_expedition",
			ColorHint = "#88AA44",
			DisplayOrder = 100
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.CostModifiers,
			name: "Cost Modifiers",
			description: "Ability cost reductions."
		) {
			IconName = "icon_category_cost",
			ColorHint = "#AA88FF",
			DisplayOrder = 110,
			StatsArePercentages = true,
			ShowInCharacterSheet = false
		});

		db.RegisterProto(new StatCategoryProto(
			id: Ids.StatCategories.Meta,
			name: "Character",
			description: "Level, experience, and other meta stats."
		) {
			IconName = "icon_category_meta",
			ColorHint = "#FFFFFF",
			DisplayOrder = 0, // First
			ShowInTooltips = false
		});
	}

	#endregion

	#region Attributes

	private void RegisterAttributes(GameDb db) {
		var cat = Ids.StatCategories.Attributes;

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Attributes.Strength,
			name: "Strength",
			description: "Physical power. Affects melee damage, carry capacity, and athletics.",
			category: cat
		) {
			IconName = "icon_stat_strength",
			ColorHint = "#FF4444",
			Abbreviation = "STR",
			DefaultValue = 10,
			MinValue = 1,
			MaxValue = 30,
			DisplayOrder = 10,
			Tags = [Ids.Tags.Attribute.Strength, Ids.Tags.Meta.Offensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Attributes.Dexterity,
			name: "Dexterity",
			description: "Agility and reflexes. Affects ranged attacks, AC, and finesse weapons.",
			category: cat
		) {
			IconName = "icon_stat_dexterity",
			ColorHint = "#44FF44",
			Abbreviation = "DEX",
			DefaultValue = 10,
			MinValue = 1,
			MaxValue = 30,
			DisplayOrder = 20,
			Tags = [Ids.Tags.Attribute.Dexterity, Ids.Tags.Meta.Offensive, Ids.Tags.Meta.Defensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Attributes.Constitution,
			name: "Constitution",
			description: "Health and endurance. Affects hit points and stamina.",
			category: cat
		) {
			IconName = "icon_stat_constitution",
			ColorHint = "#FFAA44",
			Abbreviation = "CON",
			DefaultValue = 10,
			MinValue = 1,
			MaxValue = 30,
			DisplayOrder = 30,
			Tags = [Ids.Tags.Attribute.Constitution, Ids.Tags.Meta.Defensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Attributes.Intelligence,
			name: "Intelligence",
			description: "Mental acuity. Affects spell power and mana pool.",
			category: cat
		) {
			IconName = "icon_stat_intelligence",
			ColorHint = "#4444FF",
			Abbreviation = "INT",
			DefaultValue = 10,
			MinValue = 1,
			MaxValue = 30,
			DisplayOrder = 40,
			Tags = [Ids.Tags.Attribute.Intelligence, Ids.Tags.Meta.Offensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Attributes.Wisdom,
			name: "Wisdom",
			description: "Perception and willpower. Affects divine magic and awareness.",
			category: cat
		) {
			IconName = "icon_stat_wisdom",
			ColorHint = "#FF44FF",
			Abbreviation = "WIS",
			DefaultValue = 10,
			MinValue = 1,
			MaxValue = 30,
			DisplayOrder = 50,
			Tags = [Ids.Tags.Attribute.Wisdom, Ids.Tags.Meta.Utility]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Attributes.Charisma,
			name: "Charisma",
			description: "Force of personality. Affects social interactions and some magic.",
			category: cat
		) {
			IconName = "icon_stat_charisma",
			ColorHint = "#FFFF44",
			Abbreviation = "CHA",
			DefaultValue = 10,
			MinValue = 1,
			MaxValue = 30,
			DisplayOrder = 60,
			Tags = [Ids.Tags.Attribute.Charisma, Ids.Tags.Meta.Utility]
		});

		db.RegisterProto(new StatCategoryProto(
				id: Ids.StatCategories.BuildingUpgrades,
				name: "Building Upgrades",
				description: "Bonuses from village buildings."
			) {
				IconName = "icon_category_building",
				ColorHint = "#8B4513",
				DisplayOrder = 200,
				ShowInCharacterSheet = false
			});

		db.RegisterProto(new StatCategoryProto(
				id: Ids.StatCategories.Vulnerabilities,
				name: "Vulnerabilities",
				description: "Damage type vulnerabilities (percentage increase)."
			) {
				IconName = "icon_category_vulnerabilities",
				ColorHint = "#FF4444",
				DisplayOrder = 55,
				StatsArePercentages = true,
				AllowNegativeStats = false,
				ShowInCharacterSheet = false
			});


	}

	#endregion

	#region Resources

	private void RegisterResources(GameDb db) {
		var cat = Ids.StatCategories.Resource;

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Resource.MaxHealth,
			name: "Max Health",
			description: "Maximum hit points.",
			category: cat
		) {
			IconName = "icon_stat_health",
			ColorHint = "#FF4444",
			DefaultValue = 10,
			MinValue = 1,
			DisplayOrder = 10,
			DerivedFrom = [
				new StatDerivation(Ids.Stats.Attributes.Constitution, Multiplier: 1f, UseModifier: true)
			]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Resource.CurrentHealth,
			name: "Health",
			description: "Current hit points.",
			category: cat
		) {
			IconName = "icon_stat_health",
			ColorHint = "#FF4444",
			DefaultValue = 10,
			MinValue = 0,
			DisplayOrder = 11
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Resource.MaxMana,
			name: "Max Mana",
			description: "Maximum magical energy.",
			category: cat
		) {
			IconName = "icon_stat_mana",
			ColorHint = "#4444FF",
			DefaultValue = 0,
			MinValue = 0,
			DisplayOrder = 20,
			DerivedFrom = [
				new StatDerivation(Ids.Stats.Attributes.Intelligence, Multiplier: 2f, UseModifier: true)
			]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Resource.CurrentMana,
			name: "Mana",
			description: "Current magical energy.",
			category: cat
		) {
			IconName = "icon_stat_mana",
			ColorHint = "#4444FF",
			DefaultValue = 0,
			MinValue = 0,
			DisplayOrder = 21
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Resource.MaxStamina,
			name: "Max Stamina",
			description: "Maximum physical energy.",
			category: cat
		) {
			IconName = "icon_stat_stamina",
			ColorHint = "#44FF44",
			DefaultValue = 100,
			MinValue = 0,
			DisplayOrder = 30
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Resource.CurrentStamina,
			name: "Stamina",
			description: "Current physical energy.",
			category: cat
		) {
			IconName = "icon_stat_stamina",
			ColorHint = "#44FF44",
			DefaultValue = 100,
			MinValue = 0,
			DisplayOrder = 31
		});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Resource.Fatigue,
				name: "Fatigue",
				description: "Accumulated exhaustion. Reduces effective maximum stamina.",
				category: cat
			) {
				IconName = "icon_stat_fatigue",
				ColorHint = "#888888",
				DefaultValue = 0,
				MinValue = 0,
				MaxValue = 100,
				DisplayOrder = 32,
				HigherIsBetter = false
			});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Resource.MaxFatigue,
				name: "Max Fatigue",
				description: "Maximum fatigue before collapse.",
				category: cat
			) {
				IconName = "icon_stat_fatigue",
				ColorHint = "#888888",
				DefaultValue = 100,
				MinValue = 1,
				DisplayOrder = 33,
				HigherIsBetter = false
			});
	}

	#endregion

	#region Combat

	private void RegisterCombat(GameDb db) {
		var cat = Ids.StatCategories.Combat;

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Combat.InitiativeRoll,
				name: "Initiative Roll",
				description: "Current combat initiative roll result.",
				category: cat
			) {
				IconName = "icon_stat_initiative",
				ColorHint = "#44FFFF",
				DefaultValue = 0,
				DisplayOrder = 41
			});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Combat.AttackBonus,
			name: "Attack Bonus",
			description: "Bonus to attack rolls.",
			category: cat
		) {
			IconName = "icon_stat_attack",
			ColorHint = "#FF6644",
			DefaultValue = 0,
			DisplayOrder = 10,
			DisplayFormat = "+{0}",
			Tags = [Ids.Tags.Meta.Offensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Combat.DamBonus,
			name: "Damage Bonus",
			description: "Flat bonus to all damage dealt.",
			category: cat
		) {
			IconName = "icon_stat_damage",
			ColorHint = "#FF4444",
			DefaultValue = 0,
			DisplayOrder = 20,
			DisplayFormat = "+{0}",
			Tags = [Ids.Tags.Meta.Offensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Combat.SpellPower,
			name: "Spell Power",
			description: "Increases spell damage and healing.",
			category: cat
		) {
			IconName = "icon_stat_spellpower",
			ColorHint = "#AA44FF",
			DefaultValue = 0,
			DisplayOrder = 30,
			Tags = [Ids.Tags.Meta.Offensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Combat.Initiative,
			name: "Initiative",
			description: "Determines turn order in combat.",
			category: cat
		) {
			IconName = "icon_stat_initiative",
			ColorHint = "#44FFFF",
			DefaultValue = 0,
			DisplayOrder = 40,
			DerivedFrom = [
				new StatDerivation(Ids.Stats.Attributes.Dexterity, UseModifier: true)
			]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Combat.CriticalChance,
			name: "Critical Chance",
			description: "Chance to deal a critical hit.",
			category: cat
		) {
			IconName = "icon_stat_crit",
			ColorHint = "#FFFF44",
			DefaultValue = 5,
			MinValue = 0,
			MaxValue = 100,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 50,
			Tags = [Ids.Tags.Meta.Offensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Combat.CriticalDamage,
			name: "Critical Damage",
			description: "Damage multiplier on critical hits.",
			category: cat
		) {
			IconName = "icon_stat_critdmg",
			ColorHint = "#FF8844",
			DefaultValue = 150,
			MinValue = 100,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 60,
			Tags = [Ids.Tags.Meta.Offensive]
		});
	}

	#endregion

	#region Defense

	private void RegisterDefense(GameDb db) {
		var cat = Ids.StatCategories.Defense;

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Combat.ArmorClass,
			name: "Armor Class",
			description: "Difficulty to hit with attacks.",
			category: cat
		) {
			IconName = "icon_stat_armor",
			ColorHint = "#8888FF",
			Abbreviation = "AC",
			DefaultValue = 10,
			MinValue = 0,
			DisplayOrder = 10,
			DerivedFrom = [
				new StatDerivation(Ids.Stats.Attributes.Dexterity, UseModifier: true)
			],
			Tags = [Ids.Tags.Meta.Defensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Combat.BlockChance,
			name: "Block Chance",
			description: "Chance to block incoming attacks with a shield.",
			category: cat
		) {
			IconName = "icon_stat_block",
			ColorHint = "#6688FF",
			DefaultValue = 0,
			MinValue = 0,
			MaxValue = 75,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 20,
			Tags = [Ids.Tags.Meta.Defensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Combat.DodgeChance,
			name: "Dodge Chance",
			description: "Chance to completely avoid attacks.",
			category: cat
		) {
			IconName = "icon_stat_dodge",
			ColorHint = "#44FFAA",
			DefaultValue = 0,
			MinValue = 0,
			MaxValue = 50,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 30,
			Tags = [Ids.Tags.Meta.Defensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Combat.ParryChance,
			name: "Parry Chance",
			description: "Chance to deflect melee attacks.",
			category: cat
		) {
			IconName = "icon_stat_parry",
			ColorHint = "#AAFFAA",
			DefaultValue = 0,
			MinValue = 0,
			MaxValue = 50,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 40,
			Tags = [Ids.Tags.Meta.Defensive]
		});
	}

	#endregion

	#region Resistances

	private void RegisterResistances(GameDb db) {
		var cat = Ids.StatCategories.Resistances;

		RegisterResistance(db, Ids.Stats.Resistances.Physical, "Physical", "Reduces physical damage taken.", "#AAAAAA", 10);
		RegisterResistance(db, Ids.Stats.Resistances.Magical, "Magical", "Reduces all magical damage taken.", "#AA44FF", 20);
		RegisterResistance(db, Ids.Stats.Resistances.Fire, "Fire", "Reduces fire damage taken.", "#FF4400", 30, Ids.Tags.Element.Fire);
		RegisterResistance(db, Ids.Stats.Resistances.Cold, "Cold", "Reduces cold damage taken.", "#44AAFF", 40, Ids.Tags.Element.Cold);
		RegisterResistance(db, Ids.Stats.Resistances.Lightning, "Lightning", "Reduces lightning damage taken.", "#FFFF00", 50, Ids.Tags.Element.Lightning);
		RegisterResistance(db, Ids.Stats.Resistances.Poison, "Poison", "Reduces poison damage taken.", "#44FF44", 60, Ids.Tags.Element.Poison);
		RegisterResistance(db, Ids.Stats.Resistances.Acid, "Acid", "Reduces acid damage taken.", "#88FF00", 70, Ids.Tags.Element.Acid);
		RegisterResistance(db, Ids.Stats.Resistances.Holy, "Holy", "Reduces holy damage taken.", "#FFFFAA", 80, Ids.Tags.Element.Holy);
		RegisterResistance(db, Ids.Stats.Resistances.Necrotic, "Necrotic", "Reduces necrotic damage taken.", "#440044", 90, Ids.Tags.Element.Necrotic);
		RegisterResistance(db, Ids.Stats.Resistances.Psychic, "Psychic", "Reduces psychic damage taken.", "#FF44FF", 100, Ids.Tags.Element.Psychic);
		RegisterResistance(db, Ids.Stats.Resistances.Force, "Force", "Reduces force damage taken.", "#4444FF", 110, Ids.Tags.Element.Force);
	}

	private void RegisterResistance(GameDb db, StatProto.ID id, string name, string desc, string color, int order, TagProto.ID? elementTag = null) {
		var tags = new List<TagProto.ID> { Ids.Tags.Meta.Defensive, Ids.Tags.Resistance.Physical };
		if (elementTag != null) tags.Add(elementTag.Value);

		db.RegisterProto(new StatProto(id, $"{name} Resistance", desc, Ids.StatCategories.Resistances) {
			IconName = $"icon_resist_{name.ToLower()}",
			ColorHint = color,
			DefaultValue = 0,
			MinValue = -100,
			MaxValue = 100,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = order,
			Tags = tags
		});
	}

	#endregion

	#region Vulnerabilities

	private void RegisterVulnerabilities(GameDb db) {
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Physical, "Physical", "#AAAAAA", 10);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Magical, "Magical", "#AA44FF", 20);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Fire, "Fire", "#FF4400", 30, Ids.Tags.Element.Fire);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Cold, "Cold", "#44AAFF", 40, Ids.Tags.Element.Cold);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Lightning, "Lightning", "#FFFF00", 50, Ids.Tags.Element.Lightning);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Poison, "Poison", "#44FF44", 60, Ids.Tags.Element.Poison);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Acid, "Acid", "#88FF00", 70, Ids.Tags.Element.Acid);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Holy, "Holy", "#FFFFAA", 80, Ids.Tags.Element.Holy);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Necrotic, "Necrotic", "#440044", 90, Ids.Tags.Element.Necrotic);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Psychic, "Psychic", "#FF44FF", 100, Ids.Tags.Element.Psychic);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Force, "Force", "#4444FF", 110, Ids.Tags.Element.Force);
		RegisterVulnerability(db, Ids.Stats.Vulnerabilities.Silver, "Silver", "#C0C0C0", 120);
	}

	private void RegisterVulnerability(GameDb db, StatProto.ID id, string name, string color, int order, TagProto.ID? elementTag = null) {
		var tags = new List<TagProto.ID>();
		if (elementTag != null) tags.Add(elementTag.Value);

		db.RegisterProto(new StatProto(id, $"{name} Vulnerability", $"Extra {name.ToLower()} damage taken.", Ids.StatCategories.Vulnerabilities) {
			IconName = $"icon_vuln_{name.ToLower()}",
			ColorHint = color,
			DefaultValue = 0,
			MinValue = 0,
			MaxValue = 200,
			DisplayAsPercent = true,
			DisplayFormat = "+{0}%",
			DisplayOrder = order,
			HigherIsBetter = false,
			Tags = tags
		});
	}

	#endregion

	#region Damage Bonuses

	private void RegisterDamageBonuses(GameDb db) {
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Physical, "Physical", "#AAAAAA", 10);
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Fire, "Fire", "#FF4400", 20, Ids.Tags.Element.Fire);
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Cold, "Cold", "#44AAFF", 30, Ids.Tags.Element.Cold);
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Lightning, "Lightning", "#FFFF00", 40, Ids.Tags.Element.Lightning);
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Poison, "Poison", "#44FF44", 50, Ids.Tags.Element.Poison);
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Acid, "Acid", "#88FF00", 60, Ids.Tags.Element.Acid);
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Holy, "Holy", "#FFFFAA", 70, Ids.Tags.Element.Holy);
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Necrotic, "Necrotic", "#440044", 80, Ids.Tags.Element.Necrotic);
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Psychic, "Psychic", "#FF44FF", 90, Ids.Tags.Element.Psychic);
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Force, "Force", "#4444FF", 100, Ids.Tags.Element.Force);
		RegisterDamageBonus(db, Ids.Stats.DamageBonus.Arcane, "Arcane", "#AA44FF", 110, Ids.Tags.Element.Arcane);
	}

	private void RegisterDamageBonus(GameDb db, StatProto.ID id, string name, string color, int order, TagProto.ID? elementTag = null) {
		var tags = new List<TagProto.ID> { Ids.Tags.Meta.Offensive };
		if (elementTag != null) tags.Add(elementTag.Value);

		db.RegisterProto(new StatProto(id, $"{name} Damage", $"Bonus {name.ToLower()} damage dealt.", Ids.StatCategories.DamageBonuses) {
			IconName = $"icon_dmg_{name.ToLower()}",
			ColorHint = color,
			DefaultValue = 0,
			DisplayAsPercent = true,
			DisplayFormat = "+{0}%",
			DisplayOrder = order,
			Tags = tags
		});
	}

	#endregion

	#region Movement

	private void RegisterMovement(GameDb db) {
		var cat = Ids.StatCategories.Movement;

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Movement.MovementSpeed,
			name: "Movement Speed",
			description: "How fast you move.",
			category: cat
		) {
			IconName = "icon_stat_movespeed",
			ColorHint = "#44FFFF",
			DefaultValue = 100,
			MinValue = 10,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 10
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Movement.AttackSpeed,
			name: "Attack Speed",
			description: "How fast you attack.",
			category: cat
		) {
			IconName = "icon_stat_attackspeed",
			ColorHint = "#FF8844",
			DefaultValue = 100,
			MinValue = 10,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 20,
			Tags = [Ids.Tags.Meta.Offensive]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Movement.CastSpeed,
			name: "Cast Speed",
			description: "How fast you cast spells.",
			category: cat
		) {
			IconName = "icon_stat_castspeed",
			ColorHint = "#AA44FF",
			DefaultValue = 100,
			MinValue = 10,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 30,
			Tags = [Ids.Tags.Meta.Offensive]
		});
	}

	#endregion

	#region Regeneration

	private void RegisterRegeneration(GameDb db) {
		var cat = Ids.StatCategories.Regeneration;

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Regeneration.Health,
			name: "Health Regen",
			description: "Health restored per tick.",
			category: cat
		) {
			IconName = "icon_stat_healthregen",
			ColorHint = "#FF4444",
			DefaultValue = 0,
			DisplayOrder = 10
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Regeneration.Mana,
			name: "Mana Regen",
			description: "Mana restored per tick.",
			category: cat
		) {
			IconName = "icon_stat_manaregen",
			ColorHint = "#4444FF",
			DefaultValue = 0,
			DisplayOrder = 20
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Regeneration.Stamina,
			name: "Stamina Regen",
			description: "Stamina restored per tick.",
			category: cat
		) {
			IconName = "icon_stat_staminaregen",
			ColorHint = "#44FF44",
			DefaultValue = 1,
			DisplayOrder = 30
		});
	}

	#endregion

	#region Economy

	private void RegisterEconomy(GameDb db) {
		var cat = Ids.StatCategories.Economy;

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Economy.GoldFind,
			name: "Gold Find",
			description: "Bonus gold from all sources.",
			category: cat
		) {
			IconName = "icon_stat_gold",
			ColorHint = "#FFD700",
			DefaultValue = 0,
			DisplayAsPercent = true,
			DisplayFormat = "+{0}%",
			DisplayOrder = 10
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Economy.ItemFind,
			name: "Item Find",
			description: "Bonus item drop chance.",
			category: cat
		) {
			IconName = "icon_stat_itemfind",
			ColorHint = "#44FF44",
			DefaultValue = 0,
			DisplayAsPercent = true,
			DisplayFormat = "+{0}%",
			DisplayOrder = 20
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Economy.ExperienceGain,
			name: "Experience Gain",
			description: "Bonus experience from all sources.",
			category: cat
		) {
			IconName = "icon_stat_exp",
			ColorHint = "#AA44FF",
			DefaultValue = 0,
			DisplayAsPercent = true,
			DisplayFormat = "+{0}%",
			DisplayOrder = 30
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Economy.CraftingSpeed,
			name: "Crafting Speed",
			description: "How fast crafting completes.",
			category: cat
		) {
			IconName = "icon_stat_crafting",
			ColorHint = "#FF8844",
			DefaultValue = 100,
			MinValue = 10,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 40
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Economy.GatheringSpeed,
			name: "Gathering Speed",
			description: "How fast gathering completes.",
			category: cat
		) {
			IconName = "icon_stat_gathering",
			ColorHint = "#88AA44",
			DefaultValue = 100,
			MinValue = 10,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 50
		});
	}

	#endregion

	#region Expedition

	private void RegisterExpedition(GameDb db) {
		var cat = Ids.StatCategories.Expedition;

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Expedition.FoodConsumption,
			name: "Food Consumption",
			description: "Rate of food consumption during expeditions.",
			category: cat
		) {
			IconName = "icon_stat_food",
			ColorHint = "#FFAA44",
			DefaultValue = 100.Percent(),
			MinValue = 0,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 10,
			HigherIsBetter = false
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Expedition.FatigueRate,
			name: "Fatigue Rate",
			description: "How quickly you gain fatigue.",
			category: cat
		) {
			IconName = "icon_stat_fatigue",
			ColorHint = "#888888",
			DefaultValue = 100,
			MinValue = 0,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 20,
			HigherIsBetter = false
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Expedition.TravelSpeed,
			name: "Travel Speed",
			description: "How fast you travel on the map.",
			category: cat
		) {
			IconName = "icon_stat_travel",
			ColorHint = "#44FFFF",
			DefaultValue = 100,
			MinValue = 10,
			DisplayAsPercent = true,
			DisplayFormat = "{0}%",
			DisplayOrder = 30
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Expedition.CarryCapacity,
			name: "Carry Capacity",
			description: "Maximum weight you can carry.",
			category: cat
		) {
			IconName = "icon_stat_carry",
			ColorHint = "#AAAAAA",
			DefaultValue = 100,
			MinValue = 10,
			DisplayOrder = 40,
			DerivedFrom = [
				new StatDerivation(Ids.Stats.Attributes.Strength, Multiplier: 15f)
			]
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Expedition.VisionRange,
			name: "Vision Range",
			description: "How far you can see on the map.",
			category: cat
		) {
			IconName = "icon_stat_vision",
			ColorHint = "#FFFFAA",
			DefaultValue = 3,
			MinValue = 1,
			DisplayOrder = 50
		});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Expedition.GoldOnHand,
				name: "Gold",
				description: "Currency for purchases.",
				category: cat
			) {
				IconName = "icon_stat_gold",
				ColorHint = "#FFD700",
				DefaultValue = 0,
				MinValue = 0,
				DisplayOrder = 60
			});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Expedition.FoodOnHand,
				name: "Food",
				description: "Days of food remaining.",
				category: cat
			) {
				IconName = "icon_stat_food",
				ColorHint = "#FFAA44",
				DefaultValue = 3,
				MinValue = 0,
				DisplayOrder = 70
			});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Expedition.MedicalSupplies,
				name: "Medical Supplies",
				description: "Bandages and healing items.",
				category: cat
			) {
				IconName = "icon_stat_medical",
				ColorHint = "#FF4444",
				DefaultValue = 2,
				MinValue = 0,
				DisplayOrder = 80
			});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Expedition.CampingSupplies,
				name: "Camping Supplies",
				description: "Tents, bedrolls, fire starters.",
				category: cat
			) {
				IconName = "icon_stat_camping",
				ColorHint = "#88AA44",
				DefaultValue = 1,
				MinValue = 0,
				DisplayOrder = 90
			});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Expedition.Torches,
				name: "Torches",
				description: "Light sources for caves and night.",
				category: cat
			) {
				IconName = "icon_stat_torches",
				ColorHint = "#FF8800",
				DefaultValue = 3,
				MinValue = 0,
				DisplayOrder = 100
			});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Expedition.Water,
				name: "Water",
				description: "Water supply for desert and hot biomes.",
				category: cat
			) {
				IconName = "icon_stat_water",
				ColorHint = "#44AAFF",
				DefaultValue = 5,
				MinValue = 0,
				DisplayOrder = 110
			});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Resource.Morale,
				name: "Morale",
				description: "Party morale (0-100).",
				category: cat
			) {
				IconName = "icon_stat_morale",
				ColorHint = "#44FF44",
				DefaultValue = 100,
				MinValue = 0,
				MaxValue = 100,
				DisplayOrder = 120
			});
	}



	#endregion

	#region Cost Modifiers

	private void RegisterCostModifiers(GameDb db) {
		var cat = Ids.StatCategories.CostModifiers;

		db.RegisterProto(new StatProto(
			id: Ids.Stats.CostMod.ManaCost,
			name: "Mana Cost Reduction",
			description: "Reduces mana cost of spells.",
			category: cat
		) {
			IconName = "icon_stat_manacost",
			ColorHint = "#4444FF",
			DefaultValue = 0,
			MaxValue = 75,
			DisplayAsPercent = true,
			DisplayFormat = "-{0}%",
			DisplayOrder = 10
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.CostMod.StaminaCost,
			name: "Stamina Cost Reduction",
			description: "Reduces stamina cost of abilities.",
			category: cat
		) {
			IconName = "icon_stat_staminacost",
			ColorHint = "#44FF44",
			DefaultValue = 0,
			MaxValue = 75,
			DisplayAsPercent = true,
			DisplayFormat = "-{0}%",
			DisplayOrder = 20
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.CostMod.Cooldown,
			name: "Cooldown Reduction",
			description: "Reduces ability cooldowns.",
			category: cat
		) {
			IconName = "icon_stat_cooldown",
			ColorHint = "#FFAA44",
			DefaultValue = 0,
			MaxValue = 75,
			DisplayAsPercent = true,
			DisplayFormat = "-{0}%",
			DisplayOrder = 30
		});
	}

	#endregion

	#region Meta

	private void RegisterMeta(GameDb db) {
		var cat = Ids.StatCategories.Meta;

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Meta.GoldAwarded,
				name: "Gold Awarded",
				description: "Gold rewarded by this entity.",
				category: cat
			) {
				IconName = "icon_stat_gold",
				ColorHint = "#FFD700",
				DefaultValue = 0,
				MinValue = 0,
				DisplayOrder = 25
			});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Meta.ExperienceAwarded,
				name: "Experience Awarded",
				description: "Experience rewarded by this entity.",
				category: cat
			) {
				IconName = "icon_stat_exp",
				ColorHint = "#AA44FF",
				DefaultValue = 0,
				MinValue = 0,
				DisplayOrder = 22
			});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Meta.ExperienceToNextLevel,
				name: "XP to Level",
				description: "Experience needed for next level.",
				category: cat
			) {
				IconName = "icon_stat_exp",
				ColorHint = "#AA44FF",
				DefaultValue = 100,
				MinValue = 0,
				DisplayOrder = 21
			});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Meta.Level,
			name: "Level",
			description: "Character level.",
			category: cat
		) {
			IconName = "icon_stat_level",
			ColorHint = "#FFD700",
			DefaultValue = 1,
			MinValue = 1,
			DisplayOrder = 10
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Meta.Experience,
			name: "Experience",
			description: "Experience points.",
			category: cat
		) {
			IconName = "icon_stat_exp",
			ColorHint = "#AA44FF",
			DefaultValue = 0,
			MinValue = 0,
			DisplayOrder = 20
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Meta.Luck,
			name: "Luck",
			description: "Affects random outcomes.",
			category: cat
		) {
			IconName = "icon_stat_luck",
			ColorHint = "#44FF44",
			DefaultValue = 0,
			DisplayOrder = 30
		});

		db.RegisterProto(new StatProto(
			id: Ids.Stats.Meta.Size,
			name: "Size",
			description: "Physical size category.",
			category: cat
		) {
			IconName = "icon_stat_size",
			ColorHint = "#AAAAAA",
			DefaultValue = 1, // Medium
			MinValue = 0, // Tiny
			MaxValue = 4, // Gargantuan
			DisplayOrder = 40
		});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Meta.CreationPointsPerLevel,
				name: "Creation Points Per Level",
				description: "Points gained per level for attributes, skills, and spells.",
				category: cat
			) {
				IconName = "icon_stat_points",
				ColorHint = "#44AAFF",
				DefaultValue = 10,
				MinValue = 0,
				DisplayOrder = 50
			});

		db.RegisterProto(new StatProto(
				id: Ids.Stats.Meta.CreationPointsUnspent,
				name: "Unspent Creation Points",
				description: "Creation points saved from previous level-ups.",
				category: cat
			) {
				IconName = "icon_stat_points",
				ColorHint = "#44AAFF",
				DefaultValue = 0,
				MinValue = 0,
				DisplayOrder = 51
			});
	}

	#endregion

	#region Building Upgrades

private void RegisterBuildingUpgrades(GameDb db) {
	var cat = Ids.StatCategories.BuildingUpgrades;

	// Starting Resources
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.StartingGold, "Starting Gold", "Bonus gold at run start.", 10);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.StartingFood, "Starting Food", "Bonus food at run start.", 20);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.StartingMedical, "Starting Medical", "Bonus medical supplies at run start.", 30);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.StartingCamping, "Starting Camping", "Bonus camping supplies at run start.", 40);

	// Morale & Rest
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.MaxMorale, "Max Morale", "Maximum morale bonus.", 50);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.RestEffectiveness, "Rest Effectiveness", "Bonus to rest healing.", 60, isPercent: true);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.FreeHealing, "Free Healing", "Free healing per visit.", 70);

	// Construction
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.BuildingCostReduction, "Building Cost Reduction", "Reduces building costs.", 80, isPercent: true);

	// Consumables
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.PotionEffectiveness, "Potion Effectiveness", "Increases potion effects.", 90, isPercent: true);

	// Exploration
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.FogRevealRadius, "Fog Reveal Radius", "Bonus fog reveal distance.", 100);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.AmbushResistance, "Ambush Resistance", "Reduces ambush chance.", 110, isPercent: true);

	// Hunting & Food
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.HuntingBonus, "Hunting Bonus", "Bonus to hunting yields.", 120, isPercent: true);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.FoodFromCombat, "Food From Combat", "Chance to find food after combat.", 130, isPercent: true);

	// Divine
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.BlessingDuration, "Blessing Duration", "Extends blessing durations.", 140, isPercent: true);

	// Crafting
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.CraftingQuality, "Crafting Quality", "Bonus to crafted item quality.", 150, isPercent: true);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.SalvageBonus, "Salvage Bonus", "Bonus materials from salvaging.", 160, isPercent: true);

	// Meta Progression
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.AchievementBonus, "Achievement Bonus", "Bonus to achievement rewards.", 170, isPercent: true);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.DeathBonus, "Death Bonus", "Bonus resources on death.", 180, isPercent: true);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.LegacyExperience, "Legacy Experience", "Bonus legacy XP gain.", 190, isPercent: true);

	// Combat Rewards
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.CombatGold, "Combat Gold", "Bonus gold from combat.", 200, isPercent: true);

	// Discovery
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.EventPreview, "Event Preview", "See event outcomes ahead.", 210);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.LoreDiscovery, "Lore Discovery", "Bonus to lore discovery.", 220, isPercent: true);

	// Portal
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.PortalCharges, "Portal Charges", "Number of portal uses.", 230);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.PortalRange, "Portal Range", "Maximum portal distance.", 240);

	// Personal
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.PersonalStorage, "Personal Storage", "Bonus storage slots.", 250);

	// Party
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.PartySize, "Party Size", "Maximum party members.", 260);
	RegisterBuildingStat(db, Ids.Stats.BuildingUpgrades.CompanionExperience, "Companion Experience", "Bonus companion XP.", 270, isPercent: true);
}

private void RegisterBuildingStat(GameDb db, StatProto.ID id, string name, string desc, int order, bool isPercent = false) {
	db.RegisterProto(new StatProto(id, name, desc, Ids.StatCategories.BuildingUpgrades) {
		IconName = "icon_stat_building",
		ColorHint = "#8B4513",
		DefaultValue = 0,
		DisplayAsPercent = isPercent,
		DisplayFormat = isPercent ? "+{0}%" : "+{0}",
		DisplayOrder = order
	});
}

#endregion


}