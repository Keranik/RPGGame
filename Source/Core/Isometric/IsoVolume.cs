using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGGame.Core.Isometric;

/// <summary>
/// An immutable 3D volume in isometric space representing multi-tile, multi-level entities.
/// Composed of a horizontal footprint (<see cref="IsoBounds"/>) at a base level, extending upward.
/// 
/// <para>
/// Perfect for buildings, towers, trees, dragons, staircases, and any entity that occupies
/// multiple tiles and/or multiple vertical levels.
/// </para>
/// 
/// <example>
/// <code>
/// // A 3x3 two-story building at ground level
/// var building = new IsoVolume(
///     new IsoBounds(new IsoPos(0, 0), new IsoPos(2, 2)),
///     IsoLevel.Ground,
///     heightInLevels: 2
/// );
/// 
/// // A tall tower (1x1 base, 5 levels tall)
/// var tower = IsoVolume.Tower(new IsoPos(10, 10), height: 5);
/// 
/// // A large dragon (4x3 footprint, 2 levels tall for wings)
/// var dragon = IsoVolume.FromCenter(
///     center: new IsoPos(20, 20),
///     halfWidth: 2, halfDepth: 1,
///     baseLevel: IsoLevel.Ground,
///     heightInLevels: 2
/// );
/// 
/// // Check if player is inside building
/// if (building.Contains(playerPos)) {
///     ApplyIndoorEffects();
/// }
/// 
/// // Check collision between dragon and tower
/// if (dragon.Overlaps(tower)) {
///     HandleCollision();
/// }
/// 
/// // Enumerate all positions in a structure
/// foreach (var pos in building.EnumeratePositions()) {
///     PlaceInteriorTile(pos);
/// }
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct IsoVolume : IEquatable<IsoVolume>, IFormattable {
    #region Constants

    /// <summary>Maximum supported height in levels.</summary>
    public const int MAX_HEIGHT = 100;

    /// <summary>Minimum height (must be at least 1 level).</summary>
    public const int MIN_HEIGHT = 1;

    #endregion

    #region Fields

    private readonly IsoBounds _baseBounds;
    private readonly IsoLevel _baseLevel;
    private readonly int _heightInLevels;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a volume from base bounds, base level, and height.
    /// </summary>
    /// <param name="baseBounds">Horizontal footprint at the base level.</param>
    /// <param name="baseLevel">The bottom level of the volume.</param>
    /// <param name="heightInLevels">Number of levels tall (minimum 1).</param>
    public IsoVolume(IsoBounds baseBounds, IsoLevel baseLevel, int heightInLevels) {
        _baseBounds = baseBounds.AtLevel(baseLevel);
        _baseLevel = baseLevel;
        _heightInLevels = Math.Clamp(heightInLevels, MIN_HEIGHT, MAX_HEIGHT);
    }

    /// <summary>
    /// Creates a volume from an origin position, size, and height.
    /// </summary>
    /// <param name="origin">Bottom-corner position of the volume.</param>
    /// <param name="width">Width in tiles (X direction).</param>
    /// <param name="depth">Depth in tiles (Y direction).</param>
    /// <param name="heightInLevels">Number of levels tall.</param>
    public IsoVolume(IsoPos origin, int width, int depth, int heightInLevels)
        : this(
            new IsoBounds(origin, width, depth),
            origin.Level,
            heightInLevels
        ) { }

    /// <summary>
    /// Creates a single-tile volume at a position with specified height.
    /// </summary>
    /// <param name="position">Position of the single tile.</param>
    /// <param name="heightInLevels">Number of levels tall.</param>
    public IsoVolume(IsoPos position, int heightInLevels)
        : this(
            new IsoBounds(position, position),
            position.Level,
            heightInLevels
        ) { }

    #endregion

    #region Static Presets

    /// <summary>A zero-sized volume at origin.</summary>
    public static IsoVolume Empty => new(IsoBounds.Zero, IsoLevel.Ground, 1);

    /// <summary>A single tile at ground level.</summary>
    public static IsoVolume Unit => new(IsoPos.Origin, 1);

    /// <summary>Invalid volume marker.</summary>
    public static IsoVolume Invalid => new(IsoBounds.Invalid, IsoLevel.Invalid, 1);

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates a volume centered on a position.
    /// </summary>
    /// <param name="center">Center position.</param>
    /// <param name="halfWidth">Half-width in X direction.</param>
    /// <param name="halfDepth">Half-depth in Y direction.</param>
    /// <param name="baseLevel">Base level of the volume.</param>
    /// <param name="heightInLevels">Height in levels.</param>
    public static IsoVolume FromCenter(IsoPos center, int halfWidth, int halfDepth,
        IsoLevel baseLevel, int heightInLevels) {
        var bounds = new IsoBounds(center.AtGround(), halfWidth, halfDepth, 0);
        return new IsoVolume(bounds, baseLevel, heightInLevels);
    }

    /// <summary>
    /// Creates a volume centered on a position with uniform half-size.
    /// </summary>
    public static IsoVolume FromCenter(IsoPos center, int halfSize, int heightInLevels) =>
        FromCenter(center, halfSize, halfSize, center.Level, heightInLevels);

    /// <summary>
    /// Creates a tower (1x1 footprint, variable height).
    /// </summary>
    /// <param name="position">Base position of the tower.</param>
    /// <param name="height">Height in levels.</param>
    public static IsoVolume Tower(IsoPos position, int height) =>
        new(position, height);

    /// <summary>
    /// Creates a building with specified dimensions.
    /// </summary>
    /// <param name="corner">Corner position.</param>
    /// <param name="width">Width in tiles.</param>
    /// <param name="depth">Depth in tiles.</param>
    /// <param name="floors">Number of floors/levels.</param>
    public static IsoVolume Building(IsoPos corner, int width, int depth, int floors) =>
        new(corner, width, depth, floors);

    /// <summary>
    /// Creates a tree-like volume (typically 1x1 base with height).
    /// </summary>
    /// <param name="position">Base of the tree.</param>
    /// <param name="height">Height in levels (trunk + canopy).</param>
    public static IsoVolume Tree(IsoPos position, int height = 2) =>
        new(position, height);

    /// <summary>
    /// Creates a creature volume with a footprint and height.
    /// </summary>
    /// <param name="center">Center position of the creature.</param>
    /// <param name="size">Size category (1=small, 2=medium, 3=large, etc.).</param>
    /// <param name="heightInLevels">Vertical size.</param>
    public static IsoVolume Creature(IsoPos center, int size, int heightInLevels = 1) {
        int halfSize = Math.Max(0, (size - 1) / 2);
        return FromCenter(center, halfSize, halfSize, center.Level, heightInLevels);
    }

    /// <summary>
    /// Creates a staircase volume spanning multiple levels.
    /// </summary>
    /// <param name="bottomPos">Bottom of the staircase.</param>
    /// <param name="direction">Direction the stairs go.</param>
    /// <param name="levelSpan">Number of levels the staircase spans.</param>
    public static IsoVolume Staircase(IsoPos bottomPos, IsoAngle direction, int levelSpan) {
        // Stairs typically extend in a direction as they go up
        var dirRel = direction.ToDirection().Horizontal();
        var endPos = bottomPos + (dirRel * levelSpan);
        var bounds = new IsoBounds(bottomPos, endPos.WithLevel(bottomPos.Level));
        return new IsoVolume(bounds, bottomPos.Level, levelSpan);
    }

    /// <summary>
    /// Creates a volume that encloses all given positions.
    /// </summary>
    public static IsoVolume Enclosing(params IsoPos[] positions) {
        if (positions.Length == 0) return Invalid;

        var min = positions[0];
        var max = positions[0];

        foreach (var pos in positions) {
            min = IsoPos.Min(min, pos);
            max = IsoPos.Max(max, pos);
        }

        var baseBounds = new IsoBounds(min.AtGround(), max.AtGround());
        int height = max.Level - min.Level + 1;

        return new IsoVolume(baseBounds, min.Level, height);
    }

    /// <summary>
    /// Creates a volume that encloses all given positions.
    /// </summary>
    public static IsoVolume Enclosing(IEnumerable<IsoPos> positions) {
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

        if (!min.HasValue) return Invalid;

        var baseBounds = new IsoBounds(min.Value.AtGround(), max!.Value.AtGround());
        int height = max.Value.Level - min.Value.Level + 1;

        return new IsoVolume(baseBounds, min.Value.Level, height);
    }

    #endregion

    #region Properties - Core

    /// <summary>The horizontal footprint at the base level.</summary>
    public IsoBounds BaseBounds => _baseBounds;

    /// <summary>The bottom level of this volume.</summary>
    public IsoLevel BaseLevel => _baseLevel;

    /// <summary>Number of levels this volume spans.</summary>
    public int HeightInLevels => _heightInLevels;

    #endregion

    #region Properties - Derived Bounds

    /// <summary>The horizontal footprint at the top level.</summary>
    public IsoBounds TopBounds => _baseBounds.AtLevel(TopLevel);

    /// <summary>The top level of this volume.</summary>
    public IsoLevel TopLevel => _baseLevel + (_heightInLevels - 1);

    /// <summary>The minimum corner position (base level).</summary>
    public IsoPos Min => _baseBounds.Min.WithLevel(_baseLevel);

    /// <summary>The maximum corner position (top level).</summary>
    public IsoPos Max => _baseBounds.Max.WithLevel(TopLevel);

    /// <summary>The center position of this volume.</summary>
    public IsoPos Center {
        get {
            var boundsCenter = _baseBounds.Center;
            int midLevel = _baseLevel.Value + _heightInLevels / 2;
            return new IsoPos(boundsCenter.X, boundsCenter.Y, new IsoLevel(midLevel));
        }
    }

    /// <summary>Center at the base level.</summary>
    public IsoPos BaseCenter => _baseBounds.Center.WithLevel(_baseLevel);

    /// <summary>Center at the top level.</summary>
    public IsoPos TopCenter => _baseBounds.Center.WithLevel(TopLevel);

    #endregion

    #region Properties - Dimensions

    /// <summary>Width of the footprint (X extent).</summary>
    public int Width => _baseBounds.Width;

    /// <summary>Depth of the footprint (Y extent).</summary>
    public int Depth => _baseBounds.Height;

    /// <summary>Total horizontal area (tiles on one level).</summary>
    public int FootprintArea => _baseBounds.Area;

    /// <summary>Total volume in tile-levels.</summary>
    public int TotalVolume => FootprintArea * _heightInLevels;

    /// <summary>Size as Vector3Int (width, depth, height).</summary>
    public Vector3Int Size => new(Width, Depth, _heightInLevels);

    #endregion

    #region Properties - State Queries

    /// <summary>True if this volume is valid.</summary>
    public bool IsValid => _baseBounds.IsValid && _baseLevel.IsValid && _heightInLevels >= MIN_HEIGHT;

    /// <summary>True if this is a single tile (1x1x1).</summary>
    public bool IsSingleTile => Width == 1 && Depth == 1 && _heightInLevels == 1;

    /// <summary>True if this volume spans only one level.</summary>
    public bool IsSingleLevel => _heightInLevels == 1;

    /// <summary>True if the footprint is a single tile (1x1).</summary>
    public bool IsSingleTileFootprint => Width == 1 && Depth == 1;

    /// <summary>True if the footprint is square.</summary>
    public bool IsSquareFootprint => Width == Depth;

    /// <summary>True if the volume is a cube (width == depth == height).</summary>
    public bool IsCube => Width == Depth && Depth == _heightInLevels;

    /// <summary>True if any part of this volume is underground.</summary>
    public bool HasUndergroundPortion => _baseLevel.IsUnderground;

    /// <summary>True if any part of this volume is above ground.</summary>
    public bool HasAboveGroundPortion => TopLevel.IsAboveGround;

    /// <summary>True if this volume spans both underground and above ground.</summary>
    public bool SpansGroundLevel => _baseLevel.IsUnderground && TopLevel.IsSurface;

    #endregion

    #region Instance Methods - Containment

    /// <summary>
    /// Returns true if the position is inside this volume.
    /// </summary>
    public bool Contains(IsoPos pos) {
        // Check level first (faster rejection)
        if (pos.Level < _baseLevel || pos.Level > TopLevel) return false;

        // Check horizontal bounds
        return _baseBounds.ContainsXY(pos);
    }

    /// <summary>
    /// Returns true if the entire other volume is inside this volume.
    /// </summary>
    public bool Contains(IsoVolume other) =>
        Contains(other.Min) && Contains(other.Max);

    /// <summary>
    /// Returns true if all given positions are inside this volume.
    /// </summary>
    public bool ContainsAll(params IsoPos[] positions) {
        foreach (var pos in positions) {
            if (!Contains(pos)) return false;
        }
        return true;
    }

    /// <summary>
    /// Returns true if any of the given positions are inside this volume.
    /// </summary>
    public bool ContainsAny(params IsoPos[] positions) {
        foreach (var pos in positions) {
            if (Contains(pos)) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if the position is on the surface (edge or top/bottom) of this volume.
    /// </summary>
    public bool IsOnSurface(IsoPos pos) {
        if (!Contains(pos)) return false;

        // Top or bottom level
        if (pos.Level == _baseLevel || pos.Level == TopLevel) return true;

        // Edge of footprint
        return _baseBounds.IsOnEdge(pos);
    }

    /// <summary>
    /// Returns true if the position is in the interior (not on surface) of this volume.
    /// </summary>
    public bool IsInterior(IsoPos pos) {
        if (!Contains(pos)) return false;

        // Must be on an interior level
        if (pos.Level == _baseLevel || pos.Level == TopLevel) return false;

        // Must be in interior of footprint
        return !_baseBounds.IsOnEdge(pos);
    }

    #endregion

    #region Instance Methods - Overlap and Intersection

    /// <summary>
    /// Returns true if this volume overlaps with another.
    /// </summary>
    public bool Overlaps(IsoVolume other) {
        // Check vertical overlap
        if (TopLevel < other._baseLevel || _baseLevel > other.TopLevel) return false;

        // Check horizontal overlap
        return _baseBounds.Overlaps(other._baseBounds);
    }

    /// <summary>
    /// Returns true if this volume overlaps with a 2D bounds on any level.
    /// </summary>
    public bool Overlaps(IsoBounds bounds) {
        // Check if bounds level is within our range
        if (bounds.MinLevel > TopLevel || bounds.MaxLevel < _baseLevel) return false;

        return _baseBounds.Overlaps(bounds);
    }

    /// <summary>
    /// Returns the intersection of this volume with another (or Invalid if no intersection).
    /// </summary>
    public IsoVolume Intersection(IsoVolume other) {
        if (!Overlaps(other)) return Invalid;

        // Intersect levels
        var minLevel = new IsoLevel(Math.Max(_baseLevel.Value, other._baseLevel.Value));
        var maxLevel = new IsoLevel(Math.Min(TopLevel.Value, other.TopLevel.Value));
        int height = maxLevel - minLevel + 1;

        // Intersect horizontal bounds
        var intersectedBounds = _baseBounds.Intersection(other._baseBounds);

        return new IsoVolume(intersectedBounds, minLevel, height);
    }

    /// <summary>
    /// Returns the union of this volume with another (smallest volume containing both).
    /// </summary>
    public IsoVolume Union(IsoVolume other) {
        var minLevel = new IsoLevel(Math.Min(_baseLevel.Value, other._baseLevel.Value));
        var maxLevel = new IsoLevel(Math.Max(TopLevel.Value, other.TopLevel.Value));
        int height = maxLevel - minLevel + 1;

        var unionBounds = _baseBounds.Union(other._baseBounds);

        return new IsoVolume(unionBounds, minLevel, height);
    }

    /// <summary>
    /// Returns the distance from this volume to a position (0 if inside).
    /// </summary>
    public IsoDistance DistanceTo(IsoPos pos) {
        if (Contains(pos)) return IsoDistance.Zero;

        // Find closest point on volume
        var closestXY = _baseBounds.ClosestPoint(pos);
        int closestLevel = Math.Clamp(pos.Level.Value, _baseLevel.Value, TopLevel.Value);
        var closest = new IsoPos(closestXY.X, closestXY.Y, new IsoLevel(closestLevel));

        return pos.DistanceTo(closest);
    }

    /// <summary>
    /// Returns the distance from this volume to another volume (0 if overlapping).
    /// </summary>
    public IsoDistance DistanceTo(IsoVolume other) {
        if (Overlaps(other)) return IsoDistance.Zero;

        // Find closest points between the two volumes
        var thisCenter = Center;
        var otherCenter = other.Center;

        // Get closest positions
        // This is an approximation - true minimum distance would require more complex math
        var closest1 = ClosestPointTo(otherCenter);
        var closest2 = other.ClosestPointTo(thisCenter);

        return closest1.DistanceTo(closest2);
    }

    /// <summary>
    /// Returns the closest point inside this volume to an external position.
    /// </summary>
    public IsoPos ClosestPointTo(IsoPos pos) {
        var closestXY = _baseBounds.ClosestPoint(pos);
        int closestLevel = Math.Clamp(pos.Level.Value, _baseLevel.Value, TopLevel.Value);
        return new IsoPos(closestXY.X, closestXY.Y, new IsoLevel(closestLevel));
    }

    #endregion

    #region Instance Methods - Level Slicing

    /// <summary>
    /// Returns the bounds at a specific level, or null if the level is outside this volume.
    /// </summary>
    public IsoBounds? AtLevel(IsoLevel level) {
        if (level < _baseLevel || level > TopLevel) return null;
        return _baseBounds.AtLevel(level);
    }

    /// <summary>
    /// Returns true if this volume includes the specified level.
    /// </summary>
    public bool IncludesLevel(IsoLevel level) =>
        level >= _baseLevel && level <= TopLevel;

    /// <summary>
    /// Returns all levels spanned by this volume.
    /// </summary>
    public IEnumerable<IsoLevel> EnumerateLevels() {
        for (int l = _baseLevel.Value; l <= TopLevel.Value; l++) {
            yield return new IsoLevel(l);
        }
    }

    /// <summary>
    /// Returns the bounds at each level of this volume.
    /// </summary>
    public IEnumerable<IsoBounds> EnumerateLevelSlices() {
        foreach (var level in EnumerateLevels()) {
            yield return _baseBounds.AtLevel(level);
        }
    }

    #endregion

    #region Instance Methods - Position Enumeration

    /// <summary>
    /// Enumerates all positions within this volume.
    /// </summary>
    public IEnumerable<IsoPos> EnumeratePositions() {
        foreach (var level in EnumerateLevels()) {
            foreach (var pos in _baseBounds.EnumerateLevel(level)) {
                yield return pos;
            }
        }
    }

    /// <summary>
    /// Enumerates positions on the surface of this volume (exterior shell).
    /// </summary>
    public IEnumerable<IsoPos> EnumerateSurface() {
        // Bottom and top faces
        foreach (var pos in _baseBounds.EnumerateLevel(_baseLevel)) {
            yield return pos;
        }

        if (_heightInLevels > 1) {
            foreach (var pos in _baseBounds.EnumerateLevel(TopLevel)) {
                yield return pos;
            }
        }

        // Side faces (edges of each middle level)
        for (int l = _baseLevel.Value + 1; l < TopLevel.Value; l++) {
            var level = new IsoLevel(l);
            foreach (var pos in _baseBounds.EnumerateEdge(level)) {
                yield return pos;
            }
        }
    }

    /// <summary>
    /// Enumerates positions in the interior of this volume (not on surface).
    /// </summary>
    public IEnumerable<IsoPos> EnumerateInterior() {
        if (_heightInLevels <= 2 || Width <= 2 || Depth <= 2) yield break;

        for (int l = _baseLevel.Value + 1; l < TopLevel.Value; l++) {
            var level = new IsoLevel(l);
            foreach (var pos in _baseBounds.EnumerateInterior(level)) {
                yield return pos;
            }
        }
    }

    /// <summary>
    /// Enumerates corner positions of this volume.
    /// </summary>
    public IEnumerable<IsoPos> EnumerateCorners() {
        // Base corners
        foreach (var pos in _baseBounds.EnumerateCorners(_baseLevel)) {
            yield return pos;
        }

        // Top corners (if more than 1 level)
        if (_heightInLevels > 1) {
            foreach (var pos in _baseBounds.EnumerateCorners(TopLevel)) {
                yield return pos;
            }
        }
    }

    /// <summary>
    /// Returns a random position within this volume.
    /// </summary>
    public IsoPos RandomPosition() {
        var xyPos = _baseBounds.RandomPositionOnLevel(_baseLevel);
        int randomLevel = UnityEngine.Random.Range(_baseLevel.Value, TopLevel.Value + 1);
        return new IsoPos(xyPos.X, xyPos.Y, new IsoLevel(randomLevel));
    }

    /// <summary>
    /// Returns a random position on the surface of this volume.
    /// </summary>
    public IsoPos RandomSurfacePosition() {
        // Choose a random face
        int face = UnityEngine.Random.Range(0, 6);

        return face switch {
            0 => _baseBounds.RandomPositionOnLevel(_baseLevel),  // Bottom
            1 => _baseBounds.RandomPositionOnLevel(TopLevel),     // Top
            _ => RandomEdgePosition()                              // Sides
        };
    }

    private IsoPos RandomEdgePosition() {
        var level = new IsoLevel(UnityEngine.Random.Range(_baseLevel.Value, TopLevel.Value + 1));
        var edgePositions = new List<IsoPos>();
        foreach (var pos in _baseBounds.EnumerateEdge(level)) {
            edgePositions.Add(pos);
        }
        return edgePositions.Count > 0
            ? edgePositions[UnityEngine.Random.Range(0, edgePositions.Count)]
            : _baseBounds.RandomPositionOnLevel(level);
    }

    #endregion

    #region Instance Methods - Modifications

    /// <summary>
    /// Returns a new volume with a different height.
    /// </summary>
    public IsoVolume WithHeight(int newHeight) =>
        new(_baseBounds, _baseLevel, newHeight);

    /// <summary>
    /// Returns a new volume with the height extended by the specified amount.
    /// </summary>
    public IsoVolume ExtendedUp(int additionalLevels) =>
        new(_baseBounds, _baseLevel, _heightInLevels + additionalLevels);

    /// <summary>
    /// Returns a new volume with the base extended down by the specified amount.
    /// </summary>
    public IsoVolume ExtendedDown(int additionalLevels) =>
        new(_baseBounds, _baseLevel - additionalLevels, _heightInLevels + additionalLevels);

    /// <summary>
    /// Returns a new volume at a different base level.
    /// </summary>
    public IsoVolume AtBaseLevel(IsoLevel newBaseLevel) =>
        new(_baseBounds, newBaseLevel, _heightInLevels);

    /// <summary>
    /// Returns a new volume moved by the specified offset.
    /// </summary>
    public IsoVolume Offset(IsoRel offset) =>
        new(_baseBounds.Shifted(offset), _baseLevel + offset.DLevel, _heightInLevels);

    /// <summary>
    /// Returns a new volume moved by the specified offset.
    /// </summary>
    public IsoVolume Offset(int dx, int dy, int dLevel) =>
        Offset(new IsoRel(dx, dy, dLevel));

    /// <summary>
    /// Returns a new volume centered on a position (same size).
    /// </summary>
    public IsoVolume CenteredOn(IsoPos newCenter) {
        var currentCenter = Center;
        var offset = currentCenter.RelativeTo(newCenter);
        return Offset(offset);
    }

    /// <summary>
    /// Returns a new volume expanded in all horizontal directions.
    /// </summary>
    public IsoVolume ExpandedHorizontal(int amount) =>
        new(_baseBounds.ExpandedXY(amount), _baseLevel, _heightInLevels);

    /// <summary>
    /// Returns a new volume expanded in all directions (horizontal and vertical).
    /// </summary>
    public IsoVolume Expanded(int amount) =>
        new(_baseBounds.ExpandedXY(amount), _baseLevel - amount, _heightInLevels + amount * 2);

    /// <summary>
    /// Returns a new volume contracted in all horizontal directions.
    /// </summary>
    public IsoVolume ContractedHorizontal(int amount) =>
        ExpandedHorizontal(-amount);

    /// <summary>
    /// Returns a new volume with a different footprint but same height.
    /// </summary>
    public IsoVolume WithBounds(IsoBounds newBounds) =>
        new(newBounds, _baseLevel, _heightInLevels);

    /// <summary>
    /// Returns a new volume scaled by the given factor (centered).
    /// </summary>
    public IsoVolume Scaled(float factor) {
        int newWidth = Mathf.Max(1, Mathf.RoundToInt(Width * factor));
        int newDepth = Mathf.Max(1, Mathf.RoundToInt(Depth * factor));
        int newHeight = Mathf.Max(1, Mathf.RoundToInt(_heightInLevels * factor));

        return FromCenter(Center, newWidth / 2, newDepth / 2, _baseLevel, newHeight);
    }

    #endregion

    #region Instance Methods - Conversion

    /// <summary>
    /// Converts to a Unity Bounds in world space.
    /// </summary>
    public Bounds ToWorldBounds(float levelHeight = IsoLevel.LEVEL_HEIGHT_UNITS) {
        var minWorld = Min.ToWorld(levelHeight);
        var maxWorld = Max.ToWorld(levelHeight) + new Vector3(1, levelHeight, 1);
        return new Bounds((minWorld + maxWorld) / 2, maxWorld - minWorld);
    }

    /// <summary>
    /// Converts to a BoundsInt (x, y = iso coords, z = level).
    /// </summary>
    public BoundsInt ToBoundsInt() => new(
        _baseBounds.Min.X, _baseBounds.Min.Y, _baseLevel.Value,
        Width, Depth, _heightInLevels
    );

    /// <summary>
    /// Returns a descriptive string for this volume type.
    /// </summary>
    public string GetDescription() {
        if (IsSingleTile) return "Single tile";
        if (IsSingleTileFootprint) return $"Column ({_heightInLevels} levels)";
        if (IsSingleLevel) return $"Floor ({Width}×{Depth})";
        if (IsCube) return $"Cube ({Width}³)";
        return $"Volume ({Width}×{Depth}×{_heightInLevels})";
    }

    #endregion

    #region Operators

    public static bool operator ==(IsoVolume a, IsoVolume b) =>
        a._baseBounds == b._baseBounds &&
        a._baseLevel == b._baseLevel &&
        a._heightInLevels == b._heightInLevels;

    public static bool operator !=(IsoVolume a, IsoVolume b) => !(a == b);

    /// <summary>Offsets a volume by a relative direction.</summary>
    public static IsoVolume operator +(IsoVolume volume, IsoRel offset) => volume.Offset(offset);

    /// <summary>Offsets a volume in the opposite direction.</summary>
    public static IsoVolume operator -(IsoVolume volume, IsoRel offset) =>
        volume.Offset(offset.Opposite());

    #endregion

    #region IEquatable

    public bool Equals(IsoVolume other) => this == other;
    public override bool Equals(object? obj) => obj is IsoVolume other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_baseBounds, _baseLevel, _heightInLevels);

    #endregion

    #region IFormattable

    public override string ToString() =>
        $"Volume({Width}×{Depth}×{_heightInLevels} @ {_baseLevel})";

    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") return ToString();

        return format.ToLowerInvariant() switch {
            "size" => $"{Width}×{Depth}×{_heightInLevels}",
            "pos" => $"{Min} → {Max}",
            "full" => $"Volume: {Width}×{Depth} footprint, {_heightInLevels} levels @ {_baseLevel:name}",
            "vol" => $"{TotalVolume} cells",
            "desc" => GetDescription(),
            _ => ToString()
        };
    }

    #endregion
}

