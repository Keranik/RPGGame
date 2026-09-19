using RPGGame.Core.Isometric;
using RPGGame.Core.Prototypes.Village;
using UnityEngine;

namespace RPGGame.Core.Village;

/// <summary>
/// Represents the tile type in the village grid.
/// </summary>
public enum VillageTileType {
	/// <summary>Empty buildable ground.</summary>
	Empty,
	/// <summary>Occupied by a building.</summary>
	Building,
	/// <summary>Path/road tile.</summary>
	Path,
	/// <summary>Decorative tile (cannot build).</summary>
	Decoration,
	/// <summary>Water feature.</summary>
	Water,
	/// <summary>Blocked/unbuildable.</summary>
	Blocked
}

/// <summary>
/// Configuration for procedural village generation.
/// </summary>
public class VillageGenerationConfig {
	/// <summary>Size of the village grid (NxN).</summary>
	public int Size { get; init; } = VillageLayout.DEFAULT_SIZE;

	/// <summary>Seed for deterministic generation.</summary>
	public uint Seed { get; init; } = 0;

	/// <summary>Density of buildable area (affects decoration/blocked placement).</summary>
	public Percent BuildableDensity { get; init; } = 70.Percent();

	/// <summary>Chance for water features.</summary>
	public ChanceRPG WaterChance { get; init; } = 15.Chance();

	/// <summary>Plaza size around anchor (radius in tiles).</summary>
	public int PlazaRadius { get; init; } = 2;

	/// <summary>Whether to generate radial roads from center.</summary>
	public bool RadialRoads { get; init; } = true;

	/// <summary>Number of radial road spokes (if RadialRoads is true).</summary>
	public int RadialRoadCount { get; init; } = 4;

	/// <summary>Whether to generate a ring road around the village.</summary>
	public bool RingRoad { get; init; } = true;

	/// <summary>Ring road distance from edge (in tiles).</summary>
	public int RingRoadInset { get; init; } = 3;

	/// <summary>Chance for secondary connecting paths.</summary>
	public ChanceRPG SecondaryPathChance { get; init; } = 40.Chance();

	/// <summary>Maximum decoration cluster size.</summary>
	public int MaxDecorationClusterSize { get; init; } = 4;

	/// <summary>Gate placement preference.</summary>
	public GatePlacement GatePlacement { get; init; } = GatePlacement.South;

	// Presets
	public static VillageGenerationConfig Default => new();

	public static VillageGenerationConfig DenseTown => new() {
		BuildableDensity = 85.Percent(),
		WaterChance = 5.Chance(),
		PlazaRadius = 1,
		SecondaryPathChance = 60.Chance(),
		MaxDecorationClusterSize = 2
	};

	public static VillageGenerationConfig SprawlingVillage => new() {
		Size = 17,
		BuildableDensity = 55.Percent(),
		WaterChance = 25.Chance(),
		PlazaRadius = 3,
		RadialRoadCount = 6,
		SecondaryPathChance = 30.Chance(),
		MaxDecorationClusterSize = 6
	};

	public static VillageGenerationConfig Outpost => new() {
		Size = 9,
		BuildableDensity = 90.Percent(),
		WaterChance = 0.Chance(),
		PlazaRadius = 1,
		RadialRoads = false,
		RingRoad = false,
		SecondaryPathChance = 20.Chance()
	};
}

/// <summary>
/// Gate placement preference for village generation.
/// </summary>
public enum GatePlacement {
	South,
	North,
	East,
	West,
	Random,
	ClosestToOrigin
}

/// <summary>
/// Manages the village grid layout and building placement.
/// Supports procedural generation with deterministic seeding.
/// </summary>
public class VillageLayout {
	#region Constants

	public const int DEFAULT_SIZE = 13;
	private const int MAX_GENERATION_ITERATIONS = 1000;

	#endregion

	#region Fields

	private readonly int k_width;
	private readonly int k_height;
	private readonly VillageTileType[,] k_tileTypes;
	private readonly string?[,] k_buildingIds;
	private readonly Dictionary<string, Building> k_buildings = [];
	private readonly GameDb k_gameDb;

	// Generation state
	private uint k_generationSeed;
	private Vector2Int k_gatePosition;
	private Vector2Int k_anchorPosition;

	#endregion

	#region Properties

	public int Width => k_width;
	public int Height => k_height;
	public int CenterX => k_width / 2;
	public int CenterY => k_height / 2;
	public Vector2Int Center => new(CenterX, CenterY);
	public IReadOnlyCollection<Building> Buildings => k_buildings.Values;
	public Building? Anchor => GetBuildingByType(Ids.Buildings.Anchor);
	public Building? Gate => GetBuildingByType(Ids.Buildings.Gate);
	public uint GenerationSeed => k_generationSeed;
	public Vector2Int GatePosition => k_gatePosition;
	public Vector2Int AnchorPosition => k_anchorPosition;

	#endregion

	#region Events

	public event Action<Building>? OnBuildingPlaced;
	public event Action<Building>? OnBuildingRemoved;
	public event Action<Building, Vector2Int, Vector2Int>? OnBuildingMoved;
	public event Action<Building>? OnBuildingUpgraded;

