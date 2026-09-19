using System.Text;
using RPGGame.Core.Events;
using RPGGame.Core.Isometric;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Prototypes.Locations;
using UnityEngine;

namespace RPGGame.Core.Expedition;

/// <summary>
/// Generates procedural path networks for expeditions using the isometric coordinate system.
/// Uses <see cref="IsoPos"/> for all positions and <see cref="TilesRPG"/> for all distances.
/// 
/// <para>
/// Key Design Principles:
/// - Full IsoPos/TilesRPG type safety throughout
/// - Infinite generation in ALL directions (positive and negative coordinates)
/// - Real height variation using noise and terrain-aware detection
/// - Lazy/chunked generation with visible horizon
/// - Teaser branches at intersections, extended when chosen
/// - Tree-like divergence: branches commit to direction, never reconnect
/// </para>
/// </summary>
public class IsoPathGenerator {
    private readonly GameDb _gameDb;
    private readonly EventManager _eventManager;
    private readonly FastNoiseLite _heightNoise;
    private readonly FastNoiseLite _variationNoise;

    #region Configuration

    /// <summary>
    /// Extensive configuration for path generation.
    /// All distances use TilesRPG, all angles use AngleRPG.
    /// </summary>
    public class Config {
        // ═══════════════════════════════════════════════════════════════
        // CHUNKED GENERATION SETTINGS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>How many nodes ahead to keep generated (horizon).</summary>
        public int VisibleHorizon { get; init; } = 8;

        /// <summary>Nodes to generate when extending a path chunk.</summary>
        public int ChunkSize { get; init; } = 8;

        /// <summary>Minimum nodes before we trigger extension.</summary>
        public int ExtensionThreshold { get; init; } = 4;

        /// <summary>Length of teaser branches at intersections (before choice).</summary>
        public int TeaserBranchLength { get; init; } = 6;

        /// <summary>Nodes to generate after choosing a teaser branch.</summary>
        public int ChosenBranchExtension { get; init; } = 6;

        // ═══════════════════════════════════════════════════════════════
        // SPACING SETTINGS (using TilesRPG)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Distance between nodes in tiles.</summary>
        public TilesRPG NodeSpacing { get; init; } = 5.Tiles();

        /// <summary>Minimum forward progress per node (magnitude).</summary>
        public TilesRPG MinForwardProgress { get; init; } = 4.Tiles();

        /// <summary>Tutorial node spacing (tighter for quick intro).</summary>
        public TilesRPG TutorialNodeSpacing { get; init; } = 2.Tiles();

        /// <summary>Number of tutorial nodes before switching to normal spacing.</summary>
        public int TutorialNodeCount { get; init; } = 5;

        /// <summary>Branch node spacing multiplier.</summary>
        public float BranchSpacingMultiplier { get; init; } = 0.85f;

        /// <summary>Starting position for path generation.</summary>
        public IsoPos StartPosition { get; init; } = IsoPos.Origin;

        // ═══════════════════════════════════════════════════════════════
        // DIRECTION SETTINGS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>If true, picks a random starting direction.</summary>
        public bool RandomizeStartingDirection { get; init; } = true;

        /// <summary>Initial direction (used when not randomizing).</summary>
        public AngleRPG InitialDirection { get; init; } = AngleRPG.East;

        /// <summary>Direction variance per node (degrees). Controls meandering.</summary>
        public AngleRPG DirectionVariance { get; init; } = 35.Degrees();

        /// <summary>Forward bias (0-1). Higher values keep paths straighter.</summary>
        public float ForwardBias { get; init; } = 0.7f;

        /// <summary>Maximum angle deviation from current heading.</summary>
        public AngleRPG MaxAngleDeviation { get; init; } = 45.Degrees();

        /// <summary>Maximum cumulative drift from starting direction.</summary>
        public AngleRPG MaxDriftFromStart { get; init; } = 90.Degrees();

		// ═══════════════════════════════════════════════════════════════
		// BRANCHING SETTINGS
		// ═══════════════════════════════════════════════════════════════

		/// <summary>Base chance of intersection at each eligible node.</summary>
		public Percent IntersectionChance { get; init; } = 10.Percent();

		/// <summary>Minimum nodes between branch points.</summary>
		public int MinNodesBetweenBranches { get; init; } = 16;

		/// <summary>Maximum branch depth (nested branches).</summary>
		public int MaxBranchDepth { get; init; } = int.MaxValue;

		/// <summary>
		/// Minimum angular separation between the two branch paths.
		/// Ensures branches create distinct "Y" shapes.
		/// </summary>
		public AngleRPG MinBranchSeparation { get; init; } = 45.Degrees();

		/// <summary>
		/// Maximum angle each branch can deviate from the forward direction.
		/// Keeps both branches generally moving forward, not sideways.
		/// </summary>
		public AngleRPG MaxBranchDeviationFromForward { get; init; } = 65.Degrees();

        // ═══════════════════════════════════════════════════════════════
        // HEIGHT/TERRAIN SETTINGS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Base noise frequency for height generation.</summary>
        public float HeightNoiseFrequency { get; init; } = 0.005f;

        /// <summary>Secondary noise frequency for variation.</summary>
        public float HeightVariationFrequency { get; init; } = 0.015f;

        /// <summary>Base amplitude for height noise (in levels).</summary>
        public float HeightNoiseAmplitude { get; init; } = 0.5f;

		/// <summary>
		/// Threshold for elevated features (hills/mountains).
		/// Noise values above this become elevated terrain.
		/// Higher = fewer hills, more flat areas. (0.0-1.0)
		/// </summary>
		public Percent FeatureThreshold { get; init; } = 75.Percent();

		/// <summary>
		/// Threshold for depressed features (valleys/ravines).
		/// Noise values below negative of this become valleys.
		/// Higher = fewer valleys, more flat areas. (0.0-1.0)
		/// </summary>
		public Percent ValleyThreshold { get; init; } = 75.Percent();

        /// <summary>Height delta threshold to mark as hill.</summary>
        public TilesRPG HillThreshold { get; init; } = 1.Tiles();

        /// <summary>Height delta threshold to mark as cliff.</summary>
        public TilesRPG CliffThreshold { get; init; } = 2.Tiles();

        /// <summary>Biome height bias multiplier.</summary>
        public float BiomeHeightBias { get; init; } = 1.5f;

        /// <summary>Whether to enable height variation.</summary>
        public bool EnableHeightVariation { get; init; } = true;

        /// <summary>Minimum height level (can be negative for valleys).</summary>
        public int MinHeightLevel { get; init; } = -8;

        /// <summary>Maximum height level.</summary>
        public int MaxHeightLevel { get; init; } = 8;

		// ═══════════════════════════════════════════════════════════════
		// VALLEY-SEEKING PATH SETTINGS 
		// ═══════════════════════════════════════════════════════════════

		/// <summary>How strongly paths prefer lower terrain (0 = ignore, 1 = strongly prefer valleys).</summary>
		public float ValleySeekingStrength { get; init; } = 0.7f;

		/// <summary>Number of candidate positions to sample when choosing next node.</summary>
		public int PathCandidateSamples { get; init; } = 5;

		/// <summary>Maximum height the path will climb per node (avoids cliff faces).</summary>
		public int MaxClimbPerNode { get; init; } = 1;

		/// <summary>Bonus weight for positions that descend (going downhill).</summary>
		public float DescentBonus { get; init; } = 1.5f;

        // ═══════════════════════════════════════════════════════════════
        // CONTENT SETTINGS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Base spacing between campsites.</summary>
        public int CampsiteSpacing { get; init; } = 30;

        /// <summary>Base chance of resource node.</summary>
        public Percent ResourceNodeChance { get; init; } = 35.Percent();

        /// <summary>Base chance of event.</summary>
        public Percent EventChance { get; init; } = 75.Percent();

        /// <summary>Base chance of landmark.</summary>
        public Percent LandmarkChance { get; init; } = 25.Percent();

        /// <summary>Base chance of dungeon at branch end.</summary>
        public Percent DungeonChance { get; init; } = 30.Percent();

        // ═══════════════════════════════════════════════════════════════
        // TUTORIAL SETTINGS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Node index where tutorial dungeon is placed.</summary>
        public int TutorialDungeonNodeIndex { get; init; } = -1;

        /// <summary>Guaranteed events to place at start.</summary>
        public List<EventProto.ID> GuaranteedStartEvents { get; init; } = [];

        /// <summary>Node indices for guaranteed events.</summary>
        public List<int> GuaranteedEventNodeIndices { get; init; } = [];

        /// <summary>Whether this is a tutorial run.</summary>
        public bool IsTutorialRun { get; init; } = false;

        /// <summary>Node index where first branch appears.</summary>
        public int FirstBranchNodeIndex { get; init; } = -1;

        /// <summary>Specific dungeon for tutorial.</summary>
        public DungeonProto.ID? GuaranteedFirstDungeon { get; init; } = null;

        // ═══════════════════════════════════════════════════════════════
        // COMPUTED PROPERTIES
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Branch node spacing.</summary>
        internal TilesRPG BranchNodeSpacing => TilesRPG.FromTiles(NodeSpacing.Value * BranchSpacingMultiplier);

        /// <summary>Intersection chance as ChanceRPG.</summary>
        internal ChanceRPG IntersectionChanceRPG => IntersectionChance.ToChance();

        /// <summary>Event chance as ChanceRPG.</summary>
        internal ChanceRPG EventChanceRPG => EventChance.ToChance();

        /// <summary>Landmark chance as ChanceRPG.</summary>
        internal ChanceRPG LandmarkChanceRPG => LandmarkChance.ToChance();

        /// <summary>Resource node chance as ChanceRPG.</summary>
        internal ChanceRPG ResourceNodeChanceRPG => ResourceNodeChance.ToChance();

        /// <summary>Dungeon chance as ChanceRPG.</summary>
        internal ChanceRPG DungeonChanceRPG => DungeonChance.ToChance();

        /// <summary>Gets appropriate node spacing based on index.</summary>
        internal TilesRPG GetNodeSpacing(int nodeIndex) {
            if (IsTutorialRun && nodeIndex < TutorialNodeCount) {
                return TutorialNodeSpacing;
            }
            return NodeSpacing;
        }

        /// <summary>Gets minimum forward progress based on index.</summary>
        internal TilesRPG GetMinForwardProgress(int nodeIndex) {
            if (IsTutorialRun && nodeIndex < TutorialNodeCount) {
                return TutorialNodeSpacing;
            }
            return MinForwardProgress;
        }
    }

    #endregion

    #region Generation State

	public class GenerationState {
		public int BaseSeed { get; set; }
		public int ChunkIndex { get; set; }
		public BiomeStateData BiomeState { get; set; } = new();
		public int LastBranchNodeIndex { get; set; } = -20;
		public int TotalNodesGenerated { get; set; }
		public int GuaranteedEventIndex { get; set; }
		public bool TutorialBranchesGenerated { get; set; }
		public bool TutorialComplete { get; set; }
        
		/// <summary>Active branches keyed by branch index.</summary>
		public Dictionary<int, BranchState> ActiveBranches { get; set; } = [];
        
		/// <summary>Next branch index to assign.</summary>
		public int NextBranchIndex { get; set; }
        
		/// <summary>Current active branch (-1 = main path).</summary>
		public int ActiveBranchIndex { get; set; } = -1;
        
		/// <summary>Frontier node IDs for extension.</summary>
		public List<PathNodeId> FrontierNodeIds { get; set; } = [];
        
		public float StartingDirectionRadians { get; set; }
		public PathNodeIdFactory NodeIdFactory { get; set; } = new();
		public Dictionary<Vector2Int, int> HeightCache { get; set; } = [];

