using RPGGame.Core;
using RPGGame.Core.Items;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Display tier for slot displays, controlling the level of detail shown.
/// </summary>
public enum DisplayTier {
    /// <summary>
    /// Compact view: Icon, rarity frame, stack count, cooldown indicator.
    /// Used in grids, lists, spinners, action bars.
    /// </summary>
    Compact,

    /// <summary>
    /// Summary/Tooltip view: Name, type, key stats, tags.
    /// Shown on hover or as pinned tooltip. "Diablo-style" at-a-glance info.
    /// </summary>
    Summary,

    /// <summary>
    /// Detailed/Inspect view: Full stats, description, lore, comparisons.
    /// Opened via "Inspect" context menu or dedicated panel.
    /// </summary>
    Detailed
}

/// <summary>
/// Non-generic base class for all slot displays.
/// Holds static drag state shared across all slot types.
/// </summary>
public abstract class BaseGameSlotDisplay : VisualElement {
    #region Static Drag State

    /// <summary>Currently dragging slot.</summary>
    protected static BaseGameSlotDisplay? s_dragSource;

    /// <summary>Visual ghost element during drag.</summary>
    protected static VisualElement? s_dragGhost;

    /// <summary>Whether any slot is currently being dragged.</summary>
    public static bool IsDragInProgress => s_dragSource != null;

    /// <summary>The slot currently being dragged, if any.</summary>
    public static BaseGameSlotDisplay? DragSource => s_dragSource;

    /// <summary>Cancels any active drag operation.</summary>
    public static void CancelDrag() {
        if (s_dragSource is ISlotDragHandler handler) {
            handler.OnDragCancelled();
        }
        s_dragGhost?.RemoveFromHierarchy();
        s_dragGhost = null;
        s_dragSource = null;
    }

    #endregion

    #region Abstract Properties

    /// <summary>Whether this slot is empty.</summary>
    public abstract bool IsEmpty { get; }

    /// <summary>Optional slot identifier.</summary>
    public abstract object? SlotId { get; }

    #endregion

    #region Abstract Methods

    /// <summary>Gets the item being dragged from this slot.</summary>
    public abstract ItemInstance? GetDraggedItem();

    #endregion
}

/// <summary>
/// Interface for handling drag cancellation.
/// </summary>
internal interface ISlotDragHandler {
    void OnDragCancelled();
}