	#endregion

	#region Constructors

	/// <summary>
	/// Creates a new village with default procedural generation.
	/// </summary>
	public VillageLayout(GameDb gameDb, int size = DEFAULT_SIZE)
		: this(gameDb, new VillageGenerationConfig { Size = size }) { }

	/// <summary>
	/// Creates a new village with custom generation configuration.
	/// </summary>
	public VillageLayout(GameDb gameDb, VillageGenerationConfig config) {
		k_gameDb = gameDb;
		k_width = config.Size;
		k_height = config.Size;
		k_tileTypes = new VillageTileType[k_width, k_height];
		k_buildingIds = new string?[k_width, k_height];

		GenerateProcedural(config);
	}

	/// <summary>
	/// Creates a village layout from save data.
	/// </summary>
	public VillageLayout(GameDb gameDb, VillageLayoutSaveData saveData) {
		k_gameDb = gameDb;
		k_width = saveData.Width;
		k_height = saveData.Height;
		k_tileTypes = new VillageTileType[k_width, k_height];
		k_buildingIds = new string?[k_width, k_height];
		k_generationSeed = saveData.GenerationSeed;
		k_gatePosition = saveData.GatePosition;
		k_anchorPosition = saveData.AnchorPosition;

		// Restore tile types
		for (int x = 0; x < k_width; x++) {
			for (int y = 0; y < k_height; y++) {
				int index = y * k_width + x;
				if (index < saveData.TileTypes.Count) {
					k_tileTypes[x, y] = saveData.TileTypes[index];
				}
			}
		}

		// Restore buildings
		foreach (var buildingSave in saveData.Buildings) {
			var buildingProto = GetBuildingProto(buildingSave.BuildingId);
			if (buildingProto != null) {
				var building = new Building(buildingProto, buildingSave);
				PlaceBuildingInternal(building, false);
			}
		}
	}

	#endregion

	#region Procedural Generation

	/// <summary>
	/// Generates the village procedurally based on configuration.
	/// </summary>
	private void GenerateProcedural(VillageGenerationConfig config) {
		// Initialize seed
		k_generationSeed = config.Seed != 0
			? config.Seed
			: (uint)System.DateTime.UtcNow.Ticks;

		var rng = GameRandom.FromSeed(k_generationSeed);

		// Step 1: Initialize all tiles as empty
		InitializeGrid();

		// Step 2: Determine key positions
		DetermineKeyPositions(config, ref rng);

		// Step 3: Generate plaza around anchor
		GeneratePlaza(config.PlazaRadius);

		// Step 4: Generate road network
		GenerateRoadNetwork(config, ref rng);

		// Step 5: Place decorations and terrain features
		GenerateTerrainFeatures(config, ref rng);

		// Step 6: Place core buildings
		PlaceCoreBuildings();

		Debug.Log($"VillageLayout: Generated {k_width}x{k_height} village with seed {k_generationSeed}");
	}

	/// <summary>
	/// Initializes all tiles to empty.
	/// </summary>
	private void InitializeGrid() {
		for (int x = 0; x < k_width; x++) {
			for (int y = 0; y < k_height; y++) {
				k_tileTypes[x, y] = VillageTileType.Empty;
			}
		}
	}

	/// <summary>
	/// Determines anchor and gate positions based on config.
	/// </summary>
	private void DetermineKeyPositions(VillageGenerationConfig config, ref RandomStream rng) {
		// Anchor is always at or near center
		int centerX = CenterX;
		int centerY = CenterY;

		// Add slight random offset for variety (±1 tile)
		int offsetX = rng.NextInt(-1, 2);
		int offsetY = rng.NextInt(-1, 2);
		k_anchorPosition = new Vector2Int(
			Mathf.Clamp(centerX + offsetX, 2, k_width - 3),
			Mathf.Clamp(centerY + offsetY, 2, k_height - 3)
		);

		// Determine gate position based on preference
		k_gatePosition = config.GatePlacement switch {
			GatePlacement.South => new Vector2Int(centerX, 0),
			GatePlacement.North => new Vector2Int(centerX, k_height - 1),
			GatePlacement.East => new Vector2Int(k_width - 1, centerY),
			GatePlacement.West => new Vector2Int(0, centerY),
			GatePlacement.Random => GetRandomEdgePosition(ref rng),
			GatePlacement.ClosestToOrigin => new Vector2Int(centerX, 0),
			_ => new Vector2Int(centerX, 0)
		};
	}

	/// <summary>
	/// Gets a random position on the edge of the grid.
	/// </summary>
	private Vector2Int GetRandomEdgePosition(ref RandomStream rng) {
		int side = rng.NextInt(4);
		int center = k_width / 2;

		return side switch {
			0 => new Vector2Int(center + rng.NextInt(-2, 3), 0),           // South
			1 => new Vector2Int(center + rng.NextInt(-2, 3), k_height - 1), // North
			2 => new Vector2Int(k_width - 1, center + rng.NextInt(-2, 3)), // East
			3 => new Vector2Int(0, center + rng.NextInt(-2, 3)),           // West
			_ => new Vector2Int(center, 0)
		};
	}