		public AngleRPG StartingDirection => AngleRPG.FromRadians(StartingDirectionRadians);
		public RandomStream GetChunkRandom() => GameRandom.For("PathGen.Chunk", BaseSeed, ChunkIndex);
		public RandomStream GetBranchRandom(int branchIndex) => GameRandom.For("PathGen.Branch", BaseSeed, branchIndex);
		public int GetNextBranchIndex() => NextBranchIndex++;
	}

    /// <summary>Serializable biome state.</summary>
    public class BiomeStateData {
        public string CurrentBiomeId { get; set; } = "";
        public int NodesRemainingInBiome { get; set; }
        public int CurrentBiomeIndex { get; set; }
        public float CurrentDangerLevel { get; set; } = 0.2f;
    }

	/// <summary>State for a single branch path.</summary>
	public class BranchState {
		/// <summary>Unique identifier for this branch.</summary>
		public int BranchIndex { get; set; }
        
		/// <summary>The intersection node where this branch starts.</summary>
		public PathNodeId StartNodeId { get; set; } = PathNodeId.Invalid;
        
		/// <summary>The last node in this branch.</summary>
		public PathNodeId EndNodeId { get; set; } = PathNodeId.Invalid;
        
		public int Depth { get; set; }
		public bool IsTeaser { get; set; } = true;
		public bool IsChosen { get; set; }
		public float BaseAngleRadians { get; set; }
		public string BiomeId { get; set; } = "";
		public int NodesGenerated { get; set; }

		internal AngleRPG BaseAngle => AngleRPG.FromRadians(BaseAngleRadians);
	}

    #endregion

    #region Cached Data

    private List<LandmarkProto> _landmarkProtos = [];
    private List<DungeonProto> _dungeonProtos = [];
    private List<CampProto> _campProtos = [];
    private List<SettlementProto> _settlementProtos = [];
    private List<BossArenaProto> _bossArenaProtos = [];
    private List<BiomeProto> _mainPathBiomes = [];
    private List<BiomeProto> _branchBiomes = [];
    private Dictionary<TerrainProto.ID, TerrainProto> _terrainLookup = [];

    private Config _config = null!;
    private GenerationState _state = null!;

    #endregion

    #region Constructor

    public IsoPathGenerator(GameDb gameDb, EventManager eventManager) {
        _gameDb = gameDb;
        _eventManager = eventManager;

        // Initialize noise generators for height
        _heightNoise = new FastNoiseLite();
        _heightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        _heightNoise.SetFrequency(0.05f);

        _variationNoise = new FastNoiseLite();
        _variationNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        _variationNoise.SetFrequency(0.15f);

        CachePrototypes();
    }

    #endregion

    #region Public API - Initial Generation

    /// <summary>
    /// Gets terrain color from a terrain proto.
    /// </summary>
    public ColorRPG GetTerrainColor(TerrainProto.ID terrainId) {
        var proto = _gameDb.Get<TerrainProto>(terrainId);
        return proto != null ? ColorRPG.FromHex(proto.MapColor) : ColorRPG.FromHex("#4a7c4e");
    }

        /// <summary>
    /// Generates the initial path chunk for a new expedition.
    /// Supports infinite generation in all directions.
    /// The path starts in a valley (village at dead-end) and follows low terrain.
    /// </summary>
    public IsoExpeditionPath GenerateInitial(int seed, Config? config = null) {
        config ??= new Config();
        _config = config;

        // Configure noise with seed
        _heightNoise.SetSeed(seed);
        _heightNoise.SetFrequency(config.HeightNoiseFrequency);
        _variationNoise.SetSeed(seed + 1000);
        _variationNoise.SetFrequency(config.HeightVariationFrequency);

        // Initialize state
        _state = new GenerationState {
            BaseSeed = seed,
            ChunkIndex = 0,
            LastBranchNodeIndex = -config.MinNodesBetweenBranches
        };

        // Pick starting direction
		var directionRng = GameRandom.For("PathGen.Direction", seed);
        AngleRPG startingDirection;

        if (config.RandomizeStartingDirection) {
            float randomAngle = (float)(directionRng.NextDouble() * 360.0);
            startingDirection = AngleRPG.FromDegrees(randomAngle);
            Debug.Log($"IsoPathGenerator: Random direction = {startingDirection.Degrees:F0}°");
        } else {
            startingDirection = config.InitialDirection;
            Debug.Log($"IsoPathGenerator: Fixed direction = {startingDirection.Degrees:F0}°");
        }

        _state.StartingDirectionRadians = startingDirection.Radians;

        InitializeBiomeState();

        var path = new IsoExpeditionPath(seed);
        path.GenerationState = _state;

        // === FIND a flat, low area for the village - don't create tiles ===
        IsoPos startPos = FindFlatLowArea(config.StartPosition, directionRng);
        Debug.Log($"IsoPathGenerator: Village placed at {startPos} (height={startPos.Level.Value})");

        // Generate initial chunk
        int initialNodes = config.IsTutorialRun
            ? config.FirstBranchNodeIndex + 1
            : config.VisibleHorizon;

        GenerateMainPathChunk(path, 0, initialNodes, startPos);

        // Tutorial intersection
        if (config.IsTutorialRun && config.FirstBranchNodeIndex > 0) {
            GenerateTutorialIntersection(path);
        }

        // Set initial position
        if (path.StartNode != null) {
            path.SetCurrentNode(path.StartNode);
        }

        // Analyze terrain for hills/mountains
        AnalyzeTerrainFeatures(path);

        Debug.Log($"IsoPathGenerator: Generated {path.NodeCount} nodes, frontier: {_state.FrontierNodeIds.Count}");

        return path;
    }

    /// <summary>
    /// Finds a flat, low area for the starting village.
    /// Samples terrain around the target to find the best spot - no tile manipulation.
    /// Prefers: low elevation + flat (small height variance with neighbors).
    /// </summary>
	private IsoPos FindFlatLowArea(IsoPos target, RandomStream rng) {
        const int SEARCH_RADIUS = 12;
        const int FLATNESS_SAMPLE_RADIUS = 2;

        IsoPos bestPos = target;
        float bestScore = float.MaxValue;

        for (int dx = -SEARCH_RADIUS; dx <= SEARCH_RADIUS; dx++) {
            for (int dy = -SEARCH_RADIUS; dy <= SEARCH_RADIUS; dy++) {
                var candidatePos = new IsoPos(target.X + dx, target.Y + dy, IsoLevel.Ground);
                int centerHeight = CalculateHeightUncached(candidatePos);

                // Sample neighbors to check flatness
                int minNeighbor = centerHeight;
                int maxNeighbor = centerHeight;

                for (int nx = -FLATNESS_SAMPLE_RADIUS; nx <= FLATNESS_SAMPLE_RADIUS; nx++) {
                    for (int ny = -FLATNESS_SAMPLE_RADIUS; ny <= FLATNESS_SAMPLE_RADIUS; ny++) {
                        if (nx == 0 && ny == 0) continue;
                        var neighborPos = new IsoPos(candidatePos.X + nx, candidatePos.Y + ny, IsoLevel.Ground);
                        int neighborHeight = CalculateHeightUncached(neighborPos);
                        minNeighbor = Mathf.Min(minNeighbor, neighborHeight);
                        maxNeighbor = Mathf.Max(maxNeighbor, neighborHeight);
                    }
                }

                int heightVariance = maxNeighbor - minNeighbor;

                // Score: prefer low + flat
                // Height matters most, but flatness is important too
                float heightScore = centerHeight * 2f;          // Lower is better
                float flatnessScore = heightVariance * 3f;      // Flatter is better
                float distanceScore = Mathf.Sqrt(dx * dx + dy * dy) * 0.1f;  // Prefer closer to target

                float totalScore = heightScore + flatnessScore + distanceScore;

                if (totalScore < bestScore) {
                    bestScore = totalScore;
                    bestPos = new IsoPos(candidatePos.X, candidatePos.Y, new IsoLevel(centerHeight));
                }
            }
        }

        return bestPos;
    }

	/// <summary>
	/// Calculates height without caching - used during initial area search.
	/// </summary>
	private int CalculateHeightUncached(IsoPos pos) {
		if (!_config.EnableHeightVariation) {
			return 0;
		}

		return CalculateHeightCore(pos);
	}

	/// <summary>
	/// Core height calculation logic shared by cached and uncached methods.
	/// Uses feature-based approach: mostly flat with distinct features.
	/// </summary>
	private int CalculateHeightCore(IsoPos pos) {
		// Feature placement noise
		float featureNoise = _heightNoise.GetNoise(pos.X, pos.Y);

		// Get thresholds from config (Percent.Fraction gives 0.0-1.0)
		float featureThreshold = _config.FeatureThreshold.Fraction;
		float valleyThreshold = -_config.ValleyThreshold.Fraction;

		if (featureNoise > featureThreshold) {
			// Elevated feature (hill/mountain)
			float featureStrength = (featureNoise - featureThreshold) / (1f - featureThreshold);
			featureStrength = Mathf.Pow(featureStrength, 0.7f);

			// Local variation within the feature
			float localVariation = _variationNoise.GetNoise(pos.X, pos.Y) * 0.3f;
			featureStrength = Mathf.Clamp01(featureStrength + localVariation);

			// Apply amplitude scaling
			float scaledMaxHeight = _config.MaxHeightLevel * _config.HeightNoiseAmplitude;
			return Mathf.RoundToInt(featureStrength * scaledMaxHeight);

		}
		if (featureNoise < valleyThreshold) {
			// Depressed feature (valley/ravine)
			float valleyStrength = (valleyThreshold - featureNoise) / (1f - Mathf.Abs(valleyThreshold));
			valleyStrength = Mathf.Pow(valleyStrength, 0.8f);

			float localVariation = _variationNoise.GetNoise(pos.X, pos.Y) * 0.2f;
			valleyStrength = Mathf.Clamp01(valleyStrength + localVariation);

			// Apply amplitude scaling
			float scaledMinHeight = Mathf.Abs(_config.MinHeightLevel) * _config.HeightNoiseAmplitude;
			return -Mathf.RoundToInt(valleyStrength * scaledMinHeight);
		}

		// Flat ground - the default for most terrain
		return 0;
	}

    /// <summary>
    /// Finds or creates a valley for the starting village.
    /// Searches around the target position for the lowest point, 
    /// or forces the area to be a valley if none exists.
    /// </summary>
	private IsoPos FindOrCreateValleyStart(IsoPos target, RandomStream rng) {
        const int SEARCH_RADIUS = 8;
        const int VALLEY_FORCE_RADIUS = 3;

        IsoPos bestPos = target;
        int lowestHeight = int.MaxValue;

        // Search for existing low point
        for (int dx = -SEARCH_RADIUS; dx <= SEARCH_RADIUS; dx++) {
            for (int dy = -SEARCH_RADIUS; dy <= SEARCH_RADIUS; dy++) {
                var candidatePos = new IsoPos(target.X + dx, target.Y + dy, IsoLevel.Ground);
                int height = CalculateHeightUncached(candidatePos);

                // Prefer positions closer to target when heights are similar
                float distancePenalty = Mathf.Sqrt(dx * dx + dy * dy) * 0.1f;
                float score = height + distancePenalty;

                if (score < lowestHeight) {
                    lowestHeight = height;
                    bestPos = candidatePos;
                }
            }
        }

        // Force the starting area to be a valley floor
        // This ensures the village is always in a proper valley, surrounded by higher terrain
        int valleyFloorHeight = _config.MinHeightLevel + 1; // Just above minimum (not underwater)

        // Cache the valley floor for the village area
        for (int dx = -VALLEY_FORCE_RADIUS; dx <= VALLEY_FORCE_RADIUS; dx++) {
            for (int dy = -VALLEY_FORCE_RADIUS; dy <= VALLEY_FORCE_RADIUS; dy++) {
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= VALLEY_FORCE_RADIUS) {
                    var pos = new Vector2Int(bestPos.X + dx, bestPos.Y + dy);
                    // Slightly vary the floor for natural look
                    int floorHeight = valleyFloorHeight + (int)(rng.NextDouble() * 0.5);
                    _state.HeightCache[pos] = floorHeight;
                }
            }
        }

