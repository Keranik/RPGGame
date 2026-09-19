using System;
using UnityEngine;

namespace RPGGame.Core.Isometric;

/// <summary>
/// An immutable wrapper for discrete Z-levels in an isometric world.
/// Represents vertical layers like underground, ground floor, upper floors, etc.
/// 
/// <para>Positive levels = above ground, Negative levels = below ground.</para>
/// 
/// <example>
/// <code>
/// IsoLevel ground = IsoLevel.Ground;           // Level 0
/// IsoLevel basement = IsoLevel.Underground1;   // Level -1
/// IsoLevel roof = ground + 1.Levels();         // Level 1
/// IsoLevel deep = 3.LevelsDown();              // Level -3
/// 
/// if (current.IsUnderground) { ApplyDarknessEffect(); }
/// if (current.IsAboveGround) { ApplySkyVisibility(); }
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct IsoLevel : IEquatable<IsoLevel>, IComparable<IsoLevel>, IFormattable {
    #region Constants

    /// <summary>Maximum supported level (prevents overflow in calculations).</summary>
    public const int MAX_LEVEL = 100;

    /// <summary>Minimum supported level (deepest underground).</summary>
    public const int MIN_LEVEL = -100;

    /// <summary>Height of one level in world units (configurable via theme/settings).</summary>
    public const float LEVEL_HEIGHT_UNITS = 2f;

    #endregion

    #region Fields

    private readonly int _level;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new IsoLevel from an integer level value.
    /// </summary>
    /// <param name="level">The level value (clamped to MIN_LEVEL..MAX_LEVEL).</param>
    public IsoLevel(int level) {
        _level = Math.Clamp(level, MIN_LEVEL, MAX_LEVEL);
    }

    #endregion

    #region Static Presets

    /// <summary>Ground level (0) - the primary surface level.</summary>
    public static IsoLevel Ground => new(0);

    /// <summary>First underground level (-1).</summary>
    public static IsoLevel Underground1 => new(-1);

    /// <summary>Second underground level (-2).</summary>
    public static IsoLevel Underground2 => new(-2);

    /// <summary>Third underground level (-3).</summary>
    public static IsoLevel Underground3 => new(-3);

    /// <summary>The abyss - deepest possible level.</summary>
    public static IsoLevel Abyss => new(MIN_LEVEL);

    /// <summary>First floor above ground (1).</summary>
    public static IsoLevel Floor1 => new(1);

    /// <summary>Second floor above ground (2).</summary>
    public static IsoLevel Floor2 => new(2);

    /// <summary>Roof/top level - typically for rooftops.</summary>
    public static IsoLevel Roof => new(3);

    /// <summary>Sky level - highest possible level.</summary>
    public static IsoLevel Sky => new(MAX_LEVEL);

    /// <summary>Invalid/unset level marker.</summary>
    public static IsoLevel Invalid => new(int.MinValue);

    #endregion

    #region Properties

    /// <summary>The raw integer level value.</summary>
    public int Value => _level;

    /// <summary>True if this is ground level (0).</summary>
    public bool IsGround => _level == 0;

    /// <summary>True if this level is underground (negative).</summary>
    public bool IsUnderground => _level < 0;

    /// <summary>True if this level is above ground (positive).</summary>
    public bool IsAboveGround => _level > 0;

    /// <summary>True if this level is at or above ground.</summary>
    public bool IsSurface => _level >= 0;

    /// <summary>True if this is the maximum level.</summary>
    public bool IsMaxLevel => _level == MAX_LEVEL;

    /// <summary>True if this is the minimum level.</summary>
    public bool IsMinLevel => _level == MIN_LEVEL;

    /// <summary>True if this is a valid level (not the Invalid marker).</summary>
    public bool IsValid => _level != int.MinValue && _level >= MIN_LEVEL && _level <= MAX_LEVEL;

    /// <summary>Returns the depth below ground (0 if at or above ground).</summary>
    public int Depth => _level < 0 ? -_level : 0;

    /// <summary>Returns the height above ground (0 if at or below ground).</summary>
    public int Height => _level > 0 ? _level : 0;

    /// <summary>Returns the absolute distance from ground level.</summary>
    public int DistanceFromGround => Math.Abs(_level);

    #endregion

    #region Instance Methods - Level Changes

    /// <summary>
    /// Returns a new IsoLevel one level up.
    /// </summary>
    public IsoLevel Up() => new(_level + 1);

    /// <summary>
    /// Returns a new IsoLevel one level down.
    /// </summary>
    public IsoLevel Down() => new(_level - 1);

    /// <summary>
    /// Returns a new IsoLevel offset by the specified amount.
    /// </summary>
    /// <param name="offset">Positive = up, Negative = down.</param>
    public IsoLevel Offset(int offset) => new(_level + offset);

    /// <summary>
    /// Returns this level clamped to a specific range.
    /// </summary>
    public IsoLevel Clamped(IsoLevel min, IsoLevel max) =>
        new(Math.Clamp(_level, min._level, max._level));

    /// <summary>
    /// Returns this level clamped to non-negative (surface and above).
    /// </summary>
    public IsoLevel ClampedToSurface() => new(Math.Max(0, _level));

    /// <summary>
    /// Returns this level clamped to underground only.
    /// </summary>
    public IsoLevel ClampedToUnderground() => new(Math.Min(0, _level));

    #endregion

    #region Instance Methods - Conversion

    /// <summary>
    /// Converts this level to a world Y coordinate.
    /// </summary>
    /// <param name="levelHeight">Height of one level in world units.</param>
    public float ToWorldY(float levelHeight = LEVEL_HEIGHT_UNITS) => _level * levelHeight;

    /// <summary>
    /// Creates an IsoLevel from a world Y coordinate.
    /// </summary>
    /// <param name="worldY">World Y coordinate.</param>
    /// <param name="levelHeight">Height of one level in world units.</param>
    public static IsoLevel FromWorldY(float worldY, float levelHeight = LEVEL_HEIGHT_UNITS) =>
        new(Mathf.RoundToInt(worldY / levelHeight));

    /// <summary>
    /// Returns a display name for this level.
    /// </summary>
    public string GetDisplayName() {
        return _level switch {
            < -3 => $"Deep Underground {-_level}",
            -3 => "Underground 3",
            -2 => "Underground 2",
            -1 => "Underground 1",
            0 => "Ground Floor",
            1 => "1st Floor",
            2 => "2nd Floor",
            3 => "Roof",
            > 3 => $"Level {_level}"
        };
    }

    /// <summary>
    /// Returns an icon/emoji for this level.
    /// </summary>
    public string GetIcon() {
        if (_level < -2) return "⛏️";      // Deep underground
        if (_level < 0) return "🕳️";       // Underground
        if (_level == 0) return "🏠";      // Ground
        if (_level <= 3) return "🏢";      // Building floors
        return "☁️";                        // Sky/high
    }

    #endregion

    #region Instance Methods - Queries

    /// <summary>
    /// Returns the signed level difference to another level.
    /// Positive if other is higher, negative if lower.
    /// </summary>
    public int DifferenceTo(IsoLevel other) => other._level - _level;

    /// <summary>
    /// Returns the absolute level difference to another level.
    /// </summary>
    public int DistanceTo(IsoLevel other) => Math.Abs(other._level - _level);

    /// <summary>
    /// Returns true if this level is within the specified range of another.
    /// </summary>
    public bool IsWithinRange(IsoLevel other, int range) => DistanceTo(other) <= range;

    /// <summary>
    /// Returns true if this level can see the target level (line of sight check).
    /// By default, you can see 1 level up/down without obstruction.
    /// </summary>
    /// <param name="target">Target level to check visibility to.</param>
    /// <param name="maxVisibleLevels">Maximum level difference for visibility.</param>
    public bool CanSee(IsoLevel target, int maxVisibleLevels = 1) =>
        DistanceTo(target) <= maxVisibleLevels;

    /// <summary>
    /// Returns true if this level is between (inclusive) two other levels.
    /// </summary>
    public bool IsBetween(IsoLevel min, IsoLevel max) =>
        _level >= min._level && _level <= max._level;

    /// <summary>
    /// Enumerates all levels from this level to another (inclusive).
    /// </summary>
    public System.Collections.Generic.IEnumerable<IsoLevel> To(IsoLevel target) {
        int step = target._level >= _level ? 1 : -1;
        for (int level = _level; level != target._level + step; level += step) {
            yield return new IsoLevel(level);
        }
    }

    #endregion

    #region Operators - Arithmetic

    /// <summary>Adds levels to this level.</summary>
    public static IsoLevel operator +(IsoLevel level, int offset) => level.Offset(offset);

    /// <summary>Subtracts levels from this level.</summary>
    public static IsoLevel operator -(IsoLevel level, int offset) => level.Offset(-offset);

    /// <summary>Returns the difference between two levels.</summary>
    public static int operator -(IsoLevel a, IsoLevel b) => a._level - b._level;

    /// <summary>Increments by one level.</summary>
    public static IsoLevel operator ++(IsoLevel level) => level.Up();

    /// <summary>Decrements by one level.</summary>
    public static IsoLevel operator --(IsoLevel level) => level.Down();

    #endregion

    #region Operators - Comparison

    public static bool operator ==(IsoLevel a, IsoLevel b) => a._level == b._level;
    public static bool operator !=(IsoLevel a, IsoLevel b) => a._level != b._level;
    public static bool operator <(IsoLevel a, IsoLevel b) => a._level < b._level;
    public static bool operator >(IsoLevel a, IsoLevel b) => a._level > b._level;
    public static bool operator <=(IsoLevel a, IsoLevel b) => a._level <= b._level;
    public static bool operator >=(IsoLevel a, IsoLevel b) => a._level >= b._level;

    #endregion

    #region Operators - Conversion

    /// <summary>Implicit conversion to int.</summary>
    public static implicit operator int(IsoLevel level) => level._level;

    /// <summary>Explicit conversion from int (use extension methods for clarity).</summary>
    public static explicit operator IsoLevel(int level) => new(level);

    #endregion

    #region IEquatable / IComparable

    public bool Equals(IsoLevel other) => _level == other._level;
    public override bool Equals(object? obj) => obj is IsoLevel other && Equals(other);
    public override int GetHashCode() => _level.GetHashCode();
    public int CompareTo(IsoLevel other) => _level.CompareTo(other._level);

    #endregion

    #region IFormattable

    public override string ToString() => _level switch {
        0 => "Ground",
        > 0 => $"+{_level}",
        < 0 => _level.ToString()
    };

    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") return ToString();

        return format.ToLowerInvariant() switch {
            "n" or "name" => GetDisplayName(),
            "i" or "icon" => GetIcon(),
            "v" or "value" => _level.ToString(),
            "s" or "signed" => _level >= 0 ? $"+{_level}" : _level.ToString(),
            "y" or "world" => ToWorldY().ToString("F1"),
            _ => ToString()
        };
    }

    #endregion
}

