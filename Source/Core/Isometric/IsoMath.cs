using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGGame.Core.Isometric;

/// <summary>
/// Static utility methods for isometric coordinate math and common operations.
/// </summary>
public static class IsoMath {
    #region Constants

    /// <summary>Standard isometric angle (arctan(0.5) ≈ 26.565°).</summary>
    public const float ISO_ANGLE = 26.565f;

    /// <summary>Cosine of the isometric angle.</summary>
    public static readonly float ISO_COS = Mathf.Cos(ISO_ANGLE * Mathf.Deg2Rad);

    /// <summary>Sine of the isometric angle.</summary>
    public static readonly float ISO_SIN = Mathf.Sin(ISO_ANGLE * Mathf.Deg2Rad);

    /// <summary>Diagonal movement cost (√2).</summary>
    public const float DIAGONAL_COST = 1.41421356f;

    /// <summary>Level change cost multiplier.</summary>
    public const float LEVEL_COST = 2f;

    #endregion

    #region Screen/World Conversion

    /// <summary>
    /// Converts isometric tile coordinates to screen pixel position.
    /// </summary>
    /// <param name="isoX">Isometric X coordinate.</param>
    /// <param name="isoY">Isometric Y coordinate.</param>
    /// <param name="level">Vertical level.</param>
    /// <param name="tileWidth">Tile width in pixels.</param>
    /// <param name="tileHeight">Tile height in pixels.</param>
    /// <param name="levelHeight">Level height offset in pixels.</param>
    public static Vector2 IsoToScreen(
        int isoX, int isoY, int level = 0,
        float tileWidth = 64f, float tileHeight = 32f, float levelHeight = 48f) {
        float screenX = (isoX - isoY) * (tileWidth / 2f);
        float screenY = (isoX + isoY) * (tileHeight / 2f) - (level * levelHeight);
        return new Vector2(screenX, screenY);
    }

    /// <summary>
    /// Converts screen pixel position to isometric tile coordinates.
    /// </summary>
    /// <param name="screenX">Screen X position.</param>
    /// <param name="screenY">Screen Y position.</param>
    /// <param name="level">Assumed level for conversion.</param>
    /// <param name="tileWidth">Tile width in pixels.</param>
    /// <param name="tileHeight">Tile height in pixels.</param>
    /// <param name="levelHeight">Level height offset in pixels.</param>
    public static (int isoX, int isoY) ScreenToIso(
        float screenX, float screenY, int level = 0,
        float tileWidth = 64f, float tileHeight = 32f, float levelHeight = 48f) {
        // Adjust for level offset
        screenY += level * levelHeight;

        float isoX = (screenX / (tileWidth / 2f) + screenY / (tileHeight / 2f)) / 2f;
        float isoY = (screenY / (tileHeight / 2f) - screenX / (tileWidth / 2f)) / 2f;

        return (Mathf.RoundToInt(isoX), Mathf.RoundToInt(isoY));
    }

    /// <summary>
    /// Converts isometric coordinates to 3D world position.
    /// </summary>
    public static Vector3 IsoToWorld(int isoX, int isoY, int level, float levelHeight = 2f) {
        return new Vector3(isoX, level * levelHeight, isoY);
    }

    /// <summary>
    /// Converts 3D world position to isometric coordinates.
    /// </summary>
    public static (int isoX, int isoY, IsoLevel level) WorldToIso(Vector3 worldPos, float levelHeight = 2f) {
        int isoX = Mathf.RoundToInt(worldPos.x);
        int isoY = Mathf.RoundToInt(worldPos.z);
        var level = IsoLevel.FromWorldY(worldPos.y, levelHeight);
        return (isoX, isoY, level);
    }

    #endregion

    #region Distance Calculations

    /// <summary>
    /// Calculates Manhattan distance on the horizontal plane.
    /// </summary>
    public static int ManhattanDistance(int x1, int y1, int x2, int y2) =>
        Math.Abs(x2 - x1) + Math.Abs(y2 - y1);