	/// <summary>
	/// Generates the central plaza around the anchor.
	/// </summary>
	private void GeneratePlaza(int radius) {
		int cx = k_anchorPosition.x;
		int cy = k_anchorPosition.y;

		for (int dx = -radius; dx <= radius; dx++) {
			for (int dy = -radius; dy <= radius; dy++) {
				int x = cx + dx;
				int y = cy + dy;

				if (!IsInBounds(x, y)) {
					continue;
				}

				// Use Chebyshev distance for square-ish plaza
				// Could use Manhattan for diamond, or Euclidean for circular
				float dist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
				if (dist <= radius) {
					k_tileTypes[x, y] = VillageTileType.Path;
				}
			}
		}
	}

	/// <summary>
	/// Generates the road network connecting key points.
	/// </summary>
	private void GenerateRoadNetwork(VillageGenerationConfig config, ref RandomStream rng) {
		// Always connect gate to anchor
		GeneratePath(k_gatePosition, k_anchorPosition, ref rng);

		// Radial roads from center to edges
		if (config.RadialRoads) {
			GenerateRadialRoads(config.RadialRoadCount, ref rng);
		}

		// Ring road around the village
		if (config.RingRoad) {
			GenerateRingRoad(config.RingRoadInset, ref rng);
		}

		// Secondary connecting paths
		if (rng.Check(config.SecondaryPathChance)) {
			GenerateSecondaryPaths(ref rng);
		}
	}

	/// <summary>
	/// Generates radial roads from center to edges.
	/// </summary>
	private void GenerateRadialRoads(int count, ref RandomStream rng) {
		// Start with base angles, then add small random offsets
		float angleStep = 360f / count;
		float startOffset = rng.NextFloat(0f, angleStep);

		for (int i = 0; i < count; i++) {
			float angle = startOffset + i * angleStep;
			var direction = AngleRPG.FromDegrees(angle).ToDirection2D();

			// Find edge point in this direction
			int edgeX = k_anchorPosition.x + Mathf.RoundToInt(direction.x * (k_width / 2));
			int edgeY = k_anchorPosition.y + Mathf.RoundToInt(direction.y * (k_height / 2));

			// Clamp to grid bounds
			edgeX = Mathf.Clamp(edgeX, 0, k_width - 1);
			edgeY = Mathf.Clamp(edgeY, 0, k_height - 1);

			GeneratePath(k_anchorPosition, new Vector2Int(edgeX, edgeY), ref rng);
		}
	}

	/// <summary>
	/// Generates a ring road around the village perimeter.
	/// </summary>
	private void GenerateRingRoad(int inset, ref RandomStream rng) {
		int minX = inset;
		int maxX = k_width - 1 - inset;
		int minY = inset;
		int maxY = k_height - 1 - inset;

		if (minX >= maxX || minY >= maxY) {
			return;
		}

		// Add some noise to the ring road
		float noiseStrength = 0.3f;

		// Top edge
		for (int x = minX; x <= maxX; x++) {
			int y = maxY + Mathf.RoundToInt((rng.NextFloat() * 2f - 1f) * noiseStrength * inset);
			y = Mathf.Clamp(y, minY, k_height - 1);
			SetPath(x, y);
		}

		// Bottom edge
		for (int x = minX; x <= maxX; x++) {
			int y = minY + Mathf.RoundToInt((rng.NextFloat() * 2f - 1f) * noiseStrength * inset);
			y = Mathf.Clamp(y, 0, maxY);
			SetPath(x, y);
		}

		// Left edge
		for (int y = minY; y <= maxY; y++) {
			int x = minX + Mathf.RoundToInt((rng.NextFloat() * 2f - 1f) * noiseStrength * inset);
			x = Mathf.Clamp(x, 0, maxX);
			SetPath(x, y);
		}

		// Right edge
		for (int y = minY; y <= maxY; y++) {
			int x = maxX + Mathf.RoundToInt((rng.NextFloat() * 2f - 1f) * noiseStrength * inset);
			x = Mathf.Clamp(x, minX, k_width - 1);
			SetPath(x, y);
		}

		// Connect corners
		GeneratePath(new Vector2Int(minX, minY), new Vector2Int(minX, maxY), ref rng);
		GeneratePath(new Vector2Int(minX, maxY), new Vector2Int(maxX, maxY), ref rng);
		GeneratePath(new Vector2Int(maxX, maxY), new Vector2Int(maxX, minY), ref rng);
		GeneratePath(new Vector2Int(maxX, minY), new Vector2Int(minX, minY), ref rng);
	}

	/// <summary>
	/// Generates secondary connecting paths between existing roads.
	/// </summary>
	private void GenerateSecondaryPaths(ref RandomStream rng) {
		int pathCount = rng.NextInt(2, 5);

		for (int i = 0; i < pathCount; i++) {
			// Find two random path tiles and connect them
			var pathTiles = GetTilesOfType(VillageTileType.Path).ToList();
			if (pathTiles.Count < 2) {
				break;
			}

			var start = rng.Pick(pathTiles);
			var end = rng.Pick(pathTiles);

			// Only create path if they're somewhat apart
			if (Vector2Int.Distance(start, end) > 3) {
				GeneratePath(start, end, ref rng);
			}
		}
	}

