using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGGame.Core.Isometric;

/// <summary>
/// A ray in isometric 3D space for line-of-sight, projectile paths, and mouse picking.
/// 
/// <example>
/// <code>
/// // Line of sight check
/// var ray = IsoRay.FromPositions(shooter, target);
/// var result = ray.Cast(pos => IsBlocking(pos));
/// if (result.Hit) {
///     Debug.Log($"Blocked at {result.HitPosition}");
/// }
/// 
/// // Mouse picking
/// var pickRay = IsoRay.FromScreen(mousePos);
/// var groundHit = pickRay.CastToLevel(IsoLevel.Ground, pos => HasTile(pos));
/// 
/// // Projectile path
/// foreach (var pos in ray.Trace()) {
///     SpawnTrailParticle(pos.ToWorld());
/// }
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct IsoRay : IEquatable<IsoRay>, IFormattable {
    #region Fields

    private readonly IsoPos _origin;
    private readonly IsoRel _direction;
    private readonly float _maxDistance;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a ray from origin in the specified direction.
    /// </summary>
    /// <param name="origin">Starting position.</param>
    /// <param name="direction">Direction (will be normalized).</param>
    /// <param name="maxDistance">Maximum distance to trace.</param>
    public IsoRay(IsoPos origin, IsoRel direction, float maxDistance = float.MaxValue) {
        _origin = origin;
        _direction = direction.IsZero ? IsoRel.North : direction.Normalized();
        _maxDistance = Math.Max(0, maxDistance);
    }

    /// <summary>
    /// Creates a ray from origin towards a target position.
    /// </summary>
    public IsoRay(IsoPos origin, IsoPos target) {
        _origin = origin;
        var rel = origin.RelativeTo(target);
        _direction = rel.IsZero ? IsoRel.North : rel.Normalized();
        _maxDistance = rel.ToDistance().ToMovementCost();
    }

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates a ray from one position to another.
    /// </summary>
    public static IsoRay FromPositions(IsoPos origin, IsoPos target) => new(origin, target);

    /// <summary>
    /// Creates a ray from origin in a direction with max distance.
    /// </summary>
    public static IsoRay FromDirection(IsoPos origin, IsoAngle direction, float maxDistance = float.MaxValue) =>
        new(origin, direction.ToDirection(), maxDistance);

    /// <summary>
    /// Creates a ray from origin in a direction with max distance.
    /// </summary>
    public static IsoRay FromDirection(IsoPos origin, IsoRel direction, float maxDistance = float.MaxValue) =>
        new(origin, direction, maxDistance);

    /// <summary>
    /// Creates a downward ray from screen coordinates for mouse picking.
    /// </summary>
    /// <param name="screenPos">Screen position in pixels.</param>
    /// <param name="startLevel">Level to start the ray from (default: Sky).</param>
    /// <param name="tileWidth">Width of one tile in pixels.</param>
    /// <param name="tileHeight">Height of one tile in pixels.</param>
    public static IsoRay FromScreen(
        Vector2 screenPos,
        IsoLevel? startLevel = null,
        float tileWidth = IsoPos.DEFAULT_TILE_WIDTH,
        float tileHeight = IsoPos.DEFAULT_TILE_HEIGHT) {
        // Convert screen to iso coordinates
        var isoPos = IsoPos.FromScreen(screenPos, tileWidth, tileHeight, startLevel ?? IsoLevel.Sky);

        // Ray goes "down" through levels for picking
        return new IsoRay(isoPos, IsoRel.Down, IsoLevel.MAX_LEVEL * 2 + 1);
    }

    /// <summary>
    /// Creates an upward ray from screen coordinates.
    /// </summary>
    public static IsoRay FromScreenUp(
        Vector2 screenPos,
        IsoLevel? startLevel = null,
        float tileWidth = IsoPos.DEFAULT_TILE_WIDTH,
        float tileHeight = IsoPos.DEFAULT_TILE_HEIGHT) {
        var isoPos = IsoPos.FromScreen(screenPos, tileWidth, tileHeight, startLevel ?? IsoLevel.Underground3);
        return new IsoRay(isoPos, IsoRel.Up, IsoLevel.MAX_LEVEL * 2 + 1);
    }

    /// <summary>
    /// Creates a horizontal ray (no level change).
    /// </summary>
    public static IsoRay Horizontal(IsoPos origin, IsoAngle direction, float maxDistance = float.MaxValue) =>
        new(origin, direction.ToDirection().Horizontal(), maxDistance);

    /// <summary>
    /// Creates a horizontal ray towards a target (ignoring level difference).
    /// </summary>
    public static IsoRay HorizontalTo(IsoPos origin, IsoPos target) {
        var rel = origin.RelativeTo(target).Horizontal();
        return new IsoRay(origin, rel, rel.ChebyshevLengthXY().ToFloat());
    }

    /// <summary>
    /// Creates a vertical ray (up or down only).
    /// </summary>
    public static IsoRay Vertical(IsoPos origin, bool up, int maxLevels = IsoLevel.MAX_LEVEL) =>
        new(origin, up ? IsoRel.Up : IsoRel.Down, maxLevels);

    #endregion

    #region Properties

    /// <summary>Ray origin position.</summary>
    public IsoPos Origin => _origin;

    /// <summary>Normalized ray direction.</summary>
    public IsoRel Direction => _direction;

    /// <summary>Maximum distance this ray will travel.</summary>
    public float MaxDistance => _maxDistance;

    /// <summary>True if this is a horizontal ray (no level change).</summary>
    public bool IsHorizontal => _direction.DLevel == 0;

    /// <summary>True if this is a vertical ray (no horizontal movement).</summary>
    public bool IsVertical => _direction.X == 0 && _direction.Y == 0;

    /// <summary>True if this ray goes upward.</summary>
    public bool GoesUp => _direction.DLevel > 0;

    /// <summary>True if this ray goes downward.</summary>
    public bool GoesDown => _direction.DLevel < 0;

    /// <summary>The angle of this ray on the horizontal plane.</summary>
    public IsoAngle Angle => IsoAngle.FromIsoRel(_direction);

    #endregion

    #region Instance Methods - Tracing

    /// <summary>
    /// Traces the ray and returns all positions along it using 3D Bresenham.
    /// </summary>
    /// <param name="includeOrigin">Whether to include the starting position.</param>
    public IEnumerable<IsoPos> Trace(bool includeOrigin = false) {
        if (includeOrigin) {
            yield return _origin;
        }

        // Scale direction to reach max distance
        int steps = Mathf.CeilToInt(_maxDistance);
        if (steps <= 0) yield break;

        // Use 3D Bresenham-like stepping
        float dx = _direction.X;
        float dy = _direction.Y;
        float dz = _direction.DLevel;

        float x = _origin.X;
        float y = _origin.Y;
        float z = _origin.Level.Value;

        IsoPos lastPos = _origin;
        float distanceTraveled = 0;

        for (int step = 1; step <= steps * 2 && distanceTraveled < _maxDistance; step++) {
            x += dx * 0.5f;
            y += dy * 0.5f;
            z += dz * 0.5f;

            var currentPos = new IsoPos(
                Mathf.RoundToInt(x),
                Mathf.RoundToInt(y),
                new IsoLevel(Mathf.RoundToInt(z))
            );

            if (currentPos != lastPos) {
                distanceTraveled = _origin.DistanceTo(currentPos).ToMovementCost();
                if (distanceTraveled > _maxDistance) yield break;

                yield return currentPos;
                lastPos = currentPos;
            }
        }
    }

    /// <summary>
    /// Traces the ray on the horizontal plane only (ignoring level).
    /// </summary>
    public IEnumerable<IsoPos> TraceHorizontal(bool includeOrigin = false) {
        if (includeOrigin) {
            yield return _origin;
        }

        if (_direction.X == 0 && _direction.Y == 0) yield break;

        int x0 = _origin.X, y0 = _origin.Y;
        int dx = _direction.X, dy = _direction.Y;

        // Normalize to unit steps
        int sx = Math.Sign(dx);
        int sy = Math.Sign(dy);
        dx = Math.Abs(dx);
        dy = Math.Abs(dy);

        int err = dx - dy;
        int x = x0, y = y0;
        int steps = 0;
        int maxSteps = Mathf.CeilToInt(_maxDistance);

        while (steps < maxSteps) {
            int e2 = 2 * err;

            if (e2 > -dy) {
                err -= dy;
                x += sx;
            }

            if (e2 < dx) {
                err += dx;
                y += sy;
            }

            steps++;
            yield return new IsoPos(x, y, _origin.Level);
        }
    }

    /// <summary>
    /// Gets the position at a specific distance along the ray.
    /// </summary>
    public IsoPos GetPointAtDistance(float distance) {
        if (distance <= 0) return _origin;
        if (distance >= _maxDistance) distance = _maxDistance;

        return new IsoPos(
            _origin.X + Mathf.RoundToInt(_direction.X * distance),
            _origin.Y + Mathf.RoundToInt(_direction.Y * distance),
            _origin.Level.Offset(Mathf.RoundToInt(_direction.DLevel * distance))
        );
    }

    /// <summary>
    /// Gets the endpoint of this ray (at max distance).
    /// </summary>
    public IsoPos GetEndpoint() => GetPointAtDistance(_maxDistance);

    #endregion

    #region Instance Methods - Casting

    /// <summary>
    /// Casts the ray and returns the first hit.
    /// </summary>
    /// <param name="isBlocking">Function to check if a position blocks the ray.</param>
    /// <param name="includeOrigin">Whether to test the origin position.</param>
    public IsoRayHit Cast(Func<IsoPos, bool> isBlocking, bool includeOrigin = false) {
        float distanceTraveled = 0;

        foreach (var pos in Trace(includeOrigin)) {
            distanceTraveled = _origin.DistanceTo(pos).ToMovementCost();

            if (isBlocking(pos)) {
                return new IsoRayHit(true, pos, distanceTraveled, this);
            }
        }

        return new IsoRayHit(false, GetEndpoint(), _maxDistance, this);
    }

    /// <summary>
    /// Casts the ray to a specific level, returning the first solid position found.
    /// </summary>
    /// <param name="targetLevel">The level to find.</param>
    /// <param name="hasTile">Function to check if a position has a tile.</param>
    public IsoRayHit CastToLevel(IsoLevel targetLevel, Func<IsoPos, bool> hasTile) {
        foreach (var pos in Trace(includeOrigin: false)) {
            if (pos.Level == targetLevel && hasTile(pos)) {
                float distance = _origin.DistanceTo(pos).ToMovementCost();
                return new IsoRayHit(true, pos, distance, this);
            }

            // If we've passed the target level going the wrong way, stop
            if (GoesDown && pos.Level < targetLevel) break;
            if (GoesUp && pos.Level > targetLevel) break;
        }

        return new IsoRayHit(false, GetEndpoint(), _maxDistance, this);
    }

    /// <summary>
    /// Casts the ray to ground level.
    /// </summary>
    public IsoRayHit CastToGround(Func<IsoPos, bool> hasTile) =>
        CastToLevel(IsoLevel.Ground, hasTile);

    /// <summary>
    /// Casts the ray and returns all hits (piercing ray).
    /// </summary>
    /// <param name="isBlocking">Function to check if a position blocks/hits.</param>
    /// <param name="maxHits">Maximum number of hits to return.</param>
    public IEnumerable<IsoRayHit> CastAll(Func<IsoPos, bool> isBlocking, int maxHits = int.MaxValue) {
        int hitCount = 0;

        foreach (var pos in Trace(includeOrigin: false)) {
            if (isBlocking(pos)) {
                float distance = _origin.DistanceTo(pos).ToMovementCost();
                yield return new IsoRayHit(true, pos, distance, this);

                hitCount++;
                if (hitCount >= maxHits) yield break;
            }
        }
    }

    /// <summary>
    /// Checks line of sight to a target.
    /// </summary>
    /// <param name="target">Target position.</param>
    /// <param name="isBlocking">Function to check if a position blocks sight.</param>
    public bool HasLineOfSight(IsoPos target, Func<IsoPos, bool> isBlocking) {
        var rayToTarget = new IsoRay(_origin, target);

        foreach (var pos in rayToTarget.Trace(includeOrigin: false)) {
            if (pos == target) return true;
            if (isBlocking(pos)) return false;
        }

        return true;
    }

    /// <summary>
    /// Checks line of sight on the horizontal plane only.
    /// </summary>
    public bool HasLineOfSightXY(IsoPos target, Func<IsoPos, bool> isBlocking) {
        foreach (var pos in _origin.LineTo(target, includeStart: false, includeEnd: false)) {
            if (isBlocking(pos)) return false;
        }
        return true;
    }

    #endregion

    #region Instance Methods - Modifications

    /// <summary>
    /// Returns a new ray with a different origin.
    /// </summary>
    public IsoRay WithOrigin(IsoPos origin) => new(origin, _direction, _maxDistance);

    /// <summary>
    /// Returns a new ray with a different direction.
    /// </summary>
    public IsoRay WithDirection(IsoRel direction) => new(_origin, direction, _maxDistance);

    /// <summary>
    /// Returns a new ray with a different direction.
    /// </summary>
    public IsoRay WithDirection(IsoAngle direction) => new(_origin, direction.ToDirection(), _maxDistance);

    /// <summary>
    /// Returns a new ray with a different max distance.
    /// </summary>
    public IsoRay WithMaxDistance(float maxDistance) => new(_origin, _direction, maxDistance);

    /// <summary>
    /// Returns a new ray that only travels horizontally.
    /// </summary>
    public IsoRay Flattened() => new(_origin, _direction.Horizontal(), _maxDistance);

    /// <summary>
    /// Returns a new ray with the direction reversed.
    /// </summary>
    public IsoRay Reversed() => new(_origin, _direction.Opposite(), _maxDistance);

    /// <summary>
    /// Returns a new ray rotated by the specified steps (45° each).
    /// </summary>
    public IsoRay Rotated(int steps45) => new(_origin, _direction.Rotate45Steps(steps45), _maxDistance);

    #endregion

    #region Instance Methods - Conversion

    /// <summary>
    /// Converts to a Unity Ray in world space.
    /// </summary>
    public Ray ToWorldRay(float levelHeight = IsoLevel.LEVEL_HEIGHT_UNITS) {
        var worldOrigin = _origin.ToWorld(levelHeight);
        var worldDir = _direction.ToWorld(levelHeight).normalized;
        return new Ray(worldOrigin, worldDir);
    }

    /// <summary>
    /// Creates a bounds that contains the entire ray path.
    /// </summary>
    public IsoBounds ToBounds() {
        var endpoint = GetEndpoint();
        return new IsoBounds(_origin, endpoint);
    }

    #endregion

    #region Operators

    public static bool operator ==(IsoRay a, IsoRay b) =>
        a._origin == b._origin && a._direction == b._direction &&
        Math.Abs(a._maxDistance - b._maxDistance) < 0.001f;

    public static bool operator !=(IsoRay a, IsoRay b) => !(a == b);

    #endregion

    #region IEquatable

    public bool Equals(IsoRay other) => this == other;
    public override bool Equals(object? obj) => obj is IsoRay other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_origin, _direction, _maxDistance);

    #endregion

    #region IFormattable

    public override string ToString() =>
        $"Ray({_origin} → {_direction:name}, max {_maxDistance:F1})";

    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") return ToString();

        return format.ToLowerInvariant() switch {
            "short" => $"{_origin} → {_direction:short}",
            "full" => $"Origin: {_origin}, Dir: {_direction}, Max: {_maxDistance:F1}",
            _ => ToString()
        };
    }

    #endregion
}

