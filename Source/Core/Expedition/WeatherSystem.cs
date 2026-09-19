namespace RPGGame.Core.Expedition;

/// <summary>
/// Manages weather conditions that affect travel and combat.
/// </summary>
public class WeatherSystem {
	private static readonly Random SharedRandom = new();

	#region Properties

	/// <summary>Current weather condition.</summary>
	public WeatherType CurrentWeather { get; private set; } = WeatherType.Clear;

	/// <summary>Weather intensity (0-1).</summary>
	public float Intensity { get; private set; } = 0.5f;

	/// <summary>Remaining duration of current weather (in-game hours).</summary>
	public float RemainingDuration { get; private set; }

	/// <summary>Whether weather is transitioning to a new state.</summary>
	public bool IsTransitioning { get; private set; }

	/// <summary>The next weather after transition.</summary>
	public WeatherType? NextWeather { get; private set; }

	#endregion

	#region Events

	/// <summary>Fired when weather changes.</summary>
	public event Action<WeatherType, WeatherType>? OnWeatherChanged;

	/// <summary>Fired when weather intensity changes significantly.</summary>
	public event Action<float>? OnIntensityChanged;

	#endregion

	#region Configuration

	private readonly Random k_random;
	private readonly WeatherConfig k_config;

	public class WeatherConfig {
		public float MinDuration { get; init; } = 2f; // hours
		public float MaxDuration { get; init; } = 12f; // hours
		public float TransitionDuration { get; init; } = 0.5f; // hours
		public Dictionary<WeatherType, float> BaseWeights { get; init; } = new() {
			{ WeatherType.Clear, 30f },
			{ WeatherType.Cloudy, 25f },
			{ WeatherType.Overcast, 15f },
			{ WeatherType.LightRain, 12f },
			{ WeatherType.Rain, 8f },
			{ WeatherType.HeavyRain, 4f },
			{ WeatherType.Storm, 2f },
			{ WeatherType.Fog, 4f }
		};
	}

	#endregion

	#region Constructor

	public WeatherSystem(Random? random = null, WeatherConfig? config = null) {
		k_random = random ?? SharedRandom;
		k_config = config ?? new WeatherConfig();

		// Start with random weather
		CurrentWeather = RollNewWeather(WeatherType.Clear);
		RemainingDuration = RollDuration();
		Intensity = RollIntensity();
	}

	#endregion

	#region Update

	/// <summary>
	/// Updates weather based on elapsed time.
	/// </summary>
	/// <param name="hoursElapsed">In-game hours that passed.</param>
	public void Update(float hoursElapsed) {
		if (IsTransitioning) {
			// Handle transition
			RemainingDuration -= hoursElapsed;
			if (RemainingDuration <= 0 && NextWeather.HasValue) {
				var oldWeather = CurrentWeather;
				CurrentWeather = NextWeather.Value;
				NextWeather = null;
				IsTransitioning = false;
				RemainingDuration = RollDuration();
				Intensity = RollIntensity();
				OnWeatherChanged?.Invoke(oldWeather, CurrentWeather);
			}
		}
		else {
			// Count down current weather
			RemainingDuration -= hoursElapsed;
			if (RemainingDuration <= 0) {
				StartTransition();
			}
			else {
				// Vary intensity slightly
				float intensityDelta = (float)(k_random.NextDouble() - 0.5) * 0.1f * hoursElapsed;
				float oldIntensity = Intensity;
				Intensity = Math.Clamp(Intensity + intensityDelta, 0.2f, 1f);
				if (Math.Abs(oldIntensity - Intensity) > 0.2f) {
					OnIntensityChanged?.Invoke(Intensity);
				}
			}
		}
	}

	private void StartTransition() {
		IsTransitioning = true;
		NextWeather = RollNewWeather(CurrentWeather);
		RemainingDuration = k_config.TransitionDuration;
	}

	#endregion

	#region Weather Rolling

	private WeatherType RollNewWeather(WeatherType current) {
		// Weight adjacent weather types more heavily for natural transitions
		var weights = new Dictionary<WeatherType, float>(k_config.BaseWeights);

		// Boost probability of similar weather
		var adjacent = GetAdjacentWeather(current);
		foreach (var w in adjacent) {
			if (weights.ContainsKey(w)) {
				weights[w] *= 2f;
			}
		}

		// Reduce chance of same weather
		if (weights.ContainsKey(current)) {
			weights[current] *= 0.5f;
		}

		// Roll
		float totalWeight = weights.Values.Sum();
		float roll = (float)(k_random.NextDouble() * totalWeight);
		float cumulative = 0;

		foreach (var (weather, weight) in weights) {
			cumulative += weight;
			if (roll <= cumulative) {
				return weather;
			}
		}

		return WeatherType.Clear;
	}

