using System.Collections.Generic;
using RPGGame.Core;
using RPGGame.Core.Characters.Creation;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.CharacterCreation;

/// <summary>
/// Panel for allocating attribute points during character creation.
/// Shows current values, costs, and +/- buttons.
/// </summary>
public class AttributeAllocatorPanel : VisualElement {
	#region Private Fields

	private readonly CharacterCreationManager k_manager;
	private readonly Dictionary<StatProto.ID, AttributeRow> k_rows = new();

	#endregion

	#region Constructor

	public AttributeAllocatorPanel(CharacterCreationManager manager) {
		k_manager = manager;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		style.flexDirection = FlexDirection.Column;
		// Add top margin - approximately 10% of typical screen height (216px for 2160px screen)
		style.marginTop = spacing.XXXL;

		// Header
		var header = new GameLabel("Attributes")
			.SetStyle(LabelStyle.TitleLarge)
			.SetMarginBottom(spacing.MD)
			.Build();
		Add(header);

		// Attribute rows
		CreateAttributeRow(Ids.Stats.Attributes.Strength, "STR", "Strength");
		CreateAttributeRow(Ids.Stats.Attributes.Dexterity, "DEX", "Dexterity");
		CreateAttributeRow(Ids.Stats.Attributes.Constitution, "CON", "Constitution");
		CreateAttributeRow(Ids.Stats.Attributes.Intelligence, "INT", "Intelligence");
		CreateAttributeRow(Ids.Stats.Attributes.Wisdom, "WIS", "Wisdom");
		CreateAttributeRow(Ids.Stats.Attributes.Charisma, "CHA", "Charisma");

		// Subscribe to state changes
		k_manager.OnStateChanged += OnStateChanged;

		// Initial refresh
		Refresh();
	}

	#endregion

	#region Private Methods

	private void CreateAttributeRow(StatProto.ID attributeId, string abbrev, string fullName) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var listStyles = theme.Components.List;

		var row = new GameContainer($"attr-row-{abbrev}")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetSpaceBetween()
			.SetMinHeight(listStyles.ItemHeightCompact)
			.SetMarginBottom(spacing.SM)
			.SetPadding(spacing.SM, spacing.MD, spacing.SM, spacing.MD)
			.Build();

		// Left side: Name - wider to prevent overlap
		var nameContainer = new GameContainer("name-container")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetMinWidth(200)
			.Build();

		var abbrevLabel = new GameLabel(abbrev)
			.SetStyle(LabelStyle.TitleMedium)
			.SetColor(LabelColor.Primary)
			.SetWidth(60)
			.Build();

		var fullNameLabel = new GameLabel(fullName)
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.SetMarginLeft(spacing.SM)
			.Build();

		nameContainer
			.AddChild(abbrevLabel)
			.AddChild(fullNameLabel);

		// Center: Value and modifier - more space
		var valueContainer = new GameContainer("value-container")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetJustifyContent(Justify.Center)
			.SetMinWidth(120)
			.Build();

		var valueLabel = new GameLabel("10")
			.SetStyle(LabelStyle.DisplaySmall)
			.SetColor(LabelColor.Primary)
			.SetWidth(50)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();

		var modifierLabel = new GameLabel("(+0)")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.SetWidth(60)
			.SetMarginLeft(spacing.XS)
			.Build();

		valueContainer
			.AddChild(valueLabel)
			.AddChild(modifierLabel);

		// Right side: Buttons and cost
		var buttonStyles = theme.Components.Button;
		var buttonContainer = new GameContainer("button-container")
			.SetRow()
			.SetAlignItems(Align.Center)
			.Build();

		var decreaseButton = new GameButton("-")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(() => OnDecreaseClicked(attributeId))
			.SetMinWidth(buttonStyles.Small.Height)
			.SetMinHeight(buttonStyles.Small.Height)
			.Build();

		var increaseButton = new GameButton("+")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Small)
			.OnClick(() => OnIncreaseClicked(attributeId))
			.SetMinWidth(buttonStyles.Small.Height)
			.SetMinHeight(buttonStyles.Small.Height)
			.SetMarginLeft(spacing.SM)
			.Build();

		var costLabel = new GameLabel("")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Tertiary)
			.SetWidth(80)
			.SetMarginLeft(spacing.MD)
			.SetTextAlign(TextAnchor.MiddleRight)
			.Build();

		buttonContainer
			.AddChild(decreaseButton)
			.AddChild(increaseButton)
			.AddChild(costLabel);

		row
			.AddChild(nameContainer)
			.AddChild(valueContainer)
			.AddChild(buttonContainer);

		Add(row);

		k_rows[attributeId] = new AttributeRow {
			ValueLabel = valueLabel,
			ModifierLabel = modifierLabel,
			DecreaseButton = decreaseButton,
			IncreaseButton = increaseButton,
			CostLabel = costLabel
		};
	}

	private void OnIncreaseClicked(StatProto.ID attributeId) {
		k_manager.IncreaseAttribute(attributeId);
	}

	private void OnDecreaseClicked(StatProto.ID attributeId) {
		k_manager.DecreaseAttribute(attributeId);
	}

	private void OnStateChanged(CharacterCreationState state) {
		Refresh();
	}

	public void Refresh() {
		var state = k_manager.CurrentState;
		var colors = GameTheme.Current.Colors;
		bool isLocked = k_manager.IsModificationLocked;

		foreach (var (attributeId, row) in k_rows) {
			int value = state.Attributes.GetValueOrDefault(attributeId, 10);
			int modifier = (value - 10) / 2;
			string modSign = modifier >= 0 ? "+" : "";

			row.ValueLabel.SetText(value.ToString());
			row.ModifierLabel.SetText($"({modSign}{modifier})");

			// Update modifier color
			if (modifier > 0) {
				row.ModifierLabel.SetColor(colors.Success);
			} else if (modifier < 0) {
				row.ModifierLabel.SetColor(colors.Error);
			} else {
				row.ModifierLabel.SetColor(LabelColor.Secondary);
			}

			// Update button states - disabled when locked
			bool canIncrease = k_manager.CanIncreaseAttribute(attributeId);
			bool canDecrease = k_manager.CanDecreaseAttribute(attributeId);

			row.IncreaseButton.SetEnabled(canIncrease && !isLocked);
			row.DecreaseButton.SetEnabled(canDecrease && !isLocked);

			// Update cost display
			if (canIncrease && !isLocked) {
				int cost = k_manager.GetAttributeIncreaseCost(attributeId);
				row.CostLabel.SetText($"({cost} pts)");
			} else if (isLocked) {
				row.CostLabel.SetText("🔒");
			} else {
				row.CostLabel.SetText("");
			}
		}
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		k_manager.OnStateChanged -= OnStateChanged;
		base.RemoveFromHierarchy();
	}

	#endregion

	#region Helper Class

	private class AttributeRow {
		public GameLabel ValueLabel { get; set; } = null!;
		public GameLabel ModifierLabel { get; set; } = null!;
		public GameButton DecreaseButton { get; set; } = null!;
		public GameButton IncreaseButton { get; set; } = null!;
		public GameLabel CostLabel { get; set; } = null!;
	}

	#endregion
}