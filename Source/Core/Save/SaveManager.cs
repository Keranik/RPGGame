using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RPGGame.Core.Metrics;
using RPGGame.Core.Simulation;
using RPGGame.Core.Village;
using RPGGame.Core.Items;
using UnityEngine;

namespace RPGGame.Core.Save;

/// <summary>
/// Manages saving and loading game state.
/// Handles save file I/O, versioning, checksums, and data serialization.
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class SaveManager {
	#region Constants

	/// <summary>
	/// Current save format version.
	/// </summary>
	public const int CURRENT_SAVE_VERSION = 1;

	/// <summary>
	/// Number of manual save slots.
	/// </summary>
	public const int MAX_SAVE_SLOTS = 5;

	/// <summary>
	/// Auto-save slot index.
	/// </summary>
	public const int AUTOSAVE_SLOT = -1;

	private const string SAVE_FOLDER = "Saves";
	private const string SAVE_EXTENSION = ".json";
	private const string BACKUP_EXTENSION = ".bak";
	private const string META_FILENAME = "meta.json";

	#endregion

	#region Fields

	private readonly GameStateManager k_gameState;
	private readonly MetricsManager k_metrics;
	private readonly VillageManager k_village;
	private readonly InventoryManager k_inventory;

	private readonly string k_savePath;
	private readonly JsonSerializerSettings k_jsonSettings;

	private DateTime k_sessionStartTime;
	private double k_previousPlaytime;

	#endregion

	#region Properties

	/// <summary>
	/// Current session playtime in seconds.
	/// </summary>
	public double SessionPlaytime => (DateTime.Now - k_sessionStartTime).TotalSeconds;

	/// <summary>
	/// Total playtime including previous sessions.
	/// </summary>
	public double TotalPlaytime => k_previousPlaytime + SessionPlaytime;

	/// <summary>
	/// Whether auto-save is enabled.
	/// </summary>
	public bool AutoSaveEnabled { get; set; } = true;

	/// <summary>
	/// Auto-save interval in seconds.
	/// </summary>
	public float AutoSaveInterval { get; set; } = 300f; // 5 minutes

	/// <summary>
	/// Time since last auto-save.
	/// </summary>
	public float TimeSinceLastAutoSave { get; private set; }

	#endregion

	#region Events

	public event Action<int>? OnSaveStarted;
	public event Action<int, bool>? OnSaveCompleted;
	public event Action<int>? OnLoadStarted;
	public event Action<int, bool>? OnLoadCompleted;
	public event Action? OnAutoSave;

	#endregion

	#region Constructor

	public SaveManager(
		GameStateManager gameState,
		MetricsManager metrics,
		VillageManager village,
		InventoryManager inventory
	) {
		k_gameState = gameState;
		k_metrics = metrics;
		k_village = village;
		k_inventory = inventory;

		k_savePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
		k_sessionStartTime = DateTime.Now;

		k_jsonSettings = new JsonSerializerSettings {
			Formatting = Formatting.Indented,
			ContractResolver = new SaveContractResolver(), // Changed from CamelCasePropertyNamesContractResolver
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.None
		};

		EnsureSaveDirectory();
		LoadMetaData();

		Debug.Log($"SaveManager initialized. Save path: {k_savePath}");
	}

	#endregion

	#region Save Operations

	/// <summary>
	/// Saves the game to a slot.
	/// </summary>
	public async Task<bool> SaveGameAsync(int slot) {
		OnSaveStarted?.Invoke(slot);

		try {
			var saveData = CreateSaveData();
			saveData.Checksum = CalculateChecksum(saveData);

			string filePath = GetSaveFilePath(slot);

			// Create backup of existing save
			if (File.Exists(filePath)) {
				string backupPath = filePath + BACKUP_EXTENSION;
				File.Copy(filePath, backupPath, overwrite: true);
			}

			// Write save file
			string json = JsonConvert.SerializeObject(saveData, k_jsonSettings);
			await File.WriteAllTextAsync(filePath, json);

			// Update meta data
			await SaveMetaDataAsync();

			k_metrics.Increment(MetricType.TimesSaved);

			OnSaveCompleted?.Invoke(slot, true);
			Debug.Log($"Game saved to slot {slot}");

			return true;
		} catch (Exception ex) {
			Debug.LogError($"Failed to save game: {ex.Message}");
			OnSaveCompleted?.Invoke(slot, false);
			return false;
		}
	}

	/// <summary>
	/// Saves the game synchronously.
	/// </summary>
	public bool SaveGame(int slot) {
		return SaveGameAsync(slot).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Triggers auto-save if enabled and interval elapsed.
	/// </summary>
	public void TryAutoSave(float deltaTime) {
		if (!AutoSaveEnabled) {
			return;
		}

		TimeSinceLastAutoSave += deltaTime;

		if (TimeSinceLastAutoSave >= AutoSaveInterval) {
			TimeSinceLastAutoSave = 0;
			PerformAutoSave();
		}
	}

	/// <summary>
	/// Forces an auto-save.
	/// </summary>
	public void PerformAutoSave() {
		OnAutoSave?.Invoke();
		_ = SaveGameAsync(AUTOSAVE_SLOT);
		TimeSinceLastAutoSave = 0;
	}

	private SaveData CreateSaveData() {
		var data = new SaveData {
			Version = CURRENT_SAVE_VERSION,
			Timestamp = DateTime.Now,
			PlaytimeSeconds = TotalPlaytime,
			GamePhase = k_gameState.CurrentPhase
		};

		// Meta progression - use the ToData() method from MetaProgression
		var meta = k_gameState.MetaProgression;
		if (meta != null) {
			data.MetaProgression = meta.ToData();
		}

		// Run state - use the ToData() method from RunState
		var run = k_gameState.RunState;
		if (run != null) {
			data.RunState = run.ToData();

			// Character data
			data.Character = new CharacterSaveData {
				Name = run.Character.Name,
				ClassId = run.Character.ClassId?.Value ?? run.CharacterClassId,
				Level = run.Character.Level,
				PortraitName = run.Character.PortraitName
			};
		}

		// Village
		data.Village = k_village.GetSaveData();

		// Inventory
		data.Inventory = k_inventory.ToData();

		// Metrics
		data.Metrics = k_metrics.ToData();

		return data;
	}

	#endregion

	#region Load Operations

	/// <summary>
	/// Loads a game from a slot.
	/// </summary>
	public async Task<bool> LoadGameAsync(int slot) {
		OnLoadStarted?.Invoke(slot);

		try {
			string filePath = GetSaveFilePath(slot);

			if (!File.Exists(filePath)) {
				Debug.LogWarning($"Save file not found: {filePath}");
				OnLoadCompleted?.Invoke(slot, false);
				return false;
			}

			string json = await File.ReadAllTextAsync(filePath);
			var saveData = JsonConvert.DeserializeObject<SaveData>(json, k_jsonSettings);

			if (saveData == null) {
				Debug.LogError("Failed to deserialize save data");
				OnLoadCompleted?.Invoke(slot, false);
				return false;
			}

			// Verify checksum
			if (!VerifyChecksum(saveData)) {
				Debug.LogWarning("Save file checksum mismatch - file may be corrupted");
				// Continue loading anyway, but could prompt user
			}

			// Check version compatibility
			if (saveData.Version > CURRENT_SAVE_VERSION) {
				Debug.LogError($"Save file version {saveData.Version} is newer than current {CURRENT_SAVE_VERSION}");
				OnLoadCompleted?.Invoke(slot, false);
				return false;
			}

			// Apply save data
			ApplySaveData(saveData);

			// Update playtime tracking
			k_previousPlaytime = saveData.PlaytimeSeconds;
			k_sessionStartTime = DateTime.Now;

			k_metrics.Increment(MetricType.TimesLoaded);

			OnLoadCompleted?.Invoke(slot, true);
			Debug.Log($"Game loaded from slot {slot}");

			return true;
		} catch (Exception ex) {
			Debug.LogError($"Failed to load game: {ex.Message}");
			OnLoadCompleted?.Invoke(slot, false);
			return false;
		}
	}

	/// <summary>
	/// Loads a game synchronously.
	/// </summary>
	public bool LoadGame(int slot) {
		return LoadGameAsync(slot).GetAwaiter().GetResult();
	}

	private void ApplySaveData(SaveData data) {
			// Restore meta progression
			if (data.MetaProgression != null) {
				k_gameState.MetaProgression.FromData(data.MetaProgression);
				SaveContractResolver.RebuildAfterLoad(k_gameState.MetaProgression);
			}

			// Restore run state
			if (data.RunState != null) {
				var runState = RunState.FromData(data.RunState);
				SaveContractResolver.RebuildAfterLoad(runState);
				SaveContractResolver.RebuildAfterLoad(runState.Character);
				k_gameState.RestoreRunState(runState);
			}

			// Restore village
			if (data.Village != null) {
				k_village.LoadVillage(data.Village);
			}

			// Restore inventory
			if (data.Inventory != null) {
				k_inventory.FromData(data.Inventory);
			}

			// Restore metrics
			if (data.Metrics != null) {
				k_metrics.FromData(data.Metrics);
			}

			// Set game phase
			k_gameState.SetPhase(data.GamePhase);
	}

	#endregion

	#region Slot Management

	/// <summary>
	/// Gets info for all save slots.
	/// </summary>
	public List<SaveSlotInfo> GetAllSlotInfo() {
		var slots = new List<SaveSlotInfo>();

		// Auto-save slot
		slots.Add(GetSlotInfo(AUTOSAVE_SLOT));

		// Manual slots
		for (int i = 0; i < MAX_SAVE_SLOTS; i++) {
			slots.Add(GetSlotInfo(i));
		}

		return slots;
	}

	/// <summary>
	/// Gets info for a specific slot.
	/// </summary>
	public SaveSlotInfo GetSlotInfo(int slot) {
		string filePath = GetSaveFilePath(slot);

		if (!File.Exists(filePath)) {
			return new SaveSlotInfo {
				SlotIndex = slot,
				HasData = false,
				IsAutoSave = slot == AUTOSAVE_SLOT
			};
		}

		try {
			string json = File.ReadAllText(filePath);
			var data = JsonConvert.DeserializeObject<SaveData>(json, k_jsonSettings);

			if (data == null) {
				return SaveSlotInfo.Empty(slot);
			}

			return new SaveSlotInfo {
				SlotIndex = slot,
				HasData = true,
				IsAutoSave = slot == AUTOSAVE_SLOT,
				CharacterName = data.Character?.Name ?? "Unknown",
				CharacterClass = data.Character?.ClassId ?? "",
				Level = data.Character?.Level ?? 1,
				RunNumber = data.MetaProgression?.TotalRuns ?? 1,
				CurrentDay = data.RunState?.CurrentDay ?? 1,
				PlaytimeSeconds = data.PlaytimeSeconds,
				SaveTimestamp = data.Timestamp,
				GamePhase = data.GamePhase,
				FogClears = data.MetaProgression?.FogClears ?? 0,
				TrueEndingAchieved = data.MetaProgression?.TrueEndingAchieved ?? false,
				SaveVersion = data.Version
			};
		} catch (Exception ex) {
			Debug.LogWarning($"Failed to read save slot {slot}: {ex.Message}");
			return SaveSlotInfo.Empty(slot);
		}
	}

	/// <summary>
	/// Deletes a save slot.
	/// </summary>
	public bool DeleteSlot(int slot) {
		try {
			string filePath = GetSaveFilePath(slot);

			if (File.Exists(filePath)) {
				File.Delete(filePath);
			}

			string backupPath = filePath + BACKUP_EXTENSION;
			if (File.Exists(backupPath)) {
				File.Delete(backupPath);
			}

			Debug.Log($"Deleted save slot {slot}");
			return true;
		} catch (Exception ex) {
			Debug.LogError($"Failed to delete save slot: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	/// Checks if a slot has save data.
	/// </summary>
	public bool HasSaveData(int slot) {
		return File.Exists(GetSaveFilePath(slot));
	}

	/// <summary>
	/// Copies a save to another slot.
	/// </summary>
	public bool CopySlot(int sourceSlot, int targetSlot) {
		try {
			string sourcePath = GetSaveFilePath(sourceSlot);
			string targetPath = GetSaveFilePath(targetSlot);

			if (!File.Exists(sourcePath)) {
				return false;
			}

			File.Copy(sourcePath, targetPath, overwrite: true);
			Debug.Log($"Copied save from slot {sourceSlot} to {targetSlot}");
			return true;
		} catch (Exception ex) {
			Debug.LogError($"Failed to copy save: {ex.Message}");
			return false;
		}
	}

	#endregion

	#region Helpers

	private string GetSaveFilePath(int slot) {
		string filename = slot == AUTOSAVE_SLOT
			? "autosave" + SAVE_EXTENSION
			: $"save_{slot}" + SAVE_EXTENSION;

		return Path.Combine(k_savePath, filename);
	}

	private void EnsureSaveDirectory() {
		if (!Directory.Exists(k_savePath)) {
			Directory.CreateDirectory(k_savePath);
		}
	}

	private string CalculateChecksum(SaveData data) {
		// Create a copy without the checksum field
		var temp = data.Checksum;
		data.Checksum = null;

		string json = JsonConvert.SerializeObject(data, k_jsonSettings);
		data.Checksum = temp;

		using var sha256 = SHA256.Create();
		byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
		return Convert.ToBase64String(hash);
	}

	private bool VerifyChecksum(SaveData data) {
		if (string.IsNullOrEmpty(data.Checksum)) {
			return true; // No checksum to verify
		}

		string expected = data.Checksum;
		string actual = CalculateChecksum(data);

		return expected == actual;
	}

	private void LoadMetaData() {
		string metaPath = Path.Combine(k_savePath, META_FILENAME);

		if (File.Exists(metaPath)) {
			try {
				string json = File.ReadAllText(metaPath);
				var meta = JsonConvert.DeserializeObject<SaveMetaData>(json, k_jsonSettings);
				if (meta != null) {
					k_previousPlaytime = meta.TotalPlaytimeSeconds;
				}
			} catch {
				// Ignore errors loading meta
			}
		}
	}

	private async Task SaveMetaDataAsync() {
		string metaPath = Path.Combine(k_savePath, META_FILENAME);

		var meta = new SaveMetaData {
			TotalPlaytimeSeconds = TotalPlaytime,
			LastSaveTimestamp = DateTime.Now
		};

		string json = JsonConvert.SerializeObject(meta, k_jsonSettings);
		await File.WriteAllTextAsync(metaPath, json);
	}

	#endregion

	#region Backup/Restore

	/// <summary>
	/// Restores a save from backup.
	/// </summary>
	public bool RestoreFromBackup(int slot) {
		try {
			string filePath = GetSaveFilePath(slot);
			string backupPath = filePath + BACKUP_EXTENSION;

			if (!File.Exists(backupPath)) {
				return false;
			}

			File.Copy(backupPath, filePath, overwrite: true);
			Debug.Log($"Restored slot {slot} from backup");
			return true;
		} catch (Exception ex) {
			Debug.LogError($"Failed to restore from backup: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	/// Exports save data to a file.
	/// </summary>
	public async Task<bool> ExportSaveAsync(int slot, string exportPath) {
		try {
			string sourcePath = GetSaveFilePath(slot);

			if (!File.Exists(sourcePath)) {
				return false;
			}

			File.Copy(sourcePath, exportPath, overwrite: true);
			Debug.Log($"Exported save to {exportPath}");
			return true;
		} catch (Exception ex) {
			Debug.LogError($"Failed to export save: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	/// Imports save data from a file.
	/// </summary>
	public async Task<bool> ImportSaveAsync(string importPath, int targetSlot) {
		try {
			if (!File.Exists(importPath)) {
				return false;
			}

			// Validate the import file
			string json = await File.ReadAllTextAsync(importPath);
			var data = JsonConvert.DeserializeObject<SaveData>(json, k_jsonSettings);

			if (data == null) {
				Debug.LogError("Invalid save file format");
				return false;
			}

			string targetPath = GetSaveFilePath(targetSlot);
			File.Copy(importPath, targetPath, overwrite: true);

			Debug.Log($"Imported save to slot {targetSlot}");
			return true;
		} catch (Exception ex) {
			Debug.LogError($"Failed to import save: {ex.Message}");
			return false;
		}
	}

	#endregion
}

#region Supporting Types

/// <summary>
/// Global save metadata (not slot-specific).
/// </summary>
public class SaveMetaData {
	public double TotalPlaytimeSeconds { get; set; }
	public DateTime LastSaveTimestamp { get; set; }
}

#endregion