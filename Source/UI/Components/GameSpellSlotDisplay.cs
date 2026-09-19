using RPGGame.Core;
using RPGGame.Core.Items;
using RPGGame.Core.Prototypes.Spells;
using RPGGame.Core.Spells;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// Slot display for spells.
/// </summary>
public class GameSpellSlotDisplay : BaseGameSlotDisplay<GameSpellSlotDisplay> {
	#region Fields

	private SpellProto? k_spellProto;
	private bool k_isPrepared = true;
	private int k_currentCooldownTurns;

	private readonly GameLabel k_schoolBadge;
	private readonly GameLabel k_manaCostBadge;
	private readonly GameLabel k_levelBadge;

	#endregion

	#region Properties

	public SpellProto? SpellProto => k_spellProto;
	public override bool IsEmpty => k_spellProto == null;
	public bool IsPrepared => k_isPrepared;
	public bool IsOnCooldown => k_currentCooldownTurns > 0;

	#endregion

	#region Constructor

	public GameSpellSlotDisplay() : base() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spacing = theme.Spacing;

		k_schoolBadge = new GameLabel()
			.SetStyle(LabelStyle.Caption)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetAbsolute()
			.SetLeft(spacing.XXS)
			.SetTop(spacing.XXS)
			.SetPadding(0, spacing.XXS)
			.SetBorderRadius(borders.RadiusXS)
			.Hide()
			.Build();
		k_rootContainer.Add(k_schoolBadge);

		k_manaCostBadge = new GameLabel()
			.SetStyle(LabelStyle.Caption)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetAbsolute()
			.SetLeft(spacing.XXS)
			.SetBottom(spacing.XXS)
			.SetBackgroundColor(colors.ManaBackground)
			.SetColor(colors.Mana)
			.SetPadding(0, spacing.XXS)
			.SetBorderRadius(borders.RadiusXS)
			.Hide()
			.Build();
		k_rootContainer.Add(k_manaCostBadge);

