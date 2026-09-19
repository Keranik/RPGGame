using RPGGame.Core.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace RPGGame.Core.Input;

/// <summary>
/// Central input manager that wraps Unity's Input System.
/// Provides unified input handling across all game phases.
/// Supports mouse, touch, keyboard, and gamepad.
/// 
/// IMPORTANT: This manager respects UI layering. Pointer events are NOT fired
/// to the game world if the pointer is over a UI element that handles input.
/// </summary>
/// [Dependency(RegistrationType.Singleton)]
public class GameInputManager : MonoBehaviour {
	#region Singleton

	private static GameInputManager? s_instance;
	public static GameInputManager? Instance => s_instance;

	#endregion

	#region Fields

	private GameStateManager? k_stateManager;

	// Input devices (cached, refreshed as needed)
	private Mouse? k_mouse;
	private Keyboard? k_keyboard;
	private Touchscreen? k_touchscreen;
	private Gamepad? k_gamepad;

	// Current input state
	private Vector2 k_pointerPosition;
	private Vector2 k_moveInput;
	private bool k_isUsingGamepad;

	// Active input context
	private IInputContext? k_activeContext;
	private readonly Dictionary<GamePhase, IInputContext> k_contexts = new();

	// UI state tracking
	private UIDocument? k_primaryUIDocument;
	private readonly HashSet<VisualElement> k_modalElements = new();

	// Debug
	private bool k_debugClickLogging = true;

	#endregion

	#region Properties

	/// <summary>
	/// Current pointer/mouse position in screen coordinates.
	/// </summary>
	public Vector2 PointerPosition => k_pointerPosition;

	/// <summary>
	/// Current movement input (WASD, left stick, etc.)
	/// </summary>
	public Vector2 MoveInput => k_moveInput;

	/// <summary>
	/// Whether the player is currently using a gamepad.
	/// </summary>
	public bool IsUsingGamepad => k_isUsingGamepad;

	/// <summary>
	/// Currently active input context.
	/// </summary>
	public IInputContext? ActiveContext => k_activeContext;

	/// <summary>
	/// Whether the pointer is currently over a UI element.
	/// </summary>
	public bool IsPointerOverUI { get; private set; }

	/// <summary>
	/// Whether a modal UI element is currently active.
	/// When true, all game world input is blocked.
	/// </summary>
	public bool IsModalActive => k_modalElements.Count > 0;

	#endregion

	#region Events

	// Pointer/Mouse events - ONLY fired when not over UI
	public event Action<Vector2>? OnWorldPrimaryAction;
	public event Action<Vector2>? OnWorldSecondaryAction;
	public event Action<Vector2>? OnWorldPointerDown;
	public event Action<Vector2>? OnWorldPointerUp;
	public event Action<Vector2>? OnWorldPointerMove;

	// Raw pointer events - ALWAYS fired
	public event Action<Vector2>? OnPointerMove;
	public event Action<Vector2>? OnPrimaryAction;
	public event Action<Vector2>? OnSecondaryAction;
	public event Action<Vector2>? OnPointerDown;
	public event Action<Vector2>? OnPointerUp;

	// Movement events
	public event Action<Vector2>? OnMoveInput;

	// Zoom events - fired when scroll wheel or pinch gesture detected
	/// <summary>
	/// Fired when zoom input is detected (scroll wheel, pinch gesture, gamepad triggers).
	/// Positive delta = zoom out, negative delta = zoom in.
	/// Only fired when not over UI and no modal is active.
	/// </summary>
	public event Action<float>? OnZoom;

	// UI Navigation events
	public event Action? OnNavigateUp;
	public event Action? OnNavigateDown;
	public event Action? OnNavigateLeft;
	public event Action? OnNavigateRight;
	public event Action? OnConfirm;
	public event Action? OnCancel;

	// Game control events
	public event Action? OnPause;
	public event Action? OnOpenInventory;
	public event Action? OnOpenMap;
	public event Action? OnOpenCharacter;

	// Speed control events
	public event Action? OnSpeedUp;
	public event Action? OnSpeedDown;
	public event Action? OnTogglePause;

	// Device change events
	public event Action<bool>? OnInputDeviceChanged;

	#endregion

	#region Initialization

	public static GameInputManager Create(GameStateManager stateManager) {
		var go = new GameObject("GameInputManager");
		var manager = go.AddComponent<GameInputManager>();
		manager.Initialize(stateManager);
		DontDestroyOnLoad(go);
		return manager;
	}