	private List<WeatherType> GetAdjacentWeather(WeatherType current) {
		return current switch {
			WeatherType.Clear => [WeatherType.Cloudy, WeatherType.Fog],
			WeatherType.Cloudy => [WeatherType.Clear, WeatherType.Overcast, WeatherType.LightRain],
			WeatherType.Overcast => [WeatherType.Cloudy, WeatherType.LightRain, WeatherType.Rain],
			WeatherType.LightRain => [WeatherType.Cloudy, WeatherType.Overcast, WeatherType.Rain],
			WeatherType.Rain => [WeatherType.LightRain, WeatherType.HeavyRain, WeatherType.Overcast],
			WeatherType.HeavyRain => [WeatherType.Rain, WeatherType.Storm],
			WeatherType.Storm => [WeatherType.HeavyRain, WeatherType.Rain],
			WeatherType.Fog => [WeatherType.Clear, WeatherType.Cloudy],
			WeatherType.Snow => [WeatherType.Cloudy, WeatherType.Blizzard],
			WeatherType.Blizzard => [WeatherType.Snow],
			_ => [WeatherType.Clear]
		};
	}

	private float RollDuration() {
		return k_config.MinDuration + (float)(k_random.NextDouble() * (k_config.MaxDuration - k_config.MinDuration));
	}

	private float RollIntensity() {
		return 0.3f + (float)(k_random.NextDouble() * 0.7f);
	}

	#endregion

	#region Effects

	/// <summary>
	/// Gets the current weather effects.
	/// </summary>
	public WeatherEffects GetEffects() {
		return CurrentWeather.GetEffects(Intensity);
	}

	/// <summary>
	/// Force sets the weather (for story events).
	/// </summary>
	public void ForceWeather(WeatherType weather, float duration, float intensity = 0.5f) {
		var oldWeather = CurrentWeather;
		CurrentWeather = weather;
		RemainingDuration = duration;
		Intensity = intensity;
		IsTransitioning = false;
		NextWeather = null;
		OnWeatherChanged?.Invoke(oldWeather, weather);
	}

	#endregion

	#region Serialization

	public WeatherData ToData() {
		return new WeatherData {
			CurrentWeather = CurrentWeather,
			Intensity = Intensity,
			RemainingDuration = RemainingDuration,
			IsTransitioning = IsTransitioning,
			NextWeather = NextWeather
		};
	}

	public static WeatherSystem FromData(WeatherData data, Random? random = null) {
		var system = new WeatherSystem(random) {
			CurrentWeather = data.CurrentWeather,
			Intensity = data.Intensity,
			RemainingDuration = data.RemainingDuration,
			IsTransitioning = data.IsTransitioning,
			NextWeather = data.NextWeather
		};
		return system;
	}

	#endregion
}

#region Weather Types

/// <summary>
/// Types of weather conditions.
/// </summary>
public enum WeatherType {
	Clear,
	Cloudy,
	Overcast,
	LightRain,
	Rain,
	HeavyRain,
	Storm,
	Fog,
	Snow,
	Blizzard,
	Hail,
	Wind,
	Sandstorm,
	AshFall,
	MagicStorm
}

/// <summary>
/// Effects applied by weather conditions.
/// </summary>
public class WeatherEffects {
	public float MovementModifier { get; init; } = 1f;
	public float VisibilityModifier { get; init; } = 1f;
	public float MoraleModifier { get; init; }
	public float StaminaDrainModifier { get; init; } = 1f;
	public float EncounterRateModifier { get; init; } = 1f;
	public float RestEffectivenessModifier { get; init; } = 1f;
	public bool PreventsCamping { get; init; }
	public bool CausesDamage { get; init; }
	public DamageType? DamageType { get; init; }
	public int DamagePerHour { get; init; }
	public string Description { get; init; } = "";
}

