using System;
using System.Collections.Generic;
using System.Linq;
using RPGGame.Core;
using RPGGame.Core.Characters.Creation;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine.UIElements;

namespace RPGGame.UI.CharacterCreation;

/// <summary>
/// Browsing panel for selecting skills or spells during character creation.
/// Supports filtering by school, cost, and search.
/// </summary>
public class SkillSpellBrowserPanel : VisualElement {
	#region Private Fields

	private readonly CharacterCreationManager k_manager;
	private readonly GameDb k_gameDb;
	private readonly bool k_isSpellMode;

	private GameFilteredList<Proto>? k_filteredList;
	private GameContainer? k_detailPanel;
	private GameLabel? k_detailName;
	private GameLabel? k_detailDescription;
	private GameLabel? k_detailCost;
	private GameButton? k_addButton;

	private Proto? k_selectedProto;
	private bool k_filtersInitialized;

	#endregion

	#region Events

	public event Action<Proto>? OnItemAdded;
	public event Action? OnClosed;

	#endregion

	#region Constructor

	public SkillSpellBrowserPanel(CharacterCreationManager manager, GameDb gameDb, bool isSpellMode) {
		k_manager = manager;
		k_gameDb = gameDb;
		k_isSpellMode = isSpellMode;

		BuildUI();
		PopulateItems();

		k_manager.OnStateChanged += OnStateChanged;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var dialogStyles = theme.Components.Dialog;
		var windowStyles = theme.Components.Window;

		style.flexDirection = FlexDirection.Column;
		style.flexGrow = 1;
		style.flexShrink = 0;

		// Main container - use theme sizing
		var mainContainer = new GameContainer("browser-main")
			.SetColumn()
			.SetGrow()
			.SetBackgroundColor(colors.Surface)
			.SetLeftSeparator(colors.SurfaceBorder, borders.WidthThin)
			.SetPadding(spacing.LG)
			.SetMinWidth(dialogStyles.LargeWidth)
			.SetMinHeight(windowStyles.MinHeight)
			.Build();

		// Header
		var headerContainer = new GameContainer("browser-header")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.MD)
			.Build();

		var title = new GameLabel(k_isSpellMode ? "Select Spell" : "Select Skill")
			.SetStyle(LabelStyle.TitleLarge)
			.Build();

		var closeButton = new GameButton("✕")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.OnClick(() => OnClosed?.Invoke())
			.Build();

		headerContainer
			.AddChild(title)
			.AddChild(closeButton);

		// Main content: List + Detail
		var contentContainer = new GameContainer("browser-content")
			.SetRow()
			.SetGrow()
			.Build();

		// Filtered list - use proportional sizing
		var listContainer = new GameContainer("list-container")
			.SetGrow()
			.SetMarginRight(spacing.LG)
			.SetMinHeight(dialogStyles.ScrollableContentHeight)
			.Build();

		k_filteredList = new GameFilteredList<Proto>()
			.SetItemTemplate(CreateItemRow)
			.SetSearchTextExtractor(p => p.DisplayText.Name)
			.SetShowSearch(true)
			.SetShowCount(true)
			.SetEmptyMessage(k_isSpellMode ? "No spells available" : "No skills available")
			.OnItemSelected(OnItemSelected)
			.OnItemDoubleClicked(OnItemDoubleClicked);

		listContainer.AddChild(k_filteredList);

		// Detail panel - use theme sizing
		float detailWidth = dialogStyles.SmallWidth * 0.5f;
		k_detailPanel = new GameContainer("detail-panel")
			.SetWidth(detailWidth)
			.SetMinWidth(detailWidth)
			.SetBackgroundColor(colors.BackgroundSecondary)
			.SetBorderRadius(borders.RadiusMD)
			.SetPadding(spacing.LG)
			.Build();

		k_detailName = new GameLabel("")
			.SetStyle(LabelStyle.TitleMedium)
			.SetMarginBottom(spacing.SM)
			.Build();

		k_detailDescription = new GameLabel("")
			.SetStyle(LabelStyle.BodyMedium)
			.SetColor(LabelColor.Secondary)
			.SetWhiteSpace(WhiteSpace.Normal)
			.SetMarginBottom(spacing.LG)
			.Build();

		k_detailCost = new GameLabel("")
			.SetStyle(LabelStyle.LabelLarge)
			.SetColor(LabelColor.Warning)
			.SetMarginBottom(spacing.LG)
			.Build();

		k_addButton = new GameButton(k_isSpellMode ? "Learn Spell" : "Learn Skill")
			.SetVariant(ButtonVariant.Primary)
			.SetFullWidth()
			.OnClick(OnAddClicked)
			.Build();
		k_addButton.SetEnabled(false);

		k_detailPanel
			.AddChild(k_detailName)
			.AddChild(k_detailDescription)
			.AddChild(k_detailCost)
			.AddChild(k_addButton);

