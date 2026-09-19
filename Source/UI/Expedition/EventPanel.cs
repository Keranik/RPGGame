using RPGGame.Core;
using RPGGame.Core.Events;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Rewards;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Expedition;

/// <summary>
/// Modal panel for displaying events and choices during expedition.
/// Shows event title, description, flavor text, and available choices with
/// detailed outcome previews, success chances, and effect breakdowns.
/// </summary>
public class EventPanel : VisualElement {
	#region Fields

	private readonly EventManager k_eventManager;
	private readonly GameSession k_session;
	private readonly GameStateManager k_stateManager;

	private GamePanel k_content = null!;
	private GameLabel k_titleLabel = null!;
	private GameLabel k_descriptionLabel = null!;
	private GameLabel k_flavorLabel = null!;
	private GamePanel k_choicesContainer = null!;
	private GamePanel k_outcomePanel = null!;
	private GameLabel k_outcomeLabel = null!;
	private GameButton k_continueButton = null!;

	// Choice detail panel (shows when hovering/selecting a choice)
	private GamePanel k_choiceDetailPanel = null!;
	private GameLabel k_choiceDetailTitle = null!;
	private GameLabel k_choiceDetailDescription = null!;
	private GameContainer k_successPreview = null!;
	private GameContainer k_failurePreview = null!;
	private VisualElement k_failureSection = null!;
	private GameLabel k_successChanceLabel = null!;

	private EventProto? k_currentEvent;
	private EventChoiceProto? k_hoveredChoice;
	private bool k_combatTriggered;
	private bool k_waitingForReward;
	private int k_pendingRewardCount;

	#endregion

	#region Properties

	public bool IsShowingOutcome => k_outcomePanel.style.display == DisplayStyle.Flex;

	#endregion

	#region Events

	public event Action? OnEventCompleted;
	public event Action<PendingReward, Action<ItemInstance?>>? OnRewardPendingForUI;

	#endregion

	#region Constructor

	public EventPanel(
		EventManager eventManager,
		GameSession session,
		GameStateManager stateManager
	) {
		k_eventManager = eventManager;
		k_session = session;
		k_stateManager = stateManager;

		BuildUI();
		SubscribeToEvents();

		style.display = DisplayStyle.None;
	}

	#endregion

	#region Event Subscriptions

	private void SubscribeToEvents() {
		k_eventManager.OnCombatTriggered += OnCombatTriggeredFromEvent;
		k_eventManager.OnRewardPending += OnRewardPending;
	}

	private void UnsubscribeFromEvents() {
		k_eventManager.OnCombatTriggered -= OnCombatTriggeredFromEvent;
		k_eventManager.OnRewardPending -= OnRewardPending;
	}

	private void OnCombatTriggeredFromEvent(string encounterId) {
		k_combatTriggered = true;
		Hide();
		Debug.Log($"EventPanel: Hidden due to combat trigger ({encounterId})");
	}

	private void OnRewardPending(PendingReward reward, Action<ItemInstance?> onComplete) {
		k_waitingForReward = true;
		k_pendingRewardCount++;
		Debug.Log($"EventPanel: Reward pending, count={k_pendingRewardCount}");

		void wrappedComplete(ItemInstance? item) {
			onComplete(item);
			k_pendingRewardCount--;
			Debug.Log($"EventPanel: Reward claimed, remaining={k_pendingRewardCount}");

			if (k_pendingRewardCount <= 0) {
				k_waitingForReward = false;
				k_pendingRewardCount = 0;
				schedule.Execute(ShowOutcome).ExecuteLater(100);
			}
		}

		OnRewardPendingForUI?.Invoke(reward, wrappedComplete);
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var dialogStyles = theme.Components.Dialog;

		// Full screen overlay
		style.flexGrow = 1f;
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		style.alignItems = Align.Center;
		style.justifyContent = Justify.Center;
		style.backgroundColor = colors.BackgroundOverlay;
		pickingMode = PickingMode.Position;

		// Center container - this stays centered and doesn't move
		var centerContainer = new GameContainer("event-center-container")
			.SetRelative()
			.Build();

		// Main content panel - centered
		k_content = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.XL)
			.SetMinWidth(dialogStyles.MinWidth)
			.SetMaxWidth(dialogStyles.MaxWidth)
			.SetMaxHeight(Length.Percent(85))
			.Build();

