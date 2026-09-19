using System;
using System.Collections.Generic;
using System.Linq;
using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes.Expedition;
using UnityEngine;

namespace RPGGame.Core.Isometric.Generation;

/// <summary>
/// Generates natural terrain features using radial falloff with rule-based transitions.
/// Uses deterministic random streams isolated from other game systems.
/// 
/// <para>
/// Integrates with <see cref="IsoPathGenerator"/> height system and uses
/// <see cref="IsoPos"/>, <see cref="TilesRPG"/>, <see cref="IsoRel"/>, 
/// and <see cref="GridPosRPG"/> throughout.
/// </para>
/// </summary>
public class TerrainFeatureGenerator {
	#region Types

	public readonly struct TerrainRing {
		public readonly float MaxThreshold;
		public readonly TerrainProto.ID Terrain;
		public readonly int HeightOffset;

		public TerrainRing(float maxThreshold, TerrainProto.ID terrain, int heightOffset = 0) {
			MaxThreshold = Mathf.Clamp01(maxThreshold);
			Terrain = terrain;
			HeightOffset = heightOffset;
		}
	}

	public class FeatureTemplate {
		public string Name { get; init; } = "Unnamed";
		public List<TerrainRing> Rings { get; init; } = [];
		public (TilesRPG Min, TilesRPG Max) RadiusRange { get; init; } = (4.Tiles(), 12.Tiles());
		public float EdgeNoiseStrength { get; init; } = 0.25f;
		public float ShapeNoiseStrength { get; init; } = 0.3f;
		public float FalloffExponent { get; init; } = 1.5f;
		public HeightProfile HeightMode { get; init; } = HeightProfile.Flat;
		public int PeakHeight { get; init; } = 0;
		public TilesRPG MinSpacing { get; init; } = 8.Tiles();
		public Percent SpawnChance { get; init; } = 100.Percent();
		public float DangerModifier { get; init; } = 0f;
	}

	public enum HeightProfile { Flat, Peak, Depression, Crater, Slope }

	public readonly struct TerrainResult {
		public readonly TerrainProto.ID Terrain;
		public readonly int Height;
		public readonly float Density;
		public readonly bool IsFeature;
		public readonly string? FeatureName;

		public TerrainResult(
			TerrainProto.ID terrain,
			int height = 0,
			float density = 1f,
			bool isFeature = false,
			string? featureName = null
		) {
			Terrain = terrain;
			Height = height;
			Density = density;
			IsFeature = isFeature;
			FeatureName = featureName;
		}

		public static readonly TerrainResult Empty = new(new TerrainProto.ID(""), 0, 0f, false);
		public bool IsValid => !string.IsNullOrEmpty(Terrain.Value);
	}

	public readonly struct PlacedFeature {
		public readonly IsoPos Center;
		public readonly TilesRPG Radius;
		public readonly FeatureTemplate Template;
		public readonly int FeatureIndex;

		public PlacedFeature(IsoPos center, TilesRPG radius, FeatureTemplate template, int featureIndex) {
			Center = center;
			Radius = radius;
			Template = template;
			FeatureIndex = featureIndex;
		}
	}

	#endregion

	#region Starting Valley Configuration

	/// <summary>
	/// Configuration for the starting valley (steephead/blind valley).
	/// The valley follows the actual path nodes, creating a winding canyon.
	/// </summary>
	public static class ValleyConfig {
		/// <summary>Maximum wall height at the steephead (back of valley).</summary>
		public const int MaxWallHeight = 6;

		/// <summary>Minimum wall height at the valley mouth (transitions to 0).</summary>
		public const int MinWallHeight = 1;

		/// <summary>Valley floor height (same as ground level for smooth exit).</summary>
		public const int FloorHeight = 0;

		/// <summary>How far back the village sits from center (toward steephead). 0-1 range.</summary>
		public const float VillageBacksetRatio = 0.5f;

		/// <summary>Base radius of the valley floor around the village.</summary>
		public const int FloorRadius = 3;

		/// <summary>Minimum clearance (half-width) around the path.</summary>
		public const float PathClearance = 3.5f;

		/// <summary>How much extra padding behind the village for the steephead.</summary>
		public const int SteepheadDepth = 4;

		/// <summary>Wall thickness (distance from path edge to max wall height).</summary>
		public const float WallThickness = 5f;

		/// <summary>Noise strength for wall height variation (0-1).</summary>
		public const float WallNoiseStrength = 0.2f;

		/// <summary>Exponent for height falloff curve (higher = steeper walls).</summary>
		public const float HeightFalloffExponent = 0.5f;

		/// <summary>How many tiles beyond the last node to extend the valley mouth.</summary>
		public const int ValleyMouthExtension = 3;
	}

	/// <summary>
	/// Data for a placed starting valley.
	/// </summary>
	public readonly struct StartingValleyData {
		public readonly IsoPos VillageCenter;
		public readonly AngleRPG ExitDirection;
		public readonly int TutorialPathLength;
		public readonly TilesRPG ValleyRadius;
		public readonly IsoPos ExitPosition;
		public readonly bool IsPlaced;

		public StartingValleyData(
			IsoPos villageCenter,
			AngleRPG exitDirection,
			int tutorialPathLength,
			TilesRPG valleyRadius,
			IsoPos exitPosition
		) {
			VillageCenter = villageCenter;
			ExitDirection = exitDirection;
			TutorialPathLength = tutorialPathLength;
			ValleyRadius = valleyRadius;
			ExitPosition = exitPosition;
			IsPlaced = true;
		}

		public static readonly StartingValleyData None = new();
	}

	#endregion

	#region Fields

	private readonly FastNoiseLite _shapeNoise;
	private readonly FastNoiseLite _ringNoise;
	private readonly FastNoiseLite _placementNoise;
	private readonly FastNoiseLite _variationNoise;

	private readonly RandomStream _baseStream;

	// Use GridPosRPG as cache key - proper struct with GetHashCode
	private readonly Dictionary<GridPosRPG, TerrainResult> _featureCache = new();
	private readonly List<PlacedFeature> _placedFeatures = [];

	// Height cache using GridPosRPG for consistency
	private Dictionary<GridPosRPG, int>? _externalHeightCache;

	// Starting valley state
	private StartingValleyData _startingValley = StartingValleyData.None;

