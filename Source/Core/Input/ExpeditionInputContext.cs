using RPGGame.Core.Expedition;
using RPGGame.Core.Isometric.Rendering;
using RPGGame.Core.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGGame.Core.Input;

/// <summary>
/// Input context for the expedition phase.
/// Handles map interaction, node clicking, camera panning, and camera controls via the isometric renderer.
/// </summary>
public class ExpeditionInputContext : IInputContext {
    private readonly ExpeditionManager _expeditionManager;
    private readonly ExpeditionIsoRenderer _isoRenderer;
    private readonly IsoCameraController _cameraController;
    private readonly GameStateManager _stateManager;

    private bool _isSubscribed;
    
    // Manual pan state
    private bool _isPanning;
    private Vector2 _lastMouseScreenPos;

    public ExpeditionInputContext(
        ExpeditionManager expeditionManager,
        ExpeditionIsoRenderer isoRenderer,
        IsoCameraController cameraController,
        GameStateManager stateManager) {
        _expeditionManager = expeditionManager;
        _isoRenderer = isoRenderer;
        _cameraController = cameraController;
        _stateManager = stateManager;
    }

    public void OnActivate() {
        if (_isSubscribed) return;

        var inputManager = GameInputManager.Instance;
        if (inputManager != null) {
            inputManager.OnWorldPrimaryAction += OnWorldPrimaryAction;
            inputManager.OnWorldSecondaryAction += OnWorldSecondaryAction;
            inputManager.OnZoom += OnZoom;
            _isSubscribed = true;
        }

        // Render the expedition map if active
        if (_expeditionManager.IsActive) {
            _isoRenderer.Render();
            _isoRenderer.SetupCamera();
            
            // Set up initial pan bounds based on render radius
            UpdateCameraPanBounds();
        }

        _isPanning = false;
        Debug.Log("ExpeditionInputContext: Activated");
    }

    public void OnDeactivate() {
        if (!_isSubscribed) return;

        var inputManager = GameInputManager.Instance;
        if (inputManager != null) {
            inputManager.OnWorldPrimaryAction -= OnWorldPrimaryAction;
            inputManager.OnWorldSecondaryAction -= OnWorldSecondaryAction;
            inputManager.OnZoom -= OnZoom;
            _isSubscribed = false;
        }

        // End any active panning
        if (_isPanning) {
            _cameraController.EndManualPan();
            _isPanning = false;
        }

        Debug.Log("ExpeditionInputContext: Deactivated");
    }

    public void ProcessInput(GameInputManager inputManager) {
        if (Mouse.current == null) return;

        var mousePos = Mouse.current.position.ReadValue();

        // Handle right-click panning
        HandlePanning(mousePos);

        // Handle hover (only when not panning)
        if (!_isPanning) {
            var worldPos = _cameraController.ScreenToGroundPlane(mousePos);
            if (worldPos.HasValue) {
                _isoRenderer.HandleHover(worldPos.Value);
            } else {
                _isoRenderer.ClearHover();
            }
        }

        // Update pan bounds as player moves
        UpdateCameraPanBounds();
    }

    private void HandlePanning(Vector2 mousePos) {
        if (Mouse.current == null) return;

        bool rightMouseDown = Mouse.current.rightButton.isPressed;
        bool rightMousePressed = Mouse.current.rightButton.wasPressedThisFrame;
        bool rightMouseReleased = Mouse.current.rightButton.wasReleasedThisFrame;

        if (rightMousePressed) {
            _cameraController.BeginManualPan();
            _isPanning = true;
            _lastMouseScreenPos = mousePos;
        }

        if (rightMouseReleased && _isPanning) {
            _cameraController.EndManualPan();
            _isPanning = false;
        }

        if (rightMouseDown && _isPanning) {
            // Calculate screen delta and convert to world pan
            Vector2 screenDelta = mousePos - _lastMouseScreenPos;
            
            if (screenDelta.sqrMagnitude > 0.01f) {
                Vector3 worldDelta = _cameraController.ScreenDeltaToWorldPan(screenDelta);
                _cameraController.ApplyManualPan(worldDelta);
            }

            _lastMouseScreenPos = mousePos;
        }
    }

    private void UpdateCameraPanBounds() {
        var travel = _expeditionManager.Travel;
        if (travel == null) return;

        // Set pan bounds based on rendered area
        // The renderer uses a radius of ~20 tiles by default, use slightly less for margin
        const float RENDER_RADIUS = 18f;

        Vector3 playerWorldPos = _isoRenderer.ToWorldPosition(travel.VisualPosition);
        _cameraController.SetPanBoundsFromRadius(playerWorldPos, RENDER_RADIUS);
    }

    private void OnWorldPrimaryAction(Vector2 screenPosition) {
        // Don't process clicks while panning
        if (_isPanning) return;

        var worldPos = _cameraController.ScreenToGroundPlane(screenPosition);
        if (worldPos.HasValue) {
            _isoRenderer.HandleClick(worldPos.Value);
        }
    }

    private void OnWorldSecondaryAction(Vector2 screenPosition) {
        // Secondary action is now handled by panning in ProcessInput
        // This callback is for the initial press which starts the pan
    }

    private void OnZoom(float delta) {
        _cameraController.Zoom(delta);
    }
}