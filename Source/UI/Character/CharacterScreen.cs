using System;
using System.Collections.Generic;
using System.Linq;
using RPGGame.Core;
using RPGGame.Core.Characters;
using RPGGame.Core.Generation;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Item.Equipment;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Simulation;
using RPGGame.Core.Spells;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Character;

/// <summary>
/// The main character/inventory screen combining equipment, inventory, stats, skills, and spells.
/// Opens as a toggleable overlay that doesn't pause the game.
/// 
/// Layout:
/// - Left: Equipment paper doll with character portrait
/// - Center: Inventory grid
/// - Right: Tabbed panel for Stats/Skills/Spells
/// </summary>
public class CharacterScreen : VisualElement {
	#region Constants

	public const string WINDOW_ID = "character-screen";
	private const float SKILL_SLOT_SIZE = 48f;
	private const float SPELL_SLOT_SIZE = 48f;

	#endregion

	#region Private Fields

	private readonly GameDb k_gameDb;
	private readonly GameSession k_session;
	private readonly InventoryManager k_inventoryManager;

	// Main containers
	private GameContainer? k_root;
	private GameContainer? k_header;
	private GameContainer? k_content;
	private GameContainer? k_footer;

	// Column containers
	private GameContainer? k_leftColumn;
	private GameContainer? k_centerColumn;
	private GameContainer? k_rightColumn;

	// Components
	private GameEquipmentPanel? k_equipmentPanel;
	private GameInventoryPanel? k_inventoryPanel;
	private GameTabView? k_rightTabs;

	// Header elements
	private GameLabel? k_characterNameLabel;
	private GameLabel? k_characterClassLabel;
	private GameLabel? k_characterLevelLabel;

	// Tab content
	private GameScrollView? k_statsScrollView;
	private GameScrollView? k_skillsScrollView;
	private GameScrollView? k_spellsScrollView;

	// Skill/Spell details panel
	private GameContainer? k_detailsPanel;
	private GameLabel? k_detailsName;
	private GameLabel? k_detailsDescription;
	private GameContainer? k_detailsTags;
	private GameContainer? k_detailsStats;

	// Slot tracking for cleanup
	private readonly List<GameSkillSlotDisplay> k_skillSlots = [];
	private readonly List<GameSpellSlotDisplay> k_spellSlots = [];

	// State
	private ItemInstance? k_selectedItem;
	private RightPanelTab k_activeTab = RightPanelTab.Stats;
	private bool k_isBuilt;

	#endregion

	#region Constructor (DI)

	/// <summary>
	/// Constructor with dependency injection.
	/// </summary>
	public CharacterScreen(GameDb gameDb, GameSession session, InventoryManager inventoryManager) {
		k_gameDb = gameDb;
		k_session = session;
		k_inventoryManager = inventoryManager;

		// Hidden by default
		style.width = Length.Percent(100);
		style.height = Length.Percent(100);
		style.display = DisplayStyle.None;
		style.position = Position.Absolute;
		pickingMode = PickingMode.Ignore;

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Lazy Build

	/// <summary>
	/// Builds the UI on first show (lazy initialization).
	/// </summary>
	private void EnsureBuilt() {
		if (k_isBuilt) return;

		BuildUI();
		ApplyTheme();
		k_isBuilt = true;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;

		// Root container - fills most of the screen
		k_root = new GameContainer("character-screen-root")
			.SetAbsolute()
			.SetTop(Length.Percent(5))
			.SetLeft(Length.Percent(5))
			.SetWidth(Length.Percent(90))
			.SetHeight(Length.Percent(90))
			.SetColumn()
			.SetBackgroundColor(colors.Background)
			.SetBorderRadius(borders.RadiusLG)
			.SetBorderWidth(borders.WidthMedium)
			.SetBorderColor(colors.SurfaceBorder)
			.Build();

		k_root.pickingMode = PickingMode.Position;

		BuildHeader();
		BuildContent();
		BuildFooter();

		k_root.AddChild(k_header!);
		k_root.AddChild(k_content!);
		k_root.AddChild(k_footer!);

		Add(k_root);
	}

	private void BuildHeader() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var windowStyles = theme.Components.Window;

		k_header = new GameContainer("header")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.SM, spacing.MD, spacing.SM, spacing.MD)
			.SetMinHeight(windowStyles.HeaderHeight)
			.SetBottomSeparator(colors.SurfaceBorder, theme.Borders.WidthThin)
			.Build();

		var charInfo = new GameContainer("char-info")
			.SetRow()
			.SetAlignItems(Align.Center)
			.Build();

		k_characterNameLabel = new GameLabel("Character")
			.SetStyle(LabelStyle.TitleLarge)
			.Build();

