using System;
using UnityEngine;

namespace RPGGame.Core.Isometric;

/// <summary>
/// An immutable distance measure in isometric 3D space, accounting for both 
/// horizontal tile distance and vertical level difference.
/// 
/// <para>
/// Useful for pathfinding costs, range checks, and movement calculations
/// where climbing/descending has different cost than horizontal movement.
/// </para>
/// 
/// <example>
/// <code>
/// IsoDistance dist = posA.DistanceTo(posB);
/// 
/// if (dist.HorizontalTiles &lt;= 5.Tiles() &amp;&amp; dist.VerticalLevels &lt;= 1) {
///     // Within attack range
/// }
/// 
/// float cost = dist.ToMovementCost(levelCostMultiplier: 2f);
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct IsoDistance : IEquatable<IsoDistance>, IComparable<IsoDistance>, IFormattable {
    #region Constants

    /// <summary>Default cost multiplier for vertical movement.</summary>
    public const float DEFAULT_LEVEL_COST = 2f;

    #endregion

    #region Fields

    private readonly TilesRPG _horizontalTiles;
    private readonly int _verticalLevels;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new IsoDistance with the specified horizontal and vertical components.
    /// </summary>
    /// <param name="horizontalTiles">Horizontal distance in tiles.</param>
    /// <param name="verticalLevels">Vertical distance in levels (absolute value taken).</param>
    public IsoDistance(TilesRPG horizontalTiles, int verticalLevels) {
        _horizontalTiles = horizontalTiles;
        _verticalLevels = Math.Abs(verticalLevels);
    }

    /// <summary>
    /// Creates a new IsoDistance from raw values.
    /// </summary>
    public IsoDistance(float horizontalTiles, int verticalLevels)
        : this(new TilesRPG(horizontalTiles), verticalLevels) { }

    #endregion

    #region Static Presets

    /// <summary>Zero distance.</summary>
    public static IsoDistance Zero => new(TilesRPG.Zero, 0);

    /// <summary>One tile horizontal.</summary>
    public static IsoDistance OneTile => new(TilesRPG.One, 0);

    /// <summary>One level vertical.</summary>
    public static IsoDistance OneLevel => new(TilesRPG.Zero, 1);

    /// <summary>Adjacent in 3D (1 tile and/or 1 level).</summary>
    public static IsoDistance Adjacent => new(TilesRPG.One, 1);

    /// <summary>Melee range (1 tile, same level).</summary>
    public static IsoDistance Melee => new(TilesRPG.One, 0);

    /// <summary>Short range (5 tiles).</summary>
    public static IsoDistance Short => new(5.Tiles(), 0);

    /// <summary>Medium range (10 tiles).</summary>
    public static IsoDistance Medium => new(10.Tiles(), 0);

    /// <summary>Long range (20 tiles).</summary>
    public static IsoDistance Long => new(20.Tiles(), 0);

    /// <summary>Infinite distance (for unreachable positions).</summary>
    public static IsoDistance Infinite => new(new TilesRPG(float.MaxValue), int.MaxValue);

    #endregion

    #region Properties

    /// <summary>Horizontal distance in tiles.</summary>
    public TilesRPG HorizontalTiles => _horizontalTiles;

    /// <summary>Vertical distance in levels (always non-negative).</summary>
    public int VerticalLevels => _verticalLevels;

    /// <summary>True if zero distance.</summary>
    public bool IsZero => _horizontalTiles.ToFloat() < 0.001f && _verticalLevels == 0;

    /// <summary>True if purely horizontal (no level change).</summary>
    public bool IsPurelyHorizontal => _verticalLevels == 0;

    /// <summary>True if purely vertical (no horizontal distance).</summary>
    public bool IsPurelyVertical => _horizontalTiles.ToFloat() < 0.001f && _verticalLevels > 0;

    /// <summary>True if this is an infinite/unreachable distance.</summary>
    public bool IsInfinite => _horizontalTiles.ToFloat() >= float.MaxValue / 2 || _verticalLevels >= int.MaxValue / 2;

    /// <summary>True if within melee range (1 tile, same or adjacent level).</summary>
    public bool IsMeleeRange => _horizontalTiles.ToInt() <= 1 && _verticalLevels <= 1;

    /// <summary>True if within touch range (0 tiles, same level).</summary>
    public bool IsTouchRange => _horizontalTiles.ToInt() == 0 && _verticalLevels == 0;

    /// <summary>True if on same level.</summary>
    public bool IsSameLevel => _verticalLevels == 0;

    /// <summary>The dominant component (horizontal tiles or vertical levels, whichever is greater).</summary>
    public float DominantComponent => Math.Max(_horizontalTiles.ToFloat(), _verticalLevels);

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates an IsoDistance from horizontal tiles only.
    /// </summary>
    public static IsoDistance FromTiles(TilesRPG tiles) => new(tiles, 0);

    /// <summary>
    /// Creates an IsoDistance from levels only.
    /// </summary>
    public static IsoDistance FromLevels(int levels) => new(TilesRPG.Zero, levels);

    /// <summary>
    /// Creates an IsoDistance from two positions.
    /// </summary>
    public static IsoDistance Between(IsoPos a, IsoPos b) => a.DistanceTo(b);

    /// <summary>
    /// Returns the minimum of two distances (component-wise).
    /// </summary>
    public static IsoDistance Min(IsoDistance a, IsoDistance b) => new(
        new TilesRPG(Math.Min(a._horizontalTiles.ToFloat(), b._horizontalTiles.ToFloat())),
        Math.Min(a._verticalLevels, b._verticalLevels)
    );

    /// <summary>
    /// Returns the maximum of two distances (component-wise).
    /// </summary>
    public static IsoDistance Max(IsoDistance a, IsoDistance b) => new(
        new TilesRPG(Math.Max(a._horizontalTiles.ToFloat(), b._horizontalTiles.ToFloat())),
        Math.Max(a._verticalLevels, b._verticalLevels)
    );

    #endregion

    #region Instance Methods - Calculations

    /// <summary>
    /// Calculates total movement cost with configurable level multiplier.
    /// </summary>
    /// <param name="levelCostMultiplier">Cost per level relative to tiles (default 2x).</param>
    /// <returns>Total cost as a float.</returns>
    public float ToMovementCost(float levelCostMultiplier = DEFAULT_LEVEL_COST) {
        if (IsInfinite) return float.MaxValue;
        return _horizontalTiles.ToFloat() + (_verticalLevels * levelCostMultiplier);
    }

    /// <summary>
    /// Calculates total movement cost as TilesRPG.
    /// </summary>
    public TilesRPG ToTotalTiles(float levelCostMultiplier = DEFAULT_LEVEL_COST) {
        if (IsInfinite) return new TilesRPG(float.MaxValue);
        return new TilesRPG(ToMovementCost(levelCostMultiplier));
    }

    /// <summary>
    /// Returns the 3D Euclidean distance (treating levels as height units).
    /// </summary>
    /// <param name="levelHeight">Height per level in tile-equivalent units.</param>
    public float ToEuclidean3D(float levelHeight = 1f) {
        if (IsInfinite) return float.MaxValue;
        float h = _horizontalTiles.ToFloat();
        float v = _verticalLevels * levelHeight;
        return Mathf.Sqrt(h * h + v * v);
    }

    /// <summary>
    /// Returns the 3D Manhattan distance.
    /// </summary>
    public float ToManhattan3D() {
        if (IsInfinite) return float.MaxValue;
        return _horizontalTiles.ToFloat() + _verticalLevels;
    }

    /// <summary>
    /// Checks if this distance is within a specified range.
    /// </summary>
    /// <param name="maxHorizontal">Maximum horizontal distance.</param>
    /// <param name="maxVertical">Maximum vertical distance.</param>
    public bool IsWithin(TilesRPG maxHorizontal, int maxVertical) =>
        _horizontalTiles <= maxHorizontal && _verticalLevels <= maxVertical;

    /// <summary>
    /// Checks if this distance is within another distance (component-wise).
    /// </summary>
    public bool IsWithin(IsoDistance max) =>
        _horizontalTiles <= max._horizontalTiles && _verticalLevels <= max._verticalLevels;

    /// <summary>
    /// Returns this distance clamped to a maximum.
    /// </summary>
    public IsoDistance Clamped(IsoDistance max) => new(
        new TilesRPG(Math.Min(_horizontalTiles.ToFloat(), max._horizontalTiles.ToFloat())),
        Math.Min(_verticalLevels, max._verticalLevels)
    );

    /// <summary>
    /// Returns this distance with only the horizontal component.
    /// </summary>
    public IsoDistance HorizontalOnly() => new(_horizontalTiles, 0);

    /// <summary>
    /// Returns this distance with only the vertical component.
    /// </summary>
    public IsoDistance VerticalOnly() => new(TilesRPG.Zero, _verticalLevels);

    #endregion

    #region Instance Methods - Range Categories

    /// <summary>
    /// Returns true if this distance is within the specified range category.
    /// </summary>
    public bool IsInRange(RangeCategory category) => category switch {
        RangeCategory.Touch => IsTouchRange,
        RangeCategory.Melee => IsMeleeRange,
        RangeCategory.Short => _horizontalTiles.ToInt() <= 5 && _verticalLevels <= 2,
        RangeCategory.Medium => _horizontalTiles.ToInt() <= 10 && _verticalLevels <= 3,
        RangeCategory.Long => _horizontalTiles.ToInt() <= 20 && _verticalLevels <= 5,
        RangeCategory.Extreme => _horizontalTiles.ToInt() <= 50 && _verticalLevels <= 10,
        _ => true
    };

    /// <summary>
    /// Gets the range category for this distance.
    /// </summary>
    public RangeCategory GetRangeCategory() {
        if (IsTouchRange) return RangeCategory.Touch;
        if (IsMeleeRange) return RangeCategory.Melee;
        if (_horizontalTiles.ToInt() <= 5) return RangeCategory.Short;
        if (_horizontalTiles.ToInt() <= 10) return RangeCategory.Medium;
        if (_horizontalTiles.ToInt() <= 20) return RangeCategory.Long;
        if (_horizontalTiles.ToInt() <= 50) return RangeCategory.Extreme;
        return RangeCategory.Unlimited;
    }

    #endregion

    #region Operators - Arithmetic

    public static IsoDistance operator +(IsoDistance a, IsoDistance b) => new(
        new TilesRPG(a._horizontalTiles.ToFloat() + b._horizontalTiles.ToFloat()),
        a._verticalLevels + b._verticalLevels
    );

    public static IsoDistance operator -(IsoDistance a, IsoDistance b) => new(
        new TilesRPG(Math.Max(0, a._horizontalTiles.ToFloat() - b._horizontalTiles.ToFloat())),
        Math.Max(0, a._verticalLevels - b._verticalLevels)
    );

    public static IsoDistance operator *(IsoDistance d, float scalar) => new(
        new TilesRPG(d._horizontalTiles.ToFloat() * scalar),
        Mathf.RoundToInt(d._verticalLevels * scalar)
    );

    public static IsoDistance operator *(float scalar, IsoDistance d) => d * scalar;

    public static IsoDistance operator /(IsoDistance d, float divisor) {
        if (divisor == 0) return Infinite;
        return new(
            new TilesRPG(d._horizontalTiles.ToFloat() / divisor),
            Mathf.RoundToInt(d._verticalLevels / divisor)
        );
    }

    #endregion

    #region Operators - Comparison

    public static bool operator ==(IsoDistance a, IsoDistance b) =>
        a._horizontalTiles == b._horizontalTiles && a._verticalLevels == b._verticalLevels;

    public static bool operator !=(IsoDistance a, IsoDistance b) => !(a == b);

    public static bool operator <(IsoDistance a, IsoDistance b) =>
        a.ToMovementCost() < b.ToMovementCost();

    public static bool operator >(IsoDistance a, IsoDistance b) =>
        a.ToMovementCost() > b.ToMovementCost();

    public static bool operator <=(IsoDistance a, IsoDistance b) =>
        a.ToMovementCost() <= b.ToMovementCost();

    public static bool operator >=(IsoDistance a, IsoDistance b) =>
        a.ToMovementCost() >= b.ToMovementCost();

    #endregion

    #region Operators - Conversion

    /// <summary>Creates an IsoDistance from a tuple.</summary>
    public static implicit operator IsoDistance((float tiles, int levels) t) => new(t.tiles, t.levels);

    /// <summary>Creates an IsoDistance from TilesRPG (horizontal only).</summary>
    public static implicit operator IsoDistance(TilesRPG tiles) => FromTiles(tiles);

    #endregion

    #region IEquatable / IComparable

    public bool Equals(IsoDistance other) => this == other;
    public override bool Equals(object? obj) => obj is IsoDistance other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_horizontalTiles, _verticalLevels);

    public int CompareTo(IsoDistance other) => ToMovementCost().CompareTo(other.ToMovementCost());

    #endregion

    #region IFormattable

    public override string ToString() {
        if (IsInfinite) return "∞";
        if (IsZero) return "0";

        if (IsPurelyHorizontal) return $"{_horizontalTiles.ToFloat():F1} tiles";
        if (IsPurelyVertical) return $"{_verticalLevels} level{(_verticalLevels != 1 ? "s" : "")}";

        return $"{_horizontalTiles.ToFloat():F1} tiles, {_verticalLevels} level{(_verticalLevels != 1 ? "s" : "")}";
    }

    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") return ToString();

        return format.ToLowerInvariant() switch {
            "h" or "horizontal" => $"{_horizontalTiles.ToFloat():F1}",
            "v" or "vertical" => _verticalLevels.ToString(),
            "cost" => ToMovementCost().ToString("F1"),
            "short" => IsInfinite ? "∞" : $"{_horizontalTiles.ToInt()}t{_verticalLevels}l",
            "range" => GetRangeCategory().ToString(),
            _ => ToString()
        };
    }

    #endregion
}

