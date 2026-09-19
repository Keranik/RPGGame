using RPGGame.Core;
using RPGGame.Core.Characters.Creation;
using RPGGame.Core.Characters.LevelUp;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Simulation;
using RPGGame.Core.Spells;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// Panel for applying level ups during camp.
/// Uses the same creation points system as character creation.
/// Allows spending points on attributes, skills, spells, or resources.
/// </summary>
public class LevelUpPanel : VisualElement {
	#region Fields

	private readonly GameStateManager k_stateManager;
	private readonly GameSession k_session;
	private readonly GameDb k_gameDb;

	private LevelUpManager? k_manager;

	private GamePanel k_content = null!;
	private GameLabel k_titleLabel = null!;
	private GameLabel k_levelLabel = null!;
	private GameLabel k_pointsDisplayLabel = null!;
	private GameTabView k_tabView = null!;

	// Attributes tab
	private GamePanel k_attributesPanel = null!;

	// Skills tab
	private GameScrollView k_skillsScroll = null!;

	// Spells tab
	private GameScrollView k_spellsScroll = null!;

	// Resources tab
	private GamePanel k_resourcesPanel = null!;

	// Actions
	private GameButton k_undoButton = null!;
	private GameButton k_resetButton = null!;
	private GameButton k_confirmButton = null!;
	private GameButton k_skipButton = null!;

	#endregion

	#region Events

	public event Action? OnLevelUpComplete;
	public event Action? OnLevelUpSkipped;

	#endregion

	#region Constructor

	public LevelUpPanel(GameStateManager stateManager, GameSession session, GameDb gameDb) {
		k_stateManager = stateManager;
		k_session = session;
		k_gameDb = gameDb;

		BuildUI();

		style.display = DisplayStyle.None;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var dialogStyles = theme.Components.Dialog;

		// Full screen overlay
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		style.alignItems = Align.Center;
		style.justifyContent = Justify.Center;
		style.backgroundColor = colors.BackgroundOverlay;
		pickingMode = PickingMode.Position;

		// Main content
		k_content = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.XL)
			.Build();
		k_content.style.width = dialogStyles.LargeWidth;
		k_content.style.maxWidth = Length.Percent(90);
		k_content.style.maxHeight = Length.Percent(85);

		BuildHeader();
		BuildTabs();
		BuildActions();

