using UnityEngine;

namespace RPGGame.Core.Isometric.Rendering;

/// <summary>
/// Pure C# camera controller for isometric games.
/// Handles positioning, rotation, zoom, follow, and smooth transitions.
/// 
/// <para>
/// This is a non-MonoBehaviour class designed for dependency injection.
/// Call <see cref="Update"/> each frame to process smooth transitions.
/// </para>
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class IsoCameraController {
    #region Constants

    /// <summary>Default isometric Y-axis rotation (45° for diamond grid).</summary>
    public const float DEFAULT_YAW = 45f;

    /// <summary>Default isometric X-axis tilt (45° looking down).</summary>
    public const float DEFAULT_PITCH = 45f;

    /// <summary>Default camera distance from target along the view axis.</summary>
    public const float DEFAULT_DISTANCE = 50f;

    /// <summary>Default orthographic size.</summary>
    public const float DEFAULT_ORTHO_SIZE = 35f;

    /// <summary>Default minimum zoom (ortho size).</summary>
    public const float DEFAULT_ZOOM_MIN = 5f;

    /// <summary>Default maximum zoom (ortho size).</summary>
    public const float DEFAULT_ZOOM_MAX = 100f;

    /// <summary>Default zoom speed multiplier.</summary>
    public const float DEFAULT_ZOOM_SPEED = 5f;

    /// <summary>Default smooth follow time.</summary>
    public const float DEFAULT_SMOOTH_TIME = 0.15f;

    /// <summary>Default snap-back time when releasing manual pan.</summary>
    public const float DEFAULT_SNAPBACK_TIME = 0.3f;

    /// <summary>Default maximum pan distance from follow target.</summary>
    public const float DEFAULT_MAX_PAN_DISTANCE = 15f;

    #endregion

    #region Configuration

    /// <summary>
    /// Y-axis rotation in degrees (0 = north, 45 = diamond grid orientation).
    /// </summary>
    public float Yaw { get; set; } = DEFAULT_YAW;

    /// <summary>
    /// X-axis tilt in degrees (45 = classic isometric, 30 = more top-down).
    /// </summary>
    public float Pitch { get; set; } = DEFAULT_PITCH;

    /// <summary>
    /// Camera distance from the look-at point along the view axis.
    /// </summary>
    public float Distance { get; set; } = DEFAULT_DISTANCE;

    /// <summary>
    /// Minimum orthographic size (zoomed in).
    /// </summary>
    public float ZoomMin { get; set; } = DEFAULT_ZOOM_MIN;

    /// <summary>
    /// Maximum orthographic size (zoomed out).
    /// </summary>
    public float ZoomMax { get; set; } = DEFAULT_ZOOM_MAX;

    /// <summary>
    /// Zoom speed multiplier for scroll input.
    /// </summary>
    public float ZoomSpeed { get; set; } = DEFAULT_ZOOM_SPEED;

    /// <summary>
    /// Smooth follow/pan time in seconds.
    /// </summary>
    public float SmoothTime { get; set; } = DEFAULT_SMOOTH_TIME;

    /// <summary>
    /// Time to snap back to follow target after releasing pan.
    /// </summary>
    public float SnapBackTime { get; set; } = DEFAULT_SNAPBACK_TIME;

    /// <summary>
    /// Maximum distance the camera can pan from the follow target.
    /// Set to 0 to disable pan distance limiting.
    /// </summary>
    public float MaxPanDistance { get; set; } = DEFAULT_MAX_PAN_DISTANCE;

    /// <summary>
    /// Near clip plane distance.
    /// </summary>
    public float NearClipPlane { get; set; } = 0.3f;

    /// <summary>
    /// Far clip plane distance.
    /// </summary>
    public float FarClipPlane { get; set; } = 500f;

    #endregion

    #region State

    private Camera? _camera;
    private Transform? _followTarget;
    private Vector3 _followOffset;
    private Vector3 _targetLookAt;
    private Vector3 _currentLookAt;
    private float _targetOrthoSize;
    private float _currentOrthoSize;
    private Vector3 _lookAtVelocity;
    private float _zoomVelocity;
    private bool _isFreeLookEnabled;
    private Vector3 _defaultLookAt;
    private float _defaultOrthoSize;

    // Manual pan state
    private bool _isManualPanning;
    private Vector3 _manualPanOffset;
    private float _snapBackTimer;
    private Vector3? _lastFollowTargetPosition;

    // Bounds clamping
    private Bounds? _panBounds;
    private bool _usePanBounds;

    #endregion

    #region Properties

    /// <summary>
    /// The camera being controlled.
    /// </summary>
    public Camera? Camera => _camera;

    /// <summary>
    /// Current look-at position in world space.
    /// </summary>
    public Vector3 CurrentLookAt => _currentLookAt;

    /// <summary>
    /// Target look-at position (where camera is smoothly moving to).
    /// </summary>
    public Vector3 TargetLookAt => _targetLookAt;

    /// <summary>
    /// Current orthographic size.
    /// </summary>
    public float CurrentOrthoSize => _currentOrthoSize;

    /// <summary>
    /// Target orthographic size (what zoom is smoothly moving to).
    /// </summary>
    public float TargetOrthoSize => _targetOrthoSize;

    /// <summary>
    /// The current follow target, if any.
    /// </summary>
    public Transform? FollowTarget => _followTarget;

    /// <summary>
    /// Whether free look mode is enabled.
    /// </summary>
    public bool IsFreeLookEnabled => _isFreeLookEnabled;

    /// <summary>
    /// Whether the user is currently manually panning.
    /// </summary>
    public bool IsManualPanning => _isManualPanning;

    /// <summary>
    /// Whether the controller has a valid camera reference.
    /// </summary>
    public bool IsInitialized => _camera != null;

    #endregion

    #region Events

    /// <summary>
    /// Fired when the camera finishes moving to a new position.
    /// </summary>
    public event Action? OnMoveComplete;

    /// <summary>
    /// Fired when zoom level changes.
    /// </summary>
    public event Action<float>? OnZoomChanged;

    /// <summary>
    /// Fired when manual panning starts.
    /// </summary>
    public event Action? OnPanStart;

    /// <summary>
    /// Fired when manual panning ends and snap-back begins.
    /// </summary>
    public event Action? OnPanEnd;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new IsoCameraController.
    /// Call <see cref="Initialize(Camera?)"/> to set the camera reference.
    /// </summary>
    public IsoCameraController() {
        _targetOrthoSize = DEFAULT_ORTHO_SIZE;
        _currentOrthoSize = DEFAULT_ORTHO_SIZE;
    }

    /// <summary>
    /// Creates a new IsoCameraController with the specified camera.
    /// </summary>
    /// <param name="camera">The camera to control. If null, uses Camera.main.</param>
    public IsoCameraController(Camera? camera) : this() {
        Initialize(camera);
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the controller with a camera reference.
    /// </summary>
    /// <param name="camera">The camera to control. If null, uses Camera.main.</param>
    public void Initialize(Camera? camera = null) {
        _camera = camera ?? Camera.main;

        if (_camera == null) {
            Debug.LogWarning("IsoCameraController: No camera found!");
            return;
        }

        // Capture initial state
        _currentLookAt = _camera.transform.position + _camera.transform.forward * Distance;
        _targetLookAt = _currentLookAt;
        _currentOrthoSize = _camera.orthographicSize;
        _targetOrthoSize = _currentOrthoSize;

        Debug.Log($"IsoCameraController: Initialized with camera '{_camera.name}'");
    }

    #endregion

    #region Pan Bounds

    /// <summary>
    /// Sets bounds that the camera cannot pan outside of.
    /// Use this to prevent viewing un-rendered tiles.
    /// </summary>
    /// <param name="bounds">World-space bounds to clamp to.</param>
    public void SetPanBounds(Bounds bounds) {
        _panBounds = bounds;
        _usePanBounds = true;
        Debug.Log($"IsoCameraController: Pan bounds set to {bounds.center}, size {bounds.size}");
    }

    /// <summary>
    /// Sets pan bounds based on a center point and radius.
    /// Useful for expedition where we render around the player.
    /// </summary>
    /// <param name="center">Center of the allowed pan area.</param>
    /// <param name="radius">Radius of the allowed pan area.</param>
    public void SetPanBoundsFromRadius(Vector3 center, float radius) {
        var size = new Vector3(radius * 2, 100f, radius * 2);
        _panBounds = new Bounds(center, size);
        _usePanBounds = true;
    }

    /// <summary>
    /// Clears pan bounds, allowing unlimited panning.
    /// </summary>
    public void ClearPanBounds() {
        _panBounds = null;
        _usePanBounds = false;
    }

    /// <summary>
    /// Updates pan bounds centered on a new position (e.g., as player moves).
    /// </summary>
    /// <param name="center">New center for pan bounds.</param>
    public void UpdatePanBoundsCenter(Vector3 center) {
        if (_panBounds.HasValue) {
            _panBounds = new Bounds(center, _panBounds.Value.size);
        }
    }

    #endregion

    #region Manual Pan Control

    /// <summary>
    /// Starts manual panning mode. Call this when right-click is pressed.
    /// While panning, the camera won't auto-follow the target.
    /// </summary>
    public void BeginManualPan() {
        if (_isManualPanning) return;

        _isManualPanning = true;
        _manualPanOffset = Vector3.zero;
        _snapBackTimer = 0f;

        // Store current follow target position for offset calculation
        if (_followTarget != null) {
            _lastFollowTargetPosition = _followTarget.position + _followOffset;
        }

        OnPanStart?.Invoke();
        Debug.Log("IsoCameraController: Manual pan started");
    }

    /// <summary>
    /// Ends manual panning mode. Call this when right-click is released.
    /// The camera will smoothly snap back to the follow target.
    /// </summary>
    public void EndManualPan() {
        if (!_isManualPanning) return;

        _isManualPanning = false;
        _snapBackTimer = SnapBackTime;

        OnPanEnd?.Invoke();
        Debug.Log("IsoCameraController: Manual pan ended, snapping back");
    }

    /// <summary>
    /// Applies a manual pan delta. Call this while right-click is held with mouse movement.
    /// The delta is in world-space units on the ground plane.
    /// </summary>
    /// <param name="worldDelta">Pan delta in world units (XZ plane).</param>
    public void ApplyManualPan(Vector3 worldDelta) {
        if (!_isManualPanning) return;

        _manualPanOffset += worldDelta;

        // Clamp pan offset to max distance from follow target
        if (MaxPanDistance > 0 && _manualPanOffset.magnitude > MaxPanDistance) {
            _manualPanOffset = _manualPanOffset.normalized * MaxPanDistance;
        }
    }

    /// <summary>
    /// Converts screen-space drag delta to world-space pan delta.
    /// </summary>
    /// <param name="screenDelta">Screen-space delta in pixels.</param>
    /// <returns>World-space delta for panning.</returns>
    public Vector3 ScreenDeltaToWorldPan(Vector2 screenDelta) {
        if (_camera == null) return Vector3.zero;

        // Convert screen delta to world delta based on camera orientation
        // For isometric view, we need to account for the camera angle
        float worldUnitsPerPixel = _currentOrthoSize * 2f / Screen.height;

        // Transform screen delta to camera-relative world delta
        // Screen X maps to world XZ based on camera yaw
        float yawRad = Yaw * Mathf.Deg2Rad;
        
        // Right direction in world space (perpendicular to camera forward on XZ plane)
        Vector3 right = new Vector3(Mathf.Cos(yawRad), 0, -Mathf.Sin(yawRad));
        // Forward direction on ground plane
        Vector3 forward = new Vector3(Mathf.Sin(yawRad), 0, Mathf.Cos(yawRad));

        Vector3 worldDelta = 
            right * (-screenDelta.x * worldUnitsPerPixel) +
            forward * (-screenDelta.y * worldUnitsPerPixel);

        return worldDelta;
    }

    #endregion

    #region Core Methods

    /// <summary>
    /// Sets the camera to a classic isometric view centered on a world position.
    /// </summary>
    public void SetIsometricView(Vector3 centerPosition, float worldSize = 20f, bool immediate = true) {
        if (_camera == null) {
            Initialize();
            if (_camera == null) return;
        }

        var rotation = Quaternion.Euler(Pitch, Yaw, 0f);
        Vector3 cameraPosition = centerPosition - rotation * Vector3.forward * Distance;

        _camera.orthographic = true;
        _camera.nearClipPlane = NearClipPlane;
        _camera.farClipPlane = FarClipPlane;

        float orthoSize = Mathf.Clamp(worldSize * 0.5f, ZoomMin, ZoomMax);

        if (immediate) {
            _camera.transform.position = cameraPosition;
            _camera.transform.rotation = rotation;
            _camera.orthographicSize = orthoSize;
            _currentLookAt = centerPosition;
            _targetLookAt = centerPosition;
            _currentOrthoSize = orthoSize;
            _targetOrthoSize = orthoSize;
        } else {
            _targetLookAt = centerPosition;
            _targetOrthoSize = orthoSize;
        }

        _defaultLookAt = centerPosition;
        _defaultOrthoSize = orthoSize;
        _camera.transform.rotation = rotation;

        Debug.Log($"IsoCameraController: Set isometric view - lookAt={centerPosition}, ortho={orthoSize:F1}");
    }

    /// <summary>
    /// Sets up the camera for a village view with fog padding accounted for.
    /// </summary>
    public void SetupVillageView(int villageWidth, int villageHeight, float tileSize = 1f, int fogPadding = 3, bool immediate = true) {
        int totalWidth = villageWidth + fogPadding * 2;
        int totalHeight = villageHeight + fogPadding * 2;
        float worldWidth = totalWidth * tileSize;
        float worldHeight = totalHeight * tileSize;

        var worldCenter = new Vector3(worldWidth / 2f, 0f, worldHeight / 2f);
        float maxDimension = Mathf.Max(worldWidth, worldHeight);

        SetIsometricView(worldCenter, maxDimension, immediate);
    }

    /// <summary>
    /// Sets a transform to follow. The camera will track this target.
    /// </summary>
    public void SetFollowTarget(Transform? target, Vector3 offset = default) {
        _followTarget = target;
        _followOffset = offset;
        _lastFollowTargetPosition = target != null ? target.position + offset : null;

        if (target != null) {
            Debug.Log($"IsoCameraController: Following '{target.name}'");
        } else {
            Debug.Log("IsoCameraController: Stopped following target");
        }
    }

    /// <summary>
    /// Clears the follow target.
    /// </summary>
    public void ClearFollowTarget() {
        SetFollowTarget(null);
    }

    /// <summary>
    /// Adjusts the zoom level (orthographic size).
    /// </summary>
    public void Zoom(float delta) {
        float newOrthoSize = Mathf.Clamp(
            _targetOrthoSize + delta * ZoomSpeed,
            ZoomMin,
            ZoomMax
        );

        // Prevent zooming out so far that we'd see un-rendered tiles
        if (_usePanBounds && _panBounds.HasValue) {
            float maxAllowedOrtho = CalculateMaxZoomForBounds(_panBounds.Value);
            newOrthoSize = Mathf.Min(newOrthoSize, maxAllowedOrtho);
        }

        _targetOrthoSize = newOrthoSize;
    }

    /// <summary>
    /// Sets the zoom level directly.
    /// </summary>
    public void SetZoom(float orthoSize, bool immediate = false) {
        _targetOrthoSize = Mathf.Clamp(orthoSize, ZoomMin, ZoomMax);

        if (immediate && _camera != null) {
            _camera.orthographicSize = _targetOrthoSize;
            _currentOrthoSize = _targetOrthoSize;
        }
    }

    /// <summary>
    /// Pans the camera to look at a world position.
    /// </summary>
    public void PanTo(Vector3 worldPosition, bool immediate = false) {
        _targetLookAt = ClampToBounds(worldPosition);

        if (immediate) {
            _currentLookAt = _targetLookAt;
            ApplyCameraTransform();
        }
    }

    /// <summary>
    /// Pans the camera by a delta amount.
    /// </summary>
    public void Pan(Vector3 delta) {
        _targetLookAt = ClampToBounds(_targetLookAt + delta);
    }

    /// <summary>
    /// Enables or disables free look mode.
    /// </summary>
    public void SetFreeLook(bool enabled) {
        _isFreeLookEnabled = enabled;

        if (enabled) {
            ClearFollowTarget();
        }

        Debug.Log($"IsoCameraController: Free look {(enabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Toggles free look mode.
    /// </summary>
    public void ToggleFreeLook() {
        SetFreeLook(!_isFreeLookEnabled);
    }

    /// <summary>
    /// Resets the camera to the default view.
    /// </summary>
    public void Reset(bool immediate = false) {
        ClearFollowTarget();
        EndManualPan();
        _targetLookAt = _defaultLookAt;
        _targetOrthoSize = _defaultOrthoSize;

        if (immediate) {
            _currentLookAt = _defaultLookAt;
            _currentOrthoSize = _defaultOrthoSize;
            ApplyCameraTransform();
        }

        Debug.Log("IsoCameraController: Reset to default view");
    }

    #endregion

    #region Update

    /// <summary>
    /// Updates the camera controller. Call this every frame.
    /// </summary>
    public void Update(float deltaTime) {
        if (_camera == null) return;

        bool positionChanged = false;
        bool zoomChanged = false;

        // Calculate base follow position
        Vector3 followPosition = _currentLookAt;
        if (_followTarget != null && !_isFreeLookEnabled) {
            followPosition = _followTarget.position + _followOffset;
            _lastFollowTargetPosition = followPosition;

            // Update pan bounds to follow player
            if (_usePanBounds) {
                UpdatePanBoundsCenter(followPosition);
            }
        }

        // Handle manual panning vs auto-follow
        if (_isManualPanning && _lastFollowTargetPosition.HasValue) {
            // During manual pan, target is follow position + manual offset
            _targetLookAt = ClampToBounds(_lastFollowTargetPosition.Value + _manualPanOffset);
        } else if (_snapBackTimer > 0) {
            // Snapping back to follow target
            _snapBackTimer -= deltaTime;
            
            // Smoothly reduce manual offset to zero
            float t = 1f - (_snapBackTimer / SnapBackTime);
            t = Mathf.SmoothStep(0, 1, t); // Ease in-out
            
            if (_lastFollowTargetPosition.HasValue) {
                Vector3 snapTarget = _lastFollowTargetPosition.Value;
                if (_followTarget != null) {
                    snapTarget = _followTarget.position + _followOffset;
                }
                _targetLookAt = Vector3.Lerp(
                    _lastFollowTargetPosition.Value + _manualPanOffset,
                    snapTarget,
                    t
                );
            }
            
            if (_snapBackTimer <= 0) {
                _manualPanOffset = Vector3.zero;
            }
        } else if (_followTarget != null && !_isFreeLookEnabled) {
            // Normal follow mode
            _targetLookAt = followPosition;
        }

        // Smooth look-at position
        if (Vector3.Distance(_currentLookAt, _targetLookAt) > 0.001f) {
            _currentLookAt = Vector3.SmoothDamp(
                _currentLookAt,
                _targetLookAt,
                ref _lookAtVelocity,
                SmoothTime,
                float.MaxValue,
                deltaTime
            );
            positionChanged = true;
        } else if (_lookAtVelocity.sqrMagnitude > 0.001f) {
            _lookAtVelocity = Vector3.zero;
            OnMoveComplete?.Invoke();
        }

        // Smooth zoom
        if (Mathf.Abs(_currentOrthoSize - _targetOrthoSize) > 0.01f) {
            _currentOrthoSize = Mathf.SmoothDamp(
                _currentOrthoSize,
                _targetOrthoSize,
                ref _zoomVelocity,
                SmoothTime * 0.5f,
                float.MaxValue,
                deltaTime
            );
            _camera.orthographicSize = _currentOrthoSize;
            zoomChanged = true;
        } else if (Mathf.Abs(_zoomVelocity) > 0.01f) {
            _zoomVelocity = 0f;
        }

        if (positionChanged) {
            ApplyCameraTransform();
        }

        if (zoomChanged) {
            OnZoomChanged?.Invoke(_currentOrthoSize);
        }
    }

    /// <summary>
    /// Applies the current look-at position to the camera transform.
    /// </summary>
    private void ApplyCameraTransform() {
        if (_camera == null) return;

        var rotation = Quaternion.Euler(Pitch, Yaw, 0f);
        Vector3 cameraPosition = _currentLookAt - rotation * Vector3.forward * Distance;

        _camera.transform.position = cameraPosition;
        _camera.transform.rotation = rotation;
    }

    #endregion

    #region Bounds Clamping

    /// <summary>
    /// Clamps a position to the current pan bounds.
    /// </summary>
    private Vector3 ClampToBounds(Vector3 position) {
        if (!_usePanBounds || !_panBounds.HasValue) {
            return position;
        }

        var bounds = _panBounds.Value;

        // Account for camera view size - we need to shrink bounds by visible area
        float viewRadius = _currentOrthoSize;

        float minX = bounds.min.x + viewRadius;
        float maxX = bounds.max.x - viewRadius;
        float minZ = bounds.min.z + viewRadius;
        float maxZ = bounds.max.z - viewRadius;

        // If bounds are too small, just center
        if (minX >= maxX) {
            position.x = bounds.center.x;
        } else {
            position.x = Mathf.Clamp(position.x, minX, maxX);
        }

        if (minZ >= maxZ) {
            position.z = bounds.center.z;
        } else {
            position.z = Mathf.Clamp(position.z, minZ, maxZ);
        }

        return position;
    }

    /// <summary>
    /// Calculates the maximum zoom level that keeps the view within bounds.
    /// </summary>
    private float CalculateMaxZoomForBounds(Bounds bounds) {
        // The ortho size is half the vertical view
        // Account for isometric angle
        float boundsRadius = Mathf.Min(bounds.extents.x, bounds.extents.z);
        return boundsRadius * 0.9f; // 90% to leave some margin
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Converts a screen position to a world position on the ground plane.
    /// </summary>
    public Vector3? ScreenToGroundPlane(Vector2 screenPosition, float groundY = 0f) {
        if (_camera == null) return null;

        var ray = _camera.ScreenPointToRay(screenPosition);
        var plane = new Plane(Vector3.up, new Vector3(0, groundY, 0));

        if (plane.Raycast(ray, out float distance)) {
            return ray.GetPoint(distance);
        }

        return null;
    }

    /// <summary>
    /// Gets the visible world bounds at the ground plane.
    /// </summary>
    public Bounds GetVisibleBounds(float groundY = 0f) {
        if (_camera == null) return new Bounds();

        var bottomLeft = ScreenToGroundPlane(new Vector2(0, 0), groundY);
        var bottomRight = ScreenToGroundPlane(new Vector2(Screen.width, 0), groundY);
        var topLeft = ScreenToGroundPlane(new Vector2(0, Screen.height), groundY);
        var topRight = ScreenToGroundPlane(new Vector2(Screen.width, Screen.height), groundY);

        if (!bottomLeft.HasValue || !bottomRight.HasValue ||
            !topLeft.HasValue || !topRight.HasValue) {
            return new Bounds(_currentLookAt, Vector3.one * _currentOrthoSize * 2);
        }

        var bounds = new Bounds(bottomLeft.Value, Vector3.zero);
        bounds.Encapsulate(bottomRight.Value);
        bounds.Encapsulate(topLeft.Value);
        bounds.Encapsulate(topRight.Value);

        return bounds;
    }

    /// <summary>
    /// Checks if a world position is visible on screen.
    /// </summary>
    public bool IsVisible(Vector3 worldPosition) {
        if (_camera == null) return false;

        var viewportPoint = _camera.WorldToViewportPoint(worldPosition);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
               viewportPoint.z > 0;
    }

    /// <summary>
    /// Frames a bounds area, adjusting zoom to fit.
    /// </summary>
    public void FrameBounds(Bounds bounds, float padding = 0.1f, bool immediate = false) {
        _targetLookAt = ClampToBounds(bounds.center);

        float maxSize = Mathf.Max(bounds.size.x, bounds.size.z);
        float requiredOrtho = maxSize * (1f + padding) * 0.5f;
        requiredOrtho /= Mathf.Cos(Pitch * Mathf.Deg2Rad);

        _targetOrthoSize = Mathf.Clamp(requiredOrtho, ZoomMin, ZoomMax);

        if (immediate) {
            _currentLookAt = _targetLookAt;
            _currentOrthoSize = _targetOrthoSize;
            ApplyCameraTransform();
            if (_camera != null) {
                _camera.orthographicSize = _currentOrthoSize;
            }
        }
    }

    #endregion

    #region Debug

    /// <summary>
    /// Gets debug information about the current camera state.
    /// </summary>
    public string GetDebugInfo() {
        var camPos = _camera?.transform.position ?? Vector3.zero;
        string panState = _isManualPanning ? "PANNING" : (_snapBackTimer > 0 ? "SNAPPING" : "follow");
        return $"IsoCameraController: " +
               $"Pos={camPos:F1}, " +
               $"LookAt={_currentLookAt:F1}, " +
               $"Ortho={_currentOrthoSize:F1}, " +
               $"Follow={(_followTarget != null ? _followTarget.name : "none")}, " +
               $"Pan={panState}";
    }

    #endregion
}