        // Create valley walls - gradually rising terrain around the village
        for (int dx = -SEARCH_RADIUS; dx <= SEARCH_RADIUS; dx++) {
            for (int dy = -SEARCH_RADIUS; dy <= SEARCH_RADIUS; dy++) {
                var pos = new Vector2Int(bestPos.X + dx, bestPos.Y + dy);
                if (_state.HeightCache.ContainsKey(pos)) continue; // Already set as floor

                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= SEARCH_RADIUS) {
                    // Height rises with distance from valley center
                    float riseFromCenter = (dist - VALLEY_FORCE_RADIUS) / (SEARCH_RADIUS - VALLEY_FORCE_RADIUS);
                    riseFromCenter = Mathf.Clamp01(riseFromCenter);

                    // Add noise for natural walls
                    float noise = _variationNoise.GetNoise(pos.x * 2f, pos.y * 2f) * 0.3f;

                    int wallHeight = valleyFloorHeight + Mathf.RoundToInt(riseFromCenter * 4f + noise * 2f);
                    wallHeight = Mathf.Clamp(wallHeight, _config.MinHeightLevel, _config.MaxHeightLevel);

                    _state.HeightCache[pos] = wallHeight;
                }
            }
        }

        return new IsoPos(bestPos.X, bestPos.Y, new IsoLevel(valleyFloorHeight));
    }

    #endregion

    #region Public API - Dynamic Extension

    /// <summary>
    /// Called when player reaches a node. Extends path if needed.
    /// </summary>
    public bool OnPlayerReachedNode(IsoExpeditionPath path, IsoPathNode arrivedNode) {
        if (path.GenerationState == null) {
            path.GenerationState = _state;
        }
        _state = path.GenerationState;

        bool generated = false;

        int nodesAhead = CountNodesAhead(path, arrivedNode);
        if (nodesAhead < _config.ExtensionThreshold) {
            generated = ExtendPathFromNode(path, arrivedNode);
        }

        if (arrivedNode.Type == PathNodeType.Intersection) {
            EnsureTeaserBranchesExist(path, arrivedNode);
        }

        return generated;
    }

    /// <summary>
    /// Updates generation based on player position (IsoPos).
    /// </summary>
    public bool UpdateForPlayer(IsoExpeditionPath path, IsoPos playerPos) {
        if (path.GenerationState == null) return false;
        _state = path.GenerationState;

        var nearestNode = FindNearestNode(path, playerPos);
        if (nearestNode == null) return false;

        int nodesAhead = CountNodesAhead(path, nearestNode);
        if (nodesAhead < _config.ExtensionThreshold) {
            return ExtendPathFromNode(path, nearestNode);
        }

        return false;
    }

    /// <summary>
    /// Called when player chooses a path at an intersection.
    /// </summary>
	public bool OnPathChosen(IsoExpeditionPath path, IsoPathNode intersection, PathNodeId chosenNodeId) {
		_state = path.GenerationState ?? _state;

		// Find which branch contains the chosen node
		foreach (var (branchIndex, branchState) in _state.ActiveBranches.ToList()) {
			// Check if this branch starts at the intersection and leads to the chosen node
			if (branchState.StartNodeId == intersection.Id) {
				// A node belongs to this branch if its ID is between start and end
				bool isChosen = chosenNodeId > branchState.StartNodeId && 
					chosenNodeId <= branchState.EndNodeId;
                
				branchState.IsChosen = isChosen;

				if (isChosen) {
					_state.ActiveBranchIndex = branchIndex;
					if (branchState.IsTeaser) {
						ExtendChosenBranch(path, branchState);
					}
					Debug.Log($"IsoPathGenerator: Player chose branch {branchIndex}");
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Extends path from a frontier node.
	/// If the node isn't a frontier, it becomes one and gets extended.
	/// </summary>
	public bool ExtendPathFromNode(IsoExpeditionPath path, IsoPathNode fromNode) {
		_state = path.GenerationState ?? _state;
		_state.ChunkIndex++;

		var random = _state.GetChunkRandom();

		IsoPathNode? frontierNode;

		if (_state.FrontierNodeIds.Contains(fromNode.Id)) {
			// Node is already a frontier - use it directly
			frontierNode = fromNode;
		} else {
			// Node is not a frontier (e.g., dungeon at end of teaser branch)
			// Check if it's a dead end that needs extension
			bool isDeadEnd = fromNode.Connections.Count <= 1;

			if (isDeadEnd) {
				// This is a dead end - make it a frontier and extend from here
				frontierNode = fromNode;
				Debug.Log($"ExtendPathFromNode: Converting dead-end {fromNode.Id} to frontier");
			} else {
				// Not a dead end - find nearest existing frontier
				frontierNode = FindNearestFrontier(path, fromNode);
			}
		}

		if (frontierNode == null) {
			Debug.LogWarning($"ExtendPathFromNode: No frontier found, creating new path from {fromNode.Id}");
			// Last resort: just extend from the provided node
			frontierNode = fromNode;
		}

		GenerateChunkFromNode(path, frontierNode, _config.ChunkSize, ref random);

		// Remove from frontier list if it was there
		_state.FrontierNodeIds.Remove(frontierNode.Id);

		// Analyze new terrain
		AnalyzeTerrainFeatures(path);

		Debug.Log($"IsoPathGenerator: Extended from {frontierNode.Id}, chunk {_state.ChunkIndex}");
		return true;
	}

    #endregion

    #region Height Generation

	/// <summary>
	/// Pre-calculates heights for tiles between two positions and caches them.
	/// Ensures smooth height transitions along paths.
	/// </summary>
	private void CacheHeightsAlongPath(IsoPos from, IsoPos to, string? biomeId) {
		int steps = Mathf.Max(Mathf.Abs(to.X - from.X), Mathf.Abs(to.Y - from.Y)) * 2;
		if (steps == 0) return;

		int fromHeight = from.Level.Value;
		int toHeight = to.Level.Value;

		for (int i = 1; i < steps; i++) {  // Skip endpoints, they're already cached
			float t = (float)i / steps;
			int x = Mathf.RoundToInt(Mathf.Lerp(from.X, to.X, t));
			int y = Mathf.RoundToInt(Mathf.Lerp(from.Y, to.Y, t));
        
			var key = new Vector2Int(x, y);
			if (_state.HeightCache.ContainsKey(key)) continue;

			// Interpolate height with small noise variation
			float baseHeight = Mathf.Lerp(fromHeight, toHeight, t);
			float noise = _variationNoise.GetNoise(x, y) * 0.5f;  // Small variation
			int height = Mathf.RoundToInt(baseHeight + noise);
			height = Mathf.Clamp(height, _config.MinHeightLevel, _config.MaxHeightLevel);

			_state.HeightCache[key] = height;
		}
	}

	/// <summary>
	/// Calculates height for a position using noise and biome data.
	/// Uses a "feature detection" approach to create mostly flat terrain 
	/// with distinct elevated/depressed features.
	/// </summary>
	private int CalculateHeight(IsoPos pos, string? biomeId = null) {
		if (!_config.EnableHeightVariation) {
			return 0;
		}

		// Check cache first
		var key = new Vector2Int(pos.X, pos.Y);
		if (_state.HeightCache.TryGetValue(key, out int cachedHeight)) {
			return cachedHeight;
		}

		int height = CalculateHeightCore(pos);

		// Apply biome bias
		if (!string.IsNullOrEmpty(biomeId)) {
			float biomeBias = GetBiomeHeightBias(biomeId);
			if (Mathf.Abs(biomeBias) > 0.1f) {
				height += Mathf.RoundToInt(biomeBias * _config.BiomeHeightBias);
			}
		}

		// Clamp to valid range
		height = Mathf.Clamp(height, _config.MinHeightLevel, _config.MaxHeightLevel);

		_state.HeightCache[key] = height;
		return height;
	}


    /// <summary>
    /// Gets height bias for a biome (e.g., mountain biomes are higher).
    /// </summary>
    private float GetBiomeHeightBias(string biomeId) {
        if (_gameDb.TryGetProto<BiomeProto>(new BiomeProto.ID(biomeId), out var biome)) {
            // Use danger level as proxy for height (more dangerous = higher/mountainous)
            return biome.DangerLevel * 2f;
        }
        return 0f;
    }

    /// <summary>
    /// Analyzes all nodes for terrain features (hills, cliffs, mountains).
    /// Updates node metadata based on height deltas with neighbors.
    /// </summary>
    private void AnalyzeTerrainFeatures(IsoExpeditionPath path) {
        float hillThreshold = _config.HillThreshold.Value;
        float cliffThreshold = _config.CliffThreshold.Value;

        foreach (var node in path.Nodes.Values) {
            int nodeHeight = node.Position.Level.Value;
            var neighbors = GetNeighborHeights(node.Position);

            if (neighbors.Count == 0) continue;

            float maxDelta = 0f;
            float avgDelta = 0f;
            int higherNeighbors = 0;

            foreach (var neighborHeight in neighbors) {
                float delta = Mathf.Abs(nodeHeight - neighborHeight);
                maxDelta = Mathf.Max(maxDelta, delta);
                avgDelta += delta;

                if (neighborHeight > nodeHeight) {
                    higherNeighbors++;
                }
            }

            avgDelta /= neighbors.Count;

            // Determine terrain feature
            if (maxDelta >= cliffThreshold) {
                node.TerrainFeature = TerrainFeature.Cliff;
            } else if (maxDelta >= hillThreshold) {
                node.TerrainFeature = TerrainFeature.Hill;
            } else if (nodeHeight >= _config.MaxHeightLevel - 1 && higherNeighbors == 0) {
                node.TerrainFeature = TerrainFeature.MountainPeak;
            } else if (nodeHeight <= _config.MinHeightLevel + 1) {
                node.TerrainFeature = TerrainFeature.Valley;
            } else {
                node.TerrainFeature = TerrainFeature.Flat;
            }
        }
    }

    /// <summary>
    /// Gets heights of neighboring positions.
    /// </summary>
    private List<int> GetNeighborHeights(IsoPos pos) {
        var heights = new List<int>();
        var key = new Vector2Int(pos.X, pos.Y);

        // Check 8 neighbors
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                if (dx == 0 && dy == 0) continue;

                var neighborKey = new Vector2Int(pos.X + dx, pos.Y + dy);
                if (_state.HeightCache.TryGetValue(neighborKey, out int height)) {
                    heights.Add(height);
                }
            }
        }

        return heights;
    }

    #endregion

    #region Main Path Generation

    private void GenerateMainPathChunk(IsoExpeditionPath path, int startIndex, int nodeCount, IsoPos startPos) {
        var random = _state.GetChunkRandom();

        // Calculate initial height at start position
        int startHeight = CalculateHeight(startPos, _state.BiomeState.CurrentBiomeId);
        IsoPos currentPos = startPos.WithLevel(new IsoLevel(startHeight));

        AngleRPG currentAngle = startIndex == 0
            ? _state.StartingDirection
            : CalculateForwardAngle(path, GetLastMainPathNode(path)!);

        IsoPathNode? previousNode = startIndex == 0 ? null : GetLastMainPathNode(path);

        for (int i = 0; i < nodeCount; i++) {
            int globalIndex = startIndex + i;

            var nodeType = DetermineNodeType(globalIndex, ref random);

            // Check for guaranteed event
            var guaranteedEvent = GetGuaranteedEventForNode(globalIndex);
            if (guaranteedEvent.HasValue) {
                nodeType = PathNodeType.EventLocation;
            }

            // Tutorial dungeon
            if (_config.TutorialDungeonNodeIndex > 0 && globalIndex == _config.TutorialDungeonNodeIndex) {
                nodeType = PathNodeType.Dungeon;
            }

            var terrainId = GetTerrainForNode(globalIndex, ref random);

			var nodeId = _state.NodeIdFactory.GetNextId();
			var node = new IsoPathNode(nodeId, currentPos, nodeType) {
				TerrainId = terrainId,
				BiomeId = new BiomeProto.ID(_state.BiomeState.CurrentBiomeId),
				DangerLevel = _state.BiomeState.CurrentDangerLevel
			};

            // Apply guaranteed event
            if (guaranteedEvent.HasValue) {
                node.EventId = guaranteedEvent.Value.Value;
                node.Name = GetEventNodeName(guaranteedEvent.Value);
            }

            // Tutorial dungeon
            if (_config.TutorialDungeonNodeIndex > 0 && globalIndex == _config.TutorialDungeonNodeIndex) {
                if (_config.GuaranteedFirstDungeon.HasValue) {
                    var dungeonProto = _gameDb.Get<DungeonProto>(_config.GuaranteedFirstDungeon.Value);
                    if (dungeonProto != null) {
                        ApplyLocationToNode(node, dungeonProto);
                    }
                }
            }

            ApplyNodeTypeData(node, nodeType, globalIndex);

            path.AddNode(node);
            _state.TotalNodesGenerated++;

            if (globalIndex == 0) {
                path.SetStartNode(node);
            }

            // Connect to previous
            if (previousNode != null) {
                TilesRPG distance = CalculateDistance(previousNode.Position, currentPos);
                previousNode.AddConnection(node, distance, terrainId);
				CacheHeightsAlongPath(previousNode.Position, currentPos, _state.BiomeState.CurrentBiomeId);
            }

            previousNode = node;

            // Calculate next position
            if (i < nodeCount - 1) {
                currentPos = CalculateNextPosition(currentPos, ref currentAngle, path, ref random, isMainPath: true);
            }
        }

		// Mark last node as frontier
		if (previousNode != null && !_state.FrontierNodeIds.Contains(previousNode.Id)) {
			_state.FrontierNodeIds.Add(previousNode.Id);
		}
    }

    private void GenerateChunkFromNode(
        IsoExpeditionPath path,
        IsoPathNode startNode,
        int nodeCount,
		ref RandomStream rng
    ) {
        IsoPos currentPos = startNode.Position;
        AngleRPG currentAngle = CalculateForwardAngle(path, startNode);
        IsoPathNode previousNode = startNode;

        for (int i = 0; i < nodeCount; i++) {
            currentPos = CalculateNextPosition(currentPos, ref currentAngle, path, ref rng, isMainPath: true);

            bool isBranchPoint = ShouldCreateBranch(i, ref rng);
            var nodeType = isBranchPoint ? PathNodeType.Intersection : PathNodeType.Waypoint;

            if (!isBranchPoint) {
                nodeType = DetermineWaypointContent(ref rng);
            }

            var terrainId = GetTerrainForNode(_state.TotalNodesGenerated, ref rng);

			var nodeId = _state.NodeIdFactory.GetNextId();
            var node = new IsoPathNode(nodeId, currentPos, nodeType) {
                TerrainId = terrainId,
                BiomeId = new BiomeProto.ID(_state.BiomeState.CurrentBiomeId),
                DangerLevel = _state.BiomeState.CurrentDangerLevel
            };

            ApplyWaypointContent(node, nodeType, ref rng);

            path.AddNode(node);
            _state.TotalNodesGenerated++;

            TilesRPG distance = CalculateDistance(previousNode.Position, currentPos);
            previousNode.AddConnection(node, distance, terrainId);

            previousNode = node;

            if (isBranchPoint) {
                _state.LastBranchNodeIndex = _state.TotalNodesGenerated;
                GenerateTeaserBranches(path, node, ref rng);

                // FIX: Do NOT add intersection to frontiers.
                // The intersection is a decision point where generation must pause.
                // Teaser branches are generated but not yet active frontiers.
                // They become frontiers only when the player chooses a branch (in OnPathChosen).

                AdvanceBiomeProgress(i + 1);
                Debug.Log($"GenerateChunkFromNode: Intersection at {node.Id}");
                return;
            }
        }

        if (!_state.FrontierNodeIds.Contains(previousNode.Id)) {
            _state.FrontierNodeIds.Add(previousNode.Id);
        }

        AdvanceBiomeProgress(nodeCount);
    }

    #endregion

    #region Teaser Branch Generation

	/// <summary>
	/// Calculates a pair of branch angles that:
	/// 1. Are both biased toward the forward direction
	/// 2. Have at least MinBranchSeparation between them
	/// 3. Don't exceed MaxBranchDeviationFromForward from the travel direction
	/// </summary>
	private (AngleRPG left, AngleRPG right) CalculateBranchPair(AngleRPG forwardAngle, ref RandomStream rng) {
		float minSeparation = _config.MinBranchSeparation.Degrees;
		float maxDeviation = _config.MaxBranchDeviationFromForward.Degrees;

		// Each branch gets half the separation as base offset, plus small random variance
		float halfSeparation = minSeparation / 2f;
        
		// Small random adjustment (but keep within max deviation)
		float maxVariance = Mathf.Min(10f, maxDeviation - halfSeparation);
		float leftVariance = rng.NextFloat() * maxVariance;
		float rightVariance = rng.NextFloat() * maxVariance;

		// Left branch goes left of forward, right goes right
		float leftOffset = -(halfSeparation + leftVariance);
		float rightOffset = (halfSeparation + rightVariance);

		// Clamp to max deviation from forward
		leftOffset = Mathf.Clamp(leftOffset, -maxDeviation, 0);
		rightOffset = Mathf.Clamp(rightOffset, 0, maxDeviation);

		// Ensure we still meet minimum separation after clamping
		float actualSeparation = rightOffset - leftOffset;
		if (actualSeparation < minSeparation) {
			float deficit = minSeparation - actualSeparation;
			leftOffset -= deficit / 2f;
			rightOffset += deficit / 2f;
		}

		AngleRPG leftAngle = forwardAngle.Rotate(leftOffset);
		AngleRPG rightAngle = forwardAngle.Rotate(rightOffset);

		return (leftAngle, rightAngle);
	}

	 /// <summary>
    /// Generates exactly two teaser branches at an intersection.
    /// Both branches are biased forward with minimum angular separation.
    /// </summary>
	private void GenerateTeaserBranches(IsoExpeditionPath path, IsoPathNode intersection, ref RandomStream rng) {
		AngleRPG forwardAngle = CalculateForwardAngle(path, intersection);

		// Always exactly 2 branches
		var (leftAngle, rightAngle) = CalculateBranchPair(forwardAngle, ref rng);

		// Create left branch
		int leftBranchIndex = _state.GetNextBranchIndex();
		if (!_state.ActiveBranches.ContainsKey(leftBranchIndex)) {
			var leftBranch = new BranchState {
				BranchIndex = leftBranchIndex,
				StartNodeId = intersection.Id,
				Depth = 1,
				IsTeaser = true,
				IsChosen = false,
				BaseAngleRadians = leftAngle.Radians,
				BiomeId = SelectBranchBiome(ref rng)
			};
			GenerateTeaserPath(path, intersection, leftBranch, ref rng);
			_state.ActiveBranches[leftBranchIndex] = leftBranch;
		}

		// Create right branch
		int rightBranchIndex = _state.GetNextBranchIndex();
		if (!_state.ActiveBranches.ContainsKey(rightBranchIndex)) {
			var rightBranch = new BranchState {
				BranchIndex = rightBranchIndex,
				StartNodeId = intersection.Id,
				Depth = 1,
				IsTeaser = true,
				IsChosen = false,
				BaseAngleRadians = rightAngle.Radians,
				BiomeId = SelectBranchBiome(ref rng)
			};
			GenerateTeaserPath(path, intersection, rightBranch, ref rng);
			_state.ActiveBranches[rightBranchIndex] = rightBranch;
		}

		_state.FrontierNodeIds.Remove(intersection.Id);
        
		float separation = Mathf.Abs(leftAngle.ShortestTurn(rightAngle));
		Debug.Log($"GenerateTeaserBranches: Created 2 branches from node {intersection.Id.Value}, " +
			$"L={leftAngle.Degrees:F0}°, R={rightAngle.Degrees:F0}°, separation={separation:F0}°");
	}

    /// <summary>
    /// Adjusts a branch angle to ensure minimum separation from existing branches.
    /// </summary>
    private AngleRPG EnforceBranchSeparation(
        AngleRPG candidate,
        List<AngleRPG> existingAngles,
        float minSeparation,
        float preferredSign,
		ref RandomStream rng
    ) {
        if (existingAngles.Count == 0) {
            return candidate;
        }

        const int MAX_ATTEMPTS = 10;
        AngleRPG bestAngle = candidate;
        float bestMinSeparation = 0f;

        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++) {
            float worstSeparation = float.MaxValue;

            // Check separation from all existing branches
            foreach (var existing in existingAngles) {
                float separation = Mathf.Abs(candidate.ShortestTurn(existing));
                worstSeparation = Mathf.Min(worstSeparation, separation);
            }

            // If we meet minimum separation, we're done
            if (worstSeparation >= minSeparation) {
                return candidate;
            }

            // Track best attempt
            if (worstSeparation > bestMinSeparation) {
                bestMinSeparation = worstSeparation;
                bestAngle = candidate;
            }

            // Adjust angle away from closest existing branch
            AngleRPG closestExisting = existingAngles[0];
            float closestDist = float.MaxValue;
            foreach (var existing in existingAngles) {
                float dist = Mathf.Abs(candidate.ShortestTurn(existing));
                if (dist < closestDist) {
                    closestDist = dist;
                    closestExisting = existing;
                }
            }

            // Push away from closest branch
            float pushDirection = Mathf.Sign(candidate.ShortestTurn(closestExisting));
            float pushAmount = (minSeparation - closestDist) + 5f; // Extra 5° buffer
            
            // Push in preferred direction if possible
            if (preferredSign != 0 && Mathf.Sign(pushDirection) != preferredSign) {
                pushDirection = preferredSign;
                pushAmount = minSeparation * 0.5f;
            }

            candidate = candidate.Rotate(-pushDirection * pushAmount);
        }

        // Return best attempt if we couldn't achieve full separation
        Debug.LogWarning($"EnforceBranchSeparation: Could not achieve {minSeparation}° separation, best was {bestMinSeparation:F1}°");
        return bestAngle;
    }

	/// <summary>
	/// Calculates a branch angle ensuring minimum separation from existing branches.
	/// Creates distinct "Y" shaped splits rather than near-parallel paths.
	/// </summary>
	private AngleRPG CalculateBranchAngleWithSeparation(
		int branchIndex,
		int totalBranches,
		AngleRPG forwardAngle,
		List<AngleRPG> existingBranchAngles,
		ref RandomStream rng
	) {
		float minSeparation = _config.MinBranchSeparation.Degrees;
		float maxDrift = _config.MaxDriftFromStart.Degrees * 1.5f;
		AngleRPG startingDirection = _state.StartingDirection;

		// Calculate base offset - alternate left/right from forward
		float sign = (branchIndex % 2 == 0) ? 1f : -1f;
        
		// Base offset is at least half the minimum separation
		float baseOffset = minSeparation * 0.6f;
        
		// Spread multiplier for additional branches
		float spreadMultiplier = 1f;
		if (totalBranches > 2) {
			int pairIndex = branchIndex / 2;
			spreadMultiplier = 1f + (pairIndex * 0.4f);
		}

		// Small random variance (but not enough to violate separation)
		float maxVariance = minSeparation * 0.2f;
		float randomOffset = (float)(rng.NextFloat() - 0.5) * 2f * maxVariance;

		float targetOffset = sign * baseOffset * spreadMultiplier + randomOffset;

		// Calculate candidate angle
		AngleRPG candidateAngle = forwardAngle.Rotate(targetOffset);

		// Enforce separation from all existing branches
		candidateAngle = EnforceBranchSeparation(candidateAngle, existingBranchAngles, minSeparation, sign, ref rng);

		// Clamp to max drift from starting direction
		float driftFromStart = candidateAngle.ShortestTurn(startingDirection);
		if (Mathf.Abs(driftFromStart) > maxDrift) {
			float clampedDrift = Mathf.Sign(driftFromStart) * maxDrift;
			candidateAngle = startingDirection.Rotate(-clampedDrift);
		}

		return candidateAngle;
	}

	private void GenerateTeaserPath(
		IsoExpeditionPath path,
		IsoPathNode startNode,
		BranchState branchState,
		ref RandomStream rng
	) {
		int teaserLength = _config.TeaserBranchLength;
		IsoPos currentPos = startNode.Position;
		AngleRPG currentAngle = branchState.BaseAngle;
		IsoPathNode previousNode = startNode;

		for (int i = 0; i < teaserLength; i++) {
			currentPos = CalculateNextPosition(currentPos, ref currentAngle, path, ref rng, isMainPath: false);

			var nodeType = PathNodeType.Waypoint;

			if (i == teaserLength - 1) {
				if (_config.DungeonChanceRPG.RollsSuccess()) {
					nodeType = PathNodeType.Dungeon;
				} else if (ChanceRPG.FiftyFifty.RollsSuccess()) {
					nodeType = PathNodeType.Landmark;
				}
			}

			var terrainId = GetTerrainForBranch(branchState.BiomeId, ref rng);

			var nodeId = _state.NodeIdFactory.GetNextId();
			var node = new IsoPathNode(nodeId, currentPos, nodeType) {
				TerrainId = terrainId,
				BiomeId = new BiomeProto.ID(branchState.BiomeId),
				DangerLevel = _state.BiomeState.CurrentDangerLevel + 0.05f
			};

			if (nodeType == PathNodeType.Dungeon) {
				ApplyDungeonToNode(node, ref rng);
			} else if (nodeType == PathNodeType.Landmark) {
				ApplyLandmarkToNode(node, ref rng);
			}

			path.AddNode(node);
			branchState.NodesGenerated++;

			TilesRPG distance = CalculateDistance(previousNode.Position, currentPos);
			previousNode.AddConnection(node, distance, terrainId);

			previousNode = node;
		}

		branchState.EndNodeId = previousNode.Id;
	}

    private void ExtendChosenBranch(IsoExpeditionPath path, BranchState branchState) {
        var endNode = path.GetNodeById(branchState.EndNodeId);
        if (endNode == null) return;

        branchState.IsTeaser = false;
        var random = _state.GetBranchRandom(branchState.BranchIndex);

        IsoPos currentPos = endNode.Position;
        AngleRPG currentAngle = branchState.BaseAngle;
        IsoPathNode previousNode = endNode;

        int extensionLength = _config.ChosenBranchExtension;

        for (int i = 0; i < extensionLength; i++) {
            currentPos = CalculateNextPosition(currentPos, ref currentAngle, path, ref random, isMainPath: false);

            var nodeType = DetermineWaypointContent(ref random);

            bool isSubBranch = ShouldCreateSubBranch(branchState.Depth, i, ref random);
            if (isSubBranch) {
                nodeType = PathNodeType.Intersection;
            }

            var terrainId = GetTerrainForBranch(_state.BiomeState.CurrentBiomeId, ref random);

            var nodeId = _state.NodeIdFactory.GetNextId();
            var node = new IsoPathNode(nodeId, currentPos, nodeType) {
                TerrainId = terrainId,
                BiomeId = new BiomeProto.ID(_state.BiomeState.CurrentBiomeId),
                DangerLevel = _state.BiomeState.CurrentDangerLevel + (branchState.Depth * 0.1f)
            };

            ApplyWaypointContent(node, nodeType, ref random);

            path.AddNode(node);
            branchState.NodesGenerated++;

            TilesRPG distance = CalculateDistance(previousNode.Position, currentPos);
            previousNode.AddConnection(node, distance, terrainId);

            previousNode = node;

            if (isSubBranch && branchState.Depth < _config.MaxBranchDepth) {
                GenerateTeaserBranches(path, node, ref random);
                branchState.EndNodeId = node.Id;
                Debug.Log($"Extended branch {branchState.BranchIndex}: sub-intersection at {i + 1}");
                return;
            }
        }

        branchState.EndNodeId = previousNode.Id;

        if (!_state.FrontierNodeIds.Contains(previousNode.Id)) {
            _state.FrontierNodeIds.Add(previousNode.Id);
        }

        Debug.Log($"Extended branch {branchState.BranchIndex}: +{extensionLength} nodes");
    }

    #endregion

    #region Tutorial Generation

	private void GenerateTutorialIntersection(IsoExpeditionPath path) {
		var intersectionNode = path.Nodes.Values
			.FirstOrDefault(n => n.Type == PathNodeType.Intersection);

		if (intersectionNode == null) {
			Debug.LogWarning("Tutorial intersection not found");
			return;
		}

		var rng = GameRandom.For("PathGen.Tutorial", _state.BaseSeed);
        
		// Use the same 2-branch logic as regular intersections
		GenerateTeaserBranches(path, intersectionNode, ref rng);
        
		_state.TutorialBranchesGenerated = true;
		Debug.Log($"GenerateTutorialIntersection: Created 2 branches from {intersectionNode.Id}");
	}

    #endregion

    #region Position Calculation

    /// <summary>
    /// Calculates next position using TilesRPG and IsoPos.
    /// Supports negative coordinates for infinite generation in all directions.
    /// </summary>
    private IsoPos CalculateNextPosition(
        IsoPos currentPos,
        ref AngleRPG currentAngle,
        IsoExpeditionPath path,
        ref RandomStream rng,
        bool isMainPath
    ) {
        IsoPos result = isMainPath
            ? CalculateMainPathPosition(currentPos, ref currentAngle, path, ref rng)
            : CalculateBranchPosition(currentPos, ref currentAngle, path, ref rng);

        return result;
    }

    private IsoPos CalculateMainPathPosition(
        IsoPos currentPos,
        ref AngleRPG currentAngle,
        IsoExpeditionPath path,
        ref RandomStream rng
    ) {
        AngleRPG startingDirection = _state.StartingDirection;

        // Calculate drift correction
        float driftFromStart = currentAngle.ShortestTurn(startingDirection);
        float maxDrift = _config.MaxDriftFromStart.Degrees;

        float correctionStrength = 0f;
        if (Mathf.Abs(driftFromStart) > maxDrift * 0.7f) {
            float driftRatio = Mathf.Abs(driftFromStart) / maxDrift;
            correctionStrength = Mathf.Lerp(0f, 0.5f, (driftRatio - 0.7f) / 0.3f);
        }

        float startPull = driftFromStart * correctionStrength;
        float localPull = -currentAngle.SignedDegrees * _config.ForwardBias * 0.1f;
        float varianceDegrees = _config.DirectionVariance.Degrees;
        float variance = (float)((rng.NextFloat() - 0.5) * 2 * varianceDegrees);

        AngleRPG baseAngle = currentAngle.Rotate(startPull + localPull + variance);

        float clampedDrift = Mathf.Clamp(baseAngle.ShortestTurn(startingDirection), -maxDrift, maxDrift);
        baseAngle = startingDirection.Rotate(-clampedDrift);

        // Get spacing
        float nodeSpacing = _config.GetNodeSpacing(_state.TotalNodesGenerated).Value;
        float minProgress = _config.GetMinForwardProgress(_state.TotalNodesGenerated).Value;

        // === VALLEY-SEEKING: Sample multiple candidates and pick lowest valid one ===
        int currentHeight = currentPos.Level.Value;
        IsoPos bestCandidate = currentPos;
        float bestScore = float.MaxValue;

        int sampleCount = _config.PathCandidateSamples;
        float angleSpread = _config.DirectionVariance.Degrees * 0.8f;

        for (int i = 0; i < sampleCount; i++) {
            // Spread samples across the valid angle range
            float sampleOffset = (i - sampleCount / 2f) * (angleSpread / sampleCount);
            AngleRPG sampleAngle = baseAngle.Rotate(sampleOffset);

            Vector2 direction = sampleAngle.ToDirection2D();
            int dx = Mathf.RoundToInt(direction.x * nodeSpacing);
            int dy = Mathf.RoundToInt(direction.y * nodeSpacing);

            // Ensure minimum movement
            float magnitude = Mathf.Sqrt(dx * dx + dy * dy);
            if (magnitude < minProgress && magnitude > 0.001f) {
                float scale = minProgress / magnitude;
                dx = Mathf.RoundToInt(dx * scale);
                dy = Mathf.RoundToInt(dy * scale);
            } else if (magnitude < 0.001f) {
                dx = Mathf.RoundToInt(Mathf.Cos(sampleAngle.Radians) * minProgress);
                dy = Mathf.RoundToInt(Mathf.Sin(sampleAngle.Radians) * minProgress);
                if (dx == 0 && dy == 0) {
                    dx = rng.NextInt(-1, 2);
                    dy = rng.NextInt(-1, 2);
                    if (dx == 0 && dy == 0) dx = rng.NextInt(0, 2) * 2 - 1;
                }
            }

            IsoPos candidatePos2D = currentPos + new IsoRel(dx, dy);
            int candidateHeight = CalculateHeight(candidatePos2D, _state.BiomeState.CurrentBiomeId);

            // Skip if climb is too steep
            int heightDelta = candidateHeight - currentHeight;
            if (heightDelta > _config.MaxClimbPerNode) {
                continue;
            }

            // Score: lower height = better, descending = bonus
            float heightScore = candidateHeight;

            // Bonus for going downhill
            if (heightDelta < 0) {
                heightScore += heightDelta * _config.DescentBonus;
            }

            // Penalty for steep climbs
            if (heightDelta > 0) {
                heightScore += heightDelta * 2f;
            }

            // Small penalty for deviation from base angle (prefer forward)
            float angleDeviation = Mathf.Abs(sampleOffset) / angleSpread;
            heightScore += angleDeviation * 0.5f;

            // Apply valley-seeking strength
            float finalScore = Mathf.Lerp(angleDeviation, heightScore, _config.ValleySeekingStrength);

            if (finalScore < bestScore) {
                bestScore = finalScore;
                bestCandidate = new IsoPos(candidatePos2D.X, candidatePos2D.Y, new IsoLevel(candidateHeight));

                // Update angle to match chosen direction
                currentAngle = sampleAngle;
            }
        }

        // If no valid candidate found, fall back to original logic
        if (bestCandidate == currentPos) {
            Vector2 direction = baseAngle.ToDirection2D();
            int dx = Mathf.RoundToInt(direction.x * nodeSpacing);
            int dy = Mathf.RoundToInt(direction.y * nodeSpacing);

            if (Mathf.Sqrt(dx * dx + dy * dy) < minProgress) {
                dx = Mathf.RoundToInt(direction.x * minProgress);
                dy = Mathf.RoundToInt(direction.y * minProgress);
            }

            IsoPos fallbackPos2D = currentPos + new IsoRel(dx, dy);
            int fallbackHeight = CalculateHeight(fallbackPos2D, _state.BiomeState.CurrentBiomeId);
            bestCandidate = new IsoPos(fallbackPos2D.X, fallbackPos2D.Y, new IsoLevel(fallbackHeight));
            currentAngle = baseAngle;
        }

        return AvoidCollision(path, bestCandidate, ref rng, currentAngle);
    }

    /// <summary>
    /// Calculates next position for a branch, favoring valleys and low terrain.
    /// Branches maintain forward momentum while avoiding steep climbs.
    /// </summary>
    private IsoPos CalculateBranchPosition(
        IsoPos currentPos,
        ref AngleRPG currentAngle,
        IsoExpeditionPath path,
        ref RandomStream rng
    ) {
        float nodeSpacing = _config.BranchNodeSpacing.Value;
        int currentHeight = currentPos.Level.Value;

        // Sample multiple candidates and pick the one with lowest terrain
        int sampleCount = _config.PathCandidateSamples;
        float angleSpread = 15f; // Tighter spread for branches - they commit to direction

        IsoPos bestCandidate = currentPos;
        float bestScore = float.MaxValue;
        AngleRPG bestAngle = currentAngle;

        for (int i = 0; i < sampleCount; i++) {
            // Small angle variations around the branch direction
            float sampleOffset = (i - sampleCount / 2f) * (angleSpread / sampleCount);
            AngleRPG sampleAngle = currentAngle.Rotate(sampleOffset);

            Vector2 direction = sampleAngle.ToDirection2D();
            int dx = Mathf.RoundToInt(direction.x * nodeSpacing);
            int dy = Mathf.RoundToInt(direction.y * nodeSpacing);

            // Ensure minimum movement
            float magnitude = Mathf.Sqrt(dx * dx + dy * dy);
            if (magnitude < 2f) {
                dx = Mathf.RoundToInt(direction.x * 3f);
                dy = Mathf.RoundToInt(direction.y * 3f);
            }

            IsoPos candidatePos2D = currentPos + new IsoRel(dx, dy);
            int candidateHeight = CalculateHeight(candidatePos2D, _state.BiomeState.CurrentBiomeId);

            // Skip if climb is too steep
            int heightDelta = candidateHeight - currentHeight;
            if (heightDelta > _config.MaxClimbPerNode) {
                continue;
            }

            // Score: prefer low terrain and gentle slopes
            float heightScore = candidateHeight * 2f;

            // Bonus for descending
            if (heightDelta < 0) {
                heightScore += heightDelta * _config.DescentBonus;
            }

            // Penalty for climbing
            if (heightDelta > 0) {
                heightScore += heightDelta * 3f;
            }

            // Small penalty for deviating from branch angle
            float angleDeviation = Mathf.Abs(sampleOffset) / angleSpread;
            heightScore += angleDeviation * 0.3f;

            // Apply valley-seeking strength
            float finalScore = Mathf.Lerp(angleDeviation, heightScore, _config.ValleySeekingStrength);

            if (finalScore < bestScore) {
                bestScore = finalScore;
                bestCandidate = new IsoPos(candidatePos2D.X, candidatePos2D.Y, new IsoLevel(candidateHeight));
                bestAngle = sampleAngle;
            }
        }

        // Fallback if no valid candidate found
        if (bestCandidate == currentPos) {
            Vector2 direction = currentAngle.ToDirection2D();
            int dx = Mathf.RoundToInt(direction.x * nodeSpacing);
            int dy = Mathf.RoundToInt(direction.y * nodeSpacing);
            if (dx == 0 && dy == 0) {
                dx = rng.NextInt(-1, 2);
                dy = rng.NextInt(-1, 2);
                if (dx == 0 && dy == 0) dx = 1;
            }

            IsoPos fallbackPos = currentPos + new IsoRel(dx, dy);
            int fallbackHeight = CalculateHeight(fallbackPos, _state.BiomeState.CurrentBiomeId);
            bestCandidate = new IsoPos(fallbackPos.X, fallbackPos.Y, new IsoLevel(fallbackHeight));
        } else {
            // Update angle to match chosen direction
            currentAngle = bestAngle;
        }

        return AvoidCollision(path, bestCandidate, ref rng, currentAngle);
    }

	private AngleRPG CalculateForwardAngle(IsoExpeditionPath path, IsoPathNode node) {
		// Find the node we came FROM (the parent/previous node in the path)
		IsoPathNode? previousNode = FindPreviousNodeInPath(path, node);

		if (previousNode != null) {
			// Forward direction = FROM previous TO current (continuing the same direction)
			// NOTE: IsoPos subtraction (a - b) returns b.RelativeTo(a), which is a - b
			// So we need (current - previous) which gives us the direction FROM previous TO current
			// But the operator is backwards! It does: node.Position.RelativeTo(previousNode.Position)
			// which is: previousNode - node (the OPPOSITE of what we want)
			
			// Fix: swap the order or negate the result
			IsoRel offset = previousNode.Position - node.Position;  // This gives us: node - previous (what we want)
			AngleRPG forwardAngle = AngleRPG.FromDirection(new Vector2(offset.X, offset.Y));
            
			Debug.Log($"CalculateForwardAngle: node={node.Id} (NodeId={node.Id.Value}), " +
				$"previous={previousNode.Id} (NodeId={previousNode.Id.Value}), " +
				$"offset=({offset.X}, {offset.Y}), angle={forwardAngle.Degrees:F0}°");
            
			return forwardAngle;
		}

		// Fallback: use starting direction
		AngleRPG fallbackAngle = _state?.StartingDirection ?? _config.InitialDirection;
		Debug.Log($"CalculateForwardAngle: node={node.Id} has NO previous node, using fallback={fallbackAngle.Degrees:F0}°");
		return fallbackAngle;
	}

    /// <summary>
    /// Finds the previous node in the path leading to this node.
    /// For main path nodes, this is the node with a lower main path index.
    /// For branch starts, this is the main path node that connects to the intersection.
    /// </summary>
	private IsoPathNode? FindPreviousNodeInPath(IsoExpeditionPath path, IsoPathNode node) {
		IsoPathNode? previousNode = null;
        
		foreach (var conn in node.Connections) {
			var targetNode = path.GetNode(conn.TargetNodeId);
			if (targetNode == null) continue;
            
			// Previous node has a LOWER ID (created earlier)
			if (targetNode.Id < node.Id) {
				// Take the one with the highest ID that's still less than ours
				if (previousNode == null || targetNode.Id > previousNode.Id) {
					previousNode = targetNode;
				}
			}
		}
        
		return previousNode;
	}

    private IsoPos AvoidCollision(IsoExpeditionPath path, IsoPos pos, ref RandomStream rng, AngleRPG currentAngle) {
        int attempts = 0;
        Vector2 direction = currentAngle.ToDirection2D();

        while (path.GetNodeAt(pos) != null && attempts < 10) {
            int nudgeX, nudgeY;

            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)) {
                nudgeX = direction.x >= 0 ? 1 : -1;
                nudgeY = rng.NextInt(-1, 2); // -1, 0, or 1
            } else {
                nudgeX = rng.NextInt(-1, 2);
                nudgeY = direction.y >= 0 ? 1 : -1;
            }

            pos += new IsoRel(nudgeX, nudgeY);
            attempts++;
        }

        return pos;
    }

    /// <summary>
    /// Calculates 3D distance between two IsoPos using TilesRPG.
    /// </summary>
    private TilesRPG CalculateDistance(IsoPos from, IsoPos to) {
        int dx = Mathf.Abs(to.X - from.X);
        int dy = Mathf.Abs(to.Y - from.Y);
        int dz = Mathf.Abs(to.Level.Value - from.Level.Value);
        float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        return TilesRPG.FromTiles(dist);
    }

    #endregion

    #region Biome & Terrain

    private void InitializeBiomeState() {
        if (_mainPathBiomes.Count == 0) {
            CreateFallbackBiomes();
        }
		var biomeRng = GameRandom.For("PathGen.Biome", _state.BaseSeed);
        var startBiome = _mainPathBiomes[0];
        _state.BiomeState = new BiomeStateData {
            CurrentBiomeId = startBiome.Id.Value,
            CurrentBiomeIndex = 0,
            NodesRemainingInBiome = startBiome.GetRandomLength(ref biomeRng),
            CurrentDangerLevel = startBiome.DangerLevel
        };
    }

    private void AdvanceBiomeProgress(int nodesGenerated) {
        _state.BiomeState.NodesRemainingInBiome -= nodesGenerated;

        if (_state.BiomeState.NodesRemainingInBiome <= 0) {
            int nextIndex = Math.Min(_state.BiomeState.CurrentBiomeIndex + 1, _mainPathBiomes.Count - 1);
            var nextBiome = _mainPathBiomes[nextIndex];

            _state.BiomeState.CurrentBiomeId = nextBiome.Id.Value;
            _state.BiomeState.CurrentBiomeIndex = nextIndex;
			var chunkRng = _state.GetChunkRandom();
            _state.BiomeState.NodesRemainingInBiome = nextBiome.GetRandomLength(ref chunkRng);
            _state.BiomeState.CurrentDangerLevel = nextBiome.DangerLevel;

            Debug.Log($"Biome transition: {nextBiome.DisplayText.Name}");
        }

        _state.BiomeState.CurrentDangerLevel += 0.001f * nodesGenerated;
    }

    private TerrainProto.ID GetTerrainForNode(int nodeIndex, ref RandomStream rng) {
        if (_gameDb.TryGetProto<BiomeProto>(new BiomeProto.ID(_state.BiomeState.CurrentBiomeId), out var biome)) {
            return biome.SelectTerrain(ref rng);
        }
        return Ids.Terrains.Plains.Grass;
    }

    private TerrainProto.ID GetTerrainForBranch(string biomeId, ref RandomStream rng) {
        if (_gameDb.TryGetProto<BiomeProto>(new BiomeProto.ID(biomeId), out var biome)) {
            return biome.SelectTerrain(ref rng);
        }
        return Ids.Terrains.Plains.Grass;
    }

    private string SelectBranchBiome(ref RandomStream rng) {
        if (_branchBiomes.Count == 0) return _state.BiomeState.CurrentBiomeId;

        var candidates = _branchBiomes
            .Where(b => b.DangerLevel <= _state.BiomeState.CurrentDangerLevel + 0.3f)
            .ToList();

        if (candidates.Count == 0) candidates = _branchBiomes;

        return candidates[rng.NextInt(candidates.Count)].Id.Value;
    }

    #endregion

    #region Node Type & Content

    private PathNodeType DetermineNodeType(int nodeIndex, ref RandomStream rng) {
        if (nodeIndex == 0) return PathNodeType.Village;

        if (_config.FirstBranchNodeIndex > 0 && nodeIndex == _config.FirstBranchNodeIndex) {
            return PathNodeType.Intersection;
        }

        return PathNodeType.Waypoint;
    }

    private PathNodeType DetermineWaypointContent(ref RandomStream rng) {
        float roll = (float)rng.NextDouble();

        float eventProb = _config.EventChanceRPG.Probability;
        float landmarkProb = _config.LandmarkChanceRPG.Probability;
        float resourceProb = _config.ResourceNodeChanceRPG.Probability;

        if (roll < eventProb) return PathNodeType.EventLocation;
        if (roll < eventProb + landmarkProb) return PathNodeType.Landmark;
        if (roll < eventProb + landmarkProb + resourceProb) return PathNodeType.ResourceNode;

        if (_state.TotalNodesGenerated % _config.CampsiteSpacing == 0) {
            return PathNodeType.CampSite;
        }

        return PathNodeType.Waypoint;
    }

    private void ApplyWaypointContent(IsoPathNode node, PathNodeType nodeType, ref RandomStream rng) {
        switch (nodeType) {
            case PathNodeType.EventLocation:
                node.EventId = SelectRandomEventId(ref rng);
                break;
            case PathNodeType.Landmark:
                ApplyLandmarkToNode(node, ref rng);
                break;
            case PathNodeType.CampSite:
                ApplyCampToNode(node, ref rng);
                break;
            case PathNodeType.Dungeon:
                ApplyDungeonToNode(node, ref rng);
                break;
        }
    }

    private void ApplyNodeTypeData(IsoPathNode node, PathNodeType nodeType, int nodeIndex) {
        if (nodeType == PathNodeType.Village) {
            var homeVillage = _gameDb.Get<SettlementProto>(Ids.Locations.Settlements.HavenVillage);
            if (homeVillage != null) ApplyLocationToNode(node, homeVillage);
        }
    }

    private void ApplyDungeonToNode(IsoPathNode node, ref RandomStream rng) {
        if (_dungeonProtos.Count == 0) return;
        var dungeon = _dungeonProtos[rng.NextInt(_dungeonProtos.Count)];
        ApplyLocationToNode(node, dungeon);
    }

    private void ApplyLandmarkToNode(IsoPathNode node, ref RandomStream rng) {
        if (_landmarkProtos.Count == 0) return;
        var landmark = _landmarkProtos[rng.NextInt(_landmarkProtos.Count)];
        ApplyLocationToNode(node, landmark);
    }

    private void ApplyCampToNode(IsoPathNode node, ref RandomStream rng) {
        if (_campProtos.Count == 0) return;
        var camp = _campProtos[rng.NextInt(_campProtos.Count)];
        ApplyLocationToNode(node, camp);
    }

    private void ApplyLocationToNode(IsoPathNode node, MapLocationProto location) {
        node.Name = location.DisplayText.Name;
        node.Description = location.DisplayText.Description;
        node.LocationProtoId = location.Id.Value;

        if (location.FirstVisitEventId.HasValue) {
            node.EventId = location.FirstVisitEventId.Value.Value;
        }
    }

    private string? SelectRandomEventId(ref RandomStream rng) {
        var validEvents = _gameDb.GetAll<EventProto>()
            .Where(e => e.Type != EventType.Story)
            .Where(e => !e.Id.Value.Contains("Intro_"))
            .ToList();

        if (validEvents.Count == 0) return null;

        float totalWeight = validEvents.Sum(e => e.SpawnWeight);
        float roll = (float)(rng.NextFloat() * totalWeight);
        float cumulative = 0;

        foreach (var evt in validEvents) {
            cumulative += evt.SpawnWeight;
            if (roll <= cumulative) return evt.Id.Value;
        }

        return validEvents.LastOrDefault()?.Id.Value;
    }

    #endregion

        #region Helpers

    private EventProto.ID? GetGuaranteedEventForNode(int nodeIndex) {
        if (_config.GuaranteedStartEvents.Count == 0) return null;
        if (_state.GuaranteedEventIndex >= _config.GuaranteedStartEvents.Count) return null;

        int targetIndex = _state.GuaranteedEventIndex < _config.GuaranteedEventNodeIndices.Count
            ? _config.GuaranteedEventNodeIndices[_state.GuaranteedEventIndex]
            : 1 + _state.GuaranteedEventIndex * 2;

        if (nodeIndex == targetIndex) {
            var eventId = _config.GuaranteedStartEvents[_state.GuaranteedEventIndex];
            _state.GuaranteedEventIndex++;
            return eventId;
        }

        return null;
    }

    private string GetEventNodeName(EventProto.ID eventId) {
        if (_gameDb.TryGetProto<EventProto>(eventId, out var proto)) {
            return proto.Title;
        }
        return "Point of Interest";
    }

    private bool ShouldCreateBranch(int indexInChunk, ref RandomStream rng) {
        int nodesSinceBranch = _state.TotalNodesGenerated - _state.LastBranchNodeIndex;

        if (nodesSinceBranch < _config.MinNodesBetweenBranches) return false;

        float roll = rng.NextFloat();
        return roll < _config.IntersectionChanceRPG.Probability;
    }

    private bool ShouldCreateSubBranch(int depth, int indexInBranch, ref RandomStream rng) {
        if (depth >= _config.MaxBranchDepth) return false;
        if (indexInBranch < 5) return false;

        if (indexInBranch >= 10 && indexInBranch <= 12) {
            return true;
        }

        float roll = rng.NextFloat();
        return roll < _config.IntersectionChanceRPG.Probability;
    }

	private int CountNodesAhead(IsoExpeditionPath path, IsoPathNode fromNode) {
		var visited = new HashSet<PathNodeId> { fromNode.Id };
		var queue = new Queue<(IsoPathNode node, int depth)>();
		queue.Enqueue((fromNode, 0));
		int maxDepth = 0;

		while (queue.Count > 0) {
			var (node, depth) = queue.Dequeue();
			maxDepth = Math.Max(maxDepth, depth);

			foreach (var conn in node.Connections) {
				if (!visited.Contains(conn.TargetNodeId)) {
					visited.Add(conn.TargetNodeId);
					var target = path.GetNode(conn.TargetNodeId);
					
					// Only follow forward connections (higher IDs) to avoid counting the path behind us
					if (target != null && target.Id > node.Id) {
						queue.Enqueue((target, depth + 1));
					}
				}
			}
		}

		return maxDepth;
	}

    private IsoPathNode? FindNearestFrontier(IsoExpeditionPath path, IsoPathNode fromNode) {
        IsoPathNode? nearest = null;
        float nearestDist = float.MaxValue;

        IsoPos fromPos = fromNode.Position;

        foreach (var fid in _state.FrontierNodeIds) {
            var frontier = path.GetNode(fid);
            if (frontier == null) continue;

            float dist = CalculateDistance(fromPos, frontier.Position).Value;

            if (dist < nearestDist) {
                nearestDist = dist;
                nearest = frontier;
            }
        }

        return nearest;
    }

    private IsoPathNode? FindNearestNode(IsoExpeditionPath path, IsoPos pos) {
        IsoPathNode? nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var node in path.Nodes.Values) {
            float dist = CalculateDistance(pos, node.Position).Value;

            if (dist < nearestDist) {
                nearestDist = dist;
                nearest = node;
            }
        }

        return nearest;
    }

	/// <summary>
	/// Gets the last node on the main path (highest ID node that's not in a branch).
	/// With PathNodeId, we can simply find the node with the highest ID that was 
	/// created before any branches were generated.
	/// </summary>
	private IsoPathNode? GetLastMainPathNode(IsoExpeditionPath path) {
		// With monotonic IDs, we can just order by NodeId
		// The "main path" nodes are all nodes created before any branches
		// For now, just get the node with the highest ID
		return path.Nodes.Values
			.OrderByDescending(n => n.Id.Value)
			.FirstOrDefault();
	}

    private IsoPos GetLastMainPathPosition(IsoExpeditionPath path) {
        var last = GetLastMainPathNode(path);
        return last?.Position ?? IsoPos.Origin;
    }

	private void EnsureTeaserBranchesExist(IsoExpeditionPath path, IsoPathNode intersection) {
		// Check if any branch starts at this intersection
		bool hasTeaserBranches = _state.ActiveBranches.Values
			.Any(b => b.StartNodeId == intersection.Id);

		if (hasTeaserBranches) {
			return;
		}

		// Check if intersection already has forward connections (nodes with higher IDs)
		int forwardConnections = intersection.Connections
			.Count(c => {
				var target = path.GetNode(c.TargetNodeId);
				return target != null && target.Id > intersection.Id;
			});

		if (forwardConnections >= 2) {
			return;
		}

		var random = _state.GetChunkRandom();
		GenerateTeaserBranches(path, intersection, ref random);
	}

    #endregion

    #region Prototype Caching

    private void CachePrototypes() {
        _landmarkProtos = _gameDb.GetAll<LandmarkProto>()
            .Where(l => l.CanSpawnProcedurally).ToList();

        _dungeonProtos = _gameDb.GetAll<DungeonProto>()
            .Where(d => d.CanSpawnProcedurally).ToList();

        _campProtos = _gameDb.GetAll<CampProto>()
            .Where(c => c.CanSpawnProcedurally).ToList();

        _settlementProtos = _gameDb.GetAll<SettlementProto>()
            .Where(s => s.CanSpawnProcedurally).ToList();

        _bossArenaProtos = _gameDb.GetAll<BossArenaProto>()
            .Where(b => b.CanSpawnProcedurally).ToList();

        var allBiomes = _gameDb.GetAll<BiomeProto>().ToList();

        _mainPathBiomes = allBiomes
            .Where(b => b.IsMainPathBiome)
            .OrderBy(b => b.ProgressionOrder)
            .ToList();

        _branchBiomes = allBiomes
            .Where(b => b.CanAppearInBranches)
            .ToList();

        _terrainLookup = _gameDb.GetAll<TerrainProto>()
            .ToDictionary(t => t.Id, t => t);

        if (_mainPathBiomes.Count == 0) {
            CreateFallbackBiomes();
        }
    }

    private void CreateFallbackBiomes() {
        var fallbackBiome = new BiomeProto(
            id: new BiomeProto.ID("Biome_Fallback"),
            name: "Wilderness",
            description: "Untamed wilderness.",
            primaryTerrain: Ids.Terrains.Plains.Grass,
            progressionOrder: 0,
            dangerLevel: 0.3f
        ) { MinLength = 5, MaxLength = 50 };

        _mainPathBiomes = [fallbackBiome];
        _branchBiomes = [fallbackBiome];
    }

    #endregion

    #region Public Utilities

    /// <summary>Gets terrain stats for gameplay calculations.</summary>
    public (float movementSpeed, float staminaDrain, float fatigueRate) GetTerrainStats(TerrainProto.ID terrainId) {
        if (_terrainLookup.TryGetValue(terrainId, out var terrain)) {
            return (terrain.MovementSpeedMultiplier, terrain.StaminaDrainPerDistance, terrain.FatigueRateMultiplier);
        }
        return (1.0f, 1.0f, 1.0f);
    }

    /// <summary>Gets distance between two nodes using TilesRPG.</summary>
    public TilesRPG GetDistanceBetween(IsoPathNode a, IsoPathNode b) {
        return CalculateDistance(a.Position, b.Position);
    }

    /// <summary>Gets direction from one node to another as IsoRel.</summary>
    public IsoRel GetDirectionBetween(IsoPathNode from, IsoPathNode to) {
        return to.Position - from.Position;
    }

    /// <summary>Checks if two nodes are within a certain distance.</summary>
    public bool AreNodesWithinDistance(IsoPathNode a, IsoPathNode b, TilesRPG maxDistance) {
        TilesRPG distance = GetDistanceBetween(a, b);
        return distance <= maxDistance;
    }

    /// <summary>Gets world position for a node's center.</summary>
    public Vector3 GetNodeWorldCenter(IsoPathNode node, TilesRPG tileSize) {
        return node.Position.ToWorldGridCenter(tileSize, tileSize);
    }

    /// <summary>Converts world position to IsoPos.</summary>
    public IsoPos WorldToIsoPos(Vector3 worldPos, TilesRPG tileSize) {
        float size = tileSize.Value;
        int x = Mathf.FloorToInt(worldPos.x / size);
        int y = Mathf.FloorToInt(worldPos.z / size);
        int level = CalculateHeight(new IsoPos(x, y));
        return new IsoPos(x, y, new IsoLevel(level));
    }

    /// <summary>Finds nearest path node to a world position.</summary>
    public IsoPathNode? FindNearestNodeToWorld(IsoExpeditionPath path, Vector3 worldPos, TilesRPG tileSize) {
        IsoPos isoPos = WorldToIsoPos(worldPos, tileSize);
        return FindNearestNode(path, isoPos);
    }

	/// <summary>Gets current generation state for debugging.</summary>
	public GenerationState? GetCurrentState() => _state;

	/// <summary>Gets current configuration - the SINGLE SOURCE OF TRUTH for height generation.</summary>
	public Config GetCurrentConfig() => _config;

    #endregion

    #region Debug Utilities

    /// <summary>Gets debug info about current generation state.</summary>
    public string GetDebugInfo() {
        if (_state == null) return "No generation state";

        var sb = new StringBuilder();
        sb.AppendLine("=== IsoPathGenerator Debug ===");
        sb.AppendLine($"Seed: {_state.BaseSeed}");
        sb.AppendLine($"Chunk: {_state.ChunkIndex}");
        sb.AppendLine($"Total Nodes: {_state.TotalNodesGenerated}");
        sb.AppendLine($"Frontier Count: {_state.FrontierNodeIds.Count}");
        sb.AppendLine($"Active Branches: {_state.ActiveBranches.Count}");
        sb.AppendLine($"Current Biome: {_state.BiomeState.CurrentBiomeId}");
        sb.AppendLine($"Danger Level: {_state.BiomeState.CurrentDangerLevel:P0}");
        sb.AppendLine($"Height Cache Size: {_state.HeightCache.Count}");
        sb.AppendLine($"Starting Direction: {_state.StartingDirection.Degrees:F0}°");

        if (_state.ActiveBranches.Count > 0) {
            sb.AppendLine("\n--- Active Branches ---");
            foreach (var (id, branch) in _state.ActiveBranches) {
                string status = branch.IsChosen ? "CHOSEN" : (branch.IsTeaser ? "teaser" : "extended");
                sb.AppendLine($"  {id}: {status}, depth={branch.Depth}, nodes={branch.NodesGenerated}");
            }
        }

        if (_state.FrontierNodeIds.Count > 0) {
            sb.AppendLine("\n--- Frontier Nodes ---");
            foreach (var fid in _state.FrontierNodeIds.Take(5)) {
                sb.AppendLine($"  {fid}");
            }
            if (_state.FrontierNodeIds.Count > 5) {
                sb.AppendLine($"  ... and {_state.FrontierNodeIds.Count - 5} more");
            }
        }

        return sb.ToString();
    }

    /// <summary>Draws debug gizmos for the path.</summary>
    public void DrawDebugGizmos(IsoExpeditionPath path, TilesRPG tileSize) {
#if UNITY_EDITOR
        if (path == null) return;

        float size = tileSize.Value;

        foreach (var node in path.Nodes.Values) {
            Vector3 worldPos = node.Position.ToWorldGridCenter(tileSize, tileSize);

            ColorRPG nodeColor = node.Type switch {
                PathNodeType.Village => ColorRPG.HealthGreen,
                PathNodeType.Intersection => ColorRPG.StaminaYellow,
                PathNodeType.Dungeon => ColorRPG.RarityEpic,
                PathNodeType.Landmark => ColorRPG.ManaBlue,
                PathNodeType.CampSite => ColorRPG.BonfireOrange,
                PathNodeType.EventLocation => ColorRPG.Arcane,
                PathNodeType.ResourceNode => ColorRPG.Nature,
                PathNodeType.BossLocation => ColorRPG.CritRed,
                _ => ColorRPG.Silver
            };

            // Color by terrain feature
            if (node.TerrainFeature == TerrainFeature.MountainPeak) {
                nodeColor = ColorRPG.White;
            } else if (node.TerrainFeature == TerrainFeature.Cliff) {
                nodeColor = ColorRPG.FromHex("#8B4513");
            } else if (node.TerrainFeature == TerrainFeature.Hill) {
                nodeColor = ColorRPG.FromHex("#9ACD32");
            } else if (node.TerrainFeature == TerrainFeature.Valley) {
                nodeColor = ColorRPG.FromHex("#228B22");
            }

            Gizmos.color = nodeColor;
            Gizmos.DrawSphere(worldPos, 0.3f * size);

            // Draw connections
            Gizmos.color = ColorRPG.Silver.WithAlpha(0.5f);
            foreach (var conn in node.Connections) {
                var targetNode = path.GetNode(conn.TargetNodeId);
                if (targetNode != null) {
                    Vector3 targetWorld = targetNode.Position.ToWorldGridCenter(tileSize, tileSize);
                    Gizmos.DrawLine(worldPos, targetWorld);
                }
            }
        }

        // Draw frontier nodes
        if (_state != null) {
            Gizmos.color = ColorRPG.CritRed;
            foreach (var fid in _state.FrontierNodeIds) {
                var frontierNode = path.GetNode(fid);
                if (frontierNode != null) {
                    Vector3 worldPos = frontierNode.Position.ToWorldGridCenter(tileSize, tileSize);
                    Gizmos.DrawWireSphere(worldPos, 0.5f * size);
                }
            }
        }

        // Draw active branch directions
        if (_state?.ActiveBranches != null) {
            foreach (var (branchId, branchState) in _state.ActiveBranches) {
                var startNode = path.GetNode(branchState.StartNodeId);
                if (startNode != null) {
                    Vector3 startWorld = startNode.Position.ToWorldGridCenter(tileSize, tileSize);

                    AngleRPG angle = branchState.BaseAngle;
                    Vector2 dir = angle.ToDirection2D();
                    Vector3 endPoint = startWorld + new Vector3(dir.x, 0, dir.y) * 2f * size;

                    Gizmos.color = branchState.IsChosen ? ColorRPG.Gold : ColorRPG.Copper;
                    Gizmos.DrawLine(startWorld, endPoint);
                    Gizmos.DrawSphere(endPoint, 0.15f * size);
                }
            }
        }
#endif
    }

    /// <summary>Validates path integrity and reports issues.</summary>
	public List<string> ValidatePath(IsoExpeditionPath path) {
		var issues = new List<string>();

		if (path == null) {
			issues.Add("Path is null");
			return issues;
		}

		if (path.StartNode == null) {
			issues.Add("Path has no start node");
		}

		// Check for orphaned nodes
		foreach (var node in path.Nodes.Values) {
			if (node.Connections.Count == 0 && node != path.StartNode) {
				issues.Add($"Orphaned node: {node.Id}");
			}
		}

		// Check for broken connections
		foreach (var node in path.Nodes.Values) {
			foreach (var conn in node.Connections) {
				if (!path.Nodes.ContainsKey(conn.TargetNodeId)) {
					issues.Add($"Broken connection: {node.Id} -> {conn.TargetNodeId}");
				}
			}
		}

		// Check frontier validity
		if (_state != null) {
			foreach (var fid in _state.FrontierNodeIds) {
				if (!path.Nodes.ContainsKey(fid)) {
					issues.Add($"Frontier references missing node: {fid}");
				}
			}
		}

		// Check for duplicate positions
		var positions = new Dictionary<Vector3Int, List<PathNodeId>>();
		foreach (var node in path.Nodes.Values) {
			var key = node.Position.ToVector3Int();
			if (!positions.ContainsKey(key)) {
				positions[key] = [];
			}
			positions[key].Add(node.Id);
		}

		foreach (var (pos, nodeIds) in positions) {
			if (nodeIds.Count > 1) {
				issues.Add($"Overlapping nodes at {pos}: {string.Join(", ", nodeIds)}");
			}
		}

		if (issues.Count == 0) {
			Debug.Log($"IsoPathGenerator: Path validation passed ({path.NodeCount} nodes)");
		} else {
			Debug.LogWarning($"IsoPathGenerator: Path validation found {issues.Count} issues");
		}

		return issues;
	}

    #endregion
}

