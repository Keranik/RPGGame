using System;
using System.Collections.Generic;
using System.Linq;
using RPGGame.Core;
using RPGGame.Core.Characters;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Simulation;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// A unified panel for displaying character or enemy details.
/// Supports Compact, Summary, and Full view modes with optional expansion.
/// Works with LiveCharacter for both player characters and enemies.
/// </summary>
public class GameEntityDetailsPanel : VisualElement {
	#region Private Fields

	private readonly GameDb k_gameDb;
	private readonly GameSession k_session;
	private InventoryManager k_inventoryManager => GameServices.Inventory;

	private EntityViewMode k_viewMode = EntityViewMode.Compact;
	private ExpandMode k_expandMode = ExpandMode.None;
	private bool k_isExpanded = false;

	// Layout containers
	private VisualElement k_root = null!;
	private VisualElement k_headerRow = null!;
	private VisualElement k_barsSection = null!;
	private VisualElement k_attributesSection = null!;
	private VisualElement k_weaponSection = null!;
	private VisualElement k_detailsSection = null!;
	private VisualElement k_expandButtonContainer = null!;

	// Header elements
	private VisualElement k_portrait = null!;
	private Label k_portraitInitials = null!;
	private GameLabel k_nameLabel = null!;
	private GameLabel k_levelLabel = null!;
	private GameLabel k_classLabel = null!;

	// Resource bars
	private GameStatBar k_healthBar = null!;
	private GameStatBar k_manaBar = null!;
	private GameStatBar k_staminaBar = null!;
	private GameStatBar k_fatigueBar = null!;
	private GameStatBar k_xpBar = null!;

	// Weapon display
	private GameLabel k_weaponLabel = null!;

	// Attribute labels (for Summary/Full)
	private readonly Dictionary<string, GameLabel> k_attributeLabels = new();

	// Callbacks
	private Action? k_onExpandClicked;
	private Action? k_onOpenDetailsClicked;

	// Override character (for displaying enemies or other characters)
	private LiveCharacter? k_overrideCharacter;

	#endregion

	#region Constructor

