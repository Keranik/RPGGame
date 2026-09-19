using System;
using UnityEngine;

namespace RPGGame.Core.Isometric;

/// <summary>
/// An immutable facing direction in isometric space.
/// Uses the 8 compass directions plus optional vertical component for 3D facing.
/// 
/// <para>
/// In isometric view, directions are rotated 45° from screen space:
/// - NE points to screen-right
/// - NW points to screen-up
/// - SE points to screen-down
/// - SW points to screen-left
/// </para>
/// 
/// <example>
/// <code>
/// IsoAngle facing = IsoAngle.NorthEast;
/// IsoAngle turned = facing.TurnRight();           // East
/// IsoAngle opposite = facing.Opposite();          // SouthWest
/// IsoRel moveDir = facing.ToDirection();          // (1, 1, 0)
/// 
/// // For sprites
/// int spriteIndex = facing.ToSpriteIndex();       // 0-7
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct IsoAngle : IEquatable<IsoAngle>, IFormattable {
    #region Constants

    /// <summary>Number of horizontal directions.</summary>
    public const int DIRECTION_COUNT = 8;

    #endregion

    #region Fields

    // Stored as octant index (0-7) for efficiency
    // 0=N, 1=NE, 2=E, 3=SE, 4=S, 5=SW, 6=W, 7=NW
    private readonly int _octant;
    private readonly int _pitch; // -1=down, 0=level, 1=up

    #endregion

    #region Constructors

    /// <summary>
    /// Creates an IsoAngle from an octant index.
    /// </summary>
    /// <param name="octant">Octant index (0-7, 0=North).</param>
    /// <param name="pitch">Vertical pitch (-1=down, 0=level, 1=up).</param>
    public IsoAngle(int octant, int pitch = 0) {
        _octant = ((octant % DIRECTION_COUNT) + DIRECTION_COUNT) % DIRECTION_COUNT;
        _pitch = Math.Clamp(pitch, -1, 1);
    }

    #endregion

    #region Static Presets - Cardinal

    /// <summary>North (up on map, screen up-right in iso).</summary>
    public static IsoAngle North => new(0);

    /// <summary>East (right on map, screen down-right in iso).</summary>
    public static IsoAngle East => new(2);

    /// <summary>South (down on map, screen down-left in iso).</summary>
    public static IsoAngle South => new(4);

    /// <summary>West (left on map, screen up-left in iso).</summary>
    public static IsoAngle West => new(6);

    #endregion

    #region Static Presets - Diagonal (Primary in Isometric)

    /// <summary>NorthEast (screen right in iso view).</summary>
    public static IsoAngle NorthEast => new(1);

    /// <summary>SouthEast (screen down in iso view).</summary>
    public static IsoAngle SouthEast => new(3);

    /// <summary>SouthWest (screen left in iso view).</summary>
    public static IsoAngle SouthWest => new(5);

    /// <summary>NorthWest (screen up in iso view).</summary>
    public static IsoAngle NorthWest => new(7);

    #endregion

    #region Static Presets - With Pitch

    /// <summary>North looking up.</summary>
    public static IsoAngle NorthUp => new(0, 1);

    /// <summary>North looking down.</summary>
    public static IsoAngle NorthDown => new(0, -1);

    /// <summary>East looking up.</summary>
    public static IsoAngle EastUp => new(2, 1);

    /// <summary>East looking down.</summary>
    public static IsoAngle EastDown => new(2, -1);

    #endregion

    #region Static Collections

    /// <summary>All 8 horizontal directions in clockwise order starting from North.</summary>
    public static IsoAngle[] AllDirections { get; } = [
        North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
    ];

    /// <summary>The 4 cardinal directions.</summary>
    public static IsoAngle[] CardinalDirections { get; } = [North, East, South, West];

    /// <summary>The 4 diagonal directions (primary directions in isometric view).</summary>
    public static IsoAngle[] DiagonalDirections { get; } = [NorthEast, SouthEast, SouthWest, NorthWest];

    /// <summary>The 4 isometric screen-aligned directions.</summary>
    public static IsoAngle[] ScreenDirections { get; } = DiagonalDirections;

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates an IsoAngle from an AngleRPG.
    /// </summary>
    public static IsoAngle FromAngle(AngleRPG angle) {
        float deg = angle.ToDegrees();
        // Convert to octant (0=North at 90°, clockwise)
        int octant = Mathf.RoundToInt((90f - deg) / 45f);
        return new IsoAngle(octant);
    }

    /// <summary>
    /// Creates an IsoAngle from a direction vector.
    /// </summary>
    public static IsoAngle FromDirection(Vector2 direction) {
        if (direction.sqrMagnitude < 0.001f) return North;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return FromAngle(new AngleRPG(angle));
    }

    /// <summary>
    /// Creates an IsoAngle from an IsoRel.
    /// </summary>
    public static IsoAngle FromIsoRel(IsoRel rel) {
        int pitch = Math.Sign(rel.DLevel);
        return new IsoAngle(GetOctantFromXY(rel.X, rel.Y), pitch);
    }

    /// <summary>
    /// Returns a random direction.
    /// </summary>
    public static IsoAngle Random() => new(UnityEngine.Random.Range(0, 8));

    /// <summary>
    /// Returns a random cardinal direction.
    /// </summary>
    public static IsoAngle RandomCardinal() => CardinalDirections[UnityEngine.Random.Range(0, 4)];

    /// <summary>
    /// Returns a random diagonal direction.
    /// </summary>
    public static IsoAngle RandomDiagonal() => DiagonalDirections[UnityEngine.Random.Range(0, 4)];

    private static int GetOctantFromXY(int x, int y) {
        return (x, y) switch {
            (0, > 0) => 0,    // North
            (> 0, > 0) => 1,  // NorthEast
            (> 0, 0) => 2,    // East
            (> 0, < 0) => 3,  // SouthEast
            (0, < 0) => 4,    // South
            (< 0, < 0) => 5,  // SouthWest
            (< 0, 0) => 6,    // West
            (< 0, > 0) => 7,  // NorthWest
            _ => 0            // Default to North
        };
    }

    #endregion

    #region Properties

    /// <summary>The octant index (0-7, 0=North, clockwise).</summary>
    public int Octant => _octant;

    /// <summary>Vertical pitch (-1=down, 0=level, 1=up).</summary>
    public int Pitch => _pitch;

    /// <summary>True if this is a cardinal direction (N/E/S/W).</summary>
    public bool IsCardinal => _octant % 2 == 0;

    /// <summary>True if this is a diagonal direction (NE/SE/SW/NW).</summary>
    public bool IsDiagonal => _octant % 2 == 1;

    /// <summary>True if looking up.</summary>
    public bool IsLookingUp => _pitch > 0;

    /// <summary>True if looking down.</summary>
    public bool IsLookingDown => _pitch < 0;

    /// <summary>True if looking level (no pitch).</summary>
    public bool IsLevel => _pitch == 0;

    /// <summary>The angle in degrees (0=East, 90=North, counter-clockwise).</summary>
    public float Degrees => 90f - (_octant * 45f);

    #endregion

    #region Instance Methods - Rotation

    /// <summary>
    /// Turns right (clockwise) by the specified number of steps (each step = 45°).
    /// </summary>
    public IsoAngle TurnRight(int steps = 1) => new((_octant + steps) % DIRECTION_COUNT, _pitch);

    /// <summary>
    /// Turns left (counter-clockwise) by the specified number of steps.
    /// </summary>
    public IsoAngle TurnLeft(int steps = 1) => TurnRight(-steps);

    /// <summary>
    /// Returns the opposite direction (180° turn).
    /// </summary>
    public IsoAngle Opposite() => new((_octant + 4) % DIRECTION_COUNT, -_pitch);

    /// <summary>
    /// Rotates by 90° clockwise.
    /// </summary>
    public IsoAngle Rotate90CW() => TurnRight(2);

    /// <summary>
    /// Rotates by 90° counter-clockwise.
    /// </summary>
    public IsoAngle Rotate90CCW() => TurnLeft(2);

    /// <summary>
    /// Rotates by 45° clockwise.
    /// </summary>
    public IsoAngle Rotate45CW() => TurnRight(1);

    /// <summary>
    /// Rotates by 45° counter-clockwise.
    /// </summary>
    public IsoAngle Rotate45CCW() => TurnLeft(1);

    /// <summary>
    /// Returns this direction with level pitch.
    /// </summary>
    public IsoAngle Level() => new(_octant, 0);

    /// <summary>
    /// Returns this direction looking up.
    /// </summary>
    public IsoAngle LookUp() => new(_octant, 1);

    /// <summary>
    /// Returns this direction looking down.
    /// </summary>
    public IsoAngle LookDown() => new(_octant, -1);

    /// <summary>
    /// Returns the nearest cardinal direction.
    /// </summary>
    public IsoAngle ToCardinal() {
        int cardinal = ((_octant + 1) / 2) * 2;
        return new IsoAngle(cardinal % DIRECTION_COUNT, _pitch);
    }

    /// <summary>
    /// Returns the nearest diagonal direction.
    /// </summary>
    public IsoAngle ToDiagonal() {
        int diagonal = (_octant / 2) * 2 + 1;
        return new IsoAngle(diagonal % DIRECTION_COUNT, _pitch);
    }

    #endregion

    #region Instance Methods - Conversion

    /// <summary>
    /// Converts to an IsoRel unit direction.
    /// </summary>
    public IsoRel ToDirection() {
        var (x, y) = GetXYFromOctant(_octant);
        return new IsoRel(x, y, _pitch);
    }

    /// <summary>
    /// Converts to an AngleRPG.
    /// </summary>
    public AngleRPG ToAngleRPG() => new AngleRPG(Degrees);

    /// <summary>
    /// Converts to a unit Vector2 direction.
    /// </summary>
    public Vector2 ToVector2() {
        float rad = Degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    /// <summary>
    /// Converts to a unit Vector3 direction (XZ plane with Y for pitch).
    /// </summary>
    public Vector3 ToVector3() {
        var (x, y) = GetXYFromOctant(_octant);
        float mag = (x != 0 && y != 0) ? 0.707f : 1f; // Normalize diagonals
        return new Vector3(x * mag, _pitch * 0.5f, y * mag);
    }

    /// <summary>
    /// Returns the sprite index for 8-directional sprites (0=N, 1=NE, ..., 7=NW).
    /// </summary>
    public int ToSpriteIndex() => _octant;

    /// <summary>
    /// Returns the sprite index for 4-directional sprites (0=N, 1=E, 2=S, 3=W).
    /// </summary>
    public int ToSpriteIndex4() => ((_octant + 1) / 2) % 4;

    /// <summary>
    /// Converts to screen-space direction for isometric rendering.
    /// </summary>
    public Vector2 ToScreenDirection() {
        // In isometric 2:1, directions are rotated
        return _octant switch {
            0 => new Vector2(0.5f, 0.25f),    // North -> up-right
            1 => new Vector2(1f, 0f),          // NorthEast -> right
            2 => new Vector2(0.5f, -0.25f),   // East -> down-right
            3 => new Vector2(0f, -0.5f),       // SouthEast -> down
            4 => new Vector2(-0.5f, -0.25f),  // South -> down-left
            5 => new Vector2(-1f, 0f),         // SouthWest -> left
            6 => new Vector2(-0.5f, 0.25f),   // West -> up-left
            7 => new Vector2(0f, 0.5f),        // NorthWest -> up
            _ => Vector2.zero
        };
    }

    private static (int x, int y) GetXYFromOctant(int octant) {
        return octant switch {
            0 => (0, 1),    // North
            1 => (1, 1),    // NorthEast
            2 => (1, 0),    // East
            3 => (1, -1),   // SouthEast
            4 => (0, -1),   // South
            5 => (-1, -1),  // SouthWest
            6 => (-1, 0),   // West
            7 => (-1, 1),   // NorthWest
            _ => (0, 0)
        };
    }

    #endregion

    #region Instance Methods - Queries

    /// <summary>
    /// Returns the angular difference to another direction (in 45° steps, -4 to 4).
    /// </summary>
    public int StepsTo(IsoAngle other) {
        int diff = other._octant - _octant;
        if (diff > 4) diff -= 8;
        if (diff < -4) diff += 8;
        return diff;
    }

    /// <summary>
    /// Returns true if facing the same general direction (within 45°).
    /// </summary>
    public bool IsFacing(IsoAngle other) => Math.Abs(StepsTo(other)) <= 1;

    /// <summary>
    /// Returns true if facing opposite directions (within 45° of opposite).
    /// </summary>
    public bool IsOpposite(IsoAngle other) => Math.Abs(StepsTo(other.Opposite())) <= 1;

    /// <summary>
    /// Returns the direction from one position to another.
    /// </summary>
    public static IsoAngle FromTo(IsoPos from, IsoPos to) {
        var rel = from.RelativeTo(to);
        return FromIsoRel(rel);
    }

    #endregion

    #region Operators

    public static bool operator ==(IsoAngle a, IsoAngle b) =>
        a._octant == b._octant && a._pitch == b._pitch;

    public static bool operator !=(IsoAngle a, IsoAngle b) => !(a == b);

    /// <summary>Rotates right by the specified steps.</summary>
    public static IsoAngle operator +(IsoAngle angle, int steps) => angle.TurnRight(steps);

    /// <summary>Rotates left by the specified steps.</summary>
    public static IsoAngle operator -(IsoAngle angle, int steps) => angle.TurnLeft(steps);

    /// <summary>Multiplies direction by distance to get an IsoRel.</summary>
    public static IsoRel operator *(IsoAngle angle, int distance) =>
        angle.ToDirection() * distance;

    /// <summary>Multiplies direction by distance to get an IsoRel.</summary>
    public static IsoRel operator *(IsoAngle angle, TilesRPG distance) =>
        angle.ToDirection() * distance;

    #endregion

    #region IEquatable

    public bool Equals(IsoAngle other) => this == other;
    public override bool Equals(object? obj) => obj is IsoAngle other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_octant, _pitch);

    #endregion

    #region IFormattable

    public override string ToString() => GetDirectionName();

    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") return ToString();

        return format.ToLowerInvariant() switch {
            "s" or "short" => GetShortName(),
            "i" or "index" => _octant.ToString(),
            "d" or "degrees" => $"{Degrees:F0}°",
            "arrow" => GetArrowSymbol(),
            _ => ToString()
        };
    }

    private string GetDirectionName() {
        string horizontal = _octant switch {
            0 => "North",
            1 => "NorthEast",
            2 => "East",
            3 => "SouthEast",
            4 => "South",
            5 => "SouthWest",
            6 => "West",
            7 => "NorthWest",
            _ => "?"
        };

        if (_pitch == 0) return horizontal;
        return $"{horizontal}{(_pitch > 0 ? "Up" : "Down")}";
    }

    private string GetShortName() {
        string h = _octant switch {
            0 => "N", 1 => "NE", 2 => "E", 3 => "SE",
            4 => "S", 5 => "SW", 6 => "W", 7 => "NW",
            _ => "?"
        };
        if (_pitch == 0) return h;
        return $"{h}{(_pitch > 0 ? "↑" : "↓")}";
    }

    private string GetArrowSymbol() => _octant switch {
        0 => "↑", 1 => "↗", 2 => "→", 3 => "↘",
        4 => "↓", 5 => "↙", 6 => "←", 7 => "↖",
        _ => "•"
    };

    #endregion
}

#region Extension Methods

public static class IsoAngleExtensions {
    /// <summary>
    /// Creates an IsoAngle facing the target from the source.
    /// </summary>
    public static IsoAngle FacingTo(this IsoPos source, IsoPos target) =>
        IsoAngle.FromTo(source, target);

    /// <summary>
    /// Returns the position in the specified direction at the given distance.
    /// </summary>
    public static IsoPos InDirection(this IsoPos pos, IsoAngle direction, int distance = 1) =>
        pos + (direction.ToDirection() * distance);
}

#endregion