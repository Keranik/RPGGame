using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A spinning reel component for slot machines, gacha, and random selection displays.
/// Supports vertical/horizontal scrolling with easing animations.
/// </summary>
public class GameSpinner<T> : VisualElement {
	#region Private Fields

	// Structure
	private readonly VisualElement k_container;
	private readonly VisualElement k_viewport;
	private readonly VisualElement k_strip;
	private readonly VisualElement k_highlightOverlay;

	// Data
	private IList<T> k_items = new List<T>();
	private Func<T, VisualElement> k_itemTemplate;

	// Configuration (set from theme or fluent API)
	private SpinnerOrientation k_orientation = SpinnerOrientation.Vertical;
	private int k_visibleItems;
	private float k_itemSlotSize;  // Size in the scroll direction
	private float k_width;         // Total container width

	// Animation settings
	private int k_spinDurationMs = 2000;
	private int k_extraSpins = 3;

	// State
	private int k_currentIndex = 0;
	private bool k_isSpinning = false;
	private float k_currentOffset = 0;

	// Animation state
	private IVisualElementScheduledItem k_spinAnimation;
	private float k_startOffset = 0;
	private float k_targetOffset = 0;
	private float k_spinStartTime;
	private int k_targetIndex;

	// Callbacks
	private Action<T, int> k_onSpinComplete;
	private Action k_onSpinStart;

	#endregion

	#region Properties

	public T CurrentItem => k_items.Count > 0 ? k_items[k_currentIndex] : default;
	public int CurrentIndex => k_currentIndex;
	public bool IsSpinning => k_isSpinning;

	#endregion

	#region Constructor

	public GameSpinner() {
		var theme = GameTheme.Current;
		var spinnerStyles = theme.Components.Spinner;

		// Initialize from theme defaults
		k_visibleItems = spinnerStyles.DefaultVisibleItems;
		k_itemSlotSize = spinnerStyles.ItemSlotSize;
		k_width = spinnerStyles.Width;

		// Build structure
		k_container = new VisualElement {
			name = "spinner-container",
			style = {
				overflow = Overflow.Hidden,
				alignItems = Align.Center,
				justifyContent = Justify.Center
			}
		};

		k_viewport = new VisualElement {
			name = "spinner-viewport",
			style = {
				overflow = Overflow.Hidden,
				position = Position.Relative
			}
		};

		k_strip = new VisualElement {
			name = "spinner-strip",
			style = {
				position = Position.Absolute,
				flexDirection = FlexDirection.Column
			}
		};

		k_highlightOverlay = new VisualElement {
			name = "spinner-highlight",
			style = {
				position = Position.Absolute
			}
		};
		k_highlightOverlay.pickingMode = PickingMode.Ignore;

		k_viewport.Add(k_strip);
		k_container.Add(k_viewport);
		k_container.Add(k_highlightOverlay);
		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API - Data

	public GameSpinner<T> SetItems(IList<T> items) {
		k_items = items ?? new List<T>();
		return this;
	}

	public GameSpinner<T> SetItems(IEnumerable<T> items) {
		return SetItems(items?.ToList());
	}

	public GameSpinner<T> SetItemTemplate(Func<T, VisualElement> template) {
		k_itemTemplate = template;
		return this;
	}

	#endregion

	#region Fluent API - Appearance

	public GameSpinner<T> SetOrientation(SpinnerOrientation orientation) {
		k_orientation = orientation;
		k_strip.style.flexDirection = orientation == SpinnerOrientation.Vertical
			? FlexDirection.Column
			: FlexDirection.Row;
		return this;
	}

	public GameSpinner<T> SetVisibleItems(int count) {
		k_visibleItems = Mathf.Max(1, count);
		return this;
	}

	public GameSpinner<T> SetItemSize(float size) {
		k_itemSlotSize = size;
		return this;
	}

	public GameSpinner<T> SetWidth(float width) {
		k_width = width;
		return this;
	}

	public GameSpinner<T> SetSize(float width, float height) {
		k_width = width;
		style.width = width;
		style.height = height;
		k_container.style.width = width;
		k_container.style.height = height;
		return this;
	}

	public GameSpinner<T> SetShowHighlight(bool show) {
		k_highlightOverlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
		return this;
	}

	#endregion

	#region Fluent API - Behavior

	public GameSpinner<T> SetSpinDuration(int milliseconds) {
		k_spinDurationMs = milliseconds;
		return this;
	}

	public GameSpinner<T> SetExtraSpins(int spins) {
		k_extraSpins = Mathf.Max(1, spins);
		return this;
	}

	public GameSpinner<T> OnSpinComplete(Action<T, int> callback) {
		k_onSpinComplete = callback;
		return this;
	}

	public GameSpinner<T> OnSpinStart(Action callback) {
		k_onSpinStart = callback;
		return this;
	}

	#endregion

	#region Build

	public GameSpinner<T> Build() {
		ApplyLayout();
		ApplyTheme();
		RebuildStrip();
		SetImmediateIndex(0);
		return this;
	}

	private void ApplyLayout() {
		// Calculate dimensions based on orientation
		float scrollAxisSize = k_visibleItems * k_itemSlotSize;

		if (k_orientation == SpinnerOrientation.Horizontal) {
			// Horizontal: items scroll left/right
			// Width = visible items * slot size, Height = single slot size
			float totalWidth = scrollAxisSize;
			float totalHeight = k_itemSlotSize;

			style.width = totalWidth;
			style.height = totalHeight;
			k_container.style.width = totalWidth;
			k_container.style.height = totalHeight;
			k_viewport.style.width = totalWidth;
			k_viewport.style.height = totalHeight;
		} else {
			// Vertical: items scroll up/down
			// Width = container width, Height = visible items * slot size
			float totalWidth = k_width;
			float totalHeight = scrollAxisSize;

			style.width = totalWidth;
			style.height = totalHeight;
			k_container.style.width = totalWidth;
			k_container.style.height = totalHeight;
			k_viewport.style.width = totalWidth;
			k_viewport.style.height = totalHeight;
		}

		ApplyHighlightPosition();
	}

	private void ApplyHighlightPosition() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spinnerStyles = theme.Components.Spinner;

		// Highlight is centered on the middle visible item
		float highlightPos = ((k_visibleItems - 1) / 2f) * k_itemSlotSize;
		float padding = spinnerStyles.HighlightPadding;

		if (k_orientation == SpinnerOrientation.Horizontal) {
			// Horizontal: highlight is a vertical strip at center
			k_highlightOverlay.style.top = padding;
			k_highlightOverlay.style.bottom = padding;
			k_highlightOverlay.style.left = highlightPos;
			k_highlightOverlay.style.width = k_itemSlotSize;
			k_highlightOverlay.style.height = StyleKeyword.Auto;
		} else {
			// Vertical: highlight is a horizontal strip at center
			k_highlightOverlay.style.left = padding;
			k_highlightOverlay.style.right = padding;
			k_highlightOverlay.style.top = highlightPos;
			k_highlightOverlay.style.height = k_itemSlotSize;
			k_highlightOverlay.style.width = StyleKeyword.Auto;
		}

		borders.ApplyWidth(k_highlightOverlay.style, borders.WidthMedium);
		borders.ApplyColor(k_highlightOverlay.style, colors.Primary);
		borders.ApplyRadius(k_highlightOverlay.style, borders.RadiusSM);
	}

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;