	// Height overrides from the starting valley (takes priority over external cache)
	private readonly Dictionary<GridPosRPG, int> _valleyHeightOverrides = new();

	#endregion

	#region Predefined Templates

	public static class Templates {
		public static readonly FeatureTemplate DeepForest = new() {
			Name = "DeepForest",
			Rings = [
				new(0.3f, Ids.Terrains.Forests.DeepForest),
				new(0.6f, Ids.Terrains.Forests.Forest),
				new(0.85f, Ids.Terrains.Forests.Forest),
				new(1.0f, Ids.Terrains.Plains.Meadow)
			],
			RadiusRange = (6.Tiles(), 15.Tiles()),
			EdgeNoiseStrength = 0.3f,
			ShapeNoiseStrength = 0.35f,
			FalloffExponent = 1.2f,
			MinSpacing = 10.Tiles(),
			DangerModifier = 0.05f
		};

		public static readonly FeatureTemplate MountainPeak = new() {
			Name = "MountainPeak",
			Rings = [
				new(0.15f, Ids.Terrains.Mountains.Peak, 3),
				new(0.4f, Ids.Terrains.Mountains.Mountain, 2),
				new(0.7f, Ids.Terrains.Plains.Hills, 1),
				new(1.0f, Ids.Terrains.Plains.Grass)
			],
			RadiusRange = (8.Tiles(), 20.Tiles()),
			EdgeNoiseStrength = 0.2f,
			ShapeNoiseStrength = 0.25f,
			FalloffExponent = 2.0f,
			HeightMode = HeightProfile.Peak,
			PeakHeight = 4,
			MinSpacing = 15.Tiles(),
			DangerModifier = 0.1f
		};

		public static readonly FeatureTemplate Lake = new() {
			Name = "Lake",
			Rings = [
				new(0.4f, Ids.Terrains.Water.Lake, -2),
				new(0.7f, Ids.Terrains.Water.Shallows, -1),
				new(0.9f, Ids.Terrains.Coastal.Beach),
				new(1.0f, Ids.Terrains.Plains.Meadow)
			],
			RadiusRange = (5.Tiles(), 12.Tiles()),
			EdgeNoiseStrength = 0.35f,
			ShapeNoiseStrength = 0.4f,
			FalloffExponent = 1.8f,
			HeightMode = HeightProfile.Depression,
			PeakHeight = 2,
			MinSpacing = 12.Tiles()
		};

		public static readonly FeatureTemplate RockyOutcrop = new() {
			Name = "RockyOutcrop",
			Rings = [
				new(0.5f, Ids.Terrains.Mountains.Cliff, 1),
				new(0.8f, Ids.Terrains.Plains.Hills),
				new(1.0f, Ids.Terrains.Plains.Grass)
			],
			RadiusRange = (3.Tiles(), 6.Tiles()),
			EdgeNoiseStrength = 0.4f,
			ShapeNoiseStrength = 0.5f,
			FalloffExponent = 2.5f,
			HeightMode = HeightProfile.Peak,
			PeakHeight = 2,
			MinSpacing = 6.Tiles()
		};

		public static readonly FeatureTemplate Swamp = new() {
			Name = "Swamp",
			Rings = [
				new(0.3f, Ids.Terrains.Wetlands.Bog, -1),
				new(0.6f, Ids.Terrains.Wetlands.Swamp),
				new(0.85f, Ids.Terrains.Wetlands.Marsh),
				new(1.0f, Ids.Terrains.Plains.Meadow)
			],
			RadiusRange = (6.Tiles(), 14.Tiles()),
			EdgeNoiseStrength = 0.45f,
			ShapeNoiseStrength = 0.5f,
			FalloffExponent = 1.0f,
			HeightMode = HeightProfile.Depression,
			PeakHeight = 1,
			MinSpacing = 10.Tiles(),
			DangerModifier = 0.08f
		};

		public static readonly FeatureTemplate Oasis = new() {
			Name = "Oasis",
			Rings = [
				new(0.2f, Ids.Terrains.Water.Pond),
				new(0.5f, Ids.Terrains.Plains.Meadow),
				new(0.75f, Ids.Terrains.Forests.Orchard),
				new(1.0f, Ids.Terrains.Desert.OpenDesert)
			],
			RadiusRange = (4.Tiles(), 8.Tiles()),
			EdgeNoiseStrength = 0.25f,
			ShapeNoiseStrength = 0.3f,
			FalloffExponent = 1.5f,
			MinSpacing = 15.Tiles(),
			SpawnChance = 30.Percent()
		};

		public static readonly FeatureTemplate River = new() {
			Name = "River",
			Rings = [
				new(0.3f, Ids.Terrains.Water.River, -1),
				new(0.6f, Ids.Terrains.Water.Shallows),
				new(0.85f, Ids.Terrains.Coastal.Beach),
				new(1.0f, Ids.Terrains.Plains.Meadow)
			],
			RadiusRange = (2.Tiles(), 4.Tiles()),
			EdgeNoiseStrength = 0.2f,
			ShapeNoiseStrength = 0.15f,
			FalloffExponent = 2.0f,
			HeightMode = HeightProfile.Depression,
			PeakHeight = 1,
			MinSpacing = 3.Tiles()
		};

		public static readonly FeatureTemplate Clearing = new() {
			Name = "Clearing",
			Rings = [
				new(0.6f, Ids.Terrains.Plains.Meadow),
				new(0.85f, Ids.Terrains.Plains.Grass),
				new(1.0f, Ids.Terrains.Forests.Forest)
			],
			RadiusRange = (4.Tiles(), 8.Tiles()),
			EdgeNoiseStrength = 0.3f,
			ShapeNoiseStrength = 0.4f,
			FalloffExponent = 1.3f,
			MinSpacing = 8.Tiles()
		};

		public static readonly FeatureTemplate Ruins = new() {
			Name = "Ruins",
			Rings = [
				new(0.4f, Ids.Terrains.Ruins.OpenRuins, 1),
				new(0.7f, Ids.Terrains.Plains.Grass),
				new(1.0f, Ids.Terrains.Plains.Meadow)
			],
			RadiusRange = (3.Tiles(), 7.Tiles()),
			EdgeNoiseStrength = 0.5f,
			ShapeNoiseStrength = 0.6f,
			FalloffExponent = 2.0f,
			HeightMode = HeightProfile.Peak,
			PeakHeight = 1,
			MinSpacing = 12.Tiles(),
			SpawnChance = 20.Percent(),
			DangerModifier = 0.15f
		};
	}

