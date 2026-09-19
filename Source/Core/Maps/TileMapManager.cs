using RPGGame.Core.Simulation;
using RPGGame.Core.Village;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RPGGame.Core.Maps;

/// <summary>
/// Central manager for all tilemaps in the game.
/// Creates a single Grid with child tilemap holders for different game phases.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class TilemapManager {
	#region Fields

	private readonly GameStateManager k_stateManager;

	// Root grid
	private GameObject? k_mainGridObject;
	private Grid? k_mainGrid;

	// Tilemap holders for different contexts
	private GameObject? k_villageHolder;
	private GameObject? k_expeditionHolder;
	private GameObject? k_combatHolder;

	// Village tilemaps
	private Tilemap? k_villageGroundTilemap;
	private Tilemap? k_villageBuildingTilemap;
	private Tilemap? k_villageFogTilemap;

	// Expedition tilemaps
	private Tilemap? k_expeditionGroundTilemap;
	private Tilemap? k_expeditionFeaturesTilemap;
	private Tilemap? k_expeditionFogTilemap;

	// Shared resources
	private Sprite? k_defaultTileSprite;

	#endregion

	#region Properties

	/// <summary>
	/// The main grid for all tilemaps.
	/// </summary>
	public Grid? MainGrid => k_mainGrid;

	/// <summary>
	/// Village ground tilemap.
	/// </summary>
	public Tilemap? VillageGroundTilemap => k_villageGroundTilemap;

	/// <summary>
	/// Village building tilemap.
	/// </summary>
	public Tilemap? VillageBuildingTilemap => k_villageBuildingTilemap;

	/// <summary>
	/// Village fog boundary tilemap.
	/// </summary>
	public Tilemap? VillageFogTilemap => k_villageFogTilemap;

	/// <summary>
	/// Expedition ground tilemap.
	/// </summary>
	public Tilemap? ExpeditionGroundTilemap => k_expeditionGroundTilemap;

	/// <summary>
	/// Expedition features tilemap (nodes, landmarks).
	/// </summary>
	public Tilemap? ExpeditionFeaturesTilemap => k_expeditionFeaturesTilemap;

	/// <summary>
	/// Expedition fog tilemap.
	/// </summary>
	public Tilemap? ExpeditionFogTilemap => k_expeditionFogTilemap;

	/// <summary>
	/// Whether the manager has been initialized.
	/// </summary>
	public bool IsInitialized => k_mainGrid != null;

	#endregion

	#region Constructor

	public TilemapManager(GameStateManager stateManager) {
		k_stateManager = stateManager;

		Initialize();
		k_stateManager.OnPhaseChanged += OnPhaseChanged;
		Debug.Log("TilemapManager: Initialized");
	}

	#endregion

	#region Initialization

	private void Initialize() {
		CreateMainGrid();
		LoadSharedResources();
		CreateTilemapHolders();
		SetupCamera();
	}

	private void CreateMainGrid() {
		k_mainGridObject = new GameObject("GameGrid");
		k_mainGrid = k_mainGridObject.AddComponent<Grid>();
		k_mainGrid.cellLayout = GridLayout.CellLayout.Isometric;
		k_mainGrid.cellSize = new Vector3(1f, 0.5f, 0f);
		k_mainGrid.cellGap = Vector3.zero;

		UnityEngine.Object.DontDestroyOnLoad(k_mainGridObject);

		Debug.Log("TilemapManager: Main grid created");
	}

	private void LoadSharedResources() {
		k_defaultTileSprite = UnityEngine.Resources.Load<Sprite>("Sprites/Tilemap/DefaultTile");
		if (k_defaultTileSprite == null) {
			Debug.LogWarning("TilemapManager: DefaultTile sprite not found, will use procedural tiles");
		}
	}

	private void CreateTilemapHolders() {
		if (k_mainGridObject == null) return;

		// Village holder
		k_villageHolder = new GameObject("Tilemaps_Village");
		k_villageHolder.transform.SetParent(k_mainGridObject.transform);
		CreateVillageTilemaps();

		// Expedition holder
		k_expeditionHolder = new GameObject("Tilemaps_Expedition");
		k_expeditionHolder.transform.SetParent(k_mainGridObject.transform);
		CreateExpeditionTilemaps();
		k_expeditionHolder.SetActive(false);

		// Combat holder (for later)
		k_combatHolder = new GameObject("Tilemaps_Combat");
		k_combatHolder.transform.SetParent(k_mainGridObject.transform);
		k_combatHolder.SetActive(false);

		Debug.Log("TilemapManager: Tilemap holders created");
	}

	private void CreateVillageTilemaps() {
		if (k_villageHolder == null) return;

		// Ground layer
		var groundObj = new GameObject("Village_Ground");
		groundObj.transform.SetParent(k_villageHolder.transform);
		k_villageGroundTilemap = groundObj.AddComponent<Tilemap>();
		var groundRenderer = groundObj.AddComponent<TilemapRenderer>();
		groundRenderer.sortingOrder = 0;
		groundRenderer.mode = TilemapRenderer.Mode.Individual;

		// Building layer
		var buildingObj = new GameObject("Village_Buildings");
		buildingObj.transform.SetParent(k_villageHolder.transform);
		k_villageBuildingTilemap = buildingObj.AddComponent<Tilemap>();
		var buildingRenderer = buildingObj.AddComponent<TilemapRenderer>();
		buildingRenderer.sortingOrder = 1;
		buildingRenderer.mode = TilemapRenderer.Mode.Individual;

		// Fog layer
		var fogObj = new GameObject("Village_Fog");
		fogObj.transform.SetParent(k_villageHolder.transform);
		k_villageFogTilemap = fogObj.AddComponent<Tilemap>();
		var fogRenderer = fogObj.AddComponent<TilemapRenderer>();
		fogRenderer.sortingOrder = 2;
		fogRenderer.mode = TilemapRenderer.Mode.Individual;

		Debug.Log("TilemapManager: Village tilemaps created");
	}

	private void CreateExpeditionTilemaps() {
		if (k_expeditionHolder == null) return;

		// Ground layer
		var groundObj = new GameObject("Expedition_Ground");
		groundObj.transform.SetParent(k_expeditionHolder.transform);
		k_expeditionGroundTilemap = groundObj.AddComponent<Tilemap>();
		var groundRenderer = groundObj.AddComponent<TilemapRenderer>();
		groundRenderer.sortingOrder = 0;
		groundRenderer.mode = TilemapRenderer.Mode.Individual;

		// Features layer (nodes, landmarks, resources)
		var featuresObj = new GameObject("Expedition_Features");
		featuresObj.transform.SetParent(k_expeditionHolder.transform);
		k_expeditionFeaturesTilemap = featuresObj.AddComponent<Tilemap>();
		var featuresRenderer = featuresObj.AddComponent<TilemapRenderer>();
		featuresRenderer.sortingOrder = 1;
		featuresRenderer.mode = TilemapRenderer.Mode.Individual;

		// Fog layer
		var fogObj = new GameObject("Expedition_Fog");
		fogObj.transform.SetParent(k_expeditionHolder.transform);
		k_expeditionFogTilemap = fogObj.AddComponent<Tilemap>();
		var fogRenderer = fogObj.AddComponent<TilemapRenderer>();
		fogRenderer.sortingOrder = 2;
		fogRenderer.mode = TilemapRenderer.Mode.Individual;

		Debug.Log("TilemapManager: Expedition tilemaps created");
	}

	private void SetupCamera() {
		var mainCamera = Camera.main;
		if (mainCamera == null) {
			Debug.LogWarning("TilemapManager: No main camera found");
			return;
		}

		mainCamera.orthographic = true;
		// Initial position will be set by the renderer when it renders
	}

	#endregion

	#region Phase Management

	private void OnPhaseChanged(GamePhase oldPhase, GamePhase newPhase) {
		// Show/hide tilemap holders based on phase
		bool showVillage = newPhase == GamePhase.Village;
		bool showExpedition = newPhase == GamePhase.Expedition || 
			newPhase == GamePhase.Event ||
			newPhase == GamePhase.Camp ||
			newPhase == GamePhase.Combat; // Keep visible as backdrop during combat
		bool showCombat = newPhase == GamePhase.Combat;

		k_villageHolder?.SetActive(showVillage);
		k_expeditionHolder?.SetActive(showExpedition);
		k_combatHolder?.SetActive(showCombat);

		Debug.Log($"TilemapManager: Phase changed to {newPhase}, Village={showVillage}, Expedition={showExpedition}, Combat={showCombat}");
	}

	#endregion

	#region Camera Control

	/// <summary>
	/// Centers the camera on the village.
	/// </summary>
	public void CenterCameraOnVillage(VillageLayout layout) {
		var mainCamera = Camera.main;
		if (mainCamera == null || k_villageGroundTilemap == null) return;

		// Calculate center tile
		int centerX = layout.CenterX;
		int centerY = layout.CenterY;

		// Convert to world position
		Vector3 centerWorld = k_villageGroundTilemap.GetCellCenterWorld(new Vector3Int(centerX, centerY, 0));

		// Position camera
		mainCamera.transform.position = new Vector3(centerWorld.x, centerWorld.y, -10f);

		// Calculate orthographic size to fit the village
		float worldWidth = layout.Width * k_mainGrid!.cellSize.x;
		float worldHeight = layout.Height * k_mainGrid.cellSize.y;
		float aspectRatio = (float)Screen.width / Screen.height;
		float verticalSize = worldHeight * 0.6f;
		float horizontalSize = worldWidth * 0.6f / aspectRatio;

		mainCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

		Debug.Log($"TilemapManager: Camera centered on village at {centerWorld}, ortho size: {mainCamera.orthographicSize}");
	}

	/// <summary>
	/// Centers the camera on a world position (for expedition).
	/// </summary>
	public void CenterCameraOnPosition(Vector2 worldPosition, float orthographicSize = 5f) {
		var mainCamera = Camera.main;
		if (mainCamera == null) return;

		mainCamera.transform.position = new Vector3(worldPosition.x, worldPosition.y, -10f);
		mainCamera.orthographicSize = orthographicSize;
	}

	/// <summary>
	/// Converts a tile position to world position on the expedition tilemap.
	/// </summary>
	public Vector3 ExpeditionTileToWorld(Vector2Int tilePosition) {
		if (k_expeditionGroundTilemap == null) {
			return new Vector3(tilePosition.x, tilePosition.y, 0);
		}
		return k_expeditionGroundTilemap.GetCellCenterWorld(new Vector3Int(tilePosition.x, tilePosition.y, 0));
	}

	#endregion

	#region Utility

	/// <summary>
	/// Creates an isometric diamond tile sprite with smooth anti-aliased edges.
	/// </summary>
	public Sprite CreateIsometricTileSprite(Color color, int width = 256, int height = 128) {
		// Use higher resolution for smoother edges
		int texWidth = width * 2;
		int texHeight = height * 2;
	
		var texture = new Texture2D(texWidth, texHeight) {
			filterMode = FilterMode.Bilinear,  // Smooth filtering instead of Point
			wrapMode = TextureWrapMode.Clamp
		};

		var pixels = new Color[texWidth * texHeight];
		for (int i = 0; i < pixels.Length; i++) {
			pixels[i] = Color.clear;
		}

		int halfWidth = texWidth / 2;
		int halfHeight = texHeight / 2;

		// Draw the diamond with anti-aliased edges
		for (int y = 0; y < texHeight; y++) {
			for (int x = 0; x < texWidth; x++) {
				// Calculate signed distance from the diamond edge
				float nx = (float)(x - halfWidth) / halfWidth;  // -1 to 1
				float ny = (float)(y - halfHeight) / halfHeight; // -1 to 1
			
				// Diamond shape: |x| + |y| <= 1
				float dist = Mathf.Abs(nx) + Mathf.Abs(ny);
			
				if (dist <= 0.95f) {
					// Fully inside
					pixels[y * texWidth + x] = color;
				} else if (dist <= 1.05f) {
					// Edge - anti-alias with smooth falloff
					float alpha = 1f - ((dist - 0.95f) / 0.1f);
					alpha = Mathf.Clamp01(alpha);
					pixels[y * texWidth + x] = new Color(color.r, color.g, color.b, color.a * alpha);
				}
				// else: outside, stays clear
			}
		}

		texture.SetPixels(pixels);
		texture.Apply();

		return Sprite.Create(
				texture,
				new Rect(0, 0, texWidth, texHeight),
				new Vector2(0.5f, 0.5f),
				128f // Higher PPU for the higher resolution texture
			);
	}

	/// <summary>
	/// Converts screen position to tile position on the village tilemap.
	/// </summary>
	public Vector2Int? ScreenToVillageTile(Vector2 screenPosition) {
		if (k_villageGroundTilemap == null) return null;

		var mainCamera = Camera.main;
		if (mainCamera == null) return null;

		var worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
		var cellPos = k_villageGroundTilemap.WorldToCell(worldPos);

		return new Vector2Int(cellPos.x, cellPos.y);
	}

	/// <summary>
	/// Converts screen position to tile position on the expedition tilemap.
	/// </summary>
	public Vector2Int? ScreenToExpeditionTile(Vector2 screenPosition) {
		if (k_expeditionGroundTilemap == null) return null;

		var mainCamera = Camera.main;
		if (mainCamera == null) return null;

		var worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
		var cellPos = k_expeditionGroundTilemap.WorldToCell(worldPos);

		return new Vector2Int(cellPos.x, cellPos.y);
	}

	/// <summary>
	/// Clears all expedition tilemaps.
	/// </summary>
	public void ClearExpeditionTilemaps() {
		k_expeditionGroundTilemap?.ClearAllTiles();
		k_expeditionFeaturesTilemap?.ClearAllTiles();
		k_expeditionFogTilemap?.ClearAllTiles();
	}

	#endregion

	#region Cleanup

	public void Cleanup() {
		k_stateManager.OnPhaseChanged -= OnPhaseChanged;

		if (k_mainGridObject != null) {
			UnityEngine.Object.Destroy(k_mainGridObject);
		}

		Debug.Log("TilemapManager: Cleaned up");
	}

	#endregion
}