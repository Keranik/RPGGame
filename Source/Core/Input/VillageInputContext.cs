using RPGGame.Core.Isometric;
using RPGGame.Core.Isometric.Rendering;
using RPGGame.Core.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGGame.Core.Input;

/// <summary>
/// Input context for the village phase.
/// Handles tile clicking, building selection via ground plane raycasting.
/// </summary>
public class VillageInputContext : IInputContext {
    private readonly VillageIsoRenderer _isoRenderer;
    private readonly IsoCameraController _cameraController;
    private readonly GameStateManager _stateManager;

    private bool _isSubscribed;

    public VillageInputContext(
        VillageIsoRenderer isoRenderer, 
        IsoCameraController cameraController,
        GameStateManager stateManager) {
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

        // Initialize and render
        _isoRenderer.Render();

        Debug.Log("VillageInputContext: Activated");
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

        Debug.Log("VillageInputContext: Deactivated");
    }

    public void ProcessInput(GameInputManager inputManager) {
        // Handle hover each frame using new Input System
        if (Mouse.current != null) {
            var mousePos = Mouse.current.position.ReadValue();
            var worldPos = _cameraController.ScreenToGroundPlane(mousePos);
            if (worldPos.HasValue) {
                _isoRenderer.HandleHover(worldPos.Value);
            } else {
                _isoRenderer.ClearHover();
            }
        }
    }

    private void OnWorldPrimaryAction(Vector2 screenPosition) {
        var worldPos = _cameraController.ScreenToGroundPlane(screenPosition);
        if (worldPos.HasValue) {
            _isoRenderer.HandleClick(worldPos.Value);
        }
    }

    private void OnWorldSecondaryAction(Vector2 screenPosition) {
        // Secondary action - could be used for context menu, cancel, etc.
        Debug.Log($"VillageInputContext: Secondary action at {screenPosition}");
    }

    private void OnZoom(float delta) {
        _cameraController.Zoom(delta);
    }
}