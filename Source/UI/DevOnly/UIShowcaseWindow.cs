using RPGGame.Core.Items;
using RPGGame.UI.Components;
using RPGGame.UI.Styles;
using RPGGame.UI.Templates;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.DevOnly;

#nullable disable

/// <summary>
/// A development showcase window that displays all UI components for testing and demonstration.
/// Organized into categories with interactive examples.
/// </summary>
public class UIShowcaseWindow : BaseWindow<UIShowcaseWindow> {
	#region Private Fields

	private GameTabView k_mainTabs;

	// Demo state
	private int k_counterValue = 1000;
	private float k_healthValue = 75;
	private float k_manaValue = 50;
	private bool k_isSpinning = false;

	#endregion

	#region Constructor

	public UIShowcaseWindow() {
		// Set window size and position
		SetPosition(5, 5, 90, 90);
		
		// Apply theme styling
		var theme = GameTheme.Current;
		style.backgroundColor = theme.Colors.Background;
		theme.Borders.ApplyRadius(style, theme.Borders.DialogRadius);
		theme.Borders.ApplyColor(style, theme.Colors.SurfaceBorder);
		theme.Borders.ApplyWidth(style, theme.Borders.WidthThin);
	}

	#endregion

	#region Window Content Overrides

	protected override void AddHeaderContent() {
		var header = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.AddChild(new GameLabel("🎨 UI Component Showcase")
				.SetStyle(LabelStyle.HeadlineMedium)
				.SetColor(LabelColor.Primary)
				.Build())
			.AddChild(new GameButton("✕")
				.SetVariant(ButtonVariant.Ghost)
				.SetSize(ButtonSize.Small)
				.OnClick(() => Close())
				.Build())
			.Build();

		HeaderContainer.Add(header);
		HeaderContainer.Add(new GameDivider().Build());
	}

	protected override void AddCustomContent() {
		// Initialize toast system
		GameToast.Initialize(this);

		k_mainTabs = new GameTabView()
			.AddTab("Buttons", CreateButtonsSection())
			.AddTab("Inputs", CreateInputsSection())
			.AddTab("Data Display", CreateDataDisplaySection())
			.AddTab("Layout", CreateLayoutSection())
			.AddTab("Feedback", CreateFeedbackSection())
			.AddTab("Game Specific", CreateGameSpecificSection())
			.AddTab("Themes", CreateThemesSection())
			.SetVariant(TabVariant.Default)
			.Build();

		ContentContainer.Add(k_mainTabs);
		ContentContainer.style.flexGrow = 1;
	}

	protected override void AddFooterContent() {
		var footer = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetJustify(Justify.SpaceBetween)
			.SetAlignment(Align.Center)
			.AddChild(new GameLabel("Development Tool - Not for production")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Tertiary)
				.Build())
			.AddChild(new GameLabel($"Theme: {GameTheme.Current.DisplayName}")
				.SetStyle(LabelStyle.Caption)
				.SetColor(LabelColor.Secondary)
				.Build())
			.Build();

