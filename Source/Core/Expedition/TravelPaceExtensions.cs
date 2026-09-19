namespace RPGGame.Core.Expedition;

public static class TravelPaceExtensions {
	extension(TravelPace pace) {
		/// <summary>
		/// Gets the movement speed multiplier for this pace.
		/// 1.0 = normal walking speed. 0 = stationary.
		/// </summary>
		public float GetSpeedMultiplier() {
			return pace switch {
				TravelPace.Wait => 0f, // Stationary
				TravelPace.Sneak => 0.3f,
				TravelPace.Walk => 1.0f,
				TravelPace.Jog => 1.8f,
				TravelPace.Run => 2.5f,
				TravelPace.Sprint => 4.0f,
				_ => 1.0f
			};
		}
		/// <summary>
		/// Gets the stamina cost per tile for this pace.
		/// Negative = recovery. 0 = no change.
		/// </summary>
		public float GetStaminaCostPerTile() {
			return pace switch {
				TravelPace.Wait => -3.0f, // Good recovery while waiting
				TravelPace.Sneak => 0.5f, // Low cost but slow
				TravelPace.Walk => -1.0f, // Negative = recovery
				TravelPace.Jog => 2.0f,
				TravelPace.Run => 5.0f,
				TravelPace.Sprint => 12.0f,
				_ => 0f
			};
		}
		/// <summary>
		/// Gets the noise level for this pace.
		/// Higher noise = more likely to trigger encounters, scare wildlife.
		/// </summary>
		public float GetNoiseLevel() {
			return pace switch {
				TravelPace.Wait => 0.05f, // Very quiet - just standing
				TravelPace.Sneak => 0.1f,
				TravelPace.Walk => 0.4f,
				TravelPace.Jog => 0.7f,
				TravelPace.Run => 1.0f,
				TravelPace.Sprint => 1.5f,
				_ => 0.4f
			};
		}
		/// <summary>
		/// Gets the perception/awareness modifier.
		/// Higher = better chance to notice things, avoid ambushes.
		/// </summary>
		public float GetPerceptionModifier() {
			return pace switch {
				TravelPace.Wait => 2.0f, // Maximum awareness - observing
				TravelPace.Sneak => 1.5f, // Very aware
				TravelPace.Walk => 1.0f,
				TravelPace.Jog => 0.8f,
				TravelPace.Run => 0.5f,
				TravelPace.Sprint => 0.2f, // Tunnel vision
				_ => 1.0f
			};
		}

		/// <summary>
		/// Gets the ambush avoidance modifier.
		/// Higher = harder to ambush. Combines perception with movement pattern.
		/// </summary>
		public float GetAmbushAvoidance() {
			return pace switch {
				TravelPace.Wait => 0.95f, // Very hard to ambush - you're watching
				TravelPace.Sneak => 0.9f, // Hard to ambush - you see them first
				TravelPace.Walk => 0.6f,
				TravelPace.Jog => 0.4f,
				TravelPace.Run => 0.25f,
				TravelPace.Sprint => 0.1f, // Easy to ambush
				_ => 0.5f
			};
		}
		/// <summary>
		/// Gets the fatigue accumulation rate per hour at this pace.
		/// </summary>
		public float GetFatigueRatePerHour() {
			return pace switch {
				TravelPace.Wait => -2.0f, // Negative = recovery while waiting
				TravelPace.Sneak => 0.5f, // Low fatigue - slow and steady
				TravelPace.Walk => 1.0f,
				TravelPace.Jog => 2.0f,
				TravelPace.Run => 4.0f,
				TravelPace.Sprint => 8.0f, // Exhausting
				_ => 1.0f
			};
		}
		/// <summary>
		/// Gets the wildlife scare radius multiplier.
		/// Higher = animals flee from further away.
		/// </summary>
		public float GetWildlifeScareFactor() {
			return pace switch {
				TravelPace.Wait => 0.1f, // Animals may approach
				TravelPace.Sneak => 0.2f, // Can get close to animals
				TravelPace.Walk => 0.5f,
				TravelPace.Jog => 1.0f,
				TravelPace.Run => 1.5f,
				TravelPace.Sprint => 2.5f, // Everything runs away
				_ => 0.5f
			};
		}
		/// <summary>
		/// Gets the display name with icon.
		/// </summary>
		public string GetDisplayName() {
			return pace switch {
				TravelPace.Wait => "⏸ Wait",
				TravelPace.Sneak => "🤫 Sneak",
				TravelPace.Walk => "🚶 Walk",
				TravelPace.Jog => "🏃 Jog",
				TravelPace.Run => "💨 Run",
				TravelPace.Sprint => "⚡ Sprint",
				_ => pace.ToString()
			};
		}
		/// <summary>
		/// Gets short description for tooltips.
		/// </summary>
		public string GetDescription() {
			return pace switch {
				TravelPace.Wait => "Stay in place. Time passes, stamina recovers, fatigue decreases. Can set up camp.",
				TravelPace.Sneak => "Move slowly and quietly. Low stamina cost, high perception, minimal noise.",
				TravelPace.Walk => "Normal travel pace. Stamina recovers while walking.",
				TravelPace.Jog => "Faster travel with moderate stamina cost. Some noise.",
				TravelPace.Run => "Fast travel. High stamina cost and noise. Reduced perception.",
				TravelPace.Sprint => "Maximum speed. Very high stamina cost. Attracts attention.",
				_ => ""
			};
		}
		/// <summary>
		/// Checks if this pace requires stamina to maintain.
		/// </summary>
		public bool RequiresStamina() {
			return pace switch {
				TravelPace.Wait => false, // Waiting doesn't require stamina
				TravelPace.Walk => false, // Walking recovers stamina
				_ => true
			};
		}
		/// <summary>
		/// Gets the minimum stamina required to use this pace.
		/// </summary>
		public float GetMinimumStamina() {
			return pace switch {
				TravelPace.Wait => 0f, // Can always wait
				TravelPace.Sneak => 5f,
				TravelPace.Walk => 0f,
				TravelPace.Jog => 10f,
				TravelPace.Run => 20f,
				TravelPace.Sprint => 30f,
				_ => 0f
			};
		}
		/// <summary>
		/// Whether this pace involves movement.
		/// </summary>
		public bool IsMoving() {
			return pace != TravelPace.Wait;
		}
		/// <summary>
		/// Whether this pace allows setting up camp.
		/// </summary>
		public bool CanSetupCamp() {
			return pace == TravelPace.Wait;
		}
	}
}
