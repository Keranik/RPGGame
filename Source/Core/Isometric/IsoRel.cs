using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGGame.Core.Isometric;

/// <summary>
/// An immutable relative offset/direction in isometric 3D space.
/// Includes horizontal (X, Y) and vertical (level) components.
/// 
/// <example>
/// <code>
/// IsoRel dir = IsoRel.NorthEast;
/// IsoRel upstairs = IsoRel.Up;
/// IsoRel move = IsoRel.East * 3.Tiles();
/// IsoRel climb = IsoRel.North + 1.LevelsUp();
/// 
/// IsoPos newPos = currentPos + dir;
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct IsoRel : IEquatable<IsoRel>, IFormattable {
    #region Fields

    private readonly int _x;
    private readonly int _y;
    private readonly int _dLevel;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new IsoRel with the specified offsets.
    /// </summary>
    public IsoRel(int x, int y, int dLevel = 0) {
        _x = x;
        _y = y;
        _dLevel = dLevel;
    }

    #endregion

    #region Static Presets - Cardinal Directions

    /// <summary>Zero offset.</summary>
    public static IsoRel Zero => new(0, 0, 0);

    /// <summary>North (+Y in iso space).</summary>
    public static IsoRel North => new(0, 1, 0);

    /// <summary>South (-Y in iso space).</summary>
    public static IsoRel South => new(0, -1, 0);

    /// <summary>East (+X in iso space).</summary>
    public static IsoRel East => new(1, 0, 0);

    /// <summary>West (-X in iso space).</summary>
    public static IsoRel West => new(-1, 0, 0);

    #endregion

    #region Static Presets - Diagonal Directions

    /// <summary>NorthEast (+X, +Y).</summary>
    public static IsoRel NorthEast => new(1, 1, 0);

    /// <summary>NorthWest (-X, +Y).</summary>
    public static IsoRel NorthWest => new(-1, 1, 0);

    /// <summary>SouthEast (+X, -Y).</summary>
    public static IsoRel SouthEast => new(1, -1, 0);

    /// <summary>SouthWest (-X, -Y).</summary>
    public static IsoRel SouthWest => new(-1, -1, 0);

    #endregion

    #region Static Presets - Vertical

    /// <summary>One level up.</summary>
    public static IsoRel Up => new(0, 0, 1);

    /// <summary>One level down.</summary>
    public static IsoRel Down => new(0, 0, -1);

    /// <summary>North and up (climbing).</summary>
    public static IsoRel NorthUp => new(0, 1, 1);

    /// <summary>North and down (descending).</summary>
    public static IsoRel NorthDown => new(0, 1, -1);

    /// <summary>East and up.</summary>
    public static IsoRel EastUp => new(1, 0, 1);

    /// <summary>East and down.</summary>
    public static IsoRel EastDown => new(1, 0, -1);

    #endregion

    #region Static Collections

    /// <summary>The four cardinal directions.</summary>
    public static IReadOnlyList<IsoRel> CardinalDirections { get; } = [North, East, South, West];

    /// <summary>All eight horizontal directions.</summary>
    public static IReadOnlyList<IsoRel> EightDirections { get; } = [
        North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
    ];

    /// <summary>Six directions including up/down.</summary>
    public static IReadOnlyList<IsoRel> SixDirections { get; } = [
        North, East, South, West, Up, Down
    ];

    /// <summary>All 26 directions in 3D space.</summary>
    public static IReadOnlyList<IsoRel> AllDirections26 { get; } = GenerateAll26Directions();

    private static IsoRel[] GenerateAll26Directions() {
        var dirs = new List<IsoRel>();
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                for (int dz = -1; dz <= 1; dz++) {
                    if (dx != 0 || dy != 0 || dz != 0) {
                        dirs.Add(new IsoRel(dx, dy, dz));
                    }
                }
            }
        }
        return dirs.ToArray();
    }

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates an IsoRel from an angle on the horizontal plane.
    /// </summary>
    public static IsoRel FromAngle(AngleRPG angle) {
        float rad = angle.ToRadians();
        int x = Mathf.RoundToInt(Mathf.Cos(rad));
        int y = Mathf.RoundToInt(Mathf.Sin(rad));
        return new IsoRel(x, y, 0);
    }

    /// <summary>
    /// Creates an IsoRel from a Vector3Int.
    /// </summary>
    public static IsoRel FromVector3Int(Vector3Int v) => new(v.x, v.y, v.z);

    /// <summary>
    /// Creates an IsoRel from a Vector2Int (horizontal only).
    /// </summary>
    public static IsoRel FromVector2Int(Vector2Int v) => new(v.x, v.y, 0);

    /// <summary>
    /// Returns a random cardinal direction.
    /// </summary>
    public static IsoRel RandomCardinal() => CardinalDirections[UnityEngine.Random.Range(0, 4)];

    /// <summary>
    /// Returns a random 8-way direction.
    /// </summary>
    public static IsoRel Random8Way() => EightDirections[UnityEngine.Random.Range(0, 8)];

    /// <summary>
    /// Returns a random 3D direction.
    /// </summary>
    public static IsoRel Random3D() => AllDirections26[UnityEngine.Random.Range(0, AllDirections26.Count)];

    #endregion

    #region Properties

    /// <summary>X offset (East-West).</summary>
    public int X => _x;

    /// <summary>Y offset (North-South on iso plane).</summary>
    public int Y => _y;

    /// <summary>Level offset (vertical).</summary>
    public int DLevel => _dLevel;

    /// <summary>True if this is a zero offset.</summary>
    public bool IsZero => _x == 0 && _y == 0 && _dLevel == 0;

    /// <summary>True if horizontal only (no level change).</summary>
    public bool IsHorizontal => _dLevel == 0;

    /// <summary>True if vertical only (no horizontal movement).</summary>
    public bool IsVertical => _x == 0 && _y == 0 && _dLevel != 0;

    /// <summary>True if this is a cardinal direction (no diagonal).</summary>
    public bool IsCardinal => (_x == 0) != (_y == 0) && _dLevel == 0;

    /// <summary>True if this is a diagonal direction.</summary>
    public bool IsDiagonal => _x != 0 && _y != 0;

    /// <summary>Squared magnitude for distance comparisons.</summary>
    public int SqrMagnitude => _x * _x + _y * _y + _dLevel * _dLevel;

    /// <summary>Squared horizontal magnitude.</summary>
    public int SqrMagnitudeXY => _x * _x + _y * _y;

    #endregion

    #region Instance Methods - Transformations

    /// <summary>
    /// Returns the opposite direction.
    /// </summary>
    public IsoRel Opposite() => new(-_x, -_y, -_dLevel);

    /// <summary>
    /// Returns this offset with only the horizontal component.
    /// </summary>
    public IsoRel Horizontal() => new(_x, _y, 0);

    /// <summary>
    /// Returns this offset with only the vertical component.
    /// </summary>
    public IsoRel Vertical() => new(0, 0, _dLevel);

    /// <summary>
    /// Returns a unit direction (each component clamped to -1, 0, or 1).
    /// </summary>
    public IsoRel Normalized() => new(
        Math.Sign(_x),
        Math.Sign(_y),
        Math.Sign(_dLevel)
    );

    /// <summary>
    /// Rotates the horizontal component 90° clockwise.
    /// </summary>
    public IsoRel RotateCW() => new(_y, -_x, _dLevel);

    /// <summary>
    /// Rotates the horizontal component 90° counter-clockwise.
    /// </summary>
    public IsoRel RotateCCW() => new(-_y, _x, _dLevel);

    /// <summary>
    /// Rotates the horizontal component 180°.
    /// </summary>
    public IsoRel Rotate180() => new(-_x, -_y, _dLevel);

    /// <summary>
    /// Rotates by the specified number of 45° steps.
    /// </summary>
    public IsoRel Rotate45Steps(int steps) {
        steps = ((steps % 8) + 8) % 8;
        var current = this;
        for (int i = 0; i < steps; i++) {
            current = current.Rotate45CW();
        }
        return current;
    }

    /// <summary>
    /// Rotates the horizontal component 45° clockwise.
    /// </summary>
    public IsoRel Rotate45CW() {
        // Use lookup for 8-way directions
        return (_x, _y) switch {
            (0, 1) => NorthEast.WithLevel(_dLevel),   // N -> NE
            (1, 1) => East.WithLevel(_dLevel),        // NE -> E
            (1, 0) => SouthEast.WithLevel(_dLevel),   // E -> SE
            (1, -1) => South.WithLevel(_dLevel),      // SE -> S
            (0, -1) => SouthWest.WithLevel(_dLevel),  // S -> SW
            (-1, -1) => West.WithLevel(_dLevel),      // SW -> W
            (-1, 0) => NorthWest.WithLevel(_dLevel),  // W -> NW
            (-1, 1) => North.WithLevel(_dLevel),      // NW -> N
            _ => this
        };
    }

    /// <summary>
    /// Returns this offset with a different level change.
    /// </summary>
    public IsoRel WithLevel(int dLevel) => new(_x, _y, dLevel);

    /// <summary>
    /// Returns this offset adding to the level change.
    /// </summary>
    public IsoRel AddLevel(int dLevel) => new(_x, _y, _dLevel + dLevel);

    #endregion

    #region Instance Methods - Measurements

    /// <summary>
    /// Returns the horizontal Manhattan length.
    /// </summary>
    public TilesRPG ManhattanLengthXY() => new(Math.Abs(_x) + Math.Abs(_y));

    /// <summary>
    /// Returns the horizontal Chebyshev length.
    /// </summary>
    public TilesRPG ChebyshevLengthXY() => new(Math.Max(Math.Abs(_x), Math.Abs(_y)));

    /// <summary>
    /// Returns the horizontal Euclidean length.
    /// </summary>
    public TilesRPG EuclideanLengthXY() => new(Mathf.Sqrt(_x * _x + _y * _y));

    /// <summary>
    /// Returns the full 3D distance.
    /// </summary>
    public IsoDistance ToDistance() => new(ChebyshevLengthXY(), Math.Abs(_dLevel));

    /// <summary>
    /// Returns the angle on the horizontal plane.
    /// </summary>
    public AngleRPG ToAngle() {
        if (_x == 0 && _y == 0) return AngleRPG.Zero;
        return AngleRPG.FromDirection(new Vector2(_x, _y));
    }

    #endregion

    #region Instance Methods - Conversion

    /// <summary>
    /// Converts to a Vector3Int.
    /// </summary>
    public Vector3Int ToVector3Int() => new(_x, _y, _dLevel);

    /// <summary>
    /// Converts to a Vector2Int (horizontal only).
    /// </summary>
    public Vector2Int ToVector2Int() => new(_x, _y);

    /// <summary>
    /// Converts to a world-space Vector3.
    /// </summary>
    public Vector3 ToWorld(float levelHeight = IsoLevel.LEVEL_HEIGHT_UNITS) =>
        new(_x, _dLevel * levelHeight, _y);

    /// <summary>
    /// Converts to a screen-space Vector2.
    /// </summary>
    public Vector2 ToScreen(float tileWidth = IsoPos.DEFAULT_TILE_WIDTH,
        float tileHeight = IsoPos.DEFAULT_TILE_HEIGHT,
        float levelHeight = IsoPos.DEFAULT_LEVEL_HEIGHT) {
        float screenX = (_x - _y) * (tileWidth / 2f);
        float screenY = (_x + _y) * (tileHeight / 2f) - (_dLevel * levelHeight);
        return new Vector2(screenX, screenY);
    }

    /// <summary>
    /// Converts to a GridRelRPG.
    /// </summary>
    public GridRelRPG ToGridRel() => new(_x, _dLevel, _y);

    #endregion

    #region Operators - Arithmetic

    public static IsoRel operator +(IsoRel a, IsoRel b) =>
        new(a._x + b._x, a._y + b._y, a._dLevel + b._dLevel);

    public static IsoRel operator -(IsoRel a, IsoRel b) =>
        new(a._x - b._x, a._y - b._y, a._dLevel - b._dLevel);

    public static IsoRel operator -(IsoRel a) => a.Opposite();

    public static IsoRel operator *(IsoRel dir, int scalar) =>
        new(dir._x * scalar, dir._y * scalar, dir._dLevel * scalar);

    public static IsoRel operator *(int scalar, IsoRel dir) => dir * scalar;

    public static IsoRel operator *(IsoRel dir, TilesRPG tiles) =>
        new(
            Mathf.RoundToInt(dir._x * tiles.ToFloat()),
            Mathf.RoundToInt(dir._y * tiles.ToFloat()),
            dir._dLevel
        );

    public static IsoRel operator *(TilesRPG tiles, IsoRel dir) => dir * tiles;

    #endregion

    #region Operators - Comparison

    public static bool operator ==(IsoRel a, IsoRel b) =>
        a._x == b._x && a._y == b._y && a._dLevel == b._dLevel;

    public static bool operator !=(IsoRel a, IsoRel b) => !(a == b);

    #endregion

    #region Operators - Conversion

    public static implicit operator IsoRel((int x, int y) t) => new(t.x, t.y, 0);
    public static implicit operator IsoRel((int x, int y, int dLevel) t) => new(t.x, t.y, t.dLevel);
    public static implicit operator Vector3Int(IsoRel r) => r.ToVector3Int();

    #endregion

    #region IEquatable

    public bool Equals(IsoRel other) => this == other;
    public override bool Equals(object? obj) => obj is IsoRel other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_x, _y, _dLevel);

    #endregion

    #region IFormattable

    public override string ToString() {
        string levelPart = _dLevel switch {
            > 0 => $", +{_dLevel}↑",
            < 0 => $", {_dLevel}↓",
            _ => ""
        };
        return $"({_x}, {_y}{levelPart})";
    }

    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") return ToString();

        return format.ToLowerInvariant() switch {
            "n" or "name" => GetDirectionName(),
            "xy" => $"({_x}, {_y})",
            "v" => $"{_x}, {_y}, {_dLevel}",
            _ => ToString()
        };
    }

    private string GetDirectionName() {
        string horizontal = (_x, _y) switch {
            (0, 1) => "North",
            (1, 1) => "NorthEast",
            (1, 0) => "East",
            (1, -1) => "SouthEast",
            (0, -1) => "South",
            (-1, -1) => "SouthWest",
            (-1, 0) => "West",
            (-1, 1) => "NorthWest",
            (0, 0) => _dLevel == 0 ? "None" : "",
            _ => $"({_x}, {_y})"
        };

        string vertical = _dLevel switch {
            > 0 => "Up",
            < 0 => "Down",
            _ => ""
        };

        if (string.IsNullOrEmpty(horizontal)) return vertical;
        if (string.IsNullOrEmpty(vertical)) return horizontal;
        return $"{horizontal}{vertical}";
    }

    #endregion
}

#region Extension Methods

public static class IsoRelExtensions {
    /// <summary>
    /// Combines a horizontal offset with a level change.
    /// </summary>
    public static IsoRel WithLevelChange(this IsoRel rel, int dLevel) =>
        new(rel.X, rel.Y, dLevel);

    /// <summary>
    /// Creates an upward level offset.
    /// </summary>
    public static IsoRel Up(this int levels) => new(0, 0, levels);

    /// <summary>
    /// Creates a downward level offset.
    /// </summary>
    public static IsoRel Down(this int levels) => new(0, 0, -levels);
}

#endregion