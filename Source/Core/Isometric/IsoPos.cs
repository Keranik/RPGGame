using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGGame.Core.Isometric;

/// <summary>
/// An immutable absolute position in isometric 3D space with discrete X, Y (horizontal) 
/// and Z-level (vertical layer).
/// 
/// <para>
/// Uses (X, Y) for horizontal position on the isometric plane and IsoLevel for vertical layers.
/// This separates the discrete level system from continuous world height.
/// </para>
/// 
/// <example>
/// <code>
/// IsoPos origin = IsoPos.Origin;
/// IsoPos pos = new IsoPos(5, 3, IsoLevel.Ground);
/// IsoPos underground = pos.WithLevel(IsoLevel.Underground1);
/// IsoPos moved = pos + IsoRel.NorthEast;
/// 
/// Vector3 world = pos.ToWorld();
/// Vector2 screen = pos.ToScreen();
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct IsoPos : IEquatable<IsoPos>, IFormattable {
    #region Constants

    /// <summary>Isometric projection angle (typically 26.565° for 2:1 ratio).</summary>
    public const float ISO_ANGLE_DEGREES = 26.565f;

    /// <summary>Tile width in screen pixels (configurable).</summary>
    public const float DEFAULT_TILE_WIDTH = 64f;

    /// <summary>Tile height in screen pixels (half of width for 2:1 isometric).</summary>
    public const float DEFAULT_TILE_HEIGHT = 32f;

    /// <summary>Height per level in screen pixels.</summary>
    public const float DEFAULT_LEVEL_HEIGHT = 48f;

    #endregion

    #region Fields

    private readonly int _x;
    private readonly int _y;
    private readonly IsoLevel _level;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new IsoPos at the specified coordinates and level.
    /// </summary>
    /// <param name="x">X coordinate (East-West axis).</param>
    /// <param name="y">Y coordinate (North-South axis on iso plane).</param>
    /// <param name="level">Vertical level.</param>
    public IsoPos(int x, int y, IsoLevel level) {
        _x = x;
        _y = y;
        _level = level;
    }

    /// <summary>
    /// Creates a new IsoPos at ground level.
    /// </summary>
    public IsoPos(int x, int y) : this(x, y, IsoLevel.Ground) { }

    #endregion

    #region Static Presets

    /// <summary>Origin position at ground level (0, 0, Ground).</summary>
    public static IsoPos Origin => new(0, 0, IsoLevel.Ground);

    /// <summary>Invalid/unset position marker.</summary>
    public static IsoPos Invalid => new(int.MinValue, int.MinValue, IsoLevel.Invalid);

    #endregion

    #region Properties

    /// <summary>X coordinate (East-West).</summary>
    public int X => _x;

    /// <summary>Y coordinate (North-South on iso plane).</summary>
    public int Y => _y;

    /// <summary>Vertical level.</summary>
    public IsoLevel Level => _level;

    /// <summary>True if at origin (0, 0, Ground).</summary>
    public bool IsOrigin => _x == 0 && _y == 0 && _level.IsGround;

    /// <summary>True if this position is valid.</summary>
    public bool IsValid => _level.IsValid && _x != int.MinValue && _y != int.MinValue;

    /// <summary>True if at ground level.</summary>
    public bool IsGroundLevel => _level.IsGround;

    /// <summary>True if underground.</summary>
    public bool IsUnderground => _level.IsUnderground;

    /// <summary>True if above ground.</summary>
    public bool IsAboveGround => _level.IsAboveGround;

    /// <summary>Returns the 2D position (X, Y) as a Vector2Int.</summary>
    public Vector2Int XY => new(_x, _y);

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates an IsoPos from a Vector3Int (x, y = iso Y, z = level).
    /// </summary>
    public static IsoPos FromVector3Int(Vector3Int v) => new(v.x, v.y, new IsoLevel(v.z));

    /// <summary>
    /// Creates an IsoPos from a Vector2Int at ground level.
    /// </summary>
    public static IsoPos FromVector2Int(Vector2Int v) => new(v.x, v.y, IsoLevel.Ground);

    /// <summary>
    /// Creates an IsoPos from screen coordinates using isometric projection.
    /// </summary>
    /// <param name="screenPos">Screen position in pixels.</param>
    /// <param name="tileWidth">Width of one tile in pixels.</param>
    /// <param name="tileHeight">Height of one tile in pixels.</param>
    /// <param name="level">Assumed level for the conversion.</param>
    public static IsoPos FromScreen(Vector2 screenPos, float tileWidth = DEFAULT_TILE_WIDTH,
        float tileHeight = DEFAULT_TILE_HEIGHT, IsoLevel level = default) {
        // Inverse isometric projection
        float isoX = (screenPos.x / (tileWidth / 2f) + screenPos.y / (tileHeight / 2f)) / 2f;
        float isoY = (screenPos.y / (tileHeight / 2f) - screenPos.x / (tileWidth / 2f)) / 2f;

        return new IsoPos(Mathf.RoundToInt(isoX), Mathf.RoundToInt(isoY), level);
    }

    /// <summary>
    /// Creates an IsoPos from world coordinates (simple grid, no isometric skew).
    /// </summary>
    /// <param name="worldPos">World position.</param>
    /// <param name="tileSize">Size of one tile in world units.</param>
    /// <param name="levelHeight">Height of one level in world units.</param>
    public static IsoPos FromWorld(Vector3 worldPos, float tileSize = 1f, float levelHeight = 1f) {
        int x = Mathf.FloorToInt(worldPos.x / tileSize);
        int y = Mathf.FloorToInt(worldPos.z / tileSize);  // World Z maps to grid Y
        var level = IsoLevel.FromWorldY(worldPos.y, levelHeight);
        return new IsoPos(x, y, level);
    }

    /// <summary>
    /// Creates an IsoPos from world coordinates using isometric projection (diamond grid).
    /// </summary>
    /// <param name="worldPos">World position.</param>
    /// <param name="tileWidth">Width of one tile in world units.</param>
    /// <param name="tileHeight">Height of one tile in world units (half of width for 2:1).</param>
    /// <param name="levelHeight">Height of one level in world units.</param>
    public static IsoPos FromWorldIsometric(Vector3 worldPos, float tileWidth = 1f,
        float tileHeight = 0.5f, float levelHeight = 1f) {
        // Inverse isometric projection
        float isoX = (worldPos.x / (tileWidth / 2f) + worldPos.z / (tileHeight / 2f)) / 2f;
        float isoY = (worldPos.z / (tileHeight / 2f) - worldPos.x / (tileWidth / 2f)) / 2f;
        var level = IsoLevel.FromWorldY(worldPos.y, levelHeight);
        return new IsoPos(Mathf.RoundToInt(isoX), Mathf.RoundToInt(isoY), level);
    }

    /// <summary>
    /// Returns the component-wise minimum of two positions.
    /// </summary>
    public static IsoPos Min(IsoPos a, IsoPos b) => new(
        Math.Min(a._x, b._x),
        Math.Min(a._y, b._y),
        new IsoLevel(Math.Min(a._level.Value, b._level.Value))
    );

    /// <summary>
    /// Returns the component-wise maximum of two positions.
    /// </summary>
    public static IsoPos Max(IsoPos a, IsoPos b) => new(
        Math.Max(a._x, b._x),
        Math.Max(a._y, b._y),
        new IsoLevel(Math.Max(a._level.Value, b._level.Value))
    );

    #endregion

    #region Instance Methods - Conversion

    /// <summary>
    /// Converts to screen coordinates for rendering (2D isometric projection).
    /// </summary>
    /// <param name="tileWidth">Width of one tile in pixels.</param>
    /// <param name="tileHeight">Height of one tile in pixels.</param>
    /// <param name="levelHeight">Screen height offset per level.</param>
    public Vector2 ToScreen(float tileWidth = DEFAULT_TILE_WIDTH, float tileHeight = DEFAULT_TILE_HEIGHT,
        float levelHeight = DEFAULT_LEVEL_HEIGHT) {
        // Standard isometric projection (2:1 diamond)
        float screenX = (_x - _y) * (tileWidth / 2f);
        float screenY = (_x + _y) * (tileHeight / 2f) - (_level.Value * levelHeight);
        return new Vector2(screenX, screenY);
    }

    /// <summary>
    /// Converts to 3D world coordinates using diamond isometric projection.
    /// Use this for 2D sprite-based isometric rendering where sprites are pre-rendered diamonds.
    /// X = left/right, Y = up/down (height), Z = forward/back (depth).
    /// </summary>
    /// <param name="tileWidth">Screen width of one tile.</param>
    /// <param name="tileHeight">Screen height of one tile (half of width for 2:1 ratio).</param>
    /// <param name="levelHeight">World height per level.</param>
    /// <returns>World position with isometric skew applied.</returns>
    public Vector3 ToWorld(float tileWidth = 1f, float tileHeight = 0.5f, float levelHeight = 1f) {
        float worldX = (_x - _y) * (tileWidth / 2f);
        float worldZ = (_x + _y) * (tileHeight / 2f);
        float worldY = _level.Value * levelHeight;
        return new Vector3(worldX, worldY, worldZ);
    }

    /// <summary>
    /// Converts to 3D world coordinates using simple grid positioning (no isometric skew).
    /// Use this when the camera angle provides the isometric look (3D orthographic camera).
    /// Grid X maps to World X, Grid Y maps to World Z, Level maps to World Y.
    /// </summary>
    /// <param name="tileSize">Size of one tile in world units.</param>
    /// <param name="levelHeight">Height of one level in world units.</param>
    /// <returns>World position on a flat grid.</returns>
	public Vector3 ToWorldGrid(TilesRPG tileSize, TilesRPG levelHeight) {
		float tileSizeUnits = tileSize.Value;
		float levelHeightUnits = levelHeight.Value;

		float worldX = _x * tileSizeUnits;
		float worldZ = _y * tileSizeUnits;
		float worldY = _level.Value * levelHeightUnits;

		return new Vector3(worldX, worldY, worldZ);
	}

    /// <summary>
    /// Converts to 3D world coordinates at the CENTER of the tile (simple grid).
    /// </summary>
    /// <param name="tileSize">Size of one tile in world units.</param>
    /// <param name="levelHeight">Height of one level in world units.</param>
    /// <returns>World position at tile center.</returns>
	public Vector3 ToWorldGridCenter(TilesRPG tileSize, TilesRPG levelHeight) {
		float tileSizeUnits = tileSize.Value;
		float halfTile = tileSizeUnits * 0.5f;
		float levelHeightUnits = levelHeight.Value;

		float worldX = _x * tileSizeUnits + halfTile;
		float worldZ = _y * tileSizeUnits + halfTile;
		float worldY = _level.Value * levelHeightUnits;

		return new Vector3(worldX, worldY, worldZ);
	}

    /// <summary>
    /// Converts to world coordinates at the center of the tile.
    /// </summary>
    public Vector3 ToWorldCenter(float levelHeight = IsoLevel.LEVEL_HEIGHT_UNITS) {
        return new Vector3(_x + 0.5f, _level.ToWorldY(levelHeight), _y + 0.5f);
    }

    /// <summary>
    /// Converts to a Vector3Int (x, y, level as z).
    /// </summary>
    public Vector3Int ToVector3Int() => new(_x, _y, _level.Value);

    /// <summary>
    /// Converts to a Vector2Int (x, y only).
    /// </summary>
    public Vector2Int ToVector2Int() => new(_x, _y);

    /// <summary>
    /// Converts to a GridPosRPG for 2D grid operations.
    /// </summary>
    public GridPosRPG ToGridPos() => new(_x, _level.Value, _y);

    #endregion

    #region Instance Methods - Position Changes

    /// <summary>
    /// Returns a new IsoPos at the specified level.
    /// </summary>
    public IsoPos WithLevel(IsoLevel level) => new(_x, _y, level);

    /// <summary>
    /// Returns a new IsoPos at ground level.
    /// </summary>
    public IsoPos AtGround() => WithLevel(IsoLevel.Ground);

    /// <summary>
    /// Returns a new IsoPos one level up.
    /// </summary>
    public IsoPos Up() => new(_x, _y, _level.Up());

    /// <summary>
    /// Returns a new IsoPos one level down.
    /// </summary>
    public IsoPos Down() => new(_x, _y, _level.Down());

    /// <summary>
    /// Returns a new IsoPos offset by the given amounts.
    /// </summary>
    public IsoPos Offset(int dx, int dy, int dLevel = 0) =>
        new(_x + dx, _y + dy, _level.Offset(dLevel));

    /// <summary>
    /// Returns a new IsoPos offset by the given IsoRel.
    /// </summary>
    public IsoPos Offset(IsoRel offset) =>
        new(_x + offset.X, _y + offset.Y, _level.Offset(offset.DLevel));

    /// <summary>
    /// Returns this position with X modified.
    /// </summary>
    public IsoPos WithX(int x) => new(x, _y, _level);

    /// <summary>
    /// Returns this position with Y modified.
    /// </summary>
    public IsoPos WithY(int y) => new(_x, y, _level);

    /// <summary>
    /// Returns this position clamped to the specified bounds.
    /// </summary>
    public IsoPos Clamped(IsoPos min, IsoPos max) => new(
        Math.Clamp(_x, min._x, max._x),
        Math.Clamp(_y, min._y, max._y),
        _level.Clamped(min._level, max._level)
    );

    /// <summary>
    /// Returns this position clamped to non-negative coordinates.
    /// </summary>
    public IsoPos ClampedPositive() => new(
        Math.Max(0, _x),
        Math.Max(0, _y),
        _level
    );

    #endregion

    #region Instance Methods - Relative Positions

    /// <summary>
    /// Returns the relative offset from this position to another.
    /// </summary>
    public IsoRel RelativeTo(IsoPos other) =>
        new(other._x - _x, other._y - _y, other._level - _level);

    /// <summary>
    /// Returns the direction from this position to another.
    /// </summary>
    public IsoRel DirectionTo(IsoPos other) => RelativeTo(other).Normalized();

    /// <summary>
    /// Returns the 8-way horizontal direction from this position to another.
    /// </summary>
    public IsoRel DirectionTo8Way(IsoPos other) {
        var rel = RelativeTo(other);
        return IsoRel.FromAngle(rel.ToAngle());
    }

    #endregion

    #region Instance Methods - Distance

    /// <summary>
    /// Calculates the Manhattan distance on the horizontal plane.
    /// </summary>
    public TilesRPG ManhattanDistanceXY(IsoPos other) =>
        new TilesRPG(Math.Abs(_x - other._x) + Math.Abs(_y - other._y));

    /// <summary>
    /// Calculates the Chebyshev distance on the horizontal plane.
    /// </summary>
    public TilesRPG ChebyshevDistanceXY(IsoPos other) =>
        new TilesRPG(Math.Max(Math.Abs(_x - other._x), Math.Abs(_y - other._y)));

    /// <summary>
    /// Calculates the full 3D Manhattan distance including level difference.
    /// </summary>
    public IsoDistance DistanceTo(IsoPos other) =>
        new IsoDistance(ManhattanDistanceXY(other), _level.DistanceTo(other._level));

    /// <summary>
    /// Returns true if this position is within range of another (horizontal only).
    /// </summary>
    public bool IsWithinRangeXY(IsoPos other, TilesRPG range) =>
        ChebyshevDistanceXY(other) <= range;

    /// <summary>
    /// Returns true if this position is within range including level difference.
    /// </summary>
    public bool IsWithinRange(IsoPos other, IsoDistance range) =>
        DistanceTo(other) <= range;

    /// <summary>
    /// Returns true if adjacent horizontally (8-way, same level).
    /// </summary>
    public bool IsAdjacentXY(IsoPos other) =>
        _level == other._level && ChebyshevDistanceXY(other).ToInt() == 1;

    /// <summary>
    /// Returns true if adjacent including vertical (can be one level up/down).
    /// </summary>
    public bool IsAdjacent3D(IsoPos other) {
        var dist = DistanceTo(other);
        return dist.HorizontalTiles.ToInt() <= 1 && dist.VerticalLevels <= 1;
    }

    #endregion

    #region Instance Methods - Neighbors

    /// <summary>
    /// Returns the 4 cardinal neighbors on the same level.
    /// </summary>
    public IsoPos[] Neighbors4() => [
        this + IsoRel.North,
        this + IsoRel.East,
        this + IsoRel.South,
        this + IsoRel.West
    ];

    /// <summary>
    /// Returns the 8 compass neighbors on the same level.
    /// </summary>
    public IsoPos[] Neighbors8() => [
        this + IsoRel.North,
        this + IsoRel.NorthEast,
        this + IsoRel.East,
        this + IsoRel.SouthEast,
        this + IsoRel.South,
        this + IsoRel.SouthWest,
        this + IsoRel.West,
        this + IsoRel.NorthWest
    ];

    /// <summary>
    /// Returns the 6 3D neighbors (4 cardinal + up + down).
    /// </summary>
    public IsoPos[] Neighbors6() => [
        this + IsoRel.North,
        this + IsoRel.East,
        this + IsoRel.South,
        this + IsoRel.West,
        this + IsoRel.Up,
        this + IsoRel.Down
    ];

    /// <summary>
    /// Enumerates horizontal neighbors lazily.
    /// </summary>
    public IEnumerable<IsoPos> EnumerateNeighbors8() {
        foreach (var dir in IsoRel.EightDirections) {
            yield return this + dir;
        }
    }

    /// <summary>
    /// Returns all positions within a horizontal radius on this level.
    /// </summary>
    public IEnumerable<IsoPos> GetArea(int radius, bool includeCenter = true) {
        for (int dx = -radius; dx <= radius; dx++) {
            for (int dy = -radius; dy <= radius; dy++) {
                if (!includeCenter && dx == 0 && dy == 0) continue;
                if (Math.Max(Math.Abs(dx), Math.Abs(dy)) <= radius) {
                    yield return Offset(dx, dy);
                }
            }
        }
    }

    #endregion

    #region Instance Methods - Line of Sight

    /// <summary>
    /// Returns positions on a line to target (Bresenham on horizontal plane).
    /// </summary>
    public IEnumerable<IsoPos> LineTo(IsoPos target, bool includeStart = true, bool includeEnd = true) {
        int x0 = _x, y0 = _y;
        int x1 = target._x, y1 = target._y;

        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        int x = x0, y = y0;
        bool isFirst = true;

        while (true) {
            bool isLast = (x == x1 && y == y1);

            if ((isFirst && includeStart) || (!isFirst && !isLast) || (isLast && includeEnd)) {
                // Interpolate level for 3D lines
                float t = (dx != 0 || dy != 0)
                    ? (float)(Math.Abs(x - x0) + Math.Abs(y - y0)) / (Math.Abs(x1 - x0) + Math.Abs(y1 - y0))
                    : 0f;
                int level = Mathf.RoundToInt(Mathf.Lerp(_level.Value, target._level.Value, t));
                yield return new IsoPos(x, y, new IsoLevel(level));
            }

            isFirst = false;
            if (isLast) break;

            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x += sx; }
            if (e2 <= dx) { err += dx; y += sy; }
        }
    }

    /// <summary>
    /// Returns true if there's line of sight to target.
    /// </summary>
    public bool HasLineOfSight(IsoPos target, Func<IsoPos, bool> isBlocked) {
        foreach (var pos in LineTo(target, includeStart: false, includeEnd: false)) {
            if (isBlocked(pos)) return false;
        }
        return true;
    }

    #endregion

    #region Operators

    public static IsoPos operator +(IsoPos pos, IsoRel offset) => pos.Offset(offset);
    public static IsoPos operator +(IsoRel offset, IsoPos pos) => pos + offset;
    public static IsoPos operator -(IsoPos pos, IsoRel offset) =>
        new(pos._x - offset.X, pos._y - offset.Y, pos._level.Offset(-offset.DLevel));
    public static IsoRel operator -(IsoPos a, IsoPos b) => a.RelativeTo(b);

    public static bool operator ==(IsoPos a, IsoPos b) =>
        a._x == b._x && a._y == b._y && a._level == b._level;
    public static bool operator !=(IsoPos a, IsoPos b) => !(a == b);

    public static implicit operator IsoPos((int x, int y) t) => new(t.x, t.y);
    public static implicit operator IsoPos((int x, int y, IsoLevel level) t) => new(t.x, t.y, t.level);
    public static implicit operator IsoPos((int x, int y, int level) t) => new(t.x, t.y, new IsoLevel(t.level));

    #endregion

    #region IEquatable

    public bool Equals(IsoPos other) => this == other;
    public override bool Equals(object? obj) => obj is IsoPos other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_x, _y, _level);

    #endregion

    #region IFormattable

    public override string ToString() => $"({_x}, {_y}, {_level})";

    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") return ToString();

        return format.ToLowerInvariant() switch {
            "xy" => $"({_x}, {_y})",
            "screen" => ToScreen().ToString(),
            "world" => ToWorld().ToString("F1"),
            "grid" => ToWorldGridCenter(TilesRPG.One, TilesRPG.One).ToString("F1"),
            "full" => $"({_x}, {_y}) @ {_level:name}",
            _ => ToString()
        };
    }

    #endregion
}

#region Extension Methods

public static class IsoPosExtensions {
    /// <summary>
    /// Creates an IsoPos at ground level.
    /// </summary>
    public static IsoPos IsoPos(this (int x, int y) pos) => new IsoPos(pos.x, pos.y);

    /// <summary>
    /// Creates an IsoPos with the specified level.
    /// </summary>
    public static IsoPos At(this (int x, int y) pos, IsoLevel level) => new IsoPos(pos.x, pos.y, level);

    /// <summary>
    /// Converts a Vector2Int to an IsoPos at ground level.
    /// </summary>
    public static IsoPos ToIsoPos(this Vector2Int v) => Isometric.IsoPos.FromVector2Int(v);

    /// <summary>
    /// Converts a Vector3 world position to an IsoPos (simple grid).
    /// </summary>
	public static IsoPos ToIsoPos(this Vector3 worldPos, TilesRPG tileSize = default) {
		float size = tileSize.IsZero ? 1f : tileSize.Value;
		return Isometric.IsoPos.FromWorld(worldPos, size);
	}
}

#endregion