		FooterContainer.Add(new GameDivider().Build());
		FooterContainer.Add(footer);
	}

	#endregion

	#region Buttons Section

	private VisualElement CreateButtonsSection() {
		var container = new ScrollView(ScrollViewMode.Vertical);

		// Button Variants
		container.Add(CreateSectionHeader("Button Variants"));
		var variantsRow = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetWrap(true)
			.AddChild(new GameButton("Primary").SetVariant(ButtonVariant.Primary).Build())
			.AddChild(new GameButton("Secondary").SetVariant(ButtonVariant.Secondary).Build())
			.AddChild(new GameButton("Success").SetVariant(ButtonVariant.Success).Build())
			.AddChild(new GameButton("Warning").SetVariant(ButtonVariant.Warning).Build())
			.AddChild(new GameButton("Danger").SetVariant(ButtonVariant.Danger).Build())
			.AddChild(new GameButton("Outline").SetVariant(ButtonVariant.Outline).Build())
			.AddChild(new GameButton("Ghost").SetVariant(ButtonVariant.Ghost).Build())
			.AddChild(new GameButton("Link").SetVariant(ButtonVariant.Link).Build())
			.Build();
		container.Add(variantsRow);

		// Button Sizes
		container.Add(CreateSectionHeader("Button Sizes"));
		var sizesRow = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.AddChild(new GameButton("Small").SetSize(ButtonSize.Small).Build())
			.AddChild(new GameButton("Medium").SetSize(ButtonSize.Medium).Build())
			.AddChild(new GameButton("Large").SetSize(ButtonSize.Large).Build())
			.Build();
		container.Add(sizesRow);

		// Button States
		container.Add(CreateSectionHeader("Button States"));
		var statesRow = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.AddChild(new GameButton("Normal").Build())
			.AddChild(new GameButton("Disabled").SetEnabled(false).Build())
			.AddChild(new GameButton("Loading").SetLoading(true).Build())
			.Build();
		container.Add(statesRow);

		// Full Width Button
		container.Add(CreateSectionHeader("Full Width"));
		container.Add(new GameButton("Full Width Button").SetFullWidth().Build());

		// Interactive Demo
		container.Add(CreateSectionHeader("Interactive Demo"));
		var clickCount = 0;
		var clickLabel = new GameLabel("Click count: 0").SetStyle(LabelStyle.BodyMedium).Build();
		var interactiveRow = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.AddChild(clickLabel)
			.AddChild(new GameButton("Click Me!")
				.SetVariant(ButtonVariant.Primary)
				.OnClick(() => {
					clickCount++;
					clickLabel.SetText($"Click count: {clickCount}");
					GameToast.Show($"Clicked {clickCount} times!", ToastType.Info, 1500);
				})
				.Build())
			.Build();
		container.Add(interactiveRow);

		return container;
	}

	#endregion

	#region Inputs Section

	private VisualElement CreateInputsSection() {
		var container = new ScrollView(ScrollViewMode.Vertical);

		// Text Fields
		container.Add(CreateSectionHeader("Text Fields"));
		var textFieldsPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.AddChild(new GameTextField()
				.SetLabel("Username")
				.SetPlaceholder("Enter username...")
				.SetHelperText("3-20 characters")
				.Build())
			.AddChild(new GameTextField()
				.SetLabel("Password")
				.SetPlaceholder("Enter password...")
				.SetPassword()
				.Build())
			.AddChild(new GameTextField()
				.SetLabel("Email")
				.SetPlaceholder("your@email.com")
				.SetValidator(v => !string.IsNullOrEmpty(v) && v.Contains("@") ? null : "Invalid email format")
				.Build())
			.AddChild(new GameTextField()
				.SetLabel("Bio")
				.SetPlaceholder("Tell us about yourself...")
				.SetMultiline()
				.SetMaxLength(200)
				.Build())
			.Build();
		container.Add(textFieldsPanel);

		// Dropdowns
		container.Add(CreateSectionHeader("Dropdowns"));
		var dropdownPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Horizontal)
			.AddChild(new GameDropdown()
				.SetLabel("Character Class")
				.SetOptions("Warrior", "Mage", "Rogue", "Cleric", "Ranger")
				.SetSelectedIndex(0)
				.Build())
			.AddChild(new GameDropdown()
				.SetLabel("Difficulty")
				.SetOptions("Easy", "Normal", "Hard", "Nightmare")
				.SetVariant(DropdownVariant.Filled)
				.SetSelectedIndex(1)
				.Build())
			.Build();
		container.Add(dropdownPanel);

		// Sliders
		container.Add(CreateSectionHeader("Sliders"));
		var sliderPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.AddChild(new GameSlider()
				.SetLabel("Master Volume")
				.SetRange(0, 100)
				.SetValue(75)
				.SetShowValue()
				.SetValueFormat("{0}%")
				.Build())
			.AddChild(new GameSlider()
				.SetLabel("Music Volume")
				.SetRange(0, 100)
				.SetValue(60)
				.SetShowValue()
				.SetValueFormat("{0}%")
				.SetVariant(SliderVariant.Secondary)
				.Build())
			.AddChild(new GameSlider()
				.SetLabel("Brightness")
				.SetRange(0, 100)
				.SetValue(50)
				.SetShowValue()
				.SetVariant(SliderVariant.Success)
				.Build())
			.Build();
		container.Add(sliderPanel);

		// Checkboxes & Toggle Groups
		container.Add(CreateSectionHeader("Checkboxes & Toggle Groups"));
		var checkboxPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Horizontal)
			.AddChild(new GamePanel()
				.SetVariant(PanelVariant.Ghost)
				.AddChild(new GameLabel("Checkboxes").SetStyle(LabelStyle.LabelMedium).Build())
				.AddChild(new GameCheckbox().SetLabel("Enable Music").SetChecked(true).Build())
				.AddChild(new GameCheckbox().SetLabel("Enable SFX").SetChecked(true).Build())
				.AddChild(new GameCheckbox().SetLabel("Show Tutorials").Build())
				.AddChild(new GameCheckbox().SetLabel("Indeterminate").SetIndeterminate().Build())
				.Build())
			.AddChild(new GameDivider().SetOrientation(DividerOrientation.Vertical).Build())
			.AddChild(new GamePanel()
				.SetVariant(PanelVariant.Ghost)
				.AddChild(new GameLabel("Toggle Group").SetStyle(LabelStyle.LabelMedium).Build())
				.AddChild(new GameToggleGroup()
					.AddOption("Easy", "easy")
					.AddOption("Normal", "normal")
					.AddOption("Hard", "hard")
					.SetSelected("normal")
					.Build())
				.Build())
			.AddChild(new GameDivider().SetOrientation(DividerOrientation.Vertical).Build())
			.AddChild(new GamePanel()
				.SetVariant(PanelVariant.Ghost)
				.AddChild(new GameLabel("Button Toggle").SetStyle(LabelStyle.LabelMedium).Build())
				.AddChild(new GameToggleGroup()
					.AddOption("Day", "day")
					.AddOption("Night", "night")
					.SetVariant(ToggleGroupVariant.Buttons)
					.SetLayout(ToggleGroupLayout.Horizontal)
					.SetSelected("day")
					.Build())
				.Build())
			.Build();
		container.Add(checkboxPanel);

		return container;
	}

	#endregion

	#region Data Display Section

	private VisualElement CreateDataDisplaySection() {
		var container = new ScrollView(ScrollViewMode.Vertical);

		// Labels
		container.Add(CreateSectionHeader("Typography & Labels"));
		var labelsPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.AddChild(new GameLabel("Display Large").SetStyle(LabelStyle.DisplayLarge).Build())
			.AddChild(new GameLabel("Headline Medium").SetStyle(LabelStyle.HeadlineMedium).Build())
			.AddChild(new GameLabel("Title Large").SetStyle(LabelStyle.TitleLarge).Build())
			.AddChild(new GameLabel("Body Medium - Regular text content").SetStyle(LabelStyle.BodyMedium).Build())
			.AddChild(new GameLabel("Caption - Small helper text").SetStyle(LabelStyle.Caption).Build())
			.AddChild(new GamePanel()
				.SetLayout(LayoutDirection.Horizontal)
				.SetVariant(PanelVariant.Ghost)
				.AddChild(new GameLabel("Primary").SetColor(LabelColor.Primary).Build())
				.AddChild(new GameLabel("Success").SetColor(LabelColor.Success).Build())
				.AddChild(new GameLabel("Warning").SetColor(LabelColor.Warning).Build())
				.AddChild(new GameLabel("Error").SetColor(LabelColor.Error).Build())
				.AddChild(new GameLabel("Info").SetColor(LabelColor.Info).Build())
				.Build())
			.Build();
		container.Add(labelsPanel);

		// Badges
container.Add(CreateSectionHeader("Badges"));

// Badge Variants
var badgeVariantsPanel = new GamePanel()
    .SetVariant(PanelVariant.Card)
    .SetLayout(LayoutDirection.Horizontal)
    .SetWrap(true)
    .AddChild(new GameBadge("Default").Build())
    .AddChild(new GameBadge("Primary").SetVariant(BadgeVariant.Primary).Build())
    .AddChild(new GameBadge("Success").SetVariant(BadgeVariant.Success).Build())
    .AddChild(new GameBadge("Warning").SetVariant(BadgeVariant.Warning).Build())
    .AddChild(new GameBadge("Error").SetVariant(BadgeVariant.Error).Build())
    .AddChild(new GameBadge("Dark").SetVariant(BadgeVariant.Dark).Build())
    .AddChild(new GameBadge("Legendary").SetVariant(BadgeVariant.Legendary).Build())
    .AddChild(new GameBadge("Epic").SetVariant(BadgeVariant.Epic).Build())
    .Build();
container.Add(badgeVariantsPanel);

// Badge Shapes
container.Add(CreateSectionHeader("Badge Shapes"));
var badgeShapesPanel = new GamePanel()
    .SetVariant(PanelVariant.Card)
    .SetLayout(LayoutDirection.Horizontal)
    .SetWrap(true)
    .AddChild(new GameBadge("Standard").SetVariant(BadgeVariant.Primary).SetShape(BadgeShape.Standard).Build())
    .AddChild(new GameBadge("99+").SetVariant(BadgeVariant.Error).SetShape(BadgeShape.Compact).Build())
    .AddChild(new GameBadge("LVL 50").SetVariant(BadgeVariant.Primary).SetShape(BadgeShape.Beveled).Build())
    .AddChild(new GameBadge("5").SetVariant(BadgeVariant.Error).SetShape(BadgeShape.Diamond).Build())
    .AddChild(new GameBadge("NEW").SetVariant(BadgeVariant.Success).SetShape(BadgeShape.Hexagon).Build())
    .AddChild(new GameBadge("RARE").SetVariant(BadgeVariant.Info).SetShape(BadgeShape.Shield).Build())
    .AddChild(new GameBadge("$9.99").SetVariant(BadgeVariant.Warning).SetShape(BadgeShape.Tag).Build())
    .AddChild(new GameBadge("BETA").SetVariant(BadgeVariant.Dark).SetShape(BadgeShape.Bracket).Build())
    .Build();
container.Add(badgeShapesPanel);

// Status badges with indicators
container.Add(CreateSectionHeader("Status Badges"));
var statusPanel = new GamePanel()
    .SetVariant(PanelVariant.Card)
    .SetLayout(LayoutDirection.Horizontal)
    .AddChild(new GameBadge("Online").SetVariant(BadgeVariant.Success).SetShowIndicator().Build())
    .AddChild(new GameBadge("Away").SetVariant(BadgeVariant.Warning).SetShowIndicator().Build())
    .AddChild(new GameBadge("Offline").SetVariant(BadgeVariant.Dark).SetShowIndicator().Build())
    .AddChild(new GameBadge("LIVE").SetVariant(BadgeVariant.Error).SetPulse().Build())
    .Build();
container.Add(statusPanel);

		// Counters
		container.Add(CreateSectionHeader("Counter Displays"));
		var counterPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Horizontal)
			.Build();

		var goldCounter = new GameCounterDisplay()
			.SetValue(k_counterValue)
			.SetLabel("Gold")
			.SetSize(CounterSize.Large)
			.SetAnimated(true)
			.Build();

		var addGoldBtn = new GameButton("+100")
			.SetVariant(ButtonVariant.Success)
			.SetSize(ButtonSize.Small)
			.OnClick(() => {
				k_counterValue += 100;
				goldCounter.SetValue(k_counterValue);
			})
			.Build();

		counterPanel.Content.Add(goldCounter);
		counterPanel.Content.Add(addGoldBtn);
		container.Add(counterPanel);

		// Timers
		container.Add(CreateSectionHeader("Timers"));
		var timerPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Horizontal)
			.Build();

		var countdownTimer = new GameTimer()
			.SetDurationSeconds(30)
			.SetFormat(TimerFormat.MinutesSeconds)
			.SetShowProgress()
			.OnComplete(() => GameToast.Show("Timer complete!", ToastType.Success))
			.Build();

		var countUpTimer = new GameTimer()
			.SetCountUp()
			.SetFormat(TimerFormat.HoursMinutesSeconds)
			.Build();

		var startBtn = new GameButton("Start")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Small)
			.OnClick(() => {
				countdownTimer.Start();
				countUpTimer.Start();
			})
			.Build();

		var resetBtn = new GameButton("Reset")
			.SetVariant(ButtonVariant.Outline)
			.SetSize(ButtonSize.Small)
			.OnClick(() => {
				countdownTimer.Reset();
				countUpTimer.Reset();
			})
			.Build();

		timerPanel.Content.Add(new GameLabel("Countdown:").Build());
		timerPanel.Content.Add(countdownTimer);
		timerPanel.Content.Add(new GameLabel("Elapsed:").Build());
		timerPanel.Content.Add(countUpTimer);
		timerPanel.Content.Add(startBtn);
		timerPanel.Content.Add(resetBtn);
		container.Add(timerPanel);

		// Stat Bars
		container.Add(CreateSectionHeader("Stat Bars"));
		var statPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.Build();

		var healthBar = new GameStatBar()
			.SetLabel("Health")
			.SetRange(0, 100)
			.SetValue(k_healthValue)
			.SetVariant(StatBarVariant.Health)
			.SetShowValue(true)
			.SetWidth(300)
			.Build();

		var manaBar = new GameStatBar()
			.SetLabel("Mana")
			.SetRange(0, 100)
			.SetValue(k_manaValue)
			.SetVariant(StatBarVariant.Mana)
			.SetShowValue(true)
			.SetWidth(300)
			.Build();

		var expBar = new GameStatBar()
			.SetLabel("Experience")
			.SetRange(0, 1000)
			.SetValue(350)
			.SetVariant(StatBarVariant.Experience)
			.SetShowValue(true)
			.SetValueFormat("{0}/{1} XP")
			.SetWidth(300)
			.Build();

		var damageBtn = new GameButton("Take Damage")
			.SetVariant(ButtonVariant.Danger)
			.SetSize(ButtonSize.Small)
			.OnClick(() => {
				k_healthValue = Mathf.Max(0, k_healthValue - 15);
				healthBar.SetValue(k_healthValue);
			})
			.Build();

		var healBtn = new GameButton("Heal")
			.SetVariant(ButtonVariant.Success)
			.SetSize(ButtonSize.Small)
			.OnClick(() => {
				k_healthValue = Mathf.Min(100, k_healthValue + 20);
				healthBar.SetValue(k_healthValue);
			})
			.Build();

		statPanel.Content.Add(healthBar);
		statPanel.Content.Add(manaBar);
		statPanel.Content.Add(expBar);
		statPanel.Content.Add(new GamePanel()
			.SetLayout(LayoutDirection.Horizontal)
			.SetVariant(PanelVariant.Ghost)
			.AddChild(damageBtn)
			.AddChild(healBtn)
			.Build());
		container.Add(statPanel);

		return container;
	}

	#endregion

	#region Layout Section

	private VisualElement CreateLayoutSection() {
		var container = new ScrollView(ScrollViewMode.Vertical);

		// Panels
		container.Add(CreateSectionHeader("Panel Variants"));
		var panelsRow = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.SetWrap(true)
			.AddChild(new GamePanel()
				.SetVariant(PanelVariant.Default)
				.SetSize(150, 80)
				.AddChild(new GameLabel("Default").Build())
				.Build())
			.AddChild(new GamePanel()
				.SetVariant(PanelVariant.Elevated)
				.SetSize(150, 80)
				.AddChild(new GameLabel("Elevated").Build())
				.Build())
			.AddChild(new GamePanel()
				.SetVariant(PanelVariant.Outlined)
				.SetSize(150, 80)
				.AddChild(new GameLabel("Outlined").Build())
				.Build())
			.AddChild(new GamePanel()
				.SetVariant(PanelVariant.Filled)
				.SetSize(150, 80)
				.AddChild(new GameLabel("Filled").Build())
				.Build())
			.AddChild(new GamePanel()
				.SetVariant(PanelVariant.Card)
				.SetSize(150, 80)
				.AddChild(new GameLabel("Card").Build())
				.Build())
			.Build();
		container.Add(panelsRow);

		// Cards
		container.Add(CreateSectionHeader("Cards"));
		var cardsRow = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetLayout(LayoutDirection.Horizontal)
			.AddChild(new GameCard()
				.SetHeader("Basic Card")
				.SetSubheader("With subheader")
				.SetContent(new GameLabel("This is card content.").Build())
				.SetWidth(200)
				.Build())
			.AddChild(new GameCard()
				.SetVariant(CardVariant.Outlined)
				.SetHeader("Outlined Card")
				.SetContent(new GameLabel("Card with outline.").Build())
				.AddFooterAction(new GameButton("Action").SetSize(ButtonSize.Small).Build())
				.SetWidth(200)
				.Build())
			.AddChild(new GameCard()
				.SetHeader("Clickable Card")
				.SetContent(new GameLabel("Click me!").Build())
				.SetClickable(() => GameToast.Show("Card clicked!", ToastType.Info))
				.SetWidth(200)
				.Build())
			.Build();
		container.Add(cardsRow);

		// Dividers
		container.Add(CreateSectionHeader("Dividers"));
		var dividerPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.AddChild(new GameLabel("Content above").Build())
			.AddChild(new GameDivider().Build())
			.AddChild(new GameLabel("Content below").Build())
			.AddChild(new GameDivider().SetLabel("OR").Build())
			.AddChild(new GameLabel("More content").Build())
			.AddChild(new GameDivider().SetVariant(DividerVariant.Strong).Build())
			.AddChild(new GameLabel("Strong divider above").Build())
			.Build();
		container.Add(dividerPanel);

		// Accordion
		container.Add(CreateSectionHeader("Accordion"));
		var accordion = new GameAccordion()
			.AddSection("Section 1 - Expanded", new GameLabel("This section is expanded by default.").Build())
			.AddSection("Section 2 - Collapsed", new GameLabel("This section was collapsed by default.").Build(), collapsed: true)
			.AddSection("Section 3 - Also Collapsed", new GamePanel()
				.SetVariant(PanelVariant.Ghost)
				.AddChild(new GameLabel("This section contains multiple elements.").Build())
				.AddChild(new GameButton("A Button").SetSize(ButtonSize.Small).Build())
				.Build(), collapsed: true)
			.SetAllowMultiple(false)
			.Build();
		container.Add(accordion);

		// Grid
		container.Add(CreateSectionHeader("Grid Layout"));
		var gridItems = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
		var grid = new GameGrid<string>()
			.SetColumns(3)
			.SetCellSize(60)
			.SetGap(8)
			.SetItems(gridItems)
			.SetItemTemplate(item => new GameLabel(item).SetStyle(LabelStyle.TitleMedium).SetTextAlign(TextAnchor.MiddleCenter).Build())
			.SetVariant(GridVariant.Slots)
			.SetSelectionType(GridSelectionType.Single)
			.OnCellClicked((item, x, y) => GameToast.Show($"Clicked: {item} at ({x}, {y})", ToastType.Info))
			.Build();
		container.Add(grid);

		// List View
		container.Add(CreateSectionHeader("List View"));
		var listItems = new List<string> { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" };
		var listView = new GameListView<string>()
			.SetItems(listItems)
			.SetItemTemplate(item => new GameLabel(item).SetStyle(LabelStyle.BodyMedium).Build())
			.SetSelectionType(ListSelectionType.Single)
			.SetHeight(150)
			.OnSelectionChanged(items => {
				var selected = items.FirstOrDefault();
				if (selected != null) {
					GameToast.Show($"Selected: {selected}", ToastType.Info);
				}
			})
			.Build();
		container.Add(listView);

		return container;
	}

	#endregion

	#region Feedback Section

	private VisualElement CreateFeedbackSection() {
		var container = new ScrollView(ScrollViewMode.Vertical);

		// Toasts
		container.Add(CreateSectionHeader("Toast Notifications"));
		var toastPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Horizontal)
			.AddChild(new GameButton("Info Toast")
				.SetVariant(ButtonVariant.Primary)
				.OnClick(() => GameToast.Show("This is an info message", ToastType.Info))
				.Build())
			.AddChild(new GameButton("Success Toast")
				.SetVariant(ButtonVariant.Success)
				.OnClick(() => GameToast.Show("Operation successful!", ToastType.Success))
				.Build())
			.AddChild(new GameButton("Warning Toast")
				.SetVariant(ButtonVariant.Warning)
				.OnClick(() => GameToast.Show("Warning: Check your input", ToastType.Warning))
				.Build())
			.AddChild(new GameButton("Error Toast")
				.SetVariant(ButtonVariant.Danger)
				.OnClick(() => GameToast.Show("An error occurred!", ToastType.Error))
				.Build())
			.Build();
		container.Add(toastPanel);

		// Dialogs
		container.Add(CreateSectionHeader("Dialogs"));
		var dialogPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Horizontal)
			.AddChild(new GameButton("Alert Dialog")
				.SetVariant(ButtonVariant.Primary)
				.OnClick(() => GameDialog.Alert("Alert", "This is an alert message!").Show(this))
				.Build())
			.AddChild(new GameButton("Confirm Dialog")
				.SetVariant(ButtonVariant.Secondary)
				.OnClick(() => GameDialog.Confirm(
					"Confirm Action",
					"Are you sure you want to proceed?",
					() => GameToast.Show("Confirmed!", ToastType.Success),
					() => GameToast.Show("Cancelled", ToastType.Info)
				).Show(this))
				.Build())
			.AddChild(new GameButton("Danger Dialog")
				.SetVariant(ButtonVariant.Danger)
				.OnClick(() => GameDialog.ConfirmDanger(
					"Delete Item",
					"This action cannot be undone. Are you sure?",
					() => GameToast.Show("Deleted!", ToastType.Error),
					() => GameToast.Show("Cancelled", ToastType.Info)
				).Show(this))
				.Build())
			.AddChild(new GameButton("Custom Dialog")
				.SetVariant(ButtonVariant.Outline)
				.OnClick(() => {
					var customContent = new GamePanel()
						.SetVariant(PanelVariant.Ghost)
						.AddChild(new GameTextField().SetLabel("Name").SetPlaceholder("Enter name...").Build())
						.AddChild(new GameDropdown().SetLabel("Type").SetOptions("Option A", "Option B", "Option C").Build())
						.Build();

					new GameDialog()
						.SetTitle("Custom Form")
						.SetContent(customContent)
						.AddAction("Cancel", DialogActionType.Secondary, null)
						.AddAction("Submit", DialogActionType.Primary, () => GameToast.Show("Submitted!", ToastType.Success))
						.Build()
						.Show(this);
				})
				.Build())
			.Build();
		container.Add(dialogPanel);

		// Tooltips
		container.Add(CreateSectionHeader("Tooltips"));
		var tooltipPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Horizontal)
			.Build();

		var topBtn = new GameButton("Top Tooltip").Build();
		topBtn.AddTooltip("Tooltip on top", TooltipPosition.Top);

		var bottomBtn = new GameButton("Bottom Tooltip").Build();
		bottomBtn.AddTooltip("Tooltip on bottom", TooltipPosition.Bottom);

		var leftBtn = new GameButton("Left Tooltip").Build();
		leftBtn.AddTooltip("Tooltip on left", TooltipPosition.Left);

		var rightBtn = new GameButton("Right Tooltip").Build();
		rightBtn.AddTooltip("Tooltip on right", TooltipPosition.Right);

		tooltipPanel.Content.Add(topBtn);
		tooltipPanel.Content.Add(bottomBtn);
		tooltipPanel.Content.Add(leftBtn);
		tooltipPanel.Content.Add(rightBtn);
		container.Add(tooltipPanel);

		return container;
	}

	#endregion

	#region Game Specific Section

	private VisualElement CreateGameSpecificSection() {
		var container = new ScrollView(ScrollViewMode.Vertical);

		// Rarity Frames
		container.Add(CreateSectionHeader("Rarity Frames"));
		var rarityPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Horizontal)
			.AddChild(CreateRarityDemo(RarityType.Common, "Common"))
			.AddChild(CreateRarityDemo(RarityType.Uncommon, "Uncommon"))
			.AddChild(CreateRarityDemo(RarityType.Rare, "Rare"))
			.AddChild(CreateRarityDemo(RarityType.Epic, "Epic"))
			.AddChild(CreateRarityDemo(RarityType.Legendary, "Legendary"))
			.AddChild(CreateRarityDemo(RarityType.Mythic, "Mythic"))
			.Build();
		container.Add(rarityPanel);

		// Spinner/Slot Demo
		container.Add(CreateSectionHeader("Spinner (Slot Machine Reel)"));
		var spinnerItems = new List<string> { "🍎", "🍊", "🍋", "🍇", "🍒", "⭐", "💎" };

		var spinnerPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.Build();

		var resultLabel = new GameLabel("Spin the reel!").SetStyle(LabelStyle.TitleMedium).Build();

		var spinner = new GameSpinner<string>()
			.SetItems(spinnerItems)
			.SetItemTemplate(item => new GameLabel(item).SetStyle(LabelStyle.DisplaySmall).SetTextAlign(TextAnchor.MiddleCenter).Build())
			.SetVisibleItems(3)
			.SetItemSize(60)
			.SetSpinDuration(2000)
			.SetSize(80, 180)
			.OnSpinStart(() => {
				k_isSpinning = true;
				resultLabel.SetText("Spinning...");
			})
			.OnSpinComplete((result, index) => {
				k_isSpinning = false;
				resultLabel.SetText($"Result: {result}");
				if (result == "💎") {
					GameToast.Show("JACKPOT! 💎", ToastType.Success, 3000);
				}
			})
			.Build();

		var spinBtn = new GameButton("SPIN!")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.OnClick(() => {
				if (!k_isSpinning) {
					spinner.SpinToRandom();
				}
			})
			.Build();

		spinnerPanel.Content.Add(new GamePanel()
			.SetLayout(LayoutDirection.Horizontal)
			.SetVariant(PanelVariant.Ghost)
			.SetAlignment(Align.Center)
			.SetJustify(Justify.Center)
			.AddChild(spinner)
			.Build());
		spinnerPanel.Content.Add(resultLabel);
		spinnerPanel.Content.Add(spinBtn);
		container.Add(spinnerPanel);

		// Multi-Reel Slot Machine Demo
		container.Add(CreateSectionHeader("Multi-Reel Slot Machine"));
		var slotPanel = CreateSlotMachineDemo();
		container.Add(slotPanel);

		return container;
	}

	private VisualElement CreateRarityDemo(RarityType rarity, string label) {
		var wrapper = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetAlignment(Align.Center)
			.SetMargin(4, 8, 4, 8)
			.AddChild(new GameRarityFrame()
				.SetRarity(rarity)
				.SetSize(64)
				.SetShowGlow(rarity >= RarityType.Rare)
				.SetContent(new GameLabel("?").SetStyle(LabelStyle.TitleLarge).SetTextAlign(TextAnchor.MiddleCenter).Build())
				.Build())
			.AddChild(new GameLabel(label).SetStyle(LabelStyle.Caption).Build())
			.Build();
		return wrapper;
	}

	private VisualElement CreateSlotMachineDemo() {
		var slotItems = new List<string> { "🍎", "🍊", "🍋", "🍇", "🍒", "⭐", "💎" };
		var spinners = new List<GameSpinner<string>>();

		var slotContainer = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.Build();

		var reelsRow = new GamePanel()
			.SetLayout(LayoutDirection.Horizontal)
			.SetVariant(PanelVariant.Ghost)
			.SetJustify(Justify.Center)
			.Build();

		// Create 3 reels
		for (int i = 0; i < 3; i++) {
			int reelIndex = i;
			var reel = new GameSpinner<string>()
				.SetItems(slotItems)
				.SetItemTemplate(item => new GameLabel(item).SetStyle(LabelStyle.DisplaySmall).SetTextAlign(TextAnchor.MiddleCenter).Build())
				.SetVisibleItems(3)
				.SetItemSize(50)
				.SetSpinDuration(1500 + (reelIndex * 300))
				.SetSize(60, 150)
				.Build();

			spinners.Add(reel);
			reelsRow.Content.Add(reel);
		}

		var slotResultLabel = new GameLabel("Press SPIN to play!").SetStyle(LabelStyle.TitleMedium).Build();
		var spinAllBtn = new GameButton("🎰 SPIN ALL 🎰")
			.SetVariant(ButtonVariant.Primary)
			.SetSize(ButtonSize.Large)
			.OnClick(() => {
				bool anySpinning = spinners.Any(r => r.IsSpinning);
				if (!anySpinning) {
					foreach (var reel in spinners) {
						reel.SpinToRandom();
					}
					slotResultLabel.SetText("Spinning...");
				}
			})
			.Build();

		slotContainer.Content.Add(reelsRow);
		slotContainer.Content.Add(slotResultLabel);
		slotContainer.Content.Add(spinAllBtn);

		return slotContainer;
	}

	#endregion

	#region Themes Section

	private VisualElement CreateThemesSection() {
		var container = new ScrollView(ScrollViewMode.Vertical);

		container.Add(CreateSectionHeader("Theme Selector"));
		var themePanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Horizontal)
			.SetWrap(true)
			.AddChild(new GameButton("Dark Theme")
				.SetVariant(ButtonVariant.Primary)
				.OnClick(() => {
					GameTheme.SetTheme(ThemePresets.CreateDark());
					GameToast.Show("Dark theme applied", ToastType.Info);
				})
				.Build())
			.AddChild(new GameButton("Light Theme")
				.SetVariant(ButtonVariant.Secondary)
				.OnClick(() => {
					GameTheme.SetTheme(ThemePresets.CreateLight());
					GameToast.Show("Light theme applied", ToastType.Info);
				})
				.Build())
			.AddChild(new GameButton("Fantasy Theme")
				.SetVariant(ButtonVariant.Outline)
				.OnClick(() => {
					GameTheme.SetTheme(ThemePresets.CreateFantasy());
					GameToast.Show("Fantasy theme applied", ToastType.Info);
				})
				.Build())
			.AddChild(new GameButton("Sci-Fi Theme")
				.SetVariant(ButtonVariant.Outline)
				.OnClick(() => {
					GameTheme.SetTheme(ThemePresets.CreateSciFi());
					GameToast.Show("Sci-Fi theme applied", ToastType.Info);
				})
				.Build())
			.AddChild(new GameButton("High Contrast")
				.SetVariant(ButtonVariant.Outline)
				.OnClick(() => {
					GameTheme.SetTheme(ThemePresets.CreateHighContrast());
					GameToast.Show("High Contrast theme applied", ToastType.Info);
				})
				.Build())
			.AddChild(new GameButton("Colorblind Friendly")
				.SetVariant(ButtonVariant.Outline)
				.OnClick(() => {
					GameTheme.SetTheme(ThemePresets.CreateColorblindFriendly());
					GameToast.Show("Colorblind Friendly theme applied", ToastType.Info);
				})
				.Build())
			.Build();
		container.Add(themePanel);

		// Color Palette Preview
		container.Add(CreateSectionHeader("Current Color Palette"));
		container.Add(CreateColorPalettePreview());

		// Accessibility Settings
		container.Add(CreateSectionHeader("Accessibility"));
		var accessPanel = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.AddChild(new GameSlider()
				.SetLabel("UI Scale")
				.SetRange(0.75f, 1.5f)
				.SetValue(GameTheme.Current.Accessibility.UIScale)
				.SetShowValue()
				.SetValueFormat("{0:F2}x")
				.OnValueChanged(v => {
					GameTheme.Current.Accessibility.UIScale = v;
					GameTheme.NotifyThemeChanged();
				})
				.Build())
			.AddChild(new GameSlider()
				.SetLabel("Font Size Adjustment")
				.SetRange(-4, 8)
				.SetValue(GameTheme.Current.Accessibility.FontSizeAdjustment)
				.SetShowValue()
				.SetValueFormat("{0:+0;-0;0}px")
				.OnValueChanged(v => {
					GameTheme.Current.Accessibility.FontSizeAdjustment = (int)v;
					GameTheme.NotifyThemeChanged();
				})
				.Build())
			.AddChild(new GameCheckbox()
				.SetLabel("Reduce Motion")
				.SetChecked(GameTheme.Current.Accessibility.ReduceMotion)
				.OnValueChanged(v => {
					GameTheme.Current.Accessibility.ReduceMotion = v;
					GameTheme.NotifyThemeChanged();
				})
				.Build())
			.AddChild(new GameCheckbox()
				.SetLabel("Force High Contrast")
				.SetChecked(GameTheme.Current.Accessibility.ForceHighContrast)
				.OnValueChanged(v => {
					GameTheme.Current.Accessibility.ForceHighContrast = v;
					GameTheme.NotifyThemeChanged();
				})
				.Build())
			.Build();
		container.Add(accessPanel);

		return container;
	}

	private VisualElement CreateColorPalettePreview() {
		var colors = GameTheme.Current.Colors;

		var grid = new GamePanel()
			.SetVariant(PanelVariant.Card)
			.SetLayout(LayoutDirection.Horizontal)
			.SetWrap(true)
			.Build();

		var colorList = new (string name, Color color)[] {
			("Primary", colors.Primary),
			("Secondary", colors.Secondary),
			("Accent", colors.Accent),
			("Success", colors.Success),
			("Warning", colors.Warning),
			("Error", colors.Error),
			("Info", colors.Info),
			("Background", colors.Background),
			("Surface", colors.Surface),
			("Text Primary", colors.TextPrimary),
			("Text Secondary", colors.TextSecondary)
		};

		foreach (var (name, color) in colorList) {
			var swatch = new GamePanel()
				.SetVariant(PanelVariant.Ghost)
				.SetAlignment(Align.Center)
				.SetMargin(4, 4, 4, 4)
				.Build();

			var colorBox = new VisualElement {
				style = {
					width = 40,
					height = 40,
					backgroundColor = color,
					borderTopLeftRadius = 4,
					borderTopRightRadius = 4,
					borderBottomLeftRadius = 4,
					borderBottomRightRadius = 4
				}
			};

			swatch.Content.Add(colorBox);
			swatch.Content.Add(new GameLabel(name).SetStyle(LabelStyle.Caption).Build());
			grid.Content.Add(swatch);
		}

		return grid;
	}

	#endregion

	#region Helpers

	private VisualElement CreateSectionHeader(string title) {
		var header = new GamePanel()
			.SetVariant(PanelVariant.Ghost)
			.SetMargin(16, 0, 8, 0)
			.AddChild(new GameLabel(title).SetStyle(LabelStyle.HeadlineSmall).SetColor(LabelColor.Primary).Build())
			.AddChild(new GameDivider().SetVariant(DividerVariant.Light).Build())
			.Build();
		return header;
	}

	#endregion
}