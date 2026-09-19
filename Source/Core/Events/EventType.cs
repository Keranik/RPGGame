namespace RPGGame.Core.Events;

/// <summary>
/// Categories of events that can occur during expeditions.
/// </summary>
public enum EventType {
	/// <summary>Combat encounter with enemies.</summary>
	Combat,
	/// <summary>Resource discovery (loot, items, gold).</summary>
	Resource,
	/// <summary>NPC encounter (friendly, neutral, hostile).</summary>
	Encounter,
	/// <summary>Player choice with multiple outcomes.</summary>
	Choice,
	/// <summary>Story/lore event.</summary>
	Story,
	/// <summary>Environmental hazard or challenge.</summary>
	Environmental,
	/// <summary>Discovery of secret or hidden area.</summary>
	Discovery,
	/// <summary>Skill check challenge.</summary>
	SkillCheck,
	/// <summary>Shop or trading opportunity.</summary>
	Trade,
	/// <summary>Rest or recovery opportunity.</summary>
	Rest,
	/// <summary>Trap or ambush.</summary>
	Trap,
	/// <summary>Boss encounter.</summary>
	Boss
}

/// <summary>
/// Rarity/importance of an event.
/// </summary>
public enum EventRarity {
	/// <summary>Common event, happens frequently.</summary>
	Common,
	/// <summary>Uncommon event, moderate frequency.</summary>
	Uncommon,
	/// <summary>Rare event, infrequent.</summary>
	Rare,
	/// <summary>Epic event, very rare.</summary>
	Epic,
	/// <summary>Legendary event, extremely rare.</summary>
	Legendary,
	/// <summary>Unique event, can only happen once per run.</summary>
	Unique
}

/// <summary>
/// Extension methods for EventType.
/// </summary>
public static class EventTypeExtensions {
	/// <summary>
	/// Gets the icon name for this event type.
	/// </summary>
	public static string GetIconName(this EventType type) {
		return type switch {
			EventType.Combat => "icon_swords",
			EventType.Resource => "icon_chest",
			EventType.Encounter => "icon_person",
			EventType.Choice => "icon_question",
			EventType.Story => "icon_book",
			EventType.Environmental => "icon_warning",
			EventType.Discovery => "icon_star",
			EventType.SkillCheck => "icon_dice",
			EventType.Trade => "icon_coins",
			EventType.Rest => "icon_campfire",
			EventType.Trap => "icon_skull",
			EventType.Boss => "icon_crown",
			_ => "icon_event"
		};
	}

	/// <summary>
	/// Gets the display name for this event type.
	/// </summary>
	public static string GetDisplayName(this EventType type) {
		return type switch {
			EventType.SkillCheck => "Skill Check",
			_ => type.ToString()
		};
	}

	/// <summary>
	/// Gets whether this event type pauses travel.
	/// </summary>
	public static bool PausesTravel(this EventType type) {
		return type switch {
			EventType.Resource => false, // Can auto-collect
			EventType.Rest => false,
			_ => true
		};
	}

	/// <summary>
	/// Gets base spawn weight for this event type.
	/// </summary>
	public static float GetBaseWeight(this EventType type) {
		return type switch {
			EventType.Combat => 25f,
			EventType.Resource => 20f,
			EventType.Encounter => 15f,
			EventType.Choice => 10f,
			EventType.Environmental => 10f,
			EventType.SkillCheck => 8f,
			EventType.Discovery => 5f,
			EventType.Story => 4f,
			EventType.Trade => 2f,
			EventType.Trap => 5f,
			EventType.Boss => 0f, // Boss events are placed, not random
			EventType.Rest => 3f,
			_ => 10f
		};
	}
}

/// <summary>
/// Extension methods for EventRarity.
/// </summary>
public static class EventRarityExtensions {
	/// <summary>
	/// Gets spawn weight multiplier for this rarity.
	/// </summary>
	public static float GetWeightMultiplier(this EventRarity rarity) {
		return rarity switch {
			EventRarity.Common => 1f,
			EventRarity.Uncommon => 0.5f,
			EventRarity.Rare => 0.2f,
			EventRarity.Epic => 0.05f,
			EventRarity.Legendary => 0.01f,
			EventRarity.Unique => 0f, // Unique events are placed specifically
			_ => 1f
		};
	}

	/// <summary>
	/// Gets the color for this rarity.
	/// </summary>
	public static UnityEngine.Color GetColor(this EventRarity rarity) {
		return rarity switch {
			EventRarity.Common => new UnityEngine.Color(0.8f, 0.8f, 0.8f),
			EventRarity.Uncommon => new UnityEngine.Color(0.3f, 0.8f, 0.3f),
			EventRarity.Rare => new UnityEngine.Color(0.3f, 0.5f, 1f),
			EventRarity.Epic => new UnityEngine.Color(0.7f, 0.3f, 0.9f),
			EventRarity.Legendary => new UnityEngine.Color(1f, 0.6f, 0f),
			EventRarity.Unique => new UnityEngine.Color(1f, 0.2f, 0.2f),
			_ => UnityEngine.Color.white
		};
	}
}