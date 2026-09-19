#nullable disable

using System.Reflection;
using RPGGame.Core.Assets;
using RPGGame.Core.Combat;
using RPGGame.Core.Dungeons;
using RPGGame.Core.Events;
using RPGGame.Core.Expedition;
using RPGGame.Core.Input;
using RPGGame.Core.Isometric.Rendering;
using RPGGame.Core.Items;
using RPGGame.Core.Maps;
using RPGGame.Core.Metrics;
using RPGGame.Core.Rewards;
using RPGGame.Core.Simulation;
using RPGGame.Core.Village;
using RPGGame.UI;
using RPGGame.UI.Character;
using RPGGame.UI.Components;
using RPGGame.UI.Screens;
using RPGGame.UI.Styles;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace RPGGame.Core;

public class GameInitializer {
	private readonly GameDb k_gameDb;
	private readonly UiManager k_uiManager;
	private readonly GameLoop k_gameLoop;
	private readonly GameStateManager k_stateManager;
	private readonly GameSession k_session;
	private readonly VillageManager k_villageManager;
	private readonly TilemapManager k_tilemapManager;
	private readonly ExpeditionManager k_expeditionManager;
	private readonly CombatManager k_combatManager;
	private readonly EventManager k_eventManager;
	private readonly DungeonManager k_dungeonManager;
	private readonly MetricsManager k_metrics;
	private readonly InventoryManager k_inventoryManager;
	private readonly RewardManager k_rewardManager;
	private readonly AssetManager k_assetManager;
	private readonly IsoCameraController k_isoCameraController;
	private readonly LightController k_lightController;

	// Isometric renderers
	private readonly VillageIsoRenderer k_villageIsoRenderer;
	private readonly ExpeditionIsoRenderer k_expeditionIsoRenderer;

	private UIDocument k_gameUiDocument;
	private MainMenuScreen k_mainMenuScreen;
	private GameplayScreen k_gameplayScreen;
	private VillageScreen k_villageScreen;
	private ExpeditionScreen k_expeditionScreen;
	private CombatScreen k_combatScreen;
	private RunSummaryScreen k_runSummaryScreen;
	private CharacterScreen k_characterScreen;
	private CharacterCreatorScreen k_characterCreatorScreen;
	private GameInputManager k_inputManager;

	public GameDb GameDb => k_gameDb;
	public GameInputManager InputManager => k_inputManager;
	public TilemapManager TilemapManager => k_tilemapManager;
	public CharacterScreen CharacterScreen => k_characterScreen;
	public AssetManager Assets => k_assetManager;

	public GameInitializer(
		GameDb gameDb,
		UiManager uiManager,
		GameLoop gameLoop,
		GameStateManager stateManager,
		GameSession session,
		VillageManager villageManager,
		TilemapManager tilemapManager,
		VillageIsoRenderer villageIsoRenderer,
		ExpeditionManager expeditionManager,
		ExpeditionIsoRenderer expeditionIsoRenderer,
		CombatManager combatManager,
		EventManager eventManager,
		DungeonManager dungeonManager,
		MetricsManager metrics,
		InventoryManager inventoryManager,
		RewardManager rewardManager,
		AssetManager assetManager,
		IsoCameraController isoCameraController,
		LightController lightController
	) {
		k_gameDb = gameDb;
		k_uiManager = uiManager;
		k_gameLoop = gameLoop;
		k_stateManager = stateManager;
		k_session = session;
		k_villageManager = villageManager;
		k_tilemapManager = tilemapManager;
		k_expeditionManager = expeditionManager;
		k_combatManager = combatManager;
		k_eventManager = eventManager;
		k_dungeonManager = dungeonManager;
		k_metrics = metrics;
		k_inventoryManager = inventoryManager;
		k_rewardManager = rewardManager;
		k_assetManager = assetManager;
		k_isoCameraController = isoCameraController;
		k_lightController = lightController;

		// Isometric renderers
		k_villageIsoRenderer = villageIsoRenderer;
		k_expeditionIsoRenderer = expeditionIsoRenderer;

		InitializeAssets();
		InitializeGame();
		InitializeInputSystem();
		CreateAndInitializeGameUiDocument(k_uiManager.UiBuilder);
		InitializeScreens();
		InitializeRenderingSystems();
	}

