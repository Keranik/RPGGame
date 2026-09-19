using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed tooltip component that can be attached to any VisualElement.
/// Supports multiple positions, custom content, and modal/pinned mode with close button.
/// 
/// <para>Modes:</para>
/// <list type="bullet">
/// <item>Hover tooltip: Shows on mouse enter, hides on mouse leave (default)</item>
/// <item>Pinned/Modal: Shows close button, stays open until dismissed</item>
/// </list>
/// </summary>
public class GameTooltip : VisualElement {
    #region Private Fields

    private readonly VisualElement k_contentContainer;
    private readonly VisualElement k_arrow;
    private readonly Label k_textLabel;
    private readonly GameContainer k_headerContainer;
    private readonly GameButton k_closeButton;

    private VisualElement k_targetElement;
    private TooltipPosition k_position = TooltipPosition.Top;
    private int k_showDelay = 500;
    private int k_hideDelay = 100;
    private bool k_isVisible;
    private bool k_isDetaching;
    private bool k_isPinned;
    private bool k_showCloseButton;
    private string k_title;

    private IVisualElementScheduledItem k_showSchedule;
    private IVisualElementScheduledItem k_hideSchedule;

    #endregion

    #region Constructors

    public GameTooltip() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        // The tooltip itself is absolutely positioned
        style.position = Position.Absolute;
        style.display = DisplayStyle.None;

        // Default picking mode - ignore for hover, position for pinned
        pickingMode = PickingMode.Ignore;

        // Content container (the visible tooltip box)
        k_contentContainer = new VisualElement {
            name = "tooltip-content",
            pickingMode = PickingMode.Ignore
        };

        // Header container with title and close button
        k_headerContainer = new GameContainer("tooltip-header")
            .SetRow()
            .SetSpaceBetween()
            .SetAlignItems(Align.Center)
            .SetFullWidth();
        k_headerContainer.style.display = DisplayStyle.None;

        // Close button
        k_closeButton = new GameButton("✕")
            .SetVariant(ButtonVariant.Ghost)
            .SetSize(ButtonSize.Small)
            .OnClick(OnCloseClicked)
            .Build();
        k_closeButton.style.minWidth = 32;
        k_closeButton.style.minHeight = 32;
        k_closeButton.style.marginLeft = spacing.SM;
        k_closeButton.pickingMode = PickingMode.Position;

        // Arrow
        k_arrow = new VisualElement {
            name = "tooltip-arrow",
            pickingMode = PickingMode.Ignore,
            style = {
                position = Position.Absolute,
                width = 12,
                height = 12
            }
        };

        // Text label
        k_textLabel = new Label {
            pickingMode = PickingMode.Ignore
        };

        k_contentContainer.Add(k_headerContainer);
        k_contentContainer.Add(k_textLabel);
        Add(k_arrow);
        Add(k_contentContainer);