/// <summary>
/// Generic base class for slot displays with proper fluent API support.
/// Uses CRTP pattern so all fluent methods return the correct derived type.
/// 
/// Supports a three-tier display system:
/// - Tier 1 (Compact): The slot itself - icon, rarity frame, stack count, cooldown
/// - Tier 2 (Summary): Hover tooltip with key info (CreateBasicTooltip)
/// - Tier 3 (Detailed): Full inspection view (CreateExpandedTooltip via Inspect action)
/// </summary>
/// <typeparam name="TSelf">The derived class type.</typeparam>
public abstract class BaseGameSlotDisplay<TSelf> : BaseGameSlotDisplay, ISlotDragHandler
    where TSelf : BaseGameSlotDisplay<TSelf>
{
    #region Constants

	protected static float DEFAULT_SIZE => GameTheme.Current.Components.Slot.SizeM;
    protected const int TOOLTIP_SHOW_DELAY_MS = 300;
    protected const float DRAG_THRESHOLD = 5f;

    #endregion

    #region Fields - UI Components

    protected readonly GameContainer k_rootContainer;
    protected readonly GameRarityFrame k_rarityFrame;
    protected readonly GameContainer k_iconContainer;
    protected readonly GameLabel k_placeholderLabel;
    protected readonly GameLabel k_stackLabel;
    protected readonly GameLabel k_badgeLabel;
    protected readonly GameStatBar k_durabilityBar;
    protected readonly GameContainer k_cooldownOverlay;
    protected readonly GameLabel k_cooldownLabel;
    protected readonly GameContainer k_selectionOverlay;
    protected readonly GameContainer k_hoverOverlay;
    protected readonly GameContainer k_dropHighlight;

    #endregion

    #region Fields - State

    protected float k_size = DEFAULT_SIZE;
    protected RarityType k_rarity = RarityType.Common;
    protected bool k_isSelected;
    protected bool k_isHovered;
    protected bool k_showPlaceholder = true;
    protected bool k_showStackCount = true;
    protected bool k_showDurability = true;
    protected bool k_showCooldown = true;
    protected bool k_isInteractive = true;
    protected bool k_isEnabled = true;
    protected float k_cooldownProgress;
    protected Duration k_cooldownRemaining = Duration.Zero;
    protected float k_durabilityPercent = 1f;
    protected int k_stackCount = 1;
    protected object? k_slotId;

    // Display tier
    protected DisplayTier k_displayTier = DisplayTier.Compact;
    protected DisplayTier? k_forcedDisplayTier;

    // Interaction control
    protected bool k_allowTooltip = true;
    protected bool k_allowContextMenu = true;

    #endregion

    #region Fields - Tooltip

    protected GameTooltip? k_basicTooltip;
    protected GameTooltip? k_expandedTooltip;
    protected bool k_expandedTooltipVisible;
    protected IVisualElementScheduledItem? k_tooltipSchedule;
    protected GameDb k_gameDb => GameServices.Db;

    #endregion

    #region Fields - Drag & Drop

    protected bool k_isDraggable;
    protected bool k_isDroppable;
    protected bool k_isDragging;
    protected Vector2 k_mouseDownPos;
    protected bool k_potentialDrag;
    protected Func<ItemInstance?, bool>? k_dropFilter;

    #endregion

    #region Fields - Callbacks

    protected Action? k_onClick;
    protected Action? k_onRightClick;
    protected Action? k_onDoubleClick;
    protected Action? k_onDragStarted;
    protected Action<ItemInstance?, BaseGameSlotDisplay>? k_onItemDropped;
    protected Action? k_onInspect;

    #endregion

    #region Properties

    public bool IsSelected => k_isSelected;
    public float Size => k_size;
    public override object? SlotId => k_slotId;

    /// <summary>
    /// Gets the effective display tier (forced tier if set, otherwise current tier).
    /// </summary>
    public DisplayTier EffectiveDisplayTier => k_forcedDisplayTier ?? k_displayTier;

    #endregion

    #region Constructor

    protected BaseGameSlotDisplay() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        var borders = theme.Borders;

        k_rootContainer = new GameContainer("slot-root")
            .SetRelative()
            .SetCenter();

        k_rarityFrame = new GameRarityFrame();

        k_iconContainer = new GameContainer("slot-icon")
            .SetFullSize()
            .SetCenter();

        k_placeholderLabel = new GameLabel()
            .SetStyle(LabelStyle.TitleMedium)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_placeholderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        k_placeholderLabel.style.display = DisplayStyle.None;
        k_placeholderLabel.style.textShadow = new TextShadow {
            offset = new Vector2(1, 1),
            blurRadius = 2,
            color = Color.black
        };
        k_iconContainer.Add(k_placeholderLabel);

        k_rarityFrame.SetContent(k_iconContainer);

        k_stackLabel = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleRight)
            .Build();
        k_stackLabel.style.position = Position.Absolute;
        k_stackLabel.style.right = 2;
        k_stackLabel.style.bottom = 2;
        k_stackLabel.style.display = DisplayStyle.None;
        k_stackLabel.style.textShadow = new TextShadow {
            offset = new Vector2(1, 1),
            blurRadius = 1,
            color = Color.black
        };

        k_badgeLabel = new GameLabel()
            .SetStyle(LabelStyle.Caption)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_badgeLabel.style.position = Position.Absolute;
        k_badgeLabel.style.right = 2;
        k_badgeLabel.style.top = 2;
        k_badgeLabel.style.display = DisplayStyle.None;
        k_badgeLabel.style.backgroundColor = colors.BackgroundElevated;
        k_badgeLabel.style.paddingLeft = 2;
        k_badgeLabel.style.paddingRight = 2;
        k_badgeLabel.style.borderTopLeftRadius = borders.RadiusXS;
        k_badgeLabel.style.borderTopRightRadius = borders.RadiusXS;
        k_badgeLabel.style.borderBottomLeftRadius = borders.RadiusXS;
        k_badgeLabel.style.borderBottomRightRadius = borders.RadiusXS;

        k_durabilityBar = new GameStatBar()
            .SetVariant(StatBarVariant.Dynamic)
            .SetDynamicThresholds(warningAt: 0.5f, dangerAt: 0.25f)
            .SetHeight(3)
            .SetShowValue(false)
            .SetAnimateChanges(false)
            .Build();
        k_durabilityBar.style.position = Position.Absolute;
        k_durabilityBar.style.left = 4;
        k_durabilityBar.style.right = 4;
        k_durabilityBar.style.bottom = 4;
        k_durabilityBar.style.display = DisplayStyle.None;

        k_cooldownOverlay = new GameContainer("slot-cooldown")
            .SetAbsoluteFill()
            .SetCenter()
            .SetBackgroundColor(new Color(0, 0, 0, 0.6f));
        k_cooldownOverlay.style.display = DisplayStyle.None;
        k_cooldownOverlay.pickingMode = PickingMode.Ignore;

        k_cooldownLabel = new GameLabel()
            .SetStyle(LabelStyle.TitleSmall)
            .SetTextAlign(TextAnchor.MiddleCenter)
            .Build();
        k_cooldownLabel.style.color = Color.white;
        k_cooldownOverlay.Add(k_cooldownLabel);

        k_selectionOverlay = new GameContainer("slot-selection")
            .SetAbsoluteFill()
            .SetBackgroundColor(colors.Primary.WithAlpha(0.3f))
            .SetBorderRadius(borders.RadiusMD);
        k_selectionOverlay.style.display = DisplayStyle.None;
        k_selectionOverlay.pickingMode = PickingMode.Ignore;

        k_hoverOverlay = new GameContainer("slot-hover")
            .SetAbsoluteFill()
            .SetBackgroundColor(colors.Primary.WithAlpha(0.1f))
            .SetBorderRadius(borders.RadiusMD);
        k_hoverOverlay.style.display = DisplayStyle.None;
        k_hoverOverlay.pickingMode = PickingMode.Ignore;

        k_dropHighlight = new GameContainer("slot-drop-highlight")
            .SetAbsolute()
            .SetTop(-2).SetRight(-2).SetBottom(-2).SetLeft(-2)
            .SetBorderWidth(2)
            .SetBorderRadius(borders.RadiusMD);
        k_dropHighlight.style.display = DisplayStyle.None;
        k_dropHighlight.pickingMode = PickingMode.Ignore;

        k_rootContainer.Add(k_rarityFrame);
        k_rootContainer.Add(k_stackLabel);
        k_rootContainer.Add(k_badgeLabel);
        k_rootContainer.Add(k_durabilityBar);
        k_rootContainer.Add(k_cooldownOverlay);
        k_rootContainer.Add(k_hoverOverlay);
        k_rootContainer.Add(k_selectionOverlay);
        k_rootContainer.Add(k_dropHighlight);

        Add(k_rootContainer);

        RegisterCallback<ClickEvent>(OnClickEvent);
        RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
        RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
        RegisterCallback<MouseEnterEvent>(OnMouseEnterEvent);
        RegisterCallback<MouseLeaveEvent>(OnMouseLeaveEvent);

        GameTheme.OnThemeChanged += OnThemeChanged;
    }

    #endregion

    #region Abstract Methods

    /// <summary>Gets the icon texture for this slot.</summary>
    protected abstract Texture2D? GetIcon();

    /// <summary>Gets the display name for this slot's content.</summary>
    protected abstract string GetDisplayName();

    /// <summary>Gets the placeholder initials when no icon is available.</summary>
    protected abstract string GetPlaceholderInitials();

    /// <summary>
    /// Creates the Summary tier tooltip (Tier 2).
    /// Shown on hover - key info at a glance.
    /// </summary>
    protected abstract GameTooltip CreateBasicTooltip();

    /// <summary>
    /// Creates the Detailed tier tooltip (Tier 3).
    /// Full inspection view with all stats, description, lore.
    /// </summary>
    protected abstract GameTooltip CreateExpandedTooltip();

    #endregion

    #region Virtual Methods

    /// <summary>Whether this slot can be dragged.</summary>
    protected virtual bool CanDrag() => false;

    /// <summary>Whether this slot can accept a dropped item.</summary>
    protected virtual bool CanAcceptDrop(ItemInstance? item) => false;

    /// <summary>
    /// Called when right-click is triggered. Override to show type-specific context menu.
    /// </summary>
    /// <param name="mousePosition">Screen position for menu placement.</param>
    protected virtual void OnRightClickAction(Vector2 mousePosition) {
        if (IsEmpty) return;
        ShowBaseContextMenu(mousePosition);
    }

    /// <summary>Called after RefreshDisplay completes for derived class updates.</summary>
    protected virtual void OnRefreshComplete() { }

    /// <summary>Creates the visual ghost element shown during drag.</summary>
    protected virtual VisualElement CreateDragGhost() {
        var ghost = new GameContainer("drag-ghost")
            .SetSize(k_size, k_size)
            .SetOpacity(0.8f)
            .Build();

        var ghostFrame = new GameRarityFrame()
            .SetRarity(k_rarity)
            .SetSize(k_size)
            .SetShowGlow(false)
            .Build();

        if (k_showPlaceholder && !IsEmpty) {
            var ghostLabel = new GameLabel()
                .SetText(GetPlaceholderInitials())
                .SetStyle(LabelStyle.TitleMedium)
                .SetTextAlign(TextAnchor.MiddleCenter)
                .Build();
            ghostLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            ghostLabel.style.color = GameTheme.Current.Colors.GetRarityColor(k_rarity);
            ghostFrame.Content.Add(ghostLabel);
        }

        ghost.Add(ghostFrame);
        return ghost;
    }

    #endregion

    #region Context Menu Helpers

    /// <summary>
    /// Shows the base context menu with Inspect option.
    /// Call this from overridden OnRightClickAction to include base options.
    /// </summary>
    protected void ShowBaseContextMenu(Vector2 mousePosition) {
        var menu = GameContextMenu.Show(this, mousePosition)
            .AddHeader(GetDisplayName());

        // Inspect option - opens Detailed tier
        menu.AddItem("🔍 Inspect", () => {
            ShowDetailedInspector();
            k_onInspect?.Invoke();
        });

        AddTypeSpecificContextMenuItems(menu);

        menu.AddDivider();
        menu.AddItem("Cancel", null);
    }

    /// <summary>
    /// Override to add type-specific context menu items after "Inspect".
    /// </summary>
    protected virtual void AddTypeSpecificContextMenuItems(GameContextMenu menu) {
        // Override in derived classes to add specific options
    }

    /// <summary>
    /// Shows the Detailed tier inspector as a centered modal tooltip.
    /// </summary>
    protected virtual void ShowDetailedInspector() {
        if (IsEmpty) return;

        HideAllTooltips();

        var detailsTooltip = CreateExpandedTooltip();
		detailsTooltip.Build();

        var root = panel?.visualTree;
        if (root != null) {
            root.Add(detailsTooltip);

            // Center on screen
            detailsTooltip.style.position = Position.Absolute;
            detailsTooltip.RegisterCallback<GeometryChangedEvent>(_ => {
                var rootBounds = root.worldBound;
                var tooltipBounds = detailsTooltip.worldBound;
                detailsTooltip.style.left = (rootBounds.width - tooltipBounds.width) / 2;
                detailsTooltip.style.top = (rootBounds.height - tooltipBounds.height) / 2;
            });

            detailsTooltip.Show();
        }
    }

    #endregion

    #region Fluent API - Appearance

    public TSelf SetSize(float size) {
        k_size = size;
        style.width = size;
        style.height = size;
        k_rootContainer.SetSize(size, size);
        k_rarityFrame.SetSize(size);
        UpdatePlaceholderFontSize();
        return (TSelf)this;
    }

    public TSelf SetRarity(RarityType rarity) {
        k_rarity = rarity;
        k_rarityFrame.SetRarity(rarity);
        k_rarityFrame.SetShowGlow(rarity >= RarityType.Rare && !IsEmpty);
        return (TSelf)this;
    }

    public TSelf SetShowPlaceholder(bool show) {
        k_showPlaceholder = show;
        RefreshDisplay();
        return (TSelf)this;
    }

    public TSelf SetShowStackCount(bool show) {
        k_showStackCount = show;
        RefreshDisplay();
        return (TSelf)this;
    }

    public TSelf SetShowDurability(bool show) {
        k_showDurability = show;
        RefreshDisplay();
        return (TSelf)this;
    }

    public TSelf SetShowCooldown(bool show) {
        k_showCooldown = show;
        RefreshDisplay();
        return (TSelf)this;
    }

    public TSelf SetSelected(bool selected) {
        k_isSelected = selected;
        k_selectionOverlay.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
        return (TSelf)this;
    }

    public TSelf SetInteractive(bool interactive) {
        k_isInteractive = interactive;
        pickingMode = interactive ? PickingMode.Position : PickingMode.Ignore;
        return (TSelf)this;
    }

    public new TSelf SetEnabled(bool enabled) {
        k_isEnabled = enabled;
        k_rootContainer.SetOpacity(enabled ? 1f : 0.5f);
        return (TSelf)this;
    }

    #endregion

    #region Fluent API - Data

    public TSelf SetSlotId(object? slotId) {
        k_slotId = slotId;
        return (TSelf)this;
    }

    public TSelf SetStackCount(int count) {
        k_stackCount = count;
        RefreshDisplay();
        return (TSelf)this;
    }

    public TSelf SetDurability(float percent) {
        k_durabilityPercent = Mathf.Clamp01(percent);
        RefreshDisplay();
        return (TSelf)this;
    }

    public TSelf SetCooldown(float progress, Duration remaining) {
        k_cooldownProgress = Mathf.Clamp01(progress);
        k_cooldownRemaining = remaining;
        RefreshDisplay();
        return (TSelf)this;
    }

    public TSelf ClearCooldown() {
        k_cooldownProgress = 0f;
        k_cooldownRemaining = Duration.Zero;
        RefreshDisplay();
        return (TSelf)this;
    }

    public TSelf SetBadge(string? text) {
        if (string.IsNullOrEmpty(text)) {
            k_badgeLabel.style.display = DisplayStyle.None;
        } else {
            k_badgeLabel.SetText(text);
            k_badgeLabel.style.display = DisplayStyle.Flex;
        }
        return (TSelf)this;
    }

    #endregion

    #region Fluent API - Drag & Drop

    public TSelf SetDraggable(bool draggable) {
        k_isDraggable = draggable;
        return (TSelf)this;
    }

    public TSelf SetDroppable(bool droppable) {
        k_isDroppable = droppable;
        return (TSelf)this;
    }

    public TSelf SetDropFilter(Func<ItemInstance?, bool> filter) {
        k_dropFilter = filter;
        return (TSelf)this;
    }

    #endregion

    #region Fluent API - Events

    public TSelf OnClick(Action callback) {
        k_onClick = callback;
        return (TSelf)this;
    }

    public TSelf OnRightClick(Action callback) {
        k_onRightClick = callback;
        return (TSelf)this;
    }

    public TSelf OnDoubleClick(Action callback) {
        k_onDoubleClick = callback;
        return (TSelf)this;
    }

    public TSelf OnDragStarted(Action callback) {
        k_onDragStarted = callback;
        return (TSelf)this;
    }

    public TSelf OnItemDropped(Action<ItemInstance?, BaseGameSlotDisplay> callback) {
        k_onItemDropped = callback;
        return (TSelf)this;
    }

    /// <summary>
    /// Called when the "Inspect" action is triggered from context menu.
    /// </summary>
    public TSelf OnInspect(Action callback) {
        k_onInspect = callback;
        return (TSelf)this;
    }

    #endregion

    #region Fluent API - Display Tier

    /// <summary>
    /// Sets the current display tier. Affects what level of detail is shown.
    /// </summary>
    public TSelf SetDisplayTier(DisplayTier tier) {
        k_displayTier = tier;
        RefreshDisplay();
        return (TSelf)this;
    }

    /// <summary>
    /// Forces a specific display tier, overriding automatic tier selection.
    /// Pass null to restore automatic behavior.
    /// </summary>
    public TSelf SetForcedDisplayTier(DisplayTier? tier) {
        k_forcedDisplayTier = tier;
        RefreshDisplay();
        return (TSelf)this;
    }

    #endregion

    #region Fluent API - Interaction Control

    /// <summary>
    /// Sets whether tooltips are allowed on hover.
    /// </summary>
    public TSelf SetAllowTooltip(bool allow) {
        k_allowTooltip = allow;
        if (!allow) {
            HideAllTooltips();
        }
        return (TSelf)this;
    }

    /// <summary>
    /// Sets whether the right-click context menu is allowed.
    /// </summary>
    public TSelf SetAllowContextMenu(bool allow) {
        k_allowContextMenu = allow;
        return (TSelf)this;
    }

    /// <summary>
    /// Configures all interaction options at once.
    /// </summary>
    public TSelf SetInteractionOptions(
        bool allowTooltip = true,
        bool allowContextMenu = true,
        bool allowDrag = false,
        bool allowDrop = false) {
        k_allowTooltip = allowTooltip;
        k_allowContextMenu = allowContextMenu;
        k_isDraggable = allowDrag;
        k_isDroppable = allowDrop;
        return (TSelf)this;
    }

    #endregion

    #region Build

    public virtual TSelf Build() {
        SetSize(DEFAULT_SIZE);
        k_rarityFrame.Build();
        ApplyTheme();
        RefreshDisplay();
        return (TSelf)this;
    }

    #endregion

    #region Display Refresh

    public virtual void RefreshDisplay() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;

        var icon = GetIcon();
        if (icon != null) {
            k_rarityFrame.SetIcon(icon);
            k_placeholderLabel.style.display = DisplayStyle.None;
        } else if (k_showPlaceholder && !IsEmpty) {
            string initials = GetPlaceholderInitials();
            k_placeholderLabel.SetText(initials);
            k_placeholderLabel.style.color = colors.GetRarityColor(k_rarity);
            k_placeholderLabel.style.display = DisplayStyle.Flex;
        } else {
            k_placeholderLabel.style.display = DisplayStyle.None;
        }

        if (k_showStackCount && k_stackCount > 1) {
            k_stackLabel.SetText(k_stackCount.ToString());
            k_stackLabel.style.display = DisplayStyle.Flex;
        } else {
            k_stackLabel.style.display = DisplayStyle.None;
        }

        if (k_showDurability && k_durabilityPercent < 1f && !IsEmpty) {
            k_durabilityBar.SetRange(0, 1);
            k_durabilityBar.SetValue(k_durabilityPercent, animate: false);
            k_durabilityBar.style.display = DisplayStyle.Flex;
        } else {
            k_durabilityBar.style.display = DisplayStyle.None;
        }

        if (k_showCooldown && k_cooldownProgress > 0f) {
            k_cooldownOverlay.style.display = DisplayStyle.Flex;
            k_cooldownOverlay.SetOpacity(0.6f * k_cooldownProgress);
            k_cooldownLabel.SetText(FormatCooldownText(k_cooldownRemaining));
        } else {
            k_cooldownOverlay.style.display = DisplayStyle.None;
        }

        k_rarityFrame.SetRarity(k_rarity);
        k_rarityFrame.SetShowGlow(k_rarity >= RarityType.Rare && !IsEmpty);

        OnRefreshComplete();
    }

    protected virtual string FormatCooldownText(Duration remaining) {
        if (remaining.TotalTicks <= 0) return "";
        if (remaining.InTurns >= 1) return $"{remaining.InTurns}";
        if (remaining.InSeconds >= 1) return $"{remaining.InSeconds:F0}s";
        return "";
    }

    protected void UpdatePlaceholderFontSize() {
        int fontSize = k_size switch {
            <= 48f => 12,
            <= 64f => 14,
            <= 80f => 16,
            <= 96f => 18,
            _ => 20
        };
        k_placeholderLabel.style.fontSize = fontSize;
    }

    protected static string GetInitials(string? name) {
        if (string.IsNullOrEmpty(name)) return "?";
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return "?";
        if (words.Length == 1) {
            return words[0].Length > 2 ? words[0][..2].ToUpper() : words[0].ToUpper();
        }
        return string.Concat(words.Take(2).Select(w => char.ToUpper(w[0])));
    }

    #endregion

    #region Input Handling

    private void OnClickEvent(ClickEvent evt) {
        if (!k_isEnabled || !k_isInteractive) return;

        if (evt.clickCount == 2) {
            k_onDoubleClick?.Invoke();
            HideAllTooltips();
        } else if (evt.button == 0) {
            ShowExpandedTooltip();
            k_onClick?.Invoke();
        }
    }

    private void OnMouseDownEvent(MouseDownEvent evt) {
        if (!k_isEnabled || !k_isInteractive) return;

        if (evt.button == 0 && k_isDraggable && CanDrag() && !IsEmpty) {
            k_mouseDownPos = evt.mousePosition;
            k_potentialDrag = true;
            HideAllTooltips();
            evt.StopPropagation();
        } else if (evt.button == 1 && k_allowContextMenu) {
            HideAllTooltips();
            k_onRightClick?.Invoke();
            OnRightClickAction(evt.mousePosition);
            evt.StopPropagation();
        }
    }

    private void OnMouseUpEvent(MouseUpEvent evt) {
        if (evt.button == 0) {
            if (k_isDragging) {
                EndDrag(evt.mousePosition);
            }
            k_potentialDrag = false;
        }
    }

    private void OnMouseMoveEvent(MouseMoveEvent evt) {
        if (k_potentialDrag && !k_isDragging) {
            float distance = Vector2.Distance(k_mouseDownPos, evt.mousePosition);
            if (distance > DRAG_THRESHOLD) {
                StartDrag();
            }
        }

        if (k_isDragging && s_dragGhost != null) {
            s_dragGhost.style.left = evt.mousePosition.x - k_size / 2;
            s_dragGhost.style.top = evt.mousePosition.y - k_size / 2;
        }
    }

    private void OnMouseEnterEvent(MouseEnterEvent evt) {
        if (!k_isEnabled || !k_isInteractive) return;

        k_isHovered = true;
        k_hoverOverlay.style.display = DisplayStyle.Flex;

        if (IsDragInProgress && k_isDroppable && s_dragSource != this) {
            bool canDrop = CheckCanAcceptDrop(s_dragSource?.GetDraggedItem());
            ShowDropHighlight(canDrop);
            return;
        }

        // Only show tooltip if allowed and not empty
        if (k_allowTooltip && !IsEmpty && !IsDragInProgress) {
            k_tooltipSchedule?.Pause();
            k_tooltipSchedule = schedule.Execute(ShowBasicTooltip);
            k_tooltipSchedule.ExecuteLater(TOOLTIP_SHOW_DELAY_MS);
        }
    }

    private void OnMouseLeaveEvent(MouseLeaveEvent evt) {
        k_isHovered = false;
        k_hoverOverlay.style.display = DisplayStyle.None;
        HideDropHighlight();
        HideBasicTooltip();
    }

    protected bool CheckCanAcceptDrop(ItemInstance? item) {
        if (!k_isDroppable) return false;
        if (k_dropFilter != null && !k_dropFilter(item)) return false;
        return CanAcceptDrop(item);
    }

    #endregion

    #region Tooltip Management

    protected virtual void ShowBasicTooltip() {
        if (IsEmpty || k_expandedTooltipVisible) return;

        k_basicTooltip ??= CreateBasicTooltip();
        k_basicTooltip.Build();

        // Ensure tooltip is in the visual tree at the root level
        var root = panel?.visualTree;
        if (root != null && k_basicTooltip.parent == null) {
            root.Add(k_basicTooltip);
        }

        // Position before showing
        var rect = worldBound;
        k_basicTooltip.style.position = Position.Absolute;
        k_basicTooltip.style.left = rect.xMax + 8;
        k_basicTooltip.style.top = rect.y;

        k_basicTooltip.Show();
    }

    protected void HideBasicTooltip() {
        k_tooltipSchedule?.Pause();
        k_tooltipSchedule = null;
        k_basicTooltip?.Hide();
    }

    protected virtual void ShowExpandedTooltip() {
        if (IsEmpty) return;

        HideBasicTooltip();

        k_expandedTooltip ??= CreateExpandedTooltip();
        k_expandedTooltip.Build();
        PositionTooltip(k_expandedTooltip);

        if (k_expandedTooltip.parent == null) {
            panel?.visualTree?.Add(k_expandedTooltip);
        }

        k_expandedTooltip.Show();
        k_expandedTooltipVisible = true;
    }

    protected void HideExpandedTooltip() {
        k_expandedTooltip?.Hide();
        k_expandedTooltipVisible = false;
    }

    protected void HideAllTooltips() {
        HideBasicTooltip();
        HideExpandedTooltip();
    }

    protected void PositionTooltip(GameTooltip tooltip) {
        var rect = worldBound;
        tooltip.style.position = Position.Absolute;
        tooltip.style.left = rect.xMax + 8;
        tooltip.style.top = rect.y;
    }

    #endregion

    #region Drag & Drop

    protected virtual void StartDrag() {
        if (IsEmpty) return;

        k_isDragging = true;
        s_dragSource = this;
        HideAllTooltips();

        s_dragGhost = CreateDragGhost();
        s_dragGhost.style.position = Position.Absolute;
        panel?.visualTree?.Add(s_dragGhost);

        k_rootContainer.SetOpacity(0.4f);

        k_onDragStarted?.Invoke();
        this.CaptureMouse();
    }

    protected virtual void EndDrag(Vector2 mousePos) {
        k_isDragging = false;
        k_potentialDrag = false;

        s_dragGhost?.RemoveFromHierarchy();
        s_dragGhost = null;

        k_rootContainer.SetOpacity(1f);
        this.ReleaseMouse();

        var target = FindDropTarget(mousePos);
        if (target != null && target != this && target.CheckCanAcceptDrop(GetDraggedItem())) {
            target.k_onItemDropped?.Invoke(GetDraggedItem(), this);
        }

        s_dragSource = null;
    }

    void ISlotDragHandler.OnDragCancelled() {
        k_isDragging = false;
        k_potentialDrag = false;
        k_rootContainer.SetOpacity(1f);
        this.ReleaseMouse();
    }

    private BaseGameSlotDisplay<TSelf>? FindDropTarget(Vector2 screenPos) {
        var element = panel?.Pick(screenPos);
        while (element != null) {
            if (element is BaseGameSlotDisplay<TSelf> slot && slot.k_isDroppable) {
                return slot;
            }
            element = element.parent;
        }
        return null;
    }

    protected void ShowDropHighlight(bool canDrop) {
        var theme = GameTheme.Current;
        var color = canDrop ? theme.Colors.Success : theme.Colors.Error;
        k_dropHighlight.SetBorderColor(color);
        k_dropHighlight.style.display = DisplayStyle.Flex;
    }

    protected void HideDropHighlight() {
        k_dropHighlight.style.display = DisplayStyle.None;
    }

    #endregion

    #region Theme

    protected virtual void ApplyTheme() {
        var theme = GameTheme.Current;
        var colors = theme.Colors;
        var borders = theme.Borders;

        k_selectionOverlay.SetBackgroundColor(colors.Primary.WithAlpha(0.3f));
        k_selectionOverlay.SetBorderRadius(borders.RadiusMD);

        k_hoverOverlay.SetBackgroundColor(colors.Primary.WithAlpha(0.1f));
        k_hoverOverlay.SetBorderRadius(borders.RadiusMD);

        k_dropHighlight.SetBorderRadius(borders.RadiusMD);
        k_badgeLabel.style.backgroundColor = colors.BackgroundElevated;
    }

    private void OnThemeChanged(GameTheme theme) {
        ApplyTheme();
        RefreshDisplay();
    }

    #endregion

    #region Cleanup

    public new void RemoveFromHierarchy() {
        if (s_dragSource == this) {
            CancelDrag();
        }
        HideAllTooltips();
        k_basicTooltip?.RemoveFromHierarchy();
        k_expandedTooltip?.RemoveFromHierarchy();
        GameTheme.OnThemeChanged -= OnThemeChanged;
        base.RemoveFromHierarchy();
    }

    #endregion
}