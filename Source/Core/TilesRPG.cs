using System;
using UnityEngine;

namespace RPGGame.Core;

/// <summary>
/// An immutable, self-documenting wrapper for tile-based distances in isometric/orthographic tilemaps.
/// Provides zero-cost abstraction with seamless Unity integration.
/// <example>
/// <code>
/// TilesRPG distance = 3.Tiles();
/// Vector3 offset = Vector3.right * distance;
/// TilesRPG halfTile = TilesRPG.Half;
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct TilesRPG : IEquatable<TilesRPG>, IComparable<TilesRPG>, IFormattable {
    #region Fields

    private readonly float _tiles;

    /// <summary>Epsilon for floating-point comparisons.</summary>
    private const float EPSILON = 0.0001f;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new TilesRPG from a tile count.
    /// </summary>
    /// <param name="tiles">Number of tiles (will be clamped to >= 0).</param>
    public TilesRPG(float tiles) {
        _tiles = Mathf.Max(0f, tiles);
    }

    #endregion

    #region Static Presets

    /// <summary>Zero tiles.</summary>
    public static TilesRPG Zero => new(0f);

    /// <summary>Half a tile (0.5).</summary>
    public static TilesRPG Half => new(0.5f);

    /// <summary>One tile.</summary>
    public static TilesRPG One => new(1f);

    /// <summary>Two tiles.</summary>
    public static TilesRPG Two => new(2f);

    /// <summary>Three tiles.</summary>
    public static TilesRPG Three => new(3f);

    /// <summary>Four tiles.</summary>
    public static TilesRPG Four => new(4f);

    /// <summary>Diagonal distance across one tile (?2 ? 1.414).</summary>
    public static TilesRPG Diagonal => new(Mathf.Sqrt(2f));

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates a TilesRPG from a tile count.
    /// </summary>
    /// <param name="tiles">Number of tiles.</param>
    /// <returns>A new TilesRPG representing the specified distance.</returns>
    public static TilesRPG FromTiles(float tiles) => new(tiles);

    /// <summary>
    /// Creates a TilesRPG from world units, converting based on tile size.
    /// </summary>
    /// <param name="worldUnits">Distance in world units.</param>
    /// <param name="tileSize">Size of one tile in world units (default 1).</param>
    /// <returns>A new TilesRPG representing the equivalent tile distance.</returns>
    public static TilesRPG FromUnits(float worldUnits, float tileSize = 1f) {
        return new TilesRPG(tileSize > 0 ? worldUnits / tileSize : 0f);
    }

    /// <summary>
    /// Calculates the tile distance between two positions.
    /// </summary>
    /// <param name="from">Starting position.</param>
    /// <param name="to">Ending position.</param>
    /// <returns>The distance in tiles.</returns>
    public static TilesRPG Distance(Vector3 from, Vector3 to) {
        return new TilesRPG(Vector3.Distance(from, to));
    }

    /// <summary>
    /// Calculates the tile distance between two positions (2D).
    /// </summary>
    /// <param name="from">Starting position.</param>
    /// <param name="to">Ending position.</param>
    /// <returns>The distance in tiles.</returns>
    public static TilesRPG Distance(Vector2 from, Vector2 to) {
        return new TilesRPG(Vector2.Distance(from, to));
    }

    /// <summary>
    /// Calculates the tile distance between two tile coordinates.
    /// </summary>
    /// <param name="from">Starting tile position.</param>
    /// <param name="to">Ending tile position.</param>
    /// <returns>The distance in tiles.</returns>
    public static TilesRPG Distance(Vector2Int from, Vector2Int to) {
        return new TilesRPG(Vector2Int.Distance(from, to));
    }

    /// <summary>
    /// Returns the minimum of two tile distances.
    /// </summary>
    public static TilesRPG Min(TilesRPG a, TilesRPG b) => new(Mathf.Min(a._tiles, b._tiles));

    /// <summary>
    /// Returns the maximum of two tile distances.
    /// </summary>
    public static TilesRPG Max(TilesRPG a, TilesRPG b) => new(Mathf.Max(a._tiles, b._tiles));

    /// <summary>
    /// Linearly interpolates between two tile distances.
    /// </summary>
    /// <param name="a">Start distance.</param>
    /// <param name="b">End distance.</param>
    /// <param name="t">Interpolation factor (0-1).</param>
    /// <returns>Interpolated distance.</returns>
    public static TilesRPG Lerp(TilesRPG a, TilesRPG b, float t) {
        return new TilesRPG(Mathf.Lerp(a._tiles, b._tiles, Mathf.Clamp01(t)));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the raw tile count as a float.
    /// </summary>
    public float Value => _tiles;

    /// <summary>
    /// Returns true if this represents zero distance.
    /// </summary>
    public bool IsZero => _tiles < EPSILON;

    /// <summary>
    /// Returns true if this represents a positive distance.
    /// </summary>
    public bool IsPositive => _tiles > EPSILON;

    #endregion

    #region Instance Methods

    /// <summary>
    /// Returns the absolute value (always positive for TilesRPG).
    /// </summary>
    public TilesRPG Abs() => new(Mathf.Abs(_tiles));

    /// <summary>
    /// Returns the ceiling (rounded up to nearest whole tile).
    /// </summary>
    public TilesRPG Ceil() => new(Mathf.Ceil(_tiles));

    /// <summary>
    /// Returns the floor (rounded down to nearest whole tile).
    /// </summary>
    public TilesRPG Floor() => new(Mathf.Floor(_tiles));

    /// <summary>
    /// Returns the rounded value (to nearest whole tile).
    /// </summary>
    public TilesRPG Round() => new(Mathf.Round(_tiles));

    /// <summary>
    /// Converts to an integer tile count (rounded).
    /// </summary>
    public int ToInt() => Mathf.RoundToInt(_tiles);

    /// <summary>
    /// Returns the raw float value.
    /// </summary>
    public float ToFloat() => _tiles;

    /// <summary>
    /// Returns a new TilesRPG clamped to a minimum value.
    /// </summary>
    /// <param name="min">Minimum allowed distance.</param>
    public TilesRPG ClampedMin(TilesRPG min) => new(Mathf.Max(_tiles, min._tiles));

    /// <summary>
    /// Returns a new TilesRPG clamped to a maximum value.
    /// </summary>
    /// <param name="max">Maximum allowed distance.</param>
    public TilesRPG ClampedMax(TilesRPG max) => new(Mathf.Min(_tiles, max._tiles));

    /// <summary>
    /// Returns a new TilesRPG clamped between min and max.
    /// </summary>
    /// <param name="min">Minimum allowed distance.</param>
    /// <param name="max">Maximum allowed distance.</param>
    public TilesRPG Clamped(TilesRPG min, TilesRPG max) {
        return new(Mathf.Clamp(_tiles, min._tiles, max._tiles));
    }

    /// <summary>
    /// Converts this tile distance to a world-space Vector3 in the given direction.
    /// </summary>
    /// <param name="direction">The direction to apply the distance.</param>
    /// <returns>A Vector3 offset.</returns>
    public Vector3 ToWorldVector3(Vector3 direction) => direction.normalized * _tiles;

    /// <summary>
    /// Converts this tile distance to a world-space Vector2 in the given direction.
    /// </summary>
    /// <param name="direction">The direction to apply the distance.</param>
    /// <returns>A Vector2 offset.</returns>
    public Vector2 ToWorldVector2(Vector2 direction) => direction.normalized * _tiles;

    /// <summary>
    /// Converts this tile distance to world units.
    /// </summary>
    /// <param name="tileSize">Size of one tile in world units.</param>
    /// <returns>Distance in world units.</returns>
    public float ToWorldUnits(float tileSize = 1f) => _tiles * tileSize;

    #endregion

    #region Operators - Implicit Conversions

    /// <summary>Implicitly converts TilesRPG to float.</summary>
    public static implicit operator float(TilesRPG t) => t._tiles;

    /// <summary>Implicitly converts float to TilesRPG.</summary>
    public static implicit operator TilesRPG(float f) => new(f);

    /// <summary>Implicitly converts int to TilesRPG.</summary>
    public static implicit operator TilesRPG(int i) => new(i);

    #endregion

    #region Operators - Arithmetic

    /// <summary>Adds two tile distances.</summary>
    public static TilesRPG operator +(TilesRPG a, TilesRPG b) => new(a._tiles + b._tiles);

    /// <summary>Adds a float to a tile distance.</summary>
    public static TilesRPG operator +(TilesRPG a, float b) => new(a._tiles + b);

    /// <summary>Adds a tile distance to a float.</summary>
    public static TilesRPG operator +(float a, TilesRPG b) => new(a + b._tiles);

    /// <summary>Subtracts two tile distances.</summary>
    public static TilesRPG operator -(TilesRPG a, TilesRPG b) => new(a._tiles - b._tiles);

    /// <summary>Subtracts a float from a tile distance.</summary>
    public static TilesRPG operator -(TilesRPG a, float b) => new(a._tiles - b);

    /// <summary>Multiplies a tile distance by a scalar.</summary>
    public static TilesRPG operator *(TilesRPG a, float b) => new(a._tiles * b);

    /// <summary>Multiplies a scalar by a tile distance.</summary>
    public static TilesRPG operator *(float a, TilesRPG b) => new(a * b._tiles);

    /// <summary>Multiplies a tile distance by an int.</summary>
    public static TilesRPG operator *(TilesRPG a, int b) => new(a._tiles * b);

    /// <summary>Multiplies an int by a tile distance.</summary>
    public static TilesRPG operator *(int a, TilesRPG b) => new(a * b._tiles);

    /// <summary>Divides a tile distance by a scalar.</summary>
    public static TilesRPG operator /(TilesRPG a, float b) => new(b != 0 ? a._tiles / b : 0f);

    /// <summary>Divides a tile distance by an int.</summary>
    public static TilesRPG operator /(TilesRPG a, int b) => new(b != 0 ? a._tiles / b : 0f);

    #endregion

    #region Operators - Vector Math

    /// <summary>Multiplies a Vector3 by a tile distance.</summary>
    public static Vector3 operator *(Vector3 v, TilesRPG t) => v * t._tiles;

    /// <summary>Multiplies a tile distance by a Vector3.</summary>
    public static Vector3 operator *(TilesRPG t, Vector3 v) => v * t._tiles;

    /// <summary>Multiplies a Vector2 by a tile distance.</summary>
    public static Vector2 operator *(Vector2 v, TilesRPG t) => v * t._tiles;

    /// <summary>Multiplies a tile distance by a Vector2.</summary>
    public static Vector2 operator *(TilesRPG t, Vector2 v) => v * t._tiles;

    #endregion

    #region Operators - Comparison

    /// <summary>Equality comparison.</summary>
    public static bool operator ==(TilesRPG a, TilesRPG b) => Mathf.Abs(a._tiles - b._tiles) < EPSILON;

    /// <summary>Inequality comparison.</summary>
    public static bool operator !=(TilesRPG a, TilesRPG b) => !(a == b);

    /// <summary>Less than comparison.</summary>
    public static bool operator <(TilesRPG a, TilesRPG b) => a._tiles < b._tiles - EPSILON;

    /// <summary>Greater than comparison.</summary>
    public static bool operator >(TilesRPG a, TilesRPG b) => a._tiles > b._tiles + EPSILON;

    /// <summary>Less than or equal comparison.</summary>
    public static bool operator <=(TilesRPG a, TilesRPG b) => a._tiles <= b._tiles + EPSILON;

    /// <summary>Greater than or equal comparison.</summary>
    public static bool operator >=(TilesRPG a, TilesRPG b) => a._tiles >= b._tiles - EPSILON;

    #endregion

    #region IEquatable / IComparable

    /// <inheritdoc />
    public bool Equals(TilesRPG other) => this == other;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TilesRPG other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _tiles.GetHashCode();

    /// <inheritdoc />
    public int CompareTo(TilesRPG other) => _tiles.CompareTo(other._tiles);

    #endregion

    #region IFormattable

    /// <summary>
    /// Returns a string representation: "3.5 tiles"
    /// </summary>
    public override string ToString() => $"{_tiles:0.##} tiles";

    /// <summary>
    /// Formats the tile distance.
    /// Formats: null/G = "3.5 tiles", "t" = "3.5t", "0.0" = numeric format
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") {
            return ToString();
        }

        if (format.EndsWith("t", StringComparison.OrdinalIgnoreCase)) {
            string numFormat = format[..^1];
            if (string.IsNullOrEmpty(numFormat)) numFormat = "0.##";
            return $"{_tiles.ToString(numFormat, formatProvider)}t";
        }

        return _tiles.ToString(format, formatProvider);
    }

    #endregion
}