#region Range Category

/// <summary>
/// Standard range categories for abilities, spells, and attacks.
/// </summary>
public enum RangeCategory {
    /// <summary>Same tile (0 tiles).</summary>
    Touch,
    /// <summary>Adjacent tile (1 tile, ±1 level).</summary>
    Melee,
    /// <summary>Short range (≤5 tiles).</summary>
    Short,
    /// <summary>Medium range (≤10 tiles).</summary>
    Medium,
    /// <summary>Long range (≤20 tiles).</summary>
    Long,
    /// <summary>Extreme range (≤50 tiles).</summary>
    Extreme,
    /// <summary>Unlimited range.</summary>
    Unlimited
}

#endregion

#region Extension Methods

public static class IsoDistanceExtensions {
    /// <summary>
    /// Creates an IsoDistance with the specified tiles and levels.
    /// </summary>
    public static IsoDistance And(this TilesRPG tiles, int levels) => new(tiles, levels);

    /// <summary>
    /// Creates an IsoDistance from tiles with specified level tolerance.
    /// </summary>
    public static IsoDistance WithLevelTolerance(this TilesRPG tiles, int levels) => new(tiles, levels);

    /// <summary>
    /// Checks if within the specified iso distance.
    /// </summary>
    public static bool IsWithin(this IsoPos pos, IsoPos target, IsoDistance range) =>
        pos.DistanceTo(target).IsWithin(range);
}

#endregion