		BuildEventContent();

		// Choice detail panel - absolutely positioned to the right of content
		// This way it doesn't affect the layout/position of k_content
		k_choiceDetailPanel = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetPadding(spacing.LG)
			.SetMinWidth(dialogStyles.MinWidth)
			.SetPosition(Position.Absolute)
			.Build();
		k_choiceDetailPanel.style.left = Length.Percent(100); // Position to the right of parent
		k_choiceDetailPanel.style.marginLeft = spacing.LG;
		k_choiceDetailPanel.style.top = 0;
		k_choiceDetailPanel.style.display = DisplayStyle.None;

		// Add drop shadow effect for the floating panel
		k_choiceDetailPanel.style.borderTopWidth = borders.WidthThin;
		k_choiceDetailPanel.style.borderRightWidth = borders.WidthThin;
		k_choiceDetailPanel.style.borderBottomWidth = borders.WidthThin;
		k_choiceDetailPanel.style.borderLeftWidth = borders.WidthThin;
		k_choiceDetailPanel.style.borderTopColor = colors.SurfaceBorder;
		k_choiceDetailPanel.style.borderRightColor = colors.SurfaceBorder;
		k_choiceDetailPanel.style.borderBottomColor = colors.SurfaceBorder;
		k_choiceDetailPanel.style.borderLeftColor = colors.SurfaceBorder;

		BuildChoiceDetailPanel();

