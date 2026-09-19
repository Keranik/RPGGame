using RPGGame.Core.Characters;
using RPGGame.Core.Combat;
using RPGGame.Core.Events;
using RPGGame.Core.Expedition;
using RPGGame.Core.Items;
using RPGGame.Core.Metrics;
using RPGGame.Core.Prototypes.Characters;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Save;
using RPGGame.Core.Stats;
using RPGGame.Core.Village;

namespace RPGGame.Core.Simulation;

/// <summary>
/// High-level game orchestrator that provides a simplified API for UI/gameplay code.
/// Acts as a facade over the various game systems.
/// </summary>
/// TODO: Figure out what actually to do with this
public class GameOrchestrator {
	#region Fields

	private readonly GameSession k_session;
	private readonly GameStateManager k_stateManager;
	private readonly VillageManager k_village;
	private readonly ExpeditionManager k_expedition;
	private readonly CombatManager k_combat;
	private readonly EventManager k_events;
	private readonly InventoryManager k_inventory;
	private readonly MetricsManager k_metrics;

	#endregion

	#region Properties

	/// <summary>
	/// Current game phase.
	/// </summary>
	public GamePhase Phase => k_session.CurrentPhase;

	/// <summary>
	/// Whether a run is active.
	/// </summary>
	public bool IsInRun => k_session.CurrentRun != null;

	/// <summary>
	/// Current run state.
	/// </summary>
	public RunState? Run => k_session.CurrentRun;

	/// <summary>
	/// Meta progression.
	/// </summary>
	public MetaProgression Meta => k_session.MetaProgression;

	#endregion

	#region Constructor

	public GameOrchestrator(
		GameSession session,
		GameStateManager stateManager,
		VillageManager village,
		ExpeditionManager expedition,
		CombatManager combat,
		EventManager events,
		InventoryManager inventory,
		MetricsManager metrics
	) {
		k_session = session;
		k_stateManager = stateManager;
		k_village = village;
		k_expedition = expedition;
		k_combat = combat;
		k_events = events;
		k_inventory = inventory;
		k_metrics = metrics;
	}

	#endregion

	#region Game Flow

	/// <summary>
	/// Initializes the game.
	/// </summary>
	public void Initialize() => k_session.Initialize();

	/// <summary>
	/// Shuts down the game.
	/// </summary>
	public void Shutdown() => k_session.Shutdown();

	/// <summary>
	/// Starts a new game with the specified character.
	/// </summary>
	public bool StartNewGame(string characterName, string classId) {
		return k_session.StartNewRun(characterName, classId);
	}

	/// <summary>
	/// Starts a new game with a typed class ID.
	/// </summary>
	public bool StartNewGame(string characterName, CharacterClassProto.ID classId) {
		return k_session.StartNewRun(characterName, classId);
	}

	/// <summary>
	/// Continues an existing game from a save slot.
	/// </summary>
	public async Task<bool> ContinueGame(int slot) {
		return await k_session.LoadGameAsync(slot);
	}

	/// <summary>
	/// Abandons the current run.
	/// </summary>
	public void AbandonRun() {
		k_session.EndRun(RunEndReason.Abandoned);
	}

	/// <summary>
	/// Returns to the main menu.
	/// </summary>
	public void ReturnToMainMenu() {
		if (IsInRun) {
			_ = k_session.SaveGameAsync(SaveManager.AUTOSAVE_SLOT);
		}
		k_stateManager.SetPhase(GamePhase.MainMenu);
	}

	#endregion

	#region Village Actions

	/// <summary>
	/// Upgrades a village building.
	/// </summary>
	public VillageBuildResult UpgradeBuilding(string buildingId) {
		if (!IsInRun) {
			return new VillageBuildResult { Success = false, Message = "Not in a run." };
		}

		var result = k_village.UpgradeBuilding(buildingId);

		if (result.Success) {
			Meta.BuildingLevels[buildingId] = result.NewLevel;
		}

		return result;
	}

	/// <summary>
	/// Buys an item from a shop.
	/// </summary>
	public BuyResult BuyItem(string itemId, int count = 1) {
		return k_inventory.BuyItem(itemId, count);
	}

	/// <summary>
	/// Sells an item.
	/// </summary>
	public SellResult SellItem(ItemInstance item, int count = -1) {
		return k_inventory.SellItem(item, count);
	}

	/// <summary>
	/// Repairs an item.
	/// </summary>
	public bool RepairItem(ItemInstance item) {
		int cost = k_inventory.GetRepairCost(item);

		if (Run == null || Run.Character.GetStat(Ids.Stats.Expedition.GoldOnHand) < cost) {
			return false;
		}

		Run.Character.BaseStats.Subtract(Ids.Stats.Expedition.GoldOnHand, cost);
		k_inventory.RepairItem(item);

		return true;
	}

