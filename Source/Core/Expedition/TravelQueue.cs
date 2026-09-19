using UnityEngine;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Manages a queue of planned stops during travel.
/// Players can mark tiles to stop at for various reasons.
/// </summary>
public class TravelQueue {
	#region Fields

	private readonly List<QueuedStop> k_stops = [];

	#endregion

	#region Properties

	/// <summary>
	/// All queued stops.
	/// </summary>
	public IReadOnlyList<QueuedStop> Stops => k_stops;

	/// <summary>
	/// Number of stops in queue.
	/// </summary>
	public int Count => k_stops.Count;

	/// <summary>
	/// Whether there are any queued stops.
	/// </summary>
	public bool HasStops => k_stops.Count > 0;

	/// <summary>
	/// The next stop in the queue (if any).
	/// </summary>
	public QueuedStop? NextStop => k_stops.Count > 0 ? k_stops[0] : null;

	#endregion

	#region Events

	/// <summary>Fired when a stop is added to the queue.</summary>
	public event Action<QueuedStop>? OnStopAdded;

	/// <summary>Fired when a stop is removed from the queue.</summary>
	public event Action<QueuedStop>? OnStopRemoved;

	/// <summary>Fired when a stop is reached.</summary>
	public event Action<QueuedStop>? OnStopReached;

	/// <summary>Fired when the queue is cleared.</summary>
	public event Action? OnQueueCleared;

	#endregion

	#region Queue Management

	/// <summary>
	/// Adds a stop to the end of the queue.
	/// </summary>
	public void AddStop(Vector2Int tile, StopReason reason, string? interactionId = null, bool autoPause = true) {
		var stop = new QueuedStop {
			Tile = tile,
			Reason = reason,
			InteractionId = interactionId,
			AutoPause = autoPause,
			QueuedAt = DateTime.Now
		};

		k_stops.Add(stop);
		OnStopAdded?.Invoke(stop);
	}

	/// <summary>
	/// Adds a stop for camping.
	/// </summary>
	public void AddCampStop(Vector2Int tile) {
		AddStop(tile, StopReason.Camp, null, true);
	}

	/// <summary>
	/// Adds a stop for an event interaction.
	/// </summary>
	public void AddEventStop(Vector2Int tile, string eventId) {
		AddStop(tile, StopReason.Event, eventId, true);
	}

	/// <summary>
	/// Adds a stop for a resource node.
	/// </summary>
	public void AddResourceStop(Vector2Int tile, string resourceId) {
		AddStop(tile, StopReason.Resource, resourceId, true);
	}

	/// <summary>
	/// Adds a custom stop (player just wants to pause here).
	/// </summary>
	public void AddCustomStop(Vector2Int tile, bool autoPause = false) {
		// Custom stops respect the "auto-pause at empty locations" setting
		AddStop(tile, StopReason.Custom, null, autoPause);
	}