		k_characterLevelLabel = new GameLabel("Lv 1")
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Primary)
			.SetMarginLeft(spacing.SM)
			.Build();

		k_characterClassLabel = new GameLabel("Class")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.SetMarginLeft(spacing.SM)
			.Build();

		charInfo
			.AddChild(k_characterNameLabel)
			.AddChild(k_characterLevelLabel)
			.AddChild(k_characterClassLabel);

		var closeButton = new GameButton("✕")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Medium)
			.OnClick(Hide)
			.Build();

		k_header
			.AddChild(charInfo)
			.AddChild(closeButton);
	}

	private void BuildContent() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var dialogStyles = theme.Components.Dialog;

		k_content = new GameContainer("content")
			.SetRow()
			.SetGrow()
			.SetPadding(spacing.MD)
			.Build();

		// Left column: Equipment panel
		k_leftColumn = new GameContainer("left-column")
			.SetColumn()
			.SetWidth(dialogStyles.SmallWidth)
			.SetMarginRight(spacing.MD)
			.Build();

		BuildEquipmentPanel();
		k_leftColumn.AddChild(k_equipmentPanel!);

		// Center column: Inventory
		k_centerColumn = new GameContainer("center-column")
			.SetColumn()
			.SetGrow()
			.SetMarginRight(spacing.MD)
			.Build();

		BuildInventoryPanel();
		k_centerColumn.AddChild(k_inventoryPanel!);

		// Right column: Stats/Skills/Spells tabs
		k_rightColumn = new GameContainer("right-column")
			.SetColumn()
			.SetWidth(dialogStyles.SmallWidth + 100)
			.Build();

		BuildRightPanel();
		k_rightColumn.AddChild(k_rightTabs!);

		// Details panel (shared for skills/spells)
		BuildDetailsPanel();
		k_rightColumn.AddChild(k_detailsPanel!);

		k_content
			.AddChild(k_leftColumn)
			.AddChild(k_centerColumn)
			.AddChild(k_rightColumn);
	}

	private void BuildFooter() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var windowStyles = theme.Components.Window;

		k_footer = new GameContainer("footer")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.XS, spacing.MD, spacing.XS, spacing.MD)
			.SetMinHeight(windowStyles.FooterHeight * 0.6f)
			.SetTopSeparator(colors.SurfaceBorder, theme.Borders.WidthThin)
			.Build();

		var hintsLabel = new GameLabel("[I] Close  [Tab] Switch Panel  [Alt] Compare")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		var goldContainer = new GameContainer("gold")
			.SetRow()
			.SetAlignItems(Align.Center)
			.Build();

		var goldIcon = new GameLabel("🪙")
			.SetStyle(LabelStyle.TitleMedium)
			.Build();

		var goldValue = new GameLabel("0")
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Warning)
			.SetMarginLeft(spacing.XS)
			.Build();
		goldValue.name = "gold-value";

		goldContainer
			.AddChild(goldIcon)
			.AddChild(goldValue);

		k_footer
			.AddChild(hintsLabel)
			.AddChild(goldContainer);
	}

	private void BuildEquipmentPanel() {
		var run = k_session.CurrentRun;
		var dialogStyles = GameTheme.Current.Components.Dialog;

		k_equipmentPanel = new GameEquipmentPanel()
			.SetSize(dialogStyles.SmallWidth - 40, dialogStyles.ScrollableContentHeight * 1.5f)
			.SetSlotSize(80)
			.OnSlotClicked(OnEquipmentSlotClicked)
			.OnSlotRightClicked(OnEquipmentSlotRightClicked)
			.OnItemEquipped(OnItemEquipped)
			.OnItemUnequipped(OnItemUnequipped)
			.Build();

		if (run != null) {
			var character = run.Character;
			k_equipmentPanel
				.SetCharacterName(character.Name)
				.SetCharacterInfo($"Level {character.Level}")
				.SetCharacterClass(new CharacterClassProto.ID(run.CharacterClassId))
				.SetCharacterLevel(character.Level);
		}
	}

	private void BuildInventoryPanel() {
		k_inventoryPanel = new GameInventoryPanel()
			.SetColumns(6)
			.SetMaxSlots(k_inventoryManager.MaxSlots)
			.SetSlotSize(96)
			.SetSlotGap(8)
			.SetShowSortControls(true)
			.SetShowFooter(true)
			.OnItemClicked(OnInventoryItemClicked)
			.OnItemRightClicked(OnInventoryItemRightClicked)
			.OnItemDoubleClicked(OnInventoryItemDoubleClicked)
			.Build();
	}

	private void BuildRightPanel() {
		k_rightTabs = new GameTabView()
			.AddTab("📊 Stats", CreateStatsTab())
			.AddTab("⚔️ Skills", CreateSkillsTab())
			.AddTab("✨ Spells", CreateSpellsTab())
			.SetVariant(TabVariant.Default)
			.OnTabChanged(OnRightTabChanged)
			.Build();
	}

	private void BuildDetailsPanel() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var panelsWidth = theme.Components.EntityDetails.CompactWidth;

		k_detailsPanel = new GameContainer("details-panel")
			.SetColumn()
			.SetPadding(spacing.SM)
			.SetBackgroundColor(colors.BackgroundSecondary)
			.SetBorderRadius(borders.RadiusMD)
			.SetMarginTop(spacing.SM)
			.SetMinWidth(panelsWidth)
			.Build();
		k_detailsPanel.style.display = DisplayStyle.None;

		// Header row
		var headerRow = new GameContainer("details-header")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.XS)
			.Build();

		k_detailsName = new GameLabel("")
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.Build();

		headerRow.Add(k_detailsName);

		// Description
		k_detailsDescription = new GameLabel("")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Secondary)
			.Build();
		k_detailsDescription.style.whiteSpace = WhiteSpace.Normal;
		k_detailsDescription.style.marginBottom = spacing.XS;

		// Tags row
		k_detailsTags = new GameContainer("details-tags")
			.SetRow()
			.SetFlexWrap(Wrap.Wrap)
			.SetMarginBottom(spacing.XS)
			.Build();

		// Stats grid
		k_detailsStats = new GameContainer("details-stats")
			.SetRow()
			.SetFlexWrap(Wrap.Wrap)
			.Build();

		k_detailsPanel.Add(headerRow);
		k_detailsPanel.Add(k_detailsDescription);
		k_detailsPanel.Add(k_detailsTags);
		k_detailsPanel.Add(k_detailsStats);
	}

	#endregion

	#region Tab Content

	private VisualElement CreateStatsTab() {
		k_statsScrollView = new GameScrollView("stats-scroll")
			.SetVertical()
			.SetGrow()
			.Build();
		return k_statsScrollView;
	}

	private VisualElement CreateSkillsTab() {
		k_skillsScrollView = new GameScrollView("skills-scroll")
			.SetVertical()
			.SetGrow()
			.Build();
		return k_skillsScrollView;
	}

	private VisualElement CreateSpellsTab() {
		k_spellsScrollView = new GameScrollView("spells-scroll")
			.SetVertical()
			.SetGrow()
			.Build();
		return k_spellsScrollView;
	}

	#endregion

	#region Stats Population

	private void RefreshStats() {
		if (k_statsScrollView == null) return;

		k_statsScrollView.ClearContent();
		var run = k_session.CurrentRun;
		if (run == null) return;

		var categories = new[] {
			(Ids.StatCategories.Attributes, "Attributes"),
			(Ids.StatCategories.Resource, "Resources"),
			(Ids.StatCategories.Combat, "Combat"),
			(Ids.StatCategories.Defense, "Defense"),
			(Ids.StatCategories.Resistances, "Resistances")
		};

		foreach (var (categoryId, categoryName) in categories) {
			var stats = k_gameDb.GetAll<StatProto>()
				.Where(s => s.Category == categoryId)
				.OrderBy(s => s.DisplayOrder)
				.ToList();

			if (stats.Count == 0) continue;

			var section = CreateStatSection(categoryName, stats, run);
			k_statsScrollView.AddChild(section);
		}
	}

	private VisualElement CreateStatSection(string title, List<StatProto> stats, RunState run) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var section = new GameContainer($"stat-section-{title.ToLower()}")
			.SetColumn()
			.SetMarginBottom(spacing.MD)
			.Build();

		var header = new GameLabel(title)
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Secondary)
			.SetMarginBottom(spacing.XS)
			.Build();

		section.Add(header);

		foreach (var stat in stats) {
			var row = CreateStatRow(stat, run);
			section.Add(row);
		}

		return section;
	}

	private VisualElement CreateStatRow(StatProto stat, RunState run) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		float value = run.Stats.Get(stat.Id);
		string formattedValue = stat.FormatValue(value);

		var row = new GameContainer($"stat-{stat.Id.Value}")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.XXS, spacing.XS, spacing.XXS, spacing.XS)
			.Build();

		row.RegisterCallback<MouseEnterEvent>(_ => row.style.backgroundColor = colors.SurfaceHover);
		row.RegisterCallback<MouseLeaveEvent>(_ => row.style.backgroundColor = StyleKeyword.Null);

		var nameContainer = new GameContainer("name")
			.SetRow()
			.SetAlignItems(Align.Center)
			.Build();

		if (!string.IsNullOrEmpty(stat.Abbreviation)) {
			var abbrevLabel = new GameLabel(stat.Abbreviation)
				.SetStyle(LabelStyle.LabelMedium)
				.SetColor(LabelColor.Primary)
				.Build();
			nameContainer.Add(abbrevLabel);
		}

		var nameLabel = new GameLabel(stat.DisplayText.Name)
			.SetStyle(LabelStyle.BodySmall)
			.Build();
		nameContainer.Add(nameLabel);

		var valueLabel = new GameLabel(formattedValue)
			.SetStyle(LabelStyle.LabelMedium)
			.Build();

		if (stat.HigherIsBetter) {
			if (value > stat.DefaultValue) valueLabel.style.color = colors.Success;
			else if (value < stat.DefaultValue) valueLabel.style.color = colors.Error;
		} else {
			if (value < stat.DefaultValue) valueLabel.style.color = colors.Success;
			else if (value > stat.DefaultValue) valueLabel.style.color = colors.Error;
		}

		row.Add(nameContainer);
		row.Add(valueLabel);

		return row;
	}

	#endregion

	#region Skills Population

	private void RefreshSkills() {
		if (k_skillsScrollView == null) return;

		k_skillsScrollView.ClearContent();
		ClearSkillSlots();
		HideDetails();

		var run = k_session.CurrentRun;
		if (run == null) return;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var allSkills = k_gameDb.GetAll<SkillProto>().ToList();

		var skillGroups = new (TagProto.ID tag, string name, string icon)[] {
			(Ids.Tags.Meta.CombatSkill, "Combat", "⚔️"),
			(Ids.Tags.Meta.MagicSkill, "Magic", "✨"),
			(Ids.Tags.Meta.PhysicalSkill, "Physical", "💪"),
			(Ids.Tags.Meta.StealthSkill, "Stealth", "🥷"),
			(Ids.Tags.Meta.SurvivalSkill, "Survival", "🏕️"),
			(Ids.Tags.Meta.CraftingSkill, "Crafting", "🔨"),
			(Ids.Tags.Meta.SocialSkill, "Social", "💬"),
			(Ids.Tags.Meta.KnowledgeSkill, "Knowledge", "📚")
		};

		foreach (var (tag, categoryName, icon) in skillGroups) {
			var skills = allSkills
				.Where(s => s.HasTag(tag))
				.OrderByDescending(s => run.Character.GetSkillRank(s.Id)) // Known skills first
				.ThenBy(s => s.DisplayText.Name)
				.ToList();

			if (skills.Count == 0) continue;

			var section = CreateSkillSection(categoryName, icon, skills, run);
			k_skillsScrollView.AddChild(section);
		}
	}

	private VisualElement CreateSkillSection(string title, string icon, List<SkillProto> skills, RunState run) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		var section = new GameContainer($"skill-section-{title.ToLower()}")
			.SetColumn()
			.SetMarginBottom(spacing.MD)
			.Build();

		// Header with icon
		var headerRow = new GameContainer("header-row")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.XS)
			.Build();

		var iconLabel = new GameLabel(icon)
			.SetStyle(LabelStyle.TitleSmall)
			.SetMarginRight(spacing.XS)
			.Build();

		var titleLabel = new GameLabel(title)
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Secondary)
			.Build();

		// Count of known skills in this category
		int knownCount = skills.Count(s => run.Character.GetSkillRank(s.Id) > 0);
		var countBadge = new GameBadge()
			.SetText($"{knownCount}/{skills.Count}")
			.SetVariant(knownCount > 0 ? BadgeVariant.Success : BadgeVariant.Default)
			.SetSize(BadgeSize.Small)
			.Build();
		countBadge.style.marginLeft = spacing.XS;

		headerRow.Add(iconLabel);
		headerRow.Add(titleLabel);
		headerRow.Add(countBadge);

		section.Add(headerRow);

		// Skills grid using GameSkillSlotDisplay
		var grid = new GameContainer("skill-grid")
			.SetRow()
			.SetFlexWrap(Wrap.Wrap)
			.Build();

		foreach (var skill in skills) {
			var skillContainer = CreateSkillSlotContainer(skill, run);
			grid.Add(skillContainer);
		}

		section.Add(grid);

		return section;
	}

	private VisualElement CreateSkillSlotContainer(SkillProto skill, RunState run) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		int rank = run.Character.GetSkillRank(skill.Id);
		bool hasSkill = rank > 0;

		var container = new GameContainer($"skill-container-{skill.Id.Value}")
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetWidth(SKILL_SLOT_SIZE + spacing.SM * 2)
			.SetPadding(spacing.XS)
			.SetMargin(spacing.XXS)
			.SetBorderRadius(theme.Borders.RadiusSM)
			.Build();

		container.RegisterCallback<MouseEnterEvent>(_ => container.style.backgroundColor = colors.SurfaceHover);
		container.RegisterCallback<MouseLeaveEvent>(_ => container.style.backgroundColor = StyleKeyword.Null);

		// Create skill slot
		var slot = new GameSkillSlotDisplay()
			.SetSkill(skill, rank)
			.SetSize(SKILL_SLOT_SIZE)
			.SetInteractive(true)
			.SetEnabled(hasSkill)
			.OnClick(() => ShowSkillDetails(skill, rank))
			.OnRightClick(() => ShowSkillDetails(skill, rank))
			.Build();

		k_skillSlots.Add(slot);

		// Name label
		var nameLabel = new GameLabel(skill.DisplayText.Name.Truncate(8))
			.SetStyle(LabelStyle.Caption)
			.SetColor(hasSkill ? LabelColor.Primary : LabelColor.Disabled)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();
		nameLabel.style.maxWidth = SKILL_SLOT_SIZE + spacing.SM;

		// Rank indicator
		VisualElement indicator;
		if (hasSkill) {
			indicator = new GameBadge()
				.SetText($"{rank}/{skill.MaxRank}")
				.SetVariant(rank >= skill.MaxRank ? BadgeVariant.Success : BadgeVariant.Info)
				.SetSize(BadgeSize.Small)
				.Build();
		} else {
			indicator = new GameLabel("—")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Disabled)
				.Build();
		}

		container.Add(slot);
		container.Add(nameLabel);
		container.Add(indicator);

		return container;
	}

	private void ShowSkillDetails(SkillProto skill, int rank) {
		if (k_detailsPanel == null) return;

		k_detailsPanel.style.display = DisplayStyle.Flex;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		bool hasSkill = rank > 0;

		// Name
		string skillIcon = skill.IsPassive ? "📖" : "⚡";
		k_detailsName?.SetText($"{skillIcon} {skill.DisplayText.Name}");
		k_detailsName?.SetColor(hasSkill ? LabelColor.Primary : LabelColor.Tertiary);

		// Description
		k_detailsDescription?.SetText(skill.DisplayText.Description ?? "No description available.");

		// Tags
		k_detailsTags?.Clear();
		if (k_detailsTags != null) {
			// Skill type
			AddTagBadge(k_detailsTags, skill.IsPassive ? "Passive" : "Active",
				skill.IsPassive ? ColorRPG.MissGray : ColorRPG.ManaBlue, spacing);

			// Linked attribute - check for attribute tags
			string? linkedAttribute = GetLinkedAttributeFromTags(skill);
			if (linkedAttribute != null) {
				AddTagBadge(k_detailsTags, linkedAttribute, ColorRPG.ExperiencePurple, spacing);
			}

			// Tags from skill
			foreach (var tagId in skill.Tags) {
				// Skip attribute tags since we already displayed them above
				if (IsAttributeTag(tagId)) continue;

				var tagProto = k_gameDb.GetOrNull<TagProto>(tagId);
				if (tagProto != null && tagProto.ShowInUI) {
					var tagColor = !string.IsNullOrEmpty(tagProto.ColorHint)
						? ColorRPG.FromHex(tagProto.ColorHint)
						: ColorRPG.MissGray;
					AddTagBadge(k_detailsTags, tagProto.DisplayText.Name, tagColor, spacing);
				}
			}
		}

		// Stats
		k_detailsStats?.Clear();
		if (k_detailsStats != null) {
			AddStatDisplay(k_detailsStats, "Rank", hasSkill ? $"{rank}/{skill.MaxRank}" : "Not learned",
				hasSkill ? ColorRPG.HealGreen : ColorRPG.MissGray, spacing);

			if (skill.RequiredLevel > 1) {
				AddStatDisplay(k_detailsStats, "Required Level", skill.RequiredLevel.ToString(), ColorRPG.ExperiencePurple, spacing);
			}

			if (!skill.IsPassive) {
				if (skill.ManaCost > 0) {
					AddStatDisplay(k_detailsStats, "Mana Cost", skill.ManaCost.ToString(), ColorRPG.ManaBlue, spacing);
				}
				if (skill.StaminaCost > 0) {
					AddStatDisplay(k_detailsStats, "Stamina Cost", skill.StaminaCost.ToString(), ColorRPG.StaminaYellow, spacing);
				}
				if (skill.Cooldown > 0) {
					AddStatDisplay(k_detailsStats, "Cooldown", $"{skill.Cooldown} turns", ColorRPG.StaminaYellow, spacing);
				}
			}
		}
	}

	/// <summary>
	/// Gets the linked attribute name from skill tags, if any.
	/// </summary>
	private static string? GetLinkedAttributeFromTags(SkillProto skill) {
		if (skill.HasTag(Ids.Tags.Attribute.Strength)) return "Strength";
		if (skill.HasTag(Ids.Tags.Attribute.Dexterity)) return "Dexterity";
		if (skill.HasTag(Ids.Tags.Attribute.Constitution)) return "Constitution";
		if (skill.HasTag(Ids.Tags.Attribute.Intelligence)) return "Intelligence";
		if (skill.HasTag(Ids.Tags.Attribute.Wisdom)) return "Wisdom";
		if (skill.HasTag(Ids.Tags.Attribute.Charisma)) return "Charisma";
		return null;
	}

	/// <summary>
	/// Checks if a tag is an attribute tag.
	/// </summary>
	private static bool IsAttributeTag(TagProto.ID tagId) {
		return tagId == Ids.Tags.Attribute.Strength
			|| tagId == Ids.Tags.Attribute.Dexterity
			|| tagId == Ids.Tags.Attribute.Constitution
			|| tagId == Ids.Tags.Attribute.Intelligence
			|| tagId == Ids.Tags.Attribute.Wisdom
			|| tagId == Ids.Tags.Attribute.Charisma;
	}

	#endregion

	#region Spells Population

	private void RefreshSpells() {
		if (k_spellsScrollView == null) return;

		k_spellsScrollView.ClearContent();
		ClearSpellSlots();
		HideDetails();

		var run = k_session.CurrentRun;
		if (run == null) return;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var knownSpellIds = run.Character.KnownSpells;
		if (knownSpellIds.Count == 0) {
			var emptyLabel = new GameLabel("No spells known")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.SetMargin(spacing.MD)
				.Build();
			k_spellsScrollView.AddChild(emptyLabel);
			return;
		}

		var spells = knownSpellIds
			.Select(id => k_gameDb.GetOrNull<SpellProto>(id))
			.Where(s => s != null)
			.Cast<SpellProto>()
			.GroupBy(s => s.School)
			.OrderBy(g => g.Key.ToString())
			.ToDictionary(g => g.Key, g => g.OrderBy(s => s.Level).ThenBy(s => s.DisplayText.Name).ToList());

		foreach (var (school, schoolSpells) in spells) {
			var section = CreateSpellSection(school, schoolSpells);
			k_spellsScrollView.AddChild(section);
		}
	}

	private VisualElement CreateSpellSection(SpellSchool school, List<SpellProto> spells) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var schoolColor = school.GetColor();

		var section = new GameContainer($"spell-section-{school}")
			.SetColumn()
			.SetMarginBottom(spacing.MD)
			.Build();

		// Header with school icon and color
		var headerRow = new GameContainer("header-row")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.XS)
			.Build();

		var iconLabel = new GameLabel(school.GetIconName())
			.SetStyle(LabelStyle.TitleSmall)
			.SetMarginRight(spacing.XS)
			.Build();
		iconLabel.style.color = schoolColor;

		var titleLabel = new GameLabel(school.GetDisplayName())
			.SetStyle(LabelStyle.TitleSmall)
			.Build();
		titleLabel.style.color = schoolColor;

		var countBadge = new GameBadge()
			.SetText(spells.Count.ToString())
			.SetVariant(BadgeVariant.Default)
			.SetSize(BadgeSize.Small)
			.Build();
		countBadge.style.marginLeft = spacing.XS;

		headerRow.Add(iconLabel);
		headerRow.Add(titleLabel);
		headerRow.Add(countBadge);

		section.Add(headerRow);

		// Spells grid using GameSpellSlotDisplay
		var grid = new GameContainer("spell-grid")
			.SetRow()
			.SetFlexWrap(Wrap.Wrap)
			.Build();

		foreach (var spell in spells) {
			var spellContainer = CreateSpellSlotContainer(spell);
			grid.Add(spellContainer);
		}

		section.Add(grid);

		return section;
	}

	private VisualElement CreateSpellSlotContainer(SpellProto spell) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var schoolColor = spell.School.GetColor();

		var container = new GameContainer($"spell-container-{spell.Id.Value}")
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetWidth(SPELL_SLOT_SIZE + spacing.SM * 2)
			.SetPadding(spacing.XS)
			.SetMargin(spacing.XXS)
			.SetBorderRadius(theme.Borders.RadiusSM)
			.Build();

		container.RegisterCallback<MouseEnterEvent>(_ => container.style.backgroundColor = colors.SurfaceHover);
		container.RegisterCallback<MouseLeaveEvent>(_ => container.style.backgroundColor = StyleKeyword.Null);

		// Create spell slot
		var slot = new GameSpellSlotDisplay()
			.SetSpell(spell)
			.SetSize(SPELL_SLOT_SIZE)
			.SetInteractive(true)
			.SetPrepared(true)
			.OnClick(() => ShowSpellDetails(spell))
			.OnRightClick(() => ShowSpellDetails(spell))
			.Build();

		k_spellSlots.Add(slot);

		// Name label
		var nameLabel = new GameLabel(spell.DisplayText.Name.Truncate(8))
			.SetStyle(LabelStyle.Caption)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();
		nameLabel.style.maxWidth = SPELL_SLOT_SIZE + spacing.SM;
		nameLabel.style.color = schoolColor;

		// Level/cost indicator
		int manaCost = spell.GetEffectiveManaCost();
		VisualElement indicator;
		if (manaCost > 0) {
			indicator = new GameBadge()
				.SetText($"{manaCost}MP")
				.SetVariant(BadgeVariant.Info)
				.SetSize(BadgeSize.Small)
				.Build();
		} else {
			indicator = new GameLabel("Free")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Success)
				.Build();
		}

		container.Add(slot);
		container.Add(nameLabel);
		container.Add(indicator);

		return container;
	}

	private void ShowSpellDetails(SpellProto spell) {
		if (k_detailsPanel == null) return;

		k_detailsPanel.style.display = DisplayStyle.Flex;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var schoolColor = spell.School.GetColor();

		// Name with school icon
		k_detailsName?.SetText($"{spell.School.GetIconName()} {spell.DisplayText.Name}");
		if (k_detailsName != null) {
			k_detailsName.style.color = schoolColor;
		}

		// Description
		k_detailsDescription?.SetText(spell.DisplayText.Description ?? "No description available.");

		// Tags
		k_detailsTags?.Clear();
		if (k_detailsTags != null) {
			// School
			AddTagBadge(k_detailsTags, spell.School.GetDisplayName(), schoolColor, spacing);

			// Level
			string levelText = spell.IsCantrip ? "Cantrip" : $"Level {spell.Level}";
			AddTagBadge(k_detailsTags, levelText, ColorRPG.ExperiencePurple, spacing);

			// Target type
			AddTagBadge(k_detailsTags, spell.TargetType.ToString(), ColorRPG.ShieldBlue, spacing);

			// Concentration
			if (spell.RequiresConcentration) {
				AddTagBadge(k_detailsTags, "⚡ Concentration", theme.Colors.Warning, spacing);
			}

			// Damage type
			if (spell.DamageDice.IsValid) {
				AddTagBadge(k_detailsTags, spell.DamageType.ToString(), GetDamageTypeColor(spell.DamageType), spacing);
			}
		}

		// Stats
		k_detailsStats?.Clear();
		if (k_detailsStats != null) {
			int manaCost = spell.GetEffectiveManaCost();
			if (manaCost > 0) {
				AddStatDisplay(k_detailsStats, "Mana Cost", manaCost.ToString(), ColorRPG.ManaBlue, spacing);
			}

			if (spell.HealthCost > 0) {
				AddStatDisplay(k_detailsStats, "Health Cost", spell.HealthCost.ToString(), ColorRPG.RageRed, spacing);
			}

			if (spell.Cooldown > 0) {
				AddStatDisplay(k_detailsStats, "Cooldown", $"{spell.Cooldown} turns", ColorRPG.StaminaYellow, spacing);
			}

			if (spell.Range > 0) {
				AddStatDisplay(k_detailsStats, "Range", $"{spell.Range} ft", ColorRPG.MissGray, spacing);
			}

			if (spell.AreaRadius > 0) {
				AddStatDisplay(k_detailsStats, "Area", $"{spell.AreaRadius} ft radius", ColorRPG.MissGray, spacing);
			}

			if (spell.DamageDice.IsValid) {
				AddStatDisplay(k_detailsStats, "Damage", spell.DamageDice.ToString(), ColorRPG.RageRed, spacing);
			}

			if (spell.HealingDice.IsValid) {
				AddStatDisplay(k_detailsStats, "Healing", spell.HealingDice.ToString(), ColorRPG.HealGreen, spacing);
			}

			if (spell.AppliesCondition.HasValue) {
				AddStatDisplay(k_detailsStats, "Applies", spell.AppliesCondition.Value.ToString(), ColorRPG.DebuffPurple, spacing);
			}
		}
	}

	#endregion

	#region Details Panel Helpers

	private void HideDetails() {
		if (k_detailsPanel != null) {
			k_detailsPanel.style.display = DisplayStyle.None;
		}
	}

	private void AddTagBadge(GameContainer parent, string text, ColorRPG color, SpacingSettings spacing) {
		var badge = new GameBadge()
			.SetText(text)
			.SetVariant(BadgeVariant.Default)
			.SetSize(BadgeSize.Small)
			.Build();
		badge.style.backgroundColor = color.WithAlpha(0.2f);
		badge.style.color = color;
		badge.style.marginRight = spacing.XXS;
		badge.style.marginBottom = spacing.XXS;
		parent.Add(badge);
	}

	private void AddStatDisplay(GameContainer parentToAddTo, string label, string value, ColorRPG color, SpacingSettings spacing) {
		var stat = new GameContainer()
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.XS)
			.SetMarginRight(spacing.SM)
			.Build();

		var labelEl = new GameLabel(label)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		var valueEl = new GameLabel(value)
			.SetStyle(LabelStyle.BodyMedium)
			.Build();
		valueEl.style.color = color;

		stat.Add(labelEl);
		stat.Add(valueEl);
		parentToAddTo.Add(stat);
	}

	private static ColorRPG GetDamageTypeColor(DamageType type) {
		return type switch {
			DamageType.Fire => ColorRPG.Fire,
			DamageType.Cold => ColorRPG.Ice,
			DamageType.Lightning => ColorRPG.Lightning,
			DamageType.Poison => ColorRPG.Poison,
			DamageType.Acid => ColorRPG.Poison.RotateHue(-30),
			DamageType.Holy => ColorRPG.Holy,
			DamageType.Necrotic => ColorRPG.NecroticPurple,
			DamageType.Psychic => ColorRPG.Arcane,
			DamageType.Force => ColorRPG.Arcane.Lighten(0.2f),
			_ => ColorRPG.RageRed
		};
	}

	#endregion

	#region Slot Cleanup

	private void ClearSkillSlots() {
		foreach (var slot in k_skillSlots) {
			slot.RemoveFromHierarchy();
		}
		k_skillSlots.Clear();
	}

	private void ClearSpellSlots() {
		foreach (var slot in k_spellSlots) {
			slot.RemoveFromHierarchy();
		}
		k_spellSlots.Clear();
	}

	#endregion

	#region Event Handlers

	private void OnEquipmentSlotClicked(EquipmentSlotProto.ID slotId, ItemInstance? item) {
		k_selectedItem = item;
	}

	private void OnEquipmentSlotRightClicked(EquipmentSlotProto.ID slotId, ItemInstance? item) { }

	private void OnItemEquipped(EquipmentSlotProto.ID slotId, ItemInstance item) {
		RefreshStats();
		GameToast.Show($"Equipped {item.DisplayName}", ToastType.Success, 1500);
	}

	private void OnItemUnequipped(EquipmentSlotProto.ID slotId, ItemInstance item) {
		RefreshStats();
		GameToast.Show($"Unequipped {item.DisplayName}", ToastType.Info, 1500);
	}

	private void OnInventoryItemClicked(ItemInstance? item) {
		k_selectedItem = item;
	}

	private void OnInventoryItemRightClicked(ItemInstance? item) { }

	private void OnInventoryItemDoubleClicked(ItemInstance? item) { }

	private void OnRightTabChanged(int index) {
		k_activeTab = (RightPanelTab)index;
		HideDetails();

		switch (k_activeTab) {
			case RightPanelTab.Stats:
				RefreshStats();
				break;
			case RightPanelTab.Skills:
				RefreshSkills();
				break;
			case RightPanelTab.Spells:
				RefreshSpells();
				break;
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Refreshes all panels with current data.
	/// </summary>
	public void RefreshAll() {
		RefreshHeader();
		RefreshStats();
		RefreshSkills();
		RefreshSpells();
		RefreshGold();
	}

	private void RefreshHeader() {
		var run = k_session.CurrentRun;
		if (run == null) return;

		var character = run.Character;

		k_characterNameLabel?.SetText(character.Name);
		k_characterLevelLabel?.SetText($"Lv {character.Level}");
		k_characterClassLabel?.SetText(run.CharacterClassId);

		k_equipmentPanel?
			.SetCharacterName(character.Name)
			.SetCharacterInfo($"Level {character.Level}")
			.SetCharacterClass(new CharacterClassProto.ID(run.CharacterClassId))
			.SetCharacterLevel(character.Level);
	}

	private void RefreshGold() {
		var run = k_session.CurrentRun;
		if (run == null) return;

		var character = run.Character;
		var goldLabel = k_footer?.Q<GameLabel>("gold-value");
		goldLabel?.SetText(character.BaseStats.GetInt(Ids.Stats.Expedition.GoldOnHand).ToString("N0"));
	}

	/// <summary>
	/// Opens the character screen.
	/// </summary>
	public void Open() {
		EnsureBuilt();
		RefreshAll();
		Show();
	}

	/// <summary>
	/// Toggles the character screen visibility.
	/// </summary>
	public void Toggle() {
		if (IsVisible) {
			Hide();
		} else {
			Open();
		}
	}

	/// <summary>
	/// Shows the screen.
	/// </summary>
	public void Show() {
		EnsureBuilt();
		RefreshAll();
		style.display = DisplayStyle.Flex;
	}

	/// <summary>
	/// Hides the screen.
	/// </summary>
	public void Hide() {
		style.display = DisplayStyle.None;
		HideDetails();
	}

	/// <summary>
	/// Whether the screen is currently visible.
	/// </summary>
	public bool IsVisible => style.display == DisplayStyle.Flex;

	#endregion

	#region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;

		style.backgroundColor = new Color(0, 0, 0, 0.5f);

		k_root?.SetBackgroundColor(colors.Background);
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		ClearSkillSlots();
		ClearSpellSlots();
		k_equipmentPanel?.RemoveFromHierarchy();
		k_inventoryPanel?.RemoveFromHierarchy();
		base.RemoveFromHierarchy();
	}

	#endregion
}

/// <summary>
/// Tabs in the right panel.
/// </summary>
public enum RightPanelTab {
	Stats,
	Skills,
	Spells
}