#region Extension Methods

/// <summary>
/// Extension methods for creating and working with IsoVolume.
/// </summary>
public static class IsoVolumeExtensions {
    /// <summary>
    /// Creates a volume from this bounds with the specified height.
    /// </summary>
    /// <example>
    /// <code>
    /// var building = bounds.WithHeight(3);
    /// </code>
    /// </example>
    public static IsoVolume WithHeight(this IsoBounds bounds, int heightInLevels) =>
        new(bounds, bounds.MinLevel, heightInLevels);

    /// <summary>
    /// Creates a volume from this bounds with height specified via extension.
    /// </summary>
    /// <example>
    /// <code>
    /// var tower = bounds.LevelsTall(5);
    /// </code>
    /// </example>
    public static IsoVolume LevelsTall(this IsoBounds bounds, int levels) =>
        bounds.WithHeight(levels);

    /// <summary>
    /// Creates a single-tile volume at this position with the specified height.
    /// </summary>
    public static IsoVolume WithHeight(this IsoPos pos, int heightInLevels) =>
        new(pos, heightInLevels);

    /// <summary>
    /// Creates a single-tile volume at this position with height specified via extension.
    /// </summary>
    /// <example>
    /// <code>
    /// var pillar = position.LevelsTall(3);
    /// </code>
    /// </example>
    public static IsoVolume LevelsTall(this IsoPos pos, int levels) =>
        pos.WithHeight(levels);

