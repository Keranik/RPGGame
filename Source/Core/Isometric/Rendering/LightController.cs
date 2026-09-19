using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RPGGame.Core.Isometric.Rendering;

/// <summary>
/// Pure logic controller for managing isometric world lighting.
/// Integrates with GameTime for day/night cycles and manages dynamic light sources.
/// 
/// <para>
/// Uses URP Light2D for visual rendering while providing logical light calculations
/// via IsoRay for gameplay purposes (visibility, stealth, etc.).
/// </para>
/// 
/// <example>
/// <code>
/// // Setup
/// var lightController = new LightController(gameTime);
/// lightController.SetGlobalLight(globalLight2D);
/// 
/// // Add player torch
/// var torchHandle = lightController.AddLightSource(new IsoLightSource {
///     Position = playerPos,
///     Radius = 5.Tiles(),
///     Color = ColorRPG.BonfireOrange,
///     Intensity = 1.0f,
///     Type = LightSourceType.Torch,
///     OwnerId = "player"
/// });
/// 
/// // Update torch position each frame
/// lightController.UpdateLightPosition(torchHandle, newPlayerPos);
/// 
/// // Check if a tile is illuminated (for gameplay)
/// bool canSee = lightController.IsTileIlluminated(targetPos, out float brightness);
/// </code>
/// </example>
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class LightController : IDisposable {
    #region Constants

    /// <summary>Minimum ambient light during night (prevents pure black).</summary>
    private const float MIN_AMBIENT_LIGHT = 0.15f;

    /// <summary>Maximum ambient light during midday.</summary>
    private const float MAX_AMBIENT_LIGHT = 1.0f;

    /// <summary>Default torch flicker speed.</summary>
    private const float DEFAULT_FLICKER_SPEED = 8f;

    /// <summary>Default torch flicker intensity variation.</summary>
    private const float DEFAULT_FLICKER_AMOUNT = 0.15f;

    #endregion

    #region Ambient Light Presets

    // ═══════════════════════════════════════════════════════════════════════════
    // CACHED AMBIENT COLORS - Avoid allocations during GetAmbientForHour
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Night ambient (22:00 - 4:59) - Dark blue moonlight.</summary>
    private static readonly ColorRPG AmbientNight = ColorRPG.FromHSL(240, 0.4f, 0.2f);

    /// <summary>Early dawn (5:00) - Deep orange horizon.</summary>
    private static readonly ColorRPG AmbientDawn5 = ColorRPG.FromHSL(30, 0.5f, 0.4f);

    /// <summary>Late dawn (6:00) - Warm orange sunrise.</summary>
    private static readonly ColorRPG AmbientDawn6 = ColorRPG.FromHSL(35, 0.4f, 0.6f);

    /// <summary>Early morning (7:00) - Golden morning light.</summary>
    private static readonly ColorRPG AmbientMorning7 = ColorRPG.FromHSL(45, 0.3f, 0.8f);

    /// <summary>Morning (8:00 - 11:59) - Warm daylight.</summary>
    private static readonly ColorRPG AmbientMorning = ColorRPG.FromHSL(45, 0.2f, 0.9f);

    /// <summary>Midday (12:00 - 13:59) - Full bright white.</summary>
    private static readonly ColorRPG AmbientMidday = ColorRPG.White;

    /// <summary>Afternoon (14:00 - 16:59) - Slightly warm daylight.</summary>
    private static readonly ColorRPG AmbientAfternoon = ColorRPG.FromHSL(40, 0.15f, 0.95f);

    /// <summary>Early dusk (17:00) - Orange sunset beginning.</summary>
    private static readonly ColorRPG AmbientDusk17 = ColorRPG.FromHSL(25, 0.5f, 0.6f);

    /// <summary>Late dusk (18:00) - Deep orange/red sunset.</summary>
    private static readonly ColorRPG AmbientDusk18 = ColorRPG.FromHSL(15, 0.6f, 0.5f);

    /// <summary>Early evening (19:00) - Purple twilight.</summary>
    private static readonly ColorRPG AmbientEvening19 = ColorRPG.FromHSL(260, 0.3f, 0.4f);

    /// <summary>Evening (20:00) - Deepening blue.</summary>
    private static readonly ColorRPG AmbientEvening20 = ColorRPG.FromHSL(250, 0.35f, 0.35f);

    /// <summary>Late evening (21:00) - Dark blue approaching night.</summary>
    private static readonly ColorRPG AmbientEvening21 = ColorRPG.FromHSL(245, 0.4f, 0.3f);

    /// <summary>Cached intensity values for each hour to avoid switch overhead.</summary>
    private static readonly float[] HourlyIntensities = {
        MIN_AMBIENT_LIGHT, // 0 - Night
        MIN_AMBIENT_LIGHT, // 1 - Night
        MIN_AMBIENT_LIGHT, // 2 - Night
        MIN_AMBIENT_LIGHT, // 3 - Night
        MIN_AMBIENT_LIGHT, // 4 - Night
        0.3f,              // 5 - Dawn
        0.6f,              // 6 - Dawn
        0.75f,             // 7 - Early Morning
        0.9f,              // 8 - Morning
        0.9f,              // 9 - Morning
        0.9f,              // 10 - Morning
        0.9f,              // 11 - Morning
        MAX_AMBIENT_LIGHT, // 12 - Midday
        MAX_AMBIENT_LIGHT, // 13 - Midday
        0.95f,             // 14 - Afternoon
        0.95f,             // 15 - Afternoon
        0.95f,             // 16 - Afternoon
        0.7f,              // 17 - Dusk
        0.4f,              // 18 - Dusk
        0.3f,              // 19 - Evening
        0.25f,             // 20 - Evening
        0.2f,              // 21 - Evening
        MIN_AMBIENT_LIGHT, // 22 - Night
        MIN_AMBIENT_LIGHT  // 23 - Night
    };

    /// <summary>Cached color values for each hour to avoid allocations.</summary>
    private static readonly ColorRPG[] HourlyColors = {
        AmbientNight,      // 0 - Night
        AmbientNight,      // 1 - Night
        AmbientNight,      // 2 - Night
        AmbientNight,      // 3 - Night
        AmbientNight,      // 4 - Night
        AmbientDawn5,      // 5 - Dawn
        AmbientDawn6,      // 6 - Dawn
        AmbientMorning7,   // 7 - Early Morning
        AmbientMorning,    // 8 - Morning
        AmbientMorning,    // 9 - Morning
        AmbientMorning,    // 10 - Morning
        AmbientMorning,    // 11 - Morning
        AmbientMidday,     // 12 - Midday
        AmbientMidday,     // 13 - Midday
        AmbientAfternoon,  // 14 - Afternoon
        AmbientAfternoon,  // 15 - Afternoon
        AmbientAfternoon,  // 16 - Afternoon
        AmbientDusk17,     // 17 - Dusk
        AmbientDusk18,     // 18 - Dusk
        AmbientEvening19,  // 19 - Evening
        AmbientEvening20,  // 20 - Evening
        AmbientEvening21,  // 21 - Evening
        AmbientNight,      // 22 - Night
        AmbientNight       // 23 - Night
    };

    #endregion

    #region Fields

    private readonly GameTime _gameTime;
    private readonly Dictionary<int, ManagedLight> _activeLights = new();
    private readonly List<int> _lightsToRemove = new();
    
    private Light2D? _globalLight;
    private GameObject? _lightContainer;
    private int _nextLightId = 1;
    
    private float _currentAmbientIntensity = 1.0f;
    private ColorRPG _currentAmbientColor = ColorRPG.White;
    private TimeOfDayPeriod _lastPeriod = TimeOfDayPeriod.Midday;
    
    private bool _isDisposed;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new LightController integrated with GameTime.
    /// </summary>
    /// <param name="gameTime">The game time service for day/night cycle.</param>
    public LightController(GameTime gameTime) {
        _gameTime = gameTime ?? throw new ArgumentNullException(nameof(gameTime));
        
        // Subscribe to time changes
        _gameTime.OnTimeOfDayChanged += OnTimeOfDayChanged;
        _gameTime.OnHourChanged += OnHourChanged;
        
        // Initialize to current time
        UpdateAmbientForTimeOfDay(_gameTime.TimeOfDay);
    }

    #endregion

    #region Properties

    /// <summary>Current ambient light intensity (0-1).</summary>
    public float AmbientIntensity => _currentAmbientIntensity;

    /// <summary>Current ambient light color.</summary>
    public ColorRPG AmbientColor => _currentAmbientColor;

    /// <summary>Number of active dynamic light sources.</summary>
    public int ActiveLightCount => _activeLights.Count;

    /// <summary>Whether it's currently dark enough to need artificial light.</summary>
    public bool NeedsArtificialLight => _currentAmbientIntensity < 0.5f;

    /// <summary>Current time of day period.</summary>
    public TimeOfDayPeriod CurrentPeriod => _gameTime.TimeOfDay;

    #endregion

    #region Initialization

    /// <summary>
    /// Sets the global/ambient light for the scene (typically the "sun").
    /// </summary>
    /// <param name="globalLight">The URP Light2D component for global illumination.</param>
	/// <summary>
	/// Sets the global/ambient light for the scene (typically the "sun").
	/// </summary>
	/// <param name="globalLight">The URP Light2D component for global illumination.</param>
	public void SetGlobalLight(Light2D globalLight) {
		_globalLight = globalLight;
        
		if (_globalLight != null) {
			// Only set type if it's not already global (avoids duplicate warning)
			if (_globalLight.lightType != Light2D.LightType.Global) {
				_globalLight.lightType = Light2D.LightType.Global;
			}
			ApplyAmbientToGlobalLight();
		}
	}

    /// <summary>
    /// Sets the container GameObject for spawned light objects.
    /// </summary>
    /// <param name="container">Parent transform for light GameObjects.</param>
    public void SetLightContainer(GameObject container) {
        _lightContainer = container;
    }

    #endregion

    #region Light Source Management

    /// <summary>
    /// Adds a new dynamic light source to the world.
    /// </summary>
    /// <param name="source">Light source configuration.</param>
    /// <returns>Handle to the light source for updates/removal.</returns>
    public int AddLightSource(IsoLightSource source) {
        int handle = _nextLightId++;
        
        var managed = new ManagedLight {
            Handle = handle,
            Source = source,
            GameObject = CreateLightGameObject(source, handle),
            BaseIntensity = source.Intensity,
            FlickerOffset = UnityEngine.Random.value * Mathf.PI * 2f
        };
        
        _activeLights[handle] = managed;
        
        Debug.Log($"LightController: Added light source {handle} ({source.Type}) at {source.Position}");
        return handle;
    }

    /// <summary>
    /// Updates the position of an existing light source.
    /// </summary>
    /// <param name="handle">Light handle from AddLightSource.</param>
    /// <param name="newPosition">New world position.</param>
    public void UpdateLightPosition(int handle, IsoPos newPosition) {
        if (!_activeLights.TryGetValue(handle, out var managed)) return;
        
        managed.Source = managed.Source with { Position = newPosition };
        
        if (managed.GameObject != null) {
            Vector3 worldPos = newPosition.ToWorldGridCenter(1f.Tiles(), 1f.Tiles());
            worldPos.y += 0.5f; // Slight elevation for torch effect
            managed.GameObject.transform.position = worldPos;
        }
        
        _activeLights[handle] = managed;
    }

    /// <summary>
    /// Updates the intensity of an existing light source.
    /// </summary>
    /// <param name="handle">Light handle.</param>
    /// <param name="intensity">New intensity (0-1).</param>
    public void UpdateLightIntensity(int handle, float intensity) {
        if (!_activeLights.TryGetValue(handle, out var managed)) return;
        
        managed.Source = managed.Source with { Intensity = intensity };
        managed.BaseIntensity = intensity;
        _activeLights[handle] = managed;
    }

    /// <summary>
    /// Removes a dynamic light source.
    /// </summary>
    /// <param name="handle">Light handle to remove.</param>
    public void RemoveLightSource(int handle) {
        if (!_activeLights.TryGetValue(handle, out var managed)) return;
        
        if (managed.GameObject != null) {
            UnityEngine.Object.Destroy(managed.GameObject);
        }
        
        _activeLights.Remove(handle);
        Debug.Log($"LightController: Removed light source {handle}");
    }

    /// <summary>
    /// Removes all light sources owned by a specific entity.
    /// </summary>
    /// <param name="ownerId">Owner identifier (e.g., "player", entity ID).</param>
    public void RemoveLightsByOwner(string ownerId) {
        _lightsToRemove.Clear();
        
        foreach (var (handle, managed) in _activeLights) {
            if (managed.Source.OwnerId == ownerId) {
                _lightsToRemove.Add(handle);
            }
        }
        
        foreach (int handle in _lightsToRemove) {
            RemoveLightSource(handle);
        }
    }

    #endregion

    #region Gameplay Queries (Using IsoRay Logic)

    /// <summary>
    /// Checks if a tile is illuminated by any light source.
    /// Uses IsoRay for logical line-of-sight, not visual shadows.
    /// </summary>
    /// <param name="position">Position to check.</param>
    /// <param name="brightness">Output brightness level (0-1).</param>
    /// <param name="isBlocking">Optional function to check if tiles block light.</param>
    /// <returns>True if the tile receives any illumination.</returns>
    public bool IsTileIlluminated(IsoPos position, out float brightness, 
        Func<IsoPos, bool>? isBlocking = null) {
        
        // Start with ambient light
        brightness = _currentAmbientIntensity;
        
        // Add contribution from each light source
        foreach (var (_, managed) in _activeLights) {
            float contribution = CalculateLightContribution(
                managed.Source, position, isBlocking);
            brightness = Mathf.Max(brightness, contribution);
        }
        
        brightness = Mathf.Clamp01(brightness);
        return brightness > MIN_AMBIENT_LIGHT;
    }

    /// <summary>
    /// Gets the dominant light color at a position (for visual effects).
    /// </summary>
    /// <param name="position">Position to sample.</param>
    /// <returns>Blended color from all contributing lights.</returns>
    public ColorRPG GetLightColorAt(IsoPos position) {
        float totalWeight = _currentAmbientIntensity;
        ColorRPG blendedColor = _currentAmbientColor;
        
        foreach (var (_, managed) in _activeLights) {
            float contribution = CalculateLightContribution(managed.Source, position, null);
            if (contribution > 0.01f) {
                blendedColor = blendedColor.LerpHSL(managed.Source.Color, contribution / (totalWeight + contribution));
                totalWeight += contribution;
            }
        }
        
        return blendedColor;
    }

    /// <summary>
    /// Gets all positions illuminated by a specific light source.
    /// Useful for fog-of-war or visibility calculations.
    /// </summary>
    /// <param name="handle">Light handle.</param>
    /// <param name="isBlocking">Function to check if tiles block light.</param>
    /// <returns>Enumerable of illuminated positions with their brightness.</returns>
    public IEnumerable<(IsoPos Position, float Brightness)> GetIlluminatedTiles(
        int handle, Func<IsoPos, bool>? isBlocking = null) {
        
        if (!_activeLights.TryGetValue(handle, out var managed)) yield break;
        
        var source = managed.Source;
        int radius = source.Radius.ToInt();
        
        for (int dx = -radius; dx <= radius; dx++) {
            for (int dy = -radius; dy <= radius; dy++) {
                var pos = source.Position.Offset(dx, dy);
                float contribution = CalculateLightContribution(source, pos, isBlocking);
                
                if (contribution > 0.01f) {
                    yield return (pos, contribution);
                }
            }
        }
    }

    /// <summary>
    /// Checks line-of-sight for light using IsoRay.
    /// </summary>
    private float CalculateLightContribution(IsoLightSource source, IsoPos target, 
        Func<IsoPos, bool>? isBlocking) {
        
        // Calculate distance
        var distance = source.Position.DistanceTo(target);
        float distTiles = distance.HorizontalTiles.Value;
        float maxDist = source.Radius.Value;
        
        if (distTiles > maxDist) return 0f;
        
        // Check line of sight if blocking function provided
        if (isBlocking != null && distTiles > 0.5f) {
            var ray = IsoRay.HorizontalTo(source.Position, target);
            if (!ray.HasLineOfSightXY(target, isBlocking)) {
                return 0f; // Blocked by obstacle
            }
        }
        
        // Calculate falloff based on light type
        float falloff = source.Type switch {
            LightSourceType.Torch => CalculateTorchFalloff(distTiles, maxDist),
            LightSourceType.Campfire => CalculateCampfireFalloff(distTiles, maxDist),
            LightSourceType.Lantern => CalculateLanternFalloff(distTiles, maxDist),
            LightSourceType.Magic => CalculateMagicFalloff(distTiles, maxDist),
            LightSourceType.Ambient => 1f, // No falloff
            _ => CalculateLinearFalloff(distTiles, maxDist)
        };
        
        return falloff * source.Intensity;
    }

    private static float CalculateTorchFalloff(float dist, float max) {
        // Torch: warm glow with quick falloff
        float t = dist / max;
        return Mathf.Pow(1f - t, 2f);
    }

    private static float CalculateCampfireFalloff(float dist, float max) {
        // Campfire: larger radius, softer edges
        float t = dist / max;
        return Mathf.Pow(1f - t, 1.5f);
    }

    private static float CalculateLanternFalloff(float dist, float max) {
        // Lantern: even light, sharper cutoff
        float t = dist / max;
        return t < 0.7f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f);
    }

    private static float CalculateMagicFalloff(float dist, float max) {
        // Magic: ethereal, pulsing implied elsewhere
        float t = dist / max;
        return 1f - t * t;
    }

    private static float CalculateLinearFalloff(float dist, float max) {
        return 1f - (dist / max);
    }

    #endregion

    #region Update Loop

	/// <summary>
	/// Updates all dynamic lights and ambient transitions.
	/// Called from GameLoop.Tick().
	/// </summary>
	/// <param name="deltaTime">Time since last update.</param>
	public void Update(float deltaTime) {
		// Smooth ambient lighting transitions based on current GameTime
		UpdateAmbientSmooth(deltaTime);
        
		// Update dynamic light effects (flicker, etc.)
		foreach (var (handle, managed) in _activeLights) {
			UpdateManagedLight(managed, deltaTime);
		}
	}


    private void UpdateManagedLight(ManagedLight managed, float deltaTime) {
        if (managed.GameObject == null) return;
        
        var light2D = managed.GameObject.GetComponent<Light2D>();
        if (light2D == null) return;
        
        // Apply flicker effect for appropriate light types
        float intensity = managed.BaseIntensity;
        
        if (managed.Source.Flickers) {
            float flickerSpeed = managed.Source.FlickerSpeed > 0 
                ? managed.Source.FlickerSpeed 
                : DEFAULT_FLICKER_SPEED;
            float flickerAmount = managed.Source.FlickerAmount > 0 
                ? managed.Source.FlickerAmount 
                : DEFAULT_FLICKER_AMOUNT;
            
            // Multi-frequency flicker for natural look
            float time = Time.time + managed.FlickerOffset;
            float flicker = Mathf.Sin(time * flickerSpeed) * 0.5f +
                           Mathf.Sin(time * flickerSpeed * 2.3f) * 0.3f +
                           Mathf.Sin(time * flickerSpeed * 5.7f) * 0.2f;
            
            intensity *= 1f + flicker * flickerAmount;
        }
        
        // Scale intensity based on ambient (torches more visible at night)
        if (_currentAmbientIntensity < 0.5f && managed.Source.Type != LightSourceType.Ambient) {
            float nightBoost = Mathf.Lerp(1f, 1.5f, 1f - _currentAmbientIntensity * 2f);
            intensity *= nightBoost;
        }
        
        light2D.intensity = Mathf.Clamp(intensity, 0f, 2f);
    }

    #endregion

    #region Time of Day

	private void OnTimeOfDayChanged(TimeOfDayPeriod period) {
		_lastPeriod = period;
		Debug.Log($"LightController: Time of day changed to {period}");
	}

	private void OnHourChanged(int hour) {
		Debug.Log($"LightController: Hour changed to {hour}:00");
	}

	/// <summary>
	/// Smoothly interpolates ambient lighting toward the target for current time.
	/// </summary>
	/// <param name="deltaTime">Time since last update (passed from GameLoop).</param>
	private void UpdateAmbientSmooth(float deltaTime) {
		var (targetIntensity, targetColor) = GetTargetAmbientForTime();
        
		const float LERP_SPEED = 2f;
        
		_currentAmbientIntensity = Mathf.Lerp(_currentAmbientIntensity, targetIntensity, deltaTime * LERP_SPEED);
		_currentAmbientColor = _currentAmbientColor.LerpHSL(targetColor, deltaTime * LERP_SPEED);
        
		ApplyAmbientToGlobalLight();
	}

	// <summary>
	/// Gets the target ambient values based on exact current time (hour + minute).
	/// Interpolates between hour boundaries for smooth transitions.
	/// </summary>
	private (float intensity, ColorRPG color) GetTargetAmbientForTime() {
		int hour = _gameTime.Hour;
		int minute = _gameTime.Minute;
		float hourFraction = minute / (float)GameTime.MINUTES_PER_HOUR;
        
		// Get current and next hour targets
		var currentTarget = GetAmbientForHour(hour);
		var nextTarget = GetAmbientForHour((hour + 1) % GameTime.HOURS_PER_DAY);
        
		// Interpolate between current hour's target and next hour's target
		float intensity = Mathf.Lerp(currentTarget.intensity, nextTarget.intensity, hourFraction);
		ColorRPG color = currentTarget.color.LerpHSL(nextTarget.color, hourFraction);
        
		return (intensity, color);
	}

	/// <summary>
	/// Gets the ambient target for a specific hour (0-23).
	/// Uses pre-cached colors and intensities for zero allocations.
	/// </summary>
	private static (float intensity, ColorRPG color) GetAmbientForHour(int hour) {
		int index = Math.Clamp(hour, 0, 23);
		return (HourlyIntensities[index], HourlyColors[index]);
	}

	private void UpdateAmbientForTimeOfDay(TimeOfDayPeriod period) {
		// This is now just for initialization - smooth update handles transitions
		var (intensity, color) = GetTargetAmbientForTime();
		_currentAmbientIntensity = intensity;
		_currentAmbientColor = color;
		ApplyAmbientToGlobalLight();
        
		Debug.Log($"LightController: Initialized ambient for {period}, intensity = {intensity:P0}");
	}

    private void UpdateAmbientForHour(int hour) {
        // Fine-tune ambient based on exact hour for smoother transitions
        float hourFraction = _gameTime.Minute / 60f;
        
        // This could lerp between period values for even smoother transitions
        // For now, just update the global light
        ApplyAmbientToGlobalLight();
    }

    private void ApplyAmbientToGlobalLight() {
        if (_globalLight == null) return;
        
        _globalLight.intensity = _currentAmbientIntensity;
        _globalLight.color = _currentAmbientColor;
    }

    #endregion

    #region Light GameObject Creation

    private GameObject CreateLightGameObject(IsoLightSource source, int handle) {
        var go = new GameObject($"Light_{source.Type}_{handle}");
        
        if (_lightContainer != null) {
            go.transform.SetParent(_lightContainer.transform);
        }
        
        Vector3 worldPos = source.Position.ToWorldGridCenter(1f.Tiles(), 1f.Tiles());
        worldPos.y += GetLightElevation(source.Type);
        go.transform.position = worldPos;
        
        var light2D = go.AddComponent<Light2D>();
        light2D.lightType = Light2D.LightType.Point;
        light2D.color = source.Color;
        light2D.intensity = source.Intensity;
        light2D.pointLightOuterRadius = source.Radius.Value;
        light2D.pointLightInnerRadius = source.Radius.Value * 0.3f;
        light2D.falloffIntensity = GetFalloffForType(source.Type);
        
        // Optional: Add shadow casting
        if (source.CastsShadows) {
            light2D.shadowsEnabled = true;
            light2D.shadowIntensity = 0.7f;
        }
        
        return go;
    }

    private static float GetLightElevation(LightSourceType type) {
        return type switch {
            LightSourceType.Torch => 0.8f,
            LightSourceType.Campfire => 0.3f,
            LightSourceType.Lantern => 1.0f,
            LightSourceType.Magic => 0.5f,
            _ => 0.5f
        };
    }

    private static float GetFalloffForType(LightSourceType type) {
        return type switch {
            LightSourceType.Torch => 0.6f,
            LightSourceType.Campfire => 0.4f,
            LightSourceType.Lantern => 0.8f,
            LightSourceType.Magic => 0.3f,
            _ => 0.5f
        };
    }

    #endregion

    #region Debug

    /// <summary>
    /// Gets debug information about current lighting state.
    /// </summary>
    public string GetDebugInfo() {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== LightController Debug ===");
        sb.AppendLine($"Time of Day: {CurrentPeriod}");
        sb.AppendLine($"Ambient Intensity: {_currentAmbientIntensity:P0}");
        sb.AppendLine($"Ambient Color: {_currentAmbientColor:Hex}");
        sb.AppendLine($"Needs Artificial Light: {NeedsArtificialLight}");
        sb.AppendLine($"Active Light Sources: {_activeLights.Count}");
        
        foreach (var (handle, managed) in _activeLights) {
            sb.AppendLine($"  [{handle}] {managed.Source.Type} at {managed.Source.Position} " +
                         $"(r={managed.Source.Radius}, i={managed.BaseIntensity:F2})");
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Draws debug gizmos for all light sources.
    /// </summary>
    public void DrawDebugGizmos(float tileSize = 1f) {
#if UNITY_EDITOR
        foreach (var (_, managed) in _activeLights) {
            var source = managed.Source;
            Vector3 worldPos = source.Position.ToWorldGridCenter(tileSize.Tiles(), 1f.Tiles());
            
            Gizmos.color = source.Color.WithAlpha(0.3f);
            Gizmos.DrawSphere(worldPos, 0.3f);
            
            Gizmos.color = source.Color.WithAlpha(0.1f);
            Gizmos.DrawWireSphere(worldPos, source.Radius.Value * tileSize);
        }
#endif
    }

    #endregion

    #region IDisposable

    public void Dispose() {
        if (_isDisposed) return;
        
        _gameTime.OnTimeOfDayChanged -= OnTimeOfDayChanged;
        _gameTime.OnHourChanged -= OnHourChanged;
        
        foreach (var (_, managed) in _activeLights) {
            if (managed.GameObject != null) {
                UnityEngine.Object.Destroy(managed.GameObject);
            }
        }
        _activeLights.Clear();
        
        _isDisposed = true;
    }

    #endregion

    #region Nested Types

    private struct ManagedLight {
        public int Handle;
        public IsoLightSource Source;
        public GameObject? GameObject;
        public float BaseIntensity;
        public float FlickerOffset;
    }

    #endregion
}

#region IsoLightSource

/// <summary>
/// Configuration for a dynamic light source in the isometric world.
/// </summary>
public readonly record struct IsoLightSource {
    /// <summary>Position of the light source.</summary>
    public IsoPos Position { get; init; }
    
    /// <summary>Light radius in tiles.</summary>
    public TilesRPG Radius { get; init; }
    
    /// <summary>Light color.</summary>
    public ColorRPG Color { get; init; }
    
    /// <summary>Light intensity (0-1, can exceed for bloom).</summary>
    public float Intensity { get; init; }
    
    /// <summary>Type of light source (affects falloff and behavior).</summary>
    public LightSourceType Type { get; init; }
    
    /// <summary>Owner entity ID (for cleanup when entity is removed).</summary>
    public string OwnerId { get; init; }
    
    /// <summary>Whether this light flickers.</summary>
    public bool Flickers { get; init; }
    
    /// <summary>Flicker speed (if flickering).</summary>
    public float FlickerSpeed { get; init; }
    
    /// <summary>Flicker intensity variation (if flickering).</summary>
    public float FlickerAmount { get; init; }
    
    /// <summary>Whether this light casts shadows.</summary>
    public bool CastsShadows { get; init; }
    
    /// <summary>Creates a default torch light source.</summary>
    public static IsoLightSource Torch(IsoPos position, string ownerId) => new() {
        Position = position,
        Radius = 5.Tiles(),
        Color = ColorRPG.BonfireOrange,
        Intensity = 0.9f,
        Type = LightSourceType.Torch,
        OwnerId = ownerId,
        Flickers = true,
        FlickerSpeed = 8f,
        FlickerAmount = 0.15f,
        CastsShadows = true
    };
    
    /// <summary>Creates a campfire light source.</summary>
    public static IsoLightSource Campfire(IsoPos position, string ownerId) => new() {
        Position = position,
        Radius = 8.Tiles(),
        Color = ColorRPG.Fire.LerpHSL(ColorRPG.Gold, 0.3f),
        Intensity = 1.2f,
        Type = LightSourceType.Campfire,
        OwnerId = ownerId,
        Flickers = true,
        FlickerSpeed = 5f,
        FlickerAmount = 0.2f,
        CastsShadows = true
    };
    
    /// <summary>Creates a lantern light source.</summary>
    public static IsoLightSource Lantern(IsoPos position, string ownerId) => new() {
        Position = position,
        Radius = 6.Tiles(),
        Color = ColorRPG.Gold.Lighten(0.2f),
        Intensity = 0.8f,
        Type = LightSourceType.Lantern,
        OwnerId = ownerId,
        Flickers = false,
        CastsShadows = true
    };
    
    /// <summary>Creates a magical light source.</summary>
    public static IsoLightSource MagicLight(IsoPos position, string ownerId, ColorRPG color) => new() {
        Position = position,
        Radius = 4.Tiles(),
        Color = color,
        Intensity = 0.7f,
        Type = LightSourceType.Magic,
        OwnerId = ownerId,
        Flickers = true,
        FlickerSpeed = 3f,
        FlickerAmount = 0.1f,
        CastsShadows = false
    };
}

/// <summary>
/// Types of light sources with different visual and gameplay behaviors.
/// </summary>
public enum LightSourceType {
    /// <summary>Handheld torch - warm, flickering, medium radius.</summary>
    Torch,
    
    /// <summary>Campfire - large radius, strong flicker.</summary>
    Campfire,
    
    /// <summary>Lantern - steady light, good radius.</summary>
    Lantern,
    
    /// <summary>Magical light - ethereal glow, customizable color.</summary>
    Magic,
    
    /// <summary>Ambient/environmental - no falloff.</summary>
    Ambient,
    
    /// <summary>Generic point light.</summary>
    Point
}

#endregion