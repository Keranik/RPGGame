#define USE_ISO_RENDERER
// Comment out the line above to fall back to old tilemap rendering

using RPGGame.Core.Isometric;
using RPGGame.Core.Prototypes.Village;
using RPGGame.Core.Village;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RPGGame.Core.Maps;

/// <summary>
/// Renders the village to tilemaps or isometric debug visualization.
/// 
/// <para>
/// Supports dual-mode rendering controlled by the USE_ISO_RENDERER define:
/// - When defined: Uses new isometric coordinate system with debug visualization
/// - When not defined: Uses traditional Unity Tilemaps via TilemapManager
/// </para>
/// 
/// <example>
/// <code>
/// // To switch rendering modes, edit the #define at the top of this file:
/// #define USE_ISO_RENDERER    // Isometric mode (new)
/// // #define USE_ISO_RENDERER // Tilemap mode (old) - commented out
/// </code>
/// </example>
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class VillageTilemapRenderer {
    #region Constants

#if USE_ISO_RENDERER
    /// <summary>Default height in levels for standard buildings.</summary>
    private const int DEFAULT_BUILDING_HEIGHT = 1;

    /// <summary>Height multiplier for tall buildings (Core, Defense).</summary>
    private const int TALL_BUILDING_HEIGHT = 2;

    /// <summary>Height for special buildings (Anchor, Gate).</summary>
    private const int SPECIAL_BUILDING_HEIGHT = 3;

    /// <summary>Duration in seconds for debug lines to persist.</summary>
    private const float DEBUG_LINE_DURATION = 0f; // 0 = single frame

    /// <summary>Size of debug cubes for tile visualization.</summary>
    private const float DEBUG_CUBE_SIZE = 0.9f;
#endif

    #endregion

    #region Fields

    private readonly VillageManager k_villageManager;
    private readonly TilemapManager k_tilemapManager;
    private readonly GameDb k_gameDb;

#if !USE_ISO_RENDERER
    // Tile cache - only needed for tilemap rendering
    private readonly Dictionary<string, Tile> k_tileCache = new();
#endif

#if USE_ISO_RENDERER
    // Isometric rendering state
    private readonly Dictionary<Building, IsoVolume> k_buildingVolumes = new();
    private bool k_showDebugGrid = true;
    private bool k_showDebugBuildings = true;
    private bool k_showDebugFog = true;
    private IsoPos? k_hoveredTile;
    private IsoPos? k_selectedTile;
#endif

    // Colors
    private static readonly Color k_emptyPlotColor = new(0.4f, 0.6f, 0.3f, 1f);
    private static readonly Color k_pathColor = new(0.6f, 0.5f, 0.4f, 1f);
    private static readonly Color k_grassColor = new(0.3f, 0.5f, 0.2f, 1f);
    private static readonly Color k_fogColor = new(0.3f, 0.3f, 0.4f, 0.9f);
    private static readonly Color k_wallColor = new(0.5f, 0.45f, 0.4f, 1f);

    private static readonly Dictionary<BuildingCategory, Color> k_buildingColors = new() {
        { BuildingCategory.Core, new Color(0.9f, 0.8f, 0.3f, 1f) },
        { BuildingCategory.Commerce, new Color(0.3f, 0.6f, 0.9f, 1f) },
        { BuildingCategory.Production, new Color(0.8f, 0.4f, 0.2f, 1f) },
        { BuildingCategory.Training, new Color(0.7f, 0.3f, 0.3f, 1f) },
        { BuildingCategory.Defense, new Color(0.5f, 0.5f, 0.5f, 1f) },
        { BuildingCategory.Utility, new Color(0.5f, 0.7f, 0.5f, 1f) },
    };

#if USE_ISO_RENDERER
    // Additional iso-specific colors
    private static readonly Color k_gridLineColor = new(0.5f, 0.5f, 0.5f, 0.3f);
    private static readonly Color k_selectedTileColor = new(1f, 1f, 0f, 0.5f);
    private static readonly Color k_hoveredTileColor = new(1f, 1f, 1f, 0.3f);
    private static readonly Color k_buildingOutlineColor = new(0.2f, 0.2f, 0.2f, 1f);
#endif

    private bool k_isInitialized;

    #endregion

    #region Properties

    /// <summary>Whether the renderer has been initialized.</summary>
    public bool IsInitialized => k_isInitialized;

#if !USE_ISO_RENDERER
    /// <summary>The ground tilemap (tilemap mode only).</summary>
    public Tilemap? GroundTilemap => k_tilemapManager.VillageGroundTilemap;

    /// <summary>The building tilemap (tilemap mode only).</summary>
    public Tilemap? BuildingTilemap => k_tilemapManager.VillageBuildingTilemap;
#endif

#if USE_ISO_RENDERER
    /// <summary>Whether to show the debug grid overlay.</summary>
    public bool ShowDebugGrid {
        get => k_showDebugGrid;
        set => k_showDebugGrid = value;
    }

    /// <summary>Whether to show debug building volumes.</summary>
    public bool ShowDebugBuildings {
        get => k_showDebugBuildings;
        set => k_showDebugBuildings = value;
    }

    /// <summary>Whether to show debug fog boundary.</summary>
    public bool ShowDebugFog {
        get => k_showDebugFog;
        set => k_showDebugFog = value;
    }

    /// <summary>The currently hovered tile position (iso mode).</summary>
    public IsoPos? HoveredTile => k_hoveredTile;

    /// <summary>The currently selected tile position (iso mode).</summary>
    public IsoPos? SelectedTile => k_selectedTile;

    /// <summary>Whether we're using isometric rendering.</summary>
    public static bool IsIsometricMode => true;
#else
    /// <summary>Whether we're using isometric rendering.</summary>
    public static bool IsIsometricMode => false;
#endif

    #endregion

    #region Events

    /// <summary>Fired when any tile is clicked.</summary>
    public event Action<Vector2Int>? OnTileClicked;

    /// <summary>Fired when a building is clicked.</summary>
    public event Action<Building>? OnBuildingClicked;

    /// <summary>Fired when an empty plot is clicked.</summary>
    public event Action<Vector2Int>? OnEmptyPlotClicked;

    /// <summary>Fired when the gate is clicked.</summary>
    public event Action? OnGateClicked;

    /// <summary>Fired when the anchor is clicked.</summary>
    public event Action? OnAnchorClicked;

#if USE_ISO_RENDERER
    /// <summary>Fired when a tile is hovered in iso mode.</summary>
    public event Action<IsoPos>? OnTileHovered;

    /// <summary>Fired when hover leaves all tiles in iso mode.</summary>
    public event Action? OnTileHoverExit;
#endif

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new VillageTilemapRenderer.
    /// </summary>
    public VillageTilemapRenderer(VillageManager villageManager, TilemapManager tilemapManager, GameDb gameDb) {
        k_villageManager = villageManager;
        k_tilemapManager = tilemapManager;
        k_gameDb = gameDb;

#if USE_ISO_RENDERER
        Debug.Log("VillageTilemapRenderer: Created (ISOMETRIC MODE)");
#else
        Debug.Log("VillageTilemapRenderer: Created (TILEMAP MODE)");
#endif
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the renderer and subscribes to village events.
    /// </summary>
    public void Initialize() {
        if (k_isInitialized) return;

#if !USE_ISO_RENDERER
        CreateTiles();
#endif

#if USE_ISO_RENDERER
        // Pre-calculate building volumes
        if (k_villageManager.Layout != null) {
            foreach (var building in k_villageManager.Layout.Buildings) {
                k_buildingVolumes[building] = CreateBuildingVolume(building);
            }
        }
#endif

        // Subscribe to village events
        if (k_villageManager.Layout != null) {
            k_villageManager.Layout.OnBuildingPlaced += OnBuildingPlaced;
            k_villageManager.Layout.OnBuildingRemoved += OnBuildingRemoved;
            k_villageManager.Layout.OnBuildingMoved += OnBuildingMoved;
            k_villageManager.Layout.OnBuildingUpgraded += OnBuildingUpgraded;
        }

        k_isInitialized = true;

#if USE_ISO_RENDERER
        Debug.Log("VillageTilemapRenderer: Initialized (ISOMETRIC MODE)");
#else
        Debug.Log("VillageTilemapRenderer: Initialized (TILEMAP MODE)");
#endif
    }

#if !USE_ISO_RENDERER
    private void CreateTiles() {
        // Ground tiles
        CreateTile("empty", k_emptyPlotColor);
        CreateTile("path", k_pathColor);
        CreateTile("grass", k_grassColor);
        CreateTile("fog", k_fogColor);
        CreateTile("wall", k_wallColor);

        // Building tiles
        foreach (var buildingId in Ids.Buildings.AllBuildings) {
            if (k_gameDb.TryGetProto<BuildingProto>(buildingId, out var proto)) {
                CreateTile($"building_{buildingId.Value}", GetBuildingColor(proto));
            }
        }

        Debug.Log($"VillageTilemapRenderer: Created {k_tileCache.Count} tiles");
    }

    private void CreateTile(string key, Color color) {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = k_tilemapManager.CreateIsometricTileSprite(color);
        tile.color = Color.white;
        k_tileCache[key] = tile;
    }
#endif

    private Color GetBuildingColor(BuildingProto proto) {
        if (proto.Id == Ids.Buildings.Anchor) return new Color(1f, 0.85f, 0.2f, 1f);
        if (proto.Id == Ids.Buildings.Gate) return new Color(0.6f, 0.4f, 0.2f, 1f);
        return k_buildingColors.GetValueOrDefault(proto.Category, Color.magenta);
    }

#if USE_ISO_RENDERER
    /// <summary>
    /// Creates an IsoVolume for a building based on its properties.
    /// </summary>
    private IsoVolume CreateBuildingVolume(Building building) {
        var origin = new IsoPos(building.Position.x, building.Position.y, IsoLevel.Ground);
        int height = GetBuildingHeight(building);

        return new IsoVolume(origin, building.Proto.Width, building.Proto.Height, height);
    }

    /// <summary>
    /// Gets the height in levels for a building based on its category.
    /// </summary>
    private int GetBuildingHeight(Building building) {
        // Special buildings are tallest
        if (building.Proto.Id == Ids.Buildings.Anchor) return SPECIAL_BUILDING_HEIGHT;
        if (building.Proto.Id == Ids.Buildings.Gate) return SPECIAL_BUILDING_HEIGHT;

        // Category-based heights
        return building.Proto.Category switch {
            BuildingCategory.Core => TALL_BUILDING_HEIGHT,
            BuildingCategory.Defense => TALL_BUILDING_HEIGHT,
            _ => DEFAULT_BUILDING_HEIGHT
        };
    }
#endif

    #endregion

    #region Rendering

    /// <summary>
    /// Renders the entire village.
    /// </summary>
    public void RenderVillage() {
        if (!k_isInitialized) Initialize();

        var layout = k_villageManager.Layout;
        if (layout == null) {
            Debug.LogWarning("VillageTilemapRenderer: No layout to render");
            return;
        }

#if USE_ISO_RENDERER
        Debug.Log($"VillageTilemapRenderer: Rendering {layout.Width}x{layout.Height} village (ISOMETRIC)");

        // Rebuild building volumes
        k_buildingVolumes.Clear();
        foreach (var building in layout.Buildings) {
            k_buildingVolumes[building] = CreateBuildingVolume(building);
        }

        // Initial render is handled by Update/OnDrawGizmos
        // Here we just prepare the data
#else
        Debug.Log($"VillageTilemapRenderer: Rendering {layout.Width}x{layout.Height} village (TILEMAP)");

        ClearAllTiles();
        RenderGroundLayer(layout);
        RenderBuildingLayer(layout);
        RenderFogBoundary(layout);

        k_tilemapManager.CenterCameraOnVillage(layout);
#endif

        Debug.Log("VillageTilemapRenderer: Render complete");
    }

#if !USE_ISO_RENDERER
    private void ClearAllTiles() {
        k_tilemapManager.VillageGroundTilemap?.ClearAllTiles();
        k_tilemapManager.VillageBuildingTilemap?.ClearAllTiles();
        k_tilemapManager.VillageFogTilemap?.ClearAllTiles();
    }

    private void RenderGroundLayer(VillageLayout layout) {
        var tilemap = k_tilemapManager.VillageGroundTilemap;
        if (tilemap == null) return;

        for (int x = 0; x < layout.Width; x++) {
            for (int y = 0; y < layout.Height; y++) {
                var tileType = layout.GetTileType(x, y);
                var tile = GetGroundTile(tileType);
                if (tile != null) {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

    private Tile? GetGroundTile(VillageTileType tileType) {
        string key = tileType switch {
            VillageTileType.Empty => "empty",
            VillageTileType.Path => "path",
            VillageTileType.Building => "path",
            VillageTileType.Decoration => "grass",
            VillageTileType.Blocked => "wall",
            _ => "grass"
        };
        return k_tileCache.GetValueOrDefault(key);
    }

    private void RenderBuildingLayer(VillageLayout layout) {
        var tilemap = k_tilemapManager.VillageBuildingTilemap;
        if (tilemap == null) return;

        foreach (var building in layout.Buildings) {
            RenderBuilding(building);
        }
    }

    private void RenderBuilding(Building building) {
        var tilemap = k_tilemapManager.VillageBuildingTilemap;
        if (tilemap == null) return;

        string key = $"building_{building.Proto.Id.Value}";
        if (!k_tileCache.TryGetValue(key, out var tile)) {
            Debug.LogWarning($"No tile for building: {building.Proto.Id}");
            return;
        }

        foreach (var tilePos in building.OccupiedTiles) {
            tilemap.SetTile(new Vector3Int(tilePos.x, tilePos.y, 0), tile);
        }
    }

    private void RenderFogBoundary(VillageLayout layout) {
        var tilemap = k_tilemapManager.VillageFogTilemap;
        if (tilemap == null || !k_tileCache.TryGetValue("fog", out var fogTile)) return;

        int padding = 3;

        for (int x = -padding; x < layout.Width + padding; x++) {
            for (int y = -padding; y < layout.Height + padding; y++) {
                if (x >= 0 && x < layout.Width && y >= 0 && y < layout.Height) continue;
                tilemap.SetTile(new Vector3Int(x, y, 0), fogTile);
            }
        }
    }
#endif

    #endregion

    #region Isometric Debug Rendering

#if USE_ISO_RENDERER
    /// <summary>
    /// Updates the isometric debug visualization. Call this from Update().
    /// </summary>
    public void UpdateDebugVisualization() {
        var layout = k_villageManager.Layout;
        if (layout == null) return;

        if (k_showDebugGrid) {
            DrawDebugGrid(layout);
        }

        if (k_showDebugBuildings) {
            DrawDebugBuildings();
        }

        if (k_showDebugFog) {
            DrawDebugFogBoundary(layout);
        }

        // Draw hovered/selected tiles
        if (k_hoveredTile.HasValue) {
            DrawDebugTile(k_hoveredTile.Value, k_hoveredTileColor);
        }

        if (k_selectedTile.HasValue) {
            DrawDebugTile(k_selectedTile.Value, k_selectedTileColor);
        }
    }

    /// <summary>
    /// Draws the ground grid using Debug.DrawLine.
    /// </summary>
    private void DrawDebugGrid(VillageLayout layout) {
        for (int x = 0; x < layout.Width; x++) {
            for (int y = 0; y < layout.Height; y++) {
                var isoPos = new IsoPos(x, y, IsoLevel.Ground);
                var tileType = layout.GetTileType(x, y);
                var color = GetGroundColorForTileType(tileType);

                DrawDebugTileOutline(isoPos, color);
            }
        }
    }

    /// <summary>
    /// Gets the debug color for a tile type.
    /// </summary>
    private Color GetGroundColorForTileType(VillageTileType tileType) {
        return tileType switch {
            VillageTileType.Empty => k_emptyPlotColor,
            VillageTileType.Path => k_pathColor,
            VillageTileType.Building => k_pathColor,
            VillageTileType.Decoration => k_grassColor,
            VillageTileType.Blocked => k_wallColor,
            _ => k_grassColor
        };
    }

    /// <summary>
    /// Draws the outline of a single tile using Debug.DrawLine.
    /// </summary>
    private void DrawDebugTileOutline(IsoPos pos, Color color) {
        var worldPos = pos.ToWorld();
        float halfSize = DEBUG_CUBE_SIZE / 2f;

        // Draw a diamond shape (isometric tile outline)
        var top = worldPos + new Vector3(0, 0, halfSize);
        var right = worldPos + new Vector3(halfSize, 0, 0);
        var bottom = worldPos + new Vector3(0, 0, -halfSize);
        var left = worldPos + new Vector3(-halfSize, 0, 0);

        Debug.DrawLine(top, right, color, DEBUG_LINE_DURATION);
        Debug.DrawLine(right, bottom, color, DEBUG_LINE_DURATION);
        Debug.DrawLine(bottom, left, color, DEBUG_LINE_DURATION);
        Debug.DrawLine(left, top, color, DEBUG_LINE_DURATION);
    }

    /// <summary>
    /// Draws a filled debug tile (highlighted).
    /// </summary>
    private void DrawDebugTile(IsoPos pos, Color color) {
        var worldPos = pos.ToWorld();
        float halfSize = DEBUG_CUBE_SIZE / 2f;

        // Draw diamond outline
        DrawDebugTileOutline(pos, color);

        // Draw cross in center for visibility
        var center = worldPos + new Vector3(0, 0.01f, 0); // Slight Y offset to avoid z-fighting
        Debug.DrawLine(center + new Vector3(-halfSize * 0.5f, 0, 0), 
                       center + new Vector3(halfSize * 0.5f, 0, 0), color, DEBUG_LINE_DURATION);
        Debug.DrawLine(center + new Vector3(0, 0, -halfSize * 0.5f), 
                       center + new Vector3(0, 0, halfSize * 0.5f), color, DEBUG_LINE_DURATION);
    }

    /// <summary>
    /// Draws all building volumes using Debug.DrawLine.
    /// </summary>
    private void DrawDebugBuildings() {
        foreach (var (building, volume) in k_buildingVolumes) {
            var color = GetBuildingColor(building.Proto);
            DrawDebugVolume(volume, color);
        }
    }

    /// <summary>
    /// Draws an IsoVolume as a 3D wireframe box.
    /// </summary>
    private void DrawDebugVolume(IsoVolume volume, Color color) {
        var bounds = volume.ToWorldBounds();
        var min = bounds.min;
        var max = bounds.max;

        // Bottom face
        Debug.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), color, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), color, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z), color, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, min.y, min.z), color, DEBUG_LINE_DURATION);

        // Top face
        Debug.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z), color, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z), color, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z), color, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(min.x, max.y, max.z), new Vector3(min.x, max.y, min.z), color, DEBUG_LINE_DURATION);

        // Vertical edges
        Debug.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z), color, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z), color, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z), color, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), color, DEBUG_LINE_DURATION);

        // Draw outline in darker color
        var outlineColor = k_buildingOutlineColor;
        Debug.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), outlineColor, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), outlineColor, DEBUG_LINE_DURATION);
    }

    /// <summary>
    /// Draws the fog boundary as a wireframe.
    /// </summary>
    private void DrawDebugFogBoundary(VillageLayout layout) {
        int padding = 3;
        var fogColor = k_fogColor;

        // Draw outer boundary
        var min = new IsoPos(-padding, -padding, IsoLevel.Ground);
        var max = new IsoPos(layout.Width + padding - 1, layout.Height + padding - 1, IsoLevel.Ground);

        var minWorld = min.ToWorld();
        var maxWorld = max.ToWorld() + Vector3.one;

        // Outer rectangle
        Debug.DrawLine(new Vector3(minWorld.x, 0, minWorld.z), new Vector3(maxWorld.x, 0, minWorld.z), fogColor, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(maxWorld.x, 0, minWorld.z), new Vector3(maxWorld.x, 0, maxWorld.z), fogColor, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(maxWorld.x, 0, maxWorld.z), new Vector3(minWorld.x, 0, maxWorld.z), fogColor, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(minWorld.x, 0, maxWorld.z), new Vector3(minWorld.x, 0, minWorld.z), fogColor, DEBUG_LINE_DURATION);

        // Inner rectangle (village bounds)
        var villageMin = new IsoPos(0, 0, IsoLevel.Ground).ToWorld();
        var villageMax = new IsoPos(layout.Width - 1, layout.Height - 1, IsoLevel.Ground).ToWorld() + Vector3.one;

        Debug.DrawLine(new Vector3(villageMin.x, 0.1f, villageMin.z), new Vector3(villageMax.x, 0.1f, villageMin.z), Color.green, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(villageMax.x, 0.1f, villageMin.z), new Vector3(villageMax.x, 0.1f, villageMax.z), Color.green, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(villageMax.x, 0.1f, villageMax.z), new Vector3(villageMin.x, 0.1f, villageMax.z), Color.green, DEBUG_LINE_DURATION);
        Debug.DrawLine(new Vector3(villageMin.x, 0.1f, villageMax.z), new Vector3(villageMin.x, 0.1f, villageMin.z), Color.green, DEBUG_LINE_DURATION);
    }

    /// <summary>
    /// Called from OnDrawGizmos for editor visualization.
    /// </summary>
    public void DrawGizmos() {
        var layout = k_villageManager.Layout;
        if (layout == null) return;

        // Draw building volumes as solid gizmos
        if (k_showDebugBuildings) {
            foreach (var (building, volume) in k_buildingVolumes) {
                var color = GetBuildingColor(building.Proto);
                color.a = 0.5f;
                Gizmos.color = color;

                var bounds = volume.ToWorldBounds();
                Gizmos.DrawCube(bounds.center, bounds.size);

                // Draw wireframe on top
                Gizmos.color = k_buildingOutlineColor;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

        // Draw hovered tile
        if (k_hoveredTile.HasValue) {
            Gizmos.color = k_hoveredTileColor;
            var pos = k_hoveredTile.Value.ToWorld() + new Vector3(0.5f, 0.05f, 0.5f);
            Gizmos.DrawCube(pos, new Vector3(DEBUG_CUBE_SIZE, 0.1f, DEBUG_CUBE_SIZE));
        }

        // Draw selected tile
        if (k_selectedTile.HasValue) {
            Gizmos.color = k_selectedTileColor;
            var pos = k_selectedTile.Value.ToWorld() + new Vector3(0.5f, 0.1f, 0.5f);
            Gizmos.DrawWireCube(pos, new Vector3(DEBUG_CUBE_SIZE, 0.2f, DEBUG_CUBE_SIZE));
        }
    }
#endif

    #endregion

    #region Building Events

    private void OnBuildingPlaced(Building building) {
#if USE_ISO_RENDERER
        k_buildingVolumes[building] = CreateBuildingVolume(building);
        Debug.Log($"VillageTilemapRenderer: Building placed {building.DisplayName} at {building.Position} (iso volume created)");
#else
        RenderBuilding(building);
#endif
    }

    private void OnBuildingRemoved(Building building) {
#if USE_ISO_RENDERER
        k_buildingVolumes.Remove(building);
        Debug.Log($"VillageTilemapRenderer: Building removed {building.DisplayName} (iso volume removed)");
#else
        var tilemap = k_tilemapManager.VillageBuildingTilemap;
        if (tilemap == null) return;
        foreach (var tilePos in building.OccupiedTiles) {
            tilemap.SetTile(new Vector3Int(tilePos.x, tilePos.y, 0), null);
        }
#endif
    }

    private void OnBuildingMoved(Building building, Vector2Int oldPos, Vector2Int newPos) {
#if USE_ISO_RENDERER
        k_buildingVolumes[building] = CreateBuildingVolume(building);
        Debug.Log($"VillageTilemapRenderer: Building moved {building.DisplayName} from {oldPos} to {newPos}");
#else
        var tilemap = k_tilemapManager.VillageBuildingTilemap;
        if (tilemap == null) return;
        for (int x = 0; x < building.Proto.Width; x++) {
            for (int y = 0; y < building.Proto.Height; y++) {
                tilemap.SetTile(new Vector3Int(oldPos.x + x, oldPos.y + y, 0), null);
            }
        }
        RenderBuilding(building);
#endif
    }

    private void OnBuildingUpgraded(Building building) {
#if USE_ISO_RENDERER
        // Recalculate volume in case height changes with level
        k_buildingVolumes[building] = CreateBuildingVolume(building);
#endif
        Debug.Log($"VillageTilemapRenderer: Building upgraded {building.DisplayName}");
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// Handles a click at the specified world position.
    /// </summary>
    /// <param name="worldPosition">The world position that was clicked.</param>
    public void HandleClick(Vector3 worldPosition) {
#if USE_ISO_RENDERER
        HandleClickIsometric(worldPosition);
#else
        HandleClickTilemap(worldPosition);
#endif
    }

#if USE_ISO_RENDERER
    /// <summary>
    /// Handles click using isometric ray casting.
    /// </summary>
    private void HandleClickIsometric(Vector3 worldPosition) {
        var layout = k_villageManager.Layout;
        if (layout == null) return;

        // Convert world position to iso position
        var isoPos = IsoPos.FromWorld(worldPosition);
        k_selectedTile = isoPos;

        var tilePos = new Vector2Int(isoPos.X, isoPos.Y);
        Debug.Log($"VillageTilemapRenderer: ISO Click at {isoPos} (grid: {tilePos})");

        OnTileClicked?.Invoke(tilePos);

        // Check if we hit a building volume
        Building? hitBuilding = null;
        foreach (var (building, volume) in k_buildingVolumes) {
            if (volume.Contains(isoPos)) {
                hitBuilding = building;
                break;
            }
        }

        if (hitBuilding != null) {
            if (hitBuilding.Proto.Id == Ids.Buildings.Anchor) {
                OnAnchorClicked?.Invoke();
            } else if (hitBuilding.Proto.Id == Ids.Buildings.Gate) {
                OnGateClicked?.Invoke();
            } else {
                OnBuildingClicked?.Invoke(hitBuilding);
            }
        } else if (layout.IsInBounds(tilePos) && layout.GetTileType(tilePos) == VillageTileType.Empty) {
            OnEmptyPlotClicked?.Invoke(tilePos);
        }
    }

    /// <summary>
    /// Handles mouse hover for tile highlighting.
    /// </summary>
    /// <param name="worldPosition">Current mouse world position.</param>
    public void HandleHover(Vector3 worldPosition) {
        var layout = k_villageManager.Layout;
        if (layout == null) return;

        var isoPos = IsoPos.FromWorld(worldPosition);

        // Only update if position changed
        if (k_hoveredTile != isoPos) {
            var previousHover = k_hoveredTile;
            k_hoveredTile = isoPos;

            if (layout.IsInBounds(new Vector2Int(isoPos.X, isoPos.Y))) {
                OnTileHovered?.Invoke(isoPos);
            } else if (previousHover.HasValue) {
                k_hoveredTile = null;
                OnTileHoverExit?.Invoke();
            }
        }
    }

    /// <summary>
    /// Clears the current hover state.
    /// </summary>
    public void ClearHover() {
        if (k_hoveredTile.HasValue) {
            k_hoveredTile = null;
            OnTileHoverExit?.Invoke();
        }
    }

    /// <summary>
    /// Uses IsoRay for precise mouse picking through 3D volumes.
    /// </summary>
    /// <param name="screenPosition">Screen position in pixels.</param>
    /// <param name="camera">Camera for screen-to-world conversion.</param>
    /// <returns>The hit result, or null if nothing was hit.</returns>
    public IsoRayHit? RaycastFromScreen(Vector2 screenPosition, Camera camera) {
        // Create a ray from the camera through the screen position
        var ray = camera.ScreenPointToRay(screenPosition);

        // Find where the ray intersects the ground plane (Y = 0)
        var groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance)) {
            var worldPoint = ray.GetPoint(distance);
            var isoPos = IsoPos.FromWorld(worldPoint);

            // Check if we hit any building volume
            foreach (var (building, volume) in k_buildingVolumes) {
                if (volume.Contains(isoPos)) {
                    return new IsoRayHit(
                        hit: true,
                        position: isoPos,
                        distance: distance,
                        ray: IsoRay.FromDirection(isoPos, IsoRel.Down)
                    );
                }
            }

            // Return ground hit
            return new IsoRayHit(
                hit: true,
                position: isoPos,
                distance: distance,
                ray: IsoRay.FromDirection(isoPos, IsoRel.Down)
            );
        }

        return null;
    }
#endif

#if !USE_ISO_RENDERER
    /// <summary>
    /// Handles click using tilemap world-to-cell conversion.
    /// </summary>
    private void HandleClickTilemap(Vector3 worldPosition) {
        var tilemap = k_tilemapManager.VillageGroundTilemap;
        if (tilemap == null) return;

        var cellPos = tilemap.WorldToCell(worldPosition);
        var tilePos = new Vector2Int(cellPos.x, cellPos.y);

        Debug.Log($"VillageTilemapRenderer: Click at tile {tilePos}");
        OnTileClicked?.Invoke(tilePos);

        var layout = k_villageManager.Layout;
        if (layout == null) return;

        var building = layout.GetBuildingAt(tilePos);
        if (building != null) {
            if (building.Proto.Id == Ids.Buildings.Anchor) OnAnchorClicked?.Invoke();
            else if (building.Proto.Id == Ids.Buildings.Gate) OnGateClicked?.Invoke();
            else OnBuildingClicked?.Invoke(building);
        } else if (layout.IsInBounds(tilePos) && layout.GetTileType(tilePos) == VillageTileType.Empty) {
            OnEmptyPlotClicked?.Invoke(tilePos);
        }
    }
#endif

    #endregion

    #region Debug Utilities

#if USE_ISO_RENDERER
    /// <summary>
    /// Toggles all debug visualization on or off.
    /// </summary>
    public void ToggleAllDebugVisualization(bool enabled) {
        k_showDebugGrid = enabled;
        k_showDebugBuildings = enabled;
        k_showDebugFog = enabled;
    }

    /// <summary>
    /// Gets debug info about a position.
    /// </summary>
    public string GetDebugInfoAt(IsoPos pos) {
        var layout = k_villageManager.Layout;
        if (layout == null) return "No layout";

        var gridPos = new Vector2Int(pos.X, pos.Y);

        if (!layout.IsInBounds(gridPos)) {
            return $"Out of bounds: {pos}";
        }

        var tileType = layout.GetTileType(gridPos);
        var building = layout.GetBuildingAt(gridPos);

        if (building != null && k_buildingVolumes.TryGetValue(building, out var volume)) {
            return $"Pos: {pos}\nTile: {tileType}\nBuilding: {building.DisplayName}\nVolume: {volume:size}";
        }

        return $"Pos: {pos}\nTile: {tileType}";
    }
#endif

    #endregion

    #region Cleanup

    /// <summary>
    /// Cleans up resources and unsubscribes from events.
    /// </summary>
    public void Cleanup() {
        if (k_villageManager.Layout != null) {
            k_villageManager.Layout.OnBuildingPlaced -= OnBuildingPlaced;
            k_villageManager.Layout.OnBuildingRemoved -= OnBuildingRemoved;
            k_villageManager.Layout.OnBuildingMoved -= OnBuildingMoved;
            k_villageManager.Layout.OnBuildingUpgraded -= OnBuildingUpgraded;
        }

#if USE_ISO_RENDERER
        k_buildingVolumes.Clear();
        k_hoveredTile = null;
        k_selectedTile = null;
#endif

        k_isInitialized = false;
    }

    #endregion
}