	/// <summary>
	/// Removes a specific stop from the queue.
	/// </summary>
	public bool RemoveStop(QueuedStop stop) {
		if (k_stops.Remove(stop)) {
			OnStopRemoved?.Invoke(stop);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Removes a stop at a specific tile.
	/// </summary>
	public bool RemoveStopAt(Vector2Int tile) {
		var stop = k_stops.FirstOrDefault(s => s.Tile == tile);
		if (stop != null) {
			return RemoveStop(stop);
		}
		return false;
	}

	/// <summary>
	/// Removes and returns the next stop (when reached).
	/// </summary>
	public QueuedStop? PopNextStop() {
		if (k_stops.Count == 0) {
			return null;
		}

		var stop = k_stops[0];
		k_stops.RemoveAt(0);
		OnStopReached?.Invoke(stop);
		return stop;
	}

	/// <summary>
	/// Clears all queued stops.
	/// </summary>
	public void Clear() {
		k_stops.Clear();
		OnQueueCleared?.Invoke();
	}

	/// <summary>
	/// Checks if there's a stop at a specific tile.
	/// </summary>
	public bool HasStopAt(Vector2Int tile) {
		return k_stops.Any(s => s.Tile == tile);
	}

	/// <summary>
	/// Gets the stop at a specific tile (if any).
	/// </summary>
	public QueuedStop? GetStopAt(Vector2Int tile) {
		return k_stops.FirstOrDefault(s => s.Tile == tile);
	}

	/// <summary>
	/// Checks if the given tile is the next stop.
	/// </summary>
	public bool IsNextStop(Vector2Int tile) {
		return NextStop?.Tile == tile;
	}

	/// <summary>
	/// Reorders a stop to a new position in the queue.
	/// </summary>
	public void ReorderStop(int fromIndex, int toIndex) {
		if (fromIndex < 0 || fromIndex >= k_stops.Count) return;
		if (toIndex < 0 || toIndex >= k_stops.Count) return;

		var stop = k_stops[fromIndex];
		k_stops.RemoveAt(fromIndex);
		k_stops.Insert(toIndex, stop);
	}

	/// <summary>
	/// Checks if the current tile has a queued stop and returns it if so.
	/// This is a convenience method that combines HasStopAt + GetStopAt + PopNextStop.
	/// </summary>
	public QueuedStop? CheckAndConsumeStopAtTile(Vector2Int tile) {
		if (!IsNextStop(tile)) {
			return null;
		}
		return PopNextStop();
	}

	#endregion

	#region Serialization

	public TravelQueueData ToData() {
		return new TravelQueueData {
			Stops = k_stops.Select(s => s.ToData()).ToList()
		};
	}

	public static TravelQueue FromData(TravelQueueData data) {
		var queue = new TravelQueue();
		foreach (var stopData in data.Stops) {
			queue.k_stops.Add(QueuedStop.FromData(stopData));
		}
		return queue;
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// A queued stop in the travel queue.
/// </summary>
public class QueuedStop {
	/// <summary>Tile position to stop at.</summary>
	public Vector2Int Tile { get; set; }

	/// <summary>Reason for stopping.</summary>
	public StopReason Reason { get; set; }

	/// <summary>Related ID (event, resource, etc.).</summary>
	public string? InteractionId { get; set; }

	/// <summary>Whether to auto-pause simulation when reached.</summary>
	public bool AutoPause { get; set; } = true;

	/// <summary>When this stop was queued.</summary>
	public DateTime QueuedAt { get; set; }

	/// <summary>Optional notes from player.</summary>
	public string? Notes { get; set; }

	public QueuedStopData ToData() {
		return new QueuedStopData {
			TileX = Tile.x,
			TileY = Tile.y,
			Reason = Reason,
			InteractionId = InteractionId,
			AutoPause = AutoPause,
			Notes = Notes
		};
	}

	public static QueuedStop FromData(QueuedStopData data) {
		return new QueuedStop {
			Tile = new Vector2Int(data.TileX, data.TileY),
			Reason = data.Reason,
			InteractionId = data.InteractionId,
			AutoPause = data.AutoPause,
			Notes = data.Notes,
			QueuedAt = DateTime.Now
		};
	}
}

/// <summary>
/// Reason for a queued stop.
/// </summary>
public enum StopReason {
	/// <summary>Player just wants to stop here.</summary>
	Custom,

	/// <summary>Setting up camp.</summary>
	Camp,

	/// <summary>Event to interact with.</summary>
	Event,

	/// <summary>Resource node to gather.</summary>
	Resource,

	/// <summary>POI/location to enter.</summary>
	Location,

	/// <summary>Path branch decision point.</summary>
	BranchPoint,

	/// <summary>Waiting for something (time to pass, enemy to leave).</summary>
	Wait,

	/// <summary>Save point.</summary>
	Save
}

public static class StopReasonExtensions {
	public static string GetDisplayName(this StopReason reason) {
		return reason switch {
			StopReason.Custom => "Stop",
			StopReason.Camp => "Camp",
			StopReason.Event => "Event",
			StopReason.Resource => "Gather",
			StopReason.Location => "Enter",
			StopReason.BranchPoint => "Decision",
			StopReason.Wait => "Wait",
			StopReason.Save => "Save",
			_ => reason.ToString()
		};
	}

	public static string GetIcon(this StopReason reason) {
		return reason switch {
			StopReason.Custom => "📍",
			StopReason.Camp => "🏕",
			StopReason.Event => "❗",
			StopReason.Resource => "⛏",
			StopReason.Location => "🚪",
			StopReason.BranchPoint => "🔀",
			StopReason.Wait => "⏳",
			StopReason.Save => "💾",
			_ => "📍"
		};
	}

	/// <summary>
	/// Whether this stop type should always pause by default.
	/// </summary>
	public static bool DefaultAutoPause(this StopReason reason) {
		return reason switch {
			StopReason.Custom => false,  // Respects settings
			StopReason.Camp => true,
			StopReason.Event => true,
			StopReason.Resource => true,
			StopReason.Location => true,
			StopReason.BranchPoint => true,
			StopReason.Wait => false,    // Intentionally waiting
			StopReason.Save => true,
			_ => true
		};
	}
}

#endregion

#region Serialization Data

public class TravelQueueData {
	public List<QueuedStopData> Stops { get; set; } = [];
}

public class QueuedStopData {
	public int TileX { get; set; }
	public int TileY { get; set; }
	public StopReason Reason { get; set; }
	public string? InteractionId { get; set; }
	public bool AutoPause { get; set; }
	public string? Notes { get; set; }
}

#endregion