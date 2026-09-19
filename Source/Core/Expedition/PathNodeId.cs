using System;

namespace RPGGame.Core.Expedition;

/// <summary>
/// A strongly-typed, monotonically increasing identifier for path nodes.
/// Lower values are always earlier in the path (behind player).
/// Higher values are always later in the path (ahead of player).
/// </summary>
[Serializable]
public readonly struct PathNodeId : IEquatable<PathNodeId>, IComparable<PathNodeId> {
    /// <summary>The numeric value of this ID.</summary>
    public int Value { get; }

    /// <summary>Invalid/unset node ID.</summary>
    public static readonly PathNodeId Invalid = new(-1);

    /// <summary>The starting node ID (first node created).</summary>
    public static readonly PathNodeId Start = new(0);

    public PathNodeId(int value) {
        Value = value;
    }

    /// <summary>Returns true if this is a valid node ID.</summary>
    public bool IsValid => Value >= 0;

    /// <summary>Returns true if this node was created before another.</summary>
    public bool IsBefore(PathNodeId other) => Value < other.Value;

    /// <summary>Returns true if this node was created after another.</summary>
    public bool IsAfter(PathNodeId other) => Value > other.Value;

    /// <summary>Returns how many nodes were created between this and another.</summary>
    public int DistanceTo(PathNodeId other) => Math.Abs(Value - other.Value);

    #region Equality & Comparison

    public bool Equals(PathNodeId other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PathNodeId other && Equals(other);
    public override int GetHashCode() => Value;
    public int CompareTo(PathNodeId other) => Value.CompareTo(other.Value);

    public static bool operator ==(PathNodeId a, PathNodeId b) => a.Value == b.Value;
    public static bool operator !=(PathNodeId a, PathNodeId b) => a.Value != b.Value;
    public static bool operator <(PathNodeId a, PathNodeId b) => a.Value < b.Value;
    public static bool operator >(PathNodeId a, PathNodeId b) => a.Value > b.Value;
    public static bool operator <=(PathNodeId a, PathNodeId b) => a.Value <= b.Value;
    public static bool operator >=(PathNodeId a, PathNodeId b) => a.Value >= b.Value;

    #endregion

    #region Conversion

    public static implicit operator int(PathNodeId id) => id.Value;
    public static explicit operator PathNodeId(int value) => new(value);

    public override string ToString() => Value.ToString();

    #endregion
}