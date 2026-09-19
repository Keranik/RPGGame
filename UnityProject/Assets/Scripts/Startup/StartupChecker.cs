using UnityEngine;

namespace Assets.Scripts.Startup
{
    // This will run before any MonoBehaviour
    public static class StartupChecker
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnSubsystemRegistration()
        {
            Debug.Log("=== STARTUP: SubsystemRegistration ===");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void OnAfterAssembliesLoaded()
        {
            Debug.Log("=== STARTUP: AfterAssembliesLoaded ===");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnBeforeSplashScreen()
        {
            Debug.Log("=== STARTUP: BeforeSplashScreen ===");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            Debug.Log("=== STARTUP: BeforeSceneLoad ===");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Debug.Log("=== STARTUP: AfterSceneLoad ===");
        }
    }
}