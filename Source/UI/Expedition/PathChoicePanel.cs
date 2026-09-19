using RPGGame.Core;
using RPGGame.Core.Expedition;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Expedition;

/// <summary>
/// Modal panel for choosing between branching paths at intersections.
/// Displays path options side-by-side with direction indicators, bonuses/penalties,
/// and a separate panel showing current run modifiers.
/// 
/// Dark fantasy gothic styling using GameTheme.
/// </summary>
public class PathChoicePanel : VisualElement {
	#region Fields

	private readonly ExpeditionManager k_expeditionManager;

	private GameContainer k_overlay = null!;
	private GameContainer k_mainContainer = null!;
	private GameContainer k_choicePanel = null!;
	private GameContainer k_modifiersPanel = null!;
	private GameContainer k_pathsRow = null!;

	private PathChoiceData? k_hoveredChoice;

	#endregion

	#region Events

	public event Action<PathChoiceData>? OnPathChosen;

	#endregion

	#region Constructor

	public PathChoicePanel(ExpeditionManager expeditionManager) {
		k_expeditionManager = expeditionManager;

		BuildUI();

		style.display = DisplayStyle.None;

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;

		// Full screen overlay
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		pickingMode = PickingMode.Position;

		// Semi-transparent overlay
		k_overlay = new GameContainer("path-choice-overlay")
			.SetAbsoluteFill()
			.SetCenter()
			.SetBackgroundColor(colors.BackgroundOverlay)
			.Build();
		k_overlay.pickingMode = PickingMode.Position;

		// Main horizontal container (choice panel + modifiers panel)
		k_mainContainer = new GameContainer("main-container")
			.SetRow()
			.SetAlignItems(Align.FlexStart)
			.SetJustifyContent(Justify.Center)
			.Build();

		// === CHOICE PANEL (left/center) ===
		k_choicePanel = new GameContainer("choice-panel")
			.SetColumn()
			.SetBackgroundColor(colors.BackgroundSecondary)
			.SetBorderRadius(borders.RadiusLG)
			.SetBorderWidth(borders.WidthMedium)
			.SetBorderColor(colors.SurfaceBorder)
			.SetPadding(spacing.LG)
			.Build();

		// Header
		var header = new GameLabel("Choose Your Path")
			.SetStyle(LabelStyle.HeadlineMedium)
			.SetColor(LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetMarginBottom(spacing.SM)
			.Build();

		// Subtitle
		var subtitle = new GameLabel("Each path offers different challenges and rewards")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetMarginBottom(spacing.MD)
			.Build();

		// Divider after header
		var headerDivider = new GameDivider()
			.SetSpacing(spacing.SM)
			.Build();

		// Paths row container
		k_pathsRow = new GameContainer("paths-row")
			.SetRow()
			.SetJustifyContent(Justify.Center)
			.SetAlignItems(Align.Stretch)
			.SetGrow()
			.Build();

		k_choicePanel
			.AddChild(header)
			.AddChild(subtitle)
			.AddChild(headerDivider)
			.AddChild(k_pathsRow);

		// === MODIFIERS PANEL (right side) ===
		k_modifiersPanel = new GameContainer("modifiers-panel")
			.SetColumn()
			.SetBackgroundColor(colors.BackgroundSecondary)
			.SetBorderRadius(borders.RadiusLG)
			.SetBorderWidth(borders.WidthMedium)
			.SetBorderColor(colors.SurfaceBorder)
			.SetPadding(spacing.MD)
			.SetMarginLeft(spacing.LG)
			.Build();

		BuildModifiersPanel();

		k_mainContainer
			.AddChild(k_choicePanel)
			.AddChild(k_modifiersPanel);

		k_overlay.AddChild(k_mainContainer);
		Add(k_overlay);
	}

	private void BuildModifiersPanel() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_modifiersPanel.ClearChildren();

		// Header
		var header = new GameLabel("Active Run Modifiers")
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetMarginBottom(spacing.SM)
			.Build();

		k_modifiersPanel.AddChild(header);

		var divider = new GameDivider()
			.SetMargin(spacing.XS, 0, spacing.SM, 0)
			.Build();
		k_modifiersPanel.AddChild(divider);

		// Get active modifiers from ExpeditionManager
		var modifiers = k_expeditionManager.ActiveModifiers;

		if (modifiers.Count == 0) {
			var noModifiers = new GameLabel("No active modifiers")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Tertiary)
				.SetTextAlign(TextAnchor.MiddleCenter)
				.SetMarginTop(spacing.MD)
				.Build();
			k_modifiersPanel.AddChild(noModifiers);
		} else {
			// Group by bonus vs penalty
			var bonuses = modifiers.Where(m => m.Value > 0).ToList();
			var penalties = modifiers.Where(m => m.Value < 0).ToList();

			if (bonuses.Count > 0) {
				var bonusHeader = new GameLabel("Bonuses")
					.SetStyle(LabelStyle.LabelSmall)
					.SetColor(LabelColor.Success)
					.SetMarginTop(spacing.XS)
					.SetMarginBottom(spacing.XXS)
					.Build();
				k_modifiersPanel.AddChild(bonusHeader);

				foreach (var mod in bonuses) {
					var modRow = CreateModifierRow(mod, isBonus: true);
					k_modifiersPanel.AddChild(modRow);
				}
			}

			if (penalties.Count > 0) {
				var penaltyHeader = new GameLabel("Penalties")
					.SetStyle(LabelStyle.LabelSmall)
					.SetColor(LabelColor.Error)
					.SetMarginTop(spacing.SM)
					.SetMarginBottom(spacing.XXS)
					.Build();
				k_modifiersPanel.AddChild(penaltyHeader);

				foreach (var mod in penalties) {
					var modRow = CreateModifierRow(mod, isBonus: false);
					k_modifiersPanel.AddChild(modRow);
				}
			}
		}

