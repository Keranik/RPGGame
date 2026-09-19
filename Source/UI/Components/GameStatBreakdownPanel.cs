using RPGGame.Core;
using RPGGame.Core.Prototypes;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Simulation;
using RPGGame.Core.Stats;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

/// <summary>
/// A detailed stat breakdown panel showing all character stats with expandable tooltips.
/// Shows final values with color-coded modifiers and source breakdowns.
/// Uses StatProto and StatCategoryProto from the database for dynamic stat display.
/// </summary>
public class GameStatBreakdownPanel : VisualElement {
	#region Private Fields

	private readonly ScrollView k_scrollContainer;
	private readonly VisualElement k_content;
	private readonly GameDb? k_gameDb;

	private RunState? k_runState;
	private bool k_showCategories = true;
	private bool k_showSecondaryStats = true;
	private HashSet<StatCategoryProto.ID> k_expandedCategories = [];

	#endregion

	#region Constructor

	public GameStatBreakdownPanel(GameDb? gameDb = null) {
		k_gameDb = gameDb;

		k_scrollContainer = new ScrollView(ScrollViewMode.Vertical) {
			style = { flexGrow = 1 }
		};

		k_content = new VisualElement {
			name = "stat-breakdown-content",
			style = {
				flexDirection = FlexDirection.Column
			}
		};

		k_scrollContainer.Add(k_content);
		Add(k_scrollContainer);

		// Default expanded categories
		k_expandedCategories.Add(Ids.StatCategories.Attributes);
		k_expandedCategories.Add(Ids.StatCategories.Combat);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API

	/// <summary>
	/// Sets the run state to display stats from.
	/// </summary>
	public GameStatBreakdownPanel SetRunState(RunState? runState) {
		k_runState = runState;
		Refresh();
		return this;
	}

	/// <summary>
	/// Sets whether to show category headers.
	/// </summary>
	public GameStatBreakdownPanel SetShowCategories(bool show) {
		k_showCategories = show;
		Refresh();
		return this;
	}

	/// <summary>
	/// Sets whether to show secondary/derived stats.
	/// </summary>
	public GameStatBreakdownPanel SetShowSecondaryStats(bool show) {
		k_showSecondaryStats = show;
		Refresh();
		return this;
	}

	/// <summary>
	/// Sets the panel height.
	/// </summary>
	public GameStatBreakdownPanel SetHeight(float height) {
		style.height = height;
		return this;
	}

	/// <summary>
	/// Sets which categories are expanded by default.
	/// </summary>
	public GameStatBreakdownPanel SetExpandedCategories(params StatCategoryProto.ID[] categories) {
		k_expandedCategories = [.. categories];
		return this;
	}

	#endregion

	#region Build

	/// <summary>
	/// Builds the panel.
	/// </summary>
	public GameStatBreakdownPanel Build() {
		ApplyTheme();
		Refresh();
		return this;
	}

	#endregion

	#region Content Building

	/// <summary>
	/// Refreshes all stat displays.
	/// </summary>
	public void Refresh() {
		k_content.Clear();

		if (k_runState == null) {
			var noDataLabel = new Label("No character data");
			GameTheme.Current.Typography.Caption.ApplyTo(noDataLabel.style);
			noDataLabel.style.color = GameTheme.Current.Colors.TextTertiary;
			k_content.Add(noDataLabel);
			return;
		}

		if (k_showCategories && k_gameDb != null) {
			BuildCategorizedStats();
		} else {
			BuildFlatStats();
		}
	}

	private void BuildCategorizedStats() {
		if (k_gameDb == null) return;

		// Get all categories sorted by display order
		var categories = k_gameDb.GetAll<StatCategoryProto>()
			.Where(c => c.ShowInCharacterSheet)
			.OrderBy(c => c.DisplayOrder)
			.ToList();

		foreach (var category in categories) {
			// Skip secondary stats if not showing them
			if (!k_showSecondaryStats && IsSecondaryCategory(category.Id)) {
				continue;
			}

			// Get stats in this category
			var statsInCategory = k_gameDb.GetAll<StatProto>()
				.Where(s => s.Category == category.Id)
				.OrderBy(s => s.DisplayOrder)
				.ToList();

			if (statsInCategory.Count == 0) continue;

			BuildCategory(category, statsInCategory);
		}
	}

	/// <summary>
	/// Determines if a category is considered "secondary" (hidden by default).
	/// </summary>
	private bool IsSecondaryCategory(StatCategoryProto.ID categoryId) {
		return categoryId == Ids.StatCategories.Economy ||
		       categoryId == Ids.StatCategories.Expedition ||
		       categoryId == Ids.StatCategories.CostModifiers ||
		       categoryId == Ids.StatCategories.BuildingUpgrades;
	}

	private void BuildFlatStats() {
		if (k_runState == null) return;

		// Show all stats that have values
		foreach (var statId in k_runState.Stats.GetAllStats()) {
			AddStatRow(statId);
		}
	}

	private void BuildCategory(StatCategoryProto category, List<StatProto> stats) {
		var isExpanded = k_expandedCategories.Contains(category.Id);
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var typography = theme.Typography;

		// Category header (clickable)
		var header = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center,
				paddingTop = 8,
				paddingBottom = 4
			}
		};