	#endregion

	#region Constructor

	/// <summary>
	/// Creates a new TerrainFeatureGenerator.
	/// Must be called after GameRandom.Initialize().
	/// </summary>
	public TerrainFeatureGenerator() {
		_baseStream = GameRandom.For("TerrainFeatures");

		int shapeSeed = _baseStream.Fork("Shape").NextInt();
		int ringSeed = _baseStream.Fork("Ring").NextInt();
		int placementSeed = _baseStream.Fork("Placement").NextInt();
		int variationSeed = _baseStream.Fork("Variation").NextInt();

		_shapeNoise = new FastNoiseLite(shapeSeed);
		_shapeNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		_shapeNoise.SetFrequency(0.08f);
		_shapeNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		_shapeNoise.SetFractalOctaves(3);

		_ringNoise = new FastNoiseLite(ringSeed);
		_ringNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		_ringNoise.SetFrequency(0.12f);
		_ringNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		_ringNoise.SetFractalOctaves(2);

		_placementNoise = new FastNoiseLite(placementSeed);
		_placementNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		_placementNoise.SetFrequency(0.02f);
		_placementNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance);

		_variationNoise = new FastNoiseLite(variationSeed);
		_variationNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		_variationNoise.SetFrequency(0.15f);
	}

	#endregion

	#region Integration with IsoPathGenerator

	/// <summary>
	/// Links this generator to an IsoPathGenerator's height cache.
	/// </summary>
	public void LinkHeightCache(IsoPathGenerator.GenerationState? state) {
		if (state?.HeightCache != null) {
			_externalHeightCache = new Dictionary<GridPosRPG, int>();
			foreach (var (pos, height) in state.HeightCache) {
				// Vector2Int → GridPosRPG (uses X, Z convention)
				_externalHeightCache[new GridPosRPG(pos.x, pos.y)] = height;
			}
		}
	}

	/// <summary>
	/// Gets height at a GridPosRPG, using valley overrides first, then linked cache.
	/// </summary>
	private int GetHeightAt(GridPosRPG pos) {
		// Valley height overrides take priority
		if (_valleyHeightOverrides.TryGetValue(pos, out int valleyHeight)) {
			return valleyHeight;
		}

		if (_externalHeightCache != null && _externalHeightCache.TryGetValue(pos, out int height)) {
			return height;
		}
		return 0;
	}

	/// <summary>
	/// Gets height at an IsoPos, converting to GridPosRPG for lookup.
	/// </summary>
	private int GetHeightAt(IsoPos pos) {
		return GetHeightAt(new GridPosRPG(pos.X, pos.Y));
	}

	/// <summary>
	/// Creates a TerrainFeatureGenerator linked to an existing path's height data.
	/// </summary>
	public static TerrainFeatureGenerator ForPath(IsoExpeditionPath path) {
		var generator = new TerrainFeatureGenerator();
		generator.LinkHeightCache(path.GenerationState);
		return generator;
	}

	/// <summary>
	/// Writes valley height overrides back to the path generator's height cache.
	/// Call this after placing the starting valley to ensure consistency.
	/// </summary>
	public void SyncValleyHeightsToPathGenerator(IsoPathGenerator.GenerationState? state) {
		if (state?.HeightCache == null) return;

		foreach (var (gridPos, height) in _valleyHeightOverrides) {
			var key = new Vector2Int(gridPos.X, gridPos.Z);
			state.HeightCache[key] = height;
		}

		Debug.Log($"TerrainFeatureGenerator: Synced {_valleyHeightOverrides.Count} valley heights to path generator");
	}

	#endregion

	#region Starting Valley Generation

	/// <summary>
	/// Places a path-following steephead valley around the tutorial path.
	/// The valley precisely follows the actual path nodes, creating a winding canyon
	/// where the path is the only way out.
	/// 
	/// <para>
	/// - Village is at the closed end (steephead) with high walls behind
	/// - Walls rise on both sides of wherever the path goes
	/// - Height gradually transitions to 0 at the last tutorial node (valley mouth)
	/// - Minimum clearance of PathClearance tiles around the path
	/// </para>
	/// </summary>
	/// <param name="villageCenter">The position of the village node.</param>
	/// <param name="exitDirection">The direction the path leads out of the valley.</param>
	/// <param name="tutorialNodeCount">Number of tutorial nodes (affects valley length).</param>
	/// <param name="tutorialNodeSpacing">Spacing between tutorial nodes in tiles.</param>
	/// <param name="pathNodes">The actual path node positions (required for path-following valley).</param>
	public void PlaceStartingValley(
		IsoPos villageCenter,
		AngleRPG exitDirection,
		int tutorialNodeCount,
		float tutorialNodeSpacing,
		List<IsoPos>? pathNodes = null
	) {
		if (pathNodes == null || pathNodes.Count < 2) {
			Debug.LogWarning("TerrainFeatureGenerator: PlaceStartingValley requires path nodes");
			return;
		}

		// Build the path spine from the actual nodes
		var pathSpine = BuildPathSpine(pathNodes);
		
		// Find bounding box of path + padding
		int minX = int.MaxValue, maxX = int.MinValue;
		int minY = int.MaxValue, maxY = int.MinValue;
		
		foreach (var node in pathNodes) {
			minX = Mathf.Min(minX, node.X);
			maxX = Mathf.Max(maxX, node.X);
			minY = Mathf.Min(minY, node.Y);
			maxY = Mathf.Max(maxY, node.Y);
		}
		
		// Add steephead behind village
		Vector2 backDir = -exitDirection.ToDirection2D().normalized;
		int steepheadX = villageCenter.X + Mathf.RoundToInt(backDir.x * ValleyConfig.SteepheadDepth);
		int steepheadY = villageCenter.Y + Mathf.RoundToInt(backDir.y * ValleyConfig.SteepheadDepth);
		minX = Mathf.Min(minX, steepheadX);
		maxX = Mathf.Max(maxX, steepheadX);
		minY = Mathf.Min(minY, steepheadY);
		maxY = Mathf.Max(maxY, steepheadY);
		
		// Expand bounds for walls
		int wallPadding = Mathf.CeilToInt(ValleyConfig.PathClearance + ValleyConfig.WallThickness + 2);
		minX -= wallPadding;
		maxX += wallPadding;
		minY -= wallPadding;
		maxY += wallPadding;
		
		TilesRPG valleyRadius = TilesRPG.FromTiles(Mathf.Max(maxX - minX, maxY - minY) / 2f);
		IsoPos exitPosition = pathNodes[^1]; // Last node is the valley mouth
		
		// Store valley data
		_startingValley = new StartingValleyData(
			villageCenter,
			exitDirection,
			tutorialNodeCount,
			valleyRadius,
			exitPosition
		);
		
		// Generate terrain for the valley area
		for (int x = minX; x <= maxX; x++) {
			for (int y = minY; y <= maxY; y++) {
				GridPosRPG gridPos = new(x, y);
				
				// Skip if already in feature cache
				if (_featureCache.ContainsKey(gridPos)) continue;
				
				// Calculate height and terrain based on distance to path spine
				var (height, terrain, isValley) = CalculatePathFollowingValleyTile(
					x, y,
					villageCenter,
					exitDirection,
					pathSpine,
					pathNodes.Count
				);
				
				if (!isValley) continue;
				
				// Store height override
				_valleyHeightOverrides[gridPos] = height;
				
				// Calculate density (for feature blending)
				float distToPath = GetDistanceToPathSpine(x, y, pathSpine);
				float density = 1f - Mathf.Clamp01(distToPath / (ValleyConfig.PathClearance + ValleyConfig.WallThickness));
				
				// Store terrain result
				_featureCache[gridPos] = new TerrainResult(
					terrain,
					height,
					density,
					isFeature: true,
					"StartingValley"
				);
			}
		}
		
		// Record as placed feature
		_placedFeatures.Add(new PlacedFeature(
			villageCenter,
			valleyRadius,
			new FeatureTemplate { Name = "StartingValley" },
			_placedFeatures.Count
		));
		
		Debug.Log($"TerrainFeatureGenerator: Placed path-following valley - " +
		          $"village={villageCenter}, exit={exitDirection.Degrees:F0}°, " +
		          $"nodes={pathNodes.Count}, tiles={_valleyHeightOverrides.Count}");
	}

	/// <summary>
	/// Represents a segment of the path spine with progress information.
	/// </summary>
	private readonly struct PathSegment {
		public readonly Vector2 Start;
		public readonly Vector2 End;
		public readonly float StartProgress; // 0 = village, 1 = exit
		public readonly float EndProgress;
		public readonly int NodeIndex;
		
		public PathSegment(Vector2 start, Vector2 end, float startProgress, float endProgress, int nodeIndex) {
			Start = start;
			End = end;
			StartProgress = startProgress;
			EndProgress = endProgress;
			NodeIndex = nodeIndex;
		}
	}

	/// <summary>
	/// Builds a list of path segments from the node positions.
	/// Each segment knows its progress along the total path (0 = start, 1 = end).
	/// </summary>
	private List<PathSegment> BuildPathSpine(List<IsoPos> nodes) {
		var segments = new List<PathSegment>();
		
		if (nodes.Count < 2) return segments;
		
		// Calculate total path length
		float totalLength = 0f;
		for (int i = 0; i < nodes.Count - 1; i++) {
			float dx = nodes[i + 1].X - nodes[i].X;
			float dy = nodes[i + 1].Y - nodes[i].Y;
			totalLength += Mathf.Sqrt(dx * dx + dy * dy);
		}
		
		if (totalLength < 0.001f) totalLength = 1f;
		
		// Build segments with progress
		float accumulatedLength = 0f;
		for (int i = 0; i < nodes.Count - 1; i++) {
			Vector2 start = new(nodes[i].X, nodes[i].Y);
			Vector2 end = new(nodes[i + 1].X, nodes[i + 1].Y);
			
			float segmentLength = Vector2.Distance(start, end);
			float startProgress = accumulatedLength / totalLength;
			accumulatedLength += segmentLength;
			float endProgress = accumulatedLength / totalLength;
			
			segments.Add(new PathSegment(start, end, startProgress, endProgress, i));
		}
		
		return segments;
	}

	/// <summary>
	/// Gets the minimum distance from a point to the path spine,
	/// and returns the progress (0-1) along the path at the closest point.
	/// </summary>
	private (float distance, float progress) GetDistanceAndProgressToSpine(float x, float y, List<PathSegment> spine) {
		float minDist = float.MaxValue;
		float progressAtMin = 0f;
		Vector2 point = new(x, y);
		
		foreach (var segment in spine) {
			var (dist, t) = DistanceToSegment(point, segment.Start, segment.End);
			
			if (dist < minDist) {
				minDist = dist;
				// Interpolate progress based on where we are along this segment
				progressAtMin = Mathf.Lerp(segment.StartProgress, segment.EndProgress, t);
			}
		}
		
		return (minDist, progressAtMin);
	}

	/// <summary>
	/// Gets just the distance to the path spine.
	/// </summary>
	private float GetDistanceToPathSpine(float x, float y, List<PathSegment> spine) {
		return GetDistanceAndProgressToSpine(x, y, spine).distance;
	}

	/// <summary>
	/// Calculates distance from a point to a line segment and returns the t parameter (0-1).
	/// </summary>
	private (float distance, float t) DistanceToSegment(Vector2 point, Vector2 segStart, Vector2 segEnd) {
		Vector2 seg = segEnd - segStart;
		float segLengthSq = seg.sqrMagnitude;
		
		if (segLengthSq < 0.0001f) {
			return (Vector2.Distance(point, segStart), 0f);
		}
		
		float t = Mathf.Clamp01(Vector2.Dot(point - segStart, seg) / segLengthSq);
		Vector2 projection = segStart + seg * t;
		
		return (Vector2.Distance(point, projection), t);
	}

	/// <summary>
	/// Calculates height and terrain for a single tile in the path-following valley.
	/// </summary>
	private (int height, TerrainProto.ID terrain, bool isValley) CalculatePathFollowingValleyTile(
		int x, int y,
		IsoPos villageCenter,
		AngleRPG exitDirection,
		List<PathSegment> pathSpine,
		int nodeCount
	) {
		// Get distance to path and progress along path
		var (distToPath, pathProgress) = GetDistanceAndProgressToSpine(x, y, pathSpine);
		
		// Check if behind the village (steephead area)
		Vector2 exitDir = exitDirection.ToDirection2D().normalized;
		Vector2 toTile = new Vector2(x - villageCenter.X, y - villageCenter.Y);
		float alongExit = Vector2.Dot(toTile, exitDir);
		float distToVillage = toTile.magnitude;
		
		// Steephead: behind the village
		bool isSteephead = alongExit < -ValleyConfig.FloorRadius;
		
		// Determine if this tile is part of the valley
		float maxValleyDist = ValleyConfig.PathClearance + ValleyConfig.WallThickness;
		
		// For steephead, use distance from village instead of path
		if (isSteephead) {
			float steepheadDist = Mathf.Max(0, distToVillage - ValleyConfig.FloorRadius);
			if (steepheadDist > maxValleyDist) {
				return (0, Ids.Terrains.Plains.Grass, false); // Outside valley
			}
			
			// Steephead walls - always maximum height
			float wallProgress = Mathf.Clamp01(steepheadDist / ValleyConfig.WallThickness);
			
			// Add noise
			float noise = _variationNoise.GetNoise(x * 2f, y * 2f) * ValleyConfig.WallNoiseStrength;
			
			int height = Mathf.RoundToInt(
				Mathf.Pow(wallProgress, ValleyConfig.HeightFalloffExponent) * ValleyConfig.MaxWallHeight + 
				noise * ValleyConfig.MaxWallHeight
			);
			height = Mathf.Clamp(height, ValleyConfig.FloorHeight, ValleyConfig.MaxWallHeight);
			
			return GetTerrainForValleyHeight(height);
		}
		
		// Beyond the valley walls
		if (distToPath > maxValleyDist) {
			return (0, Ids.Terrains.Plains.Grass, false); // Outside valley
		}
		
		// === VALLEY FLOOR (within PathClearance of path) ===
		if (distToPath <= ValleyConfig.PathClearance) {
			// Floor height transitions from FloorHeight at village to 0 at exit
			float floorHeight = Mathf.Lerp(ValleyConfig.FloorHeight, 0f, pathProgress);
			int height = Mathf.RoundToInt(floorHeight);
			
			// Terrain based on progress
			TerrainProto.ID terrain;
			if (pathProgress < 0.3f) {
				terrain = Ids.Terrains.Plains.Meadow; // Near village
			} else if (pathProgress < 0.7f) {
				terrain = Ids.Terrains.Plains.Grass; // Mid-valley
			} else {
				terrain = Ids.Terrains.Plains.Grass; // Near exit
			}
			
			return (height, terrain, true);
		}
		
		// === VALLEY WALLS (between PathClearance and maxValleyDist) ===
		float distIntoWall = distToPath - ValleyConfig.PathClearance;
		float wallProgress2 = Mathf.Clamp01(distIntoWall / ValleyConfig.WallThickness);
		
		// Wall height decreases as we approach the exit (pathProgress → 1)
		// At village (progress=0): walls go up to MaxWallHeight
		// At exit (progress=1): walls go up to 0 (no walls, open terrain)
		float maxWallAtProgress = Mathf.Lerp(ValleyConfig.MaxWallHeight, 0f, pathProgress);
		
		// Add noise for natural variation
		float noise2 = _variationNoise.GetNoise(x * 2f, y * 2f) * ValleyConfig.WallNoiseStrength;
		
		// Calculate wall height with curved falloff
		float baseHeight = Mathf.Pow(wallProgress2, ValleyConfig.HeightFalloffExponent) * maxWallAtProgress;
		int wallHeight = Mathf.RoundToInt(baseHeight + noise2 * maxWallAtProgress);
		wallHeight = Mathf.Clamp(wallHeight, ValleyConfig.FloorHeight, ValleyConfig.MaxWallHeight);
		
		return GetTerrainForValleyHeight(wallHeight);
	}

	/// <summary>
	/// Gets terrain type based on height for valley walls.
	/// </summary>
	private (int height, TerrainProto.ID terrain, bool isValley) GetTerrainForValleyHeight(int height) {
		float normalized = (float)height / ValleyConfig.MaxWallHeight;
		
		TerrainProto.ID terrain;
		if (normalized >= 0.8f) {
			terrain = Ids.Terrains.Mountains.Peak;
		} else if (normalized >= 0.6f) {
			terrain = Ids.Terrains.Mountains.Mountain;
		} else if (normalized >= 0.4f) {
			terrain = Ids.Terrains.Mountains.Cliff;
		} else if (normalized >= 0.2f) {
			terrain = Ids.Terrains.Mountains.Highland;
		} else if (normalized >= 0.1f) {
			terrain = Ids.Terrains.Plains.Hills;
		} else {
			terrain = Ids.Terrains.Plains.Grass;
		}
		
		return (height, terrain, true);
	}

	/// <summary>
	/// Returns true if a position is inside the starting valley.
	/// </summary>
	public bool IsInsideStartingValley(GridPosRPG pos) {
		return _valleyHeightOverrides.ContainsKey(pos);
	}

	/// <summary>
	/// Returns true if a position is inside the starting valley.
	/// </summary>
	public bool IsInsideStartingValley(IsoPos pos) {
		return IsInsideStartingValley(new GridPosRPG(pos.X, pos.Y));
	}

	/// <summary>
	/// Gets the starting valley data if one has been placed.
	/// </summary>
	public StartingValleyData GetStartingValley() => _startingValley;

	/// <summary>
	/// Returns true if a starting valley has been placed.
	/// </summary>
	public bool HasStartingValley => _startingValley.IsPlaced;

	#endregion

	#region Feature Placement

	/// <summary>
	/// Places a feature at the specified center position.
	/// </summary>
	public bool PlaceFeature(IsoPos center, FeatureTemplate template, TilesRPG? radiusOverride = null) {
		var rng = _baseStream.Fork(center.X, center.Y);

		if (!rng.Check(template.SpawnChance)) {
			return false;
		}

		TilesRPG radius = radiusOverride ??
			TilesRPG.FromTiles(rng.NextFloat(template.RadiusRange.Min.Value, template.RadiusRange.Max.Value));

		// Check spacing using IsoPos distance methods
		foreach (var existing in _placedFeatures) {
			if (existing.Template.Name == template.Name) {
				TilesRPG distance = center.ChebyshevDistanceXY(existing.Center);
				TilesRPG minDist = TilesRPG.Max(template.MinSpacing, existing.Radius + radius);
				if (distance < minDist) {
					return false;
				}
			}
		}

		int featureIndex = _placedFeatures.Count;
		_placedFeatures.Add(new PlacedFeature(center, radius, template, featureIndex));

		int radiusInt = radius.ToInt() + 1;

		// Use IsoRel for neighbor iteration
		for (int dx = -radiusInt; dx <= radiusInt; dx++) {
			for (int dy = -radiusInt; dy <= radiusInt; dy++) {
				IsoRel offset = new(dx, dy, 0);
				IsoPos pos = center + offset;

				var result = CalculateFeatureTerrain(pos, center, radius, template);

				if (result.IsFeature) {
					// Use GridPosRPG as dictionary key
					GridPosRPG key = new(pos.X, pos.Y);

					// Don't overwrite starting valley tiles
					if (_valleyHeightOverrides.ContainsKey(key)) continue;

					if (!_featureCache.TryGetValue(key, out var existing) ||
					    result.Density > existing.Density) {
						_featureCache[key] = result;
					}
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Automatically places features within bounds.
	/// </summary>
	public int PopulateFeatures(
		IsoBounds bounds,
		BiomeProto biome,
		int featureCount,
		List<FeatureTemplate>? allowedTemplates = null
	) {
		var rng = _baseStream.Fork(bounds.Min.X, bounds.Min.Y);
		allowedTemplates ??= GetTemplatesForBiome(biome);

		if (allowedTemplates.Count == 0) return 0;

		int placed = 0;
		int attempts = 0;
		int maxAttempts = featureCount * 10;

		while (placed < featureCount && attempts < maxAttempts) {
			attempts++;

			int x = rng.NextInt(bounds.Min.X, bounds.Max.X + 1);
			int y = rng.NextInt(bounds.Min.Y, bounds.Max.Y + 1);

			// Use GridPosRPG for height lookup
			GridPosRPG gridPos = new(x, y);

			// Skip positions inside starting valley
			if (_valleyHeightOverrides.ContainsKey(gridPos)) continue;

			int height = GetHeightAt(gridPos);

			IsoPos center = new(x, y, new IsoLevel(height));

			float placementValue = _placementNoise.GetNoise(x, y);
			if (placementValue > 0.3f) continue;

			var template = rng.Pick(allowedTemplates);

			if (PlaceFeature(center, template)) {
				placed++;
			}
		}

		return placed;
	}

	/// <summary>
	/// Populates features along a path, avoiding path nodes and the starting valley.
	/// </summary>
	public int PopulateFeaturesAlongPath(
		IsoExpeditionPath path,
		BiomeProto biome,
		int featureCount,
		TilesRPG minDistanceFromPath
	) {
		if (path.Nodes.Count == 0) return 0;

		int minX = int.MaxValue, maxX = int.MinValue;
		int minY = int.MaxValue, maxY = int.MinValue;

		foreach (var node in path.Nodes.Values) {
			minX = Math.Min(minX, node.Position.X);
			maxX = Math.Max(maxX, node.Position.X);
			minY = Math.Min(minY, node.Position.Y);
			maxY = Math.Max(maxY, node.Position.Y);
		}

		int padding = 15;
		var bounds = new IsoBounds(
			new IsoPos(minX - padding, minY - padding, IsoLevel.Ground),
			new IsoPos(maxX + padding, maxY + padding, IsoLevel.Ground)
		);

		var rng = _baseStream.Fork("PathFeatures").Fork(path.Seed, 0);
		var templates = GetTemplatesForBiome(biome);

		if (templates.Count == 0) return 0;

		int placed = 0;
		int attempts = 0;
		int maxAttempts = featureCount * 15;

		while (placed < featureCount && attempts < maxAttempts) {
			attempts++;

			int x = rng.NextInt(bounds.Min.X, bounds.Max.X + 1);
			int y = rng.NextInt(bounds.Min.Y, bounds.Max.Y + 1);
			IsoPos candidate = new(x, y, IsoLevel.Ground);

			// Skip positions inside starting valley
			if (IsInsideStartingValley(candidate)) continue;

			bool tooClose = false;
			foreach (var node in path.Nodes.Values) {
				if (candidate.ChebyshevDistanceXY(node.Position) < minDistanceFromPath) {
					tooClose = true;
					break;
				}
			}

			if (tooClose) continue;

			float placementValue = _placementNoise.GetNoise(x, y);
			if (placementValue > 0.4f) continue;

			var template = rng.Pick(templates);

			if (PlaceFeature(candidate, template)) {
				placed++;
			}
		}

		return placed;
	}

	private static List<FeatureTemplate> GetTemplatesForBiome(BiomeProto biome) {
		var templates = new List<FeatureTemplate>();
		string biomeId = biome.Id.Value.ToLower();

		if (biomeId.Contains("forest") || biomeId.Contains("wood")) {
			templates.Add(Templates.DeepForest);
			templates.Add(Templates.Lake);
			templates.Add(Templates.RockyOutcrop);
			templates.Add(Templates.Clearing);
		} else if (biomeId.Contains("mountain") || biomeId.Contains("highland")) {
			templates.Add(Templates.MountainPeak);
			templates.Add(Templates.RockyOutcrop);
			templates.Add(Templates.Lake);
		} else if (biomeId.Contains("swamp") || biomeId.Contains("marsh")) {
			templates.Add(Templates.Swamp);
			templates.Add(Templates.DeepForest);
		} else if (biomeId.Contains("desert")) {
			templates.Add(Templates.Oasis);
			templates.Add(Templates.RockyOutcrop);
			templates.Add(Templates.Ruins);
		} else if (biomeId.Contains("plain") || biomeId.Contains("grass")) {
			templates.Add(Templates.DeepForest);
			templates.Add(Templates.Lake);
			templates.Add(Templates.RockyOutcrop);
			templates.Add(Templates.Clearing);
			templates.Add(Templates.Ruins);
		} else {
			templates.Add(Templates.DeepForest);
			templates.Add(Templates.RockyOutcrop);
		}

		return templates;
	}

	#endregion

	#region Terrain Calculation

	private TerrainResult CalculateFeatureTerrain(
		IsoPos pos,
		IsoPos center,
		TilesRPG radius,
		FeatureTemplate template
	) {
		TilesRPG distanceFromCenter = pos.ChebyshevDistanceXY(center);

		float shapeNoise = _shapeNoise.GetNoise(pos.X, pos.Y);
		float effectiveRadiusValue = radius.Value * (1f + shapeNoise * template.ShapeNoiseStrength);
		TilesRPG effectiveRadius = TilesRPG.FromTiles(effectiveRadiusValue);

		if (distanceFromCenter > effectiveRadius) {
			return TerrainResult.Empty;
		}

		float normalizedDist = distanceFromCenter.Value / effectiveRadius.Value;
		float falloffDist = Mathf.Pow(normalizedDist, 1f / template.FalloffExponent);

		float ringNoise = _ringNoise.GetNoise(pos.X, pos.Y) * template.EdgeNoiseStrength;
		float perturbedDist = Mathf.Clamp01(falloffDist + ringNoise);

		TerrainProto.ID terrain = template.Rings[^1].Terrain;
		int heightOffset = 0;

		foreach (var ring in template.Rings) {
			if (perturbedDist <= ring.MaxThreshold) {
				terrain = ring.Terrain;
				heightOffset = ring.HeightOffset;
				break;
			}
		}

		int height = CalculateHeight(normalizedDist, template, heightOffset);
		height += GetHeightAt(pos);

		float density = 1f - normalizedDist;

		return new TerrainResult(terrain, height, density, isFeature: true, template.Name);
	}

	private static int CalculateHeight(float normalizedDist, FeatureTemplate template, int ringHeightOffset) {
		float heightFactor = template.HeightMode switch {
			HeightProfile.Peak => 1f - Mathf.Pow(normalizedDist, 1.5f),
			HeightProfile.Depression => Mathf.Pow(normalizedDist, 1.5f) - 1f,
			HeightProfile.Crater => normalizedDist < 0.6f
				? Mathf.Lerp(-1f, 1f, normalizedDist / 0.6f)
				: Mathf.Lerp(1f, 0f, (normalizedDist - 0.6f) / 0.4f),
			HeightProfile.Slope => 1f - normalizedDist,
			_ => 0f
		};

		int profileHeight = Mathf.RoundToInt(heightFactor * template.PeakHeight);
		return profileHeight + ringHeightOffset;
	}

	#endregion

	#region Query Methods

	/// <summary>
	/// Gets terrain result at a GridPosRPG.
	/// </summary>
	public TerrainResult? GetTerrainAt(GridPosRPG pos) {
		return _featureCache.TryGetValue(pos, out var result) ? result : null;
	}

	/// <summary>
	/// Gets terrain result at an IsoPos.
	/// </summary>
	public TerrainResult? GetTerrainAt(IsoPos pos) {
		return GetTerrainAt(new GridPosRPG(pos.X, pos.Y));
	}

	/// <summary>
	/// Gets terrain with fallback to base biome terrain.
	/// </summary>
	public TerrainResult GetTerrainWithFallback(IsoPos pos, TerrainProto.ID baseTerrain) {
		GridPosRPG key = new(pos.X, pos.Y);
		if (_featureCache.TryGetValue(key, out var result)) {
			return result;
		}
		return new TerrainResult(baseTerrain, GetHeightAt(key), 1f, isFeature: false);
	}

	/// <summary>
	/// Gets terrain with fallback using GridPosRPG.
	/// </summary>
	public TerrainResult GetTerrainWithFallback(GridPosRPG pos, TerrainProto.ID baseTerrain) {
		if (_featureCache.TryGetValue(pos, out var result)) {
			return result;
		}
		return new TerrainResult(baseTerrain, GetHeightAt(pos), 1f, isFeature: false);
	}

	/// <summary>
	/// Checks if a position is inside any feature.
	/// </summary>
	public bool IsInsideFeature(GridPosRPG pos) => _featureCache.ContainsKey(pos);

	/// <summary>
	/// Checks if an IsoPos is inside any feature.
	/// </summary>
	public bool IsInsideFeature(IsoPos pos) => IsInsideFeature(new GridPosRPG(pos.X, pos.Y));

	public IReadOnlyList<PlacedFeature> GetPlacedFeatures() => _placedFeatures;

	public IEnumerable<PlacedFeature> GetFeaturesInRange(IsoPos center, TilesRPG range) {
		foreach (var feature in _placedFeatures) {
			if (center.ChebyshevDistanceXY(feature.Center) <= range) {
				yield return feature;
			}
		}
	}

	public PlacedFeature? GetNearestFeature(IsoPos pos) {
		PlacedFeature? nearest = null;
		TilesRPG nearestDist = TilesRPG.FromTiles(float.MaxValue);

		foreach (var feature in _placedFeatures) {
			TilesRPG dist = pos.ChebyshevDistanceXY(feature.Center);
			if (dist < nearestDist) {
				nearestDist = dist;
				nearest = feature;
			}
		}

		return nearest;
	}

	public void Clear() {
		_featureCache.Clear();
		_placedFeatures.Clear();
		_valleyHeightOverrides.Clear();
		_startingValley = StartingValleyData.None;
	}

	public int FeatureCount => _placedFeatures.Count;
	public int CachedTileCount => _featureCache.Count;
	public int ValleyTileCount => _valleyHeightOverrides.Count;

	#endregion

	#region River Generation

	/// <summary>
	/// Generates a river following a path between two points.
	/// </summary>
	public void GenerateRiver(IsoPos start, IsoPos end, TilesRPG width = default, int segments = 10) {
		if (width.IsZero) width = 3.Tiles();

		var rng = _baseStream.Fork("River").Fork(start.X, start.Y);

		IsoRel totalOffset = end - start;
		TilesRPG length = start.ManhattanDistanceXY(end);

		IsoRel perpendicular = totalOffset.RotateCW().Horizontal().Normalized();

		for (int i = 0; i <= segments; i++) {
			float t = (float)i / segments;

			int x = Mathf.RoundToInt(Mathf.Lerp(start.X, end.X, t));
			int y = Mathf.RoundToInt(Mathf.Lerp(start.Y, end.Y, t));

			GridPosRPG gridPos = new(x, y);
			int height = GetHeightAt(gridPos);

			IsoPos basePos = new(x, y, new IsoLevel(height));

			float meander = _shapeNoise.GetNoise(x * 0.1f, y * 0.1f) * length.Value * 0.15f;
			IsoRel meanderOffset = perpendicular * Mathf.RoundToInt(meander);
			IsoPos meanderPos = basePos + meanderOffset;

			float localWidthValue = width.Value * (0.8f + _variationNoise.GetNoise(x, y) * 0.4f);
			TilesRPG localWidth = TilesRPG.FromTiles(localWidthValue);

			PlaceFeature(meanderPos, Templates.River, localWidth);
		}
	}

	/// <summary>
	/// Generates a river that follows terrain valleys.
	/// </summary>
	public void GenerateValleyRiver(IsoPos start, IsoPos generalDirection, TilesRPG maxLength, TilesRPG width = default) {
		if (width.IsZero) width = 3.Tiles();

		var rng = _baseStream.Fork("ValleyRiver").Fork(start.X, start.Y);

		IsoPos current = start;
		TilesRPG traveled = TilesRPG.Zero;

		IsoRel baseDirection = (generalDirection - start).Normalized();

		while (traveled < maxLength) {
			IsoPos bestNext = current;
			int lowestHeight = int.MaxValue;

			foreach (var dir in IsoRel.EightDirections) {
				float directionDot = dir.X * baseDirection.X + dir.Y * baseDirection.Y;
				if (directionDot < -0.3f) continue;

				IsoPos candidate = current + dir;
				int candidateHeight = GetHeightAt(candidate);

				if (candidateHeight < lowestHeight) {
					lowestHeight = candidateHeight;
					bestNext = candidate;
				}
			}

			if (bestNext == current) break;

			TilesRPG localWidth = TilesRPG.FromTiles(
				width.Value * (0.8f + _variationNoise.GetNoise(current.X, current.Y) * 0.4f)
			);
			PlaceFeature(current, Templates.River, localWidth);

			traveled += current.ChebyshevDistanceXY(bestNext);
			current = bestNext;
		}
	}

	#endregion

	#region Ridge/Chain Generation

	/// <summary>
	/// Generates a ridge or chain of connected features.
	/// </summary>
	public void GenerateRidge(
		IsoPos start,
		IsoPos end,
		TilesRPG featureRadius,
		FeatureTemplate template,
		int featureCount
	) {
		if (featureCount < 2) featureCount = 2;

		var rng = _baseStream.Fork("Ridge").Fork(start.X, end.Y);

		IsoRel totalOffset = end - start;
		IsoRel perpendicular = totalOffset.RotateCW().Horizontal().Normalized();

		for (int i = 0; i < featureCount; i++) {
			float t = (float)i / (featureCount - 1);
			int x = Mathf.RoundToInt(Mathf.Lerp(start.X, end.X, t));
			int y = Mathf.RoundToInt(Mathf.Lerp(start.Y, end.Y, t));

			float perpNoise = _shapeNoise.GetNoise(x * 0.1f, y * 0.1f) * featureRadius.Value * 0.3f;
			IsoRel perpOffset = perpendicular * Mathf.RoundToInt(perpNoise);

			GridPosRPG gridPos = new(x, y);
			int height = GetHeightAt(gridPos);

			IsoPos featureCenter = new IsoPos(x, y, new IsoLevel(height)) + perpOffset;

			float sizeVariation = 0.7f + rng.NextFloat() * 0.3f;
			TilesRPG localRadius = TilesRPG.FromTiles(featureRadius.Value * sizeVariation);

			PlaceFeature(featureCenter, template, localRadius);
		}
	}

	#endregion

	#region Cluster Generation

	/// <summary>
	/// Generates a cluster of randomly placed features.
	/// </summary>
	public int GenerateCluster(
		IsoPos center,
		TilesRPG clusterRadius,
		TilesRPG featureRadius,
		FeatureTemplate template,
		int featureCount
	) {
		var rng = _baseStream.Fork("Cluster").Fork(center.X, center.Y);

		int placed = 0;
		int attempts = 0;
		int maxAttempts = featureCount * 5;

		while (placed < featureCount && attempts < maxAttempts) {
			attempts++;

			AngleRPG angle = rng.NextAngleRPG();
			float dist = rng.NextFloat() * clusterRadius.Value;

			Vector2 offset = angle.ToDirection2D() * dist;
			int x = center.X + Mathf.RoundToInt(offset.x);
			int y = center.Y + Mathf.RoundToInt(offset.y);

			GridPosRPG gridPos = new(x, y);
			int height = GetHeightAt(gridPos);

			IsoPos featureCenter = new(x, y, new IsoLevel(height));

			float sizeVariation = 0.5f + rng.NextFloat() * 0.5f;
			TilesRPG localRadius = TilesRPG.FromTiles(featureRadius.Value * sizeVariation);

			if (PlaceFeature(featureCenter, template, localRadius)) {
				placed++;
			}
		}

		return placed;
	}

	#endregion

	#region Debug

	public string GetDebugInfo() {
		var sb = new System.Text.StringBuilder();
		sb.AppendLine("=== TerrainFeatureGenerator Debug ===");
		sb.AppendLine($"Features Placed: {_placedFeatures.Count}");
		sb.AppendLine($"Cached Tiles: {_featureCache.Count}");
		sb.AppendLine($"Valley Tiles: {_valleyHeightOverrides.Count}");
		sb.AppendLine($"Height Cache Linked: {_externalHeightCache != null}");
		sb.AppendLine($"Has Starting Valley: {_startingValley.IsPlaced}");

		if (_startingValley.IsPlaced) {
			sb.AppendLine($"  Valley Center: {_startingValley.VillageCenter}");
			sb.AppendLine($"  Exit Direction: {_startingValley.ExitDirection.Degrees:F0}°");
			sb.AppendLine($"  Tutorial Length: {_startingValley.TutorialPathLength}");
			sb.AppendLine($"  Radius: {_startingValley.ValleyRadius.Value:F1}");
		}

		if (_placedFeatures.Count > 0) {
			sb.AppendLine("\n--- Features by Type ---");
			var byType = _placedFeatures.GroupBy(f => f.Template.Name);
			foreach (var group in byType) {
				sb.AppendLine($"  {group.Key}: {group.Count()}");
			}
		}

		return sb.ToString();
	}

	#endregion
}