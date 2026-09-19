using RPGGame.Core;
using RPGGame.Core.Expedition;
using RPGGame.Core.Simulation;
using RPGGame.UI.Components;
using RPGGame.UI.DevOnly;
using UnityEngine.UIElements;

namespace RPGGame.UI;

[Dependency(registrationType: RegistrationType.Singleton)]
public class UiManager {
	private readonly Dictionary<string, VisualElement> k_gameWindows;
	private readonly UiBuilder k_uiBuilder;
	private readonly ModalManager k_modalManager;

	public string? ActiveWindowId { get; set; }

	public UiBuilder UiBuilder => k_uiBuilder;

	/// <summary>
	/// Centralized modal manager for dialogs and popups.
	/// Automatically handles pause/resume of game simulation.
	/// </summary>
	public ModalManager Modals => k_modalManager;

	public UiManager(
		UiBuilder uiBuilder,
		GameLoop gameLoop,
		GameStateManager stateManager,
		ExpeditionManager? expeditionManager = null
	) {
		k_gameWindows = new Dictionary<string, VisualElement>();
		k_uiBuilder = uiBuilder;
		k_modalManager = new ModalManager(gameLoop, stateManager, this, expeditionManager);

		UnityEngine.Debug.Log("UI Manager Initialized");
	}

	#region Modal Root Setup

	/// <summary>
	/// Sets the root element for modals. Should be called during UI initialization.
	/// </summary>
	public void SetModalRoot(VisualElement root) {
		k_modalManager.SetModalRoot(root);
	}

	#endregion

	#region Window Management

	public void RegisterWindow(string windowId, VisualElement window) {
		if (!k_gameWindows.ContainsKey(key: windowId)) {
			k_gameWindows.Add(key: windowId, value: window);
		}
	}

	public void UnregisterWindow(string windowId) {
		k_gameWindows.Remove(windowId);
	}

	public void ShowWindow(string windowId) {
		if (k_gameWindows.TryGetValue(key: windowId, value: out VisualElement? window)) {
			window.style.display = DisplayStyle.Flex;
			ActiveWindowId = windowId;
		}
	}

	public void HideWindow(string windowId) {
		if (k_gameWindows.TryGetValue(key: windowId, value: out VisualElement? window)) {
			window.style.display = DisplayStyle.None;
			if (ActiveWindowId == windowId) {
				ActiveWindowId = null;
			}
		}
	}

	public void ToggleWindow(string windowId) {
		if (k_gameWindows.TryGetValue(key: windowId, value: out VisualElement? window)) {
			if (window.style.display == DisplayStyle.Flex) {
				HideWindow(windowId);
			} else {
				ShowWindow(windowId);
			}
		}
	}

	public bool IsWindowVisible(string windowId) {
		return k_gameWindows.ContainsKey(key: windowId)
			&& k_gameWindows[key: windowId].style.display == DisplayStyle.Flex;
	}

	public bool TryGetWindow(string windowId, out VisualElement? window) {
		return k_gameWindows.TryGetValue(key: windowId, value: out window);
	}

	public T? GetWindow<T>(string windowId) where T : VisualElement {
		if (k_gameWindows.TryGetValue(windowId, out var window)) {
			return window as T;
		}
		return null;
	}

	#endregion

	#region Screen Management

	/// <summary>
	/// Hides all registered windows.
	/// </summary>
	public void HideAllWindows() {
		foreach (var window in k_gameWindows.Values) {
			window.style.display = DisplayStyle.None;
		}
		ActiveWindowId = null;
	}

	#endregion

	#region Quick Modal Access

	/// <summary>
	/// Shows an alert dialog with automatic pause/resume.
	/// </summary>
	public ModalHandle ShowAlert(string title, string message, Action? onOk = null) {
		return k_modalManager.ShowAlert(title, message, onOk);
	}

	/// <summary>
	/// Shows a confirmation dialog with automatic pause/resume.
	/// </summary>
	public ModalHandle ShowConfirm(string title, string message, Action onConfirm, Action? onCancel = null) {
		return k_modalManager.ShowConfirm(title, message, onConfirm, onCancel);
	}

	/// <summary>
	/// Shows a danger confirmation dialog with automatic pause/resume.
	/// </summary>
	public ModalHandle ShowConfirmDanger(string title, string message, Action onConfirm, Action? onCancel = null) {
		return k_modalManager.ShowConfirmDanger(title, message, onConfirm, onCancel);
	}

	#endregion

	#region Development Tools

	/// <summary>
	/// Opens the UI Component Showcase window for development/testing.
	/// </summary>
	public void DEBUG_OpenUIShowcase() {
		var showcase = new UIShowcaseWindow();
		showcase.InitializeWindow();
		k_uiBuilder.AddToPopupOverlay(showcase);
	}

	#endregion
}