using RPGGame.Core;
using RPGGame.Core.Characters;
using RPGGame.Core.Combat;
using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes.Skills;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Spells;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Screens;

/// <summary>
/// A modal panel for selecting skills and spells during combat.
/// Shows available abilities organized by type with mana costs, cooldowns, and tags.
/// Uses GameSkillSlotDisplay and GameSpellSlotDisplay for rich visual presentation.
/// </summary>
public class CombatSkillsPanel : VisualElement {
	#region Constants

	private const float SLOT_SIZE = 48f;
	private const float SLOT_SIZE_LARGE = 56f;

	#endregion

	#region Private Fields

	private readonly GameDb k_gameDb;
	private readonly CombatManager k_combatManager;

	private GameContainer k_root = null!;
	private GameContainer k_header = null!;
	private GameTabView k_tabView = null!;
	private GameScrollView k_abilitiesScroll = null!;
	private GameScrollView k_spellsScroll = null!;
	private GameContainer k_footer = null!;
	private GameLabel k_manaLabel = null!;
	private GameStatBar k_manaBar = null!;
	private GameContainer k_detailsPanel = null!;
	private GameLabel k_detailsName = null!;
	private GameLabel k_detailsDescription = null!;
	private GameContainer k_detailsTags = null!;
	private GameContainer k_detailsStats = null!;

	private LiveCharacter? k_actor;
	private LiveCharacter? k_selectedTarget;

	private readonly List<GameSkillSlotDisplay> k_skillSlots = [];
	private readonly List<GameSpellSlotDisplay> k_spellSlots = [];

	private object? k_selectedAbility; // CombatAbility or SpellProto

	#endregion

	#region Events

	public event Action<CombatAction>? OnActionSelected;
	public event Action? OnCancelled;

	#endregion

	#region Constructor

	public CombatSkillsPanel(GameDb gameDb, CombatManager combatManager) {
		k_gameDb = gameDb;
		k_combatManager = combatManager;

		BuildUI();

		style.display = DisplayStyle.None;
	}

	#endregion

	#region UI Building

	private void BuildUI() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var dialogStyles = theme.Components.Dialog;

		// Full screen overlay
		style.position = Position.Absolute;
		style.left = 0;
		style.top = 0;
		style.right = 0;
		style.bottom = 0;
		style.alignItems = Align.Center;
		style.justifyContent = Justify.Center;
		pickingMode = PickingMode.Position;

		// Semi-transparent backdrop
		var backdrop = new GameContainer("backdrop")
			.SetAbsoluteFill()
			.SetBackgroundColor(colors.BackgroundOverlay)
			.Build();
		backdrop.pickingMode = PickingMode.Position;
		backdrop.RegisterCallback<ClickEvent>(_ => Hide());
		Add(backdrop);

		// Main panel
		k_root = new GameContainer("skills-panel")
			.SetColumn()
			.SetBackgroundColor(colors.Surface)
			.SetBorderRadius(borders.RadiusLG)
			.SetBorderWidth(borders.WidthMedium)
			.SetBorderColor(colors.SurfaceBorder)
			.SetWidth(dialogStyles.LargeWidth)
			.SetFlexGrow(1f)
			.SetMaxHeight(dialogStyles.ContentMaxHeightCapped)
			.Build();
		k_root.pickingMode = PickingMode.Position;

		BuildHeader();
		BuildContent();
		BuildDetailsPanel();
		BuildFooter();

