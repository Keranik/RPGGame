namespace RPGGame.UI.Styles;

#nullable disable

public class AudioFeedbackSettings {
	public bool Enabled { get; set; } = true;
	public float MasterVolume { get; set; } = 0.5f;

	public string ButtonClick { get; set; } = "ui_button_click";
	public string ButtonHover { get; set; } = "ui_button_hover";
	public string TabSwitch { get; set; } = "ui_tab_switch";
	public string ToggleOn { get; set; } = "ui_toggle_on";
	public string ToggleOff { get; set; } = "ui_toggle_off";
	public string SliderChange { get; set; } = "ui_slider_tick";
	public string DropdownOpen { get; set; } = "ui_dropdown_open";
	public string DropdownSelect { get; set; } = "ui_dropdown_select";
	public string DialogOpen { get; set; } = "ui_dialog_open";
	public string DialogClose { get; set; } = "ui_dialog_close";
	public string Notification { get; set; } = "ui_notification";
	public string Error { get; set; } = "ui_error";
	public string Success { get; set; } = "ui_success";

	public AudioFeedbackSettings Clone() {
		return new AudioFeedbackSettings { Enabled = Enabled, MasterVolume = MasterVolume };
	}
}