    /// <summary>
    /// Calculates Chebyshev (chessboard) distance on the horizontal plane.
    /// </summary>
    public static int ChebyshevDistance(int x1, int y1, int x2, int y2) =>
        Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));

    /// <summary>
    /// Calculates Euclidean distance on the horizontal plane.
    /// </summary>
    public static float EuclideanDistance(int x1, int y1, int x2, int y2) {
        int dx = x2 - x1;
        int dy = y2 - y1;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Calculates squared Euclidean distance (faster for comparisons).
    /// </summary>
    public static int SqrDistance(int x1, int y1, int x2, int y2) {
        int dx = x2 - x1;
        int dy = y2 - y1;
        return dx * dx + dy * dy;
    }

    /// <summary>
    /// Calculates 3D movement cost including level changes.
    /// </summary>
    public static float MovementCost(IsoPos from, IsoPos to, float levelCostMultiplier = LEVEL_COST) {
        int dx = Math.Abs(to.X - from.X);
        int dy = Math.Abs(to.Y - from.Y);
        int dLevel = Math.Abs(to.Level - from.Level);

        // Chebyshev with diagonal approximation
        int straightMoves = Math.Abs(dx - dy);
        int diagonalMoves = Math.Min(dx, dy);
        float horizontalCost = straightMoves + diagonalMoves * DIAGONAL_COST;

        return horizontalCost + dLevel * levelCostMultiplier;
    }

    /// <summary>
    /// Checks if two positions are adjacent (8-way, same level).
    /// </summary>
    public static bool AreAdjacent(IsoPos a, IsoPos b) =>
        a.Level == b.Level && ChebyshevDistance(a.X, a.Y, b.X, b.Y) == 1;

    /// <summary>
    /// Checks if two positions are adjacent in 3D (including level).
    /// </summary>
    public static bool AreAdjacent3D(IsoPos a, IsoPos b) {
        int dLevel = Math.Abs(a.Level - b.Level);
        int dHoriz = ChebyshevDistance(a.X, a.Y, b.X, b.Y);
        return (dHoriz <= 1 && dLevel == 0) || (dHoriz == 0 && dLevel == 1);
    }

    #endregion

    #region Sorting and Rendering Order

    /// <summary>
    /// Calculates the rendering sort order for an isometric position.
    /// Higher values should be rendered later (on top).
    /// </summary>
    /// <param name="pos">The position to calculate sort order for.</param>
    /// <param name="levelWeight">Weight given to level differences.</param>
    public static int GetSortOrder(IsoPos pos, int levelWeight = 1000) {
        // Sort by: level (highest priority), then sum of X+Y (depth)
        return pos.Level.Value * levelWeight + (pos.X + pos.Y);
    }

    /// <summary>
    /// Compares two positions for rendering order.
    /// Returns negative if a should render before b.
    /// </summary>
    public static int CompareSortOrder(IsoPos a, IsoPos b) {
        // First compare by level
        int levelCompare = a.Level.Value.CompareTo(b.Level.Value);
        if (levelCompare != 0) return levelCompare;

        // Then by depth (X + Y)
        return (a.X + a.Y).CompareTo(b.X + b.Y);
    }

    /// <summary>
    /// Sorts positions for proper isometric rendering order.
    /// </summary>
    public static void SortForRendering(List<IsoPos> positions) {
        positions.Sort(CompareSortOrder);
    }

    #endregion

    #region Line and Path Utilities

    /// <summary>
    /// Generates positions along a line using Bresenham's algorithm.
    /// </summary>
    public static IEnumerable<IsoPos> BresenhamLine(IsoPos from, IsoPos to) {
        int x0 = from.X, y0 = from.Y;
        int x1 = to.X, y1 = to.Y;

        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true) {
            // Interpolate level
            float t = (dx != 0 || dy != 0)
                ? (float)(Math.Abs(x0 - from.X) + Math.Abs(y0 - from.Y)) /
                  (Math.Abs(x1 - from.X) + Math.Abs(y1 - from.Y))
                : 0f;
            int level = Mathf.RoundToInt(Mathf.Lerp(from.Level.Value, to.Level.Value, t));

            yield return new IsoPos(x0, y0, new IsoLevel(level));

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    /// <summary>
    /// Generates positions in a circle (approximation on grid).
    /// </summary>
    public static IEnumerable<IsoPos> CirclePositions(IsoPos center, int radius) {
        int radiusSqr = radius * radius;

        for (int dx = -radius; dx <= radius; dx++) {
            for (int dy = -radius; dy <= radius; dy++) {
                if (dx * dx + dy * dy <= radiusSqr) {
                    yield return center.Offset(dx, dy);
                }
            }
        }
    }

    /// <summary>
    /// Generates positions on a circle's edge.
    /// </summary>
    public static IEnumerable<IsoPos> CircleEdge(IsoPos center, int radius) {
        if (radius <= 0) {
            yield return center;
            yield break;
        }

        // Midpoint circle algorithm
        int x = radius, y = 0;
        int err = 0;

        while (x >= y) {
            yield return center.Offset(x, y);
            yield return center.Offset(y, x);
            yield return center.Offset(-y, x);
            yield return center.Offset(-x, y);
            yield return center.Offset(-x, -y);
            yield return center.Offset(-y, -x);
            yield return center.Offset(y, -x);
            yield return center.Offset(x, -y);

            y++;
            if (err <= 0) {
                err += 2 * y + 1;
            } else {
                x--;
                err += 2 * (y - x) + 1;
            }
        }
    }

    #endregion

    #region Cone and Arc

    /// <summary>
    /// Generates positions within a cone from origin facing a direction.
    /// </summary>
    /// <param name="origin">Starting position.</param>
    /// <param name="direction">Direction the cone faces.</param>
    /// <param name="range">Maximum range of the cone.</param>
    /// <param name="halfAngle">Half of the cone's angle in degrees.</param>
    public static IEnumerable<IsoPos> ConePositions(IsoPos origin, IsoAngle direction, int range, float halfAngle = 45f) {
        var dirVector = direction.ToVector2();
        float halfAngleRad = halfAngle * Mathf.Deg2Rad;

        foreach (var pos in origin.GetArea(range, includeCenter: false)) {
            var toPos = new Vector2(pos.X - origin.X, pos.Y - origin.Y);
            if (toPos.sqrMagnitude < 0.001f) continue;

            float angle = Vector2.Angle(dirVector, toPos);
            if (angle <= halfAngle) {
                yield return pos;
            }
        }
    }

    #endregion

    #region Height and Visibility

    /// <summary>
    /// Checks if a position is visible from another considering height.
    /// </summary>
    /// <param name="observer">Observer position.</param>
    /// <param name="target">Target position.</param>
    /// <param name="getHeight">Function to get terrain height at a position.</param>
    public static bool IsVisibleWithHeight(IsoPos observer, IsoPos target, Func<IsoPos, float> getHeight) {
        float observerHeight = observer.Level.Value + getHeight(observer);
        float targetHeight = target.Level.Value + getHeight(target);

        foreach (var pos in BresenhamLine(observer, target)) {
            if (pos == observer || pos == target) continue;

            float posHeight = pos.Level.Value + getHeight(pos);

            // Calculate expected height on the line at this point
            float t = observer.ManhattanDistanceXY(pos).ToFloat() / observer.ManhattanDistanceXY(target).ToFloat();
            float lineHeight = Mathf.Lerp(observerHeight, targetHeight, t);

            if (posHeight > lineHeight) {
                return false; // Blocked by terrain
            }
        }

        return true;
    }

    /// <summary>
    /// Calculates visibility mask from a position.
    /// </summary>
    public static HashSet<IsoPos> CalculateVisibility(IsoPos origin, int range, Func<IsoPos, bool> isBlocking) {
        var visible = new HashSet<IsoPos> { origin };

        foreach (var edgePos in origin.GetArea(range).Where(p => p != origin)) {
            foreach (var linePos in BresenhamLine(origin, edgePos)) {
                if (linePos == origin) continue;

                visible.Add(linePos);

                if (isBlocking(linePos)) break;
            }
        }

        return visible;
    }

    #endregion

    #region Interpolation

    /// <summary>
    /// Linearly interpolates between two positions.
    /// </summary>
    public static IsoPos Lerp(IsoPos a, IsoPos b, float t) {
        t = Mathf.Clamp01(t);
        return new IsoPos(
            Mathf.RoundToInt(Mathf.Lerp(a.X, b.X, t)),
            Mathf.RoundToInt(Mathf.Lerp(a.Y, b.Y, t)),
            new IsoLevel(Mathf.RoundToInt(Mathf.Lerp(a.Level.Value, b.Level.Value, t)))
        );
    }

    /// <summary>
    /// Smoothly interpolates between two positions using ease-in-out.
    /// </summary>
    public static IsoPos SmoothLerp(IsoPos a, IsoPos b, float t) {
        t = Mathf.Clamp01(t);
        t = t * t * (3f - 2f * t); // Smoothstep
        return Lerp(a, b, t);
    }

    #endregion

    #region Random

    /// <summary>
    /// Returns a random position within a radius of center.
    /// </summary>
    public static IsoPos RandomInRadius(IsoPos center, int radius) {
        int dx = UnityEngine.Random.Range(-radius, radius + 1);
        int dy = UnityEngine.Random.Range(-radius, radius + 1);

        // Reject if outside circle
        while (dx * dx + dy * dy > radius * radius) {
            dx = UnityEngine.Random.Range(-radius, radius + 1);
            dy = UnityEngine.Random.Range(-radius, radius + 1);
        }

        return center.Offset(dx, dy);
    }

    /// <summary>
    /// Returns a random direction.
    /// </summary>
    public static IsoRel RandomDirection() => IsoRel.Random8Way();

    /// <summary>
    /// Returns a random position on a ring at exactly the specified distance.
    /// </summary>
    public static IsoPos RandomOnRing(IsoPos center, int distance) {
        var ring = new List<IsoPos>();
        foreach (var pos in center.GetArea(distance)) {
            if (ChebyshevDistance(center.X, center.Y, pos.X, pos.Y) == distance) {
                ring.Add(pos);
            }
        }

        return ring.Count > 0 ? ring[UnityEngine.Random.Range(0, ring.Count)] : center;
    }

    #endregion

    #region LINQ Helpers for IsoPos

    /// <summary>
    /// Filters positions to those on a specific level.
    /// </summary>
    public static IEnumerable<IsoPos> OnLevel(this IEnumerable<IsoPos> positions, IsoLevel level) {
        foreach (var pos in positions) {
            if (pos.Level == level) yield return pos;
        }
    }

    /// <summary>
    /// Filters positions to those within range of a center.
    /// </summary>
    public static IEnumerable<IsoPos> WithinRange(this IEnumerable<IsoPos> positions, IsoPos center, int range) {
        foreach (var pos in positions) {
            if (center.ChebyshevDistanceXY(pos).ToInt() <= range) yield return pos;
        }
    }

    /// <summary>
    /// Orders positions by distance from a center.
    /// </summary>
    public static IEnumerable<IsoPos> OrderByDistance(this IEnumerable<IsoPos> positions, IsoPos center) {
        var list = new List<IsoPos>(positions);
        list.Sort((a, b) => center.DistanceTo(a).CompareTo(center.DistanceTo(b)));
        return list;
    }

    /// <summary>
    /// Returns the closest position to a target.
    /// </summary>
    public static IsoPos? Closest(this IEnumerable<IsoPos> positions, IsoPos target) {
        IsoPos? closest = null;
        float minDist = float.MaxValue;

        foreach (var pos in positions) {
            float dist = target.DistanceTo(pos).ToMovementCost();
            if (dist < minDist) {
                minDist = dist;
                closest = pos;
            }
        }

        return closest;
    }

    /// <summary>
    /// Returns the furthest position from a target.
    /// </summary>
    public static IsoPos? Furthest(this IEnumerable<IsoPos> positions, IsoPos target) {
        IsoPos? furthest = null;
        float maxDist = float.MinValue;

        foreach (var pos in positions) {
            float dist = target.DistanceTo(pos).ToMovementCost();
            if (dist > maxDist) {
                maxDist = dist;
                furthest = pos;
            }
        }

        return furthest;
    }

    #endregion
}