#region IsoRayHit

/// <summary>
/// Result of an IsoRay cast operation.
/// </summary>
public readonly struct IsoRayHit : IEquatable<IsoRayHit> {
    /// <summary>True if the ray hit something.</summary>
    public bool Hit { get; }

    /// <summary>The position where the ray hit or ended.</summary>
    public IsoPos Position { get; }

    /// <summary>Distance from ray origin to hit point.</summary>
    public float Distance { get; }

    /// <summary>The ray that was cast.</summary>
    public IsoRay Ray { get; }

    /// <summary>True if the ray missed (did not hit).</summary>
    public bool Miss => !Hit;

    /// <summary>The level where the hit occurred.</summary>
    public IsoLevel Level => Position.Level;

    /// <summary>
    /// Creates a new ray hit result.
    /// </summary>
    public IsoRayHit(bool hit, IsoPos position, float distance, IsoRay ray) {
        Hit = hit;
        Position = position;
        Distance = distance;
        Ray = ray;
    }

    /// <summary>A miss result with no valid position.</summary>
    public static IsoRayHit None => new(false, IsoPos.Invalid, float.MaxValue, default);

    public bool Equals(IsoRayHit other) =>
        Hit == other.Hit && Position == other.Position && Math.Abs(Distance - other.Distance) < 0.001f;

    public override bool Equals(object? obj) => obj is IsoRayHit other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Hit, Position, Distance);

    public override string ToString() =>
        Hit ? $"Hit at {Position} (dist: {Distance:F1})" : "Miss";

    public static bool operator ==(IsoRayHit a, IsoRayHit b) => a.Equals(b);
    public static bool operator !=(IsoRayHit a, IsoRayHit b) => !a.Equals(b);
}

#endregion

#region Extension Methods

public static class IsoRayExtensions {
    /// <summary>
    /// Creates a ray from this position to a target.
    /// </summary>
    public static IsoRay RayTo(this IsoPos origin, IsoPos target) =>
        IsoRay.FromPositions(origin, target);

    /// <summary>
    /// Creates a ray from this position in a direction.
    /// </summary>
    public static IsoRay RayInDirection(this IsoPos origin, IsoAngle direction, float maxDistance = float.MaxValue) =>
        IsoRay.FromDirection(origin, direction, maxDistance);

    /// <summary>
    /// Creates a ray from this position in a direction.
    /// </summary>
    public static IsoRay RayInDirection(this IsoPos origin, IsoRel direction, float maxDistance = float.MaxValue) =>
        IsoRay.FromDirection(origin, direction, maxDistance);

    /// <summary>
    /// Checks if this position can see a target.
    /// </summary>
    public static bool CanSee(this IsoPos origin, IsoPos target, Func<IsoPos, bool> isBlocking) =>
        IsoRay.FromPositions(origin, target).HasLineOfSight(target, isBlocking);
}

#endregion