		Add(k_root);
	}

	private void BuildHeader() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		k_header = new GameContainer("header")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.SM, spacing.MD, spacing.SM, spacing.MD)
			.SetBottomSeparator(colors.SurfaceBorder)
			.Build();

		var titleRow = new GameContainer("title-row")
			.SetRow()
			
			.SetAlignItems(Align.Center)
			.Build();

		var title = new GameLabel("✨ Skills & Spells")
			.SetStyle(LabelStyle.TitleMedium)
			.Build();

		
		titleRow.Add(title);

		// Mana display with bar
		var manaContainer = new GameContainer("mana-container")
			.SetColumn()
			.SetAlignItems(Align.FlexEnd)
			.Build();

		k_manaLabel = new GameLabel("MP: 0/0")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Info)
			.Build();

		k_manaBar = new GameStatBar()
			.SetVariant(StatBarVariant.Mana)
			.SetHeight(8)
			.SetWidth(120)
			.SetShowValue(false)
			.SetAnimateChanges(true)
			.Build();

		manaContainer.Add(k_manaLabel);
		manaContainer.Add(k_manaBar);

		var closeButton = new GameButton("✕")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Small)
			.OnClick(Hide)
			.Build();

		k_header.Add(titleRow);
		k_header.Add(manaContainer);
		k_header.Add(closeButton);

		k_root.Add(k_header);
	}

	private void BuildContent() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_abilitiesScroll = new GameScrollView("abilities-scroll")
			.SetVertical()
			.SetGrow()
			.SetMinHeight(200)
			.Build();

		k_spellsScroll = new GameScrollView("spells-scroll")
			.SetVertical()
			.SetGrow()
			.SetMinHeight(200)
			.Build();

		k_tabView = new GameTabView()
			.AddTab("⚔️ Abilities", k_abilitiesScroll)
			.AddTab("✨ Spells", k_spellsScroll)
			.SetVariant(TabVariant.Default)
			.SetGrow()
			.SetContentPadding(0, spacing.SM)
			.Build();

		k_root.Add(k_tabView);
	}

	private void BuildDetailsPanel() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var borders = theme.Borders;

		k_detailsPanel = new GameContainer("details-panel")
			.SetColumn()
			.SetPadding(spacing.SM)
			.SetBackgroundColor(colors.BackgroundSecondary)
			.SetBorderRadius(borders.RadiusMD)
			.SetMargin(spacing.SM)
			.Build();
		k_detailsPanel.style.display = DisplayStyle.None;

		// Header row
		var headerRow = new GameContainer("details-header")
			.SetRow()
			.SetAlignItems(Align.Center)
			.SetMarginBottom(spacing.XS)
			.Build();

		k_detailsName = new GameLabel("")
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.Build();

		headerRow.Add(k_detailsName);

		// Description
		k_detailsDescription = new GameLabel("")
			.SetStyle(LabelStyle.BodySmall)
			.SetColor(LabelColor.Secondary)
			.Build();
		k_detailsDescription.style.whiteSpace = WhiteSpace.Normal;
		k_detailsDescription.style.marginBottom = spacing.XS;

		// Tags row
		k_detailsTags = new GameContainer("details-tags")
			.SetRow()
			.SetFlexWrap(Wrap.Wrap)
			.SetMarginBottom(spacing.XS)
			.Build();

		// Stats grid
		k_detailsStats = new GameContainer("details-stats")
			.SetRow()
			.SetFlexWrap(Wrap.Wrap)
			.Build();

		k_detailsPanel.Add(headerRow);
		k_detailsPanel.Add(k_detailsDescription);
		k_detailsPanel.Add(k_detailsTags);
		k_detailsPanel.Add(k_detailsStats);

		k_root.Add(k_detailsPanel);
	}

	private void BuildFooter() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		k_footer = new GameContainer("footer")
			.SetRow()
			.SetSpaceBetween()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.SM, spacing.MD, spacing.SM, spacing.MD)
			.SetTopSeparator(colors.SurfaceBorder)
			.Build();

		var hintLabel = new GameLabel("Click to select • Right-click for details")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		var cancelButton = new GameButton("Cancel")
			.SetVariant(ButtonVariant.Ghost)
			.SetSize(ButtonSize.Medium)
			.OnClick(Hide)
			.Build();

		k_footer.Add(hintLabel);
		k_footer.Add(cancelButton);
		k_root.Add(k_footer);
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Shows the panel for the given actor.
	/// </summary>
	public void Show(LiveCharacter actor, LiveCharacter? defaultTarget = null) {
		k_actor = actor;
		k_selectedTarget = defaultTarget ?? k_combatManager.LivingEnemies.FirstOrDefault();
		k_selectedAbility = null;

		RefreshContent();
		HideDetails();
		style.display = DisplayStyle.Flex;
	}

	/// <summary>
	/// Hides the panel.
	/// </summary>
	public void Hide() {
		style.display = DisplayStyle.None;
		ClearSlots();
		OnCancelled?.Invoke();
	}

	#endregion

	#region Content

	private void RefreshContent() {
		if (k_actor == null) return;

		// Update mana display
		float currentMana = k_actor.CurrentMana;
		float maxMana = k_actor.MaxMana;
		k_manaLabel.SetText($"MP: {currentMana:F0}/{maxMana:F0}");
		k_manaBar.SetRange(0, maxMana);
		k_manaBar.SetValue(currentMana);

		RefreshAbilities();
		RefreshSpells();
	}

	private void RefreshAbilities() {
		k_abilitiesScroll.ClearContent();
		ClearSkillSlots();

		if (k_actor == null) return;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var abilities = k_actor.Abilities
			.Where(a => a.Type != CombatAbilityType.Spell)
			.ToList();

		if (abilities.Count == 0) {
			var emptyLabel = new GameLabel("No abilities available")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.SetMargin(spacing.MD)
				.Build();
			k_abilitiesScroll.AddChild(emptyLabel);
			return;
		}

		// Group by type
		var groupedAbilities = abilities.GroupBy(a => a.Type);

		foreach (var group in groupedAbilities) {
			// Group header
			var headerLabel = new GameLabel($"{GetAbilityTypeIcon(group.Key)} {group.Key}")
				.SetStyle(LabelStyle.LabelMedium)
				.SetColor(LabelColor.Secondary)
				.SetMarginTop(spacing.SM)
				.SetMarginBottom(spacing.XS)
				.Build();
			k_abilitiesScroll.AddChild(headerLabel);

			// Ability grid
			var grid = new GameContainer($"ability-grid-{group.Key}")
				.SetRow()
				.SetFlexWrap(Wrap.Wrap)
				.Build();

			foreach (var ability in group) {
				var row = CreateAbilityRow(ability);
				grid.Add(row);
			}

			k_abilitiesScroll.AddChild(grid);
		}
	}

	private void RefreshSpells() {
		k_spellsScroll.ClearContent();
		ClearSpellSlots();

		if (k_actor == null) return;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		var spells = k_actor.KnownSpells
			.Select(id => k_gameDb.GetOrNull<SpellProto>(id))
			.Where(s => s != null)
			.Cast<SpellProto>()
			.OrderBy(s => s.Level)
			.ThenBy(s => s.School)
			.ToList();

		if (spells.Count == 0) {
			var emptyLabel = new GameLabel("No spells known")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.SetMargin(spacing.MD)
				.Build();
			k_spellsScroll.AddChild(emptyLabel);
			return;
		}

		var spellsBySchool = spells.GroupBy(s => s.School);

		foreach (var group in spellsBySchool) {
			// School header with color
			var schoolColor = group.Key.GetColor();
			var headerContainer = new GameContainer($"school-header-{group.Key}")
				.SetRow()
				.SetAlignItems(Align.Center)
				.SetMarginTop(spacing.SM)
				.SetMarginBottom(spacing.XS)
				.Build();

			var schoolIcon = new GameLabel(group.Key.GetIconName())
				.SetStyle(LabelStyle.TitleSmall)
				.SetMarginRight(spacing.XS)
				.Build();
			schoolIcon.style.color = schoolColor;

			var schoolName = new GameLabel(group.Key.GetDisplayName())
				.SetStyle(LabelStyle.LabelMedium)
				.Build();
			schoolName.style.color = schoolColor;

			var spellCount = new GameBadge()
				.SetText(group.Count().ToString())
				.SetVariant(BadgeVariant.Default)
				.SetSize(BadgeSize.Small)
				.Build();
			spellCount.style.marginLeft = spacing.XS;

			headerContainer.Add(schoolIcon);
			headerContainer.Add(schoolName);
			headerContainer.Add(spellCount);

			k_spellsScroll.AddChild(headerContainer);

			// Spell grid
			var grid = new GameContainer($"spell-grid-{group.Key}")
				.SetRow()
				.SetFlexWrap(Wrap.Wrap)
				.Build();

			foreach (var spell in group) {
				var row = CreateSpellRow(spell);
				grid.Add(row);
			}

			k_spellsScroll.AddChild(grid);
		}
	}

	private VisualElement CreateAbilityRow(CombatAbility ability) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		float currentMana = k_actor?.CurrentMana ?? 0;
		bool canUse = ability.CurrentCooldown == 0 && ability.ManaCost <= currentMana;

		// Container for slot + info
		var container = new GameContainer($"ability-{ability.Id}")
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetWidth(SLOT_SIZE_LARGE + spacing.SM * 2)
			.SetPadding(spacing.XS)
			.SetMargin(spacing.XXS)
			.SetBorderRadius(theme.Borders.RadiusSM)
			.Build();

		if (canUse) {
			container.RegisterCallback<MouseEnterEvent>(_ => {
				container.style.backgroundColor = colors.SurfaceHover;
			});
			container.RegisterCallback<MouseLeaveEvent>(_ => {
				container.style.backgroundColor = StyleKeyword.Null;
			});
		}

		// Create skill slot using SkillProto if available, or create a mock display
		var skillProto = TryGetSkillProtoForAbility(ability);

		var slot = new GameSkillSlotDisplay()
			.SetSkill(skillProto, 1)
			.SetSize(SLOT_SIZE_LARGE)
			.SetCooldownTurns(ability.CurrentCooldown)
			.SetInteractive(canUse)
			.SetEnabled(canUse)
			.OnClick(() => {
				if (canUse) SelectAbility(ability);
			})
			.OnRightClick(() => ShowAbilityDetails(ability))
			.Build();

		k_skillSlots.Add(slot);

		// Name label
		var nameLabel = new GameLabel(ability.Name.Truncate(10))
			.SetStyle(LabelStyle.Caption)
			.SetColor(canUse ? LabelColor.Primary : LabelColor.Disabled)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();
		nameLabel.style.maxWidth = SLOT_SIZE_LARGE + spacing.SM;

		// Cost/cooldown indicator
		VisualElement? indicator = null;
		if (ability.CurrentCooldown > 0) {
			indicator = new GameBadge()
				.SetText($"CD:{ability.CurrentCooldown}")
				.SetVariant(BadgeVariant.Warning)
				.SetSize(BadgeSize.Small)
				.Build();
		} else if (ability.ManaCost > 0) {
			indicator = new GameBadge()
				.SetText($"{ability.ManaCost}MP")
				.SetVariant(canUse ? BadgeVariant.Info : BadgeVariant.Error)
				.SetSize(BadgeSize.Small)
				.Build();
		} else {
			indicator = new GameLabel("Ready")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Success)
				.Build();
		}

		container.Add(slot);
		container.Add(nameLabel);
		if (indicator != null) container.Add(indicator);

		return container;
	}

	private VisualElement CreateSpellRow(SpellProto spell) {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;

		float currentMana = k_actor?.CurrentMana ?? 0;
		int manaCost = spell.GetEffectiveManaCost();
		bool canCast = manaCost <= currentMana;

		// Container for slot + info
		var container = new GameContainer($"spell-{spell.Id.Value}")
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetWidth(SLOT_SIZE_LARGE + spacing.SM * 2)
			.SetPadding(spacing.XS)
			.SetMargin(spacing.XXS)
			.SetBorderRadius(theme.Borders.RadiusSM)
			.Build();

		if (canCast) {
			container.RegisterCallback<MouseEnterEvent>(_ => {
				container.style.backgroundColor = colors.SurfaceHover;
			});
			container.RegisterCallback<MouseLeaveEvent>(_ => {
				container.style.backgroundColor = StyleKeyword.Null;
			});
		}

		// Create spell slot
		var slot = new GameSpellSlotDisplay()
			.SetSpell(spell)
			.SetSize(SLOT_SIZE_LARGE)
			.SetPrepared(canCast)
			.SetInteractive(canCast)
			.SetEnabled(canCast)
			.OnClick(() => {
				if (canCast) SelectSpell(spell);
			})
			.OnRightClick(() => ShowSpellDetails(spell))
			.Build();

		k_spellSlots.Add(slot);

		// Name label
		var nameLabel = new GameLabel(spell.DisplayText.Name.Truncate(10))
			.SetStyle(LabelStyle.Caption)
			.SetColor(canCast ? LabelColor.Primary : LabelColor.Disabled)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.Build();
		nameLabel.style.maxWidth = SLOT_SIZE_LARGE + spacing.SM;
		nameLabel.style.color = spell.School.GetColor();

		// Cost indicator
		VisualElement indicator;
		if (manaCost > 0) {
			indicator = new GameBadge()
				.SetText($"{manaCost}MP")
				.SetVariant(canCast ? BadgeVariant.Info : BadgeVariant.Error)
				.SetSize(BadgeSize.Small)
				.Build();
		} else {
			indicator = new GameLabel("Free")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Success)
				.Build();
		}

		container.Add(slot);
		container.Add(nameLabel);
		container.Add(indicator);

		return container;
	}

	#endregion

	#region Details Panel

	private void ShowAbilityDetails(CombatAbility ability) {
		k_selectedAbility = ability;
		k_detailsPanel.style.display = DisplayStyle.Flex;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;

		k_detailsName.SetText($"{GetAbilityTypeIcon(ability.Type)} {ability.Name}");
		k_detailsDescription.SetText(ability.Description ?? "No description available.");

		// Tags
		k_detailsTags.Clear();
		AddTagBadge(k_detailsTags, ability.Type.ToString(), ColorRPG.MissGray, spacing);

		if (ability.TargetType != TargetType.None) {
			AddTagBadge(k_detailsTags, $"Target: {ability.TargetType}", ColorRPG.ShieldBlue, spacing);
		}

		if (ability.DamageType != DamageType.Physical) {
			AddTagBadge(k_detailsTags, ability.DamageType.ToString(), GetDamageTypeColor(ability.DamageType), spacing);
		}

		// Stats
		k_detailsStats.Clear();

		if (ability.ManaCost > 0) {
			AddStatDisplay(k_detailsStats, "Mana Cost", ability.ManaCost.ToString(), ColorRPG.ManaBlue, spacing);
		}

		if (ability.Cooldown > 0) {
			AddStatDisplay(k_detailsStats, "Cooldown", $"{ability.Cooldown} turns", ColorRPG.StaminaYellow, spacing);
		}

		if (ability.DamageDice.IsValid) {
			AddStatDisplay(k_detailsStats, "Damage", ability.DamageDice.ToString(), ColorRPG.RageRed, spacing);
		}

		foreach (var effect in ability.Effects) {
			if (effect.HealAmount > 0) {
				AddStatDisplay(k_detailsStats, "Healing", effect.HealAmount.ToString(), ColorRPG.HealGreen, spacing);
			}
			if (effect.AppliesCondition.HasValue) {
				AddStatDisplay(k_detailsStats, "Applies", effect.AppliesCondition.Value.ToString(), ColorRPG.DebuffPurple, spacing);
			}
		}
	}

	private void ShowSpellDetails(SpellProto spell) {
		k_selectedAbility = spell;
		k_detailsPanel.style.display = DisplayStyle.Flex;

		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var themeColors = theme.Colors;
		var schoolColor = spell.School.GetColor();

		k_detailsName.SetText($"{spell.School.GetIconName()} {spell.DisplayText.Name}");
		k_detailsName.style.color = schoolColor;
		k_detailsDescription.SetText(spell.DisplayText.Description ?? "No description available.");

		// Tags
		k_detailsTags.Clear();

		// School tag
		AddTagBadge(k_detailsTags, spell.School.GetDisplayName(), schoolColor, spacing);

		// Level tag
		string levelText = spell.IsCantrip ? "Cantrip" : $"Level {spell.Level}";
		AddTagBadge(k_detailsTags, levelText, ColorRPG.ExperiencePurple, spacing);

		// Target type
		AddTagBadge(k_detailsTags, $"{GetTargetTypeIcon(spell.TargetType)} {spell.TargetType}", ColorRPG.ShieldBlue, spacing);

		// Concentration
		if (spell.RequiresConcentration) {
			AddTagBadge(k_detailsTags, "⚡ Concentration", themeColors.Warning, spacing);
		}

		// Actual proto tags from TagProto  // TODO: Re-enable after Spell Proto re-factor
		//foreach (var tagId in spell.Tags) {
		//	var tagProto = k_gameDb.GetOrNull<TagProto>(tagId);
		//	if (tagProto != null && tagProto.ShowInUI) {
		//		var tagColor = !string.IsNullOrEmpty(tagProto.ColorHint)
		//			? ColorRPG.FromHex(tagProto.ColorHint)
		//			: ColorRPG.MissGray;
		//		AddTagBadge(k_detailsTags, tagProto.DisplayText.Name, tagColor, spacing);
		//	}
		//}

		// Stats
		k_detailsStats.Clear();

		int manaCost = spell.GetEffectiveManaCost();
		if (manaCost > 0) {
			AddStatDisplay(k_detailsStats, "Mana Cost", manaCost.ToString(), ColorRPG.ManaBlue, spacing);
		}

		if (spell.HealthCost > 0) {
			AddStatDisplay(k_detailsStats, "Health Cost", spell.HealthCost.ToString(), ColorRPG.RageRed, spacing);
		}

		if (spell.Cooldown > 0) {
			AddStatDisplay(k_detailsStats, "Cooldown", $"{spell.Cooldown} turns", ColorRPG.StaminaYellow, spacing);
		}

		if (spell.Range > 0) {
			AddStatDisplay(k_detailsStats, "Range", $"{spell.Range} ft", ColorRPG.MissGray, spacing);
		}

		if (spell.AreaRadius > 0) {
			AddStatDisplay(k_detailsStats, "Area", $"{spell.AreaRadius} ft", ColorRPG.MissGray, spacing);
		}

		if (spell.DamageDice.IsValid) {
			AddStatDisplay(k_detailsStats, "Damage", $"{spell.DamageDice} {spell.DamageType}", GetDamageTypeColor(spell.DamageType), spacing);
		}

		if (spell.HealingDice.IsValid) {
			AddStatDisplay(k_detailsStats, "Healing", spell.HealingDice.ToString(), ColorRPG.HealGreen, spacing);
		}

		if (spell.AppliesCondition.HasValue) {
			AddStatDisplay(k_detailsStats, "Applies", spell.AppliesCondition.Value.ToString(), ColorRPG.DebuffPurple, spacing);
		}
	}

	private void HideDetails() {
		k_detailsPanel.style.display = DisplayStyle.None;
		k_selectedAbility = null;
	}

	private void AddTagBadge(GameContainer parent, string text, ColorRPG color, SpacingSettings spacing) {
		var badge = new GameBadge()
			.SetText(text)
			.SetVariant(BadgeVariant.Default)
			.SetSize(BadgeSize.Small)
			.Build();
		badge.style.backgroundColor = color.WithAlpha(0.2f);
		badge.style.color = color;
		badge.style.marginRight = spacing.XXS;
		badge.style.marginBottom = spacing.XXS;
		parent.Add(badge);
	}

	private void AddStatDisplay(GameContainer parent, string label, string value, ColorRPG color, SpacingSettings spacing) {
		var stat = new GameContainer()
			.SetColumn()
			.SetAlignItems(Align.Center)
			.SetPadding(spacing.XS)
			.SetMarginRight(spacing.SM)
			.Build();

		var labelEl = new GameLabel(label)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Tertiary)
			.Build();

		var valueEl = new GameLabel(value)
			.SetStyle(LabelStyle.BodyMedium)
			.Build();
		valueEl.style.color = color;

		stat.Add(labelEl);
		stat.Add(valueEl);
		parent.Add(stat);
	}

	#endregion

	#region Selection

	private void SelectAbility(CombatAbility ability) {
		if (k_actor == null) return;

		var target = GetTargetForAbility(ability.TargetType);
		var action = CombatAction.UseAbility(k_actor, ability, target);

		Hide();
		OnActionSelected?.Invoke(action);
	}

	private void SelectSpell(SpellProto spell) {
		if (k_actor == null) return;

		var combatSpell = ConvertSpellToAbility(spell);
		var target = GetTargetForAbility(spell.TargetType);
		var action = CombatAction.CastSpell(k_actor, combatSpell, target);

		Hide();
		OnActionSelected?.Invoke(action);
	}

	private LiveCharacter? GetTargetForAbility(TargetType targetType) {
		return targetType switch {
			TargetType.Self => k_actor,
			TargetType.SingleEnemy => k_selectedTarget ?? k_combatManager.LivingEnemies.FirstOrDefault(),
			TargetType.SingleAlly => k_actor,
			_ => null
		};
	}

	private CombatAbility ConvertSpellToAbility(SpellProto spell) {
		var ability = new CombatAbility {
			Id = spell.Id.Value,
			Name = spell.DisplayText.Name,
			Description = spell.DisplayText.Description,
			Type = CombatAbilityType.Spell,
			TargetType = spell.TargetType,
			ManaCost = spell.GetEffectiveManaCost(),
			Cooldown = spell.Cooldown,
			CurrentCooldown = 0,
			DamageDice = spell.DamageDice,
			DamageType = spell.DamageType
		};

		if (spell.HealingDice.IsValid) {
			ability.Effects.Add(new Core.Prototypes.Combat.AbilityEffect {
				HealAmount = DiceRoller.Roll(spell.HealingDice)
			});
		}

		if (spell.AppliesCondition.HasValue) {
			ability.Effects.Add(new Core.Prototypes.Combat.AbilityEffect {
				AppliesCondition = spell.AppliesCondition,
				ConditionDuration = spell.ConditionDuration
			});
		}

		return ability;
	}

	#endregion

	#region Helpers

	private SkillProto? TryGetSkillProtoForAbility(CombatAbility ability) {
		// Try to find a matching SkillProto for this ability
		var protoId = new SkillProto.ID($"Skill_{ability.Id}");
		return k_gameDb.GetOrNull<SkillProto>(protoId);
	}

	private static string GetAbilityTypeIcon(CombatAbilityType type) {
		return type switch {
			CombatAbilityType.Attack => "⚔️",
			CombatAbilityType.Spell => "✨",
			CombatAbilityType.Buff => "⬆️",
			CombatAbilityType.Debuff => "⬇️",
			CombatAbilityType.Heal => "❤️",
			CombatAbilityType.Utility => "🔧",
			_ => "●"
		};
	}

	private static string GetTargetTypeIcon(TargetType type) {
		return type switch {
			TargetType.Self => "👤",
			TargetType.SingleEnemy => "🎯",
			TargetType.SingleAlly => "🤝",
			TargetType.AllEnemies => "👾",
			TargetType.AllAllies => "👥",
			TargetType.All => "🌐",
			_ => "?"
		};
	}

	private static ColorRPG GetDamageTypeColor(DamageType type) {
		return type switch {
			DamageType.Fire => ColorRPG.Fire,
			DamageType.Cold => ColorRPG.Ice,
			DamageType.Lightning => ColorRPG.Lightning,
			DamageType.Poison => ColorRPG.Poison,
			DamageType.Acid => ColorRPG.Poison.RotateHue(-30),
			DamageType.Holy => ColorRPG.Holy,
			DamageType.Necrotic => ColorRPG.NecroticPurple,
			DamageType.Psychic => ColorRPG.Arcane,
			DamageType.Force => ColorRPG.Arcane.Lighten(0.2f),
			_ => ColorRPG.RageRed
		};
	}

	private void ClearSlots() {
		ClearSkillSlots();
		ClearSpellSlots();
	}

	private void ClearSkillSlots() {
		foreach (var slot in k_skillSlots) {
			slot.RemoveFromHierarchy();
		}
		k_skillSlots.Clear();
	}

	private void ClearSpellSlots() {
		foreach (var slot in k_spellSlots) {
			slot.RemoveFromHierarchy();
		}
		k_spellSlots.Clear();
	}

	#endregion

	#region Cleanup

	public new void RemoveFromHierarchy() {
		ClearSlots();
		base.RemoveFromHierarchy();
	}

	#endregion
}