/// <summary>
/// Extension methods for creating TilesRPG from numeric types.
/// </summary>
public static class TilesRPGExtensions {
    /// <summary>
    /// Converts a float to TilesRPG.
    /// </summary>
    /// <example>3.5f.Tiles()</example>
    public static TilesRPG Tiles(this float value) => TilesRPG.FromTiles(value);

    /// <summary>
    /// Converts an int to TilesRPG.
    /// </summary>
    /// <example>3.Tiles()</example>
    public static TilesRPG Tiles(this int value) => TilesRPG.FromTiles(value);

    /// <summary>
    /// Converts world units to TilesRPG.
    /// </summary>
    /// <param name="worldUnits">Distance in world units.</param>
    /// <param name="tileSize">Size of one tile.</param>
    public static TilesRPG ToTiles(this float worldUnits, float tileSize = 1f) {
        return TilesRPG.FromUnits(worldUnits, tileSize);
    }
}

#region Usage Examples
/*
 * ???????????????????????????????????????????????????????????????????????????
 * TilesRPG USAGE EXAMPLES
 * ???????????????????????????????????????????????????????????????????????????
 * 
 * // ?????????????????????????????????????????????????????????????????????????
 * // CREATION STYLES
 * // ?????????????????????????????????????????????????????????????????????????
 * TilesRPG d1 = 3;                          // Implicit from int
 * TilesRPG d2 = 3.5f;                       // Implicit from float
 * TilesRPG d3 = 3.Tiles();                  // Extension method (int)
 * TilesRPG d4 = 2.5f.Tiles();               // Extension method (float)
 * TilesRPG d5 = TilesRPG.FromTiles(4);      // Explicit factory
 * TilesRPG d6 = TilesRPG.Half;              // Preset
 * 
 * // ?????????????????????????????????????????????????????????????????????????
 * // VECTOR OPERATIONS
 * // ?????????????????????????????????????????????????????????????????????????
 * Vector3 offset = Vector3.right * 3.Tiles();
 * Vector3 moveDir = transform.forward * moveSpeed.Tiles();
 * Vector2 tileOffset = direction * TilesRPG.Diagonal;
 * 
 * // ?????????????????????????????????????????????????????????????????????????
 * // TILEMAP OPERATIONS
 * // ?????????????????????????????????????????????????????????????????????????
 * Vector3Int targetCell = baseCell + Vector3Int.RoundToInt(direction * 2.Tiles());
 * tilemap.SetTile(targetCell, someTile);
 * 
 * // Calculate spawn distance
 * TilesRPG spawnRadius = 5.Tiles();
 * Vector3 spawnOffset = spawnRadius.ToWorldVector3(randomDirection);
 * 
 * // ?????????????????????????????????????????????????????????????????????????
 * // DISTANCE CALCULATIONS
 * // ?????????????????????????????????????????????????????????????????????????
 * TilesRPG dist = TilesRPG.Distance(playerPos, enemyPos);
 * if (dist < 3.Tiles()) {
 *     // Enemy is within 3 tiles - attack!
 * }
 * 
 * TilesRPG attackRange = 2.Tiles();
 * TilesRPG meleeRange = TilesRPG.One;
 * bool inMeleeRange = dist <= meleeRange;
 * 
 * // ?????????????????????????????????????????????????????????????????????????
 * // PROCEDURAL GENERATION
 * // ?????????????????????????????????????????????????????????????????????????
 * TilesRPG roomSpacing = 8.Tiles();
 * TilesRPG corridorWidth = 2.Tiles();
 * TilesRPG wallThickness = TilesRPG.One;
 * 
 * for (int i = 0; i < roomCount; i++) {
 *     Vector3 roomCenter = startPos + Vector3.right * (roomSpacing * i);
 *     GenerateRoom(roomCenter, roomSize);
 * }
 * 
 * // ?????????????????????????????????????????????????????????????????????????
 * // PATH GENERATION
 * // ?????????????????????????????????????????????????????????????????????????
 * TilesRPG nodeSpacing = 4.Tiles();
 * TilesRPG minDistance = 2.Tiles();
 * TilesRPG maxDeviation = 1.5f.Tiles();
 * 
 * Vector3 nextNodePos = currentPos + direction * nodeSpacing;
 * 
 * // ?????????????????????????????????????????????????????????????????????????
 * // MOVEMENT & PHYSICS
 * // ?????????????????????????????????????????????????????????????????????????
 * TilesRPG moveSpeed = 5.Tiles(); // tiles per second
 * Vector3 velocity = moveDirection * moveSpeed * Time.deltaTime;
 * transform.position += velocity;
 * 
 * // Clamped movement
 * TilesRPG maxMoveThisFrame = (moveSpeed * Time.deltaTime).ClampedMax(0.5f.Tiles());
 * 
 * // ?????????????????????????????????????????????????????????????????????????
 * // ROUNDING & SNAPPING
 * // ?????????????????????????????????????????????????????????????????????????
 * TilesRPG exactDist = 3.7f.Tiles();
 * TilesRPG rounded = exactDist.Round();   // 4 tiles
 * TilesRPG floored = exactDist.Floor();   // 3 tiles
 * TilesRPG ceiled = exactDist.Ceil();     // 4 tiles
 * int wholeTiles = exactDist.ToInt();     // 4
 * 
 * // ?????????????????????????????????????????????????????????????????????????
 * // STRING FORMATTING
 * // ?????????????????????????????????????????????????????????????????????????
 * TilesRPG distance = 3.5f.Tiles();
 * Debug.Log(distance);                    // "3.5 tiles"
 * Debug.Log($"{distance:0.0t}");          // "3.5t"
 * Debug.Log($"Moved {distance:0} tiles"); // "Moved 4 tiles"
 */
#endregion