        // Subscribe to theme changes
        GameTheme.OnThemeChanged += OnThemeChanged;
    }

    #endregion

    #region Fluent API - Content

    /// <summary>
    /// Sets simple text content for the tooltip.
    /// </summary>
    public GameTooltip SetContent(string text) {
        k_textLabel.text = text;
        k_textLabel.style.display = DisplayStyle.Flex;
        return this;
    }

	/// <summary>
	/// Sets custom visual element content for the tooltip.
	/// </summary>
	public GameTooltip SetContent(VisualElement customContent) {
		k_contentContainer.Clear();
    
		// Always add header container first (it will be hidden if not needed)
		k_contentContainer.Add(k_headerContainer);
		k_contentContainer.Add(customContent);

		k_textLabel.style.display = DisplayStyle.None;
		return this;
	}

    /// <summary>
    /// Sets a title that appears at the top of the tooltip.
    /// </summary>
    public GameTooltip SetTitle(string title) {
        k_title = title;
        UpdateHeader();
        return this;
    }

    #endregion

    #region Fluent API - Appearance

    /// <summary>
    /// Sets the tooltip position relative to target element.
    /// </summary>
    public GameTooltip SetPosition(TooltipPosition position) {
        k_position = position;
        return this;
    }

    /// <summary>
    /// Sets the maximum width of the tooltip.
    /// </summary>
    public GameTooltip SetMaxWidth(float maxWidth) {
        k_contentContainer.style.maxWidth = maxWidth;
        return this;
    }

    /// <summary>
    /// Sets the minimum width of the tooltip.
    /// </summary>
    public GameTooltip SetMinWidth(float minWidth) {
        k_contentContainer.style.minWidth = minWidth;
        return this;
    }

    /// <summary>
    /// Shows a close button in the tooltip header.
    /// Automatically enables pinned mode.
    /// </summary>
    public GameTooltip SetShowCloseButton(bool show) {
        k_showCloseButton = show;
        if (show) {
            k_isPinned = true;
        }
        UpdateHeader();
        return this;
    }

    /// <summary>
    /// Sets whether this tooltip is pinned (stays open until manually closed).
    /// </summary>
    public GameTooltip SetPinned(bool pinned) {
        k_isPinned = pinned;
        pickingMode = pinned ? PickingMode.Position : PickingMode.Ignore;
        k_contentContainer.pickingMode = pinned ? PickingMode.Position : PickingMode.Ignore;
        return this;
    }

    #endregion

    #region Fluent API - Behavior

    /// <summary>
    /// Sets the show and hide delays for hover tooltips.
    /// </summary>
    public GameTooltip SetDelay(int showDelayMs, int hideDelayMs = 100) {
        k_showDelay = showDelayMs;
        k_hideDelay = hideDelayMs;
        return this;
    }

    #endregion

    #region Build & Attach

    /// <summary>
    /// Builds and applies theme to the tooltip.
    /// </summary>
    public GameTooltip Build() {
        ApplyTheme();
        return this;
    }

    /// <summary>
    /// Attaches the tooltip to a target element for hover display.
    /// </summary>
    public GameTooltip AttachTo(VisualElement target) {
        k_targetElement = target;

        if (!k_isPinned) {
            target.RegisterCallback<MouseEnterEvent>(OnTargetMouseEnter);
            target.RegisterCallback<MouseLeaveEvent>(OnTargetMouseLeave);
        }

        return this;
    }

    /// <summary>
    /// Detaches the tooltip from its target element.
    /// </summary>
    public void Detach() {
        if (k_isDetaching) return;
        k_isDetaching = true;

        try {
            if (k_targetElement != null) {
                k_targetElement.UnregisterCallback<MouseEnterEvent>(OnTargetMouseEnter);
                k_targetElement.UnregisterCallback<MouseLeaveEvent>(OnTargetMouseLeave);
                k_targetElement = null;
            }

            k_showSchedule?.Pause();
            k_hideSchedule?.Pause();
            k_showSchedule = null;
            k_hideSchedule = null;
        } finally {
            k_isDetaching = false;
        }
    }

    #endregion

    #region Show/Hide

    /// <summary>
    /// Shows the tooltip.
    /// </summary>
    public void Show() {
        if (k_isVisible) return;

        k_hideSchedule?.Pause();
        k_isVisible = true;
        style.display = DisplayStyle.Flex;

        // Position after showing so layout is calculated
        schedule.Execute(PositionTooltip);
    }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    public void Hide() {
        if (!k_isVisible) return;

        k_showSchedule?.Pause();
        k_isVisible = false;
        style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Closes and removes a pinned tooltip.
    /// </summary>
    public void Close() {
        Hide();
        if (k_isPinned) {
            RemoveFromHierarchy();
        }
    }

    #endregion

    #region Header Management

    private void UpdateHeader() {
        var theme = GameTheme.Current;
        var spacing = theme.Spacing;

        bool showHeader = k_showCloseButton || !string.IsNullOrEmpty(k_title);
        k_headerContainer.style.display = showHeader ? DisplayStyle.Flex : DisplayStyle.None;

        if (showHeader) {
            k_headerContainer.Clear();

            // Title label
            if (!string.IsNullOrEmpty(k_title)) {
                var titleLabel = new GameLabel(k_title)
                    .SetStyle(LabelStyle.TitleSmall)
                    .SetColor(LabelColor.Primary)
                    .SetFlexGrow(1f)
                    .Build();
                k_headerContainer.Add(titleLabel);
            } else {
                // Spacer to push close button to right
                var spacer = new VisualElement();
                spacer.style.flexGrow = 1;
                k_headerContainer.Add(spacer);
            }

            // Close button
            if (k_showCloseButton) {
                k_headerContainer.Add(k_closeButton);
            }

            k_headerContainer.style.marginBottom = spacing.XS;
        }
    }

    private void OnCloseClicked() {
        Close();
    }

    #endregion

    #region Positioning

    private void PositionTooltip() {
        // Force layout update to get correct size
        var tooltipRect = layout;
        float tooltipWidth = tooltipRect.width > 0 ? tooltipRect.width : 300;
        float tooltipHeight = tooltipRect.height > 0 ? tooltipRect.height : 100;

        var arrowSize = GameTheme.Current.Components.Tooltip.ArrowSize;

        float x = 0, y = 0;

        // For Center position, we don't need a target element - center on screen
        if (k_position == TooltipPosition.Center) {
            var root = panel?.visualTree;
            if (root != null) {
                var rootRect = root.worldBound;
                x = rootRect.x + (rootRect.width - tooltipWidth) / 2;
                y = rootRect.y + (rootRect.height - tooltipHeight) / 2;
            }
            // Hide arrow for centered tooltips
            k_arrow.style.display = DisplayStyle.None;
            style.left = x;
            style.top = y;
            return;
        }

        // For directional positions, we need a target element
        if (k_targetElement == null) {
            // No target - just center it
            var root = panel?.visualTree;
            if (root != null) {
                var rootRect = root.worldBound;
                x = rootRect.x + (rootRect.width - tooltipWidth) / 2;
                y = rootRect.y + (rootRect.height - tooltipHeight) / 2;
            }
            k_arrow.style.display = DisplayStyle.None;
            style.left = x;
            style.top = y;
            return;
        }

        var targetRect = k_targetElement.worldBound;

        // Show arrow for directional tooltips (but not for pinned modals)
        k_arrow.style.display = k_isPinned ? DisplayStyle.None : DisplayStyle.Flex;

        switch (k_position) {
            case TooltipPosition.Top:
                x = targetRect.x + (targetRect.width - tooltipWidth) / 2;
                y = targetRect.y - tooltipHeight - arrowSize;
                PositionArrow(TooltipPosition.Top, tooltipWidth, tooltipHeight, arrowSize);
                break;

            case TooltipPosition.Bottom:
                x = targetRect.x + (targetRect.width - tooltipWidth) / 2;
                y = targetRect.yMax + arrowSize;
                PositionArrow(TooltipPosition.Bottom, tooltipWidth, tooltipHeight, arrowSize);
                break;

            case TooltipPosition.Left:
                x = targetRect.x - tooltipWidth - arrowSize;
                y = targetRect.y + (targetRect.height - tooltipHeight) / 2;
                PositionArrow(TooltipPosition.Left, tooltipWidth, tooltipHeight, arrowSize);
                break;

            case TooltipPosition.Right:
                x = targetRect.xMax + arrowSize;
                y = targetRect.y + (targetRect.height - tooltipHeight) / 2;
                PositionArrow(TooltipPosition.Right, tooltipWidth, tooltipHeight, arrowSize);
                break;
        }

        // Clamp to screen bounds
        var rootElement = panel?.visualTree;
        if (rootElement != null) {
            var rootRect = rootElement.worldBound;
            float padding = 8f;
            x = Mathf.Clamp(x, rootRect.x + padding, rootRect.xMax - tooltipWidth - padding);
            y = Mathf.Clamp(y, rootRect.y + padding, rootRect.yMax - tooltipHeight - padding);
        }

        style.left = x;
        style.top = y;
    }

    private void PositionArrow(TooltipPosition pos, float tooltipWidth, float tooltipHeight, float arrowSize) {
        k_arrow.style.width = arrowSize;
        k_arrow.style.height = arrowSize;

        // Reset all positions
        k_arrow.style.left = StyleKeyword.Auto;
        k_arrow.style.right = StyleKeyword.Auto;
        k_arrow.style.top = StyleKeyword.Auto;
        k_arrow.style.bottom = StyleKeyword.Auto;

        switch (pos) {
            case TooltipPosition.Top:
                k_arrow.style.left = tooltipWidth / 2 - arrowSize / 2;
                k_arrow.style.bottom = -arrowSize / 2;
                k_arrow.style.rotate = new Rotate(45);
                break;

            case TooltipPosition.Bottom:
                k_arrow.style.left = tooltipWidth / 2 - arrowSize / 2;
                k_arrow.style.top = -arrowSize / 2;
                k_arrow.style.rotate = new Rotate(45);
                break;

            case TooltipPosition.Left:
                k_arrow.style.right = -arrowSize / 2;
                k_arrow.style.top = tooltipHeight / 2 - arrowSize / 2;
                k_arrow.style.rotate = new Rotate(45);
                break;

            case TooltipPosition.Right:
                k_arrow.style.left = -arrowSize / 2;
                k_arrow.style.top = tooltipHeight / 2 - arrowSize / 2;
                k_arrow.style.rotate = new Rotate(45);
                break;
        }
    }

    #endregion

    #region Theme Application

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spacing = theme.Spacing;
		var typography = theme.Typography;
		var tooltipStyles = theme.Components.Tooltip;

		// Content container styling
		k_contentContainer.style.backgroundColor = colors.TooltipBackground;
		borders.ApplyColor(k_contentContainer.style, colors.TooltipBorder);
		borders.ApplyWidth(k_contentContainer.style, borders.WidthThin);
		borders.ApplyRadius(k_contentContainer.style, borders.RadiusMD);

		// Larger padding for 4K
		k_contentContainer.style.paddingTop = spacing.SM;
		k_contentContainer.style.paddingBottom = spacing.SM;
		k_contentContainer.style.paddingLeft = spacing.MD;
		k_contentContainer.style.paddingRight = spacing.MD;

		// Larger max width for 4K
		k_contentContainer.style.maxWidth = tooltipStyles.MaxWidth * 1.5f;
		k_contentContainer.style.minWidth = 200;

		// Arrow styling
		k_arrow.style.backgroundColor = colors.TooltipBackground;
		k_arrow.style.width = tooltipStyles.ArrowSize;
		k_arrow.style.height = tooltipStyles.ArrowSize;

		// Text styling - use BodyMedium for 4K readability
		typography.BodyMedium.ApplyTo(k_textLabel.style);
		k_textLabel.style.color = colors.TextPrimary;
		k_textLabel.style.whiteSpace = WhiteSpace.Normal;

		// Header styling
		k_headerContainer.style.paddingBottom = spacing.XS;
		k_headerContainer.style.borderBottomWidth = 1;
		k_headerContainer.style.borderBottomColor = colors.SurfaceBorder;
	}

    private void OnThemeChanged(GameTheme theme) {
        ApplyTheme();
    }

    #endregion

    #region Event Handlers

    private void OnTargetMouseEnter(MouseEnterEvent evt) {
        if (k_isPinned) return;

        k_hideSchedule?.Pause();

        // Add to visual tree if not already
        if (parent == null && k_targetElement?.panel?.visualTree != null) {
            k_targetElement.panel.visualTree.Add(this);
        }

        k_showSchedule = schedule.Execute(Show).StartingIn(k_showDelay);
    }

    private void OnTargetMouseLeave(MouseLeaveEvent evt) {
        if (k_isPinned) return;

        k_showSchedule?.Pause();
        k_hideSchedule = schedule.Execute(Hide).StartingIn(k_hideDelay);
    }

    #endregion

    #region Cleanup

    new public void RemoveFromHierarchy() {
        GameTheme.OnThemeChanged -= OnThemeChanged;
        Detach();
        base.RemoveFromHierarchy();
    }

    #endregion
}