	/// <summary>
	/// Creates a new entity details panel with dependency injection.
	/// </summary>
	public GameEntityDetailsPanel(
		GameDb gameDb,
		GameSession session
	) {
		k_gameDb = gameDb;
		k_session = session;

		BuildUI();
		ApplyTheme();

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent Configuration

	/// <summary>
	/// Sets a specific character to display (overrides session character).
	/// Use this for displaying enemies or other non-player characters.
	/// </summary>
	public GameEntityDetailsPanel SetCharacter(LiveCharacter? character) {
		k_overrideCharacter = character;
		RefreshUI();
		return this;
	}

	/// <summary>
	/// Clears the override character, reverting to session's current character.
	/// </summary>
	public GameEntityDetailsPanel ClearCharacterOverride() {
		k_overrideCharacter = null;
		RefreshUI();
		return this;
	}

	/// <summary>
	/// Sets the view mode (Compact, Summary, Full).
	/// </summary>
	public GameEntityDetailsPanel SetViewMode(EntityViewMode mode) {
		k_viewMode = mode;
		UpdateViewMode();
		return this;
	}

	/// <summary>
	/// Sets the expand mode (None, InPlace, NewWindow).
	/// </summary>
	public GameEntityDetailsPanel SetExpandMode(ExpandMode mode) {
		k_expandMode = mode;
		UpdateExpandButton();
		return this;
	}

	/// <summary>
	/// Called when the expand button is clicked (InPlace mode).
	/// </summary>
	public GameEntityDetailsPanel OnExpandClicked(Action callback) {
		k_onExpandClicked = callback;
		return this;
	}

	/// <summary>
	/// Called when "Open Details" is clicked (NewWindow mode).
	/// </summary>
	public GameEntityDetailsPanel OnOpenDetailsClicked(Action callback) {
		k_onOpenDetailsClicked = callback;
		return this;
	}

	/// <summary>
	/// Sets the panel width (overrides style-based width).
	/// </summary>
	public GameEntityDetailsPanel SetWidth(float width) {
		style.width = width;
		return this;
	}

	/// <summary>
	/// Sets the panel width using Length (overrides style-based width).
	/// </summary>
	public GameEntityDetailsPanel SetWidth(Length width) {
		style.width = width;
		return this;
	}

	/// <summary>
	/// Builds and returns the panel (fluent terminal).
	/// </summary>
	public GameEntityDetailsPanel Build() {
		UpdateViewMode();
		RefreshUI();
		return this;
	}

	#endregion

	#region Character Resolution

	/// <summary>
	/// Gets the character to display (override or session character).
	/// </summary>
	private LiveCharacter? GetDisplayCharacter() {
		return k_overrideCharacter ?? k_session.CurrentRun?.Character;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var styles = theme.Components.EntityDetails;

		// Root container - starts with compact width
		k_root = new VisualElement {
			name = "entity-details-root",
			style = {
				flexDirection = FlexDirection.Column,
				backgroundColor = colors.BackgroundSecondary,
				borderTopLeftRadius = borders.RadiusMD,
				borderTopRightRadius = borders.RadiusMD,
				borderBottomLeftRadius = borders.RadiusMD,
				borderBottomRightRadius = borders.RadiusMD,
				paddingTop = spacing.SM,
				paddingBottom = spacing.SM,
				paddingLeft = spacing.SM,
				paddingRight = spacing.SM,
				minWidth = styles.CompactWidth
			}
		};

		BuildHeaderRow(spacing, colors, styles);
		BuildBarsSection(spacing, styles);
		BuildAttributesSection(spacing, colors);
		BuildWeaponSection(spacing, colors);
		BuildDetailsSection(spacing, colors);
		BuildExpandButton(spacing);

		k_root.Add(k_headerRow);
		k_root.Add(k_barsSection);
		k_root.Add(k_attributesSection);
		k_root.Add(k_weaponSection);
		k_root.Add(k_detailsSection);
		k_root.Add(k_expandButtonContainer);

		Add(k_root);
	}

	private void BuildHeaderRow(SpacingSettings spacing, ColorPalette colors, EntityDetailsPanelStyles styles) {
		k_headerRow = new VisualElement {
			name = "header-row",
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center,
				marginBottom = spacing.XS
			}
		};

		// Portrait placeholder - starts with compact size
		k_portrait = new VisualElement {
			name = "portrait",
			style = {
				width = styles.PortraitSize,
				height = styles.PortraitSize,
				backgroundColor = colors.Surface,
				borderTopLeftRadius = styles.PortraitSize / 2,
				borderTopRightRadius = styles.PortraitSize / 2,
				borderBottomLeftRadius = styles.PortraitSize / 2,
				borderBottomRightRadius = styles.PortraitSize / 2,
				marginRight = spacing.SM,
				alignItems = Align.Center,
				justifyContent = Justify.Center
			}
		};

		k_portraitInitials = new Label("?") {
			name = "portrait-initials",
			style = {
				fontSize = styles.PortraitSize * 0.4f,
				color = colors.TextTertiary,
				unityTextAlign = TextAnchor.MiddleCenter
			}
		};
		k_portrait.Add(k_portraitInitials);

		// Info column
		var infoColumn = new VisualElement {
			name = "info-column",
			style = {
				flexDirection = FlexDirection.Column,
				flexGrow = 1
			}
		};

		// Name and level row
		var nameRow = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center
			}
		};

		k_nameLabel = new GameLabel("Unknown")
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.Build();

		k_levelLabel = new GameLabel("Lv 1")
			.SetStyle(LabelStyle.LabelMedium)
			.SetColor(LabelColor.Secondary)
			.SetMarginLeft(spacing.XS)
			.Build();

		nameRow.Add(k_nameLabel);
		nameRow.Add(k_levelLabel);

		k_classLabel = new GameLabel("Class")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		infoColumn.Add(nameRow);
		infoColumn.Add(k_classLabel);

		k_headerRow.Add(k_portrait);
		k_headerRow.Add(infoColumn);
	}

	private void BuildBarsSection(SpacingSettings spacing, EntityDetailsPanelStyles styles) {
		k_barsSection = new VisualElement {
			name = "bars-section",
			style = {
				flexDirection = FlexDirection.Column,
				marginBottom = spacing.XS
			}
		};

		k_healthBar = new GameStatBar()
			.SetLabel("HP")
			.SetVariant(StatBarVariant.Dynamic)
			.SetDynamicBaseVariant(StatBarVariant.Health)
			.SetRange(0, 100)
			.SetValue(100)
			.SetShowValue()
			.SetHeight(styles.BarHeightCompact)
			.Build();

		k_manaBar = new GameStatBar()
			.SetLabel("MP")
			.SetVariant(StatBarVariant.Mana)
			.SetRange(0, 50)
			.SetValue(50)
			.SetShowValue()
			.SetHeight(styles.BarHeightCompact)
			.Build();
		k_manaBar.style.marginTop = spacing.XXS;

		k_staminaBar = new GameStatBar()
			.SetLabel("ST")
			.SetVariant(StatBarVariant.Dynamic)
			.SetDynamicBaseVariant(StatBarVariant.Stamina)
			.SetRange(0, 100)
			.SetValue(100)
			.SetShowValue()
			.SetHeight(styles.BarHeightCompact)
			.Build();
		k_staminaBar.style.marginTop = spacing.XXS;

		k_fatigueBar = new GameStatBar()
			.SetLabel("FT")
			.SetVariant(StatBarVariant.Fatigue)
			.SetDynamicThresholds(warningAt: 0.3f, dangerAt: 0.1f, invert: true)
			.SetRange(0, 100)
			.SetValue(0)
			.SetShowValue()
			.SetHeight(styles.BarHeightCompact)
			.Build();
		k_fatigueBar.style.marginTop = spacing.XXS;

		k_xpBar = new GameStatBar()
			.SetLabel("XP")
			.SetVariant(StatBarVariant.Experience)
			.SetRange(0, 100)
			.SetValue(0)
			.SetShowValue()
			.SetHeight(styles.BarHeightCompact)
			.Build();
		k_xpBar.style.marginTop = spacing.XXS;

		k_barsSection.Add(k_healthBar);
		k_barsSection.Add(k_manaBar);
		k_barsSection.Add(k_staminaBar);
		k_barsSection.Add(k_fatigueBar);
		k_barsSection.Add(k_xpBar);
	}

	private void BuildAttributesSection(SpacingSettings spacing, ColorPalette colors) {
		k_attributesSection = new VisualElement {
			name = "attributes-section",
			style = {
				flexDirection = FlexDirection.Row,
				flexWrap = Wrap.Wrap,
				marginBottom = spacing.XS,
				display = DisplayStyle.None
			}
		};

		var attributes = new[] {
			("STR", Ids.Stats.Attributes.Strength),
			("DEX", Ids.Stats.Attributes.Dexterity),
			("CON", Ids.Stats.Attributes.Constitution),
			("INT", Ids.Stats.Attributes.Intelligence),
			("WIS", Ids.Stats.Attributes.Wisdom),
			("CHA", Ids.Stats.Attributes.Charisma)
		};

		foreach (var (abbrev, statId) in attributes) {
			var attrContainer = new VisualElement {
				style = {
					flexDirection = FlexDirection.Row,
					alignItems = Align.Center,
					marginRight = spacing.SM,
					marginBottom = spacing.XXS
				}
			};

			var abbrevLabel = new GameLabel(abbrev)
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Tertiary)
				.Build();

			var valueLabel = new GameLabel("10")
				.SetStyle(LabelStyle.LabelSmall)
				.SetColor(LabelColor.Primary)
				.SetMarginLeft(spacing.XXS)
				.Build();

			k_attributeLabels[statId.Value] = valueLabel;

			attrContainer.Add(abbrevLabel);
			attrContainer.Add(valueLabel);
			k_attributesSection.Add(attrContainer);
		}
	}

	private void BuildWeaponSection(SpacingSettings spacing, ColorPalette colors) {
		k_weaponSection = new VisualElement {
			name = "weapon-section",
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center,
				marginBottom = spacing.XS
			}
		};

		var weaponIcon = new GameLabel("⚔")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Tertiary)
			.Build();

		k_weaponLabel = new GameLabel("Unarmed")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.SetMarginLeft(spacing.XXS)
			.Build();

		k_weaponSection.Add(weaponIcon);
		k_weaponSection.Add(k_weaponLabel);
	}

	private void BuildDetailsSection(SpacingSettings spacing, ColorPalette colors) {
		k_detailsSection = new VisualElement {
			name = "details-section",
			style = {
				flexDirection = FlexDirection.Column,
				display = DisplayStyle.None
			}
		};
	}

	private void BuildExpandButton(SpacingSettings spacing) {
		k_expandButtonContainer = new VisualElement {
			name = "expand-button-container",
			style = {
				flexDirection = FlexDirection.Row,
				justifyContent = Justify.Center,
				marginTop = spacing.XS,
				display = DisplayStyle.None
			}
		};
	}

	#endregion

	#region View Mode Management

	private void UpdateViewMode() {
		var effectiveMode = k_isExpanded && k_expandMode == ExpandMode.InPlace
			? GetExpandedMode(k_viewMode)
			: k_viewMode;

		var styles = GameTheme.Current.Components.EntityDetails;

		switch (effectiveMode) {
			case EntityViewMode.Compact:
				ApplyCompactStyles(styles);
				break;
			case EntityViewMode.Summary:
				ApplySummaryStyles(styles);
				break;
			case EntityViewMode.Full:
				ApplyFullStyles(styles);
				break;
		}

		UpdateExpandButton();
	}

	private EntityViewMode GetExpandedMode(EntityViewMode current) {
		return current switch {
			EntityViewMode.Compact => EntityViewMode.Summary,
			EntityViewMode.Summary => EntityViewMode.Full,
			_ => EntityViewMode.Full
		};
	}

		private void ApplyCompactStyles(EntityDetailsPanelStyles styles) {
		// Panel width
		k_root.style.minWidth = styles.CompactWidth;

		// Portrait size - compact
		k_portrait.style.width = styles.PortraitSize;
		k_portrait.style.height = styles.PortraitSize;
		k_portrait.style.borderTopLeftRadius = styles.PortraitSize / 2;
		k_portrait.style.borderTopRightRadius = styles.PortraitSize / 2;
		k_portrait.style.borderBottomLeftRadius = styles.PortraitSize / 2;
		k_portrait.style.borderBottomRightRadius = styles.PortraitSize / 2;
		k_portraitInitials.style.fontSize = styles.PortraitSize * 0.4f;

		// Bar heights - compact
		k_healthBar.SetHeight(styles.BarHeightCompact);
		k_manaBar.SetHeight(styles.BarHeightCompact);
		k_staminaBar.SetHeight(styles.BarHeightCompact);
		k_fatigueBar.SetHeight(styles.BarHeightCompact);
		k_xpBar.SetHeight(styles.BarHeightCompact);

		// Visibility - show all core resource bars including stamina/fatigue
		k_staminaBar.style.display = DisplayStyle.Flex;
		k_fatigueBar.style.display = DisplayStyle.Flex;
		k_attributesSection.style.display = DisplayStyle.None;
		k_weaponSection.style.display = DisplayStyle.None;
		k_detailsSection.style.display = DisplayStyle.None;
	}

	private void ApplySummaryStyles(EntityDetailsPanelStyles styles) {
		// Panel width
		k_root.style.minWidth = styles.SummaryWidth;

		// Portrait size - large for summary/full
		k_portrait.style.width = styles.PortraitSizeLarge;
		k_portrait.style.height = styles.PortraitSizeLarge;
		k_portrait.style.borderTopLeftRadius = styles.PortraitSizeLarge / 2;
		k_portrait.style.borderTopRightRadius = styles.PortraitSizeLarge / 2;
		k_portrait.style.borderBottomLeftRadius = styles.PortraitSizeLarge / 2;
		k_portrait.style.borderBottomRightRadius = styles.PortraitSizeLarge / 2;
		k_portraitInitials.style.fontSize = styles.PortraitSizeLarge * 0.4f;

		// Bar heights - full size
		k_healthBar.SetHeight(styles.BarHeight);
		k_manaBar.SetHeight(styles.BarHeight);
		k_staminaBar.SetHeight(styles.BarHeight);
		k_fatigueBar.SetHeight(styles.BarHeight);
		k_xpBar.SetHeight(styles.BarHeight);

		// Visibility
		k_staminaBar.style.display = DisplayStyle.Flex;
		k_fatigueBar.style.display = DisplayStyle.Flex;
		k_attributesSection.style.display = DisplayStyle.Flex;
		k_weaponSection.style.display = DisplayStyle.Flex;
		k_detailsSection.style.display = DisplayStyle.None;
	}

	private void ApplyFullStyles(EntityDetailsPanelStyles styles) {
		// Panel width
		k_root.style.minWidth = styles.FullWidth;

		// Portrait size - large for summary/full
		k_portrait.style.width = styles.PortraitSizeLarge;
		k_portrait.style.height = styles.PortraitSizeLarge;
		k_portrait.style.borderTopLeftRadius = styles.PortraitSizeLarge / 2;
		k_portrait.style.borderTopRightRadius = styles.PortraitSizeLarge / 2;
		k_portrait.style.borderBottomLeftRadius = styles.PortraitSizeLarge / 2;
		k_portrait.style.borderBottomRightRadius = styles.PortraitSizeLarge / 2;
		k_portraitInitials.style.fontSize = styles.PortraitSizeLarge * 0.4f;

		// Bar heights - full size
		k_healthBar.SetHeight(styles.BarHeight);
		k_manaBar.SetHeight(styles.BarHeight);
		k_staminaBar.SetHeight(styles.BarHeight);
		k_fatigueBar.SetHeight(styles.BarHeight);
		k_xpBar.SetHeight(styles.BarHeight);

		// Visibility
		k_staminaBar.style.display = DisplayStyle.Flex;
		k_fatigueBar.style.display = DisplayStyle.Flex;
		k_attributesSection.style.display = DisplayStyle.Flex;
		k_weaponSection.style.display = DisplayStyle.Flex;
		k_detailsSection.style.display = DisplayStyle.Flex;

		// Populate details
		PopulateDetailsSection();
	}

	

	private void UpdateExpandButton() {
		k_expandButtonContainer.Clear();

		switch (k_expandMode) {
			case ExpandMode.None:
				k_expandButtonContainer.style.display = DisplayStyle.None;
				break;

			case ExpandMode.InPlace:
				k_expandButtonContainer.style.display = DisplayStyle.Flex;
				var expandIcon = k_isExpanded ? "▲ Collapse" : "▼ Expand";
				var inPlaceBtn = new GameButton(expandIcon)
					.SetVariant(ButtonVariant.Ghost)
					.SetSize(ButtonSize.Small)
					.OnClick(OnInPlaceExpandClicked)
					.Build();
				k_expandButtonContainer.Add(inPlaceBtn);
				break;

			case ExpandMode.NewWindow:
				k_expandButtonContainer.style.display = DisplayStyle.Flex;
				var openBtn = new GameButton("📋 Open Details")
					.SetVariant(ButtonVariant.Ghost)
					.SetSize(ButtonSize.Small)
					.OnClick(() => k_onOpenDetailsClicked?.Invoke())
					.Build();
				k_expandButtonContainer.Add(openBtn);
				break;
		}
	}

	private void OnInPlaceExpandClicked() {
		k_isExpanded = !k_isExpanded;
		UpdateViewMode();
		k_onExpandClicked?.Invoke();
	}

	#endregion

	#region UI Refresh

	/// <summary>
	/// Refreshes all displayed data from the current character.
	/// </summary>
	public void RefreshUI() {
		var character = GetDisplayCharacter();
		if (character == null) return;

		RefreshHeader(character);
		RefreshBars(character);
		RefreshAttributes(character);
		RefreshWeapon(character);

		// Refresh details if in full mode
		var effectiveMode = k_isExpanded && k_expandMode == ExpandMode.InPlace
			? GetExpandedMode(k_viewMode)
			: k_viewMode;

		if (effectiveMode == EntityViewMode.Full) {
			PopulateDetailsSection();
		}
	}

	private void RefreshHeader(LiveCharacter character) {
		k_nameLabel.SetText(character.Name);
		k_levelLabel.SetText($"Lv {character.Level}");

		string typeText = character.IsPlayer && character.ClassId.HasValue
			? character.ClassId.Value.Value.Replace("Class_", "")
			: character.Type.ToString();
		k_classLabel.SetText(typeText);

		k_portraitInitials.text = GetInitials(character.Name);

		// Color portrait border for enemies based on rarity
		if (character.IsEnemy) {
			var colors = GameTheme.Current.Colors;
			var rarityColor = colors.GetRarityColor(character.Rarity);
			k_portrait.style.borderTopColor = rarityColor;
			k_portrait.style.borderRightColor = rarityColor;
			k_portrait.style.borderBottomColor = rarityColor;
			k_portrait.style.borderLeftColor = rarityColor;
			k_portrait.style.borderTopWidth = 2;
			k_portrait.style.borderRightWidth = 2;
			k_portrait.style.borderBottomWidth = 2;
			k_portrait.style.borderLeftWidth = 2;
		} else {
			// Clear border for non-enemies
			k_portrait.style.borderTopWidth = 0;
			k_portrait.style.borderRightWidth = 0;
			k_portrait.style.borderBottomWidth = 0;
			k_portrait.style.borderLeftWidth = 0;
		}
	}

	private void RefreshBars(LiveCharacter character) {
		// Health - dynamic will auto-warn at low values
		k_healthBar
			.SetRange(0, character.MaxHealth)
			.SetValue(character.CurrentHealth);

		// Mana - hide if character has no mana pool
		k_manaBar
			.SetRange(0, character.MaxMana)
			.SetValue(character.CurrentMana);
		k_manaBar.style.display = character.MaxMana > 0 ? DisplayStyle.Flex : DisplayStyle.None;

		// Stamina - use effective max (reduced by fatigue)
		float fatigue = character.BaseStats.Get(Ids.Stats.Resource.Fatigue);
		float maxFatigue = character.BaseStats.Get(Ids.Stats.Resource.MaxFatigue);
		if (maxFatigue <= 0) maxFatigue = 100f;
		
		float fatigueRatio = Math.Clamp(fatigue / maxFatigue, 0f, 1f);
		float effectiveMaxStamina = character.MaxStamina * (1f - fatigueRatio);

		k_staminaBar
			.SetRange(0, effectiveMaxStamina)
			.SetValue(character.CurrentStamina);

		// Fatigue bar - Fatigue variant auto-handles warning/danger coloring
		if (character.IsPlayer) {
			k_fatigueBar.style.display = DisplayStyle.Flex;
			k_fatigueBar
				.SetRange(0, maxFatigue)
				.SetValue(fatigue);
		} else {
			k_fatigueBar.style.display = DisplayStyle.None;
		}

		// XP - only show for player characters
		if (character.IsPlayer) {
			k_xpBar.style.display = DisplayStyle.Flex;
			k_xpBar
				.SetRange(0, character.ExperienceToNextLevel)
				.SetValue(character.Experience);
		} else {
			k_xpBar.style.display = DisplayStyle.None;
		}
	}

	private void RefreshAttributes(LiveCharacter character) {
		var colors = GameTheme.Current.Colors;

		var attributes = new (string key, StatProto.ID statId)[] {
			(Ids.Stats.Attributes.Strength.Value, Ids.Stats.Attributes.Strength),
			(Ids.Stats.Attributes.Dexterity.Value, Ids.Stats.Attributes.Dexterity),
			(Ids.Stats.Attributes.Constitution.Value, Ids.Stats.Attributes.Constitution),
			(Ids.Stats.Attributes.Intelligence.Value, Ids.Stats.Attributes.Intelligence),
			(Ids.Stats.Attributes.Wisdom.Value, Ids.Stats.Attributes.Wisdom),
			(Ids.Stats.Attributes.Charisma.Value, Ids.Stats.Attributes.Charisma)
		};

		foreach (var (key, statId) in attributes) {
			if (!k_attributeLabels.TryGetValue(key, out var label)) continue;

			int value = character.GetStatInt(statId);
			float baseValue = character.BaseStats.Get(statId);

			label.SetText(value.ToString());

			if (value > baseValue) {
				label.SetColor(colors.Success);
			} else if (value < baseValue) {
				label.SetColor(colors.Error);
			} else {
				label.SetColor(colors.TextPrimary);
			}
		}
	}

	private void RefreshWeapon(LiveCharacter character) {
		string weaponText = "Unarmed";

		if (character.IsPlayer) {
			var mainHand = k_inventoryManager.GetEquippedItem(SlotType.MainHand);
			if (mainHand != null) {
				weaponText = mainHand.DisplayName;
				var damageBonus = mainHand.GetBonusTo(Ids.Stats.Combat.DamBonus);
				if (damageBonus != 0) {
					string sign = damageBonus > 0 ? "+" : "";
					weaponText += $" ({sign}{damageBonus:F0})";
				}
			}
		} else {
			weaponText = $"{character.UnarmedDamage} {character.UnarmedDamageType}";
		}

		k_weaponLabel.SetText(weaponText);
	}

	private void PopulateDetailsSection() {
		k_detailsSection.Clear();

		var character = GetDisplayCharacter();
		if (character == null) return;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		AddDetailRow("AC", character.ArmorClass.ToString(), colors.TextPrimary);
		AddDetailRow("Attack", $"+{character.AttackBonus}", colors.TextPrimary);
		AddDetailRow("Damage", $"+{character.DamageBonus}", colors.TextPrimary);

		if (character.IsEnemy) {
			var resistances = GetSignificantResistances(character);
			if (resistances.Count > 0) {
				var divider = new GameDivider()
					.SetMargin(spacing.XS, 0)
					.Build();
				k_detailsSection.Add(divider);

				var resistHeader = new GameLabel("Resistances")
					.SetStyle(LabelStyle.Caption)
					.SetColor(LabelColor.Tertiary)
					.Build();
				k_detailsSection.Add(resistHeader);

				foreach (var (type, value) in resistances) {
					var color = value > 0 ? colors.Success : colors.Error;
					var prefix = value > 0 ? "+" : "";
					AddDetailRow(type.ToString(), $"{prefix}{value:P0}", color);
				}
			}
		}

		if (character.KnownSpells.Count > 0) {
			AddDetailRow("Spells", character.KnownSpells.Count.ToString(), colors.Info);
		}

		if (character.Abilities.Count > 0) {
			AddDetailRow("Abilities", character.Abilities.Count.ToString(), colors.Warning);
		}
	}

	private void AddDetailRow(string label, string value, Color valueColor) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var row = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				justifyContent = Justify.SpaceBetween,
				marginBottom = spacing.XXS
			}
		};

		var labelElement = new GameLabel(label)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		var valueElement = new GameLabel(value)
			.SetStyle(LabelStyle.LabelSmall)
			.Build();
		valueElement.style.color = valueColor;

		row.Add(labelElement);
		row.Add(valueElement);
		k_detailsSection.Add(row);
	}

	private List<(DamageType type, float value)> GetSignificantResistances(LiveCharacter character) {
		var result = new List<(DamageType, float)>();

		var damageTypes = Enum.GetValues(typeof(DamageType)).Cast<DamageType>();

		foreach (var type in damageTypes) {
			var resistance = character.GetResistance(type);
			var vulnerability = character.GetVulnerability(type);

			if (resistance > 0.1f) {
				result.Add((type, resistance));
			} else if (vulnerability > 0.1f) {
				result.Add((type, -vulnerability));
			}
		}

		return result.OrderByDescending(r => Math.Abs(r.Item2)).Take(3).ToList();
	}

	private string GetInitials(string name) {
		if (string.IsNullOrEmpty(name)) return "?";

		var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (words.Length == 1) {
			return words[0].Length > 1 ? words[0][..2].ToUpper() : words[0].ToUpper();
		}

		return string.Concat(words.Take(2).Select(w => char.ToUpper(w[0])));
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		k_root.style.backgroundColor = colors.BackgroundSecondary;
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
		UpdateViewMode(); // Re-apply view mode styles
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}