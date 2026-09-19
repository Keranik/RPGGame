using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGGame.Core.Isometric.Generation;

/// <summary>
/// A pure logic procedural terrain generator for creating natural-looking blobs
/// such as mountains, forests, lakes, and other terrain features on isometric maps.
/// 
/// <para>
/// Uses FastNoiseLite for natural variation and supports deterministic generation
/// via seed-based random number generation. Designed for dependency injection.
/// </para>
/// 
/// <example>
/// <code>
/// var generator = new FastBlobGenerator(worldSeed)
///     .SetMaxHeight(5)
///     .SetMaxDensity(0.9f)
///     .SetNoiseFrequency(0.1f)
///     .SetFalloffExponent(2.0f);
/// 
/// // Generate a mountain blob
/// generator.GenerateBlob(center, 8.Tiles(), (pos, height, density) => {
///     if (height > 0) {
///         PlaceMountainTile(pos, height);
///     }
/// });
/// 
/// // Generate a forest blob
/// generator.GenerateBlob(forestCenter, 12.Tiles(), (pos, height, density) => {
///     if (density > 0.3f) {
///         PlaceTree(pos, density);
///     }
/// });
/// </code>
/// </example>
/// </summary>
public class FastBlobGenerator {
    #region Fields

    private readonly int _baseSeed;
    private readonly FastNoiseLite _shapeNoise;
    private readonly FastNoiseLite _heightNoise;
    private readonly FastNoiseLite _densityNoise;
    private readonly FastNoiseLite _detailNoise;

    private int _maxHeight = 5;
    private float _maxDensity = 1.0f;
    private float _noiseFrequency = 0.08f;
    private float _falloffExponent = 2.0f;
    private float _edgeNoiseStrength = 0.3f;
    private float _heightNoiseStrength = 0.2f;
    private float _densityNoiseStrength = 0.25f;
    private float _minRadiusPercent = 0.1f;
    private IsoLevel _baseLevel = IsoLevel.Ground;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new FastBlobGenerator with the specified seed for deterministic generation.
    /// </summary>
    /// <param name="seed">Base seed for all noise functions. Same seed = same output.</param>
    public FastBlobGenerator(int seed) {
        _baseSeed = seed;

        // Shape noise - controls the overall blob outline irregularity
        _shapeNoise = new FastNoiseLite(seed);
        _shapeNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _shapeNoise.SetFrequency(_noiseFrequency);
        _shapeNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _shapeNoise.SetFractalOctaves(3);

        // Height noise - adds variation to mountain heights
        _heightNoise = new FastNoiseLite(seed + 1000);
        _heightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _heightNoise.SetFrequency(_noiseFrequency * 1.5f);
        _heightNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _heightNoise.SetFractalOctaves(2);

        // Density noise - adds variation to forest density
        _densityNoise = new FastNoiseLite(seed + 2000);
        _densityNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        _densityNoise.SetFrequency(_noiseFrequency * 2.0f);
        _densityNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        _densityNoise.SetFractalOctaves(2);

        // Detail noise - fine-grained variation for natural look
        _detailNoise = new FastNoiseLite(seed + 3000);
        _detailNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _detailNoise.SetFrequency(_noiseFrequency * 4.0f);
    }

    #endregion

    #region Fluent Configuration

    /// <summary>
    /// Sets the maximum height for mountain generation (in levels).
    /// </summary>
    /// <param name="maxHeight">Maximum height in levels (1-10 recommended).</param>
    /// <returns>This generator for method chaining.</returns>
    public FastBlobGenerator SetMaxHeight(int maxHeight) {
        _maxHeight = Mathf.Clamp(maxHeight, 1, 20);
        return this;
    }

    /// <summary>
    /// Sets the maximum density for forest/feature generation.
    /// </summary>
    /// <param name="maxDensity">Maximum density (0-1).</param>
    /// <returns>This generator for method chaining.</returns>
    public FastBlobGenerator SetMaxDensity(float maxDensity) {
        _maxDensity = Mathf.Clamp01(maxDensity);
        return this;
    }

