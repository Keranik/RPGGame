using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;

#nullable disable

/// <summary>
/// A themed accordion/collapsible section component.
/// 
/// <para>Usage:</para>
/// <code>
/// var questLog = new GameAccordion()
///     .AddSection("Main Quests", mainQuestsList)
///     .AddSection("Side Quests", sideQuestsList)
///     .AddSection("Completed", completedList, collapsed: true)
///     .SetAllowMultiple(false)
///     .Build();
/// </code>
/// </summary>
public class GameAccordion : VisualElement {
	#region Private Fields

	private readonly VisualElement k_container;
	private readonly List<AccordionSection> k_sections = new();
	private bool k_allowMultiple = true;

	#endregion

	#region Constructors

	public GameAccordion() {
		k_container = new VisualElement {
			style = { flexDirection = FlexDirection.Column }
		};

		Add(k_container);

		GameTheme.OnThemeChanged += OnThemeChanged;
	}

	#endregion

	#region Fluent API

	public GameAccordion AddSection(string title, VisualElement content, bool collapsed = false) {
		var section = new AccordionSection(title, content, !collapsed);
		section.OnToggled += OnSectionToggled;
		k_sections.Add(section);
		k_container.Add(section);
		return this;
	}

	public GameAccordion SetAllowMultiple(bool allow) {
		k_allowMultiple = allow;
		return this;
	}

	public GameAccordion ExpandAll() {
		foreach (var section in k_sections) {
			section.Expand();
		}
		return this;
	}

	public GameAccordion CollapseAll() {
		foreach (var section in k_sections) {
			section.Collapse();
		}
		return this;
	}

	public GameAccordion ExpandSection(int index) {
		if (index >= 0 && index < k_sections.Count) {
			k_sections[index].Expand();
		}
		return this;
	}

	#endregion

	#region Build

	public GameAccordion Build() {
		ApplyTheme();
		return this;
	}

	#endregion

	#region Event Handlers

	private void OnSectionToggled(AccordionSection toggledSection, bool isExpanded) {
		if (!k_allowMultiple && isExpanded) {
			foreach (var section in k_sections) {
				if (section != toggledSection) {
					section.Collapse();
				}
			}
		}
	}

	#endregion

	#region Theme

	private void ApplyTheme() {
		foreach (var section in k_sections) {
			section.ApplyTheme();
		}
	}

	private void OnThemeChanged(GameTheme theme) => ApplyTheme();

	#endregion

	#region Cleanup

	new public void RemoveFromHierarchy() {
		GameTheme.OnThemeChanged -= OnThemeChanged;
		base.RemoveFromHierarchy();
	}

	#endregion

	#region Nested Class

private class AccordionSection : VisualElement {
	private readonly VisualElement k_header;
	private readonly Label k_titleLabel;
	private readonly Label k_chevron;
	private readonly VisualElement k_contentContainer;
	private bool k_isExpanded;

	public event Action<AccordionSection, bool> OnToggled;

	public AccordionSection(string title, VisualElement content, bool isExpanded) {
		k_isExpanded = isExpanded;

		k_header = new VisualElement {
			style = {
				flexDirection = FlexDirection.Row,
				alignItems = Align.Center,
				justifyContent = Justify.SpaceBetween
			}
		};

		k_titleLabel = new Label(title);
		k_chevron = new Label(isExpanded ? "▼" : "▶");

		k_header.Add(k_titleLabel);
		k_header.Add(k_chevron);

		k_contentContainer = new VisualElement {
			style = { display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None }
		};
		k_contentContainer.Add(content);

		Add(k_header);
		Add(k_contentContainer);

		k_header.RegisterCallback<ClickEvent>(OnHeaderClick);
	}

	private void OnHeaderClick(ClickEvent evt) {
		Toggle();
	}

	public void Toggle() {
		k_isExpanded = !k_isExpanded;
		k_contentContainer.style.display = k_isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
		k_chevron.text = k_isExpanded ? "▼" : "▶";
		OnToggled?.Invoke(this, k_isExpanded);
	}

	public void Expand() {
		if (!k_isExpanded) {
			Toggle();
		}
	}

	public void Collapse() {
		if (k_isExpanded) {
			Toggle();
		}
	}

	public void ApplyTheme() {
		var theme = GameTheme.Current;
		var colors = theme.Colors;
		var borders = theme.Borders;
		var spacing = theme.Spacing;
		var typography = theme.Typography;

		// Header
		k_header.style.backgroundColor = colors.Surface;
		k_header.style.paddingTop = spacing.SM;
		k_header.style.paddingBottom = spacing.SM;
		k_header.style.paddingLeft = spacing.MD;
		k_header.style.paddingRight = spacing.MD;
		k_header.style.borderBottomWidth = borders.WidthThin;
		k_header.style.borderBottomColor = colors.SurfaceBorder;

		// Title - apply typography
		typography.TitleSmall.ApplyTo(k_titleLabel.style);
		k_titleLabel.style.color = colors.TextPrimary;

		// Chevron - small icon text
		typography.Caption.ApplyTo(k_chevron.style);
		k_chevron.style.color = colors.TextSecondary;

		// Content
		k_contentContainer.style.paddingLeft = spacing.MD;
		k_contentContainer.style.paddingRight = spacing.MD;
		k_contentContainer.style.paddingTop = spacing.SM;
		k_contentContainer.style.paddingBottom = spacing.SM;
		k_contentContainer.style.backgroundColor = colors.BackgroundSecondary;
	}
}

#endregion
}