		centerContainer.Add(k_content);
		centerContainer.Add(k_choiceDetailPanel);
		Add(centerContainer);
	}

	private void BuildEventContent() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		// Title
		k_titleLabel = new GameLabel("Event Title")
			.SetStyle(LabelStyle.HeadlineMedium)
			.SetColor(LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		var titleDivider = new GameDivider().Build();
		titleDivider.style.marginTop = spacing.MD;
		titleDivider.style.marginBottom = spacing.MD;

		// Description
		k_descriptionLabel = new GameLabel("Event description goes here...")
			.SetStyle(LabelStyle.BodyLarge)
			.SetColor(LabelColor.Primary)
			.SetWhiteSpace(WhiteSpace.Normal)
			.Build();

		// Flavor text
		k_flavorLabel = new GameLabel("Flavor text...")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Tertiary)
			.SetItalic()
			.SetWhiteSpace(WhiteSpace.Normal)
			.SetMarginTop(spacing.SM)
			.Build();

		var choicesDivider = new GameDivider().Build();
		choicesDivider.style.marginTop = spacing.LG;
		choicesDivider.style.marginBottom = spacing.MD;

		// Choices container
		k_choicesContainer = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Vertical)
			.Build();

		// Outcome panel
		k_outcomePanel = new GamePanel()
			.SetVariant(PanelVariant.Outlined)
			.SetPadding(spacing.MD)
			.SetMarginTop(spacing.MD)
			.Build();
		k_outcomePanel.style.display = DisplayStyle.None;

		k_outcomeLabel = new GameLabel("Outcome...")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Primary)
			.SetWhiteSpace(WhiteSpace.Normal)
			.Build();

		k_continueButton = new GameButton("Continue")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.SetFullWidth()
			.OnClick(OnContinueClicked)
			.Build();
		k_continueButton.style.marginTop = spacing.MD;

		k_outcomePanel.Content.Add(k_outcomeLabel);
		k_outcomePanel.Content.Add(k_continueButton);

		// Build hierarchy
		k_content.Content.Add(k_titleLabel);
		k_content.Content.Add(titleDivider);
		k_content.Content.Add(k_descriptionLabel);
		k_content.Content.Add(k_flavorLabel);
		k_content.Content.Add(choicesDivider);
		k_content.Content.Add(k_choicesContainer);
		k_content.Content.Add(k_outcomePanel);
	}

	private void BuildChoiceDetailPanel() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;

		// Title
		k_choiceDetailTitle = new GameLabel("Choice Details")
			.SetStyle(LabelStyle.HeadlineSmall)
			.SetColor(LabelColor.Primary)
			.Build();

		k_choiceDetailDescription = new GameLabel("")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.SetWhiteSpace(WhiteSpace.Normal)
			.SetMarginTop(spacing.SM)
			.Build();

		// Success chance indicator
		k_successChanceLabel = new GameLabel("")
			.SetStyle(LabelStyle.BodyLarge)
			.SetColor(LabelColor.Primary)
			.SetWhiteSpace(WhiteSpace.Normal)
			.SetMarginTop(spacing.MD)
			.Build();
		k_successChanceLabel.style.display = DisplayStyle.None;

		// Success outcome preview section
		var successHeader = new GameLabel("✓ On Success:")
			.SetStyle(LabelStyle.LabelMedium)
			.SetColor(LabelColor.Success)
			.SetMarginTop(spacing.LG)
			.Build();

		k_successPreview = new GameContainer("success-preview")
			.SetColumn()
			.SetMarginLeft(spacing.SM)
			.SetPaddingLeft(spacing.SM)
			.SetBorderLeftOnly(borders.WidthMedium)
			.SetBorderColor(colors.Success)
			.Build();

		// Failure outcome preview section
		k_failureSection = new VisualElement();
		k_failureSection.style.display = DisplayStyle.None;

		var failureHeader = new GameLabel("✗ On Failure:")
			.SetStyle(LabelStyle.LabelMedium)
			.SetColor(LabelColor.Error)
			.SetMarginTop(spacing.MD)
			.Build();

		k_failurePreview = new GameContainer("failure-preview")
			.SetColumn()
			.SetMarginLeft(spacing.SM)
			.SetPaddingLeft(spacing.SM)
			.SetBorderLeftOnly(borders.WidthMedium)
			.SetBorderColor(colors.Error)
			.Build();

		k_failureSection.Add(failureHeader);
		k_failureSection.Add(k_failurePreview);

		k_choiceDetailPanel.Content.Add(k_choiceDetailTitle);
		k_choiceDetailPanel.Content.Add(k_choiceDetailDescription);
		k_choiceDetailPanel.Content.Add(k_successChanceLabel);
		k_choiceDetailPanel.Content.Add(successHeader);
		k_choiceDetailPanel.Content.Add(k_successPreview);
		k_choiceDetailPanel.Content.Add(k_failureSection);
	}

	#endregion

	#region Public Methods

	public void ShowEvent(EventProto evt) {
		k_currentEvent = evt;
		k_combatTriggered = false;
		k_waitingForReward = false;
		k_pendingRewardCount = 0;
		k_hoveredChoice = null;

		k_titleLabel.SetText(evt.Title);
		k_descriptionLabel.SetText(evt.Description);

		if (!string.IsNullOrEmpty(evt.FlavorText)) {
			k_flavorLabel.SetText($"„ {evt.FlavorText}");
			k_flavorLabel.style.display = DisplayStyle.Flex;
		} else {
			k_flavorLabel.style.display = DisplayStyle.None;
		}

		BuildChoiceButtons(evt);

		k_choicesContainer.style.display = DisplayStyle.Flex;
		k_outcomePanel.style.display = DisplayStyle.None;
		k_choiceDetailPanel.style.display = DisplayStyle.None;

		style.display = DisplayStyle.Flex;

		Debug.Log($"EventPanel: Showing event '{evt.Title}'");
	}

	public void Hide() {
		style.display = DisplayStyle.None;
		k_currentEvent = null;
		k_hoveredChoice = null;
	}

	#endregion

	#region Choice Building

	private void BuildChoiceButtons(EventProto evt) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_choicesContainer.ClearChildren();

		var runState = k_session.CurrentRun;
		var meta = k_session.MetaProgression;
		if (runState == null) return;

		var eventState = k_eventManager.GetEventState(runState, meta);

		foreach (var choiceAvail in eventState.AvailableChoices) {
			var choice = choiceAvail.Choice;

			// Build the choice row
			var choiceRow = new GameContainer($"choice-{choice.ChoiceId}")
				.SetColumn()
				.SetMarginBottom(spacing.SM)
				.Build();

			// Main button
			var buttonText = BuildChoiceButtonText(choice, runState);
			var choiceButton = new GameButton(buttonText)
				.SetVariant(GetChoiceButtonVariant(choice, choiceAvail.IsAvailable))
				.SetSize(ButtonSize.Large)
				.SetFullWidth()
				.OnClick(() => OnChoiceSelected(choice))
				.Build();

			choiceButton.SetEnabled(choiceAvail.IsAvailable);

			// Add hover events to show details
			choiceButton.RegisterCallback<MouseEnterEvent>(_ => ShowChoiceDetails(choice, runState));
			choiceButton.RegisterCallback<MouseLeaveEvent>(_ => HideChoiceDetails());

			// Add tooltip for unavailable choices
			if (!choiceAvail.IsAvailable && !string.IsNullOrEmpty(choiceAvail.LockedReason)) {
				choiceButton.tooltip = choiceAvail.LockedReason;
			}

			choiceRow.Add(choiceButton);

			// Add inline summary for choices with skill checks or significant effects
			var summaryLabel = BuildChoiceSummaryLabel(choice, runState);
			if (summaryLabel != null) {
				summaryLabel.style.marginTop = spacing.XS;
				summaryLabel.style.marginLeft = spacing.SM;
				choiceRow.Add(summaryLabel);
			}

			k_choicesContainer.AddChild(choiceRow);
		}

		// Skip button
		if (evt.CanSkip) {
			var skipButton = new GameButton("Skip")
				.SetVariant(ButtonVariant.Ghost)
				.SetSize(ButtonSize.Medium)
				.OnClick(OnSkipClicked)
				.Build();
			skipButton.style.marginTop = spacing.SM;
			k_choicesContainer.AddChild(skipButton);
		}
	}

	private string BuildChoiceButtonText(EventChoiceProto choice, RunState runState) {
		var text = choice.Text;

		// Add cost indicator
		if (choice.Cost != null) {
			text += $" [{choice.Cost.Type}: {choice.Cost.Amount}]";
		}

		// Add skill check indicator with success chance
		if (choice.Condition?.Type == ConditionType.SkillCheck) {
			var chance = CalculateSkillCheckChance(choice.Condition, runState);
			text += $" ({chance.DescriptiveName})";
		}

		return text;
	}

	private static ButtonVariant GetChoiceButtonVariant(EventChoiceProto choice, bool isAvailable) {
		if (!isAvailable) return ButtonVariant.Ghost;

		// Check if this is a risky choice (has failure outcome with negative effects)
		if (choice.FailureOutcome != null && HasNegativeEffects(choice.FailureOutcome)) {
			return ButtonVariant.Outline; // Risky choices use outline
		}

		// Check if it triggers combat
		if (choice.SuccessOutcome?.Effects.Any(e => e.Type == OutcomeEffectType.StartCombat) == true) {
			return ButtonVariant.Secondary; // Combat choices
		}

		return ButtonVariant.Primary;
	}

	private GameLabel? BuildChoiceSummaryLabel(EventChoiceProto choice, RunState runState) {
		var parts = new List<string>();

		// Skill check chance
		if (choice.Condition?.Type == ConditionType.SkillCheck) {
			var chance = CalculateSkillCheckChance(choice.Condition, runState);
			var modifier = GetSkillCheckModifier(choice.Condition, runState);
			var sign = modifier >= 0 ? "+" : "";
			parts.Add($"🎲 d20{sign}{modifier} vs DC {(int)choice.Condition.Value} ({chance})");
		}

		// Quick effect summary
		var effectSummary = BuildQuickEffectSummary(choice);
		if (!string.IsNullOrEmpty(effectSummary)) {
			parts.Add(effectSummary);
		}

		if (parts.Count == 0) return null;

		return new GameLabel(string.Join(" • ", parts))
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Tertiary)
			.Build();
	}

	private static string BuildQuickEffectSummary(EventChoiceProto choice) {
		var parts = new List<string>();

		// Check success outcome
		if (choice.SuccessOutcome?.Effects != null) {
			foreach (var effect in choice.SuccessOutcome.Effects.Where(e => e.ShowInUI)) {
				var icon = GetEffectIcon(effect);
				if (!string.IsNullOrEmpty(icon)) {
					parts.Add(icon);
				}
			}
		}

		return string.Join(" ", parts.Take(4)); // Limit to 4 icons
	}

	private static string GetEffectIcon(OutcomeEffect effect) {
		return effect.Type switch {
			OutcomeEffectType.GainGold => $"💰+{effect.Value}",
			OutcomeEffectType.LoseGold => $"💰-{effect.Value}",
			OutcomeEffectType.Heal => $"❤️+{effect.Value}",
			OutcomeEffectType.Damage => $"💔-{effect.Value}",
			OutcomeEffectType.GainFood => $"🍖+{effect.Value}",
			OutcomeEffectType.LoseFood => $"🍖-{effect.Value}",
			OutcomeEffectType.GainExperience => $"⭐+{effect.Value}",
			OutcomeEffectType.GainMorale => $"😊+{effect.Value}",
			OutcomeEffectType.LoseMorale => $"😟-{effect.Value}",
			OutcomeEffectType.StartCombat => "⚔️",
			OutcomeEffectType.GainClassWeapon or OutcomeEffectType.GainClassArmor => "🎁",
			OutcomeEffectType.ApplyBuff => "✨",
			OutcomeEffectType.ApplyDebuff => "💀",
			_ => ""
		};
	}

	#endregion

	#region Choice Details

	private void ShowChoiceDetails(EventChoiceProto choice, RunState runState) {
		k_hoveredChoice = choice;
		k_choiceDetailPanel.style.display = DisplayStyle.Flex;

		// Title
		k_choiceDetailTitle.SetText(choice.Text);

		// Description
		if (!string.IsNullOrEmpty(choice.Description)) {
			k_choiceDetailDescription.SetText(choice.Description);
			k_choiceDetailDescription.style.display = DisplayStyle.Flex;
		} else {
			k_choiceDetailDescription.style.display = DisplayStyle.None;
		}

		// Skill check info
		if (choice.Condition?.Type == ConditionType.SkillCheck) {
			var chance = CalculateSkillCheckChance(choice.Condition, runState);
			var modifier = GetSkillCheckModifier(choice.Condition, runState);
			var dc = (int)choice.Condition.Value;

			var sign = modifier >= 0 ? "+" : "";
			var chanceColor = GetChanceColor(chance);

			k_successChanceLabel.SetText(
				$"🎲 {choice.Condition.Parameter} Check: d20{sign}{modifier} vs DC {dc}\n" +
				$"Success Chance: <color={chanceColor}>{chance} ({chance:odds})</color>"
			);
			k_successChanceLabel.style.display = DisplayStyle.Flex;
		} else {
			k_successChanceLabel.style.display = DisplayStyle.None;
		}

		// Success preview
		k_successPreview.Clear();
		if (choice.SuccessOutcome != null) {
			BuildOutcomePreview(k_successPreview, choice.SuccessOutcome);
		} else {
			k_successPreview.Add(CreateEffectLabel("No specific outcome", LabelColor.Tertiary));
		}

		// Failure preview
		k_failurePreview.Clear();
		if (choice.FailureOutcome != null) {
			BuildOutcomePreview(k_failurePreview, choice.FailureOutcome);
			k_failureSection.style.display = DisplayStyle.Flex;
		} else if (choice.Condition?.Type == ConditionType.SkillCheck) {
			k_failurePreview.Add(CreateEffectLabel("Nothing happens", LabelColor.Tertiary));
			k_failureSection.style.display = DisplayStyle.Flex;
		} else {
			// No failure outcome and no skill check - hide failure section
			k_failureSection.style.display = DisplayStyle.None;
		}
	}

	private void HideChoiceDetails() {
		k_hoveredChoice = null;
		k_choiceDetailPanel.style.display = DisplayStyle.None;
	}

	private void BuildOutcomePreview(VisualElement container, EventOutcomeProto outcome) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		// Message preview
		if (!string.IsNullOrEmpty(outcome.Message)) {
			var messageLabel = new GameLabel($"\"{outcome.Message}\"")
				.SetStyle(LabelStyle.BodySmall)
				.SetColor(LabelColor.Secondary)
				.SetItalic()
				.SetWhiteSpace(WhiteSpace.Normal)
				.SetMarginBottom(spacing.XS)
				.Build();
			container.Add(messageLabel);
		}

		// Effects
		foreach (var effect in outcome.Effects.Where(e => e.ShowInUI)) {
			var effectText = effect.GetDisplayText();
			if (string.IsNullOrEmpty(effectText)) continue;

			var color = GetEffectLabelColor(effect);
			var icon = GetEffectTypeIcon(effect.Type);
			container.Add(CreateEffectLabel($"{icon} {effectText}", color));
		}

		// If no visible effects
		if (!outcome.Effects.Any(e => e.ShowInUI)) {
			container.Add(CreateEffectLabel("(No visible effects)", LabelColor.Tertiary));
		}
	}

	private static GameLabel CreateEffectLabel(string text, LabelColor color) {
		return new GameLabel(text)
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(color)
			.Build();
	}

	private static LabelColor GetEffectLabelColor(OutcomeEffect effect) {
		return effect.Type switch {
			OutcomeEffectType.GainGold or OutcomeEffectType.GainFood or OutcomeEffectType.GainItem
				or OutcomeEffectType.Heal or OutcomeEffectType.RestoreMana or OutcomeEffectType.GainExperience
				or OutcomeEffectType.GainMorale or OutcomeEffectType.ApplyBuff or OutcomeEffectType.GainClassWeapon
				or OutcomeEffectType.GainClassArmor or OutcomeEffectType.GainClassSpell
				=> LabelColor.Success,

			OutcomeEffectType.LoseGold or OutcomeEffectType.LoseFood or OutcomeEffectType.LoseItem
				or OutcomeEffectType.Damage or OutcomeEffectType.DrainMana or OutcomeEffectType.LoseMorale
				or OutcomeEffectType.AddFatigue or OutcomeEffectType.ApplyDebuff
				=> LabelColor.Error,

			OutcomeEffectType.StartCombat => LabelColor.Warning,

			_ => LabelColor.Secondary
		};
	}

	private static string GetEffectTypeIcon(OutcomeEffectType type) {
		return type switch {
			OutcomeEffectType.GainGold or OutcomeEffectType.LoseGold => "💰",
			OutcomeEffectType.GainFood or OutcomeEffectType.LoseFood => "🍖",
			OutcomeEffectType.GainItem or OutcomeEffectType.LoseItem => "📦",
			OutcomeEffectType.Heal => "❤️",
			OutcomeEffectType.Damage => "💔",
			OutcomeEffectType.RestoreMana or OutcomeEffectType.DrainMana => "💧",
			OutcomeEffectType.GainExperience => "⭐",
			OutcomeEffectType.GainMorale or OutcomeEffectType.LoseMorale => "😊",
			OutcomeEffectType.AddFatigue => "😴",
			OutcomeEffectType.ApplyBuff => "✨",
			OutcomeEffectType.ApplyDebuff => "💀",
			OutcomeEffectType.StartCombat => "⚔️",
			OutcomeEffectType.UnlockLore => "📜",
			OutcomeEffectType.AdvanceTime => "⏰",
			OutcomeEffectType.GainClassWeapon => "🗡️",
			OutcomeEffectType.GainClassArmor => "🛡️",
			OutcomeEffectType.GainClassSpell => "📖",
			_ => "•"
		};
	}

	#endregion

	#region Skill Check Calculations

	private ChanceRPG CalculateSkillCheckChance(EventCondition condition, RunState runState) {
		int modifier = GetSkillCheckModifier(condition, runState);
		int dc = (int)condition.Value;

		// Use DiceRoller to calculate the success chance
		return DiceRoller.GetD20SuccessChance(modifier, dc);
	}

	private static int GetSkillCheckModifier(EventCondition condition, RunState runState) {
		var protoId = new Proto.ID(condition.Parameter);

		if (protoId.IsPrimaryAttribute()) {
			var statId = new StatProto.ID(condition.Parameter);
			return StatManager.GetAttributeModifier(runState.Stats.Get(statId));
		}

		if (protoId.IsStat()) {
			var statId = new StatProto.ID(condition.Parameter);
			return runState.Stats.GetInt(statId);
		}

		// TODO: Handle skills
		return 0;
	}

	private static string GetChanceColor(ChanceRPG chance) {
		return chance.Probability switch {
			>= 0.8f => "#22C55E", // Green - very likely
			>= 0.6f => "#84CC16", // Lime - likely
			>= 0.4f => "#EAB308", // Yellow - even odds
			>= 0.2f => "#F97316", // Orange - unlikely
			_ => "#EF4444"        // Red - very unlikely
		};
	}

	private static bool HasNegativeEffects(EventOutcomeProto outcome) {
		return outcome.Effects.Any(e =>
			e.Type is OutcomeEffectType.Damage or OutcomeEffectType.LoseGold or OutcomeEffectType.LoseFood
				or OutcomeEffectType.LoseItem or OutcomeEffectType.DrainMana or OutcomeEffectType.LoseMorale
				or OutcomeEffectType.AddFatigue or OutcomeEffectType.ApplyDebuff);
	}

	#endregion

	#region Event Handlers

	private void OnChoiceSelected(EventChoiceProto choice) {
		var runState = k_session.CurrentRun;
		var meta = k_session.MetaProgression;
		if (runState == null || k_currentEvent == null) return;

		k_choicesContainer.style.display = DisplayStyle.None;
		k_choiceDetailPanel.style.display = DisplayStyle.None;

		if (k_eventManager.SelectChoice(choice, runState, meta)) {
			if (k_combatTriggered) return;
			if (k_waitingForReward) {
				Debug.Log("EventPanel: Waiting for reward spinner...");
				return;
			}
			ShowOutcome();
		} else {
			k_choicesContainer.style.display = DisplayStyle.Flex;
		}
	}

	private void ShowOutcome() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;

		var outcome = k_eventManager.CurrentOutcome;
		if (outcome != null) {
			// Build detailed outcome message
			var outcomeText = outcome.Message;

			// Add effect summary
			var effectLines = outcome.Effects
				.Where(e => e.ShowInUI)
				.Select(e => $"  {GetEffectTypeIcon(e.Type)} {e.GetDisplayText()}")
				.ToList();

			if (effectLines.Count > 0) {
				outcomeText += "\n\n" + string.Join("\n", effectLines);
			}

			k_outcomeLabel.SetText(outcomeText);

			bool isSuccess = outcome == k_eventManager.SelectedChoice?.SuccessOutcome;
			k_outcomePanel.style.borderLeftColor = isSuccess ? colors.Success : colors.Error;
			k_outcomePanel.style.borderLeftWidth = borders.WidthThick;
		} else {
			k_outcomeLabel.SetText("The moment passes...");
		}

		k_outcomePanel.style.display = DisplayStyle.Flex;
	}

	private void OnContinueClicked() {
		if (k_combatTriggered) {
			Debug.Log("EventPanel: Continue clicked but combat already triggered, ignoring");
			return;
		}

		if (k_waitingForReward) {
			Debug.Log("EventPanel: Continue clicked but still waiting for rewards");
			return;
		}

		var runState = k_session.CurrentRun;
		var meta = k_session.MetaProgression;
		if (runState == null) return;

		k_eventManager.CompleteEvent(runState, meta);
		Hide();
		OnEventCompleted?.Invoke();
	}

	private void OnSkipClicked() {
		var runState = k_session.CurrentRun;
		var meta = k_session.MetaProgression;
		if (runState == null) return;

		k_eventManager.SkipEvent(runState, meta);
		Hide();
		OnEventCompleted?.Invoke();
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		UnsubscribeFromEvents();
		base.RemoveFromHierarchy();
	}

	#endregion
}