    /// <summary>
    /// Sets the noise frequency for terrain variation.
    /// Lower values = smoother, larger features. Higher values = more chaotic.
    /// </summary>
    /// <param name="frequency">Noise frequency (0.01-1.0 typical).</param>
    /// <returns>This generator for method chaining.</returns>
    public FastBlobGenerator SetNoiseFrequency(float frequency) {
        _noiseFrequency = Mathf.Clamp(frequency, 0.001f, 2.0f);
        UpdateNoiseFrequencies();
        return this;
    }

    /// <summary>
    /// Sets the falloff exponent controlling how quickly values decrease from center.
    /// Higher values = sharper falloff (steeper mountains, denser centers).
    /// </summary>
    /// <param name="exponent">Falloff exponent (1.0-4.0 typical).</param>
    /// <returns>This generator for method chaining.</returns>
    public FastBlobGenerator SetFalloffExponent(float exponent) {
        _falloffExponent = Mathf.Clamp(exponent, 0.5f, 6.0f);
        return this;
    }

    /// <summary>
    /// Sets how much noise affects the edge shape (irregularity).
    /// </summary>
    /// <param name="strength">Edge noise strength (0-1).</param>
    /// <returns>This generator for method chaining.</returns>
    public FastBlobGenerator SetEdgeNoiseStrength(float strength) {
        _edgeNoiseStrength = Mathf.Clamp01(strength);
        return this;
    }

    /// <summary>
    /// Sets how much noise affects height variation within the blob.
    /// </summary>
    /// <param name="strength">Height noise strength (0-1).</param>
    /// <returns>This generator for method chaining.</returns>
    public FastBlobGenerator SetHeightNoiseStrength(float strength) {
        _heightNoiseStrength = Mathf.Clamp01(strength);
        return this;
    }

    /// <summary>
    /// Sets how much noise affects density variation within the blob.
    /// </summary>
    /// <param name="strength">Density noise strength (0-1).</param>
    /// <returns>This generator for method chaining.</returns>
    public FastBlobGenerator SetDensityNoiseStrength(float strength) {
        _densityNoiseStrength = Mathf.Clamp01(strength);
        return this;
    }

    /// <summary>
    /// Sets the minimum radius percentage where features can spawn.
    /// Prevents features at the very edge of the blob.
    /// </summary>
    /// <param name="percent">Minimum radius as percent of max (0-0.5).</param>
    /// <returns>This generator for method chaining.</returns>
    public FastBlobGenerator SetMinRadiusPercent(float percent) {
        _minRadiusPercent = Mathf.Clamp(percent, 0f, 0.5f);
        return this;
    }

    /// <summary>
    /// Sets the base level for generation.
    /// </summary>
    /// <param name="level">Base level for the blob.</param>
    /// <returns>This generator for method chaining.</returns>
    public FastBlobGenerator SetBaseLevel(IsoLevel level) {
        _baseLevel = level;
        return this;
    }

    private void UpdateNoiseFrequencies() {
        _shapeNoise.SetFrequency(_noiseFrequency);
        _heightNoise.SetFrequency(_noiseFrequency * 1.5f);
        _densityNoise.SetFrequency(_noiseFrequency * 2.0f);
        _detailNoise.SetFrequency(_noiseFrequency * 4.0f);
    }

    #endregion

    #region Core Generation Methods

    /// <summary>
    /// Generates a terrain blob centered at the specified position.
    /// Calls the applyTile callback for each position within the blob's influence.
    /// </summary>
    /// <param name="center">Center position of the blob.</param>
    /// <param name="radius">Maximum radius of the blob in tiles.</param>
    /// <param name="applyTile">
    /// Callback invoked for each tile: (IsoPos position, int height, float density).
    /// Height is 0 to maxHeight (for mountains).
    /// Density is 0 to maxDensity (for forests/features).
    /// </param>
    public void GenerateBlob(IsoPos center, TilesRPG radius, Action<IsoPos, int, float> applyTile) {
        if (applyTile == null) return;
        if (radius.Value <= 0) return;

        int radiusInt = radius.ToInt();
        float radiusFloat = radius.Value;

        // Iterate over bounding square, cull outside radius
        for (int dx = -radiusInt; dx <= radiusInt; dx++) {
            for (int dy = -radiusInt; dy <= radiusInt; dy++) {
                IsoPos pos = center.Offset(dx, dy);
                
                // Calculate distance from center
                float distX = dx;
                float distY = dy;
                float distanceFromCenter = Mathf.Sqrt(distX * distX + distY * distY);

                // Early cull: skip if clearly outside radius
                if (distanceFromCenter > radiusFloat * 1.5f) continue;

                // Calculate blob values for this position
                var (height, density) = CalculateBlobValues(pos, center, radiusFloat, distanceFromCenter);

                // Only invoke callback if within effective blob area
                if (height > 0 || density > 0.01f) {
                    applyTile(pos, height, density);
                }
            }
        }
    }

