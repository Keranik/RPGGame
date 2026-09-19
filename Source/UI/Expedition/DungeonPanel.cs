using RPGGame.Core.Dungeons;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Expedition;

/// <summary>
/// Modal panel for dungeon encounters during expedition.
/// Shows dungeon info and "Coming Soon" message for unimplemented dungeons.
/// </summary>
public class DungeonPanel : VisualElement {
	#region Fields

	private readonly DungeonManager k_dungeonManager;

	private GamePanel k_content = null!;
	private GameLabel k_titleLabel = null!;
	private GameLabel k_descriptionLabel = null!;
	private GamePanel k_statsPanel = null!;
	private GameLabel k_comingSoonLabel = null!;
	private GamePanel k_actionsPanel = null!;
	private GameButton k_enterButton = null!;
	private GameButton k_leaveButton = null!;

	private DungeonInfo? k_currentInfo;

	#endregion

	#region Events

	public event Action? OnDungeonDismissed;
	public event Action? OnDungeonEntered;

	#endregion

	#region Constructor

	public DungeonPanel(DungeonManager dungeonManager) {
		k_dungeonManager = dungeonManager;

		BuildUI();
		SubscribeToEvents();

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

		// Main content panel
		k_content = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.XL)
			.Build();
		k_content.style.width = dialogStyles.SmallWidth;
		k_content.style.maxWidth = Length.Percent(90);

		// Icon
		var iconLabel = new GameLabel("🏰")
			.SetStyle(LabelStyle.DisplayMedium)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		// Title
		k_titleLabel = new GameLabel("Dungeon Name")
			.SetStyle(LabelStyle.HeadlineMedium)
			.SetColor(LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();
		k_titleLabel.style.marginTop = spacing.XS;

		// Description
		k_descriptionLabel = new GameLabel("Description goes here...")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetWhiteSpace(WhiteSpace.Normal)
			.Build();
		k_descriptionLabel.style.marginTop = spacing.SM;

		// Divider
		var divider1 = new GameDivider().Build();
		divider1.style.marginTop = spacing.MD;
		divider1.style.marginBottom = spacing.MD;

		// Stats panel
		BuildStatsPanel();

		// Divider
		var divider2 = new GameDivider().Build();
		divider2.style.marginTop = spacing.MD;
		divider2.style.marginBottom = spacing.MD;

		// Coming soon message
		k_comingSoonLabel = new GameLabel("Coming Soon!")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Warning)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetWhiteSpace(WhiteSpace.Normal)
			.Build();
		k_comingSoonLabel.style.display = DisplayStyle.None;

		// Actions
		BuildActionsPanel();

		// Build hierarchy
		k_content.Content.Add(iconLabel);
		k_content.Content.Add(k_titleLabel);
		k_content.Content.Add(k_descriptionLabel);
		k_content.Content.Add(divider1);
		k_content.Content.Add(k_statsPanel);
		k_content.Content.Add(divider2);
		k_content.Content.Add(k_comingSoonLabel);
		k_content.Content.Add(k_actionsPanel);

		Add(k_content);
	}

	private void BuildStatsPanel() {
		k_statsPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceAround)
			.Build();
	}

	private void BuildActionsPanel() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_actionsPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.Center)
			.Build();

		k_enterButton = new GameButton("🚪 Enter Dungeon")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.OnClick(OnEnterClicked)
			.Build();

		k_leaveButton = new GameButton("🚶 Leave")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Large)
			.OnClick(OnLeaveClicked)
			.Build();
		k_leaveButton.style.marginLeft = spacing.MD;

		k_actionsPanel.Content.Add(k_enterButton);
		k_actionsPanel.Content.Add(k_leaveButton);
	}

	#endregion

	#region Event Subscriptions

	private void SubscribeToEvents() {
		k_dungeonManager.OnDungeonComingSoon += OnDungeonComingSoon;
		k_dungeonManager.OnDungeonDismissed += OnDungeonDismissedInternal;
	}

	#endregion

	#region Public Methods

	public void ShowDungeon(DungeonInfo info) {
		k_currentInfo = info;

		k_titleLabel.SetText(info.Name);
		k_descriptionLabel.SetText(info.Description);

		// Update stats
		BuildStatDisplays(info);

		// Show/hide coming soon
		if (!info.IsImplemented) {
			k_comingSoonLabel.SetText(info.ComingSoonMessage);
			k_comingSoonLabel.style.display = DisplayStyle.Flex;
			k_enterButton.SetEnabled(false);
			k_enterButton.SetText("🚧 Coming Soon");
		} else {
			k_comingSoonLabel.style.display = DisplayStyle.None;
			k_enterButton.SetEnabled(true);
			k_enterButton.SetText("🚪 Enter Dungeon");
		}

		style.display = DisplayStyle.Flex;

		Debug.Log($"DungeonPanel: Showing dungeon '{info.Name}'");
	}

	public void Hide() {
		style.display = DisplayStyle.None;
		k_currentInfo = null;
	}

	#endregion

	#region Private Methods

	private void BuildStatDisplays(DungeonInfo info) {
		k_statsPanel.ClearChildren();

		// Difficulty
		var diffPanel = CreateStatDisplay("⚔", "Difficulty", GetDifficultyStars(info.DifficultyTier));
		k_statsPanel.Content.Add(diffPanel);

		// Level
		var levelPanel = CreateStatDisplay("📊", "Level", $"{info.RecommendedLevel}+");
		k_statsPanel.Content.Add(levelPanel);

		// Floors
		var floorPanel = CreateStatDisplay("🏛", "Floors", info.FloorCount.ToString());
		k_statsPanel.Content.Add(floorPanel);

		// Boss
		if (info.HasBoss) {
			var bossPanel = CreateStatDisplay("👹", "Boss", "Yes");
			k_statsPanel.Content.Add(bossPanel);
		}
	}

	private GamePanel CreateStatDisplay(string icon, string label, string value) {
		var panel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Vertical)
			.SetAlignment(Align.Center)
			.Build();

		var iconLabel = new GameLabel(icon)
			.SetStyle(LabelStyle.TitleLarge)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		var valueLabel = new GameLabel(value)
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		var nameLabel = new GameLabel(label)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		panel.Content.Add(iconLabel);
		panel.Content.Add(valueLabel);
		panel.Content.Add(nameLabel);

		return panel;
	}

	private string GetDifficultyStars(int tier) {
		return tier switch {
			1 => "★☆☆☆☆",
			2 => "★★☆☆☆",
			3 => "★★★☆☆",
			4 => "★★★★☆",
			5 => "★★★★★",
			_ when tier > 5 => "★★★★★+",
			_ => "☆☆☆☆☆"
		};
	}

	#endregion

	#region Event Handlers

	private void OnDungeonComingSoon(DungeonInfo info) {
		ShowDungeon(info);
	}

	private void OnDungeonDismissedInternal() {
		Hide();
	}

	private void OnEnterClicked() {
		if (k_currentInfo != null && k_currentInfo.IsImplemented) {
			if (k_dungeonManager.TryEnterDungeon()) {
				Hide();
				OnDungeonEntered?.Invoke();
			}
		} else {
			GameToast.Show("Dungeon exploration coming soon!", ToastType.Info);
		}
	}

	private void OnLeaveClicked() {
		k_dungeonManager.DismissDungeon();
		Hide();
		OnDungeonDismissed?.Invoke();
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		k_dungeonManager.OnDungeonComingSoon -= OnDungeonComingSoon;
		k_dungeonManager.OnDungeonDismissed -= OnDungeonDismissedInternal;
		base.RemoveFromHierarchy();
	}

	#endregion
}