	private void InitializeAssets() {
		Debug.Log("InitializeAssets: START");
		
		k_assetManager.Initialize();
		k_assetManager.PreloadCommonAssets();
		k_assetManager.PreloadAllTerrainGraphics();
		
		Debug.Log("InitializeAssets: COMPLETE");
	}



	public void InitializeGame() {
		IEnumerable<Type> coreDataTypes = Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(predicate: type => typeof(ICoreData).IsAssignableFrom(c: type) && !type.IsInterface && !type.IsAbstract);

		foreach (Type type in coreDataTypes) {
			try {
				ICoreData coreDataInstance = (ICoreData)Activator.CreateInstance(type: type);
				coreDataInstance.GameData(gameDatabase: k_gameDb);
				Debug.Log(message: type.Name + " data successfully registered.");
			} catch (Exception ex) {
				Debug.LogError(message: $"Failed to register data for {type.Name}: {ex.Message}");
			}
		}

		k_gameDb.LockAndInitialize();
		Debug.Log(message: "All game data successfully registered.");
	}

	private void InitializeInputSystem() {
		Debug.Log("InitializeInputSystem: START");

		k_inputManager = GameInputManager.Create(k_stateManager);
		Debug.Log("InitializeInputSystem: GameInputManager created");

		RegisterInputContexts();
		SubscribeToGlobalInputEvents();

		Debug.Log("InitializeInputSystem: COMPLETE");
	}

	private void RegisterInputContexts() {
		// Village phase - uses isometric renderer
		var villageContext = new VillageInputContext(k_villageIsoRenderer, k_isoCameraController, k_stateManager);
		k_inputManager.RegisterContext(GamePhase.Village, villageContext);
		Debug.Log("InitializeInputSystem: VillageInputContext registered");

		// Expedition phase - uses isometric renderer
		var expeditionContext = new ExpeditionInputContext(
			k_expeditionManager, 
			k_expeditionIsoRenderer, 
			k_isoCameraController, 
			k_stateManager
		);
		k_inputManager.RegisterContext(GamePhase.Expedition, expeditionContext);
		Debug.Log("InitializeInputSystem: ExpeditionInputContext registered");

		// Event phase (uses same context as expedition)
		k_inputManager.RegisterContext(GamePhase.Event, expeditionContext);
		Debug.Log("InitializeInputSystem: Event phase using ExpeditionInputContext");

		// Combat phase
		var combatContext = new CombatInputContext(k_combatManager, k_stateManager);
		k_inputManager.RegisterContext(GamePhase.Combat, combatContext);
		Debug.Log("InitializeInputSystem: CombatInputContext registered");

		// Camp phase (uses expedition context)
		k_inputManager.RegisterContext(GamePhase.Camp, expeditionContext);
		Debug.Log("InitializeInputSystem: Camp phase using ExpeditionInputContext");
	}

	private void SubscribeToGlobalInputEvents() {
		k_inputManager.OnPause += () => {
			if (k_stateManager.CurrentPhase == GamePhase.Paused) {
				k_stateManager.ResumeGame();
			} else if (k_stateManager.CurrentPhase.IsGameplayPhase()) {
				k_stateManager.PauseGame();
			}
		};

		k_inputManager.OnOpenInventory += () => {
			if (k_stateManager.CurrentPhase.IsGameplayPhase()) {
				k_characterScreen?.Toggle();
			}
		};

		k_inputManager.OnOpenCharacter += () => {
			if (k_stateManager.CurrentPhase.IsGameplayPhase()) {
				k_characterScreen?.Toggle();
			}
		};

		k_inputManager.OnOpenMap += () => {
			if (k_stateManager.CurrentPhase.IsGameplayPhase()) {
				GameToast.Show("Map (coming soon)", ToastType.Info);
			}
		};

		k_inputManager.OnInputDeviceChanged += (isGamepad) => {
			Debug.Log($"Input device changed: {(isGamepad ? "Gamepad" : "Mouse/Keyboard")}");
		};
	}