		Add(k_content);
	}

	private void BuildHeader() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		// Title row with points display
		var headerPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.Build();

		// Left: Level up icon and title
		var titleContainer = new GameContainer("title")
			.SetRow()
			.SetAlignItems(Align.Center)
			.Build();

		var levelUpIcon = new GameLabel("⬆")
			.SetStyle(LabelStyle.DisplayMedium)
			.SetColor(LabelColor.Success)
			.Build();

		k_titleLabel = new GameLabel("Level Up!")
			.SetStyle(LabelStyle.HeadlineLarge)
			.SetColor(LabelColor.Primary)
			.SetMarginLeft(spacing.MD)
			.Build();

		titleContainer.AddChild(levelUpIcon);
		titleContainer.AddChild(k_titleLabel);

		// Right: Points display
		k_pointsDisplayLabel = new GameLabel("0 points")
			.SetStyle(LabelStyle.DisplaySmall)
			.SetColor(LabelColor.Primary)
			.Build();

		headerPanel.Content.Add(titleContainer);
		headerPanel.Content.Add(k_pointsDisplayLabel);

		// Level transition label
		k_levelLabel = new GameLabel("Level 1 → 2")
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Secondary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetMarginTop(spacing.XS)
			.SetMarginBottom(spacing.MD)
			.Build();

		k_content.Content.Add(headerPanel);
		k_content.Content.Add(k_levelLabel);
	}

	private void BuildTabs() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var dialogStyles = theme.Components.Dialog;

		// Attributes tab content
		k_attributesPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Vertical)
			.SetPadding(spacing.MD)
			.Build();

		// Skills tab content
		k_skillsScroll = new GameScrollView("skills-scroll")
			.SetVertical()
			.SetPadding(spacing.MD)
			.Build();

		// Spells tab content
		k_spellsScroll = new GameScrollView("spells-scroll")
			.SetVertical()
			.SetPadding(spacing.MD)
			.Build();

		// Resources tab content
		k_resourcesPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Vertical)
			.SetPadding(spacing.MD)
			.Build();

		k_tabView = new GameTabView()
			.AddTab("💪 Attributes", k_attributesPanel)
			.AddTab("📚 Skills", k_skillsScroll)
			.AddTab("✨ Spells", k_spellsScroll)
			.AddTab("❤ Resources", k_resourcesPanel)
			.SetVariant(TabVariant.Default)
			.SetGrow()
			.SetHeight(dialogStyles.ScrollableContentHeight)
			.Build();

		k_content.Content.Add(k_tabView);
	}

	private void BuildActions() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var buttonPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.SetMarginTop(spacing.LG)
			.Build();

		// Left: Undo/Reset
		var leftButtons = new GameContainer("left-buttons")
			.SetRow()
			.Build();

		k_undoButton = new GameButton("↩ Undo")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Medium)
			.OnClick(OnUndoClicked)
			.Build();

		k_resetButton = new GameButton("↺ Reset")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Medium)
			.OnClick(OnResetClicked)
			.SetMarginLeft(spacing.SM)
			.Build();

		leftButtons.AddChild(k_undoButton);
		leftButtons.AddChild(k_resetButton);

		// Right: Skip/Confirm
		var rightButtons = new GameContainer("right-buttons")
			.SetRow()
			.Build();

		k_skipButton = new GameButton("Skip for now")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Medium)
			.OnClick(OnSkipClicked)
			.Build();

		k_confirmButton = new GameButton("✓ Confirm Level Up")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.OnClick(OnConfirmClicked)
			.SetMarginLeft(spacing.MD)
			.Build();

		rightButtons.AddChild(k_skipButton);
		rightButtons.AddChild(k_confirmButton);

		buttonPanel.Content.Add(leftButtons);
		buttonPanel.Content.Add(rightButtons);

		k_content.Content.Add(buttonPanel);
	}

	#endregion

	#region Tab Content Building

	private void BuildAttributesTab() {
		k_attributesPanel.ClearChildren();

		if (k_manager == null) return;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var hint = new GameLabel("Higher values cost more points. Cost shown in parentheses.")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.SetMarginBottom(spacing.LG)
			.Build();
		k_attributesPanel.Content.Add(hint);

		var attributes = new[] {
			(Ids.Stats.Attributes.Strength, "💪", "Strength", "Physical power, melee damage"),
			(Ids.Stats.Attributes.Dexterity, "🎯", "Dexterity", "Agility, AC, ranged attacks"),
			(Ids.Stats.Attributes.Constitution, "❤", "Constitution", "Health and endurance"),
			(Ids.Stats.Attributes.Intelligence, "🧠", "Intelligence", "Spell power and mana"),
			(Ids.Stats.Attributes.Wisdom, "👁", "Wisdom", "Perception and willpower"),
			(Ids.Stats.Attributes.Charisma, "✨", "Charisma", "Force of personality")
		};

		foreach (var (stat, icon, name, desc) in attributes) {
			var row = CreateAttributeRow(stat, icon, name, desc);
			k_attributesPanel.Content.Add(row);
		}
	}

	private GamePanel CreateAttributeRow(StatProto.ID stat, string icon, string name, string desc) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var state = k_manager!.CurrentState;

		int currentValue = state.Attributes.GetValueOrDefault(stat, 10);
		int baseValue = state.BaseAttributes.GetValueOrDefault(stat, 10);
		int cost = k_manager.GetAttributeIncreaseCost(stat);
		bool canIncrease = k_manager.CanIncreaseAttribute(stat);
		bool canDecrease = k_manager.CanDecreaseAttribute(stat);
		bool hasIncrease = currentValue > baseValue;

		var row = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.Build();
		row.style.marginBottom = spacing.SM;

		// Left: Name and description
		var leftPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Vertical)
			.Build();
		leftPanel.style.minWidth = 250;

		var nameRow = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();

		var iconLabel = new GameLabel(icon)
			.SetStyle(LabelStyle.TitleMedium)
			.Build();

		var nameLabel = new GameLabel(name)
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.SetMarginLeft(spacing.XS)
			.Build();

		nameRow.Content.Add(iconLabel);
		nameRow.Content.Add(nameLabel);

		var descLabel = new GameLabel(desc)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		leftPanel.Content.Add(nameRow);
		leftPanel.Content.Add(descLabel);

		// Right: Value, cost, and controls
		var rightPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();

		var minusButton = new GameButton("-")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(() => {
				k_manager.DecreaseAttribute(stat);
			})
			.Build();
		minusButton.SetEnabled(canDecrease);

		string valueText = hasIncrease ? $"{baseValue} → {currentValue}" : currentValue.ToString();
		var valueLabel = new GameLabel(valueText)
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(hasIncrease ? LabelColor.Success : LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();
		valueLabel.style.minWidth = 80;

		var plusButton = new GameButton("+")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(() => {
				k_manager.IncreaseAttribute(stat);
			})
			.Build();
		plusButton.SetEnabled(canIncrease);

		var costLabel = new GameLabel($"({cost} pts)")
			.SetStyle(LabelStyle.Caption)
			.SetColor(canIncrease ? LabelColor.Warning : LabelColor.Tertiary)
			.SetMarginLeft(spacing.SM)
			.Build();
		costLabel.style.minWidth = 60;

		rightPanel.Content.Add(minusButton);
		rightPanel.Content.Add(valueLabel);
		rightPanel.Content.Add(plusButton);
		rightPanel.Content.Add(costLabel);

		row.Content.Add(leftPanel);
		row.Content.Add(rightPanel);

		return row;
	}

	private void BuildResourcesTab() {
		k_resourcesPanel.ClearChildren();

		if (k_manager == null) return;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var header = new GameLabel("Increase Maximum Resources")
			.SetStyle(LabelStyle.TitleMedium)
			.SetMarginBottom(spacing.SM)
			.Build();
		k_resourcesPanel.Content.Add(header);

		var hint = new GameLabel("Each purchase adds +5 to the maximum value")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.SetMarginBottom(spacing.LG)
			.Build();
		k_resourcesPanel.Content.Add(hint);

		// Health
		var healthRow = CreateResourceRow(
			"❤", "Max Health",
			k_manager.CurrentState.BonusHealth,
			k_manager.GetHealthIncreaseCost(),
			k_manager.CanIncreaseHealth(),
			k_manager.CanDecreaseHealth(),
			() => k_manager.IncreaseHealth(),
			() => k_manager.DecreaseHealth()
		);
		k_resourcesPanel.Content.Add(healthRow);

		// Mana
		var manaRow = CreateResourceRow(
			"💧", "Max Mana",
			k_manager.CurrentState.BonusMana,
			k_manager.GetManaIncreaseCost(),
			k_manager.CanIncreaseMana(),
			k_manager.CanDecreaseMana(),
			() => k_manager.IncreaseMana(),
			() => k_manager.DecreaseMana()
		);
		k_resourcesPanel.Content.Add(manaRow);

		// Stamina
		var staminaRow = CreateResourceRow(
			"⚡", "Max Stamina",
			k_manager.CurrentState.BonusStamina,
			k_manager.GetStaminaIncreaseCost(),
			k_manager.CanIncreaseStamina(),
			k_manager.CanDecreaseStamina(),
			() => k_manager.IncreaseStamina(),
			() => k_manager.DecreaseStamina()
		);
		k_resourcesPanel.Content.Add(staminaRow);
	}

	private GamePanel CreateResourceRow(
		string icon, string name,
		int currentBonus, int cost,
		bool canIncrease, bool canDecrease,
		Action onIncrease, Action onDecrease) {

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var row = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.Build();
		row.style.marginBottom = spacing.MD;

		// Left: Name and cost
		var leftPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Vertical)
			.Build();

		var nameRow = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();

		var iconLabel = new GameLabel(icon)
			.SetStyle(LabelStyle.TitleMedium)
			.Build();

		var nameLabel = new GameLabel(name)
			.SetStyle(LabelStyle.TitleSmall)
			.SetMarginLeft(spacing.XS)
			.Build();

		nameRow.Content.Add(iconLabel);
		nameRow.Content.Add(nameLabel);

		var costHint = new GameLabel($"+5 per purchase ({cost} pts each)")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Warning)
			.Build();

		leftPanel.Content.Add(nameRow);
		leftPanel.Content.Add(costHint);

		// Right: Controls
		var rightPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetAlignment(Align.Center)
			.Build();

		var minusButton = new GameButton("-")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(onDecrease)
			.Build();
		minusButton.SetEnabled(canDecrease);

		string valueText = currentBonus > 0 ? $"+{currentBonus}" : "+0";
		var valueLabel = new GameLabel(valueText)
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(currentBonus > 0 ? LabelColor.Success : LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();
		valueLabel.style.minWidth = 60;

		var plusButton = new GameButton("+")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(onIncrease)
			.Build();
		plusButton.SetEnabled(canIncrease);

		rightPanel.Content.Add(minusButton);
		rightPanel.Content.Add(valueLabel);
		rightPanel.Content.Add(plusButton);

		row.Content.Add(leftPanel);
		row.Content.Add(rightPanel);

		return row;
	}

	private void BuildSkillsTab() {
		k_skillsScroll.ClearContent();

		if (k_manager == null) return;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		// Known skills section
		var knownHeader = new GameLabel("Known Skills")
			.SetStyle(LabelStyle.TitleMedium)
			.SetMarginBottom(spacing.SM)
			.Build();
		k_skillsScroll.AddChild(knownHeader);

		var state = k_manager.CurrentState;
		var knownSkills = state.Skills;

		if (knownSkills.Count == 0 && state.NewSkillRanks.Count == 0) {
			var noSkills = new GameLabel("No skills learned yet")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.SetMarginBottom(spacing.MD)
				.Build();
			k_skillsScroll.AddChild(noSkills);
		} else {
			// Show known skills with option to rank up
			foreach (var (skillId, baseRank) in knownSkills) {
				var skill = k_gameDb.GetOrNull<SkillProto>(skillId);
				if (skill == null) continue;

				int pendingRank = state.GetPendingSkillRank(skillId);
				var row = CreateSkillRow(skill, baseRank, pendingRank);
				k_skillsScroll.AddChild(row);
			}

			// Show newly learned skills (not in base known)
			foreach (var (skillId, pendingRank) in state.NewSkillRanks) {
				if (knownSkills.ContainsKey(skillId)) continue; // Already shown above

				var skill = k_gameDb.GetOrNull<SkillProto>(skillId);
				if (skill == null) continue;

				var row = CreateSkillRow(skill, 0, pendingRank);
				k_skillsScroll.AddChild(row);
			}
		}

		// Divider
		var divider = new GameDivider()
			.SetMargin(spacing.MD, 0)
			.Build();
		k_skillsScroll.AddChild(divider);

		// Available skills section
		var availableHeader = new GameLabel("Available to Learn")
			.SetStyle(LabelStyle.TitleMedium)
			.SetMarginBottom(spacing.SM)
			.Build();
		k_skillsScroll.AddChild(availableHeader);

		var availableSkills = k_manager.GetAvailableSkills()
			.Where(s => !knownSkills.ContainsKey(s.Id) && !state.NewSkillRanks.ContainsKey(s.Id))
			.ToList();

		if (availableSkills.Count == 0) {
			var noAvailable = new GameLabel("No new skills available at this level")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.Build();
			k_skillsScroll.AddChild(noAvailable);
		} else {
			foreach (var skill in availableSkills) {
				var row = CreateSkillRow(skill, 0, 0);
				k_skillsScroll.AddChild(row);
			}
		}
	}

	private GameContainer CreateSkillRow(SkillProto skill, int baseRank, int pendingRank) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;

		int totalRank = baseRank + pendingRank;
		bool hasPending = pendingRank > 0;
		bool isNewSkill = baseRank == 0 && pendingRank > 0;

		int cost = k_manager!.GetSkillRankCost(skill.Id);
		bool canIncrease = k_manager.CanIncreaseSkillRank(skill.Id);
		bool canDecrease = k_manager.CanDecreaseSkillRank(skill.Id);

		Color bgColor = hasPending ? colors.SurfaceSelected : colors.Surface;

		var container = new GameContainer($"skill-{skill.Id.Value}")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.SM, spacing.MD, spacing.SM, spacing.MD)
			.SetMarginBottom(spacing.XS)
			.SetBackgroundColor(bgColor)
			.SetBorderRadius(borders.RadiusSM)
			.Build();

		// Left: Name and rank display
		var leftPanel = new GameContainer("left")
			.SetColumn()
			.Build();

		string nameText = skill.DisplayText.Name;
		if (totalRank > 0) {
			if (hasPending && baseRank > 0) {
				nameText += $" (Rank {baseRank} → {totalRank})";
			} else if (isNewSkill) {
				nameText += $" (Rank {totalRank}) ✨";
			} else {
				nameText += $" (Rank {totalRank})";
			}
		}

		var nameLabel = new GameLabel(nameText)
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(hasPending ? LabelColor.Success : LabelColor.Primary)
			.Build();

		var descLabel = new GameLabel(skill.DisplayText.Description)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		leftPanel.AddChild(nameLabel);
		leftPanel.AddChild(descLabel);

		// Right: Cost and controls
		var rightPanel = new GameContainer("right")
			.SetRow()
			.SetAlignItems(Align.Center)
			.Build();

		if (baseRank > 0 && pendingRank == 0) {
			// Known skill with no pending changes - show rank up option
			var costLabel = new GameLabel($"{cost} pts")
				.SetStyle(LabelStyle.Caption)
				.SetColor(canIncrease ? LabelColor.Warning : LabelColor.Tertiary)
				.Build();
			costLabel.style.marginRight = spacing.SM;

			var plusButton = new GameButton("+")
				.SetVariant(ButtonVariant.Outline)
				.SetSize(ButtonSize.Small)
				.OnClick(() => k_manager.IncreaseSkillRank(skill.Id))
				.Build();
			plusButton.SetEnabled(canIncrease);

			rightPanel.AddChild(costLabel);
			rightPanel.AddChild(plusButton);
		} else if (hasPending) {
			// Has pending ranks - show +/- controls
			var costLabel = new GameLabel($"{cost} pts")
				.SetStyle(LabelStyle.Caption)
				.SetColor(canIncrease ? LabelColor.Warning : LabelColor.Tertiary)
				.Build();
			costLabel.style.marginRight = spacing.SM;

			var minusButton = new GameButton("-")
				.SetVariant(ButtonVariant.Outline)
				.SetSize(ButtonSize.Small)
				.OnClick(() => k_manager.DecreaseSkillRank(skill.Id))
				.Build();
			minusButton.SetEnabled(canDecrease);
			minusButton.style.marginRight = spacing.XS;

			var plusButton = new GameButton("+")
				.SetVariant(ButtonVariant.Outline)
				.SetSize(ButtonSize.Small)
				.OnClick(() => k_manager.IncreaseSkillRank(skill.Id))
				.Build();
			plusButton.SetEnabled(canIncrease);

			rightPanel.AddChild(costLabel);
			rightPanel.AddChild(minusButton);
			rightPanel.AddChild(plusButton);
		} else {
			// Not known, no pending - show learn button
			var costLabel = new GameLabel($"{cost} pts")
				.SetStyle(LabelStyle.Caption)
				.SetColor(canIncrease ? LabelColor.Warning : LabelColor.Error)
				.Build();
			costLabel.style.marginRight = spacing.SM;

			var addButton = new GameButton("Learn")
				.SetVariant(ButtonVariant.Outline)
				.SetSize(ButtonSize.Small)
				.OnClick(() => k_manager.IncreaseSkillRank(skill.Id))
				.Build();
			addButton.SetEnabled(canIncrease);

			rightPanel.AddChild(costLabel);
			rightPanel.AddChild(addButton);
		}

		container.AddChild(leftPanel);
		container.AddChild(rightPanel);

		return container;
	}

	private void BuildSpellsTab() {
		k_spellsScroll.ClearContent();

		if (k_manager == null) return;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		// Check if character can cast spells
		if (!k_manager.CanCastSpells) {
			var noMagic = new GameLabel("This character cannot learn spells")
				.SetStyle(LabelStyle.TitleMedium)
				.SetColor(LabelColor.Tertiary)
				.SetMargin(spacing.LG)
				.Build();
			k_spellsScroll.AddChild(noMagic);
			return;
		}

		var state = k_manager.CurrentState;

		// Known spells section
		var knownHeader = new GameLabel("Known Spells")
			.SetStyle(LabelStyle.TitleMedium)
			.SetMarginBottom(spacing.SM)
			.Build();
		k_spellsScroll.AddChild(knownHeader);

		if (state.Spells.Count == 0 && state.NewSpells.Count == 0) {
			var noSpells = new GameLabel("No spells learned yet")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.SetMarginBottom(spacing.MD)
				.Build();
			k_spellsScroll.AddChild(noSpells);
		} else {
			// Known spells (locked)
			foreach (var spellId in state.Spells) {
				var spell = k_gameDb.GetOrNull<SpellProto>(spellId);
				if (spell == null) continue;

				var row = CreateSpellRow(spell, isKnown: true, isPending: false);
				k_spellsScroll.AddChild(row);
			}

			// Pending spells
			foreach (var spellId in state.NewSpells) {
				var spell = k_gameDb.GetOrNull<SpellProto>(spellId);
				if (spell == null) continue;

				var row = CreateSpellRow(spell, isKnown: false, isPending: true);
				k_spellsScroll.AddChild(row);
			}
		}

		// Divider
		var divider = new GameDivider()
			.SetMargin(spacing.MD, 0)
			.Build();
		k_spellsScroll.AddChild(divider);

		// Available spells section
		var availableHeader = new GameLabel("Available to Learn")
			.SetStyle(LabelStyle.TitleMedium)
			.SetMarginBottom(spacing.SM)
			.Build();
		k_spellsScroll.AddChild(availableHeader);

		var availableSpells = k_manager.GetAvailableSpells();

		if (availableSpells.Count == 0) {
			var noAvailable = new GameLabel("No new spells available at this level")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.Build();
			k_spellsScroll.AddChild(noAvailable);
		} else {
			// Group by school
			var bySchool = availableSpells.GroupBy(s => s.School);
			foreach (var group in bySchool) {
				var schoolLabel = new GameLabel(group.Key.GetDisplayName())
					.SetStyle(LabelStyle.LabelMedium)
					.SetColor(LabelColor.Secondary)
					.SetMarginTop(spacing.SM)
					.SetMarginBottom(spacing.XS)
					.Build();
				k_spellsScroll.AddChild(schoolLabel);

				foreach (var spell in group) {
					var row = CreateSpellRow(spell, isKnown: false, isPending: false);
					k_spellsScroll.AddChild(row);
				}
			}
		}
	}

	private GameContainer CreateSpellRow(SpellProto spell, bool isKnown, bool isPending) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;

		Color bgColor = isKnown ? colors.BackgroundSecondary :
			(isPending ? colors.SurfaceSelected : colors.Surface);

		var container = new GameContainer($"spell-{spell.Id.Value}")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.SM, spacing.MD, spacing.SM, spacing.MD)
			.SetMarginBottom(spacing.XS)
			.SetBackgroundColor(bgColor)
			.SetBorderRadius(borders.RadiusSM)
			.Build();

		// Left: Level badge + name
		var leftPanel = new GameContainer("left")
			.SetRow()
			.SetAlignItems(Align.Center)
			.Build();

		var levelBadge = new GameBadge()
			.SetText(spell.IsCantrip ? "C" : spell.Level.ToString())
			.SetVariant(spell.IsCantrip ? BadgeVariant.Info : BadgeVariant.Default)
			.SetSize(BadgeSize.Small)
			.Build();
		levelBadge.style.marginRight = spacing.XS;

		string nameText = spell.DisplayText.Name;
		if (isPending) nameText += " ✨";

		var nameLabel = new GameLabel(nameText)
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(isPending ? LabelColor.Success : LabelColor.Primary)
			.Build();
		nameLabel.style.color = spell.School.GetColor();

		leftPanel.AddChild(levelBadge);
		leftPanel.AddChild(nameLabel);

		container.AddChild(leftPanel);

		// Right: Controls
		if (isKnown) {
			var lockLabel = new GameLabel("🔒")
				.SetStyle(LabelStyle.BodyMedium)
				.Build();
			container.AddChild(lockLabel);
		} else if (isPending) {
			var removeButton = new GameButton("✕")
				.SetVariant(ButtonVariant.Ghost)
				.SetSize(ButtonSize.Small)
				.OnClick(() => k_manager!.RemoveSpell(spell.Id))
				.Build();
			container.AddChild(removeButton);
		} else {
			int cost = k_manager!.GetSpellCost(spell.Id);
			bool canAdd = k_manager.CanAddSpell(spell.Id);

			var rightPanel = new GameContainer("right")
				.SetRow()
				.SetAlignItems(Align.Center)
				.Build();

			var costLabel = new GameLabel($"{cost} pts")
				.SetStyle(LabelStyle.Caption)
				.SetColor(canAdd ? LabelColor.Warning : LabelColor.Error)
				.Build();
			costLabel.style.marginRight = spacing.SM;

			var addButton = new GameButton("Learn")
				.SetVariant(ButtonVariant.Outline)
				.SetSize(ButtonSize.Small)
				.OnClick(() => k_manager.AddSpell(spell.Id))
				.Build();
			addButton.SetEnabled(canAdd);

			rightPanel.AddChild(costLabel);
			rightPanel.AddChild(addButton);

			container.AddChild(rightPanel);
		}

		return container;
	}

	#endregion

	#region Public Methods

	public void Show() {
		var run = k_session.CurrentRun;
		if (run == null) {
			Debug.LogWarning("LevelUpPanel: No current run");
			return;
		}

		// Create new manager for this level-up session
		k_manager = new LevelUpManager(k_gameDb, run);
		k_manager.OnStateChanged += OnManagerStateChanged;
		k_manager.OnValidationFailed += OnValidationFailed;

		// Update level display
		int currentLevel = run.Level;
		int newLevel = k_manager.NewLevel;
		k_levelLabel.SetText($"Level {currentLevel} → {newLevel}");

		// Build all tabs
		RefreshAllTabs();

		style.display = DisplayStyle.Flex;
		Debug.Log($"LevelUpPanel: Showing with {k_manager.PointsRemaining} creation points");
	}

	public void Hide() {
		if (k_manager != null) {
			k_manager.OnStateChanged -= OnManagerStateChanged;
			k_manager.OnValidationFailed -= OnValidationFailed;
			k_manager = null;
		}

		style.display = DisplayStyle.None;
	}

	#endregion

	#region Event Handlers

	private void OnManagerStateChanged(LevelUpState state) {
		RefreshAllTabs();
	}

	private void OnValidationFailed(string message) {
		GameToast.Show(message, ToastType.Warning);
	}

	private void RefreshAllTabs() {
		if (k_manager == null) return;

		// Update points display
		int points = k_manager.PointsRemaining;
		k_pointsDisplayLabel.SetText($"{points} pts");

		var colors = GameTheme.Current.Colors;
		if (points == 0) {
			k_pointsDisplayLabel.SetColor(LabelColor.Success);
		} else if (points < 5) {
			k_pointsDisplayLabel.SetColor(colors.Warning);
		} else {
			k_pointsDisplayLabel.SetColor(LabelColor.Primary);
		}

		// Rebuild all tabs
		BuildAttributesTab();
		BuildSkillsTab();
		BuildSpellsTab();
		BuildResourcesTab();

		// Update button states
		k_undoButton.SetEnabled(k_manager.CanUndo);
		k_confirmButton.SetEnabled(true); // Always can confirm (unspent points carry over)
	}

	private void OnUndoClicked() {
		k_manager?.Undo();
	}

	private void OnResetClicked() {
		k_manager?.Reset();
	}

	private void OnConfirmClicked() {
		if (k_manager == null) return;

		k_manager.ApplyLevelUp();

		var run = k_session.CurrentRun;
		int newLevel = run?.Level ?? 1;

		Hide();
		OnLevelUpComplete?.Invoke();

		GameToast.Show($"Level up complete! Now level {newLevel}", ToastType.Success);
	}

	private void OnSkipClicked() {
		Hide();
		OnLevelUpSkipped?.Invoke();
	}

	#endregion
}