public static class WeatherTypeExtensions {
	public static WeatherEffects GetEffects(this WeatherType weather, float intensity) {
		return weather switch {
			WeatherType.Clear => new WeatherEffects {
				Description = "Clear skies."
			},

			WeatherType.Cloudy => new WeatherEffects {
				Description = "Cloudy but dry."
			},

			WeatherType.Overcast => new WeatherEffects {
				MoraleModifier = -0.05f * intensity,
				Description = "Gray, overcast skies."
			},

			WeatherType.LightRain => new WeatherEffects {
				MovementModifier = 0.95f,
				VisibilityModifier = 0.9f,
				MoraleModifier = -0.1f * intensity,
				Description = "Light rain falls gently."
			},

			WeatherType.Rain => new WeatherEffects {
				MovementModifier = 0.85f,
				VisibilityModifier = 0.7f,
				MoraleModifier = -0.15f * intensity,
				StaminaDrainModifier = 1.1f,
				RestEffectivenessModifier = 0.9f,
				Description = "Steady rain dampens spirits."
			},

			WeatherType.HeavyRain => new WeatherEffects {
				MovementModifier = 0.7f,
				VisibilityModifier = 0.5f,
				MoraleModifier = -0.25f * intensity,
				StaminaDrainModifier = 1.25f,
				RestEffectivenessModifier = 0.7f,
				EncounterRateModifier = 0.8f,
				Description = "Heavy rain pours down relentlessly."
			},

			WeatherType.Storm => new WeatherEffects {
				MovementModifier = 0.5f,
				VisibilityModifier = 0.3f,
				MoraleModifier = -0.4f * intensity,
				StaminaDrainModifier = 1.5f,
				RestEffectivenessModifier = 0.5f,
				EncounterRateModifier = 0.5f,
				PreventsCamping = true,
				Description = "A violent storm rages!"
			},

			WeatherType.Fog => new WeatherEffects {
				MovementModifier = 0.8f,
				VisibilityModifier = 0.4f,
				MoraleModifier = -0.1f * intensity,
				EncounterRateModifier = 1.2f,
				Description = "Thick fog obscures everything."
			},

			WeatherType.Snow => new WeatherEffects {
				MovementModifier = 0.75f,
				VisibilityModifier = 0.7f,
				StaminaDrainModifier = 1.3f,
				CausesDamage = intensity > 0.7f,
				DamageType = DamageType.Cold,
				DamagePerHour = (int)(2 * intensity),
				Description = "Snow blankets the landscape."
			},

			WeatherType.Blizzard => new WeatherEffects {
				MovementModifier = 0.4f,
				VisibilityModifier = 0.2f,
				MoraleModifier = -0.5f * intensity,
				StaminaDrainModifier = 2f,
				RestEffectivenessModifier = 0.3f,
				PreventsCamping = true,
				CausesDamage = true,
				DamageType = DamageType.Cold,
				DamagePerHour = (int)(5 * intensity),
				Description = "A deadly blizzard howls!"
			},

			WeatherType.Sandstorm => new WeatherEffects {
				MovementModifier = 0.5f,
				VisibilityModifier = 0.2f,
				MoraleModifier = -0.3f * intensity,
				StaminaDrainModifier = 1.5f,
				PreventsCamping = true,
				CausesDamage = true,
				DamageType = DamageType.Physical,
				DamagePerHour = (int)(3 * intensity),
				Description = "Stinging sand whips through the air!"
			},

			WeatherType.MagicStorm => new WeatherEffects {
				MovementModifier = 0.6f,
				VisibilityModifier = 0.5f,
				EncounterRateModifier = 1.5f,
				CausesDamage = true,
				DamageType = DamageType.Arcane,
				DamagePerHour = (int)(4 * intensity),
				Description = "Arcane energy crackles in the air!"
			},

			_ => new WeatherEffects()
		};
	}

	public static string GetIconName(this WeatherType weather) {
		return weather switch {
			WeatherType.Clear => "icon_weather_clear",
			WeatherType.Cloudy => "icon_weather_cloudy",
			WeatherType.Overcast => "icon_weather_overcast",
			WeatherType.LightRain => "icon_weather_light_rain",
			WeatherType.Rain => "icon_weather_rain",
			WeatherType.HeavyRain => "icon_weather_heavy_rain",
			WeatherType.Storm => "icon_weather_storm",
			WeatherType.Fog => "icon_weather_fog",
			WeatherType.Snow => "icon_weather_snow",
			WeatherType.Blizzard => "icon_weather_blizzard",
			WeatherType.Sandstorm => "icon_weather_sandstorm",
			WeatherType.MagicStorm => "icon_weather_magic",
			_ => "icon_weather_unknown"
		};
	}

	public static string GetDisplayName(this WeatherType weather) {
		return weather switch {
			WeatherType.LightRain => "Light Rain",
			WeatherType.HeavyRain => "Heavy Rain",
			WeatherType.MagicStorm => "Arcane Storm",
			_ => weather.ToString()
		};
	}
}

#endregion

#region Serialization Data

public class WeatherData {
	public WeatherType CurrentWeather { get; set; }
	public float Intensity { get; set; }
	public float RemainingDuration { get; set; }
	public bool IsTransitioning { get; set; }
	public WeatherType? NextWeather { get; set; }
}

#endregion