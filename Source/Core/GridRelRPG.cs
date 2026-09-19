using System;
using UnityEngine;

namespace RPGGame.Core;

/// <summary>
/// An immutable, self-documenting wrapper for relative grid offsets and directions.
/// The discrete counterpart to AngleRPG for tile-based movement and pathfinding.
/// <example>
/// <code>
/// GridRelRPG offset = GridRelRPG.NorthEast * 3.Tiles();
/// GridRelRPG opposite = GridRelRPG.North.Opposite();
/// Vector3Int move = GridRelRPG.East; // Implicit conversion
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct GridRelRPG : IEquatable<GridRelRPG>, IFormattable {
    #region Fields

    private readonly int _x;
    private readonly int _y;
    private readonly int _z;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new GridRelRPG from integer offsets.
    /// </summary>
    /// <param name="x">X offset (East/West).</param>
    /// <param name="y">Y offset (Up/Down in 3D, typically 0 for 2D grids).</param>
    /// <param name="z">Z offset (North/South).</param>
    public GridRelRPG(int x, int y, int z) {
        _x = x;
        _y = y;
        _z = z;
    }

    /// <summary>
    /// Creates a new GridRelRPG from X and Z offsets (Y = 0).
    /// Convenient for 2D isometric grids on the XZ plane.
    /// </summary>
    /// <param name="x">X offset (East/West).</param>
    /// <param name="z">Z offset (North/South).</param>
    public GridRelRPG(int x, int z) : this(x, 0, z) { }

    #endregion

    #region Static Presets - Zero

    /// <summary>Zero offset (0, 0, 0).</summary>
    public static GridRelRPG Zero => new(0, 0, 0);

    #endregion

    #region Static Presets - Cardinal Directions (4-way)

    /// <summary>North direction (+Z).</summary>
    public static GridRelRPG North => new(0, 0, 1);

    /// <summary>East direction (+X).</summary>
    public static GridRelRPG East => new(1, 0, 0);

    /// <summary>South direction (-Z).</summary>
    public static GridRelRPG South => new(0, 0, -1);

    /// <summary>West direction (-X).</summary>
    public static GridRelRPG West => new(-1, 0, 0);

    /// <summary>Up direction (+Y) for 3D grids.</summary>
    public static GridRelRPG Up => new(0, 1, 0);

    /// <summary>Down direction (-Y) for 3D grids.</summary>
    public static GridRelRPG Down => new(0, -1, 0);

    #endregion

    #region Static Presets - Ordinal Directions (diagonals)

    /// <summary>NorthEast direction (+X, +Z).</summary>
    public static GridRelRPG NorthEast => new(1, 0, 1);

    /// <summary>SouthEast direction (+X, -Z).</summary>
    public static GridRelRPG SouthEast => new(1, 0, -1);

    /// <summary>SouthWest direction (-X, -Z).</summary>
    public static GridRelRPG SouthWest => new(-1, 0, -1);

    /// <summary>NorthWest direction (-X, +Z).</summary>
    public static GridRelRPG NorthWest => new(-1, 0, 1);

    #endregion

    #region Static Presets - Direction Arrays

    /// <summary>The four cardinal directions (N, E, S, W).</summary>
    public static readonly GridRelRPG[] FourDirections = [North, East, South, West];

    /// <summary>The eight compass directions (N, NE, E, SE, S, SW, W, NW).</summary>
    public static readonly GridRelRPG[] EightDirections = [
        North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
    ];

    /// <summary>The six directions for 3D grids (N, E, S, W, Up, Down).</summary>
    public static readonly GridRelRPG[] SixDirections3D = [North, East, South, West, Up, Down];

    #endregion

    #region Properties

    /// <summary>X component of the offset (East/West).</summary>
    public int X => _x;

    /// <summary>Y component of the offset (Up/Down).</summary>
    public int Y => _y;

    /// <summary>Z component of the offset (North/South).</summary>
    public int Z => _z;

    /// <summary>Returns true if this is a zero offset.</summary>
    public bool IsZero => _x == 0 && _y == 0 && _z == 0;

    /// <summary>Returns true if this is a cardinal direction (N, E, S, W, Up, Down).</summary>
    public bool IsCardinal => !IsZero && (
        (_x == 0 && _z == 0 && _y != 0) || // Up/Down
        (_x == 0 && _y == 0 && _z != 0) || // North/South
        (_y == 0 && _z == 0 && _x != 0)    // East/West
    );

    /// <summary>Returns true if this is a diagonal direction (NE, SE, SW, NW).</summary>
    public bool IsDiagonal => !IsZero && _y == 0 && _x != 0 && _z != 0;

    /// <summary>Returns true if this offset lies on the XZ plane (Y = 0).</summary>
    public bool IsFlat => _y == 0;

    /// <summary>Returns true if this is a unit direction (magnitude 1 in any axis).</summary>
    public bool IsUnitDirection => IsCardinal && (
        Mathf.Abs(_x) + Mathf.Abs(_y) + Mathf.Abs(_z) == 1
    );

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates a GridRelRPG from a Vector3Int.
    /// </summary>
    public static GridRelRPG FromVector3Int(Vector3Int v) => new(v.x, v.y, v.z);

    /// <summary>
    /// Creates a GridRelRPG from a Vector2Int (Y = 0, maps to XZ).
    /// </summary>
    public static GridRelRPG FromVector2Int(Vector2Int v) => new(v.x, 0, v.y);

    /// <summary>
    /// Creates a GridRelRPG by snapping an angle to the nearest cardinal or ordinal direction.
    /// </summary>
    /// <param name="angle">The angle to snap.</param>
    /// <param name="eightWay">If true, snaps to 8 directions; if false, snaps to 4.</param>
    /// <returns>The nearest grid direction.</returns>
    public static GridRelRPG FromAngle(AngleRPG angle, bool eightWay = true) {
        float degrees = angle.Degrees;
        
        if (eightWay) {
            // Snap to nearest 45° (8 directions)
            int index = Mathf.RoundToInt(degrees / 45f) % 8;
            return index switch {
                0 => East,      // 0°
                1 => NorthEast, // 45°
                2 => North,     // 90°
                3 => NorthWest, // 135°
                4 => West,      // 180°
                5 => SouthWest, // 225°
                6 => South,     // 270°
                7 => SouthEast, // 315°
                _ => East
            };
        } else {
            // Snap to nearest 90° (4 directions)
            int index = Mathf.RoundToInt(degrees / 90f) % 4;
            return index switch {
                0 => East,  // 0°
                1 => North, // 90°
                2 => West,  // 180°
                3 => South, // 270°
                _ => East
            };
        }
    }

    /// <summary>
    /// Creates a random cardinal direction (N, E, S, W).
    /// </summary>
    public static GridRelRPG RandomCardinal() {
        return FourDirections[UnityEngine.Random.Range(0, 4)];
    }

    /// <summary>
    /// Creates a random 8-way direction.
    /// </summary>
    public static GridRelRPG RandomEightWay() {
        return EightDirections[UnityEngine.Random.Range(0, 8)];
    }

    #endregion

    #region Instance Methods - Conversion

    /// <summary>
    /// Converts this offset to a Vector3Int.
    /// </summary>
    public Vector3Int ToVector3Int() => new(_x, _y, _z);

    /// <summary>
    /// Converts this offset to a Vector2Int (X, Z components only).
    /// </summary>
    public Vector2Int ToVector2Int() => new(_x, _z);

    /// <summary>
    /// Converts this direction to an AngleRPG (XZ plane only).
    /// Returns 0° for zero offset.
    /// </summary>
    public AngleRPG ToAngle() {
        if (_x == 0 && _z == 0) return AngleRPG.Zero;
        return AngleRPG.FromDegrees(Mathf.Atan2(_z, _x) * Mathf.Rad2Deg);
    }

    /// <summary>
    /// Converts this direction to a normalized Vector3 (for world-space operations).
    /// </summary>
    public Vector3 ToDirection3D() {
        if (IsZero) return Vector3.zero;
        return new Vector3(_x, _y, _z).normalized;
    }

    /// <summary>
    /// Converts this direction to a normalized Vector2 (XZ plane).
    /// </summary>
    public Vector2 ToDirection2D() {
        if (_x == 0 && _z == 0) return Vector2.zero;
        return new Vector2(_x, _z).normalized;
    }

    #endregion

    #region Instance Methods - Transformation

    /// <summary>
    /// Returns the opposite direction.
    /// </summary>
    public GridRelRPG Opposite() => new(-_x, -_y, -_z);

    /// <summary>
    /// Rotates 90° clockwise on the XZ plane (looking down from +Y).
    /// </summary>
    public GridRelRPG Rotate90Clockwise() => new(_z, _y, -_x);

    /// <summary>
    /// Rotates 90° counter-clockwise on the XZ plane (looking down from +Y).
    /// </summary>
    public GridRelRPG Rotate90CounterClockwise() => new(-_z, _y, _x);

    /// <summary>
    /// Rotates 180° on the XZ plane. Same as Opposite() for flat directions.
    /// </summary>
    public GridRelRPG Rotate180() => new(-_x, _y, -_z);

    /// <summary>
    /// Returns the two perpendicular directions on the XZ plane.
    /// </summary>
    public GridRelRPG[] Perpendiculars() => [
        Rotate90Clockwise(),
        Rotate90CounterClockwise()
    ];

    /// <summary>
    /// Returns this offset with Y forced to 0.
    /// </summary>
    public GridRelRPG Flattened() => new(_x, 0, _z);

    /// <summary>
    /// Returns this offset with components clamped to -1, 0, or 1 (normalized to unit direction).
    /// </summary>
    public GridRelRPG Normalized() => new(
        Math.Sign(_x),
        Math.Sign(_y),
        Math.Sign(_z)
    );

    /// <summary>
    /// Returns this offset scaled by a factor (rounded to integers).
    /// </summary>
    public GridRelRPG Scaled(int factor) => new(_x * factor, _y * factor, _z * factor);

    /// <summary>
    /// Returns this offset with absolute values.
    /// </summary>
    public GridRelRPG Abs() => new(Math.Abs(_x), Math.Abs(_y), Math.Abs(_z));

    #endregion

    #region Instance Methods - Distance Metrics

    /// <summary>
    /// Manhattan distance (L1 norm): |x| + |y| + |z|.
    /// Also known as "taxicab distance" or "city block distance".
    /// </summary>
    public TilesRPG ManhattanLength() => new(Math.Abs(_x) + Math.Abs(_y) + Math.Abs(_z));

    /// <summary>
    /// Manhattan distance on the XZ plane only: |x| + |z|.
    /// </summary>
    public TilesRPG ManhattanLengthXZ() => new(Math.Abs(_x) + Math.Abs(_z));

    /// <summary>
    /// Chebyshev distance (L∞ norm): max(|x|, |y|, |z|).
    /// Also known as "chessboard distance" - the king's move distance.
    /// </summary>
    public TilesRPG ChebyshevLength() => new(Math.Max(Math.Abs(_x), Math.Max(Math.Abs(_y), Math.Abs(_z))));

    /// <summary>
    /// Chebyshev distance on the XZ plane only: max(|x|, |z|).
    /// </summary>
    public TilesRPG ChebyshevLengthXZ() => new(Math.Max(Math.Abs(_x), Math.Abs(_z)));

    /// <summary>
    /// Euclidean distance (L2 norm): sqrt(x² + y² + z²).
    /// </summary>
    public TilesRPG EuclideanLength() => new(Mathf.Sqrt(_x * _x + _y * _y + _z * _z));

    /// <summary>
    /// Euclidean distance on the XZ plane only: sqrt(x² + z²).
    /// </summary>
    public TilesRPG EuclideanLengthXZ() => new(Mathf.Sqrt(_x * _x + _z * _z));

    /// <summary>
    /// Squared Euclidean distance (avoids sqrt for comparisons): x² + y² + z².
    /// </summary>
    public int SqrMagnitude => _x * _x + _y * _y + _z * _z;

    /// <summary>
    /// Squared Euclidean distance on the XZ plane: x² + z².
    /// </summary>
    public int SqrMagnitudeXZ => _x * _x + _z * _z;

    #endregion

    #region Instance Methods - Direction Name

    /// <summary>
    /// Gets the compass direction name for this offset.
    /// </summary>
    public string GetDirectionName() {
        if (IsZero) return "None";
        
        // Normalize to get pure direction
        int sx = Math.Sign(_x);
        int sz = Math.Sign(_z);
        int sy = Math.Sign(_y);

        if (sy != 0 && sx == 0 && sz == 0) {
            return sy > 0 ? "Up" : "Down";
        }

        return (sx, sz) switch {
            (0, 1) => "North",
            (1, 1) => "NorthEast",
            (1, 0) => "East",
            (1, -1) => "SouthEast",
            (0, -1) => "South",
            (-1, -1) => "SouthWest",
            (-1, 0) => "West",
            (-1, 1) => "NorthWest",
            _ => "None"
        };
    }

    #endregion

    #region Operators - Implicit Conversions

    /// <summary>Implicitly converts GridRelRPG to Vector3Int.</summary>
    public static implicit operator Vector3Int(GridRelRPG g) => g.ToVector3Int();

    /// <summary>Implicitly converts Vector3Int to GridRelRPG.</summary>
    public static implicit operator GridRelRPG(Vector3Int v) => new(v.x, v.y, v.z);

    /// <summary>Implicitly converts (int, int, int) tuple to GridRelRPG.</summary>
    public static implicit operator GridRelRPG((int x, int y, int z) t) => new(t.x, t.y, t.z);

    /// <summary>Implicitly converts (int, int) tuple to GridRelRPG (XZ plane).</summary>
    public static implicit operator GridRelRPG((int x, int z) t) => new(t.x, 0, t.z);

    #endregion

    #region Operators - Arithmetic with GridRelRPG

    /// <summary>Adds two grid offsets.</summary>
    public static GridRelRPG operator +(GridRelRPG a, GridRelRPG b) =>
        new(a._x + b._x, a._y + b._y, a._z + b._z);

    /// <summary>Subtracts two grid offsets.</summary>
    public static GridRelRPG operator -(GridRelRPG a, GridRelRPG b) =>
        new(a._x - b._x, a._y - b._y, a._z - b._z);

    /// <summary>Negates a grid offset (same as Opposite).</summary>
    public static GridRelRPG operator -(GridRelRPG g) => g.Opposite();

    /// <summary>Multiplies by an integer scalar.</summary>
    public static GridRelRPG operator *(GridRelRPG g, int scalar) =>
        new(g._x * scalar, g._y * scalar, g._z * scalar);

    /// <summary>Multiplies an integer scalar by a grid offset.</summary>
    public static GridRelRPG operator *(int scalar, GridRelRPG g) => g * scalar;

    /// <summary>Divides by an integer scalar (integer division).</summary>
    public static GridRelRPG operator /(GridRelRPG g, int divisor) {
        if (divisor == 0) return Zero;
        return new(g._x / divisor, g._y / divisor, g._z / divisor);
    }

    #endregion

    #region Operators - Arithmetic with TilesRPG (returns scaled Vector3Int)

    /// <summary>
    /// Multiplies direction by TilesRPG, returning a scaled Vector3Int.
    /// Perfect for: position + direction * distance
    /// </summary>
    public static Vector3Int operator *(GridRelRPG g, TilesRPG t) {
        int scale = t.ToInt();
        return new Vector3Int(g._x * scale, g._y * scale, g._z * scale);
    }

    /// <summary>
    /// Multiplies TilesRPG by direction, returning a scaled Vector3Int.
    /// </summary>
    public static Vector3Int operator *(TilesRPG t, GridRelRPG g) => g * t;

    #endregion

    #region Operators - Arithmetic with Vector3Int

    /// <summary>Adds a grid offset to a Vector3Int.</summary>
    public static Vector3Int operator +(Vector3Int v, GridRelRPG g) =>
        new(v.x + g._x, v.y + g._y, v.z + g._z);

    /// <summary>Adds a Vector3Int to a grid offset.</summary>
    public static Vector3Int operator +(GridRelRPG g, Vector3Int v) => v + g;

    /// <summary>Subtracts a grid offset from a Vector3Int.</summary>
    public static Vector3Int operator -(Vector3Int v, GridRelRPG g) =>
        new(v.x - g._x, v.y - g._y, v.z - g._z);

    #endregion

    #region Operators - Comparison

    /// <summary>Equality comparison.</summary>
    public static bool operator ==(GridRelRPG a, GridRelRPG b) =>
        a._x == b._x && a._y == b._y && a._z == b._z;

    /// <summary>Inequality comparison.</summary>
    public static bool operator !=(GridRelRPG a, GridRelRPG b) => !(a == b);

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(GridRelRPG other) => this == other;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is GridRelRPG other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_x, _y, _z);

    #endregion

    #region IFormattable

    /// <summary>
    /// Returns a string representation: "(+1, 0, +1)" or "(0, 0, 0)".
    /// </summary>
    public override string ToString() {
        string xs = _x >= 0 ? $"+{_x}" : _x.ToString();
        string ys = _y >= 0 ? $"+{_y}" : _y.ToString();
        string zs = _z >= 0 ? $"+{_z}" : _z.ToString();
        return $"({xs}, {ys}, {zs})";
    }

    /// <summary>
    /// Formats the grid offset.
    /// Formats:
    /// - null/G = "(+1, 0, +1)"
    /// - "dir" = "NorthEast"
    /// - "xz" = "(+1, +1)" (XZ only)
    /// - "v" = "1, 0, 1" (raw values, no signs)
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") {
            return ToString();
        }

        return format.ToLowerInvariant() switch {
            "dir" or "direction" => GetDirectionName(),
            "xz" => $"({(_x >= 0 ? "+" : "")}{_x}, {(_z >= 0 ? "+" : "")}{_z})",
            "v" or "values" => $"{_x}, {_y}, {_z}",
            _ => ToString()
        };
    }

    #endregion
}