	/// <summary>
	/// Generates a winding path between two points.
	/// </summary>
	private void GeneratePath(Vector2Int start, Vector2Int end, ref RandomStream rng) {
		int x = start.x;
		int y = start.y;

		int iterations = 0;
		while ((x != end.x || y != end.y) && iterations < MAX_GENERATION_ITERATIONS) {
			iterations++;

			SetPath(x, y);

			// Determine primary direction
			int dx = Math.Sign(end.x - x);
			int dy = Math.Sign(end.y - y);

			// Add some randomness to path (30% chance to deviate)
			if (rng.Check(30.Percent()) && (dx != 0 || dy != 0)) {
				// Sometimes move perpendicular
				if (rng.NextBool()) {
					if (dx != 0 && rng.NextBool()) {
						y += rng.NextBool() ? 1 : -1;
						y = Mathf.Clamp(y, 0, k_height - 1);
						continue;
					}
					if (dy != 0 && rng.NextBool()) {
						x += rng.NextBool() ? 1 : -1;
						x = Mathf.Clamp(x, 0, k_width - 1);
						continue;
					}
				}
			}

			// Move toward target
			if (dx != 0 && dy != 0) {
				// Diagonal movement: prefer one axis randomly
				if (rng.NextBool()) {
					x += dx;
				} else {
					y += dy;
				}
			} else if (dx != 0) {
				x += dx;
			} else if (dy != 0) {
				y += dy;
			}
		}

		SetPath(end.x, end.y);
	}

	/// <summary>
	/// Sets a tile as a path if it's not already a building.
	/// </summary>
	private void SetPath(int x, int y) {
		if (!IsInBounds(x, y)) {
			return;
		}
		if (k_tileTypes[x, y] == VillageTileType.Building) {
			return;
		}
		k_tileTypes[x, y] = VillageTileType.Path;
	}

	/// <summary>
	/// Generates terrain features (decorations, water, blocked areas).
	/// </summary>
	private void GenerateTerrainFeatures(VillageGenerationConfig config, ref RandomStream rng) {
		// Calculate how many tiles should be non-buildable
		int totalTiles = k_width * k_height;
		int pathTiles = GetTilesOfType(VillageTileType.Path).Count();
		int targetBuildable = (int)(totalTiles * config.BuildableDensity.Fraction);
		int targetDecorations = totalTiles - pathTiles - targetBuildable;

		int decorationsPlaced = 0;
		int attempts = 0;

		while (decorationsPlaced < targetDecorations && attempts < MAX_GENERATION_ITERATIONS) {
			attempts++;

			// Pick a random empty tile
			int x = rng.NextInt(k_width);
			int y = rng.NextInt(k_height);

			if (k_tileTypes[x, y] != VillageTileType.Empty) {
				continue;
			}

			// Don't place too close to anchor or gate
			if (Vector2Int.Distance(new Vector2Int(x, y), k_anchorPosition) < 3) {
				continue;
			}
			if (Vector2Int.Distance(new Vector2Int(x, y), k_gatePosition) < 2) {
				continue;
			}

			// Decide what to place
			VillageTileType feature;
			if (rng.Check(config.WaterChance)) {
				feature = VillageTileType.Water;
			} else if (rng.Check(50.Percent())) {
				feature = VillageTileType.Decoration;
			} else {
				feature = VillageTileType.Blocked;
			}

			// Place as a small cluster
			int clusterSize = rng.NextInt(1, config.MaxDecorationClusterSize + 1);
			decorationsPlaced += PlaceCluster(x, y, feature, clusterSize, ref rng);
		}
	}

