using RPGGame.Core;
using RPGGame.Core.Prototypes.Village;
using RPGGame.Core.Village;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Village;

/// <summary>
/// UI panel displayed when clicking on an empty plot in the village.
/// Shows available buildings that can be constructed.
/// </summary>
public class EmptyPlotPanel : VisualElement {
	#region Fields

	private readonly VillageManager k_villageManager;
	private readonly UiManager k_uiManager;

	private Vector2Int k_plotPosition;
	private GamePanel k_content = null!;
	private ScrollView k_scrollView = null!;

	#endregion

	#region Events

	public event Action? OnClosed;
	public event Action<BuildingProto.ID, Vector2Int>? OnBuildingSelected;

	#endregion

	#region Constructor

	public EmptyPlotPanel(VillageManager villageManager, UiManager uiManager) {
		k_villageManager = villageManager;
		k_uiManager = uiManager;

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

		// Full overlay
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		style.alignItems = Align.Center;
		style.justifyContent = Justify.Center;
		style.backgroundColor = colors.BackgroundOverlay;

		// Panel container
		k_content = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetPadding(spacing.XL)
			.Build();
		k_content.style.width = dialogStyles.MinWidth;
		k_content.style.maxHeight = Length.Percent(80);

		// Header
		var header = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.Build();

		var titleLabel = new GameLabel("Build New Structure")
			.SetStyle(LabelStyle.HeadlineMedium)
			.SetColor(LabelColor.Primary)
			.Build();

		var closeBtn = new GameButton("✕")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.OnClick(Close)
			.Build();

		header.Content.Add(titleLabel);
		header.Content.Add(closeBtn);

		// Divider
		var divider = new GameDivider().Build();
		divider.style.marginTop = spacing.SM;
		divider.style.marginBottom = spacing.SM;

		// Scrollable building list
		k_scrollView = new ScrollView(ScrollViewMode.Vertical);
		k_scrollView.style.flexGrow = 1;
		k_scrollView.style.maxHeight = dialogStyles.ScrollableContentHeight;

		// Cancel button
		var cancelButton = new GameButton("Cancel")
			.SetVariant(ButtonVariant.Outline)
			.SetFullWidth()
			.OnClick(Close)
			.Build();
		cancelButton.style.marginTop = spacing.MD;

		k_content.Content.Add(header);
		k_content.Content.Add(divider);
		k_content.Content.Add(k_scrollView);
		k_content.Content.Add(cancelButton);

		Add(k_content);

		// Close on background click
		RegisterCallback<ClickEvent>(evt => {
			if (evt.target == this) {
				Close();
			}
		});
	}

	#endregion

	#region Public Methods

	public void Show(Vector2Int plotPosition) {
		k_plotPosition = plotPosition;
		RefreshBuildingList();
		style.display = DisplayStyle.Flex;
	}

	public void Close() {
		style.display = DisplayStyle.None;
		OnClosed?.Invoke();
	}

	#endregion

	#region Content

	private void RefreshBuildingList() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_scrollView.Clear();

		var layout = k_villageManager.Layout;
		if (layout == null) return;

		var availableBuildings = k_villageManager.GetAvailableBuildings()
			.Where(proto => !Ids.Buildings.IsCore(proto.Id))
			.ToList();

		if (availableBuildings.Count == 0) {
			var emptyPanel = new GamePanel()
				.SetVariant(PanelVariant.Ghost)
				.SetAlignment(Align.Center)
				.AddChild(new GameLabel("No buildings available to construct.")
					.SetStyle(LabelStyle.BodyMedium)
					.SetColor(LabelColor.Tertiary)
					.Build())
				.Build();
			emptyPanel.style.paddingTop = spacing.XXXL;
			emptyPanel.style.paddingBottom = spacing.XXXL;
			k_scrollView.Add(emptyPanel);
			return;
		}

		// Group by category
		var byCategory = availableBuildings.GroupBy(b => b.Category);

		foreach (var group in byCategory) {
			// Category header
			var categoryLabel = new GameLabel(group.Key.ToString())
				.SetStyle(LabelStyle.TitleSmall)
				.SetColor(LabelColor.Secondary)
				.Build();
			categoryLabel.style.marginTop = spacing.SM;
			categoryLabel.style.marginBottom = spacing.XS;
			k_scrollView.Add(categoryLabel);

			foreach (var proto in group) {
				var canPlace = layout.CanPlaceBuilding(proto, k_plotPosition);
				var card = CreateBuildingCard(proto, canPlace);
				k_scrollView.Add(card);
			}
		}
	}

	private VisualElement CreateBuildingCard(BuildingProto proto, bool canPlace) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var card = new GameCard()
			.SetHeader(proto.DisplayText.Name)
			.SetSubheader($"Size: {proto.Width}x{proto.Height}")
			.SetVariant(canPlace ? CardVariant.Elevated : CardVariant.Outlined)
			.Build();
		card.style.marginBottom = spacing.XS;

		// Description
		var descLabel = new GameLabel(proto.DisplayText.Description)
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Secondary)
			.SetWhiteSpace(WhiteSpace.Normal)
			.Build();
		descLabel.style.marginBottom = spacing.XS;

		// Bonuses preview
		var bonusPanel = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetWrap(true)
			.Build();

		foreach (var bonus in proto.BonusesPerLevel.Take(3)) {
			var sign = bonus.Value >= 0 ? "+" : "";
			var badge = new GameBadge($"{sign}{bonus.Value} {bonus.Stat}/lvl")
				.SetVariant(BadgeVariant.Info)
				.SetShape(BadgeShape.Compact)
				.Build();
			badge.style.marginRight = spacing.XXS;
			bonusPanel.Content.Add(badge);
		}

		// Build button
		var buildButton = new GameButton("Build")
			.SetVariant(canPlace ? ButtonVariant.Primary : ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.SetEnabled(canPlace)
			.OnClick(() => SelectBuilding(proto.Id))
			.Build();

		if (!canPlace) {
			buildButton.AddTooltip("Not enough space at this location", TooltipPosition.Top);
		}

		// Footer with button
		var footer = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.FlexEnd)
			.Build();
		footer.Content.Add(buildButton);

		card.Content.Add(descLabel);
		card.Content.Add(bonusPanel);
		card.Content.Add(footer);

		return card;
	}

	private void SelectBuilding(BuildingProto.ID buildingId) {
		var building = k_villageManager.PlaceBuilding(buildingId, k_plotPosition);

		if (building != null) {
			GameToast.Show($"Built {building.DisplayName}!", ToastType.Success);
			OnBuildingSelected?.Invoke(buildingId, k_plotPosition);
			Close();
		} else {
			GameToast.Show("Failed to build here.", ToastType.Error);
		}
	}

	#endregion
}