#region Supporting Types

/// <summary>
/// Terrain feature detected by height analysis.
/// </summary>
public enum TerrainFeature {
    Flat,
    Hill,
    Cliff,
    MountainPeak,
    Valley
}

/// <summary>
/// A path node using IsoPos for position.
/// </summary>
public class IsoPathNode {
	/// <summary>Unique monotonic ID - lower = earlier in path, higher = later.</summary>
	public PathNodeId Id { get; }
    
	public IsoPos Position { get; }
	public PathNodeType Type { get; set; }
	public TerrainProto.ID TerrainId { get; set; }
	public BiomeProto.ID BiomeId { get; set; }
	public float DangerLevel { get; set; }
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? EventId { get; set; }
	public string? LocationProtoId { get; set; }
	public TerrainFeature TerrainFeature { get; set; } = TerrainFeature.Flat;
	public List<IsoPathConnection> Connections { get; } = [];

	public IsoPathNode(PathNodeId id, IsoPos position, PathNodeType type) {
		Id = id;
		Position = position;
		Type = type;
	}

	public void AddConnection(IsoPathNode target, TilesRPG distance, TerrainProto.ID terrainId) {
		Connections.Add(new IsoPathConnection(target.Id, distance, terrainId));
		target.Connections.Add(new IsoPathConnection(Id, distance, terrainId));
	}
    
