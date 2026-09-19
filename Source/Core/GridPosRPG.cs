using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGGame.Core;

/// <summary>
/// An immutable, self-documenting wrapper for absolute grid positions on a tilemap.
/// The "where am I on the grid" type, perfectly integrated with <see cref="GridRelRPG"/> for offsets.
/// <example>
/// <code>
/// GridPosRPG pos = GridPosRPG.Origin;
/// GridPosRPG moved = pos + GridRelRPG.NorthEast * 3.Tiles();
/// TilesRPG dist = pos.DistanceTo(other);
/// Vector3 world = pos.ToWorldCenter();
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct GridPosRPG : IEquatable<GridPosRPG>, IFormattable {
    #region Fields

    private readonly int _x;
    private readonly int _y;
    private readonly int _z;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new GridPosRPG from integer coordinates.
    /// </summary>
    /// <param name="x">X coordinate (East/West axis).</param>
    /// <param name="y">Y coordinate (Up/Down axis, typically 0 for 2D).</param>
    /// <param name="z">Z coordinate (North/South axis).</param>
    public GridPosRPG(int x, int y, int z) {
        _x = x;
        _y = y;
        _z = z;
    }

    /// <summary>
    /// Creates a new GridPosRPG from X and Z coordinates (Y = 0).
    /// Convenient for 2D isometric grids on the XZ plane.
    /// </summary>
    /// <param name="x">X coordinate (East/West axis).</param>
    /// <param name="z">Z coordinate (North/South axis).</param>
    public GridPosRPG(int x, int z) : this(x, 0, z) { }

    #endregion

    #region Static Presets

    /// <summary>The origin position (0, 0, 0).</summary>
    public static GridPosRPG Origin => new(0, 0, 0);

    /// <summary>An invalid/unset position marker. Use for "no position" scenarios.</summary>
    public static GridPosRPG Invalid => new(int.MinValue, int.MinValue, int.MinValue);

    #endregion

    #region Properties

    /// <summary>X coordinate (East/West axis).</summary>
    public int X => _x;

    /// <summary>Y coordinate (Up/Down axis).</summary>
    public int Y => _y;

    /// <summary>Z coordinate (North/South axis).</summary>
    public int Z => _z;

    /// <summary>Returns true if this is the origin (0, 0, 0).</summary>
    public bool IsOrigin => _x == 0 && _y == 0 && _z == 0;

    /// <summary>Returns true if this is an invalid/unset position.</summary>
    public bool IsInvalid => _x == int.MinValue && _y == int.MinValue && _z == int.MinValue;

    /// <summary>Returns true if this position is valid (not the Invalid marker).</summary>
    public bool IsValid => !IsInvalid;

    /// <summary>Returns true if this position lies on the XZ plane (Y = 0).</summary>
    public bool IsFlat => _y == 0;

    /// <summary>Returns true if all coordinates are non-negative.</summary>
    public bool IsPositive => _x >= 0 && _y >= 0 && _z >= 0;

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates a GridPosRPG from a Vector3Int.
    /// </summary>
    public static GridPosRPG FromVector3Int(Vector3Int v) => new(v.x, v.y, v.z);

    /// <summary>
    /// Creates a GridPosRPG from a Vector2Int (Y = 0, maps to XZ).
    /// </summary>
    public static GridPosRPG FromVector2Int(Vector2Int v) => new(v.x, 0, v.y);

    /// <summary>
    /// Creates a GridPosRPG from world position by rounding to nearest integers.
    /// </summary>
    /// <param name="worldPos">World position to convert.</param>
    public static GridPosRPG FromWorld(Vector3 worldPos) => new(
        Mathf.RoundToInt(worldPos.x),
        Mathf.RoundToInt(worldPos.y),
        Mathf.RoundToInt(worldPos.z)
    );

    /// <summary>
    /// Creates a GridPosRPG from world position on the XZ plane (Y = 0).
    /// </summary>
    /// <param name="worldPos">World position to convert.</param>
    public static GridPosRPG FromWorldXZ(Vector3 worldPos) => new(
        Mathf.RoundToInt(worldPos.x),
        0,
        Mathf.RoundToInt(worldPos.z)
    );

    /// <summary>
    /// Creates a GridPosRPG from a tilemap cell position.
    /// </summary>
    /// <param name="cellPos">Tilemap cell position.</param>
    public static GridPosRPG FromCell(Vector3Int cellPos) => FromVector3Int(cellPos);

    /// <summary>
    /// Returns the minimum of two positions (component-wise).
    /// </summary>
    public static GridPosRPG Min(GridPosRPG a, GridPosRPG b) => new(
        Math.Min(a._x, b._x),
        Math.Min(a._y, b._y),
        Math.Min(a._z, b._z)
    );

    /// <summary>
    /// Returns the maximum of two positions (component-wise).
    /// </summary>
    public static GridPosRPG Max(GridPosRPG a, GridPosRPG b) => new(
        Math.Max(a._x, b._x),
        Math.Max(a._y, b._y),
        Math.Max(a._z, b._z)
    );

    /// <summary>
    /// Linearly interpolates between two positions (rounded to integers).
    /// </summary>
    /// <param name="a">Start position.</param>
    /// <param name="b">End position.</param>
    /// <param name="t">Interpolation factor (0-1).</param>
    public static GridPosRPG Lerp(GridPosRPG a, GridPosRPG b, float t) {
        t = Mathf.Clamp01(t);
        return new(
            Mathf.RoundToInt(Mathf.Lerp(a._x, b._x, t)),
            Mathf.RoundToInt(Mathf.Lerp(a._y, b._y, t)),
            Mathf.RoundToInt(Mathf.Lerp(a._z, b._z, t))
        );
    }

    #endregion

    #region Instance Methods - Conversion

    /// <summary>
    /// Converts this position to a Vector3Int (for Unity tilemap operations).
    /// </summary>
    public Vector3Int ToVector3Int() => new(_x, _y, _z);

    /// <summary>
    /// Converts this position to a Vector2Int (X, Z components only).
    /// </summary>
    public Vector2Int ToVector2Int() => new(_x, _z);

    /// <summary>
    /// Converts this position to world-space coordinates (center of tile).
    /// Assumes 1 unit = 1 tile.
    /// </summary>
    public Vector3 ToWorldCenter() => new(_x, _y, _z);

    /// <summary>
    /// Converts this position to world-space coordinates with custom tile size.
    /// </summary>
    /// <param name="tileSize">Size of each tile in world units.</param>
    public Vector3 ToWorldCenter(float tileSize) => new(_x * tileSize, _y * tileSize, _z * tileSize);

    /// <summary>
    /// Converts this position to world-space coordinates with offset for tile center.
    /// </summary>
    /// <param name="tileSize">Size of each tile in world units.</param>
    /// <param name="centerOffset">Offset to add (e.g., 0.5 for center of tile).</param>
    public Vector3 ToWorldCenter(float tileSize, float centerOffset) => new(
        _x * tileSize + centerOffset,
        _y * tileSize,
        _z * tileSize + centerOffset
    );

    /// <summary>
    /// Converts this position to a 2D world position (XZ plane as XY).
    /// </summary>
    public Vector2 ToWorld2D() => new(_x, _z);

    /// <summary>
    /// Converts this position to a 2D world position with custom tile size.
    /// </summary>
    public Vector2 ToWorld2D(float tileSize) => new(_x * tileSize, _z * tileSize);

    #endregion

    #region Instance Methods - Offset & Movement

    /// <summary>
    /// Returns a new position offset by the given GridRelRPG.
    /// </summary>
    /// <param name="offset">The relative offset to apply.</param>
    public GridPosRPG Offset(GridRelRPG offset) => new(_x + offset.X, _y + offset.Y, _z + offset.Z);

    /// <summary>
    /// Returns a new position offset by raw coordinates.
    /// </summary>
    public GridPosRPG Offset(int dx, int dy, int dz) => new(_x + dx, _y + dy, _z + dz);

    /// <summary>
    /// Returns a new position offset on the XZ plane.
    /// </summary>
    public GridPosRPG OffsetXZ(int dx, int dz) => new(_x + dx, _y, _z + dz);

    /// <summary>
    /// Returns the relative offset from this position to another.
    /// </summary>
    /// <param name="other">The target position.</param>
    public GridRelRPG RelativeTo(GridPosRPG other) => new(other._x - _x, other._y - _y, other._z - _z);

    /// <summary>
    /// Returns the direction from this position to another (normalized to unit direction).
    /// </summary>
    /// <param name="target">The target position.</param>
    public GridRelRPG DirectionTo(GridPosRPG target) => RelativeTo(target).Normalized();

    /// <summary>
    /// Returns the 8-way grid direction from this position to another.
    /// </summary>
    /// <param name="target">The target position.</param>
    public GridRelRPG DirectionTo8Way(GridPosRPG target) {
        var rel = RelativeTo(target);
        if (rel.IsZero) return GridRelRPG.Zero;
        return GridRelRPG.FromAngle(rel.ToAngle(), eightWay: true);
    }

    /// <summary>
    /// Returns the 4-way cardinal direction from this position to another.
    /// </summary>
    /// <param name="target">The target position.</param>
    public GridRelRPG DirectionTo4Way(GridPosRPG target) {
        var rel = RelativeTo(target);
        if (rel.IsZero) return GridRelRPG.Zero;
        return GridRelRPG.FromAngle(rel.ToAngle(), eightWay: false);
    }

    /// <summary>
    /// Returns this position with Y forced to 0.
    /// </summary>
    public GridPosRPG Flattened() => new(_x, 0, _z);

    /// <summary>
    /// Returns this position with Y set to the specified value.
    /// </summary>
    public GridPosRPG WithY(int y) => new(_x, y, _z);

    /// <summary>
    /// Returns this position clamped to a rectangular bounds.
    /// </summary>
    public GridPosRPG Clamped(GridPosRPG min, GridPosRPG max) => new(
        Math.Clamp(_x, min._x, max._x),
        Math.Clamp(_y, min._y, max._y),
        Math.Clamp(_z, min._z, max._z)
    );

    /// <summary>
    /// Returns this position clamped to non-negative coordinates.
    /// </summary>
    public GridPosRPG ClampedPositive() => new(
        Math.Max(0, _x),
        Math.Max(0, _y),
        Math.Max(0, _z)
    );

    #endregion

    #region Instance Methods - Distance

    /// <summary>
    /// Calculates the Manhattan distance to another position.
    /// </summary>
    /// <param name="other">The target position.</param>
    public TilesRPG ManhattanDistanceTo(GridPosRPG other) => RelativeTo(other).ManhattanLength();

    /// <summary>
    /// Calculates the Manhattan distance on the XZ plane to another position.
    /// </summary>
    public TilesRPG ManhattanDistanceToXZ(GridPosRPG other) => RelativeTo(other).ManhattanLengthXZ();

    /// <summary>
    /// Calculates the Chebyshev (chessboard/king's move) distance to another position.
    /// </summary>
    public TilesRPG ChebyshevDistanceTo(GridPosRPG other) => RelativeTo(other).ChebyshevLength();

    /// <summary>
    /// Calculates the Chebyshev distance on the XZ plane to another position.
    /// </summary>
    public TilesRPG ChebyshevDistanceToXZ(GridPosRPG other) => RelativeTo(other).ChebyshevLengthXZ();

    /// <summary>
    /// Calculates the Euclidean distance to another position.
    /// </summary>
    public TilesRPG EuclideanDistanceTo(GridPosRPG other) => RelativeTo(other).EuclideanLength();

    /// <summary>
    /// Calculates the Euclidean distance on the XZ plane to another position.
    /// </summary>
    public TilesRPG EuclideanDistanceToXZ(GridPosRPG other) => RelativeTo(other).EuclideanLengthXZ();

    /// <summary>
    /// Calculates the squared Euclidean distance (avoids sqrt for comparisons).
    /// </summary>
    public int SqrDistanceTo(GridPosRPG other) => RelativeTo(other).SqrMagnitude;

    /// <summary>
    /// Calculates the squared Euclidean distance on the XZ plane.
    /// </summary>
    public int SqrDistanceToXZ(GridPosRPG other) => RelativeTo(other).SqrMagnitudeXZ;

    /// <summary>
    /// Returns true if this position is within the specified Manhattan distance of another.
    /// </summary>
    public bool IsWithinManhattan(GridPosRPG other, TilesRPG range) =>
        ManhattanDistanceToXZ(other) <= range;

    /// <summary>
    /// Returns true if this position is within the specified Chebyshev distance of another.
    /// </summary>
    public bool IsWithinChebyshev(GridPosRPG other, TilesRPG range) =>
        ChebyshevDistanceToXZ(other) <= range;

    /// <summary>
    /// Returns true if this position is within the specified Euclidean distance of another.
    /// </summary>
    public bool IsWithinEuclidean(GridPosRPG other, TilesRPG range) =>
        EuclideanDistanceToXZ(other) <= range;

    /// <summary>
    /// Returns true if this position is adjacent (4-way) to another.
    /// </summary>
    public bool IsAdjacentCardinal(GridPosRPG other) {
        var rel = RelativeTo(other);
        return rel.IsFlat && rel.ManhattanLengthXZ().ToInt() == 1 && rel.IsCardinal;
    }

    /// <summary>
    /// Returns true if this position is adjacent (8-way, including diagonals) to another.
    /// </summary>
    public bool IsAdjacentEightWay(GridPosRPG other) {
        var rel = RelativeTo(other);
        return rel.IsFlat && rel.ChebyshevLengthXZ().ToInt() == 1;
    }

    #endregion

    #region Instance Methods - Neighbors

    /// <summary>
    /// Returns the 4 cardinal neighbors (N, E, S, W).
    /// </summary>
    public GridPosRPG[] Neighbors4() => [
        this + GridRelRPG.North,
        this + GridRelRPG.East,
        this + GridRelRPG.South,
        this + GridRelRPG.West
    ];

    /// <summary>
    /// Returns the 8 compass neighbors (including diagonals).
    /// </summary>
    public GridPosRPG[] Neighbors8() => [
        this + GridRelRPG.North,
        this + GridRelRPG.NorthEast,
        this + GridRelRPG.East,
        this + GridRelRPG.SouthEast,
        this + GridRelRPG.South,
        this + GridRelRPG.SouthWest,
        this + GridRelRPG.West,
        this + GridRelRPG.NorthWest
    ];

    /// <summary>
    /// Returns the 6 neighbors for 3D grids (N, E, S, W, Up, Down).
    /// </summary>
    public GridPosRPG[] Neighbors6() => [
        this + GridRelRPG.North,
        this + GridRelRPG.East,
        this + GridRelRPG.South,
        this + GridRelRPG.West,
        this + GridRelRPG.Up,
        this + GridRelRPG.Down
    ];

    /// <summary>
    /// Yields 4 cardinal neighbors lazily (for performance-critical code).
    /// </summary>
    public IEnumerable<GridPosRPG> EnumerateNeighbors4() {
        yield return this + GridRelRPG.North;
        yield return this + GridRelRPG.East;
        yield return this + GridRelRPG.South;
        yield return this + GridRelRPG.West;
    }

    /// <summary>
    /// Yields 8 compass neighbors lazily (for performance-critical code).
    /// </summary>
    public IEnumerable<GridPosRPG> EnumerateNeighbors8() {
        foreach (var dir in GridRelRPG.EightDirections) {
            yield return this + dir;
        }
    }

    /// <summary>
    /// Returns all positions within the specified Manhattan radius (diamond shape).
    /// </summary>
    /// <param name="radius">Maximum Manhattan distance.</param>
    /// <param name="includeCenter">Whether to include this position.</param>
    public IEnumerable<GridPosRPG> GetManhattanArea(int radius, bool includeCenter = true) {
        for (int dx = -radius; dx <= radius; dx++) {
            int zRange = radius - Math.Abs(dx);
            for (int dz = -zRange; dz <= zRange; dz++) {
                if (!includeCenter && dx == 0 && dz == 0) continue;
                yield return OffsetXZ(dx, dz);
            }
        }
    }

    /// <summary>
    /// Returns all positions within the specified Chebyshev radius (square shape).
    /// </summary>
    /// <param name="radius">Maximum Chebyshev distance.</param>
    /// <param name="includeCenter">Whether to include this position.</param>
    public IEnumerable<GridPosRPG> GetChebyshevArea(int radius, bool includeCenter = true) {
        for (int dx = -radius; dx <= radius; dx++) {
            for (int dz = -radius; dz <= radius; dz++) {
                if (!includeCenter && dx == 0 && dz == 0) continue;
                yield return OffsetXZ(dx, dz);
            }
        }
    }

    /// <summary>
    /// Returns positions on the Manhattan ring at exactly the specified distance.
    /// </summary>
    /// <param name="distance">Exact Manhattan distance.</param>
    public IEnumerable<GridPosRPG> GetManhattanRing(int distance) {
        if (distance <= 0) {
            yield return this;
            yield break;
        }

        // Walk the diamond perimeter
        for (int i = 0; i < distance; i++) {
            yield return OffsetXZ(distance - i, i);      // NE edge
            yield return OffsetXZ(-i, distance - i);     // NW edge
            yield return OffsetXZ(-(distance - i), -i);  // SW edge
            yield return OffsetXZ(i, -(distance - i));   // SE edge
        }
    }

    /// <summary>
    /// Returns positions on the Chebyshev ring at exactly the specified distance.
    /// </summary>
    /// <param name="distance">Exact Chebyshev distance.</param>
    public IEnumerable<GridPosRPG> GetChebyshevRing(int distance) {
        if (distance <= 0) {
            yield return this;
            yield break;
        }

        // Walk the square perimeter
        for (int i = -distance; i <= distance; i++) {
            yield return OffsetXZ(i, distance);   // Top edge
            yield return OffsetXZ(i, -distance);  // Bottom edge
        }
        for (int i = -distance + 1; i < distance; i++) {
            yield return OffsetXZ(distance, i);   // Right edge
            yield return OffsetXZ(-distance, i);  // Left edge
        }
    }

    #endregion

    #region Instance Methods - Line Drawing

    /// <summary>
    /// Returns all positions on a line from this position to target (Bresenham's algorithm).
    /// </summary>
    /// <param name="target">End position of the line.</param>
    /// <param name="includeStart">Whether to include this position.</param>
    /// <param name="includeEnd">Whether to include the target position.</param>
    public IEnumerable<GridPosRPG> LineTo(GridPosRPG target, bool includeStart = true, bool includeEnd = true) {
        int x0 = _x, z0 = _z;
        int x1 = target._x, z1 = target._z;

        int dx = Math.Abs(x1 - x0);
        int dz = Math.Abs(z1 - z0);
        int sx = x0 < x1 ? 1 : -1;
        int sz = z0 < z1 ? 1 : -1;
        int err = dx - dz;

        int x = x0, z = z0;
        bool isFirst = true;

        while (true) {
            bool isLast = (x == x1 && z == z1);

            if ((isFirst && includeStart) || (!isFirst && !isLast) || (isLast && includeEnd)) {
                yield return new GridPosRPG(x, _y, z);
            }

            isFirst = false;

            if (isLast) break;

            int e2 = 2 * err;
            if (e2 > -dz) {
                err -= dz;
                x += sx;
            }
            if (e2 < dx) {
                err += dx;
                z += sz;
            }
        }
    }

    /// <summary>
    /// Returns true if there's a clear line of sight to target (no positions filtered out).
    /// </summary>
    /// <param name="target">End position to check.</param>
    /// <param name="isBlocked">Function to check if a position blocks line of sight.</param>
    public bool HasLineOfSightTo(GridPosRPG target, Func<GridPosRPG, bool> isBlocked) {
        foreach (var pos in LineTo(target, includeStart: false, includeEnd: false)) {
            if (isBlocked(pos)) return false;
        }
        return true;
    }

    #endregion

    #region Operators - Implicit Conversions

    /// <summary>Implicitly converts GridPosRPG to Vector3Int.</summary>
    public static implicit operator Vector3Int(GridPosRPG p) => p.ToVector3Int();

    /// <summary>Implicitly converts Vector3Int to GridPosRPG.</summary>
    public static implicit operator GridPosRPG(Vector3Int v) => new(v.x, v.y, v.z);

    /// <summary>Implicitly converts (int, int, int) tuple to GridPosRPG.</summary>
    public static implicit operator GridPosRPG((int x, int y, int z) t) => new(t.x, t.y, t.z);

    /// <summary>Implicitly converts (int, int) tuple to GridPosRPG (XZ plane).</summary>
    public static implicit operator GridPosRPG((int x, int z) t) => new(t.x, 0, t.z);

    #endregion

    #region Operators - Arithmetic with GridRelRPG

    /// <summary>Adds a relative offset to an absolute position.</summary>
    public static GridPosRPG operator +(GridPosRPG pos, GridRelRPG offset) =>
        new(pos._x + offset.X, pos._y + offset.Y, pos._z + offset.Z);

    /// <summary>Adds an absolute position to a relative offset.</summary>
    public static GridPosRPG operator +(GridRelRPG offset, GridPosRPG pos) => pos + offset;

    /// <summary>Subtracts a relative offset from an absolute position.</summary>
    public static GridPosRPG operator -(GridPosRPG pos, GridRelRPG offset) =>
        new(pos._x - offset.X, pos._y - offset.Y, pos._z - offset.Z);

    /// <summary>Subtracts two positions to get a relative offset.</summary>
    public static GridRelRPG operator -(GridPosRPG a, GridPosRPG b) =>
        new(a._x - b._x, a._y - b._y, a._z - b._z);

    #endregion

    #region Operators - Arithmetic with Vector3Int (for tilemap compatibility)

    /// <summary>Adds a Vector3Int offset to a position.</summary>
    public static GridPosRPG operator +(GridPosRPG pos, Vector3Int offset) =>
        new(pos._x + offset.x, pos._y + offset.y, pos._z + offset.z);

    /// <summary>Subtracts a Vector3Int offset from a position.</summary>
    public static GridPosRPG operator -(GridPosRPG pos, Vector3Int offset) =>
        new(pos._x - offset.x, pos._y - offset.y, pos._z - offset.z);

    #endregion

    #region Operators - Comparison

    /// <summary>Equality comparison.</summary>
    public static bool operator ==(GridPosRPG a, GridPosRPG b) =>
        a._x == b._x && a._y == b._y && a._z == b._z;

    /// <summary>Inequality comparison.</summary>
    public static bool operator !=(GridPosRPG a, GridPosRPG b) => !(a == b);

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(GridPosRPG other) => this == other;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is GridPosRPG other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_x, _y, _z);

    #endregion

    #region IFormattable

    /// <summary>
    /// Returns a string representation: "(5, 0, 3)" or "(0, 0, 0)".
    /// </summary>
    public override string ToString() => $"({_x}, {_y}, {_z})";

    /// <summary>
    /// Formats the grid position.
    /// Formats:
    /// - null/G = "(5, 0, 3)"
    /// - "xz" = "(5, 3)" (XZ only)
    /// - "world" = "5.0, 0.0, 3.0" (as world coordinates)
    /// - "cell" = "[5, 0, 3]" (cell notation)
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") {
            return ToString();
        }

        return format.ToLowerInvariant() switch {
            "xz" => $"({_x}, {_z})",
            "world" => $"{_x}.0, {_y}.0, {_z}.0",
            "cell" => $"[{_x}, {_y}, {_z}]",
            "flat" => IsFlat ? $"({_x}, {_z})" : $"({_x}, {_y}, {_z})",
            _ => ToString()
        };
    }

    #endregion
}