	public void Initialize(GameStateManager stateManager) {
		if (s_instance != null && s_instance != this) {
			Destroy(gameObject);
			return;
		}
		s_instance = this;

		k_stateManager = stateManager;
		k_stateManager.OnPhaseChanged += OnPhaseChanged;

		RefreshDevices();

		Debug.Log("GameInputManager: Initialized");
	}

	public void SetPrimaryUIDocument(UIDocument document) {
		k_primaryUIDocument = document;
		Debug.Log("GameInputManager: Primary UI document set");
	}

	private void RefreshDevices() {
		k_mouse = Mouse.current;
		k_keyboard = Keyboard.current;
		k_touchscreen = Touchscreen.current;
		k_gamepad = Gamepad.current;
	}

	#endregion

	#region Modal Management

	public void RegisterModal(VisualElement modal) {
		k_modalElements.Add(modal);
		Debug.Log($"GameInputManager: Modal registered ({k_modalElements.Count} active)");
	}

	public void UnregisterModal(VisualElement modal) {
		k_modalElements.Remove(modal);
		Debug.Log($"GameInputManager: Modal unregistered ({k_modalElements.Count} active)");
	}

	public bool TryCloseTopModal() {
		return IsModalActive;
	}

	#endregion

	#region Context Management

	public void RegisterContext(GamePhase phase, IInputContext context) {
		k_contexts[phase] = context;
		Debug.Log($"GameInputManager: Registered context for {phase}");
	}

	public void RegisterContext(IEnumerable<GamePhase> phases, IInputContext context) {
		foreach (var phase in phases) {
			k_contexts[phase] = context;
		}
	}

	private void OnPhaseChanged(GamePhase oldPhase, GamePhase newPhase) {
		k_activeContext?.OnDeactivate();

		if (k_contexts.TryGetValue(newPhase, out var context)) {
			k_activeContext = context;
			k_activeContext.OnActivate();
			Debug.Log($"GameInputManager: Switched to {context.GetType().Name} for {newPhase}");
		} else {
			k_activeContext = null;
			Debug.Log($"GameInputManager: No context for {newPhase}");
		}
	}

	#endregion

	#region Unity Update

	void Update() {
		RefreshDevicesIfNeeded();
		ProcessPointerInput();
		ProcessZoomInput();
		ProcessMovementInput();
		ProcessButtonInput();

		k_activeContext?.ProcessInput(this);
	}

	private void RefreshDevicesIfNeeded() {
		if (k_mouse == null) k_mouse = Mouse.current;
		if (k_keyboard == null) k_keyboard = Keyboard.current;
		if (k_touchscreen == null) k_touchscreen = Touchscreen.current;
		if (k_gamepad == null) k_gamepad = Gamepad.current;

		bool wasUsingGamepad = k_isUsingGamepad;

		if (k_gamepad != null && k_gamepad.wasUpdatedThisFrame) {
			k_isUsingGamepad = true;
		} else if ((k_mouse != null && k_mouse.wasUpdatedThisFrame) ||
				   (k_keyboard != null && k_keyboard.wasUpdatedThisFrame)) {
			k_isUsingGamepad = false;
		}

		if (wasUsingGamepad != k_isUsingGamepad) {
			OnInputDeviceChanged?.Invoke(k_isUsingGamepad);
		}
	}

	/// <summary>
	/// Checks if the given screen position is over an interactive UI element.
	/// </summary>
	private bool CheckPointerOverUI(Vector2 screenPosition) {
		if (k_primaryUIDocument == null) {
			return false;
		}

		var root = k_primaryUIDocument.rootVisualElement;
		if (root == null || root.panel == null) {
			return false;
		}

		// Convert screen position to panel coordinates
		// RuntimePanelUtils handles the coordinate system conversion properly
		var panelPosition = RuntimePanelUtils.ScreenToPanel(root.panel, screenPosition);

		// Use UI Toolkit's built-in hit testing
		var pickedElement = root.panel.Pick(panelPosition);

		// Not over UI if we didn't pick anything
		if (pickedElement == null) {
			return false;
		}

		// Not over UI if the picked element ignores picking
		if (pickedElement.pickingMode == PickingMode.Ignore) {
			return false;
		}

		// We're over an interactive UI element
		return true;
	}