#region Enums

public enum TooltipPosition {
    Top,
    Bottom,
    Left,
    Right,
    Center
}

#endregion

#region Extension Methods

public static class TooltipExtensions {
    /// <summary>
    /// Adds a simple text tooltip to any VisualElement.
    /// </summary>
    public static T AddTooltip<T>(this T element, string text, TooltipPosition position = TooltipPosition.Top) where T : VisualElement {
        var tooltip = new GameTooltip()
            .SetContent(text)
            .SetPosition(position)
            .Build()
            .AttachTo(element);

        return element;
    }

    /// <summary>
    /// Adds a custom tooltip to any VisualElement.
    /// </summary>
    public static T AddTooltip<T>(this T element, GameTooltip tooltip) where T : VisualElement {
        tooltip.AttachTo(element);
        return element;
    }

    /// <summary>
    /// Shows a pinned tooltip with close button at the specified position.
    /// </summary>
    public static GameTooltip ShowPinnedTooltip(this VisualElement anchor, string content, string title = null) {
        var root = anchor.panel?.visualTree;
        if (root == null) return null;

        var tooltip = new GameTooltip()
            .SetContent(content)
            .SetPosition(TooltipPosition.Center)
            .SetPinned(true)
            .SetShowCloseButton(true);

        if (!string.IsNullOrEmpty(title)) {
            tooltip.SetTitle(title);
        }

        tooltip.Build();
        root.Add(tooltip);
        tooltip.Show();

        return tooltip;
    }

    /// <summary>
    /// Shows a pinned tooltip with custom content and close button.
    /// </summary>
    public static GameTooltip ShowPinnedTooltip(this VisualElement anchor, VisualElement content, string title = null) {
        var root = anchor.panel?.visualTree;
        if (root == null) return null;

        var tooltip = new GameTooltip()
            .SetContent(content)
            .SetPosition(TooltipPosition.Center)
            .SetPinned(true)
            .SetShowCloseButton(true);

        if (!string.IsNullOrEmpty(title)) {
            tooltip.SetTitle(title);
        }

        tooltip.Build();
        root.Add(tooltip);
        tooltip.Show();

        return tooltip;
    }
}

#endregion