#region Extension Methods

/// <summary>
/// Extension methods for GridPosRPG integration.
/// </summary>
public static class GridPosRPGExtensions {
    /// <summary>
    /// Converts a Vector3Int to a GridPosRPG.
    /// </summary>
    public static GridPosRPG ToGridPos(this Vector3Int v) => GridPosRPG.FromVector3Int(v);

    /// <summary>
    /// Converts a Vector2Int to a GridPosRPG (XZ plane).
    /// </summary>
    public static GridPosRPG ToGridPos(this Vector2Int v) => GridPosRPG.FromVector2Int(v);

    /// <summary>
    /// Converts a world position to a GridPosRPG.
    /// </summary>
    public static GridPosRPG ToGridPos(this Vector3 worldPos) => GridPosRPG.FromWorld(worldPos);

    /// <summary>
    /// Converts a world position to a GridPosRPG on the XZ plane.
    /// </summary>
    public static GridPosRPG ToGridPosXZ(this Vector3 worldPos) => GridPosRPG.FromWorldXZ(worldPos);

    /// <summary>
    /// Creates a grid position from X and Z coordinates.
    /// </summary>
    public static GridPosRPG GridPos(this int x, int z) => new(x, z);
}

#endregion

#region Usage Examples
/*
 * ═══════════════════════════════════════════════════════════════════════════
 * GridPosRPG USAGE EXAMPLES
 * ═══════════════════════════════════════════════════════════════════════════
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // CREATION STYLES
 * // ─────────────────────────────────────────────────────────────────────────
 * GridPosRPG p1 = GridPosRPG.Origin;              // (0, 0, 0)
 * GridPosRPG p2 = new GridPosRPG(5, 3);           // (5, 0, 3) - XZ only
 * GridPosRPG p3 = new GridPosRPG(5, 1, 3);        // (5, 1, 3) - 3D
 * GridPosRPG p4 = (5, 0, 3);                      // Tuple implicit conversion
 * GridPosRPG p5 = (5, 3);                         // XZ-only tuple
 * GridPosRPG p6 = new Vector3Int(5, 0, 3);        // From Vector3Int
 * GridPosRPG p7 = GridPosRPG.FromWorld(transform.position);
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // TILEMAP INTEGRATION
 * // ─────────────────────────────────────────────────────────────────────────
 * GridPosRPG playerPos = tilemap.WorldToCell(transform.position).ToGridPos();
 * Vector3Int cell = playerPos; // Implicit conversion for tilemap operations
 * tilemap.SetTile(cell, someTile);
 *
 * Vector3 worldPos = playerPos.ToWorldCenter();
 * transform.position = worldPos;
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // MOVEMENT WITH GridRelRPG
 * // ─────────────────────────────────────────────────────────────────────────
 * GridPosRPG current = (10, 5);
 * GridPosRPG moved = current + GridRelRPG.NorthEast;           // (11, 0, 6)
 * GridPosRPG far = current + GridRelRPG.East * 5.Tiles();      // (15, 0, 5)
 * GridPosRPG back = current - GridRelRPG.North;                // (10, 0, 4)
 *
 * // Direction from one position to another
 * GridRelRPG dir = (target - current).Normalized();
 * GridRelRPG dir8 = current.DirectionTo8Way(target);
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // DISTANCE CALCULATIONS
 * // ─────────────────────────────────────────────────────────────────────────
 * TilesRPG manhattan = playerPos.ManhattanDistanceToXZ(enemyPos);
 * TilesRPG chebyshev = playerPos.ChebyshevDistanceToXZ(enemyPos);
 * TilesRPG euclidean = playerPos.EuclideanDistanceToXZ(enemyPos);
 *
 * if (playerPos.IsWithinManhattan(enemyPos, 5.Tiles())) {
 *     // Enemy in attack range!
 * }
 *
 * if (playerPos.IsAdjacentEightWay(treasurePos)) {
 *     // Can pick up treasure
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // NEIGHBOR ITERATION
 * // ─────────────────────────────────────────────────────────────────────────
 * foreach (var neighbor in current.Neighbors4()) {
 *     if (IsWalkable(neighbor)) {
 *         validMoves.Add(neighbor);
 *     }
 * }
 *
 * foreach (var neighbor in current.Neighbors8()) {
 *     var dir = current.RelativeTo(neighbor);
 *     float cost = dir.IsDiagonal ? 1.414f : 1f;
 *     // A* pathfinding
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // AREA QUERIES
 * // ─────────────────────────────────────────────────────────────────────────
 * // Diamond-shaped area (Manhattan)
 * foreach (var pos in center.GetManhattanArea(radius: 3)) {
 *     HighlightTile(pos);
 * }
 *
 * // Square-shaped area (Chebyshev)
 * foreach (var pos in center.GetChebyshevArea(radius: 2)) {
 *     ApplyAOE(pos);
 * }
 *
 * // Ring at exact distance
 * foreach (var pos in center.GetManhattanRing(distance: 4)) {
 *     SpawnEnemy(pos);
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // LINE OF SIGHT
 * // ─────────────────────────────────────────────────────────────────────────
 * foreach (var pos in shooter.LineTo(target)) {
 *     DrawLaserPoint(pos.ToWorldCenter());
 * }
 *
 * bool canSee = shooter.HasLineOfSightTo(target, pos => IsWall(pos));
 * if (canSee) {
 *     Attack(target);
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // BOUNDS & CLAMPING
 * // ─────────────────────────────────────────────────────────────────────────
 * GridPosRPG mapMin = (0, 0);
 * GridPosRPG mapMax = (100, 100);
 * GridPosRPG clamped = position.Clamped(mapMin, mapMax);
 *
 * if (!position.IsPositive) {
 *     position = position.ClampedPositive();
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // LERPING FOR ANIMATION
 * // ─────────────────────────────────────────────────────────────────────────
 * GridPosRPG start = (0, 0);
 * GridPosRPG end = (10, 5);
 * for (float t = 0; t <= 1; t += 0.1f) {
 *     GridPosRPG interpolated = GridPosRPG.Lerp(start, end, t);
 *     Debug.Log(interpolated);
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // STRING FORMATTING
 * // ─────────────────────────────────────────────────────────────────────────
 * GridPosRPG pos = (5, 0, 3);
 * Debug.Log(pos);                    // "(5, 0, 3)"
 * Debug.Log($"{pos:xz}");            // "(5, 3)"
 * Debug.Log($"{pos:cell}");          // "[5, 0, 3]"
 * Debug.Log($"{pos:world}");         // "5.0, 0.0, 3.0"
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // PROCEDURAL GENERATION
 * // ─────────────────────────────────────────────────────────────────────────
 * GridPosRPG roomCenter = (50, 50);
 * int roomRadius = 5;
 *
 * foreach (var pos in roomCenter.GetChebyshevArea(roomRadius)) {
 *     PlaceFloorTile(pos);
 * }
 *
 * // Corridor from room center
 * GridRelRPG corridorDir = GridRelRPG.RandomCardinal();
 * for (int i = 0; i < 10; i++) {
 *     GridPosRPG corridorPos = roomCenter + corridorDir * i;
 *     PlaceFloorTile(corridorPos);
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // COMBINED WORKFLOW
 * // ─────────────────────────────────────────────────────────────────────────
 * // Get player position
 * GridPosRPG player = tilemap.WorldToCell(playerTransform.position).ToGridPos();
 *
 * // Find direction to target
 * GridRelRPG toTarget = player.DirectionTo8Way(targetPos);
 *
 * // Move one step
 * GridPosRPG nextStep = player + toTarget;
 *
 * // Check if valid
 * if (nextStep.IsWithinChebyshev(player, 1.Tiles()) && IsWalkable(nextStep)) {
 *     playerTransform.position = nextStep.ToWorldCenter();
 * }
 */
#endregion