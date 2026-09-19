using RPGGame.Core.Prototypes.Locations;
using RPGGame.Core.Simulation;
using UnityEngine;

namespace RPGGame.Core.Dungeons;

/// <summary>
/// Manages dungeon/POI exploration.
/// Currently stubbed - dungeons show "Coming Soon" message.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class DungeonManager {
	#region Fields

	private readonly GameDb k_gameDb;
	private DungeonProto? k_currentDungeon;
	private bool k_isInDungeon;

	#endregion

	#region Properties

	/// <summary>Whether a dungeon interaction is pending.</summary>
	public bool HasPendingDungeon => k_currentDungeon != null && !k_isInDungeon;

	/// <summary>Whether currently inside a dungeon.</summary>
	public bool IsInDungeon => k_isInDungeon;

	/// <summary>The current/pending dungeon proto.</summary>
	public DungeonProto? CurrentDungeon => k_currentDungeon;

	/// <summary>Whether dungeons are fully implemented.</summary>
	public bool AreDungeonsImplemented => false; // TODO: Set to true when implemented

	#endregion

	#region Events

	/// <summary>Fired when a dungeon is approached.</summary>
	public event Action<DungeonProto>? OnDungeonApproached;

	/// <summary>Fired when dungeon interaction is dismissed.</summary>
	public event Action? OnDungeonDismissed;

	/// <summary>Fired when dungeon is entered (future).</summary>
	public event Action<DungeonProto>? OnDungeonEntered;

	/// <summary>Fired when dungeon is exited (future).</summary>
	public event Action<DungeonResult>? OnDungeonExited;

	/// <summary>Fired when "Coming Soon" should be displayed.</summary>
	public event Action<DungeonInfo>? OnDungeonComingSoon;

	#endregion

	#region Constructor

	public DungeonManager(GameDb gameDb) {
		k_gameDb = gameDb;
		Debug.Log("DungeonManager: Initialized (Dungeons Coming Soon)");
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Called when player reaches a dungeon node.
	/// Shows the dungeon info/coming soon message.
	/// </summary>
	public void ApproachDungeon(DungeonProto.ID dungeonId) {
		if (!k_gameDb.TryGetProto<DungeonProto>(dungeonId, out var proto)) {
			Debug.LogError($"DungeonManager: Dungeon not found: {dungeonId}");
			return;
		}

		ApproachDungeon(proto);
	}

	/// <summary>
	/// Called when player reaches a dungeon node.
	/// </summary>
	public void ApproachDungeon(DungeonProto proto) {
		k_currentDungeon = proto;
		OnDungeonApproached?.Invoke(proto);

		Debug.Log($"DungeonManager: Approached dungeon '{proto.DisplayText.Name}' (Coming Soon)");

		// Show coming soon message if not implemented
		if (!AreDungeonsImplemented) {
			var info = GetDungeonInfo(proto);
			OnDungeonComingSoon?.Invoke(info);
		}
	}

	/// <summary>
	/// Dismisses the dungeon interaction (player chooses to leave).
	/// </summary>
	public void DismissDungeon() {
		k_currentDungeon = null;
		k_isInDungeon = false;
		OnDungeonDismissed?.Invoke();

		Debug.Log("DungeonManager: Dungeon dismissed");
	}

	/// <summary>
	/// Attempts to enter the dungeon.
	/// Currently shows "Coming Soon" message.
	/// </summary>
	public bool TryEnterDungeon() {
		if (k_currentDungeon == null) {
			return false;
		}

		if (!AreDungeonsImplemented) {
			Debug.Log($"DungeonManager: Dungeons coming soon! ({k_currentDungeon.DisplayText.Name})");
			return false;
		}

		// Future: Actually enter the dungeon
		k_isInDungeon = true;
		OnDungeonEntered?.Invoke(k_currentDungeon);
		return true;
	}

	/// <summary>
	/// Exits the current dungeon. Called when dungeon run ends.
	/// </summary>
	public void ExitDungeon(DungeonResult result) {
		if (!k_isInDungeon || k_currentDungeon == null) {
			return;
		}

		k_isInDungeon = false;
		OnDungeonExited?.Invoke(result);

		Debug.Log($"DungeonManager: Exited dungeon '{k_currentDungeon.DisplayText.Name}' " +
		          $"(Completed: {result.Completed}, Floors: {result.FloorsCleared}/{result.TotalFloors})");

		k_currentDungeon = null;
	}

	/// <summary>
	/// Gets info about a dungeon for UI display.
	/// </summary>
	public DungeonInfo GetDungeonInfo(DungeonProto proto) {
		return new DungeonInfo {
			Name = proto.DisplayText.Name,
			Description = proto.DisplayText.Description,
			DungeonType = proto.DungeonType,
			DifficultyTier = proto.DifficultyTier,
			RecommendedLevel = proto.RecommendedLevel,
			FloorCount = proto.FloorCount,
			IsProceduralLayout = proto.IsProceduralLayout,
			HasBoss = proto.BossEncounter != null,
			Hazards = proto.Hazards.ToList(),
			Mechanics = proto.Mechanics,
			IsImplemented = AreDungeonsImplemented,
			ComingSoonMessage = "Dungeon exploration coming soon!\n\nThis location will be explorable in a future update."
		};
	}

	/// <summary>
	/// Gets info about the current/pending dungeon.
	/// </summary>
	public DungeonInfo? GetCurrentDungeonInfo() {
		if (k_currentDungeon == null) {
			return null;
		}
		return GetDungeonInfo(k_currentDungeon);
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// Info about a dungeon for UI display.
/// </summary>
public class DungeonInfo {
	public string NodeId { get; set; } = "";
	public string Name { get; set; } = "";
	public string Description { get; set; } = "";
	public DungeonType DungeonType { get; set; }
	public int DifficultyTier { get; set; }
	public int RecommendedLevel { get; set; }
	public int FloorCount { get; set; }
	public bool IsProceduralLayout { get; set; }
	public bool HasBoss { get; set; }
	public List<DungeonHazard> Hazards { get; set; } = [];
	public DungeonMechanics Mechanics { get; set; }
	public bool IsImplemented { get; set; }
	public string ComingSoonMessage { get; set; } = "";
}

/// <summary>
/// Result of completing a dungeon.
/// </summary>
public class DungeonResult {
	public string DungeonId { get; set; } = "";
	public bool Completed { get; set; }
	public int FloorsCleared { get; set; }
	public int TotalFloors { get; set; }
	public int EnemiesDefeated { get; set; }
	public int ExperienceGained { get; set; }
	public int GoldGained { get; set; }
	public List<string> LootItemIds { get; set; } = [];
	public TimeSpan Duration { get; set; }
}

#endregion