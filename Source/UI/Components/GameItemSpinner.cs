using RPGGame.Core.Generation;
using RPGGame.Core.Items;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// A specialized loot spinner that wraps GameSpinner for GeneratedItem display.
/// Shows procedurally generated items with rarity frames spinning before landing on the result.
/// </summary>
public class GameItemSpinner : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly GameSpinner<GeneratedItem> k_spinner;
	private readonly VisualElement k_resultPanel;
	private readonly Label k_resultLabel;
	private readonly Label k_resultSubLabel;

	private List<GeneratedItem> k_items = [];
	private int k_winningIndex = 0;

	private Action<GeneratedItem>? k_onSpinComplete;
	private Action? k_onSpinStart;

	#endregion

	#region Properties

	public bool IsSpinning => k_spinner.IsSpinning;

	public GeneratedItem? WinningItem => k_winningIndex >= 0 && k_winningIndex < k_items.Count
		? k_items[k_winningIndex]
		: null;

	#endregion

	#region Constructor

	public GameItemSpinner() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var spinnerStyles = theme.Components.Spinner;

		k_container = new VisualElement {
			name = "item-spinner-container",
			style = {
				flexDirection = FlexDirection.Column,
				alignItems = Align.Center
			}
		};

		// Configure spinner with theme values
		k_spinner = new GameSpinner<GeneratedItem>()
			.SetOrientation(SpinnerOrientation.Horizontal)
			.SetVisibleItems(spinnerStyles.DefaultVisibleItems)
			.SetItemSize(spinnerStyles.ItemSlotSize)
			.SetSpinDuration(2500)
			.SetExtraSpins(3)
			.SetItemTemplate(CreateItemElement)
			.OnSpinComplete(OnInternalSpinComplete)
			.OnSpinStart(OnInternalSpinStart);

		// Result panel (shown after spin)
		k_resultPanel = new VisualElement {
			name = "result-panel",
			style = {
				display = DisplayStyle.None,
				marginTop = spacing.MD,
				alignItems = Align.Center
			}
		};

		k_resultLabel = new Label();
		theme.Typography.TitleMedium.ApplyTo(k_resultLabel.style);
		k_resultLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

		k_resultSubLabel = new Label();
		theme.Typography.BodySmall.ApplyTo(k_resultSubLabel.style);
		k_resultSubLabel.style.marginTop = spacing.XXS;

		k_resultPanel.Add(k_resultLabel);
		k_resultPanel.Add(k_resultSubLabel);

		k_container.Add(k_spinner);
		k_container.Add(k_resultPanel);
		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API

	public GameItemSpinner SetItems(List<GeneratedItem> items) {
		k_items = items ?? [];
		k_spinner.SetItems(k_items);
		return this;
	}

	public GameItemSpinner SetWinningIndex(int index) {
		k_winningIndex = Math.Clamp(index, 0, Math.Max(0, k_items.Count - 1));
		return this;
	}

	public GameItemSpinner SetSpinDuration(int durationMs) {
		k_spinner.SetSpinDuration(durationMs);
		return this;
	}

	public GameItemSpinner SetExtraSpins(int spins) {
		k_spinner.SetExtraSpins(spins);
		return this;
	}

	public GameItemSpinner SetItemSize(float size) {
		k_spinner.SetItemSize(size);
		return this;
	}

	public GameItemSpinner SetVisibleItems(int count) {
		k_spinner.SetVisibleItems(count);
		return this;
	}

	public GameItemSpinner SetSize(float width, float height) {
		k_spinner.SetSize(width, height);
		return this;
	}

	public GameItemSpinner OnSpinComplete(Action<GeneratedItem> callback) {
		k_onSpinComplete = callback;
		return this;
	}

	public GameItemSpinner OnSpinStart(Action callback) {
		k_onSpinStart = callback;
		return this;
	}

	#endregion

	#region Build

	public GameItemSpinner Build() {
		k_spinner.Build();
		ApplyTheme();
		return this;
	}

	#endregion

	#region Spin Control

	public void Spin() {
		if (k_items.Count == 0) return;

		k_resultPanel.style.display = DisplayStyle.None;
		k_spinner.SpinTo(k_winningIndex);
	}

	public void SpinRandom() {
		if (k_items.Count == 0) return;

		k_winningIndex = UnityEngine.Random.Range(0, k_items.Count);
		Spin();
	}

	public void SpinWeighted(Func<GeneratedItem, float> weightSelector) {
		if (k_items.Count == 0) return;

		k_resultPanel.style.display = DisplayStyle.None;
		k_spinner.SpinToWeighted(weightSelector);
	}

	public void ShowResultImmediate() {
		if (k_items.Count == 0) return;

		k_spinner.SetImmediateIndex(k_winningIndex);
		ShowResult(k_items[k_winningIndex]);
	}

	#endregion

	#region Item Template

	private VisualElement CreateItemElement(GeneratedItem item) {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var typography = theme.Typography;
		var spinnerStyles = theme.Components.Spinner;

		var container = new VisualElement {
			style = {
				alignItems = Align.Center,
				justifyContent = Justify.Center,
				width = Length.Percent(100),
				height = Length.Percent(100)
			}
		};

		var frameSize = spinnerStyles.IconSize;

		var frame = new GameRarityFrame()
			.SetRarity(item.Rarity)
			.SetSize(frameSize)
			.SetShowGlow(item.Rarity >= RarityType.Rare)
			.Build();

		// Item initials inside frame - apply typography
		var nameLabel = new Label(GetItemInitials(item.DisplayName));
		typography.TitleMedium.ApplyTo(nameLabel.style);
		nameLabel.style.color = colors.TextPrimary;
		nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
		frame.Content.Add(nameLabel);

		container.Add(frame);
		return container;
	}

	private static string GetItemInitials(string name) {
		if (string.IsNullOrEmpty(name)) return "?";

		var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (words.Length == 1) {
			return words[0].Length > 2 ? words[0][..2].ToUpper() : words[0].ToUpper();
		}

		return string.Concat(words.Take(2).Select(w => char.ToUpper(w[0])));
	}

	#endregion

	#region Internal Callbacks

	private void OnInternalSpinStart() {
		k_onSpinStart?.Invoke();
	}

	private void OnInternalSpinComplete(GeneratedItem item, int index) {
		k_winningIndex = index;
		ShowResult(item);
		k_onSpinComplete?.Invoke(item);
	}

	private void ShowResult(GeneratedItem item) {
		var theme = GameTheme.Current;
		var colors = theme.Colors;

		k_resultLabel.text = item.DisplayName;
		k_resultLabel.style.color = colors.GetRarityColor(item.Rarity);

		k_resultSubLabel.text = $"{item.Rarity} {GetItemTypeName(item)}";
		k_resultSubLabel.style.color = colors.TextSecondary;

		k_resultPanel.style.display = DisplayStyle.Flex;
	}

	private static string GetItemTypeName(GeneratedItem item) {
		return item switch {
			GeneratedWeapon weapon => weapon.BaseProto.WeaponType.ToString(),
			GeneratedArmor armor => armor.BaseProto.ArmorType.ToString(),
			_ => "Item"
		};
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spacing = theme.Spacing;

		k_container.style.backgroundColor = colors.BackgroundSecondary;
		borders.ApplyRadius(k_container.style, borders.RadiusMD);
		k_container.style.paddingLeft = spacing.MD;
		k_container.style.paddingRight = spacing.MD;
		k_container.style.paddingTop = spacing.MD;
		k_container.style.paddingBottom = spacing.MD;
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();

		theme.Typography.TitleMedium.ApplyTo(k_resultLabel.style);
		k_resultLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

		theme.Typography.BodySmall.ApplyTo(k_resultSubLabel.style);
		k_resultSubLabel.style.marginTop = theme.Spacing.XXS;
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		k_spinner.RemoveFromHierarchy();
		base.RemoveFromHierarchy();
	}

	#endregion
}