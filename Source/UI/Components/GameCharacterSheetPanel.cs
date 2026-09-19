using System;
using System.Collections.Generic;
using System.Linq;
using RPGGame.Core;
using RPGGame.Core.Characters;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Spells;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Displays character stats, skills, and spells organized by category.
/// Dynamically populates from GameDb prototypes.
/// </summary>
public class GameCharacterSheetPanel : VisualElement {
	#region Private Fields

	private readonly GameDb k_gameDb;
	private readonly VisualElement k_container;
	private readonly VisualElement k_tabBar;
	private readonly VisualElement k_contentArea;

	private LiveCharacter? k_character;
	private CharacterSheetTab k_activeTab = CharacterSheetTab.Attributes;

	// Tab content containers
	private readonly Dictionary<CharacterSheetTab, VisualElement> k_tabContents = new();

	// Callbacks
	private Action<StatProto>? k_onStatClicked;
	private Action<SkillProto>? k_onSkillClicked;
	private Action<SpellProto>? k_onSpellClicked;

	#endregion

	#region Constructor

	public GameCharacterSheetPanel(GameDb gameDb) {
		k_gameDb = gameDb;

		k_container = new VisualElement {
			name = "character-sheet",
			style = {
				flexDirection = FlexDirection.Column,
				flexGrow = 1
			}
		};

		// Tab bar
		k_tabBar = new VisualElement {
			name = "tab-bar",
			style = {
				flexDirection = FlexDirection.Row,
				marginBottom = 8
			}
		};

		// Content area
		k_contentArea = new VisualElement {
			name = "content-area",
			style = {
				flexGrow = 1
			}
		};

		k_container.Add(k_tabBar);
		k_container.Add(k_contentArea);
		Add(k_container);

		BuildTabs();
		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API

	/// <summary>
	/// Sets the character to display.
	/// </summary>
	public GameCharacterSheetPanel SetCharacter(LiveCharacter character) {
		k_character = character;
		Refresh();
		return this;
	}

	/// <summary>
	/// Called when a stat is clicked.
	/// </summary>
	public GameCharacterSheetPanel OnStatClicked(Action<StatProto> callback) {
		k_onStatClicked = callback;
		return this;
	}

	/// <summary>
	/// Called when a skill is clicked.
	/// </summary>
	public GameCharacterSheetPanel OnSkillClicked(Action<SkillProto> callback) {
		k_onSkillClicked = callback;
		return this;
	}

	/// <summary>
	/// Called when a spell is clicked.
	/// </summary>
	public GameCharacterSheetPanel OnSpellClicked(Action<SpellProto> callback) {
		k_onSpellClicked = callback;
		return this;
	}

	/// <summary>
	/// Builds the panel.
	/// </summary>
	public GameCharacterSheetPanel Build() {
		ApplyTheme();
		Refresh();
		return this;
	}

	#endregion

	#region Tab Building

	private void BuildTabs() {
		k_tabBar.Clear();

		var tabs = new[] {
			(CharacterSheetTab.Attributes, "Attributes"),
			(CharacterSheetTab.Combat, "Combat"),
			(CharacterSheetTab.Defense, "Defense"),
			(CharacterSheetTab.Skills, "Skills"),
			(CharacterSheetTab.Spells, "Spells"),
			(CharacterSheetTab.Resistances, "Resistances")
		};

		foreach (var (tab, label) in tabs) {
			var tabButton = new GameButton(label)
				.SetVariant(k_activeTab == tab ? ButtonVariant.Primary : ButtonVariant.Ghost)
				.SetSize(ButtonSize.Small)
				.OnClick(() => SelectTab(tab))
				.Build();

			tabButton.style.marginRight = 4;
			k_tabBar.Add(tabButton);
		}
	}

	private void SelectTab(CharacterSheetTab tab) {
		k_activeTab = tab;
		BuildTabs();
		RefreshContent();
	}

	#endregion

	#region Content Building

	private void Refresh() {
		if (k_character == null) return;
		RefreshContent();
	}

	private void RefreshContent() {
		k_contentArea.Clear();

		if (k_character == null) {
			k_contentArea.Add(new GameLabel("No character selected")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.Build());
			return;
		}

		switch (k_activeTab) {
			case CharacterSheetTab.Attributes:
				BuildAttributesTab();
				break;
			case CharacterSheetTab.Combat:
				BuildCombatTab();
				break;
			case CharacterSheetTab.Defense:
				BuildDefenseTab();
				break;
			case CharacterSheetTab.Skills:
				BuildSkillsTab();
				break;
			case CharacterSheetTab.Spells:
				BuildSpellsTab();
				break;
			case CharacterSheetTab.Resistances:
				BuildResistancesTab();
				break;
		}
	}

	private void BuildAttributesTab() {
		var stats = k_gameDb.GetAll<StatProto>()
			.Where(s => s.Category == Ids.StatCategories.Attributes)
			.OrderBy(s => s.DisplayOrder)
			.ToList();

		BuildStatSection("Primary Attributes", stats);

		// Also show resources
		var resources = k_gameDb.GetAll<StatProto>()
			.Where(s => s.Category == Ids.StatCategories.Resource)
			.OrderBy(s => s.DisplayOrder)
			.ToList();

		BuildStatSection("Resources", resources);
	}

	private void BuildCombatTab() {
		var combatStats = k_gameDb.GetAll<StatProto>()
			.Where(s => s.Category == Ids.StatCategories.Combat)
			.OrderBy(s => s.DisplayOrder)
			.ToList();

		BuildStatSection("Combat Statistics", combatStats);

		var speedStats = k_gameDb.GetAll<StatProto>()
			.Where(s => s.Category == Ids.StatCategories.Movement)
			.OrderBy(s => s.DisplayOrder)
			.ToList();

		BuildStatSection("Speed & Movement", speedStats);
	}

	private void BuildDefenseTab() {
		var defenseStats = k_gameDb.GetAll<StatProto>()
			.Where(s => s.Category == Ids.StatCategories.Defense)
			.OrderBy(s => s.DisplayOrder)
			.ToList();

		BuildStatSection("Defense Statistics", defenseStats);
	}

	private void BuildSkillsTab() {
		// Group skills by their meta category tag
		var allSkills = k_gameDb.GetAll<SkillProto>().ToList();

		var skillGroups = new Dictionary<string, List<SkillProto>> {
			["Combat"] = allSkills.Where(s => s.HasTag(Ids.Tags.Meta.CombatSkill)).ToList(),
			["Magic"] = allSkills.Where(s => s.HasTag(Ids.Tags.Meta.MagicSkill)).ToList(),
			["Physical"] = allSkills.Where(s => s.HasTag(Ids.Tags.Meta.PhysicalSkill)).ToList(),
			["Stealth"] = allSkills.Where(s => s.HasTag(Ids.Tags.Meta.StealthSkill)).ToList(),
			["Survival"] = allSkills.Where(s => s.HasTag(Ids.Tags.Meta.SurvivalSkill)).ToList(),
			["Crafting"] = allSkills.Where(s => s.HasTag(Ids.Tags.Meta.CraftingSkill)).ToList(),
			["Social"] = allSkills.Where(s => s.HasTag(Ids.Tags.Meta.SocialSkill)).ToList(),
			["Knowledge"] = allSkills.Where(s => s.HasTag(Ids.Tags.Meta.KnowledgeSkill)).ToList()
		};

		var scrollView = new ScrollView(ScrollViewMode.Vertical) {
			style = { flexGrow = 1 }
		};

		foreach (var (categoryName, skills) in skillGroups) {
			if (skills.Count == 0) continue;

			var section = BuildSkillSection(categoryName, skills.OrderBy(s => s.DisplayText.Name).ToList());
			scrollView.Add(section);
		}

		k_contentArea.Add(scrollView);
	}

	private void BuildSpellsTab() {
		if (k_character == null) return;

		// Group spells by school
		var knownSpells = k_character.KnownSpells
			.Select(id => k_gameDb.GetOrNull<SpellProto>(id))
			.Where(s => s != null)
			.Cast<SpellProto>()
			.ToList();

		if (knownSpells.Count == 0) {
			k_contentArea.Add(new GameLabel("No spells known")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.Build());
			return;
		}

		var spellsBySchool = knownSpells
			.GroupBy(s => s.School)
			.OrderBy(g => g.Key.ToString())
			.ToDictionary(g => g.Key, g => g.OrderBy(s => s.Level).ThenBy(s => s.DisplayText.Name).ToList());

		var scrollView = new ScrollView(ScrollViewMode.Vertical) {
			style = { flexGrow = 1 }
		};

		foreach (var (school, spells) in spellsBySchool) {
			var section = BuildSpellSection(school.GetDisplayName(), spells);
			scrollView.Add(section);
		}

		k_contentArea.Add(scrollView);
	}

	private void BuildResistancesTab() {
		var resistances = k_gameDb.GetAll<StatProto>()
			.Where(s => s.Category == Ids.StatCategories.Resistances)
			.OrderBy(s => s.DisplayOrder)
			.ToList();

		BuildStatSection("Damage Resistances", resistances);

		var vulnerabilities = k_gameDb.GetAll<StatProto>()
			.Where(s => s.Category == Ids.StatCategories.Vulnerabilities)
			.Where(s => k_character != null && k_character.GetStat(s.Id) > 0)
			.OrderBy(s => s.DisplayOrder)
			.ToList();

		if (vulnerabilities.Count > 0) {
			BuildStatSection("Vulnerabilities", vulnerabilities);
		}
	}

	#endregion

	#region Section Builders

	private void BuildStatSection(string title, List<StatProto> stats) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var section = new GameContainer($"section-{title.ToLower().Replace(" ", "-")}")
			.SetColumn()
			.SetMarginBottom(spacing.LG)
			.Build();

		var header = new GameLabel(title)
			.SetStyle(LabelStyle.TitleMedium)
			.SetMarginBottom(spacing.SM)
			.Build();

		section.Add(header);

		foreach (var stat in stats) {
			var row = CreateStatRow(stat);
			section.Add(row);
		}

		k_contentArea.Add(section);
	}