    /// <summary>
    /// Creates a volume centered on this position.
    /// </summary>
    public static IsoVolume ToVolume(this IsoPos center, int halfWidth, int halfDepth, int heightInLevels) =>
        IsoVolume.FromCenter(center, halfWidth, halfDepth, center.Level, heightInLevels);

    /// <summary>
    /// Creates a tower at this position.
    /// </summary>
    public static IsoVolume ToTower(this IsoPos pos, int height) =>
        IsoVolume.Tower(pos, height);

    /// <summary>
    /// Creates a building at this corner position.
    /// </summary>
    public static IsoVolume ToBuilding(this IsoPos corner, int width, int depth, int floors) =>
        IsoVolume.Building(corner, width, depth, floors);

    /// <summary>
    /// Checks if this position is inside the volume.
    /// </summary>
    public static bool IsInside(this IsoPos pos, IsoVolume volume) =>
        volume.Contains(pos);

    /// <summary>
    /// Creates an integer representing levels for volume height.
    /// </summary>
    /// <example>
    /// <code>
    /// var height = 5.Levels();
    /// var volume = bounds.WithHeight(3.Levels());
    /// </code>
    /// </example>
    public static int Levels(this int count) => Math.Max(IsoVolume.MIN_HEIGHT, count);

    /// <summary>
    /// Checks if this volume collides with another.
    /// </summary>
    public static bool CollidesWith(this IsoVolume a, IsoVolume b) => a.Overlaps(b);