	/// <summary>
	/// Rests at the village inn.
	/// </summary>
	public bool RestAtInn(int hours = 8) {
		if (Run == null || Phase != GamePhase.Village) {
			return false;
		}

		float healAmount = Run.Stats.Get(Ids.Stats.Resource.MaxHealth) * 0.5f;
		Run.Stats.SetBase(Ids.Stats.Resource.CurrentHealth,
			Math.Min(Run.Stats.Get(Ids.Stats.Resource.MaxHealth),
				Run.Stats.Get(Ids.Stats.Resource.CurrentHealth) + healAmount));

		Run.Fatigue = Math.Max(0, Run.Fatigue - 50);
		Run.Morale = Math.Min(100, Run.Morale + 10);
		GameTime.Instance.AdvanceTicks(hours*GameTime.TICKS_PER_HOUR);

		return true;
	}

	#endregion

	#region Expedition Actions

	/// <summary>
	/// Leaves the village to start an expedition.
	/// </summary>
	public bool LeaveVillage() {
		return k_session.StartExpedition();
	}

	/// <summary>
	/// Returns to the village from expedition.
	/// </summary>
	public void ReturnToVillage() {
		k_session.ReturnToVillage();
	}

	/// <summary>
	/// Sets travel speed.
	/// </summary>
	public void SetTravelSpeed(TravelSpeed speed) {
		k_expedition.SetTravelSpeed(speed);
	}

	/// <summary>
	/// Pauses travel.
	/// </summary>
	public void PauseTravel() => k_expedition.PauseTravel();

	/// <summary>
	/// Resumes travel.
	/// </summary>
	public void ResumeTravel() => k_expedition.ResumeTravel();

	/// <summary>
	/// Sets up camp.
	/// </summary>
	public bool SetupCamp() {
		return k_expedition.SetupCamp();
	}

	/// <summary>
	/// Breaks camp and resumes travel.
	/// </summary>
	public void BreakCamp() {
		k_expedition.BreakCamp();
	}

	/// <summary>
	/// Uses a medical supply to heal.
	/// </summary>
	public bool UseMedicalSupply() {
		return k_expedition.UseMedicalSupply();
	}

	#endregion

	#region Combat Actions

	/// <summary>
	/// Executes a combat action.
	/// </summary>
	public CombatActionResult? ExecuteCombatAction(CombatAction action) {
		return k_combat.ExecuteAction(action);
	}

	/// <summary>
	/// Continues combat after action result is shown.
	/// </summary>
	public void ContinueCombat() {
		k_combat.ContinueAfterAction();
	}

	/// <summary>
	/// Gets available combat actions.
	/// </summary>
	public List<CombatActionType> GetAvailableActions() {
		return k_combat.GetAvailableActions();
	}

	/// <summary>
	/// Gets valid targets for an action.
	/// </summary>
	public IEnumerable<LiveCharacter> GetValidTargets(CombatActionType actionType) {
		var current = k_combat.CurrentCombatant;
		if (current == null) {
			return [];
		}

		return k_combat.GetValidTargets(current, actionType);
	}

	/// <summary>
	/// Gets current combat state.
	/// </summary>
	public CombatState GetCombatState() {
		return k_combat.GetCombatState();
	}

	#endregion

	#region Event Actions

	/// <summary>
	/// Makes a choice in the current event.
	/// </summary>
	public bool MakeEventChoice(EventChoiceProto choice) {
		if (Run == null) {
			return false;
		}
		return k_events.SelectChoice(choice, Run, Meta);
	}

	/// <summary>
	/// Completes the current event.
	/// </summary>
	public void CompleteEvent() {
		if (Run == null) {
			return;
		}
		k_events.CompleteEvent(Run, Meta);
	}

	/// <summary>
	/// Skips the current event (if allowed).
	/// </summary>
	public bool SkipEvent() {
		if (Run == null) {
			return false;
		}
		return k_events.SkipEvent(Run, Meta);
	}

	/// <summary>
	/// Gets current event state.
	/// </summary>
	public EventState GetEventState() {
		if (Run == null) {
			return new EventState { IsActive = false };
		}
		return k_events.GetEventState(Run, Meta);
	}

	#endregion

	#region Inventory Actions

	/// <summary>
	/// Uses an item.
	/// </summary>
	public ItemUseResult UseItem(ItemInstance item) {
		var context = Phase switch {
			GamePhase.Combat => ItemUseContext.CombatOnly,
			GamePhase.Village => ItemUseContext.NonCombat,
			GamePhase.Expedition when k_expedition.IsCamped => ItemUseContext.CampOnly,
			_ => ItemUseContext.NonCombat
		};

		return k_inventory.UseItem(item, context);
	}