	/// <summary>
	/// Places a cluster of tiles around a center point.
	/// </summary>
	private int PlaceCluster(int cx, int cy, VillageTileType type, int size, ref RandomStream rng) {
		int placed = 0;
		var toPlace = new Queue<Vector2Int>();
		var visited = new HashSet<Vector2Int>();

		toPlace.Enqueue(new Vector2Int(cx, cy));

		while (toPlace.Count > 0 && placed < size) {
			var pos = toPlace.Dequeue();
			if (visited.Contains(pos)) {
				continue;
			}
			visited.Add(pos);

			if (!IsInBounds(pos.x, pos.y)) {
				continue;
			}
			if (k_tileTypes[pos.x, pos.y] != VillageTileType.Empty) {
				continue;
			}

			// Don't block paths completely - ensure connectivity
			if (WouldBlockPath(pos.x, pos.y)) {
				continue;
			}

			k_tileTypes[pos.x, pos.y] = type;
			placed++;

			// Add neighbors with decreasing probability
			foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }) {
				if (rng.Check(60.Percent())) {
					toPlace.Enqueue(pos + dir);
				}
			}
		}

		return placed;
	}

	/// <summary>
	/// Checks if placing a non-traversable tile would block path connectivity.
	/// </summary>
	private bool WouldBlockPath(int x, int y) {
		// Count adjacent path tiles
		int adjacentPaths = 0;
		foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }) {
			int nx = x + dir.x;
			int ny = y + dir.y;
			if (IsInBounds(nx, ny) && k_tileTypes[nx, ny] == VillageTileType.Path) {
				adjacentPaths++;
			}
		}

		// Don't place if it's adjacent to 2+ paths (might be a chokepoint)
		return adjacentPaths >= 2;
	}

	/// <summary>
	/// Places the core buildings (Anchor and Gate).
	/// </summary>
	private void PlaceCoreBuildings() {
		// Place The Anchor at determined position
		var anchorProto = GetBuildingProto(Ids.Buildings.Anchor);
		if (anchorProto != null) {
			// Clear the tile for building
			k_tileTypes[k_anchorPosition.x, k_anchorPosition.y] = VillageTileType.Empty;
			var anchor = new Building(anchorProto, k_anchorPosition);
			PlaceBuildingInternal(anchor, false);
		}

		// Place Gate at determined position
		var gateProto = GetBuildingProto(Ids.Buildings.Gate);
		if (gateProto != null) {
			k_tileTypes[k_gatePosition.x, k_gatePosition.y] = VillageTileType.Empty;
			var gate = new Building(gateProto, k_gatePosition);
			PlaceBuildingInternal(gate, false);
		}
	}

	/// <summary>
	/// Sets up the default starter buildings.
	/// Uses RNG for varied placement each run.
	/// </summary>
	public void PlaceStarterBuildings() {
		var rng = GameRandom.FromSeed(k_generationSeed).Fork("StarterBuildings");

		// Get valid placement tiles (empty, adjacent to path)
		var validTiles = GetValidStarterBuildingTiles().ToList();
		rng.Shuffle(validTiles);

		// Define starter buildings with placement preferences
		var starterBuildings = new[] {
			(Id: Ids.Buildings.Blacksmith, PreferredDistance: 3f),
			(Id: Ids.Buildings.GeneralStore, PreferredDistance: 3f),
			(Id: Ids.Buildings.Tavern, PreferredDistance: 4f),
			(Id: Ids.Buildings.Carpenter, PreferredDistance: 2f),
		};

		foreach (var (buildingId, preferredDist) in starterBuildings) {
			var proto = GetBuildingProto(buildingId);
			if (proto == null) {
				continue;
			}

			// Find best tile for this building
			Vector2Int? bestTile = null;
			float bestScore = float.MinValue;

			foreach (var tile in validTiles) {
				if (!CanPlaceBuilding(proto, tile)) {
					continue;
				}

				// Score based on distance from anchor (prefer preferred distance)
				float distFromAnchor = Vector2Int.Distance(tile, k_anchorPosition);
				float distScore = 1f - Mathf.Abs(distFromAnchor - preferredDist) / 10f;

				// Bonus for being adjacent to path
				int pathAdjacency = CountAdjacentOfType(tile, VillageTileType.Path);
				float pathScore = pathAdjacency * 0.2f;

				// Penalty for being too close to other buildings
				float crowdingPenalty = 0f;
				foreach (var building in k_buildings.Values) {
					float dist = Vector2Int.Distance(tile, building.Position);
					if (dist < 2) {
						crowdingPenalty += 0.5f;
					}
				}

				float totalScore = distScore + pathScore - crowdingPenalty + rng.NextFloat() * 0.3f;

				if (totalScore > bestScore) {
					bestScore = totalScore;
					bestTile = tile;
				}
			}

			if (bestTile.HasValue) {
				var building = new Building(proto, bestTile.Value);
				PlaceBuildingInternal(building, true);
				validTiles.Remove(bestTile.Value);
			}
		}

		// Place villager homes in remaining corners
		var homeProto = GetBuildingProto(Ids.Buildings.Home);
		if (homeProto != null) {
			int homesPlaced = 0;
			int targetHomes = 4;

			// Prefer corner areas
			var cornerPreferences = new[] {
				new Vector2Int(2, k_height - 3),  // NW
				new Vector2Int(k_width - 3, k_height - 3), // NE
				new Vector2Int(2, 2), // SW
				new Vector2Int(k_width - 3, 2) // SE
			};

			foreach (var corner in cornerPreferences) {
				if (homesPlaced >= targetHomes) {
					break;
				}

				// Find closest valid tile to corner
				var nearbyTiles = validTiles
					.Where(t => Vector2Int.Distance(t, corner) < 4)
					.OrderBy(t => Vector2Int.Distance(t, corner))
					.ToList();

				foreach (var tile in nearbyTiles) {
					if (!CanPlaceBuilding(homeProto, tile)) {
						continue;
					}

					var building = new Building(homeProto, tile);
					PlaceBuildingInternal(building, true);
					validTiles.Remove(tile);
					homesPlaced++;
					break;
				}
			}
		}

		Debug.Log($"VillageLayout: Placed {k_buildings.Count} starter buildings");
	}

	/// <summary>
	/// Gets tiles valid for placing starter buildings.
	/// </summary>
	private IEnumerable<Vector2Int> GetValidStarterBuildingTiles() {
		for (int x = 0; x < k_width; x++) {
			for (int y = 0; y < k_height; y++) {
				if (k_tileTypes[x, y] != VillageTileType.Empty) {
					continue;
				}

				// Must be adjacent to a path
				if (CountAdjacentOfType(new Vector2Int(x, y), VillageTileType.Path) == 0) {
					continue;
				}

				// Not too close to anchor (leave plaza space)
				if (Vector2Int.Distance(new Vector2Int(x, y), k_anchorPosition) < 2) {
					continue;
				}

				yield return new Vector2Int(x, y);
			}
		}
	}

	/// <summary>
	/// Counts adjacent tiles of a specific type.
	/// </summary>
	private int CountAdjacentOfType(Vector2Int pos, VillageTileType type) {
		int count = 0;
		foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }) {
			int nx = pos.x + dir.x;
			int ny = pos.y + dir.y;
			if (IsInBounds(nx, ny) && k_tileTypes[nx, ny] == type) {
				count++;
			}
		}
		return count;
	}

	/// <summary>
	/// Gets all tiles of a specific type.
	/// </summary>
	private IEnumerable<Vector2Int> GetTilesOfType(VillageTileType type) {
		for (int x = 0; x < k_width; x++) {
			for (int y = 0; y < k_height; y++) {
				if (k_tileTypes[x, y] == type) {
					yield return new Vector2Int(x, y);
				}
			}
		}
	}

	/// <summary>
	/// Regenerates the village with a new seed.
	/// Clears all buildings and terrain, then generates fresh.
	/// </summary>
	public void Regenerate(uint? newSeed = null) {
		// Clear existing state
		k_buildings.Clear();
		for (int x = 0; x < k_width; x++) {
			for (int y = 0; y < k_height; y++) {
				k_buildingIds[x, y] = null;
			}
		}

		// Generate with new or random seed
		var config = new VillageGenerationConfig {
			Size = k_width,
			Seed = newSeed ?? (uint)System.DateTime.UtcNow.Ticks
		};

		GenerateProcedural(config);
	}

	#endregion

	#region Proto Access

	private BuildingProto? GetBuildingProto(BuildingProto.ID id) {
		if (k_gameDb.TryGetProto<BuildingProto>(id, out var proto)) {
			return proto;
		}
		return null;
	}

	private BuildingProto? GetBuildingProto(string id) {
		if (k_gameDb.TryGetProto<BuildingProto>(new BuildingProto.ID(id), out var proto)) {
			return proto;
		}
		return null;
	}

	#endregion

	#region Tile Access

	public VillageTileType GetTileType(int x, int y) {
		if (!IsInBounds(x, y)) {
			return VillageTileType.Blocked;
		}
		return k_tileTypes[x, y];
	}

	public VillageTileType GetTileType(Vector2Int pos) => GetTileType(pos.x, pos.y);

	public bool IsInBounds(int x, int y) {
		return x >= 0 && x < k_width && y >= 0 && y < k_height;
	}

	public bool IsInBounds(Vector2Int pos) => IsInBounds(pos.x, pos.y);

	public Building? GetBuildingAt(int x, int y) {
		if (!IsInBounds(x, y)) {
			return null;
		}
		var instanceId = k_buildingIds[x, y];
		if (instanceId == null) {
			return null;
		}
		return k_buildings.GetValueOrDefault(instanceId);
	}

	public Building? GetBuildingAt(Vector2Int pos) => GetBuildingAt(pos.x, pos.y);

	public Building? GetBuildingById(string instanceId) {
		return k_buildings.GetValueOrDefault(instanceId);
	}

	public Building? GetBuildingByType(BuildingProto.ID buildingTypeId) {
		return k_buildings.Values.FirstOrDefault(b => b.Proto.Id == buildingTypeId);
	}

	public Building? GetBuildingByType(string buildingTypeId) {
		return k_buildings.Values.FirstOrDefault(b => b.Proto.Id.Value == buildingTypeId);
	}

	public IEnumerable<Building> GetBuildingsByType(BuildingProto.ID buildingTypeId) {
		return k_buildings.Values.Where(b => b.Proto.Id == buildingTypeId);
	}

	public IEnumerable<Building> GetBuildingsByType(string buildingTypeId) {
		return k_buildings.Values.Where(b => b.Proto.Id.Value == buildingTypeId);
	}

	public IEnumerable<Building> GetBuildingsByCategory(BuildingCategory category) {
		return k_buildings.Values.Where(b => b.Proto.Category == category);
	}

	#endregion

	#region Building Placement

	public bool CanPlaceBuilding(BuildingProto proto, Vector2Int position) {
		for (int x = 0; x < proto.Width; x++) {
			for (int y = 0; y < proto.Height; y++) {
				int tileX = position.x + x;
				int tileY = position.y + y;

				if (!IsInBounds(tileX, tileY)) {
					return false;
				}

				var tileType = k_tileTypes[tileX, tileY];
				if (tileType == VillageTileType.Building ||
					tileType == VillageTileType.Water ||
					tileType == VillageTileType.Blocked) {
					return false;
				}

				if (k_buildingIds[tileX, tileY] != null) {
					return false;
				}
			}
		}

		return true;
	}

	public Building? PlaceBuilding(BuildingProto.ID buildingTypeId, Vector2Int position) {
		var proto = GetBuildingProto(buildingTypeId);
		if (proto == null) {
			Debug.LogError($"Building type not found: {buildingTypeId}");
			return null;
		}

		if (!CanPlaceBuilding(proto, position)) {
			Debug.LogWarning($"Cannot place {buildingTypeId} at {position}");
			return null;
		}

		var building = new Building(proto, position);
		PlaceBuildingInternal(building, true);
		return building;
	}

	public Building? PlaceBuilding(string buildingTypeId, Vector2Int position) {
		return PlaceBuilding(new BuildingProto.ID(buildingTypeId), position);
	}

	private void PlaceBuildingInternal(Building building, bool fireEvent) {
		k_buildings[building.InstanceId] = building;

		foreach (var tile in building.OccupiedTiles) {
			if (IsInBounds(tile)) {
				k_tileTypes[tile.x, tile.y] = VillageTileType.Building;
				k_buildingIds[tile.x, tile.y] = building.InstanceId;
			}
		}

		if (fireEvent) {
			OnBuildingPlaced?.Invoke(building);
		}

		Debug.Log($"Placed {building.DisplayName} at {building.Position}");
	}

	public bool RemoveBuilding(string instanceId) {
		if (!k_buildings.TryGetValue(instanceId, out var building)) {
			return false;
		}

		if (!building.CanDemolish) {
			Debug.LogWarning($"Cannot demolish {building.DisplayName}");
			return false;
		}

		foreach (var tile in building.OccupiedTiles) {
			if (IsInBounds(tile)) {
				k_tileTypes[tile.x, tile.y] = VillageTileType.Empty;
				k_buildingIds[tile.x, tile.y] = null;
			}
		}

		k_buildings.Remove(instanceId);
		OnBuildingRemoved?.Invoke(building);

		Debug.Log($"Removed {building.DisplayName}");
		return true;
	}

	public bool MoveBuilding(string instanceId, Vector2Int newPosition) {
		if (!k_buildings.TryGetValue(instanceId, out var building)) {
			return false;
		}

		if (!building.CanMove) {
			Debug.LogWarning($"Cannot move {building.DisplayName}");
			return false;
		}

		foreach (var tile in building.OccupiedTiles) {
			if (IsInBounds(tile)) {
				k_tileTypes[tile.x, tile.y] = VillageTileType.Empty;
				k_buildingIds[tile.x, tile.y] = null;
			}
		}

		if (!CanPlaceBuilding(building.Proto, newPosition)) {
			foreach (var tile in building.OccupiedTiles) {
				if (IsInBounds(tile)) {
					k_tileTypes[tile.x, tile.y] = VillageTileType.Building;
					k_buildingIds[tile.x, tile.y] = building.InstanceId;
				}
			}
			return false;
		}

		var oldPosition = building.Position;
		building.MoveTo(newPosition);

		foreach (var tile in building.OccupiedTiles) {
			if (IsInBounds(tile)) {
				k_tileTypes[tile.x, tile.y] = VillageTileType.Building;
				k_buildingIds[tile.x, tile.y] = building.InstanceId;
			}
		}

		OnBuildingMoved?.Invoke(building, oldPosition, newPosition);
		Debug.Log($"Moved {building.DisplayName} from {oldPosition} to {newPosition}");
		return true;
	}

	public bool UpgradeBuilding(string instanceId) {
		if (!k_buildings.TryGetValue(instanceId, out var building)) {
			return false;
		}

		if (!building.CanUpgrade) {
			return false;
		}

		building.Upgrade();
		OnBuildingUpgraded?.Invoke(building);
		Debug.Log($"Upgraded {building.DisplayName} to level {building.Level}");
		return true;
	}

	#endregion

	#region Adjacency

	public IEnumerable<Building> GetAdjacentBuildings(Building building) {
		var adjacent = new HashSet<Building>();

		foreach (var tile in building.OccupiedTiles) {
			var neighbors = new Vector2Int[] {
				new(tile.x - 1, tile.y),
				new(tile.x + 1, tile.y),
				new(tile.x, tile.y - 1),
				new(tile.x, tile.y + 1)
			};

			foreach (var neighbor in neighbors) {
				var adjacentBuilding = GetBuildingAt(neighbor);
				if (adjacentBuilding != null && adjacentBuilding.InstanceId != building.InstanceId) {
					adjacent.Add(adjacentBuilding);
				}
			}
		}

		return adjacent;
	}

	public Dictionary<string, List<AdjacencyBonus>> CalculateAllAdjacencyBonuses() {
		var result = new Dictionary<string, List<AdjacencyBonus>>();

		foreach (var building in k_buildings.Values) {
			var adjacent = GetAdjacentBuildings(building);
			var bonuses = building.GetActiveAdjacencyBonuses(adjacent).ToList();
			if (bonuses.Count > 0) {
				result[building.InstanceId] = bonuses;
			}
		}

		return result;
	}

	#endregion

	#region Queries

	public IEnumerable<Vector2Int> GetValidPlacementTiles(BuildingProto proto) {
		for (int x = 0; x <= k_width - proto.Width; x++) {
			for (int y = 0; y <= k_height - proto.Height; y++) {
				var pos = new Vector2Int(x, y);
				if (CanPlaceBuilding(proto, pos)) {
					yield return pos;
				}
			}
		}
	}

	public IEnumerable<Vector2Int> GetEmptyTiles() {
		for (int x = 0; x < k_width; x++) {
			for (int y = 0; y < k_height; y++) {
				if (k_tileTypes[x, y] == VillageTileType.Empty) {
					yield return new Vector2Int(x, y);
				}
			}
		}
	}

	public VillageStats GetStats() {
		int totalTiles = k_width * k_height;
		int buildingTiles = 0;
		int pathTiles = 0;
		int emptyTiles = 0;
		int decorationTiles = 0;
		int waterTiles = 0;
		int blockedTiles = 0;

		for (int x = 0; x < k_width; x++) {
			for (int y = 0; y < k_height; y++) {
				switch (k_tileTypes[x, y]) {
					case VillageTileType.Building: buildingTiles++; break;
					case VillageTileType.Path: pathTiles++; break;
					case VillageTileType.Empty: emptyTiles++; break;
					case VillageTileType.Decoration: decorationTiles++; break;
					case VillageTileType.Water: waterTiles++; break;
					case VillageTileType.Blocked: blockedTiles++; break;
				}
			}
		}

		return new VillageStats {
			TotalTiles = totalTiles,
			BuildingTiles = buildingTiles,
			PathTiles = pathTiles,
			EmptyTiles = emptyTiles,
			DecorationTiles = decorationTiles,
			WaterTiles = waterTiles,
			BlockedTiles = blockedTiles,
			BuildingCount = k_buildings.Count,
			TotalBuildingLevels = k_buildings.Values.Sum(b => b.Level)
		};
	}

	#endregion

	#region Serialization

	public VillageLayoutSaveData ToSaveData() {
		var tileTypes = new List<VillageTileType>();
		for (int y = 0; y < k_height; y++) {
			for (int x = 0; x < k_width; x++) {
				tileTypes.Add(k_tileTypes[x, y]);
			}
		}

		return new VillageLayoutSaveData {
			Width = k_width,
			Height = k_height,
			GenerationSeed = k_generationSeed,
			GatePosition = k_gatePosition,
			AnchorPosition = k_anchorPosition,
			TileTypes = tileTypes,
			Buildings = k_buildings.Values.Select(b => b.ToSaveData()).ToList()
		};
	}

	#endregion

	#region Debug

	/// <summary>
	/// Returns a debug ASCII representation of the village.
	/// </summary>
	public string ToDebugString() {
		var sb = new System.Text.StringBuilder();
		sb.AppendLine($"Village {k_width}x{k_height} (Seed: {k_generationSeed})");

		for (int y = k_height - 1; y >= 0; y--) {
			for (int x = 0; x < k_width; x++) {
				char c = k_tileTypes[x, y] switch {
					VillageTileType.Empty => '.',
					VillageTileType.Building => 'B',
					VillageTileType.Path => '#',
					VillageTileType.Decoration => '*',
					VillageTileType.Water => '~',
					VillageTileType.Blocked => 'X',
					_ => '?'
				};

				// Special markers
				if (x == k_anchorPosition.x && y == k_anchorPosition.y) {
					c = 'A';
				}
				if (x == k_gatePosition.x && y == k_gatePosition.y) {
					c = 'G';
				}

				sb.Append(c);
			}
			sb.AppendLine();
		}

		return sb.ToString();
	}

	#endregion
}

#region Supporting Types

public class VillageStats {
	public int TotalTiles { get; set; }
	public int BuildingTiles { get; set; }
	public int PathTiles { get; set; }
	public int EmptyTiles { get; set; }
	public int DecorationTiles { get; set; }
	public int WaterTiles { get; set; }
	public int BlockedTiles { get; set; }
	public int BuildingCount { get; set; }
	public int TotalBuildingLevels { get; set; }

	public float OccupancyPercent => TotalTiles > 0 ? (float)BuildingTiles / TotalTiles * 100 : 0;
	public float PathCoverage => TotalTiles > 0 ? (float)PathTiles / TotalTiles * 100 : 0;
	public float BuildableTiles => EmptyTiles + PathTiles;
}

public class VillageLayoutSaveData {
	public int Width { get; set; }
	public int Height { get; set; }
	public uint GenerationSeed { get; set; }
	public Vector2Int GatePosition { get; set; }
	public Vector2Int AnchorPosition { get; set; }
	public List<VillageTileType> TileTypes { get; set; } = [];
	public List<BuildingSaveData> Buildings { get; set; } = [];
}

#endregion