    /// <summary>
    /// Returns all volumes that overlap with this volume.
    /// </summary>
    public static IEnumerable<IsoVolume> GetOverlapping(this IsoVolume volume, IEnumerable<IsoVolume> others) {
        foreach (var other in others) {
            if (volume.Overlaps(other)) {
                yield return other;
            }
        }
    }

    /// <summary>
    /// Checks if this volume is completely above another.
    /// </summary>
    public static bool IsAbove(this IsoVolume a, IsoVolume b) =>
        a.BaseLevel > b.TopLevel;

    /// <summary>
    /// Checks if this volume is completely below another.
    /// </summary>
    public static bool IsBelow(this IsoVolume a, IsoVolume b) =>
        a.TopLevel < b.BaseLevel;

    /// <summary>
    /// Returns the vertical gap between two volumes (negative if overlapping).
    /// </summary>
    public static int VerticalGap(this IsoVolume a, IsoVolume b) {
        if (a.BaseLevel > b.TopLevel) {
            return a.BaseLevel - b.TopLevel - 1;
        }
        if (b.BaseLevel > a.TopLevel) {
            return b.BaseLevel - a.TopLevel - 1;
        }
        return -1; // Overlapping
    }
}

#endregion

#region Usage Examples
/*
 * ═══════════════════════════════════════════════════════════════════════════
 * IsoVolume USAGE EXAMPLES
 * ═══════════════════════════════════════════════════════════════════════════
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // CREATION STYLES
 * // ─────────────────────────────────────────────────────────────────────────
 * 
 * // From bounds + height
 * var bounds = new IsoBounds(new IsoPos(0, 0), new IsoPos(4, 4));
 * var building = new IsoVolume(bounds, IsoLevel.Ground, heightInLevels: 3);
 * 
 * // From position + dimensions
 * var warehouse = new IsoVolume(origin: new IsoPos(10, 10), width: 6, depth: 4, heightInLevels: 2);
 * 
 * // Single-tile tall structure
 * var pillar = new IsoVolume(new IsoPos(5, 5), heightInLevels: 4);
 * 
 * // Using factories
 * var tower = IsoVolume.Tower(new IsoPos(20, 20), height: 5);
 * var tree = IsoVolume.Tree(new IsoPos(15, 15), height: 2);
 * var dragon = IsoVolume.Creature(playerPos, size: 3, heightInLevels: 2);
 * 
 * // Using fluent extensions
 * var column = position.LevelsTall(4);
 * var floor = bounds.LevelsTall(1);
 * var house = corner.ToBuilding(width: 5, depth: 4, floors: 2);
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // COLLISION DETECTION
 * // ─────────────────────────────────────────────────────────────────────────
 * 
 * // Point containment
 * if (building.Contains(playerPos)) {
 *     EnterBuilding();
 * }
 * 
 * // Volume overlap (collision)
 * if (dragon.Overlaps(tower)) {
 *     HandleDragonTowerCollision();
 * }
 * 
 * // Find all buildings the dragon collides with
 * foreach (var b in dragon.GetOverlapping(allBuildings)) {
 *     DamageBuilding(b);
 * }
 * 
 * // Check if position is on the surface (for placing objects)
 * if (building.IsOnSurface(clickPos)) {
 *     PlaceDecoration(clickPos);
 * }
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // LEVEL SLICING
 * // ─────────────────────────────────────────────────────────────────────────
 * 
 * // Get bounds at a specific level
 * IsoBounds? floor2 = building.AtLevel(IsoLevel.Floor1);
 * if (floor2.HasValue) {
 *     RenderFloor(floor2.Value);
 * }
 * 
 * // Check if a level is included
 * if (building.IncludesLevel(IsoLevel.Underground1)) {
 *     EnableBasementLighting();
 * }
 * 
 * // Iterate all level slices
 * foreach (var slice in building.EnumerateLevelSlices()) {
 *     RenderLevelFloor(slice);
 * }
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // POSITION ENUMERATION
 * // ─────────────────────────────────────────────────────────────────────────
 * 
 * // All positions in volume (for filling)
 * foreach (var pos in building.EnumeratePositions()) {
 *     PlaceInteriorBlock(pos);
 * }
 * 
 * // Surface only (for shell)
 * foreach (var pos in tower.EnumerateSurface()) {
 *     PlaceWallBlock(pos);
 * }
 * 
 * // Interior only (hollow inside)
 * foreach (var pos in building.EnumerateInterior()) {
 *     PlaceFloorTile(pos);
 * }
 * 
 * // Corners (for pillars/supports)
 * foreach (var pos in building.EnumerateCorners()) {
 *     PlaceSupportColumn(pos);
 * }
 * 
 * // Random spawn point
 * var spawnPos = dungeon.RandomPosition();
 * SpawnEnemy(spawnPos);
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // MODIFICATIONS
 * // ─────────────────────────────────────────────────────────────────────────
 * 
 * // Change height
 * var towerUpgraded = tower.WithHeight(7);
 * 
 * // Extend up/down
 * var withBasement = building.ExtendedDown(1);
 * var withRoof = building.ExtendedUp(1);
 * 
 * // Move volume
 * var movedBuilding = building.Offset(IsoRel.East * 5);
 * var relocatedDragon = dragon.CenteredOn(newPosition);
 * 
 * // Expand/contract
 * var fortress = building.ExpandedHorizontal(2);
 * var core = building.ContractedHorizontal(1);
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // INTERSECTION / UNION
 * // ─────────────────────────────────────────────────────────────────────────
 * 
 * // Find intersection (overlap region)
 * var overlap = buildingA.Intersection(buildingB);
 * if (overlap.IsValid) {
 *     MarkConflictZone(overlap);
 * }
 * 
 * // Find union (bounding volume)
 * var combined = buildingA.Union(buildingB);
 * SetBuildingGroupBounds(combined);
 * 
 * // Enclosing volume from positions
 * var creatureVolume = IsoVolume.Enclosing(headPos, tailPos, wingPos);
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // DISTANCE QUERIES
 * // ─────────────────────────────────────────────────────────────────────────
 * 
 * // Distance from volume to point
 * var distToPlayer = building.DistanceTo(playerPos);
 * if (distToPlayer.HorizontalTiles <= 5.Tiles()) {
 *     ShowBuildingUI();
 * }
 * 
 * // Closest point on volume
 * var entryPoint = building.ClosestPointTo(playerPos);
 * NavigateTo(entryPoint);
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // SPECIAL STRUCTURES
 * // ─────────────────────────────────────────────────────────────────────────
 * 
 * // Staircase spanning levels
 * var stairs = IsoVolume.Staircase(
 *     bottomPos: new IsoPos(0, 0, IsoLevel.Ground),
 *     direction: IsoAngle.North,
 *     levelSpan: 2
 * );
 * 
 * // Multi-floor dungeon
 * var dungeon = new IsoVolume(
 *     new IsoBounds(new IsoPos(0, 0), new IsoPos(30, 30)),
 *     IsoLevel.Underground3,
 *     heightInLevels: 3
 * );
 * 
 * // Underground + above ground structure
 * var deepBuilding = building.ExtendedDown(2);
 * if (deepBuilding.SpansGroundLevel) {
 *     // Has both basement and upper floors
 * }
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // STRING FORMATTING
 * // ─────────────────────────────────────────────────────────────────────────
 * 
 * var vol = IsoVolume.Building(origin, 5, 4, 3);
 * Debug.Log(vol);                    // "Volume(5×4×3 @ Ground)"
 * Debug.Log($"{vol:size}");          // "5×4×3"
 * Debug.Log($"{vol:pos}");           // "(0, 0, Ground) → (4, 3, Floor2)"
 * Debug.Log($"{vol:full}");          // "Volume: 5×4 footprint, 3 levels @ Ground Floor"
 * Debug.Log($"{vol:vol}");           // "60 cells"
 * Debug.Log($"{vol:desc}");          // "Volume (5×4×3)"
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // WORLD CONVERSION
 * // ─────────────────────────────────────────────────────────────────────────
 * 
 * // Convert to Unity Bounds for physics
 * Bounds worldBounds = building.ToWorldBounds();
 * if (worldBounds.Contains(transform.position)) {
 *     // Player is inside building bounds
 * }
 * 
 * // Convert to BoundsInt for tilemap operations
 * BoundsInt tileBounds = building.ToBoundsInt();
 * tilemap.BoxFill(tileBounds.position, tile, ...);
 */
#endregion