	private void InitializeScreens() {
		Debug.Log("InitializeScreens: START");

		k_stateManager.OnPhaseChanged += OnPhaseChanged;
		Debug.Log("InitializeScreens: Phase change subscribed");

		// Create main menu screen
		k_mainMenuScreen = new MainMenuScreen(
			k_stateManager,
			k_uiManager,
			k_session,
			k_gameDb
		);
		k_uiManager.UiBuilder.AddToUIOverlay(k_mainMenuScreen);
		k_uiManager.RegisterWindow("mainMenu", k_mainMenuScreen);
		Debug.Log("InitializeScreens: MainMenuScreen created");

		// Create gameplay screen (shared HUD for all gameplay phases)
		k_gameplayScreen = new GameplayScreen(
			k_stateManager,
			k_uiManager,
			k_gameLoop,
			k_gameDb
		);
		k_uiManager.UiBuilder.AddToUIOverlay(k_gameplayScreen);
		k_uiManager.RegisterWindow("gameplay", k_gameplayScreen);
		Debug.Log("InitializeScreens: GameplayScreen created");

		// Create village screen
		k_villageScreen = new VillageScreen(
			k_stateManager,
			k_villageManager,
			k_villageIsoRenderer,
			k_session,
			k_uiManager
		);
		k_uiManager.UiBuilder.AddToUIOverlay(k_villageScreen);
		k_uiManager.RegisterWindow("village", k_villageScreen);
		Debug.Log("InitializeScreens: VillageScreen created");

		// Create expedition screen
		k_expeditionScreen = new ExpeditionScreen(
			k_stateManager,
			k_expeditionManager,
			k_eventManager,
			k_dungeonManager,
			k_session,
			k_gameDb,
			k_uiManager,
			k_inventoryManager
		);
		k_uiManager.UiBuilder.AddToUIOverlay(k_expeditionScreen);
		k_uiManager.RegisterWindow("expedition", k_expeditionScreen);
		Debug.Log("InitializeScreens: ExpeditionScreen created");

		// Wire up reward manager
		k_eventManager.SetRewardManager(k_rewardManager);
		k_expeditionScreen.SetRewardManager(k_rewardManager);

		// Create combat screen
		k_combatScreen = new CombatScreen(
			k_stateManager,
			k_combatManager,
			k_session,
			k_uiManager,
			k_gameDb
		);
		k_uiManager.UiBuilder.AddToUIOverlay(k_combatScreen);
		k_uiManager.RegisterWindow("combat", k_combatScreen);
		Debug.Log("InitializeScreens: CombatScreen created");

		// Create run summary screen
		k_runSummaryScreen = new RunSummaryScreen(
			k_stateManager,
			k_session,
			k_metrics
		);
		k_uiManager.UiBuilder.AddToUIOverlay(k_runSummaryScreen);
		k_uiManager.RegisterWindow("runSummary", k_runSummaryScreen);
		Debug.Log("InitializeScreens: RunSummaryScreen created");

		// Create character screen
		k_characterScreen = new CharacterScreen(k_gameDb, k_session, k_inventoryManager);
		k_uiManager.UiBuilder.AddToUIOverlay(k_characterScreen);
		k_uiManager.RegisterWindow("character", k_characterScreen);
		k_characterScreen.style.display = DisplayStyle.None;
		Debug.Log("InitializeScreens: CharacterScreen created");

		// Create character creator screen
		k_characterCreatorScreen = new CharacterCreatorScreen(
			k_stateManager,
			k_session,
			k_gameDb
		);
		k_uiManager.UiBuilder.AddToUIOverlay(k_characterCreatorScreen);
		k_uiManager.RegisterWindow("characterCreator", k_characterCreatorScreen);
		k_characterCreatorScreen.style.display = DisplayStyle.None;
		Debug.Log("InitializeScreens: CharacterCreatorScreen created");

		// Initialize toast system
		GameToast.Initialize(k_mainMenuScreen);
		Debug.Log("InitializeScreens: GameToast initialized");

		// Initialize the game session
		k_session.Initialize();
		Debug.Log("InitializeScreens: GameSession initialized");

		Debug.Log("Game screens initialized. Starting at Main Menu.");
	}