	/// <summary>Returns true if this node was created before another.</summary>
	public bool IsBefore(IsoPathNode other) => Id < other.Id;
    
	/// <summary>Returns true if this node was created after another.</summary>
	public bool IsAfter(IsoPathNode other) => Id > other.Id;
}

/// <summary>
/// A connection between path nodes using TilesRPG for distance.
/// </summary>
public class IsoPathConnection {
	public PathNodeId TargetNodeId { get; }
	public TilesRPG Distance { get; }
	public TerrainProto.ID TerrainId { get; }

	public IsoPathConnection(PathNodeId targetNodeId, TilesRPG distance, TerrainProto.ID terrainId) {
		TargetNodeId = targetNodeId;
		Distance = distance;
		TerrainId = terrainId;
	}
}

/// <summary>
/// An expedition path using IsoPos for all node positions.
/// </summary>
public class IsoExpeditionPath {
	public int Seed { get; }
	public Dictionary<PathNodeId, IsoPathNode> Nodes { get; } = [];

	private readonly Dictionary<IsoPos, IsoPathNode> NodePositions = [];

	public IsoPathNode? StartNode { get; private set; }
	public IsoPathNode? CurrentNode { get; private set; }
	public IsoPathGenerator.GenerationState? GenerationState { get; set; }