	private VisualElement CreateStatRow(StatProto stat) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		float value = k_character?.GetStat(stat.Id) ?? stat.DefaultValue;
		string formattedValue = stat.FormatValue(value);

		var row = new GameContainer($"stat-{stat.Id.Value}")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.XS, spacing.SM, spacing.XS, spacing.SM)
			.OnClick(() => k_onStatClicked?.Invoke(stat))
			.OnHoverBackground(c => c.SurfaceHover)
			.Build();

		// Left: name with abbreviation
		var nameContainer = new GameContainer("name")
			.SetRow()
			.SetAlignItems(Align.Center)
			.Build();

		if (!string.IsNullOrEmpty(stat.Abbreviation)) {
			var abbrevLabel = new GameLabel(stat.Abbreviation)
				.SetStyle(LabelStyle.LabelLarge)
				.SetColor(LabelColor.Primary)
				.SetWidth(50)
				.Build();
			nameContainer.Add(abbrevLabel);
		}

		var nameLabel = new GameLabel(stat.DisplayText.Name)
			.SetStyle(LabelStyle.BodyMedium)
			.Build();
		nameContainer.Add(nameLabel);

		// Right: value
		var valueLabel = new GameLabel(formattedValue)
			.SetStyle(LabelStyle.LabelLarge)
			.Build();

		// Color based on value
		if (stat.HigherIsBetter) {
			if (value > stat.DefaultValue) {
				valueLabel.SetColor(colors.Success);
			} else if (value < stat.DefaultValue) {
				valueLabel.SetColor(colors.Error);
			}
		} else {
			if (value < stat.DefaultValue) {
				valueLabel.SetColor(colors.Success);
			} else if (value > stat.DefaultValue) {
				valueLabel.SetColor(colors.Error);
			}
		}

		row.AddChild(nameContainer);
		row.AddChild(valueLabel);

		return row;
	}

	private VisualElement BuildSkillSection(string categoryName, List<SkillProto> skills) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var section = new GameContainer($"skill-section-{categoryName.ToLower()}")
			.SetColumn()
			.SetMarginBottom(spacing.MD)
			.Build();

		var header = new GameLabel(categoryName)
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Secondary)
			.SetMarginBottom(spacing.XS)
			.Build();

		section.Add(header);

		foreach (var skill in skills) {
			var row = CreateSkillRow(skill);
			section.Add(row);
		}

		return section;
	}

	private VisualElement CreateSkillRow(SkillProto skill) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		int rank = k_character?.GetSkillRank(skill.Id) ?? 0;
		bool hasSkill = rank > 0;

		var row = new GameContainer($"skill-{skill.Id.Value}")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.XS, spacing.SM, spacing.XS, spacing.SM)
			.OnClick(() => k_onSkillClicked?.Invoke(skill))
			.OnHoverBackground(c => c.SurfaceHover)
			.Build();

		var nameLabel = new GameLabel(skill.DisplayText.Name)
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(hasSkill ? LabelColor.Primary : LabelColor.Tertiary)
			.Build();

		var rankLabel = new GameLabel(hasSkill ? $"Rank {rank}/{skill.MaxRank}" : "—")
			.SetStyle(LabelStyle.Caption)
			.SetColor(hasSkill ? LabelColor.Success : LabelColor.Tertiary)
			.Build();

		row.AddChild(nameLabel);
		row.AddChild(rankLabel);

		return row;
	}

	private VisualElement BuildSpellSection(string schoolName, List<SpellProto> spells) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var section = new GameContainer($"spell-section-{schoolName.ToLower()}")
			.SetColumn()
			.SetMarginBottom(spacing.MD)
			.Build();

		var header = new GameLabel(schoolName)
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Secondary)
			.SetMarginBottom(spacing.XS)
			.Build();

		section.Add(header);

		foreach (var spell in spells) {
			var row = CreateSpellRow(spell);
			section.Add(row);
		}

		return section;
	}

	private VisualElement CreateSpellRow(SpellProto spell) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		var row = new GameContainer($"spell-{spell.Id.Value}")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.XS, spacing.SM, spacing.XS, spacing.SM)
			.OnClick(() => k_onSpellClicked?.Invoke(spell))
			.OnHoverBackground(c => c.SurfaceHover)
			.Build();

		// Left: level badge + name
		var leftContent = new GameContainer("left")
			.SetRow()
			.SetAlignItems(Align.Center)
			.Build();

		var levelBadge = new GameBadge()
			.SetText(spell.Level == 0 ? "C" : spell.Level.ToString())
			.SetVariant(spell.Level == 0 ? BadgeVariant.Info : BadgeVariant.Default)
			.SetSize(BadgeSize.Small)
			.Build();
		levelBadge.style.marginRight = spacing.SM;

		var nameLabel = new GameLabel(spell.DisplayText.Name)
			.SetStyle(LabelStyle.BodyMedium)
			.Build();

		leftContent.AddChild(levelBadge);
		leftContent.AddChild(nameLabel);

		// Right: mana cost
		var costLabel = new GameLabel(spell.ManaCost > 0 ? $"{spell.ManaCost} MP" : "Free")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Info)
			.Build();

		row.AddChild(leftContent);
		row.AddChild(costLabel);

		return row;
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;

		k_container.style.backgroundColor = colors.Surface;
		borders.ApplyRadius(k_container.style, borders.RadiusMD);
		theme.Spacing.PanelPadding.ApplyTo(k_container.style);
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
		Refresh();
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

/// <summary>
/// Tabs in the character sheet.
/// </summary>
public enum CharacterSheetTab {
	Attributes,
	Combat,
	Defense,
	Skills,
	Spells,
	Resistances
}