using RPGGame.Core.Village;
using UnityEngine;

namespace RPGGame.Core.Isometric.Rendering;

/// <summary>
/// Isometric renderer for the village world using square sprites on XZ plane.
/// The 3D orthographic camera at 45° angle provides the isometric diamond look.
/// Uses <see cref="TilesRPG"/> for tile measurements and <see cref="ColorRPG"/> for colors.
/// 
/// Buildings and height-aware tiles include side sprites for 3D depth effect.
/// Camera is positioned at 45° yaw, so the "front" of tiles is the bottom corner.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class VillageIsoRenderer : IsoWorldRendererBase {
    #region Constants

    private const int DEFAULT_BUILDING_HEIGHT = 1;
    private const int TALL_BUILDING_HEIGHT = 2;
    private const int SPECIAL_BUILDING_HEIGHT = 3;
    private const string ROOT_NAME = "VillageIsoRoot";
    private const string GROUND_LAYER = "Ground";
    private const string BUILDINGS_LAYER = "Buildings";
    private const string FOG_LAYER = "Fog";

    // Sprite generation
    private const int TILE_TEXTURE_SIZE = 256;
    private const int SIDE_TEXTURE_WIDTH = 256;
    private const int SIDE_TEXTURE_HEIGHT = 128;
    private const float PIXELS_PER_UNIT = 256f;

    /// <summary>Fog padding around village (number of tiles).</summary>
    public const int FOG_PADDING = 3;

    #endregion

    #region Tile Dimensions

    /// <summary>World-space size of one tile.</summary>
    public static readonly TilesRPG WorldTileSize = TilesRPG.One;

    /// <summary>Height of one building level.</summary>
    private static readonly TilesRPG BuildingLevelHeight = TilesRPG.Half;

    /// <summary>Small offset to prevent z-fighting.</summary>
    private static readonly TilesRPG ZFightOffset = 0.01f.Tiles();

    #endregion

    #region Dependencies

    private readonly VillageManager _villageManager;
    private readonly GameDb _gameDb;
    private readonly IsoCameraController _cameraController;

    #endregion

    #region State

    private Transform? _groundLayer;
    private Transform? _buildingsLayer;
    private Transform? _fogLayer;
    private Transform? _highlightLayer;

    private readonly Dictionary<Building, GameObject> _buildingObjects = new();
    private readonly Dictionary<Building, IsoVolume> _buildingVolumes = new();
    private readonly Dictionary<IsoPos, GameObject> _groundTiles = new();
    private readonly List<GameObject> _fogObjects = new();

    private GameObject? _hoverHighlight;
    private GameObject? _selectionHighlight;

    // Sprite caches
    private readonly Dictionary<ColorRPG, Sprite> _topSpriteCache = new();
    private readonly Dictionary<ColorRPG, Sprite> _leftSideCache = new();
    private readonly Dictionary<ColorRPG, Sprite> _rightSideCache = new();

    // Cached layout dimensions
    private int _layoutWidth;
    private int _layoutHeight;

    // Track if we've rendered at least once (for re-show without re-render)
    private bool _hasRendered;

    #endregion

    #region Colors

    private static readonly ColorRPG EmptyPlotColor = ColorRPG.FromHSL(100, 0.35f, 0.45f);
    private static readonly ColorRPG PathColor = ColorRPG.FromHSL(30, 0.25f, 0.55f);
    private static readonly ColorRPG GrassColor = ColorRPG.FromHSL(110, 0.4f, 0.35f);
    private static readonly ColorRPG FogColor = ColorRPG.FromHSL(240, 0.15f, 0.2f).WithAlpha(0.95f);
    private static readonly ColorRPG WallColor = ColorRPG.FromHSL(25, 0.15f, 0.5f);

    private static readonly ColorRPG HoverColor = ColorRPG.White.WithAlpha(0.4f);
    private static readonly ColorRPG SelectionColor = ColorRPG.Yellow.WithAlpha(0.6f);

    private static readonly Dictionary<BuildingCategory, ColorRPG> BuildingColors = new() {
        { BuildingCategory.Core, ColorRPG.Gold },
        { BuildingCategory.Commerce, ColorRPG.Sapphire },
        { BuildingCategory.Production, ColorRPG.Copper },
        { BuildingCategory.Training, ColorRPG.Ruby },
        { BuildingCategory.Defense, ColorRPG.Iron },
        { BuildingCategory.Utility, ColorRPG.Nature },
    };

    private static readonly ColorRPG AnchorColor = ColorRPG.Gold.Lighten(0.1f);
    private static readonly ColorRPG GateColor = ColorRPG.Bronze;

    private const float LEFT_SIDE_DARKEN = 0.30f;
    private const float RIGHT_SIDE_DARKEN = 0.15f;

    #endregion

    #region Events

    public event Action<Vector2Int>? OnTileClicked;
    public event Action<Building>? OnBuildingClicked;
    public event Action<Vector2Int>? OnEmptyPlotClicked;
    public event Action? OnGateClicked;
    public event Action? OnAnchorClicked;
    public event Action<IsoPos>? OnTileHovered;
    public event Action? OnTileHoverExit;

    #endregion

    #region Constructor

    public VillageIsoRenderer(VillageManager villageManager, GameDb gameDb, IsoCameraController cameraController) : base() {
        _villageManager = villageManager;
        _gameDb = gameDb;
        _cameraController = cameraController;

        TileWidth = WorldTileSize;
        TileHeight = WorldTileSize;
        LevelHeight = BuildingLevelHeight;

        Debug.Log("VillageIsoRenderer: Created");
    }

    #endregion

    #region Visibility Control

    /// <summary>
    /// Shows or hides the renderer without destroying objects.
    /// Use this when switching between game phases to preserve rendered state.
    /// </summary>
    /// <param name="visible">Whether the renderer should be visible.</param>
    public void SetVisible(bool visible) {
        if (RootObject != null) {
            RootObject.SetActive(visible);
        }
        IsActive = visible;

        if (visible) {
            Debug.Log("VillageIsoRenderer: Made visible");
        } else {
            // Clear hover state when hiding
            ClearHover();
            Debug.Log("VillageIsoRenderer: Hidden");
        }
    }

    /// <summary>
    /// Activates the renderer, showing it and optionally rendering if needed.
    /// </summary>
    /// <param name="forceRender">If true, forces a full re-render even if already rendered.</param>
    public void Activate(bool forceRender = false) {
        SetVisible(true);

        if (forceRender || !_hasRendered) {
            Render();
        }

        // Setup camera for village view
        var layout = _villageManager.Layout;
        if (layout != null) {
            SetupCamera(layout);
        }

        Debug.Log("VillageIsoRenderer: Activated");
    }

    /// <summary>
    /// Deactivates the renderer, hiding it but preserving all rendered objects.
    /// </summary>
    public void Deactivate() {
        SetVisible(false);
        Debug.Log("VillageIsoRenderer: Deactivated");
    }

    #endregion

    #region Coordinate Conversion

	private Vector3 GridToWorldCenter(int gridX, int gridY, TilesRPG height) {
		float tileSize = WorldTileSize.Value;  // Extract to float first
		float halfTile = tileSize * 0.5f;
        
		float worldX = (gridX + FOG_PADDING) * tileSize + halfTile;
		float worldZ = (gridY + FOG_PADDING) * tileSize + halfTile;
		float worldY = height.Value;  // Extract to float
        
		return new Vector3(worldX, worldY, worldZ);
	}

	private Vector3 GridToWorldCenter(IsoPos pos) {
		float levelHeight = BuildingLevelHeight.Value;  // Extract to float first
		return GridToWorldCenter(pos.X, pos.Y, new TilesRPG(pos.Level.Value * levelHeight));
	}

	private IsoPos WorldToGrid(Vector3 worldPos) {
		float tileSize = WorldTileSize.Value;  // Extract to float first
		float levelHeight = BuildingLevelHeight.Value;  // Extract to float first
        
		int gridX = Mathf.FloorToInt(worldPos.x / tileSize) - FOG_PADDING;
		int gridY = Mathf.FloorToInt(worldPos.z / tileSize) - FOG_PADDING;
		int level = Mathf.RoundToInt(worldPos.y / levelHeight);
        
		return new IsoPos(gridX, gridY, new IsoLevel(level));
	}

	private Vector3 FogToWorldCenter(int fogX, int fogY) {
		float tileSize = WorldTileSize.Value;  // Extract to float first
		float halfTile = tileSize * 0.5f;
        
		float worldX = fogX * tileSize + halfTile;
		float worldZ = fogY * tileSize + halfTile;
        
		return new Vector3(worldX, 0f, worldZ);
	}

    private bool IsInLayoutBounds(int gridX, int gridY) {
        return gridX >= 0 && gridX < _layoutWidth && gridY >= 0 && gridY < _layoutHeight;
    }

    #endregion

    #region Initialization

    public override void Initialize() {
        if (IsInitialized) return;

        CreateLayers();
        SubscribeToEvents();

        if (!_cameraController.IsInitialized) {
            _cameraController.Initialize();
        }

        IsInitialized = true;
        IsActive = true;

        Debug.Log("VillageIsoRenderer: Initialized");
    }

    private void CreateLayers() {
        var root = GetOrCreateRoot(ROOT_NAME);
        root.transform.position = Vector3.zero;

        _fogLayer = new GameObject(FOG_LAYER).transform;
        _fogLayer.SetParent(root.transform);
        _fogLayer.localPosition = Vector3.zero;

        _groundLayer = new GameObject(GROUND_LAYER).transform;
        _groundLayer.SetParent(root.transform);
        _groundLayer.localPosition = Vector3.zero;

        _buildingsLayer = new GameObject(BUILDINGS_LAYER).transform;
        _buildingsLayer.SetParent(root.transform);
        _buildingsLayer.localPosition = Vector3.zero;

        _highlightLayer = new GameObject("Highlights").transform;
        _highlightLayer.SetParent(root.transform);
        _highlightLayer.localPosition = Vector3.zero;

        _hoverHighlight = CreateSquareTileObject("HoverHighlight", HoverColor, _highlightLayer);
        _selectionHighlight = CreateSquareTileObject("SelectionHighlight", SelectionColor, _highlightLayer);
        _hoverHighlight.SetActive(false);
        _selectionHighlight.SetActive(false);
    }

    private void SubscribeToEvents() {
        var layout = _villageManager.Layout;
        if (layout == null) return;

        layout.OnBuildingPlaced += OnBuildingPlacedHandler;
        layout.OnBuildingRemoved += OnBuildingRemovedHandler;
        layout.OnBuildingMoved += OnBuildingMovedHandler;
        layout.OnBuildingUpgraded += OnBuildingUpgradedHandler;
    }

    private void UnsubscribeFromEvents() {
        var layout = _villageManager.Layout;
        if (layout == null) return;

        layout.OnBuildingPlaced -= OnBuildingPlacedHandler;
        layout.OnBuildingRemoved -= OnBuildingRemovedHandler;
        layout.OnBuildingMoved -= OnBuildingMovedHandler;
        layout.OnBuildingUpgraded -= OnBuildingUpgradedHandler;
    }

    #endregion

    #region Sprite Creation - Top Surfaces

    private Sprite GetOrCreateTopSprite(ColorRPG color) {
        var roundedColor = RoundColorForCache(color);

        if (_topSpriteCache.TryGetValue(roundedColor, out var cached)) {
            return cached;
        }

        var sprite = CreateSquareTileSprite(color);
        _topSpriteCache[roundedColor] = sprite;
        return sprite;
    }

    private Sprite CreateSquareTileSprite(ColorRPG fillColor) {
        int size = TILE_TEXTURE_SIZE;
        var texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;

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

    private GameObject CreateSquareTileObject(string name, ColorRPG color, Transform? parent = null) {
        var go = new GameObject(name);
        go.transform.SetParent(parent ?? RootObject?.transform);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrCreateTopSprite(color);

        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        return go;
    }

    #endregion

    #region Sprite Creation - Side Surfaces

    private Sprite GetOrCreateLeftSideSprite(ColorRPG topColor) {
        var sideColor = topColor.Darken(LEFT_SIDE_DARKEN);
        var roundedColor = RoundColorForCache(sideColor);

        if (_leftSideCache.TryGetValue(roundedColor, out var cached)) {
            return cached;
        }

        var sprite = CreateSideSprite(sideColor);
        _leftSideCache[roundedColor] = sprite;
        return sprite;
    }

    private Sprite GetOrCreateRightSideSprite(ColorRPG topColor) {
        var sideColor = topColor.Darken(RIGHT_SIDE_DARKEN);
        var roundedColor = RoundColorForCache(sideColor);

        if (_rightSideCache.TryGetValue(roundedColor, out var cached)) {
            return cached;
        }

        var sprite = CreateSideSprite(sideColor);
        _rightSideCache[roundedColor] = sprite;
        return sprite;
    }

    private Sprite CreateSideSprite(ColorRPG fillColor) {
        int width = SIDE_TEXTURE_WIDTH;
        int height = SIDE_TEXTURE_HEIGHT;
        var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;

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

    #region Rendering

    public override void Render() {
        if (!IsInitialized) Initialize();

        var layout = _villageManager.Layout;
        if (layout == null) {
            Debug.LogWarning("VillageIsoRenderer: No layout to render");
            return;
        }

        _layoutWidth = layout.Width;
        _layoutHeight = layout.Height;

        Debug.Log($"VillageIsoRenderer: Rendering {_layoutWidth}x{_layoutHeight} village with {FOG_PADDING} tile fog padding");

        ClearAllLayers();
        RenderFog(layout);
        RenderGround(layout);
        RenderBuildings(layout);
        SetupCamera(layout);

        _hasRendered = true;

        Debug.Log($"VillageIsoRenderer: Complete - {_groundTiles.Count} ground, {_buildingObjects.Count} buildings, {_fogObjects.Count} fog");
    }

    private void SetupCamera(VillageLayout layout) {
        _cameraController.SetupVillageView(
            layout.Width,
            layout.Height,
            WorldTileSize,
            FOG_PADDING,
            immediate: true
        );
    }

    public override void Update(float deltaTime) { }

    private void ClearAllLayers() {
        foreach (var tile in _groundTiles.Values) {
            UnityEngine.Object.Destroy(tile);
        }
        _groundTiles.Clear();

        foreach (var buildingObj in _buildingObjects.Values) {
            UnityEngine.Object.Destroy(buildingObj);
        }
        _buildingObjects.Clear();
        _buildingVolumes.Clear();

        foreach (var fog in _fogObjects) {
            UnityEngine.Object.Destroy(fog);
        }
        _fogObjects.Clear();
    }

    private void RenderGround(VillageLayout layout) {
        if (_groundLayer == null) return;

        for (int x = 0; x < layout.Width; x++) {
            for (int y = 0; y < layout.Height; y++) {
                var tileType = layout.GetTileType(x, y);
                var color = GetGroundColor(tileType);
                var isoPos = new IsoPos(x, y, IsoLevel.Ground);

                var tile = CreateGroundTile(isoPos, color);
                _groundTiles[isoPos] = tile;
            }
        }

        Debug.Log($"VillageIsoRenderer: Rendered {_groundTiles.Count} ground tiles");
    }

    private GameObject CreateGroundTile(IsoPos pos, ColorRPG color) {
        var go = CreateSquareTileObject($"Tile_{pos.X}_{pos.Y}", color, _groundLayer);
        go.transform.position = GridToWorldCenter(pos.X, pos.Y, ZFightOffset);

        var sr = go.GetComponent<SpriteRenderer>();
        sr.sortingOrder = CalculateSortingOrder(pos.X, pos.Y, 0);

        return go;
    }

    private ColorRPG GetGroundColor(VillageTileType tileType) {
        return tileType switch {
            VillageTileType.Empty => EmptyPlotColor,
            VillageTileType.Path => PathColor,
            VillageTileType.Building => PathColor,
            VillageTileType.Decoration => GrassColor,
            VillageTileType.Blocked => WallColor,
            _ => GrassColor
        };
    }

    private void RenderBuildings(VillageLayout layout) {
        foreach (var building in layout.Buildings) {
            RenderBuilding(building);
        }
    }

    private void RenderBuilding(Building building) {
        var volume = CreateBuildingVolume(building);
        _buildingVolumes[building] = volume;

        var color = GetBuildingColor(building);
        var go = CreateBuildingObject(building, volume, color);
        _buildingObjects[building] = go;
    }

    private IsoVolume CreateBuildingVolume(Building building) {
        var origin = new IsoPos(building.Position.x, building.Position.y, IsoLevel.Ground);
        int height = GetBuildingHeight(building);
        return new IsoVolume(origin, building.Proto.Width, building.Proto.Height, height);
    }

    private int GetBuildingHeight(Building building) {
        if (building.Proto.Id == Ids.Buildings.Anchor) return SPECIAL_BUILDING_HEIGHT;
        if (building.Proto.Id == Ids.Buildings.Gate) return SPECIAL_BUILDING_HEIGHT;

        return building.Proto.Category switch {
            BuildingCategory.Core => TALL_BUILDING_HEIGHT,
            BuildingCategory.Defense => TALL_BUILDING_HEIGHT,
            _ => DEFAULT_BUILDING_HEIGHT
        };
    }

    private ColorRPG GetBuildingColor(Building building) {
        if (building.Proto.Id == Ids.Buildings.Anchor) return AnchorColor;
        if (building.Proto.Id == Ids.Buildings.Gate) return GateColor;
        return BuildingColors.GetValueOrDefault(building.Proto.Category, ColorRPG.Magenta);
    }

    private GameObject CreateBuildingObject(Building building, IsoVolume volume, ColorRPG color) {
        var go = new GameObject($"Building_{building.Proto.Id.Value}_{building.Position.x}_{building.Position.y}");
        go.transform.SetParent(_buildingsLayer);

        var centerPos = GridToWorldCenter(building.Position.x, building.Position.y, TilesRPG.Zero);

        if (building.Proto.Width > 1 || building.Proto.Height > 1) {
            float offsetX = (building.Proto.Width - 1) * WorldTileSize / 2f;
            float offsetZ = (building.Proto.Height - 1) * WorldTileSize / 2f;
            centerPos.x += offsetX;
            centerPos.z += offsetZ;
        }

        go.transform.position = centerPos;

        float buildingWidth = building.Proto.Width * WorldTileSize * 0.9f;
        float buildingDepth = building.Proto.Height * WorldTileSize * 0.9f;
        float levelHeight = BuildingLevelHeight;

        int levelCount = volume.HeightInLevels;
        for (int level = 0; level < levelCount; level++) {
            CreateBuildingLevel(go.transform, building, color, level, levelCount,
                buildingWidth, buildingDepth, levelHeight);
        }

        return go;
    }

    private void CreateBuildingLevel(Transform parent, Building building, ColorRPG baseColor,
        int levelIndex, int totalLevels, float width, float depth, float levelHeight) {

        var levelGo = new GameObject($"Level_{levelIndex}");
        levelGo.transform.SetParent(parent);

        float yBase = levelIndex * levelHeight + ZFightOffset * 2;
        levelGo.transform.localPosition = new Vector3(0f, yBase, 0f);

        ColorRPG levelColor = baseColor;
        if (totalLevels > 1 && levelIndex > 0) {
            levelColor = baseColor.Lighten(levelIndex * 0.08f);
        }

        int baseSortOrder = CalculateSortingOrder(building.Position.x, building.Position.y, 100 + levelIndex * 10);

        float halfWidth = width / 2f;
        float halfDepth = depth / 2f;

        // TOP SURFACE
        var topGo = new GameObject("Top");
        topGo.transform.SetParent(levelGo.transform);
        topGo.transform.localPosition = new Vector3(0f, levelHeight, 0f);
        topGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        topGo.transform.localScale = new Vector3(width / WorldTileSize, depth / WorldTileSize, 1f);

        var topSr = topGo.AddComponent<SpriteRenderer>();
        topSr.sprite = GetOrCreateTopSprite(levelColor);
        topSr.sortingOrder = baseSortOrder + 5;

        // LEFT WALL (West face)
        var leftWallGo = new GameObject("LeftWall");
        leftWallGo.transform.SetParent(levelGo.transform);
        leftWallGo.transform.localPosition = new Vector3(-halfWidth, 0f, 0f);
        leftWallGo.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        float leftScaleX = depth / WorldTileSize;
        float leftScaleY = levelHeight / (SIDE_TEXTURE_HEIGHT / PIXELS_PER_UNIT);
        leftWallGo.transform.localScale = new Vector3(leftScaleX, leftScaleY, 1f);

        var leftSr = leftWallGo.AddComponent<SpriteRenderer>();
        leftSr.sprite = GetOrCreateLeftSideSprite(levelColor);
        leftSr.sortingOrder = baseSortOrder + 2;

        // RIGHT WALL (South face)
        var rightWallGo = new GameObject("RightWall");
        rightWallGo.transform.SetParent(levelGo.transform);
        rightWallGo.transform.localPosition = new Vector3(0f, 0f, -halfDepth);
        rightWallGo.transform.localRotation = Quaternion.identity;
        float rightScaleX = width / WorldTileSize;
        float rightScaleY = levelHeight / (SIDE_TEXTURE_HEIGHT / PIXELS_PER_UNIT);
        rightWallGo.transform.localScale = new Vector3(rightScaleX, rightScaleY, 1f);

        var rightSr = rightWallGo.AddComponent<SpriteRenderer>();
        rightSr.sprite = GetOrCreateRightSideSprite(levelColor);
        rightSr.sortingOrder = baseSortOrder + 3;
    }

    private void RenderFog(VillageLayout layout) {
        if (_fogLayer == null) return;

        int totalWidth = layout.Width + FOG_PADDING * 2;
        int totalHeight = layout.Height + FOG_PADDING * 2;

        int villageStartX = FOG_PADDING;
        int villageStartY = FOG_PADDING;
        int villageEndX = FOG_PADDING + layout.Width;
        int villageEndY = FOG_PADDING + layout.Height;

        for (int worldX = 0; worldX < totalWidth; worldX++) {
            for (int worldY = 0; worldY < totalHeight; worldY++) {
                if (worldX >= villageStartX && worldX < villageEndX &&
                    worldY >= villageStartY && worldY < villageEndY) {
                    continue;
                }

                var fog = CreateFogTile(worldX, worldY);
                _fogObjects.Add(fog);
            }
        }

        Debug.Log($"VillageIsoRenderer: Rendered {_fogObjects.Count} fog tiles");
    }

    private GameObject CreateFogTile(int worldX, int worldY) {
        var go = CreateSquareTileObject($"Fog_{worldX}_{worldY}", FogColor, _fogLayer);
        go.transform.position = FogToWorldCenter(worldX, worldY);

        var sr = go.GetComponent<SpriteRenderer>();
        sr.sortingOrder = CalculateSortingOrder(worldX - FOG_PADDING, worldY - FOG_PADDING, -100);

        return go;
    }

    private int CalculateSortingOrder(int gridX, int gridY, int layerOffset) {
        int depth = gridX + gridY;
        return layerOffset + depth;
    }

    #endregion

    #region Input Handling

    public override void HandleClick(Vector3 worldPosition) {
        if (!IsActive) return;

        var isoPos = WorldToGrid(worldPosition);
        var tilePos = new Vector2Int(isoPos.X, isoPos.Y);

        SelectedPosition = isoPos;
        UpdateSelectionHighlight();

        Debug.Log($"VillageIsoRenderer: Click at world ({worldPosition.x:F2}, {worldPosition.z:F2}) -> grid ({isoPos.X}, {isoPos.Y}), inBounds={IsInLayoutBounds(isoPos.X, isoPos.Y)}");

        if (!IsInLayoutBounds(isoPos.X, isoPos.Y)) {
            return;
        }

        OnTileClicked?.Invoke(tilePos);

        var layout = _villageManager.Layout;
        if (layout == null) return;

        Building? hitBuilding = null;
        foreach (var (building, volume) in _buildingVolumes) {
            if (tilePos.x >= building.Position.x &&
                tilePos.x < building.Position.x + building.Proto.Width &&
                tilePos.y >= building.Position.y &&
                tilePos.y < building.Position.y + building.Proto.Height) {
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

    public override void HandleHover(Vector3 worldPosition) {
        if (!IsActive) return;

        var isoPos = WorldToGrid(worldPosition);

        if (HoveredPosition == isoPos) return;

        HoveredPosition = isoPos;
        UpdateHoverHighlight();

        if (IsInLayoutBounds(isoPos.X, isoPos.Y)) {
            OnTileHovered?.Invoke(isoPos);
        }
    }

    public override void ClearHover() {
        if (HoveredPosition.HasValue) {
            HoveredPosition = null;
            _hoverHighlight?.SetActive(false);
            OnTileHoverExit?.Invoke();
        }
    }

    private void UpdateHoverHighlight() {
        if (_hoverHighlight == null) return;

        if (HoveredPosition.HasValue) {
            var pos = HoveredPosition.Value;

            if (IsInLayoutBounds(pos.X, pos.Y)) {
                _hoverHighlight.SetActive(true);
                _hoverHighlight.transform.position = GridToWorldCenter(pos.X, pos.Y, ZFightOffset * 2);

                var sr = _hoverHighlight.GetComponent<SpriteRenderer>();
                if (sr != null) {
                    sr.sortingOrder = CalculateSortingOrder(pos.X, pos.Y, 500);
                }
            } else {
                _hoverHighlight.SetActive(false);
            }
        } else {
            _hoverHighlight.SetActive(false);
        }
    }
    
	
    private void UpdateSelectionHighlight() {
        if (_selectionHighlight == null) return;

        if (SelectedPosition.HasValue) {
            var pos = SelectedPosition.Value;

            if (IsInLayoutBounds(pos.X, pos.Y)) {
                _selectionHighlight.SetActive(true);
                _selectionHighlight.transform.position = GridToWorldCenter(pos.X, pos.Y, ZFightOffset * 3);

                var sr = _selectionHighlight.GetComponent<SpriteRenderer>();
                if (sr != null) {
                    sr.sortingOrder = CalculateSortingOrder(pos.X, pos.Y, 501);
                }
            } else {
                _selectionHighlight.SetActive(false);
            }
        } else {
            _selectionHighlight.SetActive(false);
        }
    }

    #endregion

    #region Building Events

    private void OnBuildingPlacedHandler(Building building) {
        if (!IsActive) return;
        RenderBuilding(building);
        Debug.Log($"VillageIsoRenderer: Building placed {building.DisplayName}");
    }

    private void OnBuildingRemovedHandler(Building building) {
        if (_buildingObjects.TryGetValue(building, out var go)) {
            UnityEngine.Object.Destroy(go);
            _buildingObjects.Remove(building);
            _buildingVolumes.Remove(building);
        }
        Debug.Log($"VillageIsoRenderer: Building removed {building.DisplayName}");
    }

    private void OnBuildingMovedHandler(Building building, Vector2Int oldPos, Vector2Int newPos) {
        OnBuildingRemovedHandler(building);
        if (IsActive) {
            RenderBuilding(building);
        }
        Debug.Log($"VillageIsoRenderer: Building moved from {oldPos} to {newPos}");
    }

    private void OnBuildingUpgradedHandler(Building building) {
        OnBuildingRemovedHandler(building);
        if (IsActive) {
            RenderBuilding(building);
        }
        Debug.Log($"VillageIsoRenderer: Building upgraded {building.DisplayName}");
    }

    #endregion

    #region Cleanup

    public override void Cleanup() {
        UnsubscribeFromEvents();
        ClearAllLayers();

        CleanupSpriteCache(_topSpriteCache);
        CleanupSpriteCache(_leftSideCache);
        CleanupSpriteCache(_rightSideCache);

        DestroyRoot();

        _groundLayer = null;
        _buildingsLayer = null;
        _fogLayer = null;
        _highlightLayer = null;
        _hoverHighlight = null;
        _selectionHighlight = null;
        _layoutWidth = 0;
        _layoutHeight = 0;
        _hasRendered = false;

        IsInitialized = false;
        IsActive = false;

        Debug.Log("VillageIsoRenderer: Cleaned up");
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
        var layout = _villageManager.Layout;
        int buildingCount = layout?.Buildings.Count ?? 0;
        return $"VillageIsoRenderer: Active={IsActive}, HasRendered={_hasRendered}, " +
               $"{_layoutWidth}x{_layoutHeight} + {FOG_PADDING} padding, {buildingCount} buildings, " +
               $"Ground: {_groundTiles.Count}, Fog: {_fogObjects.Count}\n{_cameraController.GetDebugInfo()}";
    }

    public string GetDebugInfoAt(IsoPos pos) {
        var layout = _villageManager.Layout;
        if (layout == null) return "No layout";

        var gridPos = new Vector2Int(pos.X, pos.Y);
        if (!layout.IsInBounds(gridPos)) return $"Out of bounds: {pos}";

        var tileType = layout.GetTileType(gridPos);
        var building = layout.GetBuildingAt(gridPos);

        if (building != null && _buildingVolumes.TryGetValue(building, out var volume)) {
            return $"Pos: {pos}\nTile: {tileType}\nBuilding: {building.DisplayName}";
        }

        return $"Pos: {pos}\nTile: {tileType}";
    }

    #endregion
}