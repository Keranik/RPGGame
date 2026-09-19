using RPGGame.Core.Assets;
using RPGGame.Core.Expedition;
using RPGGame.Core.Isometric.Generation;
using RPGGame.Core.Maps;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Simulation;
using UnityEngine;

namespace RPGGame.Core.Isometric.Rendering;

/// <summary>
/// Isometric renderer for the expedition map using square sprites on XZ plane.
/// The 3D orthographic camera at 45° angle provides the isometric diamond look.
/// 
/// <para>
/// Uses incremental rendering for smooth progressive map reveal as the player travels.
/// Handles terrain, paths, nodes, fog of war, and player marker visualization.
/// </para>
/// 
/// <para>
/// Terrain generation is biome-aware: tiles near the path use the biome's terrain
/// distribution (primary + variants) with distance-based blending for natural transitions.
/// </para>
/// 
/// <para>
/// Terrain features (forests, lakes, mountains) are generated via <see cref="TerrainFeatureGenerator"/>
/// and rendered as decorative terrain blobs that don't affect gameplay.
/// </para>
/// 
/// <para>
/// Terrain graphics are loaded from AssetManager when available, with procedural
/// color-based fallbacks for missing assets.
/// </para>
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class ExpeditionIsoRenderer : IsoWorldRendererBase {
	#region Constants

	private const string ROOT_NAME = "ExpeditionIsoRoot";
	private const string TERRAIN_LAYER = "Terrain";
	private const string PATHS_LAYER = "Paths";
	private const string NODES_LAYER = "Nodes";
	private const string FOG_LAYER = "Fog";
	private const string INDICATORS_LAYER = "Indicators";

	// Rendering parameters
	private const int DEFAULT_RENDER_RADIUS = 20;
	private const int INCREMENTAL_RENDER_MARGIN = 3;
	private const int CULL_BUFFER = 5;
	private const float PATH_THRESHOLD = 1.5f;
	private const float INCREMENTAL_RENDER_DISTANCE = 4f;

	// Sprite generation
	private const int TILE_TEXTURE_SIZE = 256;
	private const int SIDE_TEXTURE_WIDTH = 256;
	private const int SIDE_TEXTURE_HEIGHT = 128;
	private const int INDICATOR_SIZE = 128;
	private const float PIXELS_PER_UNIT = 256f;

	// Player marker
	private const float PLAYER_MARKER_Y_OFFSET = 0.3f;
	private const int PLAYER_SUBLAYER = 20;

	// Heights for terrain features (in levels)
	private const int GROUND_HEIGHT = 0;
	private const int HILLS_HEIGHT = 1;
	private const int MOUNTAIN_HEIGHT = 2;

	private const int TERRAIN_SUBLAYER = 0;
	private const int PATH_SUBLAYER = 5;
	private const int FOG_SUBLAYER = 8;
	private const int NODE_SUBLAYER = 10;
	private const int INDICATOR_SUBLAYER = 11;

	// Sorting: height dominates, depth still matters within a level.
	private const int SORT_DEPTH_WEIGHT = 10;
	private const int SORT_LEVEL_WEIGHT = 10_000;

	// Periodic render tracking
	private float _timeSinceLastFullRender = 0f;
	private const float PERIODIC_RENDER_INTERVAL = 1.0f;
	private int _lastKnownNodeCount = 0;

	// Player torch for nighttime
	private int _playerTorchHandle = -1;
	private bool _torchActive = false;

	// Biome blending distances
	private const float BLEND_START = 2f;
	private const float BLEND_END = 5f;
	private const float VARIANT_START = 10f;

	// Terrain noise parameters
	private const float TERRAIN_NOISE_FREQUENCY = 0.05f;
	private const float TERRAIN_NOISE_BLEND = 0.05f;

	// Feature generation parameters
	private const int MIN_FEATURES_PER_PATH = 8;
	private const int MAX_FEATURES_PER_PATH = 25;
	private const float FEATURE_DENSITY_PER_NODE = 0.4f;

	#endregion

	#region Tile Dimensions

	/// <summary>World-space size of one tile.</summary>
	public static readonly TilesRPG WorldTileSize = TilesRPG.One;

	/// <summary>Height of one terrain level.</summary>
	private static readonly TilesRPG TerrainLevelHeight = TilesRPG.Half;

	/// <summary>Small offset to prevent z-fighting.</summary>
	private static readonly TilesRPG ZFightOffset = 0.01f.Tiles();

	/// <summary>Minimum distance from path for feature placement.</summary>
	private static readonly TilesRPG FeaturePathClearance = 3.Tiles();

	#endregion

	#region Dependencies

	private readonly ExpeditionManager _expeditionManager;
	private readonly GameStateManager _stateManager;
	private readonly GameLoop _gameLoop;
	private readonly IsoCameraController _cameraController;
	private readonly LightController _lightController;
	private readonly AssetManager _assetManager;

	#endregion

	#region State - Layers

	private Transform? _terrainLayer;
	private Transform? _pathsLayer;
	private Transform? _nodesLayer;
	private Transform? _fogLayer;
	private Transform? _indicatorsLayer;

	#endregion

	#region State - Rendered Objects

	private readonly Dictionary<Vector2Int, GameObject> _terrainTiles = new();
	private readonly Dictionary<Vector2Int, GameObject> _pathTiles = new();
	private readonly Dictionary<PathNodeId, GameObject> _nodeObjects = new();
	private readonly Dictionary<Vector2Int, GameObject> _fogTiles = new();
	private readonly Dictionary<PathNodeId, GameObject> _indicatorObjects = new();
	private GameObject? _playerMarker;

	#endregion

	#region State - Caching

	// Procedural fallback sprite caches (used when no asset graphics available)
	private readonly Dictionary<ColorRPG, Sprite> _topSpriteCache = new();
	private readonly Dictionary<ColorRPG, Sprite> _leftSideCache = new();
	private readonly Dictionary<ColorRPG, Sprite> _rightSideCache = new();
	private readonly Dictionary<string, ColorRPG> _terrainColorCache = new();
	
	// Track which terrains have asset graphics vs need fallback
	private readonly HashSet<TerrainProto.ID> _terrainGraphicsChecked = new();
	private readonly HashSet<TerrainProto.ID> _terrainUsesFallback = new();
	
	// Per-tile variant index cache for consistent variation
	private readonly Dictionary<Vector2Int, int> _tileVariantCache = new();
	
	private Sprite? _eventIndicatorSprite;
	private Sprite? _combatIndicatorSprite;
	private Sprite? _dungeonIndicatorSprite;

	// Height caching
	private readonly Dictionary<Vector2Int, int> _heightCache = new();
	private FastNoiseLite? _heightNoise;
	private FastNoiseLite? _variationNoise;

	// Biome-aware terrain caching
	private readonly Dictionary<Vector2Int, BiomeProto.ID> _biomeCache = new();
	private readonly Dictionary<Vector2Int, TerrainProto.ID> _proceduralTerrainCache = new();
	private FastNoiseLite? _terrainNoise;
	private FastNoiseLite? _blendNoise;

	// Terrain feature generation
	private TerrainFeatureGenerator? _featureGenerator;
	private bool _featuresPopulated = false;

	#endregion

	#region State - Render Tracking

	private Vector2Int _lastRenderedCenter = new(-1000, -1000);
	private Vector2Int _lastFogCenter = new(-1000, -1000);
	private readonly HashSet<Vector2Int> _renderedPositions = new();
	private Vector3 _playerCurrentPosition;
	private const float PLAYER_LERP_SPEED = 12f;

	#endregion

	#region Colors

	// Fog colors by density
	private static readonly ColorRPG FogLightColor = ColorRPG.FromHex("#808080").WithAlpha(0.4f);
	private static readonly ColorRPG FogMediumColor = ColorRPG.FromHex("#666680").WithAlpha(0.65f);
	private static readonly ColorRPG FogDenseColor = ColorRPG.FromHex("#4D4D66").WithAlpha(0.85f);
	private static readonly ColorRPG FogHeavyColor = ColorRPG.FromHex("#33334D").WithAlpha(0.95f);

	// Player colors
	private static readonly ColorRPG PlayerColor = ColorRPG.FromHex("#33B3FF");
	private static readonly ColorRPG PlayerGlowColor = ColorRPG.FromHex("#66CCFF").WithAlpha(0.5f);
	private static readonly ColorRPG PlayerBorderColor = ColorRPG.FromHex("#1A66B3");

	// Node type colors
	private static readonly Dictionary<PathNodeType, ColorRPG> NodeColors = new() {
		{ PathNodeType.Village, ColorRPG.FromHex("#F2D966") },
		{ PathNodeType.Settlement, ColorRPG.FromHex("#CCB38C") },
		{ PathNodeType.CampSite, ColorRPG.FromHex("#BF8C59") },
		{ PathNodeType.Dungeon, ColorRPG.FromHex("#805999") },
		{ PathNodeType.BossLocation, ColorRPG.FromHex("#E63333") },
		{ PathNodeType.EventLocation, ColorRPG.FromHex("#B3B34D") },
		{ PathNodeType.ResourceNode, ColorRPG.FromHex("#73BF73") },
		{ PathNodeType.Landmark, ColorRPG.FromHex("#9999BF") },
		{ PathNodeType.FogSource, ColorRPG.FromHex("#4D3373") },
		{ PathNodeType.Intersection, ColorRPG.FromHex("#A6998C") },
		{ PathNodeType.Waypoint, ColorRPG.FromHex("#8C8073") },
	};

	// Fallback terrain color
	private static readonly ColorRPG FallbackTerrainColor = ColorRPG.FromHex("#4a7c4e");

	// Indicator colors
	private static readonly ColorRPG EventIndicatorColor = ColorRPG.FromHex("#FFE64D");
	private static readonly ColorRPG CombatIndicatorColor = ColorRPG.FromHex("#E63333");
	private static readonly ColorRPG DungeonIndicatorColor = ColorRPG.FromHex("#994DB3");

	// Side wall shading
	private const float LEFT_SIDE_DARKEN = 0.30f;
	private const float RIGHT_SIDE_DARKEN = 0.15f;

	#endregion

	#region Events

	/// <summary>Fired when a path node is clicked.</summary>
	public event Action<PathNode>? OnNodeClicked;

	/// <summary>Fired when any tile is clicked.</summary>
	public event Action<Vector2Int>? OnTileClicked;

	/// <summary>Fired when a tile is hovered.</summary>
	public event Action<IsoPos>? OnTileHovered;

	/// <summary>Fired when hover leaves all tiles.</summary>
	public event Action? OnTileHoverExit;

	#endregion

	#region Constructor

	public ExpeditionIsoRenderer(
		ExpeditionManager expeditionManager,
		GameStateManager stateManager,
		GameLoop gameLoop,
		IsoCameraController cameraController,
		LightController lightController,
		AssetManager assetManager) : base() {

		_expeditionManager = expeditionManager;
		_stateManager = stateManager;
		_gameLoop = gameLoop;
		_cameraController = cameraController;
		_lightController = lightController;
		_assetManager = assetManager;

		TileWidth = WorldTileSize;
		TileHeight = WorldTileSize;
		LevelHeight = TerrainLevelHeight;

		_gameLoop.OnRenderUpdate += OnRenderUpdate;

		InitializeHeightNoise();
		InitializeTerrainNoise();
		Debug.Log("ExpeditionIsoRenderer: Created");
	}

	private void InitializeHeightNoise() {
		int seed = _expeditionManager.CurrentIsoPath?.Seed
			?? _expeditionManager.RunState?.WorldSeed
			?? 42;

		float heightFreq = 0.015f;
		float variationFreq = 0.05f;

		var generator = GetIsoPathGenerator();
		if (generator != null) {
			var config = generator.GetCurrentConfig();
			heightFreq = config.HeightNoiseFrequency;
			variationFreq = config.HeightVariationFrequency;
		}

		_heightNoise = new FastNoiseLite(seed);
		_heightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		_heightNoise.SetFrequency(heightFreq);

		_variationNoise = new FastNoiseLite(seed + 1000);
		_variationNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		_variationNoise.SetFrequency(variationFreq);

		Debug.Log($"ExpeditionIsoRenderer: Height noise initialized - seed={seed}, freq={heightFreq}");
	}

	private void InitializeTerrainNoise() {
		int seed = _expeditionManager.CurrentIsoPath?.Seed
			?? _expeditionManager.RunState?.WorldSeed
			?? 42;

		_terrainNoise = new FastNoiseLite(seed + 2000);
		_terrainNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		_terrainNoise.SetFrequency(TERRAIN_NOISE_FREQUENCY);

		_blendNoise = new FastNoiseLite(seed + 3000);
		_blendNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		_blendNoise.SetFrequency(TERRAIN_NOISE_BLEND);

		Debug.Log($"ExpeditionIsoRenderer: Terrain noise initialized - seed={seed}");
	}

	#endregion

	#region Feature Generator Integration

	/// <summary>
	/// Initializes the terrain feature generator linked to the current path.
	/// Features are deterministically generated from the world seed.
	/// Places the starting valley first, then other features.
	/// </summary>
	private void InitializeFeatureGenerator() {
		var isoPath = _expeditionManager.CurrentIsoPath;
		if (isoPath == null) {
			_featureGenerator = null;
			_featuresPopulated = false;
			return;
		}

		// Create feature generator linked to path's height cache
		_featureGenerator = TerrainFeatureGenerator.ForPath(isoPath);

		// Place starting valley FIRST before any other features
		PlaceStartingValleyIfNeeded(isoPath);

		_featuresPopulated = false;

		Debug.Log("ExpeditionIsoRenderer: Feature generator initialized");
	}

	/// <summary>
	/// Places the starting valley (steephead/blind valley) around the village.
	/// The valley follows the actual path nodes, creating a winding canyon
	/// where the tutorial path is the only way out.
	/// </summary>
	private void PlaceStartingValleyIfNeeded(IsoExpeditionPath isoPath) {
		if (_featureGenerator == null || isoPath.StartNode == null) {
			return;
		}

		var generatorState = isoPath.GenerationState;
		if (generatorState == null) {
			return;
		}

		// Get the starting direction from the path generator
		AngleRPG exitDirection = generatorState.StartingDirection;
		IsoPos villagePos = isoPath.StartNode.Position;

		// Get tutorial configuration from path generator
		var pathGen = _expeditionManager.GetIsoPathGenerator();
		var config = pathGen?.GetCurrentConfig();

		int tutorialNodeCount = config?.TutorialNodeCount ?? 5;
		float tutorialNodeSpacing = config?.TutorialNodeSpacing.Value ?? 2f;

		// Collect tutorial path node positions in order
		// These are the nodes that should be contained within the valley
		var tutorialNodes = isoPath.Nodes.Values
			.OrderBy(n => n.Id.Value) // Order by creation (monotonic IDs)
			.Take(tutorialNodeCount + 1) // Include village + tutorial nodes up to first intersection
			.Select(n => n.Position)
			.ToList();

		if (tutorialNodes.Count < 2) {
			Debug.LogWarning("ExpeditionIsoRenderer: Not enough tutorial nodes for valley placement");
			return;
		}

		// Place the path-following valley
		_featureGenerator.PlaceStartingValley(
				villagePos,
				exitDirection,
				tutorialNodeCount,
				tutorialNodeSpacing,
				tutorialNodes
			);

		// Sync valley heights back to the path generator's cache for consistency
		_featureGenerator.SyncValleyHeightsToPathGenerator(generatorState);

		Debug.Log($"ExpeditionIsoRenderer: Starting valley placed - " +
			$"village={villagePos}, exit={exitDirection.Degrees:F0}°, " +
			$"tutorialNodes={tutorialNodes.Count}, spacing={tutorialNodeSpacing:F1}");
	}

	/// <summary>
	/// Populates terrain features along the path if not already done.
	/// Uses biome data to select appropriate feature types.
	/// Avoids placing features inside the starting valley.
	/// </summary>
	private void PopulateFeaturesIfNeeded() {
		if (_featuresPopulated || _featureGenerator == null) {
			return;
		}

		var isoPath = _expeditionManager.CurrentIsoPath;
		if (isoPath == null || isoPath.Nodes.Count == 0) {
			return;
		}

		// Get current biome from generator state
		var generatorState = _expeditionManager.GetIsoPathGenerator()?.GetCurrentState();
		string? biomeId = generatorState?.BiomeState?.CurrentBiomeId;

		if (string.IsNullOrEmpty(biomeId)) {
			// Fallback to first node's biome
			var firstNode = isoPath.Nodes.Values.FirstOrDefault();
			biomeId = firstNode?.BiomeId.Value;
		}

		if (string.IsNullOrEmpty(biomeId)) {
			Debug.LogWarning("ExpeditionIsoRenderer: No biome found for feature population");
			_featuresPopulated = true;
			return;
		}

		if (!_expeditionManager.GameDb.TryGetProto(new BiomeProto.ID(biomeId), out BiomeProto? biome) || biome == null) {
			Debug.LogWarning($"ExpeditionIsoRenderer: Biome proto not found: {biomeId}");
			_featuresPopulated = true;
			return;
		}

		// Calculate feature count based on path size
		int nodeCount = isoPath.Nodes.Count;
		int targetFeatures = Mathf.Clamp(
			Mathf.RoundToInt(nodeCount * FEATURE_DENSITY_PER_NODE),
			MIN_FEATURES_PER_PATH,
			MAX_FEATURES_PER_PATH
		);

		// Populate features along the path, keeping clear of path nodes and valley
		int placedFeatures = _featureGenerator.PopulateFeaturesAlongPath(
			isoPath,
			biome,
			targetFeatures,
			FeaturePathClearance
		);

		_featuresPopulated = true;

		Debug.Log($"ExpeditionIsoRenderer: Populated {placedFeatures} terrain features " +
		          $"(target={targetFeatures}, biome={biome.DisplayText.Name}, " +
		          $"valleyTiles={_featureGenerator.ValleyTileCount})");
	}

	/// <summary>
	/// Clears all feature generator state.
	/// </summary>
	private void ClearFeatureGenerator() {
		_featureGenerator?.Clear();
		_featureGenerator = null;
		_featuresPopulated = false;
	}

	#endregion

	#region 2D to 3D Coordinate Conversion

	/// <summary>
	/// Converts a 2D grid position to a 3D IsoPos with height lookup.
	/// Height comes from features, IsoPathGenerator's height cache, or procedural noise.
	/// </summary>
	public IsoPos ToIsoPos(Vector2Int pos2D) {
		int height = GetHeightAt(pos2D);
		return new IsoPos(pos2D.x, pos2D.y, new IsoLevel(height));
	}

	/// <summary>
	/// Converts a 2D grid position to a 3D IsoPos with specified height override.
	/// </summary>
	public IsoPos ToIsoPos(Vector2Int pos2D, int height) {
		return new IsoPos(pos2D.x, pos2D.y, new IsoLevel(height));
	}

	/// <summary>
	/// Converts a 2D grid position to a 3D IsoPos with specified height override.
	/// </summary>
	public static IsoPos ToIsoPosStatic(Vector2Int pos2D, int height) {
		return new IsoPos(pos2D.x, pos2D.y, new IsoLevel(height));
	}

	/// <summary>
	/// Gets height at a position from features, path generator's height cache, or procedural.
	/// Priority: 1. Path nodes, 2. Terrain features, 3. Path generator cache, 4. Procedural
	/// </summary>
	private int GetHeightAt(Vector2Int pos2D) {
		var isoPath = _expeditionManager.CurrentIsoPath;

		// Check IsoPathNode directly for node positions (highest priority - interactive)
		if (isoPath != null) {
			var isoNode = isoPath.Nodes.Values.FirstOrDefault(n =>
				n.Position.X == pos2D.x && n.Position.Y == pos2D.y);
			if (isoNode != null) {
				return isoNode.Position.Level.Value;
			}
		}

		// Check terrain features for height overrides (decorative but affects visuals)
		if (_featureGenerator != null) {
			var featureResult = _featureGenerator.GetTerrainAt(new GridPosRPG(pos2D.x, pos2D.y));
			if (featureResult.HasValue && featureResult.Value.IsValid) {
				return featureResult.Value.Height;
			}
		}

		// Check IsoPathGenerator's height cache
		if (isoPath?.GenerationState?.HeightCache != null) {
			if (isoPath.GenerationState.HeightCache.TryGetValue(pos2D, out int cachedHeight)) {
				return cachedHeight;
			}
		}

		// Fall back to procedural height for off-path terrain
		return GetProceduralHeight(pos2D);
	}

	/// <summary>
	/// Generates procedural height for positions not covered by path generator.
	/// Uses same noise parameters as IsoPathGenerator for consistency.
	/// </summary>
	private int GetProceduralHeight(Vector2Int pos2D) {
		if (_heightCache.TryGetValue(pos2D, out int cached)) {
			return cached;
		}

		// First, try to get from the generator's height cache
		var isoPath = _expeditionManager.CurrentIsoPath;
		if (isoPath?.GenerationState?.HeightCache != null) {
			if (isoPath.GenerationState.HeightCache.TryGetValue(pos2D, out int genHeight)) {
				_heightCache[pos2D] = genHeight;
				return genHeight;
			}
		}

		// Calculate using generator's config as single source of truth
		int height = CalculateFeatureBasedHeight(pos2D);

		_heightCache[pos2D] = height;

		// Write back to generator's cache for consistency
		isoPath?.GenerationState?.HeightCache?.TryAdd(pos2D, height);

		return height;
	}

	/// <summary>
	/// Calculates height using IsoPathGenerator's config as the SINGLE SOURCE OF TRUTH.
	/// No fallbacks - if generator isn't available, returns ground level.
	/// </summary>
	private int CalculateFeatureBasedHeight(Vector2Int pos2D) {
		if (_heightNoise == null || _variationNoise == null) {
			return GROUND_HEIGHT;
		}

		// Get the generator and its config - this is the ONLY source of truth
		var generator = GetIsoPathGenerator();
		if (generator == null) {
			// No generator = no height variation, everything flat
			return GROUND_HEIGHT;
		}

		var config = generator.GetCurrentConfig();

		// Read ALL values from config - never use local defaults
		float featureThreshold = config.FeatureThreshold.Fraction;
		float valleyThreshold = -config.ValleyThreshold.Fraction;
		float amplitude = config.HeightNoiseAmplitude;
		int maxHeight = config.MaxHeightLevel;
		int minHeight = config.MinHeightLevel;

		// Feature placement noise
		float featureNoise = _heightNoise.GetNoise(pos2D.x, pos2D.y);

		// Feature-based approach from config thresholds
		if (featureNoise > featureThreshold) {
			// Elevated feature
			float featureStrength = (featureNoise - featureThreshold) / (1f - featureThreshold);
			featureStrength = Mathf.Pow(featureStrength, 0.7f);

			float localVariation = _variationNoise.GetNoise(pos2D.x, pos2D.y) * 0.3f;
			featureStrength = Mathf.Clamp01(featureStrength + localVariation);

			return Mathf.RoundToInt(featureStrength * maxHeight * amplitude);

		} else if (featureNoise < valleyThreshold) {
			// Valley
			float valleyStrength = (valleyThreshold - featureNoise) / (1f - Mathf.Abs(valleyThreshold));
			valleyStrength = Mathf.Pow(valleyStrength, 0.8f);

			float localVariation = _variationNoise.GetNoise(pos2D.x, pos2D.y) * 0.2f;
			valleyStrength = Mathf.Clamp01(valleyStrength + localVariation);

			return -Mathf.RoundToInt(valleyStrength * Mathf.Abs(minHeight) * amplitude);
		}

		// Flat ground - the default
		return GROUND_HEIGHT;
	}

	/// <summary>
	/// Gets the IsoPathGenerator instance from ExpeditionManager.
	/// Returns null if not using iso generator or not available.
	/// </summary>
	private IsoPathGenerator? GetIsoPathGenerator() {
		return _expeditionManager.GetIsoPathGenerator();
	}

	/// <summary>
	/// Converts a floating-point 2D position to world space with interpolated height.
	/// Used for smooth player movement across varying terrain.
	/// </summary>
	public Vector3 ToWorldPosition(Vector2 pos2D) {
		float tileSize = WorldTileSize.Value;
		float halfTile = tileSize * 0.5f;

		float worldX = pos2D.x * tileSize + halfTile;
		float worldZ = pos2D.y * tileSize + halfTile;

		// Bilinear interpolation of heights for smooth movement
		int x0 = Mathf.FloorToInt(pos2D.x);
		int y0 = Mathf.FloorToInt(pos2D.y);
		int x1 = x0 + 1;
		int y1 = y0 + 1;

		float tx = pos2D.x - x0;
		float ty = pos2D.y - y0;

		float levelHeight = TerrainLevelHeight.Value;
		float h00 = GetHeightAt(new Vector2Int(x0, y0)) * levelHeight;
		float h10 = GetHeightAt(new Vector2Int(x1, y0)) * levelHeight;
		float h01 = GetHeightAt(new Vector2Int(x0, y1)) * levelHeight;
		float h11 = GetHeightAt(new Vector2Int(x1, y1)) * levelHeight;

		float worldY = Mathf.Lerp(
			Mathf.Lerp(h00, h10, tx),
			Mathf.Lerp(h01, h11, tx),
			ty
		);

		return new Vector3(worldX, worldY, worldZ);
	}

	/// <summary>
	/// Converts an IsoPos to world space center of tile.
	/// </summary>
	private Vector3 IsoToWorldCenter(IsoPos pos) {
		float tileSize = WorldTileSize.Value;
		float halfTile = tileSize * 0.5f;
		float levelHeight = TerrainLevelHeight.Value;

		float worldX = pos.X * tileSize + halfTile;
		float worldZ = pos.Y * tileSize + halfTile;
		float worldY = pos.Level.Value * levelHeight;

		return new Vector3(worldX, worldY, worldZ);
	}

	/// <summary>
	/// Converts world position to 2D grid coordinates.
	/// </summary>
	private Vector2Int WorldToGrid2D(Vector3 worldPos) {
		float tileSize = WorldTileSize.Value;
		int gridX = Mathf.FloorToInt(worldPos.x / tileSize);
		int gridY = Mathf.FloorToInt(worldPos.z / tileSize);
		return new Vector2Int(gridX, gridY);
	}

	#endregion

	#region Initialization

	public override void Initialize() {
		if (IsInitialized) {
			return;
		}

		// Ensure terrain graphics are preloaded
		_assetManager.PreloadAllTerrainGraphics();

		CreateLayers();
		CreateIndicatorSprites();
		CreatePlayerMarker();
		SubscribeToEvents();

		if (!_cameraController.IsInitialized) {
			_cameraController.Initialize();
		}

		IsInitialized = true;
		IsActive = true;

		Debug.Log($"ExpeditionIsoRenderer: Initialized (terrain graphics: {_assetManager.TerrainGraphicsCount})");
	}

	private void CreateLayers() {
		var root = GetOrCreateRoot(ROOT_NAME);
		root.transform.position = Vector3.zero;

		_terrainLayer = new GameObject(TERRAIN_LAYER).transform;
		_terrainLayer.SetParent(root.transform);
		_terrainLayer.localPosition = Vector3.zero;

		_pathsLayer = new GameObject(PATHS_LAYER).transform;
		_pathsLayer.SetParent(root.transform);
		_pathsLayer.localPosition = Vector3.zero;

		_nodesLayer = new GameObject(NODES_LAYER).transform;
		_nodesLayer.SetParent(root.transform);
		_nodesLayer.localPosition = Vector3.zero;

		_fogLayer = new GameObject(FOG_LAYER).transform;
		_fogLayer.SetParent(root.transform);
		_fogLayer.localPosition = Vector3.zero;
		

		_indicatorsLayer = new GameObject(INDICATORS_LAYER).transform;
		_indicatorsLayer.SetParent(root.transform);
		_indicatorsLayer.localPosition = Vector3.zero;
	}

	private void CreateIndicatorSprites() {
		_eventIndicatorSprite = CreateQuestionMarkSprite(EventIndicatorColor);
		_combatIndicatorSprite = CreateExclamationSprite(CombatIndicatorColor);
		_dungeonIndicatorSprite = CreateSkullSprite(DungeonIndicatorColor);
	}

	private void CreatePlayerMarker() {
		_playerMarker = new GameObject("ExpeditionPlayer");
		var sr = _playerMarker.AddComponent<SpriteRenderer>();
		sr.sprite = CreatePlayerSprite();
		sr.sortingOrder = PLAYER_SUBLAYER;

		_playerMarker.transform.SetParent(RootObject?.transform);
		_playerMarker.SetActive(false);

		Debug.Log("ExpeditionIsoRenderer: Player marker created (hooded figure only)");
	}

	private void SubscribeToEvents() {
		_stateManager.OnPhaseChanged += OnPhaseChanged;
		_expeditionManager.OnExpeditionStarted += OnExpeditionStarted;
		_expeditionManager.OnExpeditionEnded += OnExpeditionEnded;
	}

	private void UnsubscribeFromEvents() {
		_stateManager.OnPhaseChanged -= OnPhaseChanged;
		_expeditionManager.OnExpeditionStarted -= OnExpeditionStarted;
		_expeditionManager.OnExpeditionEnded -= OnExpeditionEnded;
		_expeditionManager.OnNodeArrived -= OnNodeArrived;
	}

	#endregion

	#region Player Torch

	/// <summary>
	/// Updates the player's torch based on time of day.
	/// Automatically equips torch at night, removes during day.
	/// </summary>
	private void UpdatePlayerTorch() {
		var travel = _expeditionManager.Travel;
		if (travel == null) {
			return;
		}

		bool shouldHaveTorch = _lightController.NeedsArtificialLight && _expeditionManager.IsActive;

		if (shouldHaveTorch && !_torchActive) {
			EquipPlayerTorch();
		} else if (!shouldHaveTorch && _torchActive) {
			UnequipPlayerTorch();
		}

		// Update torch position if active
		if (_torchActive && _playerTorchHandle >= 0) {
			var visualPos = travel.VisualPosition;
			var isoPos = ToIsoPos(new Vector2Int(
				Mathf.RoundToInt(visualPos.x),
				Mathf.RoundToInt(visualPos.y)
			));
			_lightController.UpdateLightPosition(_playerTorchHandle, isoPos);
		}
	}

	/// <summary>
	/// Equips the player's torch for nighttime travel.
	/// </summary>
	private void EquipPlayerTorch() {
		if (_torchActive) {
			return;
		}

		var travel = _expeditionManager.Travel;
		if (travel == null) {
			return;
		}

		var visualPos = travel.VisualPosition;
		var isoPos = ToIsoPos(new Vector2Int(
			Mathf.RoundToInt(visualPos.x),
			Mathf.RoundToInt(visualPos.y)
		));

		_playerTorchHandle = _lightController.AddLightSource(
			IsoLightSource.Torch(isoPos, "player")
		);
		_torchActive = true;

		Debug.Log($"ExpeditionIsoRenderer: Player torch equipped (handle={_playerTorchHandle})");
	}

	/// <summary>
	/// Unequips the player's torch (daylight).
	/// </summary>
	private void UnequipPlayerTorch() {
		if (!_torchActive) {
			return;
		}

		if (_playerTorchHandle >= 0) {
			_lightController.RemoveLightSource(_playerTorchHandle);
			_playerTorchHandle = -1;
		}
		_torchActive = false;

		Debug.Log("ExpeditionIsoRenderer: Player torch unequipped");
	}

	/// <summary>
	/// Forces the torch on/off for testing purposes.
	/// </summary>
	public void SetTorchForced(bool enabled) {
		if (enabled) {
			EquipPlayerTorch();
		} else {
			UnequipPlayerTorch();
		}
	}

	#endregion

	#region Visibility Control

	/// <summary>
	/// Shows or hides the renderer without destroying objects.
	/// Use this when switching between game phases to preserve rendered state.
	/// </summary>
	public void SetVisible(bool visible) {
		if (RootObject != null) {
			RootObject.SetActive(visible);
		}

		if (_playerMarker != null) {
			_playerMarker.SetActive(visible && _expeditionManager.IsActive);
		}

		IsActive = visible;

		if (visible) {
			Debug.Log("ExpeditionIsoRenderer: Made visible");
		} else {
			ClearHover();
			Debug.Log("ExpeditionIsoRenderer: Hidden");
		}
	}

	/// <summary>
	/// Activates the renderer, showing it and rendering if expedition is active.
	/// </summary>
	public void Activate() {
		SetVisible(true);

		if (_expeditionManager.IsActive) {
			ForceFullRender();
			SetupCamera();
		}

		Debug.Log("ExpeditionIsoRenderer: Activated");
	}

	/// <summary>
	/// Deactivates the renderer, hiding it but preserving all rendered objects.
	/// </summary>
	public void Deactivate() {
		SetVisible(false);
		Debug.Log("ExpeditionIsoRenderer: Deactivated");
	}

	#endregion

	#region Terrain Graphics Resolution

	/// <summary>
	/// Gets the terrain sprites for a terrain ID, using asset graphics if available
	/// or falling back to procedural color-based sprites.
	/// </summary>
	private (Sprite top, Sprite? sideLeft, Sprite? sideRight) GetTerrainSprites(
		TerrainProto.ID terrainId,
		Vector2Int tilePos,
		ColorRPG fallbackColor
	) {
		// Check if we've already determined this terrain uses fallback
		if (_terrainUsesFallback.Contains(terrainId)) {
			return GetFallbackSprites(fallbackColor);
		}

		// Try to get asset graphics
		if (_assetManager.TryGetTerrainGraphics(terrainId, out var gfx) && gfx.IsValid) {
			// Get or create variant index for this tile position
			int variantIndex = GetTileVariantIndex(tilePos, gfx.TopVariantCount);

			var topSprite = gfx.GetTop(variantIndex);
			Sprite? leftSprite = null;
			Sprite? rightSprite = null;

			if (gfx.HasSides) {
				int sideVariantIndex = GetTileVariantIndex(tilePos, gfx.SideVariantCount);
				leftSprite = gfx.GetSideLeft(sideVariantIndex);
				rightSprite = gfx.GetSideRight(sideVariantIndex);
			}

			// If top sprite is valid, use asset graphics
			if (topSprite != null) {
				_terrainGraphicsChecked.Add(terrainId);
				return (topSprite, leftSprite, rightSprite);
			}
		}

		// Mark as using fallback and return procedural sprites
		if (!_terrainGraphicsChecked.Contains(terrainId)) {
			_terrainGraphicsChecked.Add(terrainId);
			_terrainUsesFallback.Add(terrainId);
			Debug.Log($"ExpeditionIsoRenderer: Using fallback sprites for terrain '{terrainId.Value}'");
		}

		return GetFallbackSprites(fallbackColor);
	}

	/// <summary>
	/// Gets procedural color-based fallback sprites.
	/// </summary>
	private (Sprite top, Sprite? sideLeft, Sprite? sideRight) GetFallbackSprites(ColorRPG color) {
		var topSprite = GetOrCreateTopSprite(color);
		var leftSprite = GetOrCreateLeftSideSprite(color);
		var rightSprite = GetOrCreateRightSideSprite(color);
		return (topSprite, leftSprite, rightSprite);
	}

	/// <summary>
	/// Gets a deterministic variant index for a tile position.
	/// Ensures the same tile always gets the same variant.
	/// </summary>
	private int GetTileVariantIndex(Vector2Int tilePos, int variantCount) {
		if (variantCount <= 1) {
			return 0;
		}

		if (_tileVariantCache.TryGetValue(tilePos, out int cachedIndex)) {
			return cachedIndex % variantCount;
		}

		// Generate deterministic index from position
		int hash = tilePos.x * 73856093 ^ tilePos.y * 19349663;
		int index = Mathf.Abs(hash) % variantCount;
		
		_tileVariantCache[tilePos] = index;
		return index;
	}

	#endregion

	#region Sprite Creation - Tiles

	private Sprite GetOrCreateTopSprite(ColorRPG color) {
		var rounded = RoundColorForCache(color);
		if (_topSpriteCache.TryGetValue(rounded, out var cached)) {
			return cached;
		}

		var sprite = CreateSquareTileSprite(color);
		_topSpriteCache[rounded] = sprite;
		return sprite;
	}

	private Sprite GetOrCreateLeftSideSprite(ColorRPG topColor) {
		var sideColor = topColor.Darken(LEFT_SIDE_DARKEN);
		var rounded = RoundColorForCache(sideColor);

		if (_leftSideCache.TryGetValue(rounded, out var cached)) {
			return cached;
		}

		var sprite = CreateSideSprite(sideColor);
		_leftSideCache[rounded] = sprite;
		return sprite;
	}

	private Sprite GetOrCreateRightSideSprite(ColorRPG topColor) {
		var sideColor = topColor.Darken(RIGHT_SIDE_DARKEN);
		var rounded = RoundColorForCache(sideColor);

		if (_rightSideCache.TryGetValue(rounded, out var cached)) {
			return cached;
		}

		var sprite = CreateSideSprite(sideColor);
		_rightSideCache[rounded] = sprite;
		return sprite;
	}

	private Sprite CreateSquareTileSprite(ColorRPG fillColor) {
		int size = TILE_TEXTURE_SIZE;
		var texture = new Texture2D(size, size, TextureFormat.ARGB32, false) {
			filterMode = FilterMode.Point
		};

		Color fill = fillColor;
		Color border = fillColor.Darken(0.4f);

		var pixels = new Color[size * size];
		int borderWidth = 2;

		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				bool isBorder = x < borderWidth || x >= size - borderWidth ||
								y < borderWidth || y >= size - borderWidth;
				pixels[y * size + x] = isBorder ? border : fill;
			}
		}

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(
			texture,
			new Rect(0, 0, size, size),
			new Vector2(0.5f, 0.5f),
			PIXELS_PER_UNIT
		);
	}

	private Sprite CreateSideSprite(ColorRPG fillColor) {
		int width = SIDE_TEXTURE_WIDTH;
		int height = SIDE_TEXTURE_HEIGHT;
		var texture = new Texture2D(width, height, TextureFormat.ARGB32, false) {
			filterMode = FilterMode.Point
		};

		Color baseColor = fillColor;
		Color darkColor = fillColor.Darken(0.2f);
		Color borderColor = fillColor.Darken(0.5f);

		var pixels = new Color[width * height];
		int borderWidth = 1;

		for (int y = 0; y < height; y++) {
			float t = (float)y / height;
			Color rowColor = Color.Lerp(darkColor, baseColor, t);

			for (int x = 0; x < width; x++) {
				bool isBorder = x < borderWidth || x >= width - borderWidth ||
								y < borderWidth || y >= height - borderWidth;
				pixels[y * width + x] = isBorder ? borderColor : rowColor;
			}
		}

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(
			texture,
			new Rect(0, 0, width, height),
			new Vector2(0.5f, 0f),
			PIXELS_PER_UNIT
		);
	}

	private ColorRPG RoundColorForCache(ColorRPG color) {
		return ColorRPG.FromHSL(
			Mathf.Round(color.Hue / 18f) * 18f,
			Mathf.Round(color.Saturation * 20f) / 20f,
			Mathf.Round(color.Lightness * 20f) / 20f,
			Mathf.Round(color.Alpha * 20f) / 20f
		);
	}

	#endregion

	#region Sprite Creation - Indicators

	private Sprite CreatePathArrowSprite(ColorRPG terrainColor) {
		Color unityTerrainColor = terrainColor.ToUnityColor();
		ColorRPG arrowColor = unityTerrainColor.grayscale > 0.5f
			? ColorRPG.Black
			: ColorRPG.White;

		int size = INDICATOR_SIZE;
		var texture = new Texture2D(size, size, TextureFormat.ARGB32, false) {
			filterMode = FilterMode.Bilinear
		};
		var pixels = new Color[size * size];
		for (int i = 0; i < pixels.Length; i++) {
			pixels[i] = Color.clear;
		}
		int cx = size / 2;

		// Shaft
		for (int y = 4; y <= 18; y++) {
			DrawCircle(pixels, size, cx, y, 2, arrowColor);
		}

		// Arrow head (triangle-ish)
		for (int y = 18; y <= 28; y++) {
			int halfWidth = y - 18;
			for (int x = cx - halfWidth; x <= cx + halfWidth; x++) {
				if (x < 0 || x >= size) {
					continue;
				}
				pixels[y * size + x] = arrowColor.ToUnityColor();
			}
		}

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.25f), 32f);
	}

	private Sprite CreateQuestionMarkSprite(ColorRPG color) {
		int size = INDICATOR_SIZE;
		var texture = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };

		var pixels = new Color[size * size];
		for (int i = 0; i < pixels.Length; i++) {
			pixels[i] = Color.clear;
		}

		int centerX = size / 2;

		// Top curve of ?
		for (int angle = 0; angle < 270; angle += 10) {
			float rad = angle * Mathf.Deg2Rad;
			int x = centerX + (int)(6 * Mathf.Cos(rad));
			int y = 22 + (int)(5 * Mathf.Sin(rad));
			DrawCircle(pixels, size, x, y, 2, color);
		}

		// Stem
		for (int y = 12; y <= 18; y++) {
			DrawCircle(pixels, size, centerX, y, 2, color);
		}

		// Dot
		DrawCircle(pixels, size, centerX, 6, 3, color);

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), 32f);
	}

	private Sprite CreateExclamationSprite(ColorRPG color) {
		int size = INDICATOR_SIZE;
		var texture = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };

		var pixels = new Color[size * size];
		for (int i = 0; i < pixels.Length; i++) {
			pixels[i] = Color.clear;
		}

		int centerX = size / 2;

		// Stem (tapered)
		for (int y = 12; y <= 28; y++) {
			float taper = 1f + (y - 12) * 0.1f;
			DrawCircle(pixels, size, centerX, y, (int)taper + 1, color);
		}

		// Dot
		DrawCircle(pixels, size, centerX, 6, 3, color);

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), 32f);
	}

	private Sprite CreateSkullSprite(ColorRPG color) {
		int size = INDICATOR_SIZE;
		var texture = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };

		var pixels = new Color[size * size];
		for (int i = 0; i < pixels.Length; i++) {
			pixels[i] = Color.clear;
		}

		int centerX = size / 2;

		// Head (oval)
		for (int y = 14; y <= 28; y++) {
			int width = (int)(8 * Mathf.Sin((y - 14) * Mathf.PI / 14f));
			for (int x = centerX - width; x <= centerX + width; x++) {
				if (x >= 0 && x < size) {
					pixels[y * size + x] = color;
				}
			}
		}

		// Eyes (dark holes)
		DrawCircle(pixels, size, centerX - 4, 22, 2, Color.clear);
		DrawCircle(pixels, size, centerX + 4, 22, 2, Color.clear);

		// Jaw
		for (int x = centerX - 4; x <= centerX + 4; x++) {
			for (int y = 10; y <= 14; y++) {
				pixels[y * size + x] = color;
			}
		}

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), 32f);
	}

	private Sprite CreatePlayerSprite() {
		int size = 48;
		var texture = new Texture2D(size, size, TextureFormat.ARGB32, false) { filterMode = FilterMode.Point };

		var pixels = new Color[size * size];
		for (int i = 0; i < pixels.Length; i++) {
			pixels[i] = Color.clear;
		}

		int cx = size / 2;

		Color outline = PlayerBorderColor;
		Color cloak = PlayerColor;
		Color hood = PlayerColor.Lighten(0.15f);
		Color face = ColorRPG.FromHex("#E6F7FF").WithAlpha(0.95f);

		// Cloak body (tall diamond)
		for (int y = 10; y <= 38; y++) {
			float t = Mathf.InverseLerp(10, 38, y);
			int halfW = Mathf.RoundToInt(Mathf.Lerp(3f, 12f, 1f - Mathf.Abs(t - 0.55f) * 1.6f));
			for (int x = cx - halfW; x <= cx + halfW; x++) {
				if (x < 0 || x >= size) {
					continue;
				}
				pixels[y * size + x] = cloak;
			}
		}

		// Hood (smaller diamond on top)
		for (int y = 24; y <= 42; y++) {
			float t = Mathf.InverseLerp(24, 42, y);
			int halfW = Mathf.RoundToInt(Mathf.Lerp(2f, 8f, 1f - Mathf.Abs(t - 0.65f) * 1.8f));
			for (int x = cx - halfW; x <= cx + halfW; x++) {
				if (x < 0 || x >= size) {
					continue;
				}
				pixels[y * size + x] = hood;
			}
		}

		// Face dot/highlight
		DrawCircle(pixels, size, cx, 33, 3, face);

		// Outline pass (cheap border)
		for (int y = 1; y < size - 1; y++) {
			for (int x = 1; x < size - 1; x++) {
				Color c = pixels[y * size + x];
				if (c.a <= 0.001f) {
					continue;
				}

				bool edge =
					pixels[y * size + (x - 1)].a <= 0.001f ||
					pixels[y * size + (x + 1)].a <= 0.001f ||
					pixels[(y - 1) * size + x].a <= 0.001f ||
					pixels[(y + 1) * size + x].a <= 0.001f;

				if (edge) {
					pixels[y * size + x] = outline;
				}
			}
		}

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.22f), 48f);
	}

	private void DrawCircle(Color[] pixels, int textureSize, int cx, int cy, int radius, ColorRPG color) {
		for (int y = cy - radius; y <= cy + radius; y++) {
			for (int x = cx - radius; x <= cx + radius; x++) {
				if (x >= 0 && x < textureSize && y >= 0 && y < textureSize) {
					float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
					if (dist <= radius) {
						pixels[y * textureSize + x] = color;
					}
				}
			}
		}
	}

	#endregion

	#region Main Render Entry

	public override void Render() {
		if (!IsInitialized) {
			Initialize();
		}

		var travel = _expeditionManager.Travel;
		if (travel == null) {
			Debug.LogWarning("ExpeditionIsoRenderer: No travel state");
			return;
		}

		var center = travel.CurrentPosition;
		int distanceMoved = Mathf.Max(
			Mathf.Abs(center.x - _lastRenderedCenter.x),
			Mathf.Abs(center.y - _lastRenderedCenter.y)
		);

		if (distanceMoved > DEFAULT_RENDER_RADIUS / 2 || _renderedPositions.Count == 0) {
			FullRender(travel, center);
		} else if (center != _lastRenderedCenter) {
			IncrementalRender(travel, center);
		}

		_lastRenderedCenter = center;
		UpdatePlayerPosition();
		UpdateCamera();
	}

	/// <summary>
	/// Forces a complete re-render of the visible area.
	/// </summary>
	public void ForceFullRender() {
		_lastRenderedCenter = new Vector2Int(-1000, -1000);
		_renderedPositions.Clear();
		Render();
	}

	#endregion

	#region Full Render

	private void FullRender(ExpeditionManager.TravelData travel, Vector2Int center) {
		ClearAllLayers();

		// Ensure features are populated before rendering terrain
		PopulateFeaturesIfNeeded();

		RenderTerrainLayer(center, travel);
		RenderPathsLayer(travel);
		RenderNodesLayer(travel, center);
		RenderFogLayer(center);

		// Track rendered positions
		for (int x = center.x - DEFAULT_RENDER_RADIUS; x <= center.x + DEFAULT_RENDER_RADIUS; x++) {
			for (int y = center.y - DEFAULT_RENDER_RADIUS; y <= center.y + DEFAULT_RENDER_RADIUS; y++) {
				_renderedPositions.Add(new Vector2Int(x, y));
			}
		}

		Debug.Log($"ExpeditionIsoRenderer: Full render at {center}, " +
				  $"terrain={_terrainTiles.Count}, paths={_pathTiles.Count}, " +
				  $"nodes={_nodeObjects.Count}, fog={_fogTiles.Count}, " +
				  $"features={_featureGenerator?.FeatureCount ?? 0}");
	}

	private void ClearAllLayers() {
		ClearDictionary(_terrainTiles);
		ClearDictionary(_pathTiles);
		ClearDictionary(_nodeObjects);
		ClearDictionary(_fogTiles);
		ClearDictionary(_indicatorObjects);
		_renderedPositions.Clear();
		_tileVariantCache.Clear();
	}

	private void ClearDictionary<TKey>(Dictionary<TKey, GameObject> dict) where TKey : notnull {
		foreach (var obj in dict.Values) {
			if (obj != null) {
				UnityEngine.Object.Destroy(obj);
			}
		}
		dict.Clear();
	}

	#endregion

	#region Terrain Rendering

	private void RenderTerrainLayer(Vector2Int center, ExpeditionManager.TravelData travel) {
		if (_terrainLayer == null) {
			return;
		}

		for (int x = center.x - DEFAULT_RENDER_RADIUS; x <= center.x + DEFAULT_RENDER_RADIUS; x++) {
			for (int y = center.y - DEFAULT_RENDER_RADIUS; y <= center.y + DEFAULT_RENDER_RADIUS; y++) {
				var pos2D = new Vector2Int(x, y);

				// Get terrain from path/features/procedural (priority order)
				var effectiveTerrain = GetTerrainIdAt(pos2D, travel);

				var color = GetTerrainColor(effectiveTerrain);

				// Height comes from features or IsoPathGenerator's height cache
				int height = GetHeightAt(pos2D);

				var tile = CreateTerrainTile(pos2D, effectiveTerrain, color, height);
				_terrainTiles[pos2D] = tile;
			}
		}
	}

	/// <summary>
	/// Gets the terrain ID at a position with priority:
	/// 1. Path nodes (interactive)
	/// 2. Path segments (interactive)
	/// 3. Terrain features (decorative)
	/// 4. Procedural biome-aware terrain (fallback)
	/// </summary>
	private TerrainProto.ID GetTerrainIdAt(Vector2Int pos, ExpeditionManager.TravelData travel) {
		// Priority 1: Check if on a node (highest priority - interactive)
		foreach (var node in travel.GetAllNodes()) {
			if (node.Position == pos) {
				return node.TerrainId;
			}
		}

		// Priority 2: Check if on a path (second priority - interactive)
		foreach (var node in travel.GetAllNodes()) {
			foreach (var conn in node.Connections) {
				var targetNode = travel.GetNode(conn.TargetNodeId);
				if (targetNode == null) {
					continue;
				}

				if (IsPointNearLine(pos, node.Position, targetNode.Position, PATH_THRESHOLD)) {
					return conn.TerrainId;
				}
			}
		}

		// Priority 3: Check terrain features (decorative)
		if (_featureGenerator != null) {
			var featureResult = _featureGenerator.GetTerrainAt(new GridPosRPG(pos.x, pos.y));
			if (featureResult.HasValue && featureResult.Value.IsValid) {
				return featureResult.Value.Terrain;
			}
		}

		// Priority 4: Biome-aware procedural terrain (fallback)
		return GetProceduralTerrainId(pos, travel);
	}

	/// <summary>
	/// Gets procedural terrain using biome data from nearby path nodes.
	/// Blends path terrain with surrounding terrain based on distance.
	/// </summary>
	private TerrainProto.ID GetProceduralTerrainId(Vector2Int pos, ExpeditionManager.TravelData? travel = null) {
		// Check cache first
		if (_proceduralTerrainCache.TryGetValue(pos, out var cached)) {
			return cached;
		}

		travel ??= _expeditionManager.Travel;
		var isoPath = _expeditionManager.CurrentIsoPath;

		// Find the biome for this position based on nearest path nodes
		var biomeId = GetBiomeForPosition(pos, travel, isoPath);

		// Get the biome proto
		if (!string.IsNullOrEmpty(biomeId.Value) &&
			_expeditionManager.GameDb.TryGetProto(biomeId, out BiomeProto? biome) &&
			biome != null) {
			// Use biome-aware terrain selection
			var terrain = SelectBiomeTerrainForPosition(pos, biome, travel);
			_proceduralTerrainCache[pos] = terrain;
			return terrain;
		}

		// Fallback to simple procedural if no biome found
		var fallback = GetFallbackProceduralTerrain(pos);
		_proceduralTerrainCache[pos] = fallback;
		return fallback;
	}

	/// <summary>
	/// Determines which biome a world position belongs to based on nearby path nodes.
	/// Uses distance-weighted voting from nearby nodes.
	/// </summary>
	private BiomeProto.ID GetBiomeForPosition(Vector2Int pos, ExpeditionManager.TravelData? travel, IsoExpeditionPath? isoPath) {
		// Check biome cache
		if (_biomeCache.TryGetValue(pos, out var cached)) {
			return cached;
		}

		BiomeProto.ID result = new BiomeProto.ID("");

		if (isoPath != null) {
			// Find nearest IsoPathNode(s) and use their biome
			float nearestDist = float.MaxValue;
			IsoPathNode? nearestNode = null;

			foreach (var node in isoPath.Nodes.Values) {
				float dist = Vector2.Distance(
					new Vector2(pos.x, pos.y),
					new Vector2(node.Position.X, node.Position.Y)
				);

				if (dist < nearestDist) {
					nearestDist = dist;
					nearestNode = node;
				}
			}

			if (nearestNode != null && !string.IsNullOrEmpty(nearestNode.BiomeId.Value)) {
				result = nearestNode.BiomeId;
			}
		} else if (travel != null) {
			// Fallback to legacy TravelData
			float nearestDist = float.MaxValue;
			PathNode? nearestNode = null;

			foreach (var node in travel.GetAllNodes()) {
				float dist = Vector2.Distance(pos, node.Position);
				if (dist < nearestDist) {
					nearestDist = dist;
					nearestNode = node;
				}
			}

			if (nearestNode != null) {
				// Get biome from ExpeditionManager's current state
				var currentBiomeId = _expeditionManager.GetIsoPathGenerator()?.GetCurrentState()?.BiomeState?.CurrentBiomeId;
				if (!string.IsNullOrEmpty(currentBiomeId)) {
					result = new BiomeProto.ID(currentBiomeId);
				}
			}
		}

		_biomeCache[pos] = result;
		return result;
	}

	/// <summary>
	/// Selects terrain for a position based on biome rules and distance from path.
	/// Blends path terrain with biome variants based on proximity.
	/// </summary>
	private TerrainProto.ID SelectBiomeTerrainForPosition(Vector2Int pos, BiomeProto biome, ExpeditionManager.TravelData? travel) {
		if (_terrainNoise == null || _blendNoise == null) {
			InitializeTerrainNoise();
		}

		// Calculate distance to nearest path
		float distanceToPath = GetDistanceToNearestPath(pos, travel);

		// Get noise values for variation
		float terrainNoise = (_terrainNoise?.GetNoise(pos.x, pos.y) ?? 0f) * 0.5f + 0.5f; // 0-1
		float blendNoise = (_blendNoise?.GetNoise(pos.x, pos.y) ?? 0f) * 0.5f + 0.5f; // 0-1

		// Create a seeded random for this position
		int positionSeed = pos.x * 73856093 ^ pos.y * 19349663;
		var rng = GameRandom.FromSeed((uint)positionSeed);

		if (distanceToPath < BLEND_START) {
			// Very close to path - use primary biome terrain
			return biome.PrimaryTerrain;
		}

		if (distanceToPath < BLEND_END) {
			// Blend zone - mostly primary with chance of first variant
			float blendFactor = (distanceToPath - BLEND_START) / (BLEND_END - BLEND_START);

			if (blendNoise < blendFactor * 0.3f && biome.VariantTerrains.Count > 0) {
				// Pick first (most common) variant
				return biome.VariantTerrains[0].Terrain;
			}
			return biome.PrimaryTerrain;
		}

		if (distanceToPath < VARIANT_START) {
			// Primary zone with variant chance
			float variantChance = biome.VariantTerrainChance * 0.5f; // Reduced chance

			if (terrainNoise < variantChance && biome.VariantTerrains.Count > 0) {
				return SelectWeightedVariant(biome, ref rng);
			}
			return biome.PrimaryTerrain;
		}

		// Full distribution zone - use biome's terrain selection
		return biome.SelectTerrain(ref rng);
	}

	/// <summary>
	/// Selects a terrain from the biome's variant list based on weights.
	/// </summary>
	private TerrainProto.ID SelectWeightedVariant(BiomeProto biome, ref RandomStream rng) {
		if (biome.VariantTerrains.Count == 0) {
			return biome.PrimaryTerrain;
		}

		float totalWeight = 0f;
		foreach (var variant in biome.VariantTerrains) {
			totalWeight += variant.Weight;
		}

		float roll = (float)(rng.NextFloat() * totalWeight);
		float cumulative = 0f;

		foreach (var variant in biome.VariantTerrains) {
			cumulative += variant.Weight;
			if (roll <= cumulative) {
				return variant.Terrain;
			}
		}

		return biome.VariantTerrains[^1].Terrain;
	}

	/// <summary>
	/// Calculates the distance from a position to the nearest path segment.
	/// </summary>
	private float GetDistanceToNearestPath(Vector2Int pos, ExpeditionManager.TravelData? travel) {
		if (travel == null) {
			return float.MaxValue;
		}

		float nearestDist = float.MaxValue;

		foreach (var node in travel.GetAllNodes()) {
			// Check distance to node
			float nodeDist = Vector2.Distance(pos, node.Position);
			nearestDist = Mathf.Min(nearestDist, nodeDist);

			// Check distance to connections (path segments)
			foreach (var conn in node.Connections) {
				var targetNode = travel.GetNode(conn.TargetNodeId);
				if (targetNode == null) {
					continue;
				}

				float segmentDist = DistanceToLineSegment(pos, node.Position, targetNode.Position);
				nearestDist = Mathf.Min(nearestDist, segmentDist);
			}
		}

		return nearestDist;
	}

	/// <summary>
	/// Calculates distance from a point to a line segment.
	/// </summary>
	private float DistanceToLineSegment(Vector2Int point, Vector2Int lineStart, Vector2Int lineEnd) {
		Vector2 p = point;
		Vector2 a = lineStart;
		Vector2 b = lineEnd;

		Vector2 ab = b - a;
		float abLengthSq = ab.sqrMagnitude;

		if (abLengthSq < 0.0001f) {
			return Vector2.Distance(p, a);
		}

		float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / abLengthSq);
		Vector2 projection = a + ab * t;

		return Vector2.Distance(p, projection);
	}

	/// <summary>
	/// Fallback procedural terrain when no biome is available.
	/// Uses the original noise-based approach.
	/// </summary>
	private TerrainProto.ID GetFallbackProceduralTerrain(Vector2Int pos) {
		float noise1 = Mathf.PerlinNoise(pos.x * 0.08f + 100, pos.y * 0.08f);
		float noise2 = Mathf.PerlinNoise(pos.x * 0.15f, pos.y * 0.15f + 50);
		float combined = (noise1 + noise2 * 0.5f) / 1.5f;

		int distanceFromStart = Mathf.Abs(pos.y);
		float forestBias = Mathf.Clamp01(distanceFromStart / 30f);

		if (distanceFromStart < 3) {
			return Ids.Terrains.Plains.Grass;
		}

		if (combined < 0.3f - forestBias * 0.1f) {
			return Ids.Terrains.Plains.Grass;
		} else if (combined < 0.5f) {
			return Ids.Terrains.Forests.Forest;
		} else if (combined < 0.7f + forestBias * 0.1f) {
			return Ids.Terrains.Forests.DeepForest;
		} else if (combined < 0.85f) {
			return Ids.Terrains.Plains.Hills;
		} else {
			return Ids.Terrains.Mountains.Mountain;
		}
	}

	/// <summary>
	/// Gets the terrain feature at a position from IsoPathGenerator.
	/// Returns Flat if not on a path node or using legacy generator.
	/// </summary>
	private TerrainFeature GetTerrainFeatureAt(Vector2Int pos2D) {
		var isoPath = _expeditionManager.CurrentIsoPath;
		if (isoPath != null) {
			var node = isoPath.Nodes.Values.FirstOrDefault(n =>
				n.Position.X == pos2D.x && n.Position.Y == pos2D.y);
			if (node != null) {
				return node.TerrainFeature;
			}
		}
		return TerrainFeature.Flat;
	}

	private ColorRPG GetTerrainColor(TerrainProto.ID terrainId) {
		string key = terrainId.Value;

		if (_terrainColorCache.TryGetValue(key, out var cached)) {
			return cached;
		}

		// Try to get from current travel state's cached stats
		var travel = _expeditionManager.Travel;
		if (travel != null && travel.CurrentTerrain == terrainId &&
			travel.CurrentTerrainStats.TerrainColor != default) {
			_terrainColorCache[key] = travel.CurrentTerrainStats.TerrainColor;
			return travel.CurrentTerrainStats.TerrainColor;
		}

		// Look up from proto
		if (_expeditionManager.GameDb.TryGetProto(terrainId, out TerrainProto proto)) {
			var color = ColorRPG.FromHex(proto.MapColor);
			_terrainColorCache[key] = color;
			return color;
		}

		_terrainColorCache[key] = FallbackTerrainColor;
		return FallbackTerrainColor;
	}

	private GameObject CreateTerrainTile(Vector2Int pos2D, TerrainProto.ID terrainId, ColorRPG fallbackColor, int height) {
		var isoPos = ToIsoPos(pos2D, height);
		var go = new GameObject($"Terrain_{pos2D.x}_{pos2D.y}");
		go.transform.SetParent(_terrainLayer);
		go.transform.position = IsoToWorldCenter(isoPos);

		// Get sprites - either from asset graphics or procedural fallback
		var (topSprite, sideLeftSprite, sideRightSprite) = GetTerrainSprites(terrainId, pos2D, fallbackColor);

		// Top surface
		var topGo = new GameObject("Top");
		topGo.transform.SetParent(go.transform);
		topGo.transform.localPosition = Vector3.zero;
		topGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

		var topSr = topGo.AddComponent<SpriteRenderer>();
		topSr.sprite = topSprite;
		topSr.sortingOrder = GetSortOrder(isoPos, TERRAIN_SUBLAYER);

		// Create side walls using asset sprites or fallback
		CreateTerrainSideWalls(go.transform, terrainId, fallbackColor, height, pos2D, sideLeftSprite, sideRightSprite);

		return go;
	}

	private void CreateTerrainSideWalls(
		Transform parent,
		TerrainProto.ID terrainId,
		ColorRPG fallbackColor,
		int tileHeight,
		Vector2Int pos2D,
		Sprite? assetLeftSprite,
		Sprite? assetRightSprite
	) {
		float tileSize = WorldTileSize.Value;
		float halfTile = tileSize * 0.5f;
		float levelHeight = TerrainLevelHeight.Value;

		int westNeighborHeight = GetHeightAt(new Vector2Int(pos2D.x - 1, pos2D.y));
		int southNeighborHeight = GetHeightAt(new Vector2Int(pos2D.x, pos2D.y - 1));

		int leftWallLevels = Mathf.Max(0, tileHeight - westNeighborHeight);
		int rightWallLevels = Mathf.Max(0, tileHeight - southNeighborHeight);

		var isoPos = ToIsoPos(pos2D, tileHeight);
		int baseSort = GetSortOrder(isoPos, TERRAIN_SUBLAYER);
		const int WALL_SORT_OFFSET = 1;

		if (leftWallLevels > 0) {
			float leftWallHeight = leftWallLevels * levelHeight;

			var leftWallGo = new GameObject("LeftWall");
			leftWallGo.transform.SetParent(parent);
			leftWallGo.transform.localPosition = new Vector3(-halfTile, -leftWallHeight, 0f);
			leftWallGo.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

			float scaleY = leftWallHeight / (SIDE_TEXTURE_HEIGHT / PIXELS_PER_UNIT);
			leftWallGo.transform.localScale = new Vector3(1f, scaleY, 1f);

			var leftSr = leftWallGo.AddComponent<SpriteRenderer>();
			leftSr.sprite = assetLeftSprite ?? GetOrCreateLeftSideSprite(fallbackColor);
			leftSr.sortingOrder = baseSort + WALL_SORT_OFFSET;
		}

		if (rightWallLevels > 0) {
			float rightWallHeight = rightWallLevels * levelHeight;

			var rightWallGo = new GameObject("RightWall");
			rightWallGo.transform.SetParent(parent);
			rightWallGo.transform.localPosition = new Vector3(0f, -rightWallHeight, -halfTile);
			rightWallGo.transform.localRotation = Quaternion.identity;

			float scaleY = rightWallHeight / (SIDE_TEXTURE_HEIGHT / PIXELS_PER_UNIT);
			rightWallGo.transform.localScale = new Vector3(1f, scaleY, 1f);

			var rightSr = rightWallGo.AddComponent<SpriteRenderer>();
			rightSr.sprite = assetRightSprite ?? GetOrCreateRightSideSprite(fallbackColor);
			rightSr.sortingOrder = baseSort + WALL_SORT_OFFSET;
		}
	}

	#endregion

	#region Path Rendering

	private void RenderPathsLayer(ExpeditionManager.TravelData travel) {
		// Path rendering is currently disabled - paths are shown via terrain coloring
	}

	#endregion

	#region Node Rendering

	private void RenderNodesLayer(ExpeditionManager.TravelData travel, Vector2Int center) {
		if (_nodesLayer == null) {
			return;
		}
		ClearDistantIndicators(center);

		foreach (var node in travel.GetAllNodes()) {
			float distance = Vector2Int.Distance(node.Position, center);
			if (distance > DEFAULT_RENDER_RADIUS + CULL_BUFFER) {
				continue;
			}

			var nodeObj = CreateNodeObject(node);
			_nodeObjects[node.Id] = nodeObj;
			CreateNodeIndicator(node);
		}
	}

	private GameObject CreateNodeObject(PathNode node) {
		IsoPos isoPos;
		var storedPos = node.GetIsoPosition();
		if (storedPos.IsValid) {
			isoPos = storedPos;
		} else {
			isoPos = ToIsoPos(node.Position);
		}

		var worldPos = IsoToWorldCenter(isoPos);
		worldPos.y += ZFightOffset.Value * 2;

		var color = NodeColors.GetValueOrDefault(node.Type, ColorRPG.Magenta);

		var go = new GameObject($"Node_{node.Id}");
		go.transform.SetParent(_nodesLayer);
		go.transform.position = worldPos;
		go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
		go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

		var sr = go.AddComponent<SpriteRenderer>();
		sr.sprite = GetOrCreateTopSprite(color);
		sr.sortingOrder = GetSortOrder(isoPos, NODE_SUBLAYER);

		return go;
	}

	private void CreateNodeIndicator(PathNode node) {
		Sprite? indicatorSprite = node.Type switch {
			PathNodeType.EventLocation when !string.IsNullOrEmpty(node.EventId) => _eventIndicatorSprite,
			PathNodeType.Dungeon => _dungeonIndicatorSprite,
			PathNodeType.BossLocation => _combatIndicatorSprite,
			_ => null
		};

		if (indicatorSprite == null) {
			return;
		}
		if (_indicatorObjects.ContainsKey(node.Id)) {
			return;
		}

		IsoPos isoPos;
		var storedPos = node.GetIsoPosition();
		if (storedPos.IsValid) {
			isoPos = storedPos;
		} else {
			isoPos = ToIsoPos(node.Position);
		}

		var worldPos = IsoToWorldCenter(isoPos);
		worldPos.y += 0.5f;

		var indicatorObj = new GameObject($"Indicator_{node.Id}");
		indicatorObj.transform.SetParent(_indicatorsLayer);
		indicatorObj.transform.position = worldPos;
		indicatorObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

		var sr = indicatorObj.AddComponent<SpriteRenderer>();
		sr.sprite = indicatorSprite;
		sr.sortingOrder = GetSortOrder(isoPos, INDICATOR_SUBLAYER);

		var bobber = indicatorObj.AddComponent<IndicatorBobber>();
		bobber.Initialize(worldPos.y, 0.05f, 2f);

		_indicatorObjects[node.Id] = indicatorObj;
	}

	private void ClearDistantIndicators(Vector2Int center) {
		var travel = _expeditionManager.Travel;
		if (travel == null) {
			return;
		}

		var toRemove = new List<PathNodeId>();

		foreach (var (nodeId, indicator) in _indicatorObjects) {
			if (indicator == null) {
				toRemove.Add(nodeId);
				continue;
			}

			var node = travel.GetNode(nodeId);
			if (node == null) {
				UnityEngine.Object.Destroy(indicator);
				toRemove.Add(nodeId);
				continue;
			}

			if (Vector2Int.Distance(node.Position, center) > DEFAULT_RENDER_RADIUS + CULL_BUFFER + 5) {
				UnityEngine.Object.Destroy(indicator);
				toRemove.Add(nodeId);
			}
		}

		foreach (var id in toRemove) {
			_indicatorObjects.Remove(id);
		}
	}

	#endregion

	#region Fog Rendering

	private void RenderFogLayer(Vector2Int center) {
		if (_fogLayer == null) {
			return;
		}

		int fogRenderDist = DEFAULT_RENDER_RADIUS + CULL_BUFFER;

		if (_fogTiles.Count > 0) {
			var toRemove = new List<Vector2Int>();
			foreach (var (pos, fogObj) in _fogTiles) {
				if (fogObj == null || !_terrainTiles.ContainsKey(pos)) {
					if (fogObj != null) {
						UnityEngine.Object.Destroy(fogObj);
					}
					toRemove.Add(pos);
				}
			}

			foreach (var pos in toRemove) {
				_fogTiles.Remove(pos);
			}
		}

		var fogOfWar = _expeditionManager.FogOfWar;
		int visionRange = GetVisionRange();

		for (int x = center.x - fogRenderDist; x <= center.x + fogRenderDist; x++) {
			for (int y = center.y - fogRenderDist; y <= center.y + fogRenderDist; y++) {
				var pos2D = new Vector2Int(x, y);

				if (!_terrainTiles.ContainsKey(pos2D)) {
					if (_fogTiles.TryGetValue(pos2D, out var existing)) {
						UnityEngine.Object.Destroy(existing);
						_fogTiles.Remove(pos2D);
					}
					continue;
				}

				float distance = Vector2.Distance(new Vector2(x, y), center);
				var fogColor = GetFogColorForDistance(distance, visionRange, pos2D, fogOfWar);

				if (fogColor == null) {
					if (_fogTiles.TryGetValue(pos2D, out var existing)) {
						UnityEngine.Object.Destroy(existing);
						_fogTiles.Remove(pos2D);
					}
					continue;
				}

				if (!_fogTiles.ContainsKey(pos2D)) {
					var fogTile = CreateFogTile(pos2D, fogColor.Value);
					_fogTiles[pos2D] = fogTile;
				}
			}
		}
	}

	private ColorRPG? GetFogColorForDistance(float distance, int visionRange, Vector2Int pos, FogOfWar? fogOfWar) {
		if (distance > DEFAULT_RENDER_RADIUS) {
			return FogHeavyColor;
		}

		if (distance > visionRange + 4) {
			bool wasRevealed = fogOfWar?.IsRevealed(pos) ?? false;
			return wasRevealed ? FogLightColor : FogDenseColor;
		}

		if (distance > visionRange) {
			return FogLightColor;
		}

		return null;
	}

	private int GetVisionRange() {
		int visionRange = 6;
		var runState = _expeditionManager.RunState;
		if (runState != null) {
			visionRange = runState.Stats.GetInt(Ids.Stats.Expedition.VisionRange);
			if (visionRange <= 0) {
				visionRange = 6;
			}
		}
		return visionRange;
	}

	private GameObject CreateFogTile(Vector2Int pos2D, ColorRPG color) {
		var isoPos = ToIsoPos(pos2D);
		var worldPos = IsoToWorldCenter(isoPos);

		worldPos.y += 0.1f;

		var go = new GameObject($"Fog_{pos2D.x}_{pos2D.y}");
		go.transform.SetParent(_fogLayer);
		go.transform.position = worldPos;
		go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

		var sr = go.AddComponent<SpriteRenderer>();
		sr.sprite = GetOrCreateTopSprite(color);
		sr.sortingOrder = GetSortOrder(isoPos, FOG_SUBLAYER);

		return go;
	}

	#endregion

	#region Incremental Render

	private void IncrementalRender(ExpeditionManager.TravelData travel, Vector2Int center) {
		// Ensure features are populated
		PopulateFeaturesIfNeeded();

		int renderDist = DEFAULT_RENDER_RADIUS + INCREMENTAL_RENDER_MARGIN;

		for (int x = center.x - renderDist; x <= center.x + renderDist; x++) {
			for (int y = center.y - renderDist; y <= center.y + renderDist; y++) {
				var pos = new Vector2Int(x, y);
				if (!_renderedPositions.Contains(pos)) {
					RenderTileAt(travel, pos);
					_renderedPositions.Add(pos);
				}
			}
		}

		CullDistantTiles(center);
		UpdateFogAroundPlayer(center);
	}

	private void RenderTileAt(ExpeditionManager.TravelData travel, Vector2Int pos) {
		var effectiveTerrain = GetTerrainIdAt(pos, travel);
		var terrainColor = GetTerrainColor(effectiveTerrain);

		int height = GetHeightAt(pos);

		if (!_terrainTiles.ContainsKey(pos)) {
			var terrainTile = CreateTerrainTile(pos, effectiveTerrain, terrainColor, height);
			_terrainTiles[pos] = terrainTile;
		}

		foreach (var node in travel.GetAllNodes()) {
			if (node.Position == pos && !_nodeObjects.ContainsKey(node.Id)) {
				var nodeObj = CreateNodeObject(node);
				_nodeObjects[node.Id] = nodeObj;
				CreateNodeIndicator(node);
				break;
			}
		}
	}

	private void CullDistantTiles(Vector2Int center) {
		int cullDist = DEFAULT_RENDER_RADIUS + INCREMENTAL_RENDER_MARGIN + CULL_BUFFER;
		var tilesToRemove = new List<Vector2Int>();

		foreach (var pos in _renderedPositions) {
			if (Mathf.Abs(pos.x - center.x) > cullDist || Mathf.Abs(pos.y - center.y) > cullDist) {
				tilesToRemove.Add(pos);
			}
		}

		foreach (var pos in tilesToRemove) {
			if (_terrainTiles.TryGetValue(pos, out var terrain)) {
				UnityEngine.Object.Destroy(terrain);
				_terrainTiles.Remove(pos);
			}
			if (_pathTiles.TryGetValue(pos, out var path)) {
				UnityEngine.Object.Destroy(path);
				_pathTiles.Remove(pos);
			}
			if (_fogTiles.TryGetValue(pos, out var fog)) {
				UnityEngine.Object.Destroy(fog);
				_fogTiles.Remove(pos);
			}
			_renderedPositions.Remove(pos);
			_tileVariantCache.Remove(pos);
		}

		ClearDistantIndicators(center);
	}

	private void UpdateFogAroundPlayer(Vector2Int center) {
		if (_fogLayer == null) {
			return;
		}

		var fogOfWar = _expeditionManager.FogOfWar;
		int visionRange = GetVisionRange();

		fogOfWar?.UpdateVisibility(center, visionRange);

		int updateRadius = visionRange + 6;

		for (int x = center.x - updateRadius; x <= center.x + updateRadius; x++) {
			for (int y = center.y - updateRadius; y <= center.y + updateRadius; y++) {
				var pos = new Vector2Int(x, y);

				if (!_terrainTiles.ContainsKey(pos)) {
					if (_fogTiles.TryGetValue(pos, out var existing)) {
						UnityEngine.Object.Destroy(existing);
						_fogTiles.Remove(pos);
					}
					continue;
				}

				float distance = Vector2.Distance(new Vector2(x, y), center);
				var fogColor = GetFogColorForDistance(distance, visionRange, pos, fogOfWar);

				if (fogColor == null) {
					if (_fogTiles.TryGetValue(pos, out var existing)) {
						UnityEngine.Object.Destroy(existing);
						_fogTiles.Remove(pos);
					}
				} else if (!_fogTiles.ContainsKey(pos)) {
					var fogTile = CreateFogTile(pos, fogColor.Value);
					_fogTiles[pos] = fogTile;
				}
			}
		}
	}

	#endregion

	#region Player & Camera

	private void UpdatePlayerPosition() {
		if (_playerMarker == null) {
			return;
		}

		var travel = _expeditionManager.Travel;
		if (travel == null) {
			_playerMarker.SetActive(false);
			return;
		}

		Vector2 visualPos = travel.VisualPosition;

		Vector3 targetWorldPos = ToWorldPosition(visualPos);
		targetWorldPos.y += PLAYER_MARKER_Y_OFFSET;

		float deltaTime = Time.deltaTime;
		_playerCurrentPosition = Vector3.Lerp(
			_playerCurrentPosition,
			targetWorldPos,
			deltaTime * PLAYER_LERP_SPEED
		);

		if (Vector3.Distance(_playerCurrentPosition, targetWorldPos) < 0.001f) {
			_playerCurrentPosition = targetWorldPos;
		}

		_playerMarker.transform.position = _playerCurrentPosition;
		_playerMarker.SetActive(true);

		var baseTile = new Vector2Int(
			Mathf.FloorToInt(_playerCurrentPosition.x / WorldTileSize.Value),
			Mathf.FloorToInt(_playerCurrentPosition.z / WorldTileSize.Value)
		);
		int baseHeight = GetHeightAt(baseTile);
		var baseIso = ToIsoPos(baseTile, baseHeight);

		var sr = _playerMarker.GetComponent<SpriteRenderer>();
		if (sr != null) {
			sr.sortingOrder = GetSortOrder(baseIso, PLAYER_SUBLAYER);
		}

		var glowSr = _playerMarker.transform.Find("PlayerGlow")?.GetComponent<SpriteRenderer>();
		if (glowSr != null) {
			glowSr.sortingOrder = GetSortOrder(baseIso, PLAYER_SUBLAYER - 1);
		}
	}

	private void UpdateCamera() {
		// Camera following is now handled by SetFollowTarget
	}

	/// <summary>
	/// Sets up the camera for expedition view centered on player.
	/// </summary>
	public void SetupCamera(bool preserveZoom = false) {
		var travel = _expeditionManager.Travel;
		if (travel == null) {
			return;
		}

		var playerPos = ToWorldPosition(travel.VisualPosition);

		if (preserveZoom) {
			_cameraController.PanTo(playerPos, immediate: false);
		} else {
			_cameraController.SetIsometricView(playerPos, DEFAULT_RENDER_RADIUS * 0.5f, immediate: true);
		}
	}

	#endregion

	#region Input Handling

	public override void HandleClick(Vector3 worldPosition) {
		var gridPos = WorldToGrid2D(worldPosition);
		var isoPos = ToIsoPos(gridPos);

		SelectedPosition = isoPos;
		OnTileClicked?.Invoke(gridPos);

		Debug.Log($"ExpeditionIsoRenderer: Click at {gridPos}");

		var travel = _expeditionManager.Travel;
		if (travel == null) {
			return;
		}

		foreach (var node in travel.GetAllNodes()) {
			if (node.Position == gridPos) {
				OnNodeClicked?.Invoke(node);
				break;
			}
		}
	}

	public override void HandleHover(Vector3 worldPosition) {
		var gridPos = WorldToGrid2D(worldPosition);
		var isoPos = ToIsoPos(gridPos);

		if (HoveredPosition == isoPos) {
			return;
		}

		HoveredPosition = isoPos;
		OnTileHovered?.Invoke(isoPos);
	}

	public override void ClearHover() {
		if (HoveredPosition.HasValue) {
			HoveredPosition = null;
			OnTileHoverExit?.Invoke();
		}
	}

	#endregion

	#region Helpers

	private bool IsPointNearLine(Vector2Int point, Vector2Int lineStart, Vector2Int lineEnd, float threshold) {
		Vector2 p = point;
		Vector2 a = lineStart;
		Vector2 b = lineEnd;

		Vector2 lineDir = b - a;
		float lineLength = lineDir.magnitude;

		if (lineLength < 0.001f) {
			return Vector2.Distance(p, a) <= threshold;
		}

		lineDir /= lineLength;
		Vector2 toPoint = p - a;
		float projection = Vector2.Dot(toPoint, lineDir);

		if (projection < 0 || projection > lineLength) {
			return Vector2.Distance(p, a) <= threshold ||
				Vector2.Distance(p, b) <= threshold;
		}

		Vector2 closestPoint = a + lineDir * projection;
		return Vector2.Distance(p, closestPoint) <= threshold;
	}

	private int GetSortOrder(IsoPos pos, int subLayer) {
		int levelBucket = pos.Level.Value * SORT_LEVEL_WEIGHT;
		int depth = -(pos.X + pos.Y);
		int tieBreak = -pos.X;

		return levelBucket + (depth * SORT_DEPTH_WEIGHT) + tieBreak + subLayer;
	}

	#endregion

	#region Event Handlers

	private void OnPhaseChanged(GamePhase oldPhase, GamePhase newPhase) {
		bool shouldRender = newPhase == GamePhase.Expedition ||
			newPhase == GamePhase.Event ||
			newPhase == GamePhase.Camp ||
			newPhase == GamePhase.Combat;

		if (shouldRender && _expeditionManager.IsActive) {
			_playerMarker?.SetActive(newPhase != GamePhase.Combat);

			if (newPhase != GamePhase.Combat) {
				bool preserveZoom = oldPhase == GamePhase.Event ||
					oldPhase == GamePhase.Camp;

				if (!preserveZoom) {
					ForceFullRender();
				}
				SetupCamera(preserveZoom);
			}
		} else {
			_playerMarker?.SetActive(false);
		}
	}

	private void OnExpeditionStarted() {
		Debug.Log("ExpeditionIsoRenderer: Expedition started");

		_expeditionManager.OnNodeArrived += OnNodeArrived;
		_terrainColorCache.Clear();
		_heightCache.Clear();
		_biomeCache.Clear();
		_proceduralTerrainCache.Clear();
		_tileVariantCache.Clear();
		_terrainGraphicsChecked.Clear();
		_terrainUsesFallback.Clear();

		InitializeHeightNoise();
		InitializeTerrainNoise();
		InitializeFeatureGenerator();

		var travel = _expeditionManager.Travel;
		if (travel?.CurrentNode != null) {
			Vector2 startPos = travel.CurrentNode.Position;
			_playerCurrentPosition = ToWorldPosition(startPos);
			_playerCurrentPosition.y += PLAYER_MARKER_Y_OFFSET;

			if (_playerMarker != null) {
				_playerMarker.transform.position = _playerCurrentPosition;
			}
		}

		ForceFullRender();
		SetupCamera();

		if (_playerMarker != null) {
			_cameraController.SetFollowTarget(_playerMarker.transform, Vector3.zero);
		}

		if (travel != null && !travel.IsTraveling) {
			_expeditionManager.ContinueTravel();
		}
	}

	private void OnExpeditionEnded(ExpeditionEndReason reason) {
		Debug.Log($"ExpeditionIsoRenderer: Expedition ended - {reason}");

		_expeditionManager.OnNodeArrived -= OnNodeArrived;
		_cameraController.ClearFollowTarget();

		UnequipPlayerTorch();
		ClearFeatureGenerator();

		_playerMarker?.SetActive(false);
		ClearAllLayers();
		_terrainColorCache.Clear();
		_biomeCache.Clear();
		_proceduralTerrainCache.Clear();
		_tileVariantCache.Clear();
	}

	private void OnNodeArrived(PathNode node) {
		Debug.Log($"ExpeditionIsoRenderer: Arrived at Node {node.Id}");
	}

	private void OnRenderUpdate() {
		if (!IsInitialized || !IsActive) {
			return;
		}

		var phase = _stateManager.CurrentPhase;

		bool shouldUpdate = phase == GamePhase.Expedition ||
			phase == GamePhase.Event ||
			phase == GamePhase.Camp ||
			phase == GamePhase.Combat;

		if (!shouldUpdate) {
			return;
		}

		var travel = _expeditionManager.Travel;
		if (travel == null) {
			return;
		}

		if (phase != GamePhase.Combat) {
			UpdatePlayerPosition();
			UpdatePlayerTorch();
		}

		_lightController.Update(Time.deltaTime);
		_cameraController.Update(Time.deltaTime);

		if (phase == GamePhase.Combat) {
			return;
		}

		_timeSinceLastFullRender += Time.deltaTime;

		int currentNodeCount = travel.NodeCount;
		bool nodesWereAdded = currentNodeCount > _lastKnownNodeCount;

		if (nodesWereAdded) {
			_lastKnownNodeCount = currentNodeCount;
			Debug.Log($"ExpeditionIsoRenderer: New nodes detected ({currentNodeCount}), re-rendering paths");

			ClearPathTiles();
			RenderPathsLayer(travel);

			Vector2 visualPos = travel.VisualPosition;
			Vector2Int currentTile = new(Mathf.RoundToInt(visualPos.x), Mathf.RoundToInt(visualPos.y));
			UpdateNodeObjects(travel, currentTile);
		}

		if (_timeSinceLastFullRender >= PERIODIC_RENDER_INTERVAL || nodesWereAdded) {
			_timeSinceLastFullRender = 0f;

			Vector2 visualPos = travel.VisualPosition;
			Vector2Int currentTile = new(Mathf.RoundToInt(visualPos.x), Mathf.RoundToInt(visualPos.y));

			IncrementalRender(travel, currentTile);
			UpdateFogAroundPlayer(currentTile);
			_lastRenderedCenter = currentTile;
			_lastFogCenter = currentTile;
			return;
		}

		Vector2 visualPos2 = travel.VisualPosition;
		Vector2Int currentTile2 = new(Mathf.RoundToInt(visualPos2.x), Mathf.RoundToInt(visualPos2.y));

		if (currentTile2 != _lastFogCenter) {
			_lastFogCenter = currentTile2;
			UpdateFogAroundPlayer(currentTile2);
		}

		float distanceFromLastRender = Vector2.Distance(visualPos2, _lastRenderedCenter);
		if (distanceFromLastRender >= INCREMENTAL_RENDER_DISTANCE) {
			IncrementalRender(travel, currentTile2);
			_lastRenderedCenter = currentTile2;
		}
	}

	private void UpdateNodeObjects(ExpeditionManager.TravelData travel, Vector2Int center) {
		if (_nodesLayer == null) {
			return;
		}

		foreach (var node in travel.GetAllNodes()) {
			if (_nodeObjects.ContainsKey(node.Id)) {
				continue;
			}

			float distance = Vector2Int.Distance(node.Position, center);
			if (distance > DEFAULT_RENDER_RADIUS + CULL_BUFFER) {
				continue;
			}

			var nodeObj = CreateNodeObject(node);
			_nodeObjects[node.Id] = nodeObj;
			CreateNodeIndicator(node);
		}
	}

	public override void Update(float deltaTime) {
		// Main update is handled by OnRenderUpdate via GameLoop
	}

	private void ClearPathTiles() {
		foreach (var pathObj in _pathTiles.Values) {
			if (pathObj != null) {
				UnityEngine.Object.Destroy(pathObj);
			}
		}
		_pathTiles.Clear();
	}

	#endregion

	#region Cleanup

	public override void Cleanup() {
		_gameLoop.OnRenderUpdate -= OnRenderUpdate;
		UnsubscribeFromEvents();

		ClearAllLayers();
		ClearFeatureGenerator();
		_heightCache.Clear();
		_biomeCache.Clear();
		_proceduralTerrainCache.Clear();
		_tileVariantCache.Clear();
		_terrainGraphicsChecked.Clear();
		_terrainUsesFallback.Clear();

		if (_playerMarker != null) {
			UnityEngine.Object.Destroy(_playerMarker);
			_playerMarker = null;
		}

		CleanupSpriteCache(_topSpriteCache);
		CleanupSpriteCache(_leftSideCache);
		CleanupSpriteCache(_rightSideCache);

		_terrainColorCache.Clear();

		DestroyRoot();

		_terrainLayer = null;
		_pathsLayer = null;
		_nodesLayer = null;
		_fogLayer = null;
		_indicatorsLayer = null;

		IsInitialized = false;
		IsActive = false;

		Debug.Log("ExpeditionIsoRenderer: Cleaned up");
	}

	private void CleanupSpriteCache(Dictionary<ColorRPG, Sprite> cache) {
		foreach (var sprite in cache.Values) {
			if (sprite != null && sprite.texture != null) {
				UnityEngine.Object.Destroy(sprite.texture);
			}
		}
		cache.Clear();
	}

	#endregion

	#region Debug

	public override void DrawDebugGizmos() { }

	public override string GetDebugInfo() {
		var travel = _expeditionManager.Travel;
		int currentHeight = travel != null ? GetHeightAt(travel.CurrentPosition) : 0;
		int assetTerrains = _terrainGraphicsChecked.Count - _terrainUsesFallback.Count;

		string featureInfo = _featureGenerator != null
			? $", Features={_featureGenerator.FeatureCount}, FeatureTiles={_featureGenerator.CachedTileCount}"
			: ", Features=none";

		return $"ExpeditionIsoRenderer: " +
			$"Terrain={_terrainTiles.Count}, Paths={_pathTiles.Count}, " +
			$"Nodes={_nodeObjects.Count}, Fog={_fogTiles.Count}, " +
			$"HeightCache={_heightCache.Count}, BiomeCache={_biomeCache.Count}, " +
			$"AssetGfx={assetTerrains}, FallbackGfx={_terrainUsesFallback.Count}{featureInfo}, " +
			$"Center={_lastRenderedCenter}, Height={currentHeight}\n" +
			$"{_cameraController.GetDebugInfo()}";
	}

	#endregion
}