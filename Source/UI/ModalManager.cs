using RPGGame.Core;
using RPGGame.Core.Expedition;
using RPGGame.Core.Input;
using RPGGame.Core.Simulation;
using RPGGame.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI;

/// <summary>
/// Centralized modal management system.
/// Automatically handles pausing/resuming the game simulation when modals are shown.
/// Tracks modal stack and restores previous simulation state when modals close.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class ModalManager {
	#region Fields

	private readonly GameLoop k_gameLoop;
	private readonly GameStateManager k_stateManager;
	private readonly ExpeditionManager? k_expeditionManager;
	private readonly UiManager k_uiManager;

	// Modal stack - supports nested modals
	private readonly Stack<ModalContext> k_modalStack = new();

	// Root element for modals
	private VisualElement? k_modalRoot;

	#endregion

	#region Properties

	/// <summary>
	/// Whether any modal is currently active.
	/// </summary>
	public bool IsModalActive => k_modalStack.Count > 0;

	/// <summary>
	/// Number of active modals (for nested modals).
	/// </summary>
	public int ActiveModalCount => k_modalStack.Count;

	/// <summary>
	/// The currently active modal element (topmost).
	/// </summary>
	public VisualElement? CurrentModal => k_modalStack.Count > 0 ? k_modalStack.Peek().Element : null;

	#endregion

	#region Events

	/// <summary>
	/// Fired when a modal is opened.
	/// </summary>
	public event Action<VisualElement>? OnModalOpened;

	/// <summary>
	/// Fired when a modal is closed.
	/// </summary>
	public event Action<VisualElement>? OnModalClosed;

	/// <summary>
	/// Fired when all modals are closed.
	/// </summary>
	public event Action? OnAllModalsClosed;

	#endregion

	#region Constructor

	public ModalManager(
		GameLoop gameLoop,
		GameStateManager stateManager,
		UiManager uiManager,
		ExpeditionManager? expeditionManager = null
	) {
		k_gameLoop = gameLoop;
		k_stateManager = stateManager;
		k_uiManager = uiManager;
		k_expeditionManager = expeditionManager;

		Debug.Log("ModalManager: Initialized");
	}

	#endregion

	#region Setup

	/// <summary>
	/// Sets the root element where modals will be added.
	/// </summary>
	public void SetModalRoot(VisualElement root) {
		k_modalRoot = root;
		Debug.Log("ModalManager: Modal root set");
	}

	#endregion

	#region Show Modal Methods

	/// <summary>
	/// Shows a modal element, automatically pausing the simulation.
	/// </summary>
	/// <param name="modal">The modal element to show.</param>
	/// <param name="pauseSimulation">Whether to pause the simulation while modal is open.</param>
	/// <returns>A handle to close the modal.</returns>
	public ModalHandle ShowModal(VisualElement modal, bool pauseSimulation = true) {
		if (modal == null) {
			Debug.LogError("ModalManager: Cannot show null modal");
			return new ModalHandle(this, null!);
		}

		// Capture current simulation state before pausing
		var context = new ModalContext(
			element: modal,
			pauseSimulation: pauseSimulation,
			previousGameLoopPaused: k_gameLoop.IsPaused,
			previousGameLoopActive: k_gameLoop.IsActive,
			previousSimSpeed: k_expeditionManager?.SimSpeed ?? SimulationSpeed.Normal,
			previousTravelSpeed: k_expeditionManager?.Travel?.Speed ?? TravelSpeed.Normal
		);

		// Add to modal root if available
		if (k_modalRoot != null && modal.parent != k_modalRoot) {
			k_modalRoot.Add(modal);
		}

		// Show the modal
		modal.style.display = DisplayStyle.Flex;

		// Pause simulation if requested
		if (pauseSimulation) {
			PauseForModal();
		}

		// Register with input manager
		GameInputManager.Instance?.RegisterModal(modal);

		// Push to stack
		k_modalStack.Push(context);

		Debug.Log($"ModalManager: Modal opened (stack depth: {k_modalStack.Count})");
		OnModalOpened?.Invoke(modal);

		return new ModalHandle(this, modal);
	}

	/// <summary>
	/// Shows a GameDialog, automatically pausing the simulation.
	/// </summary>
	public ModalHandle ShowDialog(GameDialog dialog, bool pauseSimulation = true) {
		// Hook into dialog's close event
		dialog.OnClose(() => {
			CloseModal(dialog);
		});

		return ShowModal(dialog, pauseSimulation);
	}

	/// <summary>
	/// Creates and shows an alert dialog.
	/// </summary>
	public ModalHandle ShowAlert(string title, string message, Action? onOk = null) {
		var dialog = GameDialog.Alert(title, message, onOk);
		return ShowDialog(dialog.Build());
	}

	/// <summary>
	/// Creates and shows a confirmation dialog.
	/// </summary>
	public ModalHandle ShowConfirm(string title, string message, Action onConfirm, Action? onCancel = null) {
		var dialog = GameDialog.Confirm(title, message, onConfirm, onCancel);
		return ShowDialog(dialog.Build());
	}

	/// <summary>
	/// Creates and shows a danger confirmation dialog.
	/// </summary>
	public ModalHandle ShowConfirmDanger(string title, string message, Action onConfirm, Action? onCancel = null) {
		var dialog = GameDialog.ConfirmDanger(title, message, onConfirm, onCancel);
		return ShowDialog(dialog.Build());
	}

	/// <summary>
	/// Creates and shows a custom dialog with content.
	/// </summary>
	public ModalHandle ShowCustomDialog(string title, VisualElement content, bool pauseSimulation = true) {
		var dialog = GameDialog.Custom(title, content);
		return ShowDialog(dialog.Build(), pauseSimulation);
	}

	#endregion

	#region Close Modal Methods

	/// <summary>
	/// Closes a specific modal.
	/// </summary>
	public void CloseModal(VisualElement modal) {
		if (modal == null) return;

		// Find and remove from stack
		var tempStack = new Stack<ModalContext>();
		ModalContext? closedContext = null;

		while (k_modalStack.Count > 0) {
			var context = k_modalStack.Pop();
			if (context.Element == modal) {
				closedContext = context;
				break;
			}
			tempStack.Push(context);
		}

		// Restore remaining modals to stack
		while (tempStack.Count > 0) {
			k_modalStack.Push(tempStack.Pop());
		}

		if (closedContext == null) {
			Debug.LogWarning("ModalManager: Modal not found in stack");
			return;
		}

		// Hide the modal
		modal.style.display = DisplayStyle.None;

		// Unregister from input manager
		GameInputManager.Instance?.UnregisterModal(modal);

		// Remove from hierarchy if we added it
		if (k_modalRoot != null && modal.parent == k_modalRoot) {
			modal.RemoveFromHierarchy();
		}

		Debug.Log($"ModalManager: Modal closed (stack depth: {k_modalStack.Count})");
		OnModalClosed?.Invoke(modal);

		// Restore simulation state if no more modals
		if (k_modalStack.Count == 0) {
			if (closedContext.PauseSimulation) {
				RestoreSimulationState(closedContext);
			}
			OnAllModalsClosed?.Invoke();
		}
	}

	/// <summary>
	/// Closes the topmost modal.
	/// </summary>
	public void CloseTopModal() {
		if (k_modalStack.Count == 0) return;
		CloseModal(k_modalStack.Peek().Element);
	}

	/// <summary>
	/// Closes all open modals.
	/// </summary>
	public void CloseAllModals() {
		// Get the bottom context for restoration
		ModalContext? bottomContext = null;
		while (k_modalStack.Count > 0) {
			var context = k_modalStack.Pop();
			if (k_modalStack.Count == 0) {
				bottomContext = context;
			}

			// Hide and cleanup
			context.Element.style.display = DisplayStyle.None;
			GameInputManager.Instance?.UnregisterModal(context.Element);

			if (k_modalRoot != null && context.Element.parent == k_modalRoot) {
				context.Element.RemoveFromHierarchy();
			}

			OnModalClosed?.Invoke(context.Element);
		}

		// Restore simulation state from the first modal that was opened
		if (bottomContext?.PauseSimulation == true) {
			RestoreSimulationState(bottomContext);
		}

		Debug.Log("ModalManager: All modals closed");
		OnAllModalsClosed?.Invoke();
	}

	#endregion

	#region Simulation Control

	private void PauseForModal() {
		// Pause the game loop
		if (k_gameLoop.IsActive && !k_gameLoop.IsPaused) {
			k_gameLoop.Pause();
			Debug.Log("ModalManager: Paused game loop for modal");
		}

		// Also pause expedition simulation if active
		if (k_expeditionManager?.IsActive == true) {
			k_expeditionManager.SetSimulationSpeed(SimulationSpeed.Paused);
		}
	}

	private void RestoreSimulationState(ModalContext context) {
		// Only restore if we were the ones who paused
		if (!context.PauseSimulation) return;

		// Restore game loop state
		if (context.PreviousGameLoopActive) {
			if (!context.PreviousGameLoopPaused) {
				k_gameLoop.Resume();
				Debug.Log("ModalManager: Resumed game loop after modal");
			}
		}

		// Restore expedition simulation state
		if (k_expeditionManager?.IsActive == true) {
			k_expeditionManager.SetSimulationSpeed(context.PreviousSimSpeed);
			Debug.Log($"ModalManager: Restored simulation speed to {context.PreviousSimSpeed}");
		}
	}

	#endregion

	#region Utility

	/// <summary>
	/// Checks if a specific modal is open.
	/// </summary>
	public bool IsModalOpen(VisualElement modal) {
		return k_modalStack.Any(c => c.Element == modal);
	}

	/// <summary>
	/// Gets all currently open modals.
	/// </summary>
	public IEnumerable<VisualElement> GetOpenModals() {
		return k_modalStack.Select(c => c.Element);
	}

	#endregion

	#region Nested Types

	/// <summary>
	/// Context stored for each open modal.
	/// </summary>
	private class ModalContext {
		public VisualElement Element { get; }
		public bool PauseSimulation { get; }
		public bool PreviousGameLoopPaused { get; }
		public bool PreviousGameLoopActive { get; }
		public SimulationSpeed PreviousSimSpeed { get; }
		public TravelSpeed PreviousTravelSpeed { get; }

		public ModalContext(
			VisualElement element,
			bool pauseSimulation,
			bool previousGameLoopPaused,
			bool previousGameLoopActive,
			SimulationSpeed previousSimSpeed,
			TravelSpeed previousTravelSpeed
		) {
			Element = element;
			PauseSimulation = pauseSimulation;
			PreviousGameLoopPaused = previousGameLoopPaused;
			PreviousGameLoopActive = previousGameLoopActive;
			PreviousSimSpeed = previousSimSpeed;
			PreviousTravelSpeed = previousTravelSpeed;
		}
	}

	#endregion
}

/// <summary>
/// Handle returned when showing a modal.
/// Provides a convenient way to close the modal.
/// </summary>
public readonly struct ModalHandle {
	private readonly ModalManager k_manager;
	private readonly VisualElement k_modal;

	public ModalHandle(ModalManager manager, VisualElement modal) {
		k_manager = manager;
		k_modal = modal;
	}

	/// <summary>
	/// Closes this modal.
	/// </summary>
	public void Close() {
		k_manager.CloseModal(k_modal);
	}

	/// <summary>
	/// Whether this modal is currently open.
	/// </summary>
	public bool IsOpen => k_manager.IsModalOpen(k_modal);
}