    /// <summary>
    /// Generates a terrain blob and returns all affected positions with their values.
    /// Useful when you need to process results before applying.
    /// </summary>
    /// <param name="center">Center position of the blob.</param>
    /// <param name="radius">Maximum radius of the blob in tiles.</param>
    /// <returns>List of tuples containing (position, height, density) for each affected tile.</returns>
    public List<(IsoPos Position, int Height, float Density)> GenerateBlobData(IsoPos center, TilesRPG radius) {
        var results = new List<(IsoPos, int, float)>();
        
        GenerateBlob(center, radius, (pos, height, density) => {
            results.Add((pos, height, density));
        });

        return results;
    }

    /// <summary>
    /// Generates a blob within specified bounds, useful for chunk-based generation.
    /// </summary>
    /// <param name="center">Center position of the blob.</param>
    /// <param name="radius">Maximum radius of the blob in tiles.</param>
    /// <param name="bounds">Bounds to constrain generation within.</param>
    /// <param name="applyTile">Callback for each affected tile.</param>
    public void GenerateBlobInBounds(IsoPos center, TilesRPG radius, IsoBounds bounds, 
        Action<IsoPos, int, float> applyTile) {
        
        if (applyTile == null) return;
        if (radius.Value <= 0) return;

        int radiusInt = radius.ToInt();
        float radiusFloat = radius.Value;

        // Calculate intersection of blob bounding box with provided bounds
        int minX = Math.Max(center.X - radiusInt, bounds.Min.X);
        int maxX = Math.Min(center.X + radiusInt, bounds.Max.X);
        int minY = Math.Max(center.Y - radiusInt, bounds.Min.Y);
        int maxY = Math.Min(center.Y + radiusInt, bounds.Max.Y);

        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                IsoPos pos = new IsoPos(x, y, _baseLevel);
                
                int dx = x - center.X;
                int dy = y - center.Y;
                float distanceFromCenter = Mathf.Sqrt(dx * dx + dy * dy);

                if (distanceFromCenter > radiusFloat * 1.5f) continue;

                var (height, density) = CalculateBlobValues(pos, center, radiusFloat, distanceFromCenter);

                if (height > 0 || density > 0.01f) {
                    applyTile(pos, height, density);
                }
            }
        }
    }

    /// <summary>
    /// Calculates blob values (height and density) for a single position.
    /// Useful for querying individual tiles without full blob generation.
    /// </summary>
    /// <param name="pos">Position to calculate values for.</param>
    /// <param name="center">Blob center.</param>
    /// <param name="radius">Blob radius.</param>
    /// <returns>Tuple of (height, density) for the position.</returns>
    public (int Height, float Density) GetBlobValuesAt(IsoPos pos, IsoPos center, TilesRPG radius) {
        float radiusFloat = radius.Value;
        int dx = pos.X - center.X;
        int dy = pos.Y - center.Y;
        float distanceFromCenter = Mathf.Sqrt(dx * dx + dy * dy);

        if (distanceFromCenter > radiusFloat * 1.5f) {
            return (0, 0f);
        }

        return CalculateBlobValues(pos, center, radiusFloat, distanceFromCenter);
    }

    #endregion

    #region Specialized Blob Types

    /// <summary>
    /// Generates a mountain blob with height-focused parameters.
    /// Configures generator for mountain-like output before generation.
    /// </summary>
    /// <param name="center">Mountain peak center.</param>
    /// <param name="radius">Mountain base radius.</param>
    /// <param name="peakHeight">Maximum peak height in levels.</param>
    /// <param name="applyTile">Callback for each affected tile (height is primary value).</param>
    public void GenerateMountain(IsoPos center, TilesRPG radius, int peakHeight, 
        Action<IsoPos, int, float> applyTile) {
        
        int originalMaxHeight = _maxHeight;
        float originalFalloff = _falloffExponent;
        float originalEdgeNoise = _edgeNoiseStrength;

        try {
            _maxHeight = peakHeight;
            _falloffExponent = 2.5f;  // Steeper mountains
            _edgeNoiseStrength = 0.25f;  // Moderate irregularity

            GenerateBlob(center, radius, applyTile);
        } finally {
            _maxHeight = originalMaxHeight;
            _falloffExponent = originalFalloff;
            _edgeNoiseStrength = originalEdgeNoise;
        }
    }

    /// <summary>
    /// Generates a forest blob with density-focused parameters.
    /// Configures generator for forest-like output before generation.
    /// </summary>
    /// <param name="center">Forest center.</param>
    /// <param name="radius">Forest extent radius.</param>
    /// <param name="maxTreeDensity">Maximum tree density (0-1).</param>
    /// <param name="applyTile">Callback for each affected tile (density is primary value).</param>
    public void GenerateForest(IsoPos center, TilesRPG radius, float maxTreeDensity, 
        Action<IsoPos, int, float> applyTile) {
        
        float originalMaxDensity = _maxDensity;
        float originalFalloff = _falloffExponent;
        float originalDensityNoise = _densityNoiseStrength;

        try {
            _maxDensity = maxTreeDensity;
            _falloffExponent = 1.5f;  // Softer edges for forests
            _densityNoiseStrength = 0.35f;  // More variation in tree placement

            GenerateBlob(center, radius, applyTile);
        } finally {
            _maxDensity = originalMaxDensity;
            _falloffExponent = originalFalloff;
            _densityNoiseStrength = originalDensityNoise;
        }
    }

    /// <summary>
    /// Generates a lake/water blob with inverted height (depth).
    /// </summary>
    /// <param name="center">Lake center.</param>
    /// <param name="radius">Lake radius.</param>
    /// <param name="maxDepth">Maximum depth in levels (returned as negative height).</param>
    /// <param name="applyTile">Callback for each affected tile (negative height = depth).</param>
    public void GenerateLake(IsoPos center, TilesRPG radius, int maxDepth, 
        Action<IsoPos, int, float> applyTile) {
        
        int originalMaxHeight = _maxHeight;
        float originalFalloff = _falloffExponent;

        try {
            _maxHeight = maxDepth;
            _falloffExponent = 3.0f;  // Steep shores

            GenerateBlob(center, radius, (pos, height, density) => {
                // Invert height to represent depth
                applyTile(pos, -height, density);
            });
        } finally {
            _maxHeight = originalMaxHeight;
            _falloffExponent = originalFalloff;
        }
    }

    /// <summary>
    /// Generates a crater with raised rim and depressed center.
    /// </summary>
    /// <param name="center">Crater center.</param>
    /// <param name="radius">Crater radius.</param>
    /// <param name="rimHeight">Height of the crater rim.</param>
    /// <param name="craterDepth">Depth of crater center (positive value).</param>
    /// <param name="applyTile">Callback for each affected tile.</param>
    public void GenerateCrater(IsoPos center, TilesRPG radius, int rimHeight, int craterDepth,
        Action<IsoPos, int, float> applyTile) {
        
        if (applyTile == null) return;

        float radiusFloat = radius.Value;
        int radiusInt = radius.ToInt();
        float rimPosition = 0.7f;  // Rim at 70% of radius

        for (int dx = -radiusInt; dx <= radiusInt; dx++) {
            for (int dy = -radiusInt; dy <= radiusInt; dy++) {
                IsoPos pos = center.Offset(dx, dy);
                float distanceFromCenter = Mathf.Sqrt(dx * dx + dy * dy);

                if (distanceFromCenter > radiusFloat) continue;

                float normalizedDist = distanceFromCenter / radiusFloat;

                // Add shape noise
                float shapeNoise = _shapeNoise.GetNoise(pos.X, pos.Y);
                float noisyDist = normalizedDist + shapeNoise * _edgeNoiseStrength * 0.2f;

                int height;
                float density;

                if (noisyDist < rimPosition * 0.5f) {
                    // Inner crater - depression
                    float depthFactor = 1f - (noisyDist / (rimPosition * 0.5f));
                    height = -Mathf.RoundToInt(craterDepth * depthFactor);
                    density = 0.1f;  // Barren crater floor
                } else if (noisyDist < rimPosition) {
                    // Transition zone - slopes up to rim
                    float rimFactor = (noisyDist - rimPosition * 0.5f) / (rimPosition * 0.5f);
                    height = Mathf.RoundToInt(Mathf.Lerp(-craterDepth * 0.3f, rimHeight, rimFactor));
                    density = 0.3f;
                } else if (noisyDist < 1.0f) {
                    // Outer slope - rim down to ground
                    float outerFactor = (noisyDist - rimPosition) / (1.0f - rimPosition);
                    height = Mathf.RoundToInt(Mathf.Lerp(rimHeight, 0, outerFactor));
                    density = Mathf.Lerp(0.3f, 0.6f, outerFactor);
                } else {
                    continue;
                }

                applyTile(pos, height, density);
            }
        }
    }

    #endregion

    #region Multi-Blob Generation

    /// <summary>
    /// Generates multiple connected blobs forming a ridge or chain.
    /// </summary>
    /// <param name="start">Starting position of the ridge.</param>
    /// <param name="end">Ending position of the ridge.</param>
    /// <param name="width">Width of the ridge in tiles.</param>
    /// <param name="blobCount">Number of blobs along the ridge.</param>
    /// <param name="applyTile">Callback for each affected tile.</param>
    public void GenerateRidge(IsoPos start, IsoPos end, TilesRPG width, int blobCount,
        Action<IsoPos, int, float> applyTile) {
        
        if (blobCount < 2) blobCount = 2;

        int dx = end.X - start.X;
        int dy = end.Y - start.Y;
        float length = Mathf.Sqrt(dx * dx + dy * dy);

        // Track all generated positions to accumulate values
        var accumulated = new Dictionary<(int, int), (int maxHeight, float maxDensity)>();

        for (int i = 0; i < blobCount; i++) {
            float t = (float)i / (blobCount - 1);
            int blobX = start.X + Mathf.RoundToInt(dx * t);
            int blobY = start.Y + Mathf.RoundToInt(dy * t);

            // Add some perpendicular noise for natural ridge shape
            float perpNoise = _shapeNoise.GetNoise(blobX * 0.1f, blobY * 0.1f) * width.Value * 0.3f;
            float perpX = -dy / length * perpNoise;
            float perpY = dx / length * perpNoise;

            IsoPos blobCenter = new IsoPos(
                blobX + Mathf.RoundToInt(perpX),
                blobY + Mathf.RoundToInt(perpY),
                _baseLevel
            );

            // Vary blob size along ridge
            float sizeVariation = 0.7f + _heightNoise.GetNoise(i * 10f, 0) * 0.3f;
            TilesRPG blobRadius = width * sizeVariation;

            GenerateBlob(blobCenter, blobRadius, (pos, height, density) => {
                var key = (pos.X, pos.Y);
                if (accumulated.TryGetValue(key, out var existing)) {
                    // Take maximum values for overlapping blobs
                    accumulated[key] = (
                        Math.Max(existing.maxHeight, height),
                        Math.Max(existing.maxDensity, density)
                    );
                } else {
                    accumulated[key] = (height, density);
                }
            });
        }

        // Apply accumulated values
        foreach (var ((x, y), (height, density)) in accumulated) {
            applyTile(new IsoPos(x, y, _baseLevel), height, density);
        }
    }

    /// <summary>
    /// Generates a cluster of randomly placed blobs.
    /// </summary>
    /// <param name="center">Center of the cluster.</param>
    /// <param name="clusterRadius">Radius within which blobs are placed.</param>
    /// <param name="blobRadius">Approximate radius of individual blobs.</param>
    /// <param name="blobCount">Number of blobs to generate.</param>
    /// <param name="applyTile">Callback for each affected tile.</param>
    public void GenerateCluster(IsoPos center, TilesRPG clusterRadius, TilesRPG blobRadius,
        int blobCount, Action<IsoPos, int, float> applyTile) {
        
		var rng = GameRandom.For("BlobGen", _baseSeed, center.X, center.Y);
        var accumulated = new Dictionary<(int, int), (int maxHeight, float maxDensity)>();

        for (int i = 0; i < blobCount; i++) {
            // Random position within cluster
            float angle = (float)(rng.NextDouble() * Math.PI * 2);
            float dist = (float)rng.NextDouble() * clusterRadius.Value;
            
            int blobX = center.X + Mathf.RoundToInt(Mathf.Cos(angle) * dist);
            int blobY = center.Y + Mathf.RoundToInt(Mathf.Sin(angle) * dist);
            IsoPos blobCenter = new IsoPos(blobX, blobY, _baseLevel);

            // Randomize blob size
            float sizeVariation = 0.5f + (float)rng.NextDouble() * 0.5f;
            TilesRPG thisBlobRadius = blobRadius * sizeVariation;

            GenerateBlob(blobCenter, thisBlobRadius, (pos, height, density) => {
                var key = (pos.X, pos.Y);
                if (accumulated.TryGetValue(key, out var existing)) {
                    accumulated[key] = (
                        Math.Max(existing.maxHeight, height),
                        Math.Max(existing.maxDensity, density)
                    );
                } else {
                    accumulated[key] = (height, density);
                }
            });
        }

        foreach (var ((x, y), (height, density)) in accumulated) {
            applyTile(new IsoPos(x, y, _baseLevel), height, density);
        }
    }

    #endregion

    #region Internal Calculations

    private (int Height, float Density) CalculateBlobValues(IsoPos pos, IsoPos center, 
        float radius, float distanceFromCenter) {
        
        // Sample shape noise for edge variation
        float shapeNoise = _shapeNoise.GetNoise(pos.X, pos.Y);
        
        // Calculate effective radius with noise
        float effectiveRadius = radius * (1f + shapeNoise * _edgeNoiseStrength);
        
        // Check if outside effective blob area
        if (distanceFromCenter > effectiveRadius) {
            return (0, 0f);
        }

        // Calculate normalized distance (0 at center, 1 at edge)
        float normalizedDist = distanceFromCenter / effectiveRadius;

        // Apply falloff curve
        float falloff = 1f - Mathf.Pow(normalizedDist, _falloffExponent);
        falloff = Mathf.Clamp01(falloff);

        // Skip if below minimum threshold
        if (normalizedDist > (1f - _minRadiusPercent) && falloff < 0.1f) {
            return (0, 0f);
        }

        // Calculate height with noise variation
        float heightNoise = _heightNoise.GetNoise(pos.X, pos.Y);
        float heightValue = falloff * (1f + heightNoise * _heightNoiseStrength);
        int height = Mathf.RoundToInt(heightValue * _maxHeight);
        height = Mathf.Clamp(height, 0, _maxHeight);

        // Calculate density with noise variation
        float densityNoise = _densityNoise.GetNoise(pos.X, pos.Y);
        float detailNoise = _detailNoise.GetNoise(pos.X, pos.Y);
        float densityValue = falloff * (1f + densityNoise * _densityNoiseStrength);
        densityValue += detailNoise * 0.1f;  // Fine detail
        float density = Mathf.Clamp01(densityValue) * _maxDensity;

        return (height, density);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Calculates the approximate tile count for a blob of given radius.
    /// Useful for pre-allocating collections or estimating generation cost.
    /// </summary>
    /// <param name="radius">Blob radius in tiles.</param>
    /// <returns>Approximate number of tiles the blob will affect.</returns>
    public static int EstimateTileCount(TilesRPG radius) {
        float r = radius.Value;
        // Approximate area with some noise reduction factor
        return Mathf.CeilToInt(Mathf.PI * r * r * 0.7f);
    }

    /// <summary>
    /// Gets a deterministic color for a position based on its height and density.
    /// Useful for debug visualization.
    /// </summary>
    /// <param name="height">Height value (0 to maxHeight).</param>
    /// <param name="density">Density value (0 to 1).</param>
    /// <returns>A ColorRPG based on the values.</returns>
    public ColorRPG GetDebugColor(int height, float density) {
        if (height > 0) {
            // Height-based coloring: brown to white (mountain)
            float t = (float)height / _maxHeight;
            return ColorRPG.FromHSL(30f, 0.6f - t * 0.4f, 0.3f + t * 0.5f);
        } else if (density > 0.01f) {
            // Density-based coloring: light to dark green (forest)
            return ColorRPG.FromHSL(120f, 0.7f, 0.5f - density * 0.3f);
        }
        return ColorRPG.Transparent;
    }

    /// <summary>
    /// Gets debug information about the current generator configuration.
    /// </summary>
    /// <returns>Multi-line string with configuration details.</returns>
    public string GetDebugInfo() {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== FastBlobGenerator Debug ===");
        sb.AppendLine($"Base Seed: {_baseSeed}");
        sb.AppendLine($"Max Height: {_maxHeight} levels");
        sb.AppendLine($"Max Density: {_maxDensity:P0}");
        sb.AppendLine($"Noise Frequency: {_noiseFrequency:F3}");
        sb.AppendLine($"Falloff Exponent: {_falloffExponent:F2}");
        sb.AppendLine($"Edge Noise Strength: {_edgeNoiseStrength:P0}");
        sb.AppendLine($"Height Noise Strength: {_heightNoiseStrength:P0}");
        sb.AppendLine($"Density Noise Strength: {_densityNoiseStrength:P0}");
        sb.AppendLine($"Min Radius Percent: {_minRadiusPercent:P0}");
        sb.AppendLine($"Base Level: {_baseLevel}");
        return sb.ToString();
    }

    /// <summary>
    /// Creates a new generator with the same configuration but a different seed.
    /// Useful for generating multiple similar but unique blobs.
    /// </summary>
    /// <param name="newSeed">New seed for the generator.</param>
    /// <returns>A new FastBlobGenerator with identical settings but different seed.</returns>
    public FastBlobGenerator WithSeed(int newSeed) {
        return new FastBlobGenerator(newSeed)
            .SetMaxHeight(_maxHeight)
            .SetMaxDensity(_maxDensity)
            .SetNoiseFrequency(_noiseFrequency)
            .SetFalloffExponent(_falloffExponent)
            .SetEdgeNoiseStrength(_edgeNoiseStrength)
            .SetHeightNoiseStrength(_heightNoiseStrength)
            .SetDensityNoiseStrength(_densityNoiseStrength)
            .SetMinRadiusPercent(_minRadiusPercent)
            .SetBaseLevel(_baseLevel);
    }

    #endregion

    #region Gizmo Support

    /// <summary>
    /// Draws debug gizmos for a blob (call from OnDrawGizmos in a MonoBehaviour).
    /// </summary>
    /// <param name="center">Blob center position.</param>
    /// <param name="radius">Blob radius.</param>
    /// <param name="tileSize">Size of one tile in world units.</param>
    public void DrawDebugGizmos(IsoPos center, TilesRPG radius, float tileSize = 1f) {
#if UNITY_EDITOR
        GenerateBlob(center, radius, (pos, height, density) => {
            Vector3 worldPos = pos.ToWorldGridCenter(tileSize.Tiles(), 1f.Tiles());
            
            if (height > 0) {
                // Draw height as vertical line
                Gizmos.color = GetDebugColor(height, density);
                Gizmos.DrawCube(worldPos + Vector3.up * height * 0.5f, 
                    new Vector3(tileSize * 0.8f, height, tileSize * 0.8f));
            } else if (density > 0.1f) {
                // Draw density as sphere
                Gizmos.color = GetDebugColor(0, density);
                Gizmos.DrawSphere(worldPos, tileSize * density * 0.4f);
            }
        });

        // Draw blob boundary
        Gizmos.color = ColorRPG.Gold.WithAlpha(0.3f);
        Vector3 centerWorld = center.ToWorldGridCenter(tileSize.Tiles(), 1f.Tiles());
        Gizmos.DrawWireSphere(centerWorld, radius.Value * tileSize);
#endif
    }

    #endregion
}