	/// <summary>
	/// Initializes all isometric rendering systems.
	/// </summary>
	private void InitializeRenderingSystems() {
		Debug.Log("InitializeRenderingSystems: START");

		// Initialize lighting system FIRST
		InitializeLightingSystem();

		// Initialize village isometric renderer (hidden by default)
		k_villageIsoRenderer.Initialize();
		k_villageIsoRenderer.SetVisible(false);
		k_villageIsoRenderer.SetLightController(k_lightController);
		Debug.Log("InitializeRenderingSystems: VillageIsoRenderer initialized (hidden)");

		// Initialize expedition isometric renderer (hidden by default)
		k_expeditionIsoRenderer.Initialize();
		k_expeditionIsoRenderer.SetVisible(false);
		k_expeditionIsoRenderer.SetLightController(k_lightController);
		Debug.Log("InitializeRenderingSystems: ExpeditionIsoRenderer initialized (hidden)");

		Debug.Log("InitializeRenderingSystems: COMPLETE");
	}

	private void InitializeLightingSystem() {
		Debug.Log("InitializeLightingSystem: START");

		// Create container for dynamic light GameObjects
		var lightContainer = new GameObject("LightContainer");
		UnityEngine.Object.DontDestroyOnLoad(lightContainer);
		k_lightController.SetLightContainer(lightContainer);

		// Try to find an existing global light in the scene first
		Light2D existingGlobalLight = null;
		foreach (var light in UnityEngine.Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None)) {
			if (light.lightType == Light2D.LightType.Global) {
				existingGlobalLight = light;
				Debug.Log($"InitializeLightingSystem: Found existing global light '{light.gameObject.name}'");
				break;
			}
		}

		Light2D globalLight;
		if (existingGlobalLight != null) {
			globalLight = existingGlobalLight;
		} else {
			var globalLightGO = new GameObject("GlobalLight2D");
			globalLightGO.transform.SetParent(lightContainer.transform);
            
			globalLight = globalLightGO.AddComponent<Light2D>();
			globalLight.lightType = Light2D.LightType.Global;
			globalLight.intensity = 1.0f;
			globalLight.color = Color.white;
            
			Debug.Log("InitializeLightingSystem: Created new global light");
		}

		k_lightController.SetGlobalLight(globalLight);

