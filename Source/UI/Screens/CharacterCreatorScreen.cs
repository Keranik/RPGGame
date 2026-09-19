using System;
using System.Linq;
using RPGGame.Core;
using RPGGame.Core.Characters;
using RPGGame.Core.Characters.Creation;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Simulation;
using RPGGame.Core.Spells;
using RPGGame.UI.CharacterCreation;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// Full-screen character creation interface.
/// Allows players to select class, allocate attributes, choose skills/spells,
/// and customize appearance.
/// </summary>
public class CharacterCreatorScreen : VisualElement {
	#region Constants

	private const float SKILL_SLOT_SIZE = 64f;
	private const float SPELL_SLOT_SIZE = 64f;
	private const int MAX_VISIBLE_SKILLS = 12;
	private const int MAX_VISIBLE_SPELLS = 12;

	#endregion

	#region Private Fields

	private readonly GameStateManager k_stateManager;
	private readonly GameSession k_session;
	private readonly GameDb k_gameDb;

	private readonly CharacterCreationManager k_manager;

	// UI Elements
	private GameContainer? k_mainContainer;
	private GameContainer? k_leftPanel;
	private GameContainer? k_centerPanel;
	private GameContainer? k_rightPanel;

	// Left panel elements
	private GameTextField? k_nameField;
	private GameScrollView? k_classScrollView;
	private GameContainer? k_classSelector;
	private GameLabel? k_classDescription;

	// Center panel elements - Attributes (compact)
	private AttributeAllocatorPanel? k_attributePanel;
	private GameContainer? k_attributePlaceholder;
	private GamePanel? k_attributeCard;

	// Center panel elements - Skills & Spells
	private GamePanel? k_skillsPanel;
	private GameContainer? k_skillSlotsContainer;
	private GameLabel? k_skillsCountLabel;
	private GamePanel? k_spellsPanel;
	private GameContainer? k_spellSlotsContainer;
	private GameLabel? k_spellsCountLabel;

	// Skill/Spell slot displays
	private readonly List<GameSkillSlotDisplay> k_skillSlots = [];
	private readonly List<GameSpellSlotDisplay> k_spellSlots = [];

	// Right panel elements
	private GameLabel? k_pointsRemainingLabel;
	private GameLabel? k_randomBonusLabel;
	private GameButton? k_rerollButton;
	private GameButton? k_resetButton;
	private GameButton? k_confirmButton;

	// Portrait
	private GameContainer? k_portraitContainer;

	// State
	private CharacterClassProto? k_selectedClass;

	#endregion

	#region Constructor

	public CharacterCreatorScreen(
		GameStateManager stateManager,
		GameSession session,
		GameDb gameDb
	) {
		k_stateManager = stateManager;
		k_session = session;
		k_gameDb = gameDb;

		k_manager = new CharacterCreationManager(gameDb);
		k_manager.OnStateChanged += OnStateChanged;
		k_manager.OnValidationFailed += OnValidationFailed;

		BuildUI();
		PopulateClasses();

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		// Set styles directly on this element
		style.width = Length.Percent(100);
		style.height = Length.Percent(100);
		style.position = Position.Absolute;
		style.backgroundColor = colors.Background;

		// Main container with three panels
		k_mainContainer = new GameContainer("creator-main")
			.SetRow()
			.SetGrow()
			.SetPadding(spacing.LG)
			.Build();

		BuildLeftPanel();
		BuildCenterPanel();
		BuildRightPanel();

		k_mainContainer
			.AddChild(k_leftPanel!)
			.AddChild(k_centerPanel!)
			.AddChild(k_rightPanel!);

		Add(k_mainContainer);
	}

	private void BuildLeftPanel() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var scrollStyles = theme.Components.ScrollView;
		var panels = theme.Components.Panel;

		// Left panel - class selection
		k_leftPanel = new GameContainer("creator-left")
			.SetColumn()
			.SetMarginRight(spacing.LG)
			.SetMaxWidth(panels.MinWidth*1.5f)
			.Build();