#region Extension Methods

/// <summary>
/// Extension methods for GridRelRPG integration.
/// </summary>
public static class GridRelRPGExtensions {
    /// <summary>
    /// Converts a Vector3Int to a GridRelRPG.
    /// </summary>
    public static GridRelRPG ToGridRel(this Vector3Int v) => GridRelRPG.FromVector3Int(v);

    /// <summary>
    /// Converts a Vector2Int to a GridRelRPG (XZ plane).
    /// </summary>
    public static GridRelRPG ToGridRel(this Vector2Int v) => GridRelRPG.FromVector2Int(v);

    /// <summary>
    /// Snaps an angle to the nearest grid direction.
    /// </summary>
    /// <param name="angle">The angle to snap.</param>
    /// <param name="eightWay">If true, snaps to 8 directions; if false, snaps to 4.</param>
    public static GridRelRPG ToGridDirection(this AngleRPG angle, bool eightWay = true) =>
        GridRelRPG.FromAngle(angle, eightWay);
}

#endregion

#region Usage Examples
/*
 * ═══════════════════════════════════════════════════════════════════════════
 * GridRelRPG USAGE EXAMPLES
 * ═══════════════════════════════════════════════════════════════════════════
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // CREATION STYLES
 * // ─────────────────────────────────────────────────────────────────────────
 * GridRelRPG d1 = GridRelRPG.North;              // Preset direction
 * GridRelRPG d2 = GridRelRPG.NorthEast;          // Diagonal preset
 * GridRelRPG d3 = (1, 0, 1);                     // Tuple implicit conversion
 * GridRelRPG d4 = (1, 1);                        // XZ-only tuple (Y=0)
 * GridRelRPG d5 = new Vector3Int(1, 0, -1);      // From Vector3Int
 * GridRelRPG d6 = GridRelRPG.FromAngle(45.Degrees()); // From AngleRPG
 * GridRelRPG d7 = GridRelRPG.RandomCardinal();   // Random N/E/S/W
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // TILEMAP MOVEMENT
 * // ─────────────────────────────────────────────────────────────────────────
 * Vector3Int currentCell = tilemap.WorldToCell(transform.position);
 * Vector3Int nextCell = currentCell + GridRelRPG.East;
 * tilemap.SetTile(nextCell, someTile);
 *
 * // Move 3 tiles in a direction
 * Vector3Int targetCell = currentCell + GridRelRPG.NorthEast * 3;
 *
 * // Using TilesRPG for distance
 * Vector3Int farCell = currentCell + GridRelRPG.North * 5.Tiles();
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // NEIGHBOR ITERATION
 * // ─────────────────────────────────────────────────────────────────────────
 * foreach (var dir in GridRelRPG.FourDirections) {
 *     Vector3Int neighbor = currentCell + dir;
 *     if (IsWalkable(neighbor)) {
 *         // Valid move
 *     }
 * }
 *
 * foreach (var dir in GridRelRPG.EightDirections) {
 *     Vector3Int neighbor = currentCell + dir;
 *     float cost = dir.IsDiagonal ? 1.414f : 1f;
 *     // A* pathfinding...
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // DIRECTION MANIPULATION
 * // ─────────────────────────────────────────────────────────────────────────
 * GridRelRPG facing = GridRelRPG.East;
 * GridRelRPG behind = facing.Opposite();           // West
 * GridRelRPG turnRight = facing.Rotate90Clockwise(); // South
 * GridRelRPG turnLeft = facing.Rotate90CounterClockwise(); // North
 *
 * var perpendiculars = facing.Perpendiculars();    // [South, North]
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // ANGLE INTEGRATION
 * // ─────────────────────────────────────────────────────────────────────────
 * AngleRPG heading = 45.Degrees();
 * GridRelRPG gridDir = heading.ToGridDirection();   // NorthEast
 * GridRelRPG cardinalOnly = heading.ToGridDirection(eightWay: false); // East or North
 *
 * // Convert back to angle
 * AngleRPG angle = GridRelRPG.NorthWest.ToAngle(); // 135°
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // DISTANCE CALCULATIONS
 * // ─────────────────────────────────────────────────────────────────────────
 * GridRelRPG offset = (3, 0, 4);
 * TilesRPG manhattan = offset.ManhattanLengthXZ();  // 7 tiles
 * TilesRPG chebyshev = offset.ChebyshevLengthXZ();  // 4 tiles (king's move)
 * TilesRPG euclidean = offset.EuclideanLengthXZ();  // 5 tiles
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // PROCEDURAL GENERATION
 * // ─────────────────────────────────────────────────────────────────────────
 * GridRelRPG mainPath = GridRelRPG.East;
 * GridRelRPG branch1 = mainPath.Rotate90Clockwise();   // Create branch
 * GridRelRPG branch2 = mainPath.Rotate90CounterClockwise();
 *
 * // Random corridor direction
 * GridRelRPG corridor = GridRelRPG.RandomCardinal();
 * for (int i = 0; i < 10; i++) {
 *     Vector3Int nextPos = startPos + corridor * i;
 *     PlaceFloor(nextPos);
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // TYPE CHECKS
 * // ─────────────────────────────────────────────────────────────────────────
 * if (dir.IsCardinal) {
 *     // No diagonal movement cost
 * }
 * if (dir.IsDiagonal) {
 *     // Apply √2 movement cost
 * }
 * if (dir.IsZero) {
 *     // No movement
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // STRING FORMATTING
 * // ─────────────────────────────────────────────────────────────────────────
 * GridRelRPG offset = GridRelRPG.NorthEast;
 * Debug.Log(offset);                    // "(+1, 0, +1)"
 * Debug.Log($"{offset:dir}");           // "NorthEast"
 * Debug.Log($"{offset:xz}");            // "(+1, +1)"
 * Debug.Log($"{offset:v}");             // "1, 0, 1"
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // COMBINED WITH TilesRPG
 * // ─────────────────────────────────────────────────────────────────────────
 * TilesRPG moveRange = 5.Tiles();
 * GridRelRPG direction = GridRelRPG.NorthEast;
 * Vector3Int destination = startPos + direction * moveRange;
 *
 * // Calculate range ring
 * TilesRPG ringRadius = 3.Tiles();
 * foreach (var dir in GridRelRPG.EightDirections) {
 *     Vector3Int ringPos = centerPos + dir * ringRadius;
 *     // Mark positions on the ring
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // IMPLICIT VECTOR3INT USAGE
 * // ─────────────────────────────────────────────────────────────────────────
 * void MoveEntity(Vector3Int direction) { ... }
 * MoveEntity(GridRelRPG.North);  // Works due to implicit conversion!
 *
 * Vector3Int pos = GridRelRPG.East * 5;  // Returns Vector3Int directly
 */
#endregion