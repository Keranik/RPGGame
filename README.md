# RPGGame

A solo-character roguelike RPG prototype with heavy D&D influence: a fog-of-war overworld where you lead expeditions from an upgradeable village hub, with turn-based combat (plus an auto-battle idler mode), survival mechanics (food, morale, weather/seasons, camping), a branching narrative event system, and run-based meta-progression — territory stays revealed between runs, equipment is lost on death, and clearing the fog advances a "true ending" lore track. Combat math is designed to be fully inspectable via an interactive combat log.

> **Status: prototype.** The .NET simulation core (managers, combat, events, saves, procedural generation) and a large UIToolkit screen/component library are implemented. Dungeons are currently stubbed, and the Unity presentation shell is early-stage. This is a work-in-progress game, not a finished release. Developed with AI assistance.

## Architecture

Two halves, built separately:

| Part | Target | What it is |
|---|---|---|
| `Source/` | .NET Standard 2.1 | The complete game-logic and UI codebase. `Core/` is the Unity-agnostic simulation: a central `GameDb` prototype registry (immutable game-data definitions with O(1) ID lookups), a `[Dependency]`-attribute DI container, a fixed-timestep `GameLoop` (0.1s ticks separating render updates, simulation ticks, and scaled calendar time), and ~20 managers (village, expedition, combat, events, dungeons, inventory, rewards, metrics…). `UI/` is a Unity UIToolkit screen library: 12 full screens, ~51 reusable components, a window stack, and programmatic UI construction. Persistence is versioned JSON saves with checksums (5 manual slots + autosave). Terrain is procedurally generated with the vendored FastNoiseLite library and rendered through an isometric renderer stack. |
| `UnityProject/` | Unity 6000.3.0f1 (URP 17.3.0, 2D) | A thin presentation shell. A single `GameManager` singleton bootstraps the game: it registers the core assembly's types in the DI container, resolves the game loop, state manager, and session, constructs the orchestrator wiring all the managers, then pumps the simulation every frame. Three scenes (main menu, game bootstrap, UI test scene), a runtime tilemap test harness, and a dev tool that procedurally generates tile templates. |

Strongly-typed string-ID catalogs (`Ids.*.cs`, ~20 files) keep content references compile-safe; nullable reference types are enabled throughout.

## Building

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (builds the netstandard2.1 core)
- Unity 6000.3.0f1 via Unity Hub

### Steps

```powershell
# 1. Build the core — the post-build step copies the DLL into
#    UnityProject/Assets/Plugins (regenerated on every build)
dotnet build Source/RPGGame.sln

# 2. Open UnityProject in the Unity editor
```

Notes:

- `UnityProject/Assets/Plugins/*.dll` / `*.pdb` are build outputs — gitignored, not committed.
- The core's `.csproj` references Unity assemblies via HintPaths pinned to a Unity 6000.3.0f1 Hub install; building `Source/` outside Unity requires that layout (or adjust the references).

## Third-party components & licenses

- `Source/Core/FastNoiseLite.cs` — [FastNoiseLite](https://github.com/Auburn/FastNoiseLite) by Jordan Peck, MIT (header preserved in-file).
- `System.Collections.Immutable` 8.0.0 (NuGet, MIT).
- TextMesh Pro, Input System, URP and other Unity registry packages (Unity's own terms; see `UnityProject/Packages/manifest.json`).
- Fonts — see [FONTS-LICENSES.md](FONTS-LICENSES.md). The D&D-homebrew-community families are CC-BY-SA 4.0 / CC BY 4.0 and require attribution (credited there). Note: the developer's personal copy used the **WhiteRabbit** font for the monospace slot (dice notation, coordinates), but its license did not permit redistribution, so the public release ships with **JetBrains Mono** (OFL 1.1) in that slot instead. After cloning, generate the TMP font asset in Unity via Window → TextMeshPro → Font Asset Creator from `Fonts/Raw/Mono/JetBrainsMono-Regular.ttf` and save it as `Mono SDF` in `UnityProject/Assets/Resources/Fonts/Raw/Mono/`.
- All sprites are original prototype work (see the audit notes in the font/asset docs).

## License

MIT — see [LICENSE](LICENSE).
