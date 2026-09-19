using System;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Factory for generating monotonically increasing path node IDs.
/// Guarantees that each call to GetNextId() returns a higher value than the previous.
/// Thread-safe for concurrent node generation.
/// </summary>
[Serializable]
public class PathNodeIdFactory {
    private int _nextId;
    private readonly object _lock = new();

    /// <summary>Creates a new factory starting at ID 0.</summary>
    public PathNodeIdFactory() : this(0) { }

    /// <summary>Creates a new factory starting at the specified ID.</summary>
    public PathNodeIdFactory(int startId) {
        _nextId = startId;
    }

    /// <summary>Gets the next available ID and increments the counter.</summary>
    public PathNodeId GetNextId() {
        lock (_lock) {
            return new PathNodeId(_nextId++);
        }
    }

    /// <summary>Peeks at the next ID without consuming it.</summary>
    public PathNodeId PeekNextId() {
        lock (_lock) {
            return new PathNodeId(_nextId);
        }
    }

    /// <summary>Gets the last ID that was issued.</summary>
    public PathNodeId LastIssuedId => new(_nextId - 1);

    /// <summary>Gets how many IDs have been issued.</summary>
    public int TotalIssued => _nextId;

    /// <summary>Resets the factory to start from 0. Use with caution!</summary>
    public void Reset() {
        lock (_lock) {
            _nextId = 0;
        }
    }

    /// <summary>
    /// Creates a factory initialized from saved state.
    /// Call this when loading a saved game.
    /// </summary>
    public static PathNodeIdFactory FromSaveState(int nextId) {
        return new PathNodeIdFactory(nextId);
    }

    /// <summary>Gets the state needed for saving.</summary>
    public int GetSaveState() => _nextId;
}