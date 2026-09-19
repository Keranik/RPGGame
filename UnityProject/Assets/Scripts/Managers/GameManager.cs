using System;
using RPGGame.Core;
using RPGGame.Core.Save;
using RPGGame.Core.Simulation;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        private GlobalDependencyDatabase k_container;
        private GlobalDependencyResolver k_resolver;
        private GameLoop k_gameLoop;
        private GameStateManager k_stateManager;
        private GameSession k_session;
        private GameOrchestrator k_orchestrator;
        private bool k_isShuttingDown;

        public static GameManager Instance { get; private set; }
        public GlobalDependencyResolver Resolver => k_resolver;
        public GameSession Session => k_session;
        public GameOrchestrator Orchestrator => k_orchestrator;

        void Awake() {
            Debug.Log("GameManager: Awake ENTRY");

            // Singleton setup
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("GameManager: Building dependency container...");

            // Build the DI container for all the managers
            k_container = new GlobalDependencyDatabase();
            var coreAssembly = typeof(GameDb).Assembly;
            k_container.RegisterAssemblyTypes(coreAssembly);
            
            Debug.Log("GameManager: Assembly types registered");

            // Build the resolver
            k_resolver = k_container.Build();
            
            Debug.Log("GameManager: Resolver built");

            // Get the managers we need
            k_gameLoop = k_resolver.Resolve<GameLoop>();
            k_stateManager = k_resolver.Resolve<GameStateManager>();
            k_session = k_resolver.Resolve<GameSession>();
			Debug.Log("GameManager: Core services resolved from DI container");

            // Manually create GameOrchestrator
            Debug.Log("GameManager: Creating GameOrchestrator...");
            k_orchestrator = new GameOrchestrator(
                k_session,
                k_stateManager,
                k_resolver.Resolve<RPGGame.Core.Village.VillageManager>(),
                k_resolver.Resolve<RPGGame.Core.Expedition.ExpeditionManager>(),
                k_resolver.Resolve<RPGGame.Core.Combat.CombatManager>(),
                k_resolver.Resolve<RPGGame.Core.Events.EventManager>(),
                k_resolver.Resolve<RPGGame.Core.Items.InventoryManager>(),
                k_resolver.Resolve<RPGGame.Core.Metrics.MetricsManager>()
            );
            Debug.Log("GameManager: GameOrchestrator created");

            // Initialize the static service locator for convenient access throughout the codebase
            GameServices.Initialize(k_resolver, k_session);
            Debug.Log("GameManager: GameServices initialized");

            Debug.Log("GameManager: Awake complete, now initializing...");

            // Initialize the game
            try {
                Debug.Log("GameManager: Resolving GameInitializer...");
                var initializer = k_resolver.Resolve<GameInitializer>();
                Debug.Log($"GameManager: Initialization complete. Phase: {k_stateManager.CurrentPhase}");
            }
            catch (Exception ex) {
                Debug.LogError($"GameManager: Initialization FAILED: {ex}");
            }
        }

        void Update() {
             // Don't process updates during shutdown
            if (k_isShuttingDown) {
                return;
            }

            if (k_gameLoop != null) {
                k_gameLoop.Tick(Time.deltaTime);
                k_gameLoop.RenderUpdate();
            }
        }

        void OnApplicationPause(bool pauseStatus) {
            if (pauseStatus && k_session?.CurrentRun != null) {
                _ = k_session.SaveGameAsync(SaveManager.AUTOSAVE_SLOT);
            }
        }

        void OnApplicationQuit() {
            k_isShuttingDown = true;  // Set flag immediately
            
            try {
                k_session?.Shutdown();
                GameServices.Reset(); // Clean up the service locator
            }
            catch (Exception ex) {
                Debug.LogWarning($"GameManager: Exception during shutdown: {ex.Message}");
            }
            
            Debug.Log("GameManager: Application quit");
        }

        #region Public API

        public T GetService<T>() where T : class {
            return k_resolver.Resolve<T>();
        }

        public void PauseGame() => k_stateManager?.PauseGame();
        public void ResumeGame() => k_stateManager?.ResumeGame();
        public void GoToMainMenu() => k_stateManager?.GoToMainMenu();
        public void ExitGame() => k_stateManager?.ExitGame();

        #endregion
    }
}