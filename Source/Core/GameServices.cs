using RPGGame.Core.Assets;
using RPGGame.Core.Combat;
using RPGGame.Core.Events;
using RPGGame.Core.Expedition;
using RPGGame.Core.Isometric;
using RPGGame.Core.Isometric.Rendering;
using RPGGame.Core.Items;
using RPGGame.Core.Metrics;
using RPGGame.Core.Save;
using RPGGame.Core.Simulation;
using RPGGame.Core.Village;

namespace RPGGame.Core;

/// <summary>
/// Static service locator providing convenient, safe access to global singletons.
/// 
/// <para>
/// Use for occasional read-only lookups in UI and systems where dependency injection
/// would be overly verbose. Heavy dependencies that are used frequently should still
/// be injected via constructor for better testability and explicit dependencies.
/// </para>
/// 
/// <example>
/// <code>
/// // In UI components (always initialized when game is running):
/// var playerGold = GameServices.Session.CurrentRun?.Character.Gold ?? 0;
/// var statProto = GameServices.Db.Get&lt;StatProto&gt;(statId);
/// 
/// // Quick lookups:
/// bool isInCombat = GameServices.Combat.IsInCombat;
/// bool isPaused = GameServices.State.IsPaused;
/// </code>
/// </example>
/// 
/// <remarks>
/// This class is initialized by GameManager during Awake() and is guaranteed to be
/// available for the entire lifetime of the game. Accessing any property before
/// initialization will throw an InvalidOperationException with a clear message.
/// </remarks>
/// </summary>
public static class GameServices {
    #region Fields

    private static GlobalDependencyResolver? s_resolver;
    private static GameSession? s_session;
    private static bool s_isInitialized;

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the service locator with the dependency resolver and session.
    /// Called by GameManager after all services are created.
    /// </summary>
    /// <param name="resolver">The global dependency resolver.</param>
    /// <param name="session">The game session instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if resolver or session is null.</exception>
    public static void Initialize(GlobalDependencyResolver resolver, GameSession session) {
        s_resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        s_session = session ?? throw new ArgumentNullException(nameof(session));
        s_isInitialized = true;
    }

    /// <summary>
    /// Resets the service locator. Only used during shutdown or testing.
    /// </summary>
    public static void Reset() {
        s_resolver = null;
        s_session = null;
        s_isInitialized = false;
    }

    private static void EnsureInitialized() {
        if (!s_isInitialized) {
            throw new InvalidOperationException(
                "GameServices has not been initialized. " +
                "This usually means you're trying to access services before GameManager.Awake() has completed. " +
                "Ensure GameManager is in the scene and has initialized properly.");
        }
    }

    #endregion

    #region Properties - Core

    /// <summary>
    /// Whether the service locator has been initialized.
    /// </summary>
    public static bool IsInitialized => s_isInitialized;

    /// <summary>
    /// The global dependency resolver for resolving any registered service.
    /// Use the typed properties below for common services.
    /// </summary>
    public static GlobalDependencyResolver Resolver {
        get {
            EnsureInitialized();
            return s_resolver!;
        }
    }

    /// <summary>
    /// The current game session. Provides access to the current run, meta progression,
    /// and coordinates all game systems.
    /// </summary>
    public static GameSession Session {
        get {
            EnsureInitialized();
            return s_session!;
        }
    }

    #endregion

    #region Properties - Data

    /// <summary>
    /// The game database containing all prototypes (items, enemies, skills, etc.).
    /// Use for looking up static game data by ID.
    /// </summary>
    public static GameDb Db {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<GameDb>();
        }
    }

    /// <summary>
    /// The game time manager for accessing current time, day/night cycle, etc.
    /// </summary>
    public static GameTime Time {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<GameTime>();
        }
    }

    #endregion

    #region Properties - Core Systems

	/// <summary>
	/// The lighting controller for day/night cycle and dynamic lights.
	/// </summary>
	public static LightController Lighting {
		get {
			EnsureInitialized();
			return s_resolver!.Resolve<LightController>();
		}
	}

    /// <summary>
    /// The main game loop. Handles tick updates and render updates.
    /// </summary>
    public static GameLoop Loop {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<GameLoop>();
        }
    }

    /// <summary>
    /// The game state manager. Handles phase transitions, pausing, etc.
    /// </summary>
    public static GameStateManager State {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<GameStateManager>();
        }
    }

    /// <summary>
    /// The isometric camera controller. Handles camera positioning, zoom, and follow.
    /// </summary>
    public static IsoCameraController Camera {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<IsoCameraController>();
        }
    }

    #endregion

    #region Properties - Renderers

    /// <summary>
    /// The village isometric renderer.
    /// </summary>
    public static VillageIsoRenderer VillageRenderer {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<VillageIsoRenderer>();
        }
    }

    /// <summary>
    /// The expedition isometric renderer.
    /// </summary>
    public static ExpeditionIsoRenderer ExpeditionRenderer {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<ExpeditionIsoRenderer>();
        }
    }

    #endregion

    #region Properties - Gameplay Managers

    /// <summary>
    /// The combat manager. Handles combat encounters, turns, and resolution.
    /// </summary>
    public static CombatManager Combat {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<CombatManager>();
        }
    }

    /// <summary>
    /// The expedition manager. Handles travel, exploration, and world map.
    /// </summary>
    public static ExpeditionManager Expedition {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<ExpeditionManager>();
        }
    }

    /// <summary>
    /// The village manager. Handles buildings, upgrades, and village state.
    /// </summary>
    public static VillageManager Village {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<VillageManager>();
        }
    }

    /// <summary>
    /// The inventory manager. Handles items, equipment, and gold.
    /// </summary>
    public static InventoryManager Inventory {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<InventoryManager>();
        }
    }

    /// <summary>
    /// The event manager. Handles story events, choices, and outcomes.
    /// </summary>
    public static EventManager Events {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<EventManager>();
        }
    }

    /// <summary>
    /// The metrics manager. Tracks statistics and achievements.
    /// </summary>
    public static MetricsManager Metrics {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<MetricsManager>();
        }
    }

    /// <summary>
    /// The save manager. Handles saving and loading game state.
    /// </summary>
    public static SaveManager Save {
        get {
            EnsureInitialized();
            return s_resolver!.Resolve<SaveManager>();
        }
    }

    
	/// <summary>
	/// The save manager. Handles saving and loading game state.
	/// </summary>
	public static AssetManager Assets {
		get {
			EnsureInitialized();
			return s_resolver!.Resolve<AssetManager>();
		}
	}

    #endregion

    #region Helper Methods

    /// <summary>
    /// Resolves a service of the specified type.
    /// Use the typed properties for common services; this is for less common ones.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    public static T Get<T>() where T : class {
        EnsureInitialized();
        return s_resolver!.Resolve<T>();
    }

    #endregion
}