		Debug.Log("InitializeLightingSystem: COMPLETE");
	}

	private void OnPhaseChanged(GamePhase oldPhase, GamePhase newPhase) {
		// Close character screen when changing major phases
		if (oldPhase.IsGameplayPhase() && !newPhase.IsGameplayPhase()) {
			k_characterScreen?.Hide();
		}

		// === RENDERER VISIBILITY MANAGEMENT ===
		// Deactivate renderers that are no longer needed
		// Activate renderers that are needed for the new phase
		
		bool needsVillageRenderer = newPhase == GamePhase.Village;
		bool needsExpeditionRenderer = newPhase == GamePhase.Expedition || 
		                               newPhase == GamePhase.Event || 
		                               newPhase == GamePhase.Camp;

		// Handle village renderer
		if (needsVillageRenderer && !k_villageIsoRenderer.IsActive) {
			k_expeditionIsoRenderer.Deactivate();
			k_villageIsoRenderer.Activate();
		} else if (!needsVillageRenderer && k_villageIsoRenderer.IsActive) {
			k_villageIsoRenderer.Deactivate();
		}

		// Handle expedition renderer
		if (needsExpeditionRenderer && !k_expeditionIsoRenderer.IsActive) {
			k_villageIsoRenderer.Deactivate();
			k_expeditionIsoRenderer.Activate();
			//k_expeditionIsoRenderer.DEBUG___TestRenderFullChunkAroundOrigin();
		} else if (!needsExpeditionRenderer && k_expeditionIsoRenderer.IsActive) {
			k_expeditionIsoRenderer.Deactivate();
		}

		// === UI VISIBILITY MANAGEMENT ===
		switch (newPhase) {
			case GamePhase.MainMenu:
				k_mainMenuScreen.style.display = DisplayStyle.Flex;
				k_characterCreatorScreen.style.display = DisplayStyle.None;
				k_gameplayScreen.style.display = DisplayStyle.None;
				k_villageScreen.style.display = DisplayStyle.None;
				k_expeditionScreen.style.display = DisplayStyle.None;
				k_combatScreen.style.display = DisplayStyle.None;
				k_runSummaryScreen.style.display = DisplayStyle.None;
				k_mainMenuScreen.Refresh();
				break;

			case GamePhase.NewGame:
				k_mainMenuScreen.style.display = DisplayStyle.None;
				k_characterCreatorScreen.style.display = DisplayStyle.Flex;
				k_gameplayScreen.style.display = DisplayStyle.None;
				k_villageScreen.style.display = DisplayStyle.None;
				k_expeditionScreen.style.display = DisplayStyle.None;
				k_combatScreen.style.display = DisplayStyle.None;
				k_runSummaryScreen.style.display = DisplayStyle.None;
				break;

			case GamePhase.Village:
				k_mainMenuScreen.style.display = DisplayStyle.None;
				k_characterCreatorScreen.style.display = DisplayStyle.None;
				k_gameplayScreen.style.display = DisplayStyle.Flex;
				k_villageScreen.style.display = DisplayStyle.Flex;
				k_expeditionScreen.style.display = DisplayStyle.None;
				k_combatScreen.style.display = DisplayStyle.None;
				k_runSummaryScreen.style.display = DisplayStyle.None;
				break;

			case GamePhase.Expedition:
			case GamePhase.Event:
			case GamePhase.Camp:
				k_mainMenuScreen.style.display = DisplayStyle.None;
				k_characterCreatorScreen.style.display = DisplayStyle.None;
				k_gameplayScreen.style.display = DisplayStyle.None;
				k_villageScreen.style.display = DisplayStyle.None;
				k_expeditionScreen.style.display = DisplayStyle.Flex;
				k_combatScreen.style.display = DisplayStyle.None;
				k_runSummaryScreen.style.display = DisplayStyle.None;
				break;

			case GamePhase.Combat:
				k_mainMenuScreen.style.display = DisplayStyle.None;
				k_characterCreatorScreen.style.display = DisplayStyle.None;
				k_gameplayScreen.style.display = DisplayStyle.None;
				k_villageScreen.style.display = DisplayStyle.None;
				k_expeditionScreen.style.display = DisplayStyle.None;
				k_combatScreen.style.display = DisplayStyle.Flex;
				k_runSummaryScreen.style.display = DisplayStyle.None;
				k_characterScreen?.Hide();
				break;

			case GamePhase.RunSummary:
			case GamePhase.TimeRewind:
				k_mainMenuScreen.style.display = DisplayStyle.None;
				k_characterCreatorScreen.style.display = DisplayStyle.None;
				k_gameplayScreen.style.display = DisplayStyle.None;
				k_villageScreen.style.display = DisplayStyle.None;
				k_expeditionScreen.style.display = DisplayStyle.None;
				k_combatScreen.style.display = DisplayStyle.None;
				k_runSummaryScreen.style.display = DisplayStyle.Flex;
				break;

			case GamePhase.LevelUp:
				// LevelUp is handled within ExpeditionScreen's camp panel
				break;

			case GamePhase.Paused:
				// Keep current screens visible, just show pause overlay
				break;

			case GamePhase.Loading:
			case GamePhase.Initializing:
				break;
		}
	}

	private void CreateAndInitializeGameUiDocument(UiBuilder uiBuilder) {
		GameObject uiDocumentObject = new GameObject("GameUIDocument");
		k_gameUiDocument = uiDocumentObject.AddComponent<UIDocument>();

		PanelSettings panelSettings = UnityEngine.Resources.Load<PanelSettings>("DefaultPanelSettings");
		if (panelSettings != null) {
			Debug.Log("DefaultPanelSettings successfully loaded from Resources.");
			k_gameUiDocument.panelSettings = panelSettings;
		} else {
			Debug.LogError("Failed to load DefaultPanelSettings from Resources!");
		}

		ThemeStyleSheet defaultTheme = UnityEngine.Resources.Load<ThemeStyleSheet>("DefaultThemeStyle");
		if (defaultTheme != null) {
			Debug.Log("DefaultThemeStyle successfully loaded from Resources.");
			if (panelSettings != null) {
				panelSettings.themeStyleSheet = defaultTheme;
			}
		} else {
			Debug.LogError("Failed to load DefaultThemeStyle from Resources!");
		}

		StyleSheet styleSheet = UnityEngine.Resources.Load<StyleSheet>("DefaultStyleSheet");
		if (styleSheet != null) {
			Debug.Log("DefaultStyleSheet successfully loaded from Resources.");
			k_gameUiDocument.rootVisualElement.styleSheets.Add(styleSheet);
		} else {
			Debug.LogError("Failed to load DefaultStyleSheet from Resources!");
		}

		VisualElement rootUi = k_gameUiDocument.rootVisualElement;
		
		// Apply default typography and font settings to root element
		// This cascades to ALL children as the baseline style
		ApplyDefaultTypographyToRoot(rootUi);

		VisualElement gameUi = uiBuilder.CreateGameUI();
		rootUi.Add(gameUi);

		// Set the UI document on input manager for pointer-over-UI detection
		k_inputManager.SetPrimaryUIDocument(k_gameUiDocument);

		Debug.Log("Game UI successfully initialized.");
	}

	// Update the ApplyDefaultTypographyToRoot method:

