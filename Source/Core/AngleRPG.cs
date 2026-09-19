using System;
using UnityEngine;

namespace RPGGame.Core;

/// <summary>
/// An immutable, self-documenting wrapper for degree-based angles with automatic wrapping.
/// Perfect for rotations, directions, pathfinding, and procedural generation.
/// <example>
/// <code>
/// AngleRPG heading = 45.Degrees();
/// AngleRPG turned = heading.Rotate(90);
/// Vector2 direction = heading.ToDirection2D();
/// transform.rotation = AngleRPG.East.ToQuaternion();
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct AngleRPG : IEquatable<AngleRPG>, IComparable<AngleRPG>, IFormattable {
    #region Fields

    private readonly float _degrees;

    /// <summary>Epsilon for floating-point comparisons.</summary>
    private const float EPSILON = 0.0001f;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new AngleRPG from degrees. Automatically wraps to [0, 360).
    /// </summary>
    /// <param name="degrees">Angle in degrees.</param>
    public AngleRPG(float degrees) {
        _degrees = Wrap(degrees);
    }

    #endregion

    #region Static Presets - Cardinal Directions

    /// <summary>Zero degrees.</summary>
    public static AngleRPG Zero => new(0f);

    /// <summary>North (0° / Up in 2D).</summary>
    public static AngleRPG North => new(90f);

    /// <summary>East (90° / Right in 2D). Standard 0° in math convention.</summary>
    public static AngleRPG East => new(0f);

    /// <summary>South (180° / Down in 2D).</summary>
    public static AngleRPG South => new(270f);

    /// <summary>West (270° / Left in 2D).</summary>
    public static AngleRPG West => new(180f);

    #endregion

    #region Static Presets - Ordinal Directions

    /// <summary>Northeast (45°).</summary>
    public static AngleRPG NorthEast => new(45f);

    /// <summary>Southeast (135°).</summary>
    public static AngleRPG SouthEast => new(315f);

    /// <summary>Southwest (225°).</summary>
    public static AngleRPG SouthWest => new(225f);

    /// <summary>Northwest (315°).</summary>
    public static AngleRPG NorthWest => new(135f);

    #endregion

    #region Static Presets - Aliases

    /// <summary>Right direction (same as East, 0°).</summary>
    public static AngleRPG Right => East;

    /// <summary>Up direction (same as North, 90°).</summary>
    public static AngleRPG Up => North;

    /// <summary>Left direction (same as West, 180°).</summary>
    public static AngleRPG Left => West;

    /// <summary>Down direction (same as South, 270°).</summary>
    public static AngleRPG Down => South;

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates an AngleRPG from degrees.
    /// </summary>
    /// <param name="degrees">Angle in degrees.</param>
    public static AngleRPG FromDegrees(float degrees) => new(degrees);

    /// <summary>
    /// Creates an AngleRPG from radians.
    /// </summary>
    /// <param name="radians">Angle in radians.</param>
    public static AngleRPG FromRadians(float radians) => new(radians * Mathf.Rad2Deg);

    /// <summary>
    /// Creates a random angle between 0 and 360 degrees (non-deterministic).
    /// For deterministic randomness, use <see cref="RandomStream.NextAngleRPG()"/>.
    /// </summary>
    public static AngleRPG Random() {
        uint seed = (uint)(DateTime.UtcNow.Ticks ^ Environment.TickCount);
        var rng = new RandomStream(seed == 0 ? 1 : seed);
        return rng.NextAngleRPG();
    }

    /// <summary>
    /// Creates a random angle within a range (non-deterministic).
    /// For deterministic randomness, use <see cref="RandomStream.NextAngleRPG(float, float)"/>.
    /// </summary>
    /// <param name="min">Minimum angle in degrees.</param>
    /// <param name="max">Maximum angle in degrees.</param>
    public static AngleRPG Random(float min, float max) {
        uint seed = (uint)(DateTime.UtcNow.Ticks ^ Environment.TickCount);
        var rng = new RandomStream(seed == 0 ? 1 : seed);
        return rng.NextAngleRPG(min, max);
    }

    /// <summary>
    /// Creates an AngleRPG from a 2D direction vector.
    /// </summary>
    /// <param name="direction">Direction vector (does not need to be normalized).</param>
    public static AngleRPG FromDirection(Vector2 direction) {
        if (direction.sqrMagnitude < EPSILON) return Zero;
        return new AngleRPG(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    /// <summary>
    /// Creates an AngleRPG from a 3D direction vector (XZ plane, Y-up).
    /// </summary>
    /// <param name="direction">Direction vector in 3D space.</param>
    public static AngleRPG FromDirection3D(Vector3 direction) {
        if (direction.sqrMagnitude < EPSILON) return Zero;
        return new AngleRPG(Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg);
    }

    /// <summary>
    /// Calculates the angle from one position to another (2D).
    /// </summary>
    /// <param name="from">Starting position.</param>
    /// <param name="to">Target position.</param>
    public static AngleRPG FromTo(Vector2 from, Vector2 to) {
        return FromDirection(to - from);
    }

    /// <summary>
    /// Calculates the angle from one position to another (3D, XZ plane).
    /// </summary>
    /// <param name="from">Starting position.</param>
    /// <param name="to">Target position.</param>
    public static AngleRPG FromTo3D(Vector3 from, Vector3 to) {
        return FromDirection3D(to - from);
    }

    /// <summary>
    /// Linearly interpolates between two angles (takes shortest path).
    /// </summary>
    /// <param name="a">Start angle.</param>
    /// <param name="b">End angle.</param>
    /// <param name="t">Interpolation factor (0-1).</param>
    public static AngleRPG Lerp(AngleRPG a, AngleRPG b, float t) {
        return new AngleRPG(Mathf.LerpAngle(a._degrees, b._degrees, Mathf.Clamp01(t)));
    }

    /// <summary>
    /// Linearly interpolates between two angles (unclamped).
    /// </summary>
    public static AngleRPG LerpUnclamped(AngleRPG a, AngleRPG b, float t) {
        return new AngleRPG(Mathf.LerpAngle(a._degrees, b._degrees, t));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the angle in degrees [0, 360).
    /// </summary>
    public float Degrees => _degrees;

    /// <summary>
    /// Gets the angle in radians [0, 2π).
    /// </summary>
    public float Radians => _degrees * Mathf.Deg2Rad;

    /// <summary>
    /// Gets the angle as signed degrees [-180, 180).
    /// </summary>
    public float SignedDegrees {
        get {
            float d = _degrees;
            if (d >= 180f) d -= 360f;
            return d;
        }
    }

    /// <summary>
    /// Returns the opposite angle (180° rotated).
    /// </summary>
    public AngleRPG Opposite => new(_degrees + 180f);

    /// <summary>
    /// Returns true if this angle is close to zero.
    /// </summary>
    public bool IsZero => _degrees < EPSILON || _degrees > 360f - EPSILON;

    #endregion

    #region Instance Methods - Rotation

    /// <summary>
    /// Rotates this angle by the specified degrees.
    /// </summary>
    /// <param name="degrees">Degrees to rotate (positive = counter-clockwise).</param>
    public AngleRPG Rotate(float degrees) => new(_degrees + degrees);

    /// <summary>
    /// Rotates this angle by another AngleRPG.
    /// </summary>
    public AngleRPG Rotate(AngleRPG angle) => new(_degrees + angle._degrees);

    /// <summary>
    /// Rotates clockwise by the specified degrees.
    /// </summary>
    /// <param name="degrees">Degrees to rotate clockwise.</param>
    public AngleRPG Clockwise(float degrees) => new(_degrees - degrees);

    /// <summary>
    /// Rotates counter-clockwise by the specified degrees.
    /// </summary>
    /// <param name="degrees">Degrees to rotate counter-clockwise.</param>
    public AngleRPG CounterClockwise(float degrees) => new(_degrees + degrees);

    /// <summary>
    /// Returns the shortest turn needed to face the target angle.
    /// </summary>
    /// <param name="target">Target angle to face.</param>
    /// <returns>Signed angle difference (-180 to +180).</returns>
    public float ShortestTurn(AngleRPG target) {
        float diff = target._degrees - _degrees;
        while (diff > 180f) diff -= 360f;
        while (diff < -180f) diff += 360f;
        return diff;
    }

    /// <summary>
    /// Rotates towards a target angle by a maximum amount.
    /// </summary>
    /// <param name="target">Target angle.</param>
    /// <param name="maxDelta">Maximum rotation in degrees.</param>
    public AngleRPG Towards(AngleRPG target, float maxDelta) {
        float turn = ShortestTurn(target);
        float clampedTurn = Mathf.Clamp(turn, -maxDelta, maxDelta);
        return new AngleRPG(_degrees + clampedTurn);
    }

    /// <summary>
    /// Rotates towards a target angle by a maximum AngleRPG amount.
    /// </summary>
    public AngleRPG Towards(AngleRPG target, AngleRPG maxTurn) {
        return Towards(target, maxTurn._degrees);
    }

    /// <summary>
    /// Adds random variation to this angle (non-deterministic).
    /// For deterministic randomness, use <see cref="RandomStream.WithVariance(AngleRPG, float)"/>.
    /// </summary>
    /// <param name="variance">Maximum variance in degrees (±).</param>
    public AngleRPG WithVariance(float variance) {
        uint seed = (uint)(DateTime.UtcNow.Ticks ^ Environment.TickCount);
        var rng = new RandomStream(seed == 0 ? 1 : seed);
        return rng.WithVariance(this, variance);
    }

    /// <summary>
    /// Adds random variation to this angle using a deterministic stream.
    /// </summary>
    /// <param name="variance">Maximum variance in degrees (±).</param>
    /// <param name="rng">The random stream to use.</param>
    public AngleRPG WithVariance(float variance, ref RandomStream rng) {
        return rng.WithVariance(this, variance);
    }

    #endregion

    #region Instance Methods - Conversion

    /// <summary>
    /// Converts to a 2D unit direction vector.
    /// </summary>
    public Vector2 ToDirection2D() {
        float rad = Radians;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    /// <summary>
    /// Converts to a 3D unit direction vector (XZ plane, Y = 0).
    /// </summary>
    public Vector3 ToDirection3D() {
        float rad = Radians;
        return new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
    }

    /// <summary>
    /// Converts to a Quaternion rotation (around Z axis for 2D).
    /// </summary>
    public Quaternion ToQuaternion() => Quaternion.Euler(0f, 0f, _degrees);

    /// <summary>
    /// Converts to a Quaternion rotation around the Y axis (for 3D top-down).
    /// </summary>
    public Quaternion ToQuaternionY() => Quaternion.Euler(0f, _degrees, 0f);

    /// <summary>
    /// Returns the cardinal/ordinal direction name.
    /// </summary>
    public string ToDirectionName() {
        float d = _degrees;
        return d switch {
            >= 337.5f or < 22.5f => "East",
            >= 22.5f and < 67.5f => "Northeast",
            >= 67.5f and < 112.5f => "North",
            >= 112.5f and < 157.5f => "Northwest",
            >= 157.5f and < 202.5f => "West",
            >= 202.5f and < 247.5f => "Southwest",
            >= 247.5f and < 292.5f => "South",
            _ => "Southeast"
        };
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Wraps an angle to [0, 360).
    /// </summary>
    private static float Wrap(float degrees) {
        degrees %= 360f;
        if (degrees < 0f) degrees += 360f;
        return degrees;
    }

    #endregion

    #region Operators - Implicit Conversions

    /// <summary>Implicitly converts AngleRPG to float (degrees).</summary>
    public static implicit operator float(AngleRPG a) => a._degrees;

    /// <summary>Implicitly converts float to AngleRPG.</summary>
    public static implicit operator AngleRPG(float degrees) => new(degrees);

    /// <summary>Implicitly converts int to AngleRPG.</summary>
    public static implicit operator AngleRPG(int degrees) => new(degrees);

    /// <summary>Implicitly converts AngleRPG to Quaternion (Z rotation).</summary>
    public static implicit operator Quaternion(AngleRPG a) => a.ToQuaternion();

    #endregion

    #region Operators - Arithmetic

    /// <summary>Adds two angles.</summary>
    public static AngleRPG operator +(AngleRPG a, AngleRPG b) => new(a._degrees + b._degrees);

    /// <summary>Adds degrees to an angle.</summary>
    public static AngleRPG operator +(AngleRPG a, float degrees) => new(a._degrees + degrees);

    /// <summary>Adds degrees to an angle.</summary>
    public static AngleRPG operator +(float degrees, AngleRPG a) => new(degrees + a._degrees);

    /// <summary>Subtracts two angles.</summary>
    public static AngleRPG operator -(AngleRPG a, AngleRPG b) => new(a._degrees - b._degrees);

    /// <summary>Subtracts degrees from an angle.</summary>
    public static AngleRPG operator -(AngleRPG a, float degrees) => new(a._degrees - degrees);

    /// <summary>Negates an angle (same as adding 180°).</summary>
    public static AngleRPG operator -(AngleRPG a) => a.Opposite;

    /// <summary>Multiplies an angle by a scalar.</summary>
    public static AngleRPG operator *(AngleRPG a, float scalar) => new(a._degrees * scalar);

    /// <summary>Multiplies a scalar by an angle.</summary>
    public static AngleRPG operator *(float scalar, AngleRPG a) => new(scalar * a._degrees);

    /// <summary>Divides an angle by a scalar.</summary>
    public static AngleRPG operator /(AngleRPG a, float scalar) => 
        new(scalar != 0 ? a._degrees / scalar : 0f);

    #endregion

    #region Operators - Comparison

    /// <summary>Equality comparison (with epsilon).</summary>
    public static bool operator ==(AngleRPG a, AngleRPG b) {
        float diff = Mathf.Abs(a._degrees - b._degrees);
        return diff < EPSILON || diff > 360f - EPSILON;
    }

    /// <summary>Inequality comparison.</summary>
    public static bool operator !=(AngleRPG a, AngleRPG b) => !(a == b);

    /// <summary>Less than comparison.</summary>
    public static bool operator <(AngleRPG a, AngleRPG b) => a._degrees < b._degrees;

    /// <summary>Greater than comparison.</summary>
    public static bool operator >(AngleRPG a, AngleRPG b) => a._degrees > b._degrees;

    /// <summary>Less than or equal comparison.</summary>
    public static bool operator <=(AngleRPG a, AngleRPG b) => a._degrees <= b._degrees;

    /// <summary>Greater than or equal comparison.</summary>
    public static bool operator >=(AngleRPG a, AngleRPG b) => a._degrees >= b._degrees;

    #endregion

    #region IEquatable / IComparable

    /// <inheritdoc />
    public bool Equals(AngleRPG other) => this == other;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AngleRPG other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _degrees.GetHashCode();

    /// <inheritdoc />
    public int CompareTo(AngleRPG other) => _degrees.CompareTo(other._degrees);

    #endregion

    #region IFormattable

    /// <summary>
    /// Returns a string representation: "90°"
    /// </summary>
    public override string ToString() => $"{_degrees:0.#}°";

    /// <summary>
    /// Formats the angle.
    /// Formats: null/G = "90°", "dir" = "East", "rad" = radians, numeric = custom
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") {
            return ToString();
        }

        return format.ToLowerInvariant() switch {
            "dir" or "direction" => ToDirectionName(),
            "rad" or "radians" => $"{Radians:0.###} rad",
            "signed" => $"{SignedDegrees:0.#}°",
            _ when format.EndsWith("°") => $"{_degrees.ToString(format[..^1], formatProvider)}°",
            _ => $"{_degrees.ToString(format, formatProvider)}°"
        };
    }

    #endregion
}

/// <summary>
/// Extension methods for creating AngleRPG from numeric types.
/// </summary>
public static class AngleRPGExtensions {
    /// <summary>
    /// Converts a float to AngleRPG (degrees).
    /// </summary>
    /// <example>45f.Degrees()</example>
    public static AngleRPG Degrees(this float value) => AngleRPG.FromDegrees(value);

    /// <summary>
    /// Converts an int to AngleRPG (degrees).
    /// </summary>
    /// <example>90.Degrees()</example>
    public static AngleRPG Degrees(this int value) => AngleRPG.FromDegrees(value);

    /// <summary>
    /// Converts radians to AngleRPG.
    /// </summary>
    /// <example>Mathf.PI.Radians()</example>
    public static AngleRPG Radians(this float value) => AngleRPG.FromRadians(value);

    /// <summary>
    /// Gets the angle from a Vector2 direction.
    /// </summary>
    public static AngleRPG ToAngle(this Vector2 direction) => AngleRPG.FromDirection(direction);

    /// <summary>
    /// Gets the angle from a Vector3 direction (XZ plane).
    /// </summary>
    public static AngleRPG ToAngle(this Vector3 direction) => AngleRPG.FromDirection3D(direction);

    /// <summary>
    /// Gets the angle in radians. Convenience method wrapping the Radians property.
    /// </summary>
    public static float ToRadians(this AngleRPG angle) => angle.Radians;

    /// <summary>
    /// Gets the angle in degrees. Convenience method wrapping the Degrees property.
    /// </summary>
    public static float ToDegrees(this AngleRPG angle) => angle.Degrees;
}

#region Usage Examples
/*
 * ═══════════════════════════════════════════════════════════════════════════
 * AngleRPG USAGE EXAMPLES
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // CREATION STYLES
 * // ─────────────────────────────────────────────────────────────────────────
 * AngleRPG a1 = 45;                         // Implicit from int
 * AngleRPG a2 = 45.5f;                      // Implicit from float
 * AngleRPG a3 = 90.Degrees();               // Extension method
 * AngleRPG a4 = Mathf.PI.Radians();         // From radians
 * AngleRPG a5 = AngleRPG.East;              // Preset
 * AngleRPG a6 = AngleRPG.Random();          // Random angle
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // DIRECTION VECTORS
 * // ─────────────────────────────────────────────────────────────────────────
 * Vector2 dir2D = AngleRPG.NorthEast.ToDirection2D();
 * Vector3 dir3D = heading.ToDirection3D();
 * 
 * // Get angle from direction
 * AngleRPG fromDir = AngleRPG.FromDirection(velocity.normalized);
 * AngleRPG fromVec = someVector.ToAngle();
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // ROTATION
 * // ─────────────────────────────────────────────────────────────────────────
 * AngleRPG rotated = heading.Rotate(90);
 * AngleRPG cw = heading.Clockwise(45);
 * AngleRPG ccw = heading.CounterClockwise(45);
 * AngleRPG opposite = heading.Opposite;
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // SMOOTH ENEMY TURNING
 * // ─────────────────────────────────────────────────────────────────────────
 * AngleRPG currentFacing = 45.Degrees();
 * AngleRPG targetAngle = AngleRPG.FromTo(enemyPos, playerPos);
 * float turnSpeed = 90f; // degrees per second
 * 
 * currentFacing = currentFacing.Towards(targetAngle, turnSpeed * Time.deltaTime);
 * transform.rotation = currentFacing.ToQuaternion();
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // PATH GENERATION (8 DIRECTIONS)
 * // ─────────────────────────────────────────────────────────────────────────
 * AngleRPG[] directions = {
 *     AngleRPG.East, AngleRPG.NorthEast, AngleRPG.North, AngleRPG.NorthWest,
 *     AngleRPG.West, AngleRPG.SouthWest, AngleRPG.South, AngleRPG.SouthEast
 * };
 * 
 * foreach (var dir in directions) {
 *     Vector3 offset = dir.ToDirection3D() * 2.Tiles();
 *     SpawnNode(centerPos + offset);
 * }
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // PROCEDURAL BRANCH ANGLES
 * // ─────────────────────────────────────────────────────────────────────────
 * AngleRPG forwardAngle = currentDirection;
 * AngleRPG leftBranch = forwardAngle + 30.Degrees();
 * AngleRPG rightBranch = forwardAngle - 30.Degrees();
 * 
 * AngleRPG randomBranch = forwardAngle.WithVariance(45f);
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // QUATERNION INTEGRATION
 * // ─────────────────────────────────────────────────────────────────────────
 * transform.rotation = AngleRPG.East.ToQuaternion();    // 2D rotation
 * transform.rotation = 45.Degrees().ToQuaternionY();    // 3D Y-axis rotation
 * transform.rotation = heading;                          // Implicit conversion
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // GIZMOS & DEBUGGING
 * // ─────────────────────────────────────────────────────────────────────────
 * void OnDrawGizmos() {
 *     AngleRPG facing = 45.Degrees();
 *     Vector3 dir = facing.ToDirection3D();
 *     Gizmos.DrawRay(transform.position, dir * 3.Tiles());
 *     
 *     // Draw arc
 *     for (float a = -45; a <= 45; a += 5) {
 *         AngleRPG angle = facing + a.Degrees();
 *         Gizmos.DrawRay(transform.position, angle.ToDirection3D() * 2.Tiles());
 *     }
 * }
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // STRING FORMATTING
 * // ─────────────────────────────────────────────────────────────────────────
 * AngleRPG angle = 90.Degrees();
 * Debug.Log(angle);                         // "90°"
 * Debug.Log($"{angle:dir}");                // "North"
 * Debug.Log($"{angle:000}");                // "090°"
 * Debug.Log($"{angle:rad}");                // "1.571 rad"
 * Debug.Log($"{angle:signed}");             // "90°" (or "-90°" for 270)
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // LERPING / ANIMATION
 * // ─────────────────────────────────────────────────────────────────────────
 * AngleRPG start = AngleRPG.West;
 * AngleRPG end = AngleRPG.East;
 * AngleRPG mid = AngleRPG.Lerp(start, end, 0.5f); // Takes shortest path!
 * 
 * // ─────────────────────────────────────────────────────────────────────────
 * // COMBINED WITH TilesRPG
 * // ─────────────────────────────────────────────────────────────────────────
 * AngleRPG spawnAngle = AngleRPG.Random();
 * TilesRPG spawnDistance = 5.Tiles();
 * Vector3 spawnPos = origin + spawnAngle.ToDirection3D() * spawnDistance;
 * 
 * // Isometric path node placement
 * AngleRPG pathAngle = forwardAngle.WithVariance(15f);
 * Vector3 nextNode = currentNode + pathAngle.ToDirection3D() * nodeSpacing.Tiles();
 */
#endregion