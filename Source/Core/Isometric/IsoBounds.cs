using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGGame.Core.Isometric;

/// <summary>
/// An immutable axis-aligned bounding box in isometric 3D space.
/// Defines a rectangular region with minimum and maximum IsoPos corners.
/// 
/// <example>
/// <code>
/// IsoBounds room = new IsoBounds(new IsoPos(0, 0), new IsoPos(10, 10));
/// IsoBounds floor1 = room.AtLevel(IsoLevel.Floor1);
/// 
/// if (room.Contains(playerPos)) {
///     // Player is in the room
/// }
/// 
/// foreach (var pos in room.EnumerateEdge()) {
///     PlaceWall(pos);
/// }
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct IsoBounds : IEquatable<IsoBounds>, IFormattable {
    #region Fields

    private readonly IsoPos _min;
    private readonly IsoPos _max;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates bounds from two corner positions (will be normalized).
    /// </summary>
    public IsoBounds(IsoPos a, IsoPos b) {
        _min = IsoPos.Min(a, b);
        _max = IsoPos.Max(a, b);
    }

    /// <summary>
    /// Creates bounds from center position and half-extents.
    /// </summary>
    public IsoBounds(IsoPos center, int halfWidth, int halfHeight, int levelSpan = 0) {
        _min = new IsoPos(
            center.X - halfWidth,
            center.Y - halfHeight,
            center.Level - levelSpan
        );
        _max = new IsoPos(
            center.X + halfWidth,
            center.Y + halfHeight,
            center.Level + levelSpan
        );
    }

    /// <summary>
    /// Creates bounds from position and size.
    /// </summary>
    public IsoBounds(IsoPos origin, int width, int height) {
        _min = origin;
        _max = new IsoPos(origin.X + width - 1, origin.Y + height - 1, origin.Level);
    }

    #endregion

    #region Static Presets

    /// <summary>A zero-size bounds at origin.</summary>
    public static IsoBounds Zero => new(IsoPos.Origin, IsoPos.Origin);

    /// <summary>A unit bounds (1x1) at origin.</summary>
    public static IsoBounds Unit => new(IsoPos.Origin, new IsoPos(1, 1));

    /// <summary>Invalid/empty bounds.</summary>
    public static IsoBounds Invalid => new(IsoPos.Invalid, IsoPos.Invalid);

    #endregion

    #region Properties

    /// <summary>Minimum corner (lowest X, Y, and Level).</summary>
    public IsoPos Min => _min;

    /// <summary>Maximum corner (highest X, Y, and Level).</summary>
    public IsoPos Max => _max;

    /// <summary>Width (X extent).</summary>
    public int Width => _max.X - _min.X + 1;

    /// <summary>Height/Depth (Y extent).</summary>
    public int Height => _max.Y - _min.Y + 1;

    /// <summary>Number of levels spanned.</summary>
    public int LevelSpan => _max.Level - _min.Level + 1;

    /// <summary>Total area on each level.</summary>
    public int Area => Width * Height;

    /// <summary>Total volume (area × levels).</summary>
    public int Volume => Area * LevelSpan;

    /// <summary>Center position.</summary>
    public IsoPos Center => new(
        (_min.X + _max.X) / 2,
        (_min.Y + _max.Y) / 2,
        new IsoLevel((_min.Level.Value + _max.Level.Value) / 2)
    );

    /// <summary>Size as Vector2Int (Width, Height).</summary>
    public Vector2Int Size => new(Width, Height);

    /// <summary>True if this bounds is valid.</summary>
    public bool IsValid => _min.IsValid && _max.IsValid;

    /// <summary>True if this bounds has zero or negative size.</summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>True if all positions are on the same level.</summary>
    public bool IsSingleLevel => LevelSpan == 1;

    /// <summary>The minimum level in this bounds.</summary>
    public IsoLevel MinLevel => _min.Level;

    /// <summary>The maximum level in this bounds.</summary>
    public IsoLevel MaxLevel => _max.Level;

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates bounds that contain all given positions.
    /// </summary>
    public static IsoBounds Enclosing(params IsoPos[] positions) {
        if (positions.Length == 0) return Invalid;

        var min = positions[0];
        var max = positions[0];

        for (int i = 1; i < positions.Length; i++) {
            min = IsoPos.Min(min, positions[i]);
            max = IsoPos.Max(max, positions[i]);
        }

        return new IsoBounds(min, max);
    }

    /// <summary>
    /// Creates bounds that contain all given positions.
    /// </summary>
    public static IsoBounds Enclosing(IEnumerable<IsoPos> positions) {
        IsoPos? min = null, max = null;

        foreach (var pos in positions) {
            if (min == null) {
                min = pos;
                max = pos;
            } else {
                min = IsoPos.Min(min.Value, pos);
                max = IsoPos.Max(max!.Value, pos);
            }
        }

        return min.HasValue ? new IsoBounds(min.Value, max!.Value) : Invalid;
    }

    /// <summary>
    /// Creates a square bounds centered on a position.
    /// </summary>
    public static IsoBounds Square(IsoPos center, int radius) =>
        new(center, radius, radius);

    /// <summary>
    /// Creates bounds from Unity BoundsInt.
    /// </summary>
    public static IsoBounds FromBoundsInt(BoundsInt bounds) => new(
        new IsoPos(bounds.xMin, bounds.yMin, new IsoLevel(bounds.zMin)),
        new IsoPos(bounds.xMax - 1, bounds.yMax - 1, new IsoLevel(bounds.zMax - 1))
    );

    #endregion

    #region Instance Methods - Queries

    /// <summary>
    /// Returns true if the position is inside this bounds (inclusive).
    /// </summary>
    public bool Contains(IsoPos pos) =>
        pos.X >= _min.X && pos.X <= _max.X &&
        pos.Y >= _min.Y && pos.Y <= _max.Y &&
        pos.Level >= _min.Level && pos.Level <= _max.Level;

    /// <summary>
    /// Returns true if the position is inside this bounds on the XY plane.
    /// </summary>
    public bool ContainsXY(IsoPos pos) =>
        pos.X >= _min.X && pos.X <= _max.X &&
        pos.Y >= _min.Y && pos.Y <= _max.Y;

    /// <summary>
    /// Returns true if this bounds overlaps with another.
    /// </summary>
    public bool Overlaps(IsoBounds other) =>
        _min.X <= other._max.X && _max.X >= other._min.X &&
        _min.Y <= other._max.Y && _max.Y >= other._min.Y &&
        _min.Level <= other._max.Level && _max.Level >= other._min.Level;

    /// <summary>
    /// Returns true if this bounds fully contains another.
    /// </summary>
    public bool Contains(IsoBounds other) =>
        Contains(other._min) && Contains(other._max);

    /// <summary>
    /// Returns the closest position inside this bounds to the given position.
    /// </summary>
    public IsoPos ClosestPoint(IsoPos pos) => new(
        Math.Clamp(pos.X, _min.X, _max.X),
        Math.Clamp(pos.Y, _min.Y, _max.Y),
        new IsoLevel(Math.Clamp(pos.Level.Value, _min.Level.Value, _max.Level.Value))
    );

    /// <summary>
    /// Returns the distance from a position to this bounds (0 if inside).
    /// </summary>
    public IsoDistance DistanceTo(IsoPos pos) {
        var closest = ClosestPoint(pos);
        return pos.DistanceTo(closest);
    }

    /// <summary>
    /// Returns true if the position is on the edge of this bounds.
    /// </summary>
    public bool IsOnEdge(IsoPos pos) {
        if (!Contains(pos)) return false;
        return pos.X == _min.X || pos.X == _max.X ||
               pos.Y == _min.Y || pos.Y == _max.Y;
    }

    /// <summary>
    /// Returns true if the position is in a corner of this bounds.
    /// </summary>
    public bool IsCorner(IsoPos pos) {
        if (!Contains(pos)) return false;
        bool xEdge = pos.X == _min.X || pos.X == _max.X;
        bool yEdge = pos.Y == _min.Y || pos.Y == _max.Y;
        return xEdge && yEdge;
    }

    #endregion

    #region Instance Methods - Modifications

    /// <summary>
    /// Returns bounds expanded by the given amount in all directions.
    /// </summary>
    public IsoBounds Expanded(int amount) => new(
        new IsoPos(_min.X - amount, _min.Y - amount, _min.Level - amount),
        new IsoPos(_max.X + amount, _max.Y + amount, _max.Level + amount)
    );

    /// <summary>
    /// Returns bounds expanded horizontally only.
    /// </summary>
    public IsoBounds ExpandedXY(int amount) => new(
        new IsoPos(_min.X - amount, _min.Y - amount, _min.Level),
        new IsoPos(_max.X + amount, _max.Y + amount, _max.Level)
    );

    /// <summary>
    /// Returns bounds contracted by the given amount.
    /// </summary>
    public IsoBounds Contracted(int amount) => Expanded(-amount);

    /// <summary>
    /// Returns bounds shifted by the given offset.
    /// </summary>
    public IsoBounds Shifted(IsoRel offset) => new(_min + offset, _max + offset);

    /// <summary>
    /// Returns bounds moved to a specific level (single-level).
    /// </summary>
    public IsoBounds AtLevel(IsoLevel level) => new(
        _min.WithLevel(level),
        _max.WithLevel(level)
    );

    /// <summary>
    /// Returns the intersection of this bounds with another.
    /// </summary>
    public IsoBounds Intersection(IsoBounds other) {
        var newMin = IsoPos.Max(_min, other._min);
        var newMax = IsoPos.Min(_max, other._max);

        if (newMin.X > newMax.X || newMin.Y > newMax.Y || newMin.Level > newMax.Level) {
            return Invalid;
        }

        return new IsoBounds(newMin, newMax);
    }

    /// <summary>
    /// Returns the union of this bounds with another.
    /// </summary>
    public IsoBounds Union(IsoBounds other) => new(
        IsoPos.Min(_min, other._min),
        IsoPos.Max(_max, other._max)
    );

    /// <summary>
    /// Returns bounds that include the given position.
    /// </summary>
    public IsoBounds Including(IsoPos pos) => new(
        IsoPos.Min(_min, pos),
        IsoPos.Max(_max, pos)
    );

    #endregion

    #region Instance Methods - Enumeration

    /// <summary>
    /// Enumerates all positions in this bounds on a single level.
    /// </summary>
    public IEnumerable<IsoPos> EnumerateLevel(IsoLevel level) {
        if (!IsValid) yield break;

        for (int x = _min.X; x <= _max.X; x++) {
            for (int y = _min.Y; y <= _max.Y; y++) {
                yield return new IsoPos(x, y, level);
            }
        }
    }

    /// <summary>
    /// Enumerates all positions in this bounds.
    /// </summary>
    public IEnumerable<IsoPos> EnumerateAll() {
        if (!IsValid) yield break;

        foreach (var level in _min.Level.To(_max.Level)) {
            foreach (var pos in EnumerateLevel(level)) {
                yield return pos;
            }
        }
    }

    /// <summary>
    /// Enumerates edge positions on a single level.
    /// </summary>
    public IEnumerable<IsoPos> EnumerateEdge(IsoLevel level) {
        if (!IsValid) yield break;

        // Top and bottom edges
        for (int x = _min.X; x <= _max.X; x++) {
            yield return new IsoPos(x, _min.Y, level);
            if (_max.Y != _min.Y) {
                yield return new IsoPos(x, _max.Y, level);
            }
        }

        // Left and right edges (excluding corners)
        for (int y = _min.Y + 1; y < _max.Y; y++) {
            yield return new IsoPos(_min.X, y, level);
            if (_max.X != _min.X) {
                yield return new IsoPos(_max.X, y, level);
            }
        }
    }

    /// <summary>
    /// Enumerates corner positions on a single level.
    /// </summary>
    public IEnumerable<IsoPos> EnumerateCorners(IsoLevel level) {
        yield return new IsoPos(_min.X, _min.Y, level);
        yield return new IsoPos(_max.X, _min.Y, level);
        yield return new IsoPos(_min.X, _max.Y, level);
        yield return new IsoPos(_max.X, _max.Y, level);
    }

    /// <summary>
    /// Enumerates interior positions (excluding edge) on a single level.
    /// </summary>
    public IEnumerable<IsoPos> EnumerateInterior(IsoLevel level) {
        if (!IsValid || Width < 3 || Height < 3) yield break;

        for (int x = _min.X + 1; x < _max.X; x++) {
            for (int y = _min.Y + 1; y < _max.Y; y++) {
                yield return new IsoPos(x, y, level);
            }
        }
    }

    /// <summary>
    /// Returns a random position within this bounds.
    /// </summary>
    public IsoPos RandomPosition() {
        if (!IsValid) return IsoPos.Invalid;

        return new IsoPos(
            UnityEngine.Random.Range(_min.X, _max.X + 1),
            UnityEngine.Random.Range(_min.Y, _max.Y + 1),
            new IsoLevel(UnityEngine.Random.Range(_min.Level.Value, _max.Level.Value + 1))
        );
    }

    /// <summary>
    /// Returns a random position on a specific level.
    /// </summary>
    public IsoPos RandomPositionOnLevel(IsoLevel level) {
        if (!IsValid) return IsoPos.Invalid;

        return new IsoPos(
            UnityEngine.Random.Range(_min.X, _max.X + 1),
            UnityEngine.Random.Range(_min.Y, _max.Y + 1),
            level
        );
    }

    #endregion

    #region Instance Methods - Conversion

    /// <summary>
    /// Converts to Unity BoundsInt.
    /// </summary>
    public BoundsInt ToBoundsInt() => new(
        _min.X, _min.Y, _min.Level.Value,
        Width, Height, LevelSpan
    );

    /// <summary>
    /// Converts to Unity Bounds (world space).
    /// </summary>
    public Bounds ToBounds(float levelHeight = IsoLevel.LEVEL_HEIGHT_UNITS) {
        var minWorld = _min.ToWorld(levelHeight);
        var maxWorld = _max.ToWorld(levelHeight) + Vector3.one;
        return new Bounds((minWorld + maxWorld) / 2, maxWorld - minWorld);
    }

    /// <summary>
    /// Converts to a RectInt (XY only).
    /// </summary>
    public RectInt ToRectInt() => new(_min.X, _min.Y, Width, Height);

    #endregion

    #region Operators

    public static bool operator ==(IsoBounds a, IsoBounds b) =>
        a._min == b._min && a._max == b._max;

    public static bool operator !=(IsoBounds a, IsoBounds b) => !(a == b);

    #endregion

    #region IEquatable

    public bool Equals(IsoBounds other) => this == other;
    public override bool Equals(object? obj) => obj is IsoBounds other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_min, _max);

    #endregion

    #region IFormattable

    public override string ToString() =>
        $"[{_min} → {_max}] ({Width}×{Height}×{LevelSpan})";

    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") return ToString();

        return format.ToLowerInvariant() switch {
            "size" => $"{Width}×{Height}×{LevelSpan}",
            "area" => $"{Area} tiles",
            "volume" => $"{Volume} cells",
            "xy" => $"[({_min.X},{_min.Y}) → ({_max.X},{_max.Y})]",
            _ => ToString()
        };
    }

    #endregion
}

#region Extension Methods

public static class IsoBoundsExtensions {
    /// <summary>
    /// Creates bounds from position to another position.
    /// </summary>
    public static IsoBounds To(this IsoPos from, IsoPos to) => new(from, to);

    /// <summary>
    /// Returns true if the position is within the bounds.
    /// </summary>
    public static bool IsInBounds(this IsoPos pos, IsoBounds bounds) => bounds.Contains(pos);
}

#endregion