	private void ProcessPointerInput() {
		Vector2 newPosition = k_pointerPosition;
		bool pointerMoved = false;
		bool primaryPressed = false;
		bool primaryReleased = false;
		bool secondaryPressed = false;

		// Mouse
		if (k_mouse != null) {
			var mousePos = k_mouse.position.ReadValue();
			if (mousePos != k_pointerPosition) {
				newPosition = mousePos;
				pointerMoved = true;
			}

			primaryPressed = k_mouse.leftButton.wasPressedThisFrame;
			primaryReleased = k_mouse.leftButton.wasReleasedThisFrame;
			secondaryPressed = k_mouse.rightButton.wasPressedThisFrame;
		}

		// Touch
		if (k_touchscreen != null) {
			var primaryTouch = k_touchscreen.primaryTouch;
			if (primaryTouch.press.isPressed) {
				newPosition = primaryTouch.position.ReadValue();
				pointerMoved = true;
			}

			if (primaryTouch.press.wasPressedThisFrame) primaryPressed = true;
			if (primaryTouch.press.wasReleasedThisFrame) primaryReleased = true;
		}

		// Update position and check UI state
		if (pointerMoved) {
			k_pointerPosition = newPosition;
		}

		// Check if over UI at current position
		IsPointerOverUI = CheckPointerOverUI(k_pointerPosition);

		// Fire movement events
		if (pointerMoved) {
			OnPointerMove?.Invoke(k_pointerPosition);

			if (!IsPointerOverUI && !IsModalActive) {
				OnWorldPointerMove?.Invoke(k_pointerPosition);
			}
		}

		// Process clicks
		if (primaryPressed) {
			// Check UI state at click position
			bool clickedOnUI = CheckPointerOverUI(newPosition);

			if (k_debugClickLogging) {
				Debug.Log($"GameInputManager: Primary click at {newPosition}, overUI={clickedOnUI}, modalActive={IsModalActive}");
			}

			OnPointerDown?.Invoke(newPosition);
			OnPrimaryAction?.Invoke(newPosition);

			// Only fire world events if not over UI and no modal
			if (!clickedOnUI && !IsModalActive) {
				if (k_debugClickLogging) {
					Debug.Log($"GameInputManager: Firing OnWorldPrimaryAction");
				}
				OnWorldPointerDown?.Invoke(newPosition);
				OnWorldPrimaryAction?.Invoke(newPosition);
			}
		}

		if (primaryReleased) {
			OnPointerUp?.Invoke(newPosition);

			if (!IsPointerOverUI && !IsModalActive) {
				OnWorldPointerUp?.Invoke(newPosition);
			}
		}

		if (secondaryPressed) {
			OnSecondaryAction?.Invoke(newPosition);

			if (!IsPointerOverUI && !IsModalActive) {
				OnWorldSecondaryAction?.Invoke(newPosition);
			}
		}
	}

	/// <summary>
	/// Processes zoom input from mouse scroll wheel, touch pinch, or gamepad triggers.
	/// </summary>
	private void ProcessZoomInput() {
		float zoomDelta = 0f;

		// Mouse scroll wheel
		if (k_mouse != null) {
			var scrollValue = k_mouse.scroll.ReadValue();
			if (Mathf.Abs(scrollValue.y) > 0.01f) {
				// Normalize scroll value - scroll up (positive) = zoom in (negative delta)
				// scroll down (negative) = zoom out (positive delta)
				zoomDelta = -scrollValue.y / 120f; // 120 is typical scroll wheel delta per notch
			}
		}

		// Gamepad triggers for zoom (hold RT to zoom in, LT to zoom out)
		if (k_gamepad != null) {
			float rtValue = k_gamepad.rightTrigger.ReadValue();
			float ltValue = k_gamepad.leftTrigger.ReadValue();
			
			if (rtValue > 0.1f || ltValue > 0.1f) {
				// RT = zoom in (negative), LT = zoom out (positive)
				zoomDelta = (ltValue - rtValue) * 0.05f; // Scale for smooth zoom
			}
		}

		// TODO: Touch pinch gesture support could be added here

		// Fire zoom event if there's input and we're not over UI
		if (Mathf.Abs(zoomDelta) > 0.001f && !IsPointerOverUI && !IsModalActive) {
			OnZoom?.Invoke(zoomDelta);
		}
	}