/// <summary>
/// Applies default typography settings from GameTheme to the root element.
/// This ensures all UI elements have reasonable 4K-appropriate defaults
/// before any specific styles are applied.
/// </summary>
private void ApplyDefaultTypographyToRoot(VisualElement root) {
	var theme = GameTheme.Current;
	var typography = theme.Typography;
	var colors = theme.Colors;

	// Try to load the font from the configured path
	FontAsset fontAsset = LoadDefaultFontAsset(typography.MonospaceFontAssetPath);
	
	if (fontAsset != null) {
		root.style.unityFontDefinition = new StyleFontDefinition(fontAsset);
		Debug.Log($"Default font applied to root element: {fontAsset.name}");
	} else {
		Debug.LogError("CRITICAL: No font asset could be loaded! UI text will not render correctly.");
		Debug.LogError($"Attempted to load from: Resources/{typography.MonospaceFontAssetPath}");
		Debug.LogError("Please ensure your FontAsset is in Assets/Resources/ folder.");
	}

	// Apply BodyMedium as the default text style (reasonable for most UI text)
	var defaultTextStyle = typography.BodyMedium;
	root.style.fontSize = defaultTextStyle.FontSize;
	root.style.letterSpacing = defaultTextStyle.LetterSpacing;
	root.style.unityFontStyleAndWeight = defaultTextStyle.FontWeight switch {
		FontWeight.SemiBold or FontWeight.Bold => FontStyle.Bold,
		_ => FontStyle.Normal
	};

	// Set default text color
	root.style.color = colors.TextPrimary;

	Debug.Log($"Default typography applied: Size={defaultTextStyle.FontSize}px");
}

/// <summary>
/// Loads the default font asset, trying multiple paths.
/// </summary>
private FontAsset LoadDefaultFontAsset(string primaryPath) {
	// Try the primary configured path first
	var fontAsset = UnityEngine.Resources.Load<FontAsset>(primaryPath);
	if (fontAsset != null) {
		Debug.Log($"Font loaded from primary path: {primaryPath}");
		return fontAsset;
	}
	Debug.LogWarning($"Font not found at primary path: {primaryPath}");

	// Try alternative paths (monospace slot: JetBrains Mono replaces the
	// WhiteRabbit font used in the personal copy - see README)
	string[] alternativePaths = [
		"Fonts/Raw/Mono/Mono SDF",
		"Fonts/Mono SDF",
		"Mono SDF",
		"MonoSDF"
	];

	foreach (var path in alternativePaths) {
		fontAsset = UnityEngine.Resources.Load<FontAsset>(path);
		if (fontAsset != null) {
			Debug.Log($"Font loaded from alternative path: {path}");
			return fontAsset;
		}
	}

	// Try to load ANY font from common Unity locations
	fontAsset = UnityEngine.Resources.Load<FontAsset>("Fonts/LiberationSans SDF");
	if (fontAsset != null) {
		Debug.LogWarning("Using fallback font: LiberationSans SDF");
		return fontAsset;
	}

	return null;
}

	#region Development Helpers

	public void OpenUIShowcase() {
		k_uiManager.DEBUG_OpenUIShowcase();
	}

	public void OpenCharacterScreen() {
		k_characterScreen?.Open();
	}

	#endregion
}