	/// <summary>
	/// Equips an item.
	/// </summary>
	public bool EquipItem(ItemInstance item) {
		return k_inventory.EquipItem(item);
	}

	/// <summary>
	/// Unequips an item.
	/// </summary>
	public bool UnequipItem(ItemInstance item) {
		return k_inventory.UnequipItem(item);
	}

	/// <summary>
	/// Gets inventory summary.
	/// </summary>
	public InventorySummary GetInventorySummary() {
		return k_inventory.GetSummary();
	}

	#endregion

	#region Character/Stats

	/// <summary>
	/// Levels up if possible.
	/// </summary>
	public bool TryLevelUp() {
		if (Run == null || Run.PendingLevelUps <= 0) {
			return false;
		}

		// Apply level up manually without StatManager
		Run.PendingLevelUps--;
		Run.HasPendingLevelUp = Run.PendingLevelUps > 0;
		Run.Stats.Add(Ids.Stats.Meta.Level, 1);
		Run.Stats.Set(Ids.Stats.Resource.CurrentHealth, Run.MaxHealth);
		Run.Stats.Set(Ids.Stats.Resource.CurrentMana, Run.MaxMana);

		return true;
	}

	/// <summary>
	/// Gets player stats for display.
	/// </summary>
	public PlayerStatsDisplay GetPlayerStats() {
		if (Run == null) {
			return new PlayerStatsDisplay();
		}

		return new PlayerStatsDisplay {
			Name = Run.Character.Name,
			ClassName = Run.CharacterClassId,
			Level = Run.Stats.GetInt(Ids.Stats.Meta.Level),
			CurrentHealth = Run.Stats.Get(Ids.Stats.Resource.CurrentHealth),
			MaxHealth = Run.Stats.Get(Ids.Stats.Resource.MaxHealth),
			CurrentMana = Run.Stats.Get(Ids.Stats.Resource.CurrentMana),
			MaxMana = Run.Stats.Get(Ids.Stats.Resource.MaxMana),
			Experience = (int)Run.Character.Experience,
			ExperienceToLevel = Run.GetExperienceToNextLevel(),
			Gold = Run.Stats.GetInt(Ids.Stats.Expedition.GoldOnHand),
			Morale = (int)Run.Morale,
			Fatigue = (int)Run.Fatigue,
			ArmorClass = Run.Stats.GetInt(Ids.Stats.Combat.ArmorClass),
			AttackBonus = Run.Stats.GetInt(Ids.Stats.Combat.AttackBonus)
		};
	}

	#endregion

	#region Save/Load

	/// <summary>
	/// Quick saves the game.
	/// </summary>
	public async Task<bool> QuickSave() {
		return await k_session.SaveGameAsync(0);
	}

	/// <summary>
	/// Quick loads the game.
	/// </summary>
	public async Task<bool> QuickLoad() {
		return await k_session.LoadGameAsync(0);
	}

	/// <summary>
	/// Saves to a specific slot.
	/// </summary>
	public async Task<bool> SaveToSlot(int slot) {
		return await k_session.SaveGameAsync(slot);
	}

	/// <summary>
	/// Loads from a specific slot.
	/// </summary>
	public async Task<bool> LoadFromSlot(int slot) {
		return await k_session.LoadGameAsync(slot);
	}

	/// <summary>
	/// Gets all save slots.
	/// </summary>
	public List<SaveSlotInfo> GetSaveSlots() {
		return k_session.GetSaveSlots();
	}

	#endregion

	#region Queries

	/// <summary>
	/// Gets available character classes.
	/// </summary>
	public List<CharacterClassProto> GetAvailableClasses() {
		return k_session.GetAvailableClasses();
	}

	/// <summary>
	/// Gets a specific class by ID.
	/// </summary>
	public CharacterClassProto? GetClassProto(CharacterClassProto.ID classId) {
		return k_session.GetClassProto(classId);
	}

	/// <summary>
	/// Gets a specific class by string ID.
	/// </summary>
	public CharacterClassProto? GetClassProto(string classId) {
		return k_session.GetClassProto(classId);
	}

	/// <summary>
	/// Gets session state.
	/// </summary>
	public SessionState GetSessionState() {
		return k_session.GetSessionState();
	}

	/// <summary>
	/// Gets lifetime statistics.
	/// </summary>
	public MetricsSummary GetLifetimeStats() {
		return k_metrics.GetMetricsSummary();
	}

	#endregion
}