		k_container.style.backgroundColor = colors.BackgroundSecondary;
		borders.ApplyColor(k_container.style, colors.SurfaceBorder);
		borders.ApplyWidth(k_container.style, borders.WidthMedium);
		borders.ApplyRadius(k_container.style, borders.RadiusMD);
	}

	#endregion

	#region Strip Management

	private void RebuildStrip() {
		k_strip.Clear();

		if (k_items.Count == 0) {
			return;
		}

		k_strip.style.flexDirection = k_orientation == SpinnerOrientation.Vertical
			? FlexDirection.Column
			: FlexDirection.Row;

		// Build 3 copies for seamless looping
		for (int repeat = 0; repeat < 3; repeat++) {
			for (int i = 0; i < k_items.Count; i++) {
				var itemElement = CreateItemElement(k_items[i], i);
				k_strip.Add(itemElement);
			}
		}
	}

	private VisualElement CreateItemElement(T item, int index) {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var typography = theme.Typography;
		var spacing = theme.Spacing;

		// Each item is a square slot
		var container = new VisualElement {
			name = $"spinner-item-{index}",
			style = {
				width = k_itemSlotSize,
				height = k_itemSlotSize,
				alignItems = Align.Center,
				justifyContent = Justify.Center,
				flexShrink = 0
			}
		};

		if (k_itemTemplate != null) {
			var content = k_itemTemplate(item);
			if (content != null) {
				container.Add(content);
			}
		} else {
			// Default: simple label
			var label = new Label(item?.ToString() ?? "?");
			typography.TitleLarge.ApplyTo(label.style);
			label.style.color = colors.TextPrimary;
			label.style.unityTextAlign = TextAnchor.MiddleCenter;
			container.Add(label);
		}

		return container;
	}

	private void UpdateStripPosition() {
		if (k_items.Count == 0) {
			return;
		}

		float centerOffset = ((k_visibleItems - 1) / 2f) * k_itemSlotSize;
		float stripOffset = -k_currentOffset + centerOffset - (k_items.Count * k_itemSlotSize);

		if (k_orientation == SpinnerOrientation.Vertical) {
			k_strip.style.top = stripOffset;
			k_strip.style.left = 0;
		} else {
			k_strip.style.left = stripOffset;
			k_strip.style.top = 0;
		}
	}

	#endregion

	#region Spin Methods

	public void SpinTo(int targetIndex) {
		if (k_isSpinning || k_items.Count == 0) {
			return;
		}

		k_targetIndex = Mathf.Clamp(targetIndex, 0, k_items.Count - 1);
		StartSpin();
	}

	public void SpinToRandom() {
		if (k_items.Count == 0) {
			return;
		}
		SpinTo(UnityEngine.Random.Range(0, k_items.Count));
	}

	public void SpinToWeighted(Func<T, float> weightSelector) {
		if (k_items.Count == 0) {
			return;
		}

		float totalWeight = k_items.Sum(weightSelector);
		float randomValue = UnityEngine.Random.Range(0f, totalWeight);
		float cumulative = 0f;

		for (int i = 0; i < k_items.Count; i++) {
			cumulative += weightSelector(k_items[i]);
			if (randomValue <= cumulative) {
				SpinTo(i);
				return;
			}
		}

		SpinTo(k_items.Count - 1);
	}

	public void SetImmediateIndex(int index) {
		if (k_isSpinning) {
			return;
		}
		k_currentIndex = Mathf.Clamp(index, 0, Mathf.Max(0, k_items.Count - 1));
		k_currentOffset = k_currentIndex * k_itemSlotSize;
		UpdateStripPosition();
	}

	private void StartSpin() {
		k_isSpinning = true;
		k_spinStartTime = Time.time;
		k_onSpinStart?.Invoke();

		k_startOffset = k_currentIndex * k_itemSlotSize;

		float itemCount = k_items.Count;
		float totalDistance = (k_extraSpins * itemCount * k_itemSlotSize) + (k_targetIndex * k_itemSlotSize);

		if (k_targetIndex <= k_currentIndex) {
			totalDistance += itemCount * k_itemSlotSize;
		}

		k_targetOffset = k_startOffset + totalDistance;
		k_spinAnimation = schedule.Execute(UpdateSpinAnimation).Every(16);
	}

	private void UpdateSpinAnimation() {
		float elapsed = (Time.time - k_spinStartTime) * 1000;
		float progress = Mathf.Clamp01(elapsed / k_spinDurationMs);

		// Ease out cubic
		float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

		float totalItemsSize = k_items.Count * k_itemSlotSize;
		float rawOffset = Mathf.Lerp(k_startOffset, k_targetOffset, easedProgress);

		k_currentOffset = rawOffset % totalItemsSize;
		if (k_currentOffset < 0) {
			k_currentOffset += totalItemsSize;
		}

		UpdateStripPosition();

		if (progress >= 1f) {
			CompleteSpinAnimation();
		}
	}

	private void CompleteSpinAnimation() {
		k_spinAnimation?.Pause();
		k_isSpinning = false;
		k_currentIndex = k_targetIndex;
		k_currentOffset = k_currentIndex * k_itemSlotSize;
		UpdateStripPosition();

		if (GameTheme.Current.Audio.Enabled) {
			Debug.Log("[Audio] Playing: ui_spinner_stop");
		}

		if (k_items.Count > 0) {
			k_onSpinComplete?.Invoke(k_items[k_currentIndex], k_currentIndex);
		}
	}

	#endregion

	#region Theme Updates

	private void OnThemeChanged(GameTheme theme) {
		var spinnerStyles = theme.Components.Spinner;

		k_itemSlotSize = spinnerStyles.ItemSlotSize;
		k_width = spinnerStyles.Width;
		k_visibleItems = spinnerStyles.DefaultVisibleItems;

		ApplyLayout();
		ApplyTheme();
		RebuildStrip();

		if (!k_isSpinning) {
			k_currentOffset = k_currentIndex * k_itemSlotSize;
			UpdateStripPosition();
		}
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		k_spinAnimation?.Pause();
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}

/// <summary>
/// Spinner scroll orientation.
/// </summary>
public enum SpinnerOrientation {
	Vertical,
	Horizontal
}