		k_levelBadge = new GameLabel()
			.SetStyle(LabelStyle.Caption)
			.SetTextAlign(TextAnchor.MiddleCenter)
			.SetAbsolute()
			.SetRight(spacing.XXS)
			.SetBottom(spacing.XXS)
			.SetBackgroundColor(colors.BackgroundElevated)
			.SetPadding(0, spacing.XXS)
			.SetBorderRadius(borders.RadiusXS)
			.Hide()
			.Build();
		k_rootContainer.Add(k_levelBadge);
	}

	#endregion

	#region Fluent API - Spell Data

	public GameSpellSlotDisplay SetSpell(SpellProto? spell) {
		k_spellProto = spell;

		if (spell != null) {
			RarityType rarity = spell.Level switch {
				0 => RarityType.Common,
				1 or 2 => RarityType.Uncommon,
				3 or 4 => RarityType.Rare,
				5 or 6 => RarityType.Epic,
				_ => RarityType.Legendary
			};
			SetRarity(rarity);
		}

		RefreshDisplay();
		return this;
	}

	public GameSpellSlotDisplay SetPrepared(bool prepared) {
		k_isPrepared = prepared;
		SetEnabled(prepared);
		return this;
	}

	public GameSpellSlotDisplay SetHotkey(string? hotkey) {
		SetBadge(hotkey);
		return this;
	}

	public GameSpellSlotDisplay SetCooldownTurns(int remainingTurns) {
		k_currentCooldownTurns = Math.Max(0, remainingTurns);

		if (k_spellProto != null && k_spellProto.Cooldown > 0) {
			float progress = (float)k_currentCooldownTurns / k_spellProto.Cooldown;
			SetCooldown(progress, remainingTurns.Turns());
		} else {
			ClearCooldown();
		}

		return this;
	}

	public GameSpellSlotDisplay ClearSpell() {
		return SetSpell(null);
	}

	#endregion

	#region Abstract Implementation

	protected override Texture2D? GetIcon() => null;

	protected override string GetDisplayName() {
		return k_spellProto?.DisplayText.Name ?? "Empty";
	}

	protected override string GetPlaceholderInitials() {
		return GetInitials(k_spellProto?.DisplayText.Name);
	}

	protected override GameTooltip CreateBasicTooltip() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var colors = theme.Colors;
		var tooltipStyles = theme.Components.Tooltip;

		var content = new GameContainer("spell-summary-tooltip")
			.SetColumn()
			.SetPadding(spacing.SM, spacing.SM)
			.SetMinWidth(tooltipStyles.MaxWidth * 0.4f);

		if (k_spellProto == null) {
			content.Add(new GameLabel()
				.SetText("Empty Slot")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.Build());
			return new GameTooltip()
				.SetContent(content)
				.SetPosition(TooltipPosition.Right)
				.SetDelay(0);
		}

		// Header: Name
		content.Add(new GameLabel()
			.SetText(k_spellProto.DisplayText.Name)
			.SetStyle(LabelStyle.TitleSmall)
			.SetColor(LabelColor.Primary)
			.Build());

		// School + Level line
		string schoolText = k_spellProto.School.GetDisplayName();
		string levelText = k_spellProto.IsCantrip ? "Cantrip" : $"Level {k_spellProto.Level}";
		content.Add(new GameLabel()
			.SetText($"{schoolText} - {levelText}")
			.SetStyle(LabelStyle.Caption)
			.SetColor(k_spellProto.School.GetColor())
			.SetMarginBottom(spacing.XS)
			.Build());

		// Resource costs
		var costParts = new List<string>();
		int manaCost = k_spellProto.GetEffectiveManaCost();
		if (manaCost > 0) costParts.Add($"Mana: {manaCost}");
		if (k_spellProto.HealthCost > 0) costParts.Add($"HP: {k_spellProto.HealthCost}");
		if (k_spellProto.Cooldown > 0) costParts.Add($"CD: {k_spellProto.Cooldown}t");

		if (costParts.Count > 0) {
			content.Add(new GameLabel()
				.SetText(string.Join("  ", costParts))
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Tertiary)
				.SetMarginBottom(spacing.XS)
				.Build());
		}

		content.Add(new GameDivider()
			.SetMargin(spacing.XS, 0)
			.Build());

		// Damage output (primary decision factor for offensive spells)
		if (k_spellProto.DamageDice.NumberOfDice > 0) {
			var dmgRow = new GameContainer()
				.SetRow()
				.SetJustifyContent(Justify.SpaceBetween)
				.SetFullWidth();

			dmgRow.Add(new GameLabel()
				.SetText("Damage")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Secondary)
				.Build());

			dmgRow.Add(new GameLabel()
				.SetText($"{k_spellProto.DamageDice} {k_spellProto.DamageType}")
				.SetStyle(LabelStyle.Caption)
				.SetColor(k_spellProto.DamageType.GetColor())
				.Build());

			content.Add(dmgRow);
		}

		// Healing output
		if (k_spellProto.HealingDice.NumberOfDice > 0) {
			var healRow = new GameContainer()
				.SetRow()
				.SetJustifyContent(Justify.SpaceBetween)
				.SetFullWidth();

			healRow.Add(new GameLabel()
				.SetText("Healing")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Secondary)
				.Build());

			healRow.Add(new GameLabel()
				.SetText(k_spellProto.HealingDice.ToString())
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Success)
				.Build());

			content.Add(healRow);
		}

		// Target + Range
		string targetText = k_spellProto.TargetType.ToString().ToDisplayName();
		if (k_spellProto.Range > 0) {
			targetText += $" - {k_spellProto.Range}ft";
		}
		if (k_spellProto.AreaRadius > 0) {
			targetText += $" - {k_spellProto.AreaRadius}ft AoE";
		}

		var targetRow = new GameContainer()
			.SetRow()
			.SetJustifyContent(Justify.SpaceBetween)
			.SetFullWidth();

		targetRow.Add(new GameLabel()
			.SetText("Target")
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.Build());

		targetRow.Add(new GameLabel()
			.SetText(targetText)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Primary)
			.Build());

		content.Add(targetRow);

		// Status effect applied
		if (k_spellProto.AppliesCondition.HasValue) {
			string conditionText = k_spellProto.AppliesCondition.Value.ToString().ToDisplayName();
			if (k_spellProto.ConditionDuration.IsFinite) {
				conditionText += $" ({k_spellProto.ConditionDuration})";
			}

			var effectRow = new GameContainer()
				.SetRow()
				.SetJustifyContent(Justify.SpaceBetween)
				.SetFullWidth();

			effectRow.Add(new GameLabel()
				.SetText("Applies")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Secondary)
				.Build());

			effectRow.Add(new GameLabel()
				.SetText(conditionText)
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Warning)
				.Build());

			content.Add(effectRow);
		}

		// Flags row (Concentration, Out of Combat)
		var flags = new List<string>();
		if (k_spellProto.RequiresConcentration) flags.Add("Concentration");
		if (k_spellProto.UsableOutOfCombat) flags.Add("Out of Combat");

		if (flags.Count > 0) {
			content.Add(new GameLabel()
				.SetText(string.Join("  |  ", flags))
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Warning)
				.SetMarginTop(spacing.XS)
				.Build());
		}

		return new GameTooltip()
			.SetContent(content)
			.SetPosition(TooltipPosition.Right)
			.SetDelay(0);
	}

	protected override void OnRightClickAction(Vector2 mousePosition) {
		if (k_spellProto == null) return;

		var menu = GameContextMenu.Show(this, mousePosition)
			.AddHeader(k_spellProto.DisplayText.Name);

		// Inspect
		menu.AddItem("Inspect", () => {
			ShowDetailedInspector();
			k_onInspect?.Invoke();
		});

		menu.AddDivider();

		// Spell-specific options
		if (!k_isPrepared) {
			menu.AddItem("Prepare", () => {
				GameToast.Show($"Prepared {k_spellProto.DisplayText.Name}", ToastType.Success);
			});
		} else {
			menu.AddItem("Unprepare", () => {
				GameToast.Show($"Unprepared {k_spellProto.DisplayText.Name}", ToastType.Info);
			});
		}

		if (IsOnCooldown) {
			menu.AddItem($"Cooldown: {k_currentCooldownTurns}t", null, enabled: false);
		}

		menu.AddDivider();
		menu.AddItem("Cancel", null);
	}

	protected override GameTooltip CreateExpandedTooltip() {
		var theme = GameTheme.Current;
		var spacing = theme.Spacing;
		var tooltipStyles = theme.Components.Tooltip;

		var content = new GameContainer("spell-tooltip")
			.SetColumn()
			.SetPadding(spacing.SM, spacing.SM)
			.SetMinWidth(tooltipStyles.MaxWidth * 0.45f);

		if (k_spellProto == null) {
			content.Add(new GameLabel()
				.SetText("Empty Slot")
				.SetStyle(LabelStyle.BodyMedium)
				.SetColor(LabelColor.Tertiary)
				.Build());
		} else {
			// Header
			content.Add(new GameLabel()
				.SetText(k_spellProto.DisplayText.Name)
				.SetStyle(LabelStyle.TitleMedium)
				.SetColor(LabelColor.Primary)
				.Build());

			// School and level line
			string schoolText = k_spellProto.School.GetDisplayName();
			string levelText = k_spellProto.IsCantrip ? "Cantrip" : $"Level {k_spellProto.Level}";
			content.Add(new GameLabel()
				.SetText($"{schoolText} - {levelText}")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Secondary)
				.SetMarginBottom(spacing.SM)
				.Build());

			// Description
			if (!string.IsNullOrEmpty(k_spellProto.DisplayText.Description)) {
				content.Add(new GameLabel()
					.SetText(k_spellProto.DisplayText.Description)
					.SetStyle(LabelStyle.BodySmall)
					.SetColor(LabelColor.Secondary)
					.SetWhiteSpace(WhiteSpace.Normal)
					.SetMarginBottom(spacing.SM)
					.Build());
			}

			// Divider
			content.Add(new GameDivider().Build());

			// Stats
			int manaCost = k_spellProto.GetEffectiveManaCost();
			if (manaCost > 0) {
				AddStatRow(content, "Mana Cost", manaCost.ToString(), spacing);
			}

			if (k_spellProto.HealthCost > 0) {
				AddStatRow(content, "Health Cost", k_spellProto.HealthCost.ToString(), spacing);
			}

			if (k_spellProto.Range > 0) {
				AddStatRow(content, "Range", $"{k_spellProto.Range} ft", spacing);
			}

			if (k_spellProto.AreaRadius > 0) {
				AddStatRow(content, "Area", $"{k_spellProto.AreaRadius} ft radius", spacing);
			}

			if (k_spellProto.Cooldown > 0) {
				AddStatRow(content, "Cooldown", $"{k_spellProto.Cooldown} turns", spacing);
			}

			if (k_spellProto.DamageDice.NumberOfDice > 0) {
				string dmg = $"{k_spellProto.DamageDice} {k_spellProto.DamageType}";
				AddStatRow(content, "Damage", dmg, spacing);
			}

			if (k_spellProto.HealingDice.NumberOfDice > 0) {
				AddStatRow(content, "Healing", k_spellProto.HealingDice.ToString(), spacing);
			}

			// Concentration
			if (k_spellProto.RequiresConcentration) {
				content.Add(new GameLabel()
					.SetText("Requires Concentration")
					.SetStyle(LabelStyle.Caption)
					.SetColor(LabelColor.Warning)
					.SetMarginTop(spacing.XS)
					.Build());
			}
		}

		return new GameTooltip()
			.SetContent(content)
			.SetPosition(TooltipPosition.Right);
	}

	private void AddStatRow(GameContainer container, string label, string value, SpacingSettings spacingSettings) {
		var row = new GameContainer()
			.SetRow()
			.SetJustifyContent(Justify.SpaceBetween)
			.SetFullWidth();

		row.Add(new GameLabel()
			.SetText(label)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Secondary)
			.Build());

		row.Add(new GameLabel()
			.SetText(value)
			.SetStyle(LabelStyle.Caption)
			.SetColor(LabelColor.Primary)
			.Build());

		container.Add(row);
	}

	public override ItemInstance? GetDraggedItem() => null;

	protected override void OnRefreshComplete() {
		if (k_spellProto == null) {
			k_schoolBadge.Hide();
			k_manaCostBadge.Hide();
			k_levelBadge.Hide();
			return;
		}

		var colors = GameTheme.Current.Colors;

		// School badge with school-appropriate color
		string schoolAbbrev = k_spellProto.School.GetDisplayName().First2();
		k_schoolBadge
			.SetText(schoolAbbrev)
			.SetBackgroundColor(k_spellProto.School.GetColor())
			.SetColor(Color.white)
			.Show();

		// Mana cost
		int manaCost = k_spellProto.GetEffectiveManaCost();
		if (manaCost > 0) {
			k_manaCostBadge
				.SetText(manaCost.ToString())
				.Show();
		} else {
			k_manaCostBadge.Hide();
		}

		// Level badge (hide for cantrips)
		if (!k_spellProto.IsCantrip) {
			k_levelBadge
				.SetText($"L{k_spellProto.Level}")
				.Show();
		} else {
			k_levelBadge.Hide();
		}
	}

	#endregion
}