	private void ProcessMovementInput() {
		Vector2 move = Vector2.zero;

		if (k_keyboard != null) {
			if (k_keyboard.wKey.isPressed || k_keyboard.upArrowKey.isPressed) move.y += 1;
			if (k_keyboard.sKey.isPressed || k_keyboard.downArrowKey.isPressed) move.y -= 1;
			if (k_keyboard.aKey.isPressed || k_keyboard.leftArrowKey.isPressed) move.x -= 1;
			if (k_keyboard.dKey.isPressed || k_keyboard.rightArrowKey.isPressed) move.x += 1;
		}

		if (k_gamepad != null) {
			var stick = k_gamepad.leftStick.ReadValue();
			if (stick.magnitude > 0.1f) {
				move = stick;
			}
		}

		if (move != k_moveInput) {
			k_moveInput = move;
			OnMoveInput?.Invoke(k_moveInput);
		}
	}

	private void ProcessButtonInput() {
		bool cancelPressed = (k_keyboard != null && k_keyboard.escapeKey.wasPressedThisFrame) ||
							  (k_gamepad != null && k_gamepad.bButton.wasPressedThisFrame);

		if (cancelPressed) {
			OnCancel?.Invoke();
			if (!IsModalActive) {
				if (k_keyboard != null && k_keyboard.escapeKey.wasPressedThisFrame) {
					OnPause?.Invoke();
				}
			}
		}

		if (k_gamepad != null && k_gamepad.startButton.wasPressedThisFrame) {
			OnPause?.Invoke();
		}

		if ((k_keyboard != null && (k_keyboard.enterKey.wasPressedThisFrame || k_keyboard.spaceKey.wasPressedThisFrame)) ||
			(k_gamepad != null && k_gamepad.aButton.wasPressedThisFrame)) {
			OnConfirm?.Invoke();
		}

		if ((k_keyboard != null && k_keyboard.iKey.wasPressedThisFrame) ||
			(k_gamepad != null && k_gamepad.yButton.wasPressedThisFrame)) {
			OnOpenInventory?.Invoke();
		}

		if ((k_keyboard != null && k_keyboard.cKey.wasPressedThisFrame) ||
			(k_gamepad != null && k_gamepad.xButton.wasPressedThisFrame)) {
			OnOpenCharacter?.Invoke();
		}

		if ((k_keyboard != null && k_keyboard.mKey.wasPressedThisFrame) ||
			(k_gamepad != null && k_gamepad.selectButton.wasPressedThisFrame)) {
			OnOpenMap?.Invoke();
		}

		if (k_keyboard != null) {
			if (k_keyboard.equalsKey.wasPressedThisFrame || k_keyboard.numpadPlusKey.wasPressedThisFrame) {
				OnSpeedUp?.Invoke();
			}
			if (k_keyboard.minusKey.wasPressedThisFrame || k_keyboard.numpadMinusKey.wasPressedThisFrame) {
				OnSpeedDown?.Invoke();
			}
			if (k_keyboard.spaceKey.wasPressedThisFrame) {
				OnTogglePause?.Invoke();
			}
		}

		if (k_gamepad != null) {
			if (k_gamepad.dpad.up.wasPressedThisFrame) OnNavigateUp?.Invoke();
			if (k_gamepad.dpad.down.wasPressedThisFrame) OnNavigateDown?.Invoke();
			if (k_gamepad.dpad.left.wasPressedThisFrame) OnNavigateLeft?.Invoke();
			if (k_gamepad.dpad.right.wasPressedThisFrame) OnNavigateRight?.Invoke();
		}
	}

	#endregion

	#region Utility Methods

	public Vector3 ScreenToWorldPoint(Vector2 screenPosition) {
		var camera = Camera.main;
		if (camera == null) return Vector3.zero;
		return camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, camera.nearClipPlane));
	}

	public Vector3 GetPointerWorldPosition() {
		return ScreenToWorldPoint(k_pointerPosition);
	}

	/// <summary>
	/// Enable or disable debug click logging.
	/// </summary>
	public void SetDebugClickLogging(bool enabled) {
		k_debugClickLogging = enabled;
	}

	#endregion

	#region Cleanup

	void OnDestroy() {
		if (k_stateManager != null) {
			k_stateManager.OnPhaseChanged -= OnPhaseChanged;
		}
		if (s_instance == this) {
			s_instance = null;
		}
	}

	#endregion
}