		// Title
		var title = new GameLabel("Create Your Character")
			.SetStyle(LabelStyle.HeadlineMedium)
			.SetMarginBottom(spacing.LG)
			.Build();

		// Name input
		k_nameField = new GameTextField()
			.SetLabel("Character Name")
			.SetPlaceholder("Enter name...")
			.SetRequired()
			.SetFullWidth()
			.SetMargin(0,0, spacing.GapMD, 0)
			.Build();

		var nameContainer = new GameContainer("name-container")
			.SetMarginBottom(spacing.GapMD)
			.AddChild(k_nameField)
			.Build();

		// Class selection header
		var classHeader = new GameContainer("class-header")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.SM)
			.Build();

		classHeader.Add(new GameLabel("Choose Class")
			.SetStyle(LabelStyle.TitleMedium)
			.Build());

		var classHint = new GameLabel("🔒 = Locked")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();
		classHeader.Add(classHint);

		// Class selector with scroll
		k_classSelector = new GameContainer("class-selector")
			.SetColumn()
			.SetFlexGrow(1f)
			.Build();

		k_classScrollView = new GameScrollView("class-scroll")
			.SetVertical()
			.SetGrow()
			.Build();
		k_classScrollView.AddChild(k_classSelector);

		// Class description
		k_classDescription = new GameLabel("")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Secondary)
			.SetWhiteSpace(WhiteSpace.Normal)
			.SetMarginTop(spacing.SM)
			.SetMarginBottom(spacing.MD)
			.Build();
		k_classDescription.style.minHeight = 60;

		// Portrait placeholder
		float portraitSize = 180;
		k_portraitContainer = new GameContainer("portrait")
			.SetSize(portraitSize, portraitSize)
			.SetBackgroundColor(colors.BackgroundSecondary)
			.SetBorderRadius(borders.RadiusMD)
			.SetAlignSelf(Align.Center)
			.SetCenter()
			.Build();

		var portraitLabel = new GameLabel("🧙")
			.SetStyle(LabelStyle.DisplayMedium)
			.SetColor(LabelColor.Tertiary)
			.Build();

		k_portraitContainer.AddChild(portraitLabel);

		k_leftPanel
			.AddChild(title)
			.AddChild(nameContainer)
			.AddChild(classHeader)
			.AddChild(k_classScrollView)
			.AddChild(k_classDescription)
			.AddChild(k_portraitContainer);
	}

	private void BuildCenterPanel() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		// Center panel - attributes, skills, spells
		k_centerPanel = new GameContainer("creator-center")
			.SetColumn()
			.SetGrow()
			.SetMarginRight(spacing.LG)
			.SetMarginTop(spacing.GapLG+spacing.GapLG)
			.Build();

		// Attributes card (compact)
		BuildAttributesCard();

		// Skills panel
		BuildSkillsPanel();

		// Spells panel
		BuildSpellsPanel();

		k_centerPanel
			.AddChild(k_attributeCard!)
			.AddChild(k_skillsPanel!)
			.AddChild(k_spellsPanel!);
	}

	private void BuildAttributesCard() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_attributeCard = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetHeader("⚔ Attributes")
			.SetPadding(spacing.MD)
			.SetMarginBottom(spacing.MD)
			.Build();

		// Placeholder for attribute panel
		k_attributePlaceholder = new GameContainer("attribute-placeholder")
			.SetPadding(spacing.MD)
			.Build();

		var selectClassHint = new GameLabel("Select a class to customize attributes")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Tertiary)
			.Build();

		k_attributePlaceholder.AddChild(selectClassHint);
		k_attributeCard.Content.Add(k_attributePlaceholder);
	}

	private void BuildSkillsPanel() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_skillsPanel = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetPadding(spacing.SM)
			.SetMarginBottom(spacing.MD)
			.SetFlexGrow(1f)
			.Build();

		// Header with count
		var skillsHeader = new GameContainer("skills-header")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.SM)
			.Build();

		skillsHeader.Add(new GameLabel("🎯 Skills")
			.SetStyle(LabelStyle.TitleSmall)
			.Build());

		k_skillsCountLabel = new GameLabel("0 selected")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.Build();
		skillsHeader.Add(k_skillsCountLabel);

		k_skillsPanel.Content.Add(skillsHeader);

		// Skills container with wrap
		k_skillSlotsContainer = new GameContainer("skills-slots")
			.SetRow()
			.SetFlexWrap(Wrap.Wrap)
			.SetAlignItems(Align.FlexStart)
			.Build();

		var skillsScroll = new GameScrollView("skills-scroll")
			.SetVertical()
			.SetGrow()
			.Build();
		skillsScroll.AddChild(k_skillSlotsContainer);

		k_skillsPanel.Content.Add(skillsScroll);

		// Empty state
		var emptySkillsLabel = new GameLabel("Select a class to see available skills")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Tertiary)
			.SetPadding(spacing.MD)
			.Build();
		k_skillSlotsContainer.Add(emptySkillsLabel);
	}

	private void BuildSpellsPanel() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_spellsPanel = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetPadding(spacing.SM)
			.SetFlexGrow(1f)
			.Build();
		k_spellsPanel.style.display = DisplayStyle.None; // Hidden until spellcaster selected

		// Header with count
		var spellsHeader = new GameContainer("spells-header")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.SM)
			.Build();

		spellsHeader.Add(new GameLabel("✨ Spells")
			.SetStyle(LabelStyle.TitleSmall)
			.Build());

		k_spellsCountLabel = new GameLabel("0 selected")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.Build();
		spellsHeader.Add(k_spellsCountLabel);

		k_spellsPanel.Content.Add(spellsHeader);

		// Spells container with wrap
		k_spellSlotsContainer = new GameContainer("spells-slots")
			.SetRow()
			.SetFlexWrap(Wrap.Wrap)
			.SetAlignItems(Align.FlexStart)
			.Build();

		var spellsScroll = new GameScrollView("spells-scroll")
			.SetVertical()
			.SetGrow()
			.Build();
		spellsScroll.AddChild(k_spellSlotsContainer);

		k_spellsPanel.Content.Add(spellsScroll);
	}

	private void BuildRightPanel() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var panels = theme.Components.Panel;

		// Right panel - summary and actions
		k_rightPanel = new GameContainer("creator-right")
			.SetColumn()
			.SetMinWidth(panels.MinWidth*1.5f)
			.SetMaxWidth(panels.MinWidth*1.5f)
			.Build();

		// Points display card
		var pointsCard = new GamePanel()
			.SetVariant(PanelVariant.Elevated)
			.SetPadding(spacing.MD)
			.Build();

		var pointsTitle = new GameLabel("Creation Points")
			.SetStyle(LabelStyle.TitleMedium)
			.SetMarginBottom(spacing.SM)
			.Build();

		k_pointsRemainingLabel = new GameLabel("--")
			.SetStyle(LabelStyle.DisplayMedium)
			.SetColor(LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		var pointsSubtitle = new GameLabel("points remaining")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Secondary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetMarginBottom(spacing.SM)
			.Build();

		// Random bonus indicator (hidden by default, no mention of bonus)
		k_randomBonusLabel = new GameLabel("🎲 Random build locked")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Warning)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetMarginBottom(spacing.SM)
			.Build();
		k_randomBonusLabel.style.display = DisplayStyle.None;

		pointsCard.Content.Add(pointsTitle);
		pointsCard.Content.Add(k_pointsRemainingLabel);
		pointsCard.Content.Add(pointsSubtitle);
		pointsCard.Content.Add(k_randomBonusLabel);

		// Reroll button
		k_rerollButton = new GameButton("🎲 Random Build")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Medium)
			.SetFullWidth()
			.OnClick(OnRerollClicked)
			.SetMarginTop(spacing.MD)
			.SetMarginBottom(spacing.SM)
			.Build();
		k_rerollButton.SetEnabled(false);
		k_rerollButton.tooltip = "Generate a random character build";

		// Reset button (only visible after random reroll)
		k_resetButton = new GameButton("↺ Reset to Defaults")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.SetFullWidth()
			.OnClick(OnResetClicked)
			.SetMarginBottom(spacing.LG)
			.Build();
		k_resetButton.style.display = DisplayStyle.None;

		// Spacer
		var spacer = new GameContainer("spacer")
			.SetGrow()
			.Build();

		// Action buttons
		k_confirmButton = new GameButton("Begin Adventure")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(OnConfirmClicked)
			.SetMarginBottom(spacing.SM)
			.Build();
		k_confirmButton.SetEnabled(false);

		var cancelButton = new GameButton("Cancel")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Medium)
			.SetFullWidth()
			.OnClick(OnCancelClicked)
			.Build();

		k_rightPanel
			.AddChild(pointsCard)
			.AddChild(k_rerollButton)
			.AddChild(k_resetButton)
			.AddChild(spacer)
			.AddChild(k_confirmButton)
			.AddChild(cancelButton);
	}

	#endregion

	#region Class Selection

	private void PopulateClasses() {
		if (k_classSelector == null) return;

		k_classSelector.ClearChildren();

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var listStyles = theme.Components.List;

		// Get ALL classes, including locked ones
		var allClasses = k_gameDb.GetAll<CharacterClassProto>()
			.OrderBy(c => !k_session.IsClassUnlocked(c.Id)) // Unlocked first
			.ThenBy(c => c.Difficulty)
			.ThenBy(c => c.DisplayText.Name)
			.ToList();

		foreach (var classProto in allClasses) {
			var capturedClass = classProto;
			bool isUnlocked = k_session.IsClassUnlocked(classProto.Id);

			var classButton = new GameContainer($"class-{classProto.Id.Value}")
				.SetRow()
				.SetAlignItems(Align.Center)
				.SetMinHeight(listStyles.ItemHeightCompact)
				.SetPadding(spacing.SM)
				.SetMarginBottom(spacing.XS)
				.SetBorderRadius(borders.RadiusSM)
				.SetBorderWidth(borders.WidthThin)
				.SetBorderColor(c => c.SurfaceBorder)
				.Build();

			if (isUnlocked) {
				classButton
					.OnClick(() => SelectClass(capturedClass))
					.OnHoverBackground(c => c.SurfaceHover);
			} else {
				classButton.SetOpacity(0.6f);
				classButton.tooltip = GetUnlockConditionText(classProto);
			}

			// Lock/unlock icon
			var statusIcon = new GameLabel(isUnlocked ? "" : "🔒")
				.SetStyle(LabelStyle.BodyMedium)
				.SetMarginRight(spacing.SM)
				.Build();
			statusIcon.style.width = 24;

			// Class info
			var infoContainer = new GameContainer("class-info")
				.SetColumn()
				.SetGrow()
				.Build();

			var nameLabel = new GameLabel(classProto.DisplayText.Name)
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(isUnlocked ? LabelColor.Primary : LabelColor.Disabled)
				.Build();

			var taglineLabel = new GameLabel(classProto.Tagline)
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Tertiary)
				.Build();

			infoContainer
				.AddChild(nameLabel)
				.AddChild(taglineLabel);

			// Difficulty stars
			var difficultyLabel = new GameLabel(GetDifficultyStars(classProto.Difficulty))
				.SetStyle(LabelStyle.BodySmall)
				.SetColor(isUnlocked ? LabelColor.Warning : LabelColor.Disabled)
				.Build();

			classButton
				.AddChild(statusIcon)
				.AddChild(infoContainer)
				.AddChild(difficultyLabel);

			k_classSelector.AddChild(classButton);
		}
	}

	private string GetUnlockConditionText(CharacterClassProto classProto) {
		if (!string.IsNullOrEmpty(classProto.UnlockHint)) {
			return classProto.UnlockHint;
		}
		if (classProto.RequiredFogClears > 0) {
			return $"Clear {classProto.RequiredFogClears} fog regions to unlock";
		}
		return $"Unlock: {classProto.DisplayText.Name}\n(Complete specific achievements)";
	}

	private void SelectClass(CharacterClassProto classProto) {
		if (k_classSelector == null || k_centerPanel == null) return;

		k_selectedClass = classProto;

		var theme = GameTheme.Current;
		var colors = theme.Colors;

		// Update visual selection for all class buttons
		foreach (var child in k_classSelector.Children()) {
			if (child is GameContainer container) {
				bool isSelected = container.name == $"class-{classProto.Id.Value}";

				if (isSelected) {
					container
						.SetBackgroundColor(colors.SurfaceSelected)
						.SetBorderColor(colors.Primary);
				} else {
					container
						.SetBackgroundColor(Color.clear)
						.SetBorderColor(c => c.SurfaceBorder);
				}
			}
		}

		// Update description
		k_classDescription?.SetText(classProto.DisplayText.Description);

		// Update portrait icon based on class role
		UpdatePortraitForClass(classProto);

		// Initialize creation manager with this class
		k_manager.SelectClass(classProto.Id);

		// Create/update attribute panel
		CreateAttributePanel();

		// Show/hide spell section
		bool canCast = classProto.CanCastSpells;
		k_spellsPanel?.SetVisible(canCast);

		// Enable reroll button
		k_rerollButton?.SetEnabled(true);

		RefreshUI();
		RefreshSkillsDisplay();
		RefreshSpellsDisplay();
	}

	private void UpdatePortraitForClass(CharacterClassProto classProto) {
		if (k_portraitContainer == null) return;

		k_portraitContainer.ClearChildren();

		// Get class-appropriate icon based on ClassRole enum
		string icon = classProto.Role switch {
			ClassRole.Tank => "🛡️",
			ClassRole.Support => "💚",
			ClassRole.Melee => "⚔️",
			ClassRole.Ranged => "🏹",
			ClassRole.Caster => "✨",
			ClassRole.Hybrid => "🎭",
			_ => "🧙"
		};

		var iconLabel = new GameLabel(icon)
			.SetStyle(LabelStyle.DisplayMedium)
			.Build();

		k_portraitContainer.AddChild(iconLabel);
	}

	private void CreateAttributePanel() {
		if (k_attributeCard == null || k_attributePlaceholder == null) return;

		// Remove placeholder if exists
		if (k_attributePlaceholder.parent != null) {
			k_attributePlaceholder.RemoveFromHierarchy();

			// Remove old attribute panel if exists
			k_attributePanel?.RemoveFromHierarchy();

			// Create new attribute panel (compact mode)
			k_attributePanel = new AttributeAllocatorPanel(k_manager);
			k_attributeCard.Content.Add(k_attributePanel);
		} else {
			// Just refresh existing panel
			k_attributePanel?.Refresh();
		}
	}

	private static string GetDifficultyStars(int difficulty) {
		return new string('★', difficulty) + new string('☆', 5 - difficulty);
	}

	#endregion

	#region Skills Display

	private void RefreshSkillsDisplay() {
		if (k_skillSlotsContainer == null || k_selectedClass == null) return;

		ClearSkillSlots();
		k_skillSlotsContainer.Clear();

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var slotStyles = theme.Components.Slot;
		var state = k_manager.CurrentState;
		bool isLocked = k_manager.IsModificationLocked;

		// Get available skills for this class
		var availableSkills = GetAvailableSkillsForClass(k_selectedClass)
			.Take(MAX_VISIBLE_SKILLS)
			.ToList();

		if (availableSkills.Count == 0) {
			var emptyLabel = new GameLabel("No skills available for this class")
				.SetStyle(LabelStyle.BodySmall)
				.SetColor(LabelColor.Tertiary)
				.SetPadding(spacing.MD)
				.Build();
			k_skillSlotsContainer.Add(emptyLabel);
			k_skillsCountLabel?.SetText("0 available");
			return;
		}

		int selectedCount = 0;

		foreach (var skill in availableSkills) {
			bool isSelected = state.SelectedSkills.ContainsKey(skill.Id);
			bool isStartingSkill = k_selectedClass.StartingSkills.Contains(skill.Id);
			int currentRank = isSelected ? state.SelectedSkills[skill.Id] : 0;

			if (isSelected) selectedCount++;

			var slotContainer = new GameContainer($"skill-slot-{skill.Id.Value}")
				.SetColumn()
				.SetAlignItems(Align.Center)
				.SetWidth(slotStyles.SizeS + spacing.SM)
				.SetMargin(spacing.XXS)
				.Build();

			var slot = new GameSkillSlotDisplay()
				.SetSkill(skill, currentRank)
				.SetSize(slotStyles.SizeS)
				.SetInteractive(!isLocked)
				.SetEnabled(!isLocked || isSelected)
				.SetSelected(isSelected) // Use SetSelected for visual highlight
				.SetAllowTooltip(true)
				.SetAllowContextMenu(false)
				.OnClick(() => OnSkillSlotClicked(skill, isSelected, isStartingSkill))
				.Build();

			// Show badge for starting skills
			if (isStartingSkill) {
				slot.SetBadge("★");
			}

			k_skillSlots.Add(slot);

			// Skill name
			var nameLabel = new GameLabel(skill.DisplayText.Name.Truncate(8))
				.SetStyle(LabelStyle.Caption)
				.SetColor(isSelected ? LabelColor.Primary : LabelColor.Secondary)
				.SetTextAlign(TextAnchor.MiddleCenter)
				.Build();

			slotContainer.Add(slot);
			slotContainer.Add(nameLabel);
			k_skillSlotsContainer.Add(slotContainer);
		}

		k_skillsCountLabel?.SetText($"{selectedCount} selected");
	}

	private void OnSkillSlotClicked(SkillProto skill, bool isCurrentlySelected, bool isStartingSkill) {
		if (k_manager.IsModificationLocked) {
			GameToast.Show("Cannot modify random build. Reset to customize.", ToastType.Warning);
			return;
		}

		if (isStartingSkill && isCurrentlySelected) {
			GameToast.Show("Starting skills cannot be removed.");
			return;
		}

		if (isCurrentlySelected) {
			k_manager.RemoveSkill(skill.Id);
		} else {
			k_manager.AddSkill(skill.Id);
		}

		RefreshSkillsDisplay();
	}

	private IEnumerable<SkillProto> GetAvailableSkillsForClass(CharacterClassProto classProto) {
		// Get skills available to this class
		return k_gameDb.GetAll<SkillProto>()
			.Where(s => IsSkillAvailableForClass(s, classProto))
			.OrderByDescending(s => classProto.StartingSkills.Contains(s.Id)) // Starting skills first
			.ThenBy(s => s.RequiredLevel)
			.ThenBy(s => s.DisplayText.Name);
	}

	private bool IsSkillAvailableForClass(SkillProto skill, CharacterClassProto classProto) {
		// Starting skills are always available
		if (classProto.StartingSkills.Contains(skill.Id)) return true;

		// Check ClassAccess restrictions (empty = available to all)
		if (skill.ClassAccess.Count > 0 && !skill.ClassAccess.Any(c => c == classProto.Id)) {
			return false;
		}

		// Check level requirement (character starts at level 1)
		if (skill.RequiredLevel > 1) return false;

		return true;
	}

	private void ClearSkillSlots() {
		foreach (var slot in k_skillSlots) {
			slot.RemoveFromHierarchy();
		}
		k_skillSlots.Clear();
	}

	#endregion

	#region Spells Display

	private void RefreshSpellsDisplay() {
		if (k_spellSlotsContainer == null || k_selectedClass == null) return;

		ClearSpellSlots();
		k_spellSlotsContainer.Clear();

		if (!k_selectedClass.CanCastSpells) {
			k_spellsCountLabel?.SetText("N/A");
			return;
		}

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var slotStyles = theme.Components.Slot;
		var state = k_manager.CurrentState;
		bool isLocked = k_manager.IsModificationLocked;

		// Get available spells for this class
		var availableSpells = GetAvailableSpellsForClass(k_selectedClass)
			.Take(MAX_VISIBLE_SPELLS)
			.ToList();

		if (availableSpells.Count == 0) {
			var emptyLabel = new GameLabel("No spells available at level 1")
				.SetStyle(LabelStyle.BodySmall)
				.SetColor(LabelColor.Tertiary)
				.SetPadding(spacing.MD)
				.Build();
			k_spellSlotsContainer.Add(emptyLabel);
			k_spellsCountLabel?.SetText("0 available");
			return;
		}

		int selectedCount = 0;

		foreach (var spell in availableSpells) {
			bool isSelected = state.SelectedSpells.Contains(spell.Id);
			bool isStartingSpell = k_selectedClass.StartingSpells.Contains(spell.Id);

			if (isSelected) selectedCount++;

			var slotContainer = new GameContainer($"spell-slot-{spell.Id.Value}")
				.SetColumn()
				.SetAlignItems(Align.Center)
				.SetWidth(slotStyles.SizeS + spacing.SM)
				.SetMargin(spacing.XXS)
				.Build();

			var slot = new GameSpellSlotDisplay()
				.SetSpell(spell)
				.SetSize(slotStyles.SizeS)
				.SetPrepared(isSelected)
				.SetInteractive(!isLocked)
				.SetEnabled(!isLocked || isSelected)
				.SetSelected(isSelected) // Use SetSelected for visual highlight
				.SetAllowTooltip(true)
				.SetAllowContextMenu(false)
				.OnClick(() => OnSpellSlotClicked(spell, isSelected, isStartingSpell))
				.Build();

			// Show badge for starting spells
			if (isStartingSpell) {
				slot.SetBadge("★");
			}

			k_spellSlots.Add(slot);

			// Spell name
			var nameLabel = new GameLabel(spell.DisplayText.Name.Truncate(8))
				.SetStyle(LabelStyle.Caption)
				.SetColor(isSelected ? LabelColor.Primary : LabelColor.Secondary)
				.SetTextAlign(TextAnchor.MiddleCenter)
				.Build();

			slotContainer.Add(slot);
			slotContainer.Add(nameLabel);
			k_spellSlotsContainer.Add(slotContainer);
		}

		k_spellsCountLabel?.SetText($"{selectedCount} selected");
	}

	private void OnSpellSlotClicked(SpellProto spell, bool isCurrentlySelected, bool isStartingSpell) {
		if (k_manager.IsModificationLocked) {
			GameToast.Show("Cannot modify random build. Reset to customize.", ToastType.Warning);
			return;
		}

		if (isStartingSpell && isCurrentlySelected) {
			GameToast.Show("Starting spells cannot be removed.");
			return;
		}

		if (isCurrentlySelected) {
			k_manager.RemoveSpell(spell.Id);
		} else {
			k_manager.AddSpell(spell.Id);
		}

		RefreshSpellsDisplay();
	}

	private IEnumerable<SpellProto> GetAvailableSpellsForClass(CharacterClassProto classProto) {
		if (!classProto.CanCastSpells) return [];

		// Get spells available to this class
		return k_gameDb.GetAll<SpellProto>()
			.Where(s => IsSpellAvailableForClass(s, classProto))
			.OrderByDescending(s => classProto.StartingSpells.Contains(s.Id)) // Starting spells first
			.ThenBy(s => s.Level)
			.ThenBy(s => s.School)
			.ThenBy(s => s.DisplayText.Name);
	}

	private bool IsSpellAvailableForClass(SpellProto spell, CharacterClassProto classProto) {
		// Starting spells are always available
		if (classProto.StartingSpells.Contains(spell.Id)) return true;

		// Check spell school restrictions using StartingSpellSchools
		// If class has specific spell schools, only allow spells from those schools
		if (classProto.StartingSpellSchools.Count > 0) {
			// Get the spell's school tag
			var spellSchoolTag = spell.School.ToTagId();
			if (!classProto.StartingSpellSchools.Any(schoolTag => schoolTag == spellSchoolTag)) {
				return false;
			}
		}

		// Only cantrips and level 1 spells at character creation
		if (spell.Level > 1) return false;

		return true;
	}

	private void ClearSpellSlots() {
		foreach (var slot in k_spellSlots) {
			slot.RemoveFromHierarchy();
		}
		k_spellSlots.Clear();
	}

	#endregion

	#region Display Refresh

	private void RefreshUI() {
		if (k_manager.CurrentState.SelectedClassId == null) {
			k_pointsRemainingLabel?.SetText("--");
			k_confirmButton?.SetEnabled(false);
			return;
		}

		var state = k_manager.CurrentState;
		var colors = GameTheme.Current.Colors;
		bool isLocked = k_manager.IsModificationLocked;

		// Update points display
		k_pointsRemainingLabel?.SetText(state.PointsRemaining.ToString());

		if (k_pointsRemainingLabel != null) {
			if (state.PointsRemaining < 0) {
				k_pointsRemainingLabel.SetColor(colors.Error);
			} else if (state.PointsRemaining < 20) {
				k_pointsRemainingLabel.SetColor(colors.Warning);
			} else {
				k_pointsRemainingLabel.SetColor(LabelColor.Primary);
			}
		}

		// Show/hide random bonus label and reset button based on lock state
		if (k_randomBonusLabel != null) {
			k_randomBonusLabel.style.display = isLocked ? DisplayStyle.Flex : DisplayStyle.None;
		}
		if (k_resetButton != null) {
			k_resetButton.style.display = isLocked ? DisplayStyle.Flex : DisplayStyle.None;
		}

		// Update reroll button text
		if (k_rerollButton != null) {
			if (isLocked) {
				k_rerollButton.SetText("🎲 Reroll Again");
			} else {
				k_rerollButton.SetText("🎲 Random Build");
			}
		}

		// Update confirm button
		var (isValid, _) = k_manager.ValidateBuild();
		k_confirmButton?.SetEnabled(isValid && state.PointsRemaining >= 0);
	}

	#endregion

	#region Event Handlers

	private void OnStateChanged(CharacterCreationState state) {
		RefreshUI();
		RefreshSkillsDisplay();
		RefreshSpellsDisplay();
	}

	private void OnValidationFailed(string message) {
		GameToast.Show(message, ToastType.Warning);
	}

	private void OnRerollClicked() {
		if (k_selectedClass == null) {
			GameToast.Show("Please select a class first", ToastType.Warning);
			return;
		}

		k_manager.RandomReroll();

		// Generate random name if empty
		if (k_nameField != null && string.IsNullOrWhiteSpace(k_nameField.Value)) {
			k_nameField.SetValue(k_manager.CurrentState.Name);
		}

		// Refresh attribute panel
		k_attributePanel?.Refresh();
	}

	private void OnResetClicked() {
		if (k_selectedClass == null) return;

		// Reset to class defaults, forfeiting any bonus
		k_manager.ResetToClassDefaults();

		// Refresh attribute panel
		k_attributePanel?.Refresh();

		GameToast.Show("Reset to class defaults.");
	}

	private void OnConfirmClicked() {
		string name = k_nameField?.Value ?? "";
		if (string.IsNullOrWhiteSpace(name)) {
			name = "Hero";
		}

		var result = k_manager.CreateCharacter();

		if (result.Success && result.State != null) {
			k_session.StartNewRunFromCreation(name, result.State);
		} else {
			foreach (var error in result.Errors) {
				GameToast.Show(error, ToastType.Error);
			}
		}
	}

	private void OnCancelClicked() {
		k_stateManager.SetPhase(GamePhase.MainMenu);
	}

	#endregion

	#region Theme

	private void OnThemeChanged(GameTheme theme) {
		style.backgroundColor = theme.Colors.Background;
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		k_manager.OnStateChanged -= OnStateChanged;
		k_manager.OnValidationFailed -= OnValidationFailed;
		k_attributePanel?.RemoveFromHierarchy();
		ClearSkillSlots();
		ClearSpellSlots();
		base.RemoveFromHierarchy();
	}

	#endregion
}