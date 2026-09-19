using RPGGame.Core.Characters;
using RPGGame.Core.Combat;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// Displays a single combatant (player or enemy) in the combat screen.
/// Shows health bar, name, and status conditions.
/// </summary>
[Obsolete("Use CombatCharacterDisplay instead")]
public class CombatantDisplay : VisualElement {
	#region Fields

	private readonly LiveCharacter k_character;
	private readonly bool k_isEnemy;
	private readonly GamePanel k_panel;
	private readonly GameLabel k_nameLabel;
	private GameStatBar k_healthBar = null!;
	private GameStatBar? k_manaBar;
	private GamePanel k_statusIcons = null!;

	#endregion

	#region Properties

	public LiveCharacter Character => k_character;
	public event Action? OnClicked;

	#endregion

	#region Constructor

	public CombatantDisplay(LiveCharacter character, bool isEnemy) {
		k_character = character;
		k_isEnemy = isEnemy;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var panelStyles = theme.Components.Panel;

		k_panel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Vertical)
			.SetAlignment(Align.Center)
			.SetPadding(spacing.SM)
			.SetWidth(panelStyles.MinWidth * 0.7f)
			.SetMargin(0, spacing.SM, spacing.SM, 0)
			.Build();

		if (k_isEnemy) {
			k_panel.SetPickingMode(PickingMode.Position);
			k_panel.RegisterCallback<ClickEvent>(_ => OnClicked?.Invoke());
			k_panel.RegisterCallback<MouseEnterEvent>(_ => OnMouseEnter());
			k_panel.RegisterCallback<MouseLeaveEvent>(_ => OnMouseLeave());
		}

		// Portrait/Icon
		var iconLabel = new GameLabel(k_isEnemy ? "👾" : "👤")
			.SetStyle(LabelStyle.DisplaySmall)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		// Name
		k_nameLabel = new GameLabel(k_character.Name)
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetFontSize(theme.Typography.BodySmall.FontSize)
			.SetMarginTop(spacing.XXS)
			.Build();

		// Health bar
		k_healthBar = new GameStatBar()
			.SetRange(0, k_character.MaxHealth)
			.SetValue(k_character.CurrentHealth)
			.SetVariant(StatBarVariant.Health)
			.SetShowValue()
			.SetWidth(Length.Percent(100))
			.SetMarginTop(spacing.XS)
			.Build();

		// Mana bar (only for player or casters with mana)
		if (!k_isEnemy || k_character.MaxMana > 0) {
			k_manaBar = new GameStatBar()
				.SetRange(0, k_character.MaxMana)
				.SetValue(k_character.CurrentMana)
				.SetVariant(StatBarVariant.Mana)
				.SetShowValue()
				.SetWidth(Length.Percent(100))
				.SetHeight(12)
				.SetMarginTop(spacing.XXS)
				.Build();
		}

		// Status icons container
		k_statusIcons = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.Center)
			.SetFlexWrap(Wrap.Wrap)
			.SetMarginTop(spacing.XXS)
			.Build();

		// Build panel
		k_panel.Content.Add(iconLabel);
		k_panel.Content.Add(k_nameLabel);
		k_panel.Content.Add(k_healthBar);
		if (k_manaBar != null) {
			k_panel.Content.Add(k_manaBar);
		}
		k_panel.Content.Add(k_statusIcons);

		Add(k_panel);

		// Initial refresh
		Refresh();

		// Subscribe to theme changes
		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Public Methods

	public void Refresh() {
		// Update health bar
		k_healthBar
			.SetRange(0, k_character.MaxHealth)
			.SetValue(k_character.CurrentHealth);

		// Update mana bar if present
		k_manaBar?
			.SetRange(0, k_character.MaxMana)
			.SetValue(k_character.CurrentMana);

		// Update opacity based on alive status
		style.opacity = k_character.IsAlive ? 1f : 0.4f;

		// Update status icons
		RefreshStatusIcons();
	}

	public void SetSelected(bool selected) {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;

		k_panel.SetBorderColor(selected ? colors.Primary : colors.SurfaceBorder);
		k_panel.SetBorderWidth(selected ? borders.WidthThick : borders.WidthThin);
	}

	#endregion

	#region Private Methods

	private void RefreshStatusIcons() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_statusIcons.ClearChildren();

		foreach (var (condition, effect) in k_character.Conditions) {
			var icon = GetConditionIcon(condition);
			var statusLabel = new GameLabel(icon)
				.SetStyle(LabelStyle.BodySmall)
				.SetMarginRight(spacing.XXS)
				.Build();

			statusLabel.tooltip = $"{condition} ({effect.RemainingDuration} turns)";
			k_statusIcons.Content.Add(statusLabel);
		}

		// Show defending status
		if (k_character.IsDefending) {
			var defendIcon = new GameLabel("🛡")
				.SetStyle(LabelStyle.BodySmall)
				.SetColor(LabelColor.Info)
				.SetMarginRight(spacing.XXS)
				.Build();
			defendIcon.tooltip = "Defending";
			k_statusIcons.Content.Add(defendIcon);
		}
	}

	private void OnMouseEnter() {
		if (!k_character.IsAlive) return;

		var theme = GameTheme.Current;
		k_panel.SetBackgroundColor(theme.Colors.BackgroundElevated);
	}

	private void OnMouseLeave() {
		var theme = GameTheme.Current;
		k_panel.SetBackgroundColor(theme.Colors.Surface);
	}

	private void OnThemeChanged(GameTheme theme) {
		// Re-apply styling on theme change
		Refresh();
	}

	private static string GetConditionIcon(StatusCondition condition) {
		return condition switch {
			StatusCondition.Poisoned => "🤢",
			StatusCondition.Burning => "🔥",
			StatusCondition.Frozen => "❄",
			StatusCondition.Stunned => "💫",
			StatusCondition.Blinded => "🙈",
			StatusCondition.Silenced => "🤐",
			StatusCondition.Weakened => "📉",
			StatusCondition.Empowered => "📈",
			StatusCondition.Shielded => "🛡",
			StatusCondition.Regenerating => "💚",
			StatusCondition.Bleeding => "🩸",
			StatusCondition.Cursed => "☠",
			StatusCondition.Blessed => "✨",
			StatusCondition.Rooted => "🌱",
			StatusCondition.Charmed => "💕",
			StatusCondition.Slowed => "🐢",
			StatusCondition.Frightened => "😨",
			StatusCondition.Confused => "❓",
			StatusCondition.Invisible => "👻",
			StatusCondition.Hasted => "⚡",
			StatusCondition.Undying => "💀",
			_ => "❓"
		};
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}