		contentContainer
			.AddChild(listContainer)
			.AddChild(k_detailPanel);

		mainContainer
			.AddChild(headerContainer)
			.AddChild(contentContainer);

		Add(mainContainer);
	}

	private VisualElement CreateItemRow(Proto proto) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var row = new GameContainer("item-row")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.XS, spacing.SM, spacing.XS, spacing.SM)
			.Build();

		var nameLabel = new GameLabel(proto.DisplayText.Name)
			.SetStyle(LabelStyle.BodyMedium)
			.Build();

		int cost = GetItemCost(proto);
		var costLabel = new GameLabel($"{cost} pts")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Warning)
			.Build();

		row
			.AddChild(nameLabel)
			.AddChild(costLabel);

		return row;
	}

	#endregion

	#region Data Population

	private void PopulateItems() {
		if (k_filteredList == null) return;

		List<Proto> items;

		if (k_isSpellMode) {
			items = k_manager.GetAvailableSpells()
				.Cast<Proto>()
				.ToList();

			// Only add filters once
			if (!k_filtersInitialized) {
				k_filteredList
					.AddFilter("evocation", "Evocation",
						p => p is SpellProto s && s.School == Core.Spells.SpellSchool.Evocation)
					.AddFilter("abjuration", "Abjuration",
						p => p is SpellProto s && s.School == Core.Spells.SpellSchool.Abjuration)
					.AddFilter("healing", "Healing",
						p => p is SpellProto s && s.HealingDice.SidesOfDice > 0);
			}
		} else {
			items = k_manager.GetAvailableSkills()
				.Cast<Proto>()
				.ToList();

			// Only add filters once
			if (!k_filtersInitialized) {
				k_filteredList
					.AddFilter("combat", "Combat",
						p => p is SkillProto s && s.HasTag(Ids.Tags.Meta.CombatSkill))
					.AddFilter("magic", "Magic",
						p => p is SkillProto s && s.HasTag(Ids.Tags.Meta.MagicSkill))
					.AddFilter("stealth", "Stealth",
						p => p is SkillProto s && s.HasTag(Ids.Tags.Meta.StealthSkill))
					.AddFilter("survival", "Survival",
						p => p is SkillProto s && s.HasTag(Ids.Tags.Meta.SurvivalSkill));
			}
		}

		k_filtersInitialized = true;
		k_filteredList.SetItems(items).Build();
	}

	#endregion

	#region Event Handlers

	private void OnItemSelected(Proto proto) {
		k_selectedProto = proto;
		UpdateDetailPanel();
	}

	private void OnItemDoubleClicked(Proto proto) {
		k_selectedProto = proto;
		OnAddClicked();
	}

	private void OnAddClicked() {
		if (k_selectedProto == null) return;

		bool success = false;

		if (k_isSpellMode && k_selectedProto is SpellProto spell) {
			success = k_manager.AddSpell(spell.Id);
		} else if (!k_isSpellMode && k_selectedProto is SkillProto skill) {
			success = k_manager.AddSkill(skill.Id);
		}

		if (success) {
			OnItemAdded?.Invoke(k_selectedProto);
			PopulateItems();
			k_selectedProto = null;
			UpdateDetailPanel();
		}
	}

	private void OnStateChanged(CharacterCreationState state) {
		PopulateItems();
		UpdateDetailPanel();
	}

	#endregion

	#region Helpers

	private void UpdateDetailPanel() {
		if (k_detailName == null || k_detailDescription == null || k_detailCost == null || k_addButton == null) {
			return;
		}

		var colors = GameTheme.Current.Colors;

		if (k_selectedProto == null) {
			k_detailName.SetText("Select an item");
			k_detailDescription.SetText("Choose a skill or spell from the list to view details.");
			k_detailCost.SetText("");
			k_addButton.SetEnabled(false);
			return;
		}

		k_detailName.SetText(k_selectedProto.DisplayText.Name);
		k_detailDescription.SetText(k_selectedProto.DisplayText.Description);

		int cost = GetItemCost(k_selectedProto);
		k_detailCost.SetText($"Cost: {cost} points");

		bool canAfford = k_manager.CurrentState.PointsRemaining >= cost;
		bool canAdd = k_isSpellMode
			? k_manager.CanAddSpell((k_selectedProto as SpellProto)!.Id)
			: k_manager.CanAddSkill((k_selectedProto as SkillProto)!.Id);

		k_addButton.SetEnabled(canAdd);

		if (!canAfford) {
			k_detailCost.SetColor(colors.Error);
		} else {
			k_detailCost.SetColor(LabelColor.Warning);
		}
	}

	private int GetItemCost(Proto proto) {
		if (proto is SpellProto spell) {
			return k_manager.GetSpellCost(spell.Id);
		} else if (proto is SkillProto skill) {
			return k_manager.GetSkillCost(skill.Id);
		}
		return 0;
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		k_manager.OnStateChanged -= OnStateChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}