		var expandIcon = new Label(isExpanded ? "▼" : "▶");
		typography.Caption.ApplyTo(expandIcon.style);
		expandIcon.style.marginRight = 4;
		expandIcon.style.color = colors.TextSecondary;

		// Use DisplayText.Name from Proto base class
		var categoryName = category.DisplayText.Name;
		var titleLabel = new Label(categoryName);
		typography.TitleSmall.ApplyTo(titleLabel.style);
		titleLabel.style.color = colors.TextPrimary;

		header.Add(expandIcon);
		header.Add(titleLabel);

		header.RegisterCallback<ClickEvent>(evt => {
			if (isExpanded) {
				k_expandedCategories.Remove(category.Id);
			} else {
				k_expandedCategories.Add(category.Id);
			}
			Refresh();
		});

		k_content.Add(header);

		// Divider
		var divider = new VisualElement {
			style = {
				height = 1,
				backgroundColor = colors.SurfaceBorder,
				marginBottom = 4
			}
		};
		k_content.Add(divider);

		// Stats (if expanded)
		if (isExpanded) {
			foreach (var stat in stats) {
				AddStatRow(stat.Id);
			}
		}
	}

	private void AddStatRow(StatProto.ID statId) {
	if (k_runState == null) return;

	var theme = GameTheme.Current;
	var colors = theme.Colors;
	var typography = theme.Typography;

	// Get stat proto for display info
	StatProto? statProto = null;
	k_gameDb?.TryGetProto(statId, out statProto);

	float finalValue = k_runState.Stats.Get(statId);

	var row = new VisualElement {
		style = {
			flexDirection = FlexDirection.Row,
			justifyContent = Justify.SpaceBetween,
			alignItems = Align.Center,
			paddingLeft = 12,
			paddingRight = 4,
			paddingTop = 2,
			paddingBottom = 2
		}
	};

	// Stat name - use DisplayText.Name from Proto
	string displayName = statProto?.DisplayText.Name ?? ExtractStatName(statId.Value);
	var nameLabel = new Label(displayName);
	typography.Caption.ApplyTo(nameLabel.style);
	nameLabel.style.color = colors.TextSecondary;
	nameLabel.style.flexGrow = 1;

	// Value display
	var valueContainer = new VisualElement {
		style = {
			flexDirection = FlexDirection.Row,
			alignItems = Align.Center
		}
	};

	// Format the value
	string formattedValue = FormatStatValue(statProto, finalValue);
	var valueLabel = new Label(formattedValue);
	typography.LabelSmall.ApplyTo(valueLabel.style);
	valueLabel.style.color = colors.TextPrimary;
	valueLabel.style.minWidth = 40;
	valueLabel.style.unityTextAlign = TextAnchor.MiddleRight;

	valueContainer.Add(valueLabel);

	// Show modifier breakdown if we have modifiers
	var modifiers = k_runState.Modifiers.Where(m => m.StatId == statId && m.IsActive).ToList();
	if (modifiers.Count > 0) {
		float modTotal = modifiers.Sum(m => m.GetEffectiveValue());
		if (Math.Abs(modTotal) > 0.01f) {
			string modText = modTotal > 0 ? $"+{modTotal:F0}" : $"{modTotal:F0}";
			Color modColor = modTotal > 0 ? colors.Success : colors.Error;

			var modLabel = new Label(modText);
			typography.Caption.ApplyTo(modLabel.style);
			modLabel.style.color = modColor;
			modLabel.style.marginLeft = 4;
			valueContainer.Add(modLabel);
		}
	}

	row.Add(nameLabel);
	row.Add(valueContainer);

	// Hover effect
	row.RegisterCallback<MouseEnterEvent>(evt => {
		row.style.backgroundColor = colors.SurfaceHover;
	});
	row.RegisterCallback<MouseLeaveEvent>(evt => {
		row.style.backgroundColor = colors.Transparent;
	});

	// Add tooltip with breakdown
	row.tooltip = GetStatBreakdown(statId, statProto);

	k_content.Add(row);
}

	/// <summary>
	/// Extracts a human-readable name from a stat ID.
	/// E.g., "Stat_Attr_Strength" → "Strength"
	/// </summary>
	private string ExtractStatName(string statIdValue) {
		// Remove common prefixes
		var name = statIdValue;
		if (name.StartsWith("Stat_")) {
			name = name[5..];
		}

		// Find the last underscore and take everything after it
		var lastUnderscore = name.LastIndexOf('_');
		if (lastUnderscore >= 0 && lastUnderscore < name.Length - 1) {
			name = name[(lastUnderscore + 1)..];
		}

		// Add spaces before capitals (e.g., "MaxHealth" → "Max Health")
		var result = new System.Text.StringBuilder();
		foreach (char c in name) {
			if (char.IsUpper(c) && result.Length > 0) {
				result.Append(' ');
			}
			result.Append(c);
		}

		return result.ToString();
	}

	private string FormatStatValue(StatProto? statProto, float value) {
		if (statProto == null) {
			return $"{value:F0}";
		}

		return statProto.FormatValue(value);
	}

	private string GetStatBreakdown(StatProto.ID statId, StatProto? statProto) {
		if (k_runState == null) return "";

		float finalValue = k_runState.Stats.Get(statId);
		string displayName = statProto?.DisplayText.Name ?? ExtractStatName(statId.Value);

		var sb = new System.Text.StringBuilder();
		sb.AppendLine(displayName);
		sb.AppendLine($"Value: {FormatStatValue(statProto, finalValue)}");

		// Show description if available
		if (statProto != null && !string.IsNullOrEmpty(statProto.DisplayText.Description)) {
			sb.AppendLine();
			sb.AppendLine(statProto.DisplayText.Description);
		}

		// List active modifiers
		var modifiers = k_runState.Modifiers.Where(m => m.StatId == statId && m.IsActive).ToList();
		if (modifiers.Count > 0) {
			sb.AppendLine();
			sb.AppendLine("Modifiers:");
			foreach (var mod in modifiers) {
				string prefix = mod.Value >= 0 ? "+" : "";
				string opStr = mod.Operation switch {
					ModifierOperation.FlatAdd => "",
					ModifierOperation.FlatSubtract => "",
					ModifierOperation.PercentIncrease => "%",
					ModifierOperation.PercentReduce => "%",
					ModifierOperation.PercentMore => "% more",
					ModifierOperation.PercentLess => "% less",
					_ => ""
				};
				sb.AppendLine($"  {mod.Source}: {prefix}{mod.Value:F0}{opStr}");
			}
		}

		return sb.ToString();
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;

		style.backgroundColor = colors.BackgroundSecondary;
		borders.ApplyRadius(style, borders.RadiusMD);
		theme.Spacing.PanelPadding.ApplyTo(style);
	}

	private void OnThemeChanged(GameTheme theme) {
		ApplyTheme();
		Refresh();
	}

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion
}