	public int NodeCount => Nodes.Count;

	public IsoExpeditionPath(int seed) {
		Seed = seed;
	}

	public void AddNode(IsoPathNode node) {
		Nodes[node.Id] = node;
		NodePositions[node.Position] = node;
	}

	public IsoPathNode? GetNode(PathNodeId id) {
		return Nodes.TryGetValue(id, out var node) ? node : null;
	}

	public IsoPathNode? GetNodeById(PathNodeId id) {
		return GetNode(id);
	}

	public IsoPathNode? GetNodeAt(IsoPos pos) {
		return NodePositions.TryGetValue(pos, out var node) ? node : null;
	}

	public void SetStartNode(IsoPathNode node) {
		StartNode = node;
		CurrentNode = node;
	}

	public void SetCurrentNode(IsoPathNode node) {
		CurrentNode = node;
	}

	public IEnumerable<IsoPathNode> GetNodesOfType(PathNodeType type) {
		return Nodes.Values.Where(n => n.Type == type);
	}

	public IEnumerable<IsoPathNode> GetNodesInRange(IsoPos center, TilesRPG range) {
		float rangeValue = range.Value;
		return Nodes.Values.Where(n => {
			int dx = Mathf.Abs(n.Position.X - center.X);
			int dy = Mathf.Abs(n.Position.Y - center.Y);
			float dist = Mathf.Sqrt(dx * dx + dy * dy);
			return dist <= rangeValue;
		});
	}

	public IEnumerable<IsoPathNode> GetNodesWithFeature(TerrainFeature feature) {
		return Nodes.Values.Where(n => n.TerrainFeature == feature);
	}
}

#endregion