#region Extension Methods

/// <summary>
/// Extension methods for creating IsoLevel values fluently.
/// </summary>
public static class IsoLevelExtensions {
    /// <summary>
    /// Creates an IsoLevel from an integer.
    /// </summary>
    /// <example>
    /// <code>
    /// IsoLevel floor2 = 2.Level();
    /// IsoLevel basement = (-1).Level();
    /// </code>
    /// </example>
    public static IsoLevel Level(this int level) => new(level);

    /// <summary>
    /// Creates an IsoLevel representing the specified number of levels (for arithmetic).
    /// </summary>
    /// <example>
    /// <code>
    /// IsoLevel higher = current + 3.Levels();
    /// </code>
    /// </example>
    public static int Levels(this int count) => count;

    /// <summary>
    /// Creates an IsoLevel offset upward from ground.
    /// </summary>
    /// <example>
    /// <code>
    /// IsoLevel roof = 3.LevelsUp();  // Level 3
    /// </code>
    /// </example>
    public static IsoLevel LevelsUp(this int count) => new(count);

    /// <summary>
    /// Creates an IsoLevel offset downward from ground.
    /// </summary>
    /// <example>
    /// <code>
    /// IsoLevel basement = 2.LevelsDown();  // Level -2
    /// </code>
    /// </example>
    public static IsoLevel LevelsDown(this int count) => new(-count);

    /// <summary>
    /// Converts a world Y position to an IsoLevel.
    /// </summary>
    public static IsoLevel ToIsoLevel(this float worldY, float levelHeight = IsoLevel.LEVEL_HEIGHT_UNITS) =>
        IsoLevel.FromWorldY(worldY, levelHeight);
}

#endregion