		// Fog strength indicator
		float fogStrength = k_expeditionManager.GetFogStrengthModifier();
		if (fogStrength > 0) {
			var fogDivider = new GameDivider()
				.SetSpacing(spacing.SM)
				.Build();
			k_modifiersPanel.AddChild(fogDivider);

			var fogRow = new GameContainer()
				.SetRow()
				.SetJustifyContent(Justify.SpaceBetween)
				.SetFullWidth()
				.Build();

			fogRow.Add(new GameLabel("Fog Density")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Secondary)
				.Build());

			fogRow.Add(new GameLabel($"{fogStrength:P0}")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Warning)
				.Build());

			k_modifiersPanel.AddChild(fogRow);
		}
	}

	private GameContainer CreateModifierRow(RunModifier mod, bool isBonus) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var row = new GameContainer()
			.SetRow()
			.SetJustifyContent(Justify.SpaceBetween)
			.SetFullWidth()
			.SetPadding(spacing.XXS, spacing.XS)
			.Build();

		// Use ToDisplayName() from StringExtensions to format enum
		var nameLabel = new GameLabel(mod.Type.ToString().ToDisplayName())
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.Build();

		string valueText = isBonus ? $"+{mod.Value:F0}%" : $"{mod.Value:F0}%";
		var valueLabel = new GameLabel(valueText)
			.SetStyle(LabelStyle.Caption)
			.SetColor(isBonus ? LabelColor.Success : LabelColor.Error)
			.Build();

		row.AddChild(nameLabel);
		row.AddChild(valueLabel);

		return row;
	}

	#endregion

	#region Public Methods

	public void ShowChoices(List<PathChoiceData> choices) {
		k_hoveredChoice = null;
		k_pathsRow.ClearChildren();

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var borders = theme.Borders;

		// Sort choices by angle for consistent left/right display
		var sortedChoices = choices
			.OrderBy(c => c.Direction.Degrees)
			.ToList();

		// Determine which is "left" and which is "right" based on angle
		var leftChoices = sortedChoices.Where(c => c.PointsLeft).ToList();
		var rightChoices = sortedChoices.Where(c => c.PointsRight || (!c.PointsLeft && !c.PointsRight)).ToList();

		// If we don't have a clean split, just use first/second
		if (leftChoices.Count == 0 || rightChoices.Count == 0) {
			if (sortedChoices.Count >= 2) {
				leftChoices = [sortedChoices[0]];
				rightChoices = [sortedChoices[1]];
			} else if (sortedChoices.Count == 1) {
				leftChoices = [sortedChoices[0]];
				rightChoices = [];
			}
		}

		// Build left path option
		if (leftChoices.Count > 0) {
			var leftPanel = CreatePathOptionPanel(leftChoices[0]);
			k_pathsRow.AddChild(leftPanel);
		}

		// Vertical divider between paths
		if (leftChoices.Count > 0 && rightChoices.Count > 0) {
			var vertDivider = new GameDivider()
				.SetOrientation(DividerOrientation.Vertical)
				.SetMargin(0, spacing.MD)
				.Build();
			k_pathsRow.AddChild(vertDivider);
		}

		// Build right path option
		if (rightChoices.Count > 0) {
			var rightPanel = CreatePathOptionPanel(rightChoices[0]);
			k_pathsRow.AddChild(rightPanel);
		}

		// Refresh modifiers panel
		BuildModifiersPanel();

		style.display = DisplayStyle.Flex;

		Debug.Log($"PathChoicePanel: Showing {choices.Count} path choices");
	}

	public void Hide() {
		style.display = DisplayStyle.None;
		k_hoveredChoice = null;
	}

	#endregion

	#region Path Option Building

	private GameContainer CreatePathOptionPanel(PathChoiceData choice) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;

		var panel = new GameContainer($"path-option")
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.MD)
			.SetFlexGrow(1)
			.SetBorderRadius(borders.RadiusMD)
			.SetBorderWidth(borders.WidthThin)
			.SetBorderColor(colors.SurfaceBorder)
			.SetBackgroundColor(colors.Surface)
			.Build();

		// Direction indicator (large arrow) - use AngleRPG's built-in direction name
		var directionContainer = new GameContainer("direction")
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.SM)
			.Build();

		var arrow = new GameLabel(GetDirectionArrow(choice.Direction))
			.SetStyle(LabelStyle.DisplayLarge)
			.SetColor(choice.IsRisky ? LabelColor.Warning : LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		// Use AngleRPG.ToDirectionName() directly
		var directionName = new GameLabel(choice.Direction.ToDirectionName())
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		directionContainer
			.AddChild(arrow)
			.AddChild(directionName);

		// Path name
		var nameLabel = new GameLabel(choice.Name)
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Primary)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetMarginBottom(spacing.XS)
			.Build();

		// Terrain info
		var terrainRow = new GameContainer()
			.SetRow()
			.SetJustifyContent(Justify.Center)
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.XS)
			.Build();

		terrainRow.Add(new GameLabel(choice.TerrainName)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.Build());

		terrainRow.Add(new GameLabel($" - {choice.Distance:F0} tiles")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build());

		// Difficulty indicator
		var difficultyRow = new GameContainer()
			.SetRow()
			.SetJustifyContent(Justify.Center)
			.SetMarginBottom(spacing.SM)
			.Build();

		difficultyRow.Add(new GameLabel("Difficulty: ")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.Build());

		difficultyRow.Add(new GameLabel(GetDifficultyDisplay(choice.DifficultyHint))
			.SetStyle(LabelStyle.Caption)
			.SetColor(GetDifficultyColor(choice.DifficultyHint))
			.Build());

		// Divider before bonuses/penalties
		var divider = new GameDivider()
			.SetSpacing(spacing.XS)
			.Build();

		// Bonuses section
		var bonusesContainer = new GameContainer("bonuses")
			.SetColumn()
			.SetFullWidth()
			.SetAlignItems(Align.FlexStart)
			.Build();

		if (choice.Bonuses.Count > 0) {
			var bonusHeader = new GameLabel("Bonuses")
				.SetStyle(LabelStyle.LabelSmall)
				.SetColor(LabelColor.Success)
				.SetMarginBottom(spacing.XXS)
				.Build();
			bonusesContainer.AddChild(bonusHeader);

			foreach (var bonus in choice.Bonuses) {
				var bonusRow = CreateBonusPenaltyRow(bonus.Type, bonus.Value, isBonus: true);
				bonusesContainer.AddChild(bonusRow);
			}
		}

		// Penalties section
		if (choice.Penalties.Count > 0) {
			var penaltyHeader = new GameLabel("Penalties")
				.SetStyle(LabelStyle.LabelSmall)
				.SetColor(LabelColor.Error)
				.SetMarginTop(spacing.XS)
				.SetMarginBottom(spacing.XXS)
				.Build();
			bonusesContainer.AddChild(penaltyHeader);

			foreach (var penalty in choice.Penalties) {
				var penaltyRow = CreateBonusPenaltyRow(penalty.Type, penalty.Value, isBonus: false);
				bonusesContainer.AddChild(penaltyRow);
			}
		}

		// Safe path indicator
		if (!choice.IsRisky) {
			var safeLabel = new GameLabel("Safe Path")
				.SetStyle(LabelStyle.LabelSmall)
				.SetColor(LabelColor.Success)
				.SetTextAlign(TextAnchor.MiddleCenter)
				.SetMarginTop(spacing.XS)
				.Build();
			bonusesContainer.AddChild(safeLabel);
		}

		panel
			.AddChild(directionContainer)
			.AddChild(nameLabel)
			.AddChild(terrainRow)
			.AddChild(difficultyRow)
			.AddChild(divider)
			.AddChild(bonusesContainer);

		// Hover and click handling
		Color normalBg = colors.Surface;
		Color hoverBg = colors.SurfaceHover;
		Color selectedBorder = choice.IsRisky ? colors.Warning : colors.Primary;

		panel.RegisterCallback<MouseEnterEvent>(_ => {
			k_hoveredChoice = choice;
			panel.SetBackgroundColor(hoverBg);
			panel.SetBorderColor(selectedBorder);
			panel.SetBorderWidth(borders.WidthMedium);
		});

		panel.RegisterCallback<MouseLeaveEvent>(_ => {
			if (k_hoveredChoice == choice) {
				k_hoveredChoice = null;
			}
			panel.SetBackgroundColor(normalBg);
			panel.SetBorderColor(colors.SurfaceBorder);
			panel.SetBorderWidth(borders.WidthThin);
		});

		panel.RegisterCallback<ClickEvent>(_ => {
			OnPathSelected(choice);
		});

		return panel;
	}

	private GameContainer CreateBonusPenaltyRow(RunModifierType type, float value, bool isBonus) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var row = new GameContainer()
			.SetRow()
			.SetJustifyContent(Justify.SpaceBetween)
			.SetFullWidth()
			.SetPadding(0, spacing.XS)
			.Build();

		// Use ToDisplayName() from StringExtensions to format enum name
		row.Add(new GameLabel(type.ToString().ToDisplayName())
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.Build());

		string valueText = isBonus ? $"+{value:F0}%" : $"+{value:F0}%";
		row.Add(new GameLabel(valueText)
			.SetStyle(LabelStyle.Caption)
			.SetColor(isBonus ? LabelColor.Success : LabelColor.Error)
			.Build());

		return row;
	}

	#endregion

	#region Direction & Display Helpers

	private string GetDirectionArrow(AngleRPG direction) {
		// Map angle to Unicode arrow character
		float deg = direction.Degrees;

		return deg switch {
			>= 337.5f or < 22.5f => "\u2192",   // → East
			>= 22.5f and < 67.5f => "\u2197",   // ↗ Northeast
			>= 67.5f and < 112.5f => "\u2191",  // ↑ North
			>= 112.5f and < 157.5f => "\u2196", // ↖ Northwest
			>= 157.5f and < 202.5f => "\u2190", // ← West
			>= 202.5f and < 247.5f => "\u2199", // ↙ Southwest
			>= 247.5f and < 292.5f => "\u2193", // ↓ South
			_ => "\u2198"                        // ↘ Southeast
		};
	}

	private string GetDifficultyDisplay(int difficulty) {
		return difficulty switch {
			1 => "Easy",
			2 => "Moderate",
			3 => "Challenging",
			4 => "Hard",
			5 => "Deadly",
			_ => "Unknown"
		};
	}

	private LabelColor GetDifficultyColor(int difficulty) {
		return difficulty switch {
			1 => LabelColor.Success,
			2 => LabelColor.Primary,
			3 => LabelColor.Warning,
			4 or 5 => LabelColor.Error,
			_ => LabelColor.Tertiary
		};
	}

	#endregion

	#region Event Handlers

	private void OnPathSelected(PathChoiceData choice) {
		Hide();
		OnPathChosen?.Invoke(choice);
	}

	#endregion

	#region Theme

	private void OnThemeChanged(GameTheme theme) {
		var colors = theme.Colors;

		// Update overlay background
		k_overlay?.SetBackgroundColor(colors.BackgroundOverlay);

		// Update panel backgrounds
		k_choicePanel?
			.SetBackgroundColor(colors.BackgroundSecondary)
			.SetBorderColor(colors.SurfaceBorder);

		k_modifiersPanel?
			.SetBackgroundColor(colors.BackgroundSecondary)
			.SetBorderColor(colors.SurfaceBorder);
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}