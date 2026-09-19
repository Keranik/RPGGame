namespace RPGGame.Core.Metrics;

/// <summary>
/// Tracks all game metrics for both the current run and lifetime statistics.
/// Provides methods to increment, set, and query metrics.
/// Supports detailed breakdowns (e.g., enemies defeated by type).
/// </summary>
[Dependency(RegistrationType.Singleton)]
public class MetricsManager {
	#region Fields

	/// <summary>
	/// Lifetime metrics - persist across all runs (saved to meta-progression).
	/// </summary>
	private readonly Dictionary<MetricType, long> k_lifetimeMetrics = [];

	/// <summary>
	/// Current run metrics - reset on death/time rewind.
	/// </summary>
	private readonly Dictionary<MetricType, long> k_runMetrics = [];

	/// <summary>
	/// Detailed breakdowns for metrics that track "by type" data.
	/// Key format: "MetricType:SubKey" (e.g., "EnemiesDefeated:Goblin")
	/// </summary>
	private readonly Dictionary<string, long> k_lifetimeBreakdowns = [];
	private readonly Dictionary<string, long> k_runBreakdowns = [];

	/// <summary>
	/// High scores / records (e.g., longest run, highest damage dealt).
	/// </summary>
	private readonly Dictionary<string, long> k_records = [];

	/// <summary>
	/// Timestamp when current run started.
	/// </summary>
	private DateTime k_runStartTime;

	/// <summary>
	/// Timestamp when the game was last resumed (for tracking play time).
	/// </summary>
	private DateTime k_sessionStartTime;

	/// <summary>
	/// Whether we're currently in an active run.
	/// </summary>
	private bool k_isInRun;

	#endregion

	#region Events

	/// <summary>
	/// Fired when any metric changes. Useful for UI updates.
	/// </summary>
	public event Action<MetricType, long, long>? OnMetricChanged;

	/// <summary>
	/// Fired when a new record is set.
	/// </summary>
	public event Action<string, long, long>? OnRecordSet;

	/// <summary>
	/// Fired when a milestone is reached (for achievements, etc.).
	/// </summary>
	public event Action<MetricType, long>? OnMilestoneReached;

	#endregion

	#region Constructor

	public MetricsManager() {
		InitializeMetrics();
		k_sessionStartTime = DateTime.Now;
		UnityEngine.Debug.Log("MetricsManager initialized");
	}

	private void InitializeMetrics() {
		// Initialize all metrics to 0
		foreach (var metric in Enum.GetValues(typeof(MetricType)).Cast<MetricType>()) {
			k_lifetimeMetrics[metric] = 0;
			k_runMetrics[metric] = 0;
		}
	}

	#endregion

	#region Run Lifecycle

	/// <summary>
	/// Called when a new run starts.
	/// </summary>
	public void StartRun() {
		// Clear run metrics
		foreach (var metric in Enum.GetValues(typeof(MetricType)).Cast<MetricType>()) {
			k_runMetrics[metric] = 0;
		}
		k_runBreakdowns.Clear();

		k_runStartTime = DateTime.Now;
		k_isInRun = true;

		// Increment total runs
		Increment(MetricType.TotalRuns);

		UnityEngine.Debug.Log("MetricsManager: New run started");
	}

	/// <summary>
	/// Called when the current run ends (death/time rewind).
	/// Transfers appropriate metrics to lifetime stats.
	/// </summary>
	public void EndRun(bool isVictory) {
		if (!k_isInRun) {
			return;
		}

		k_isInRun = false;

		// Update time metrics
		var runDuration = DateTime.Now - k_runStartTime;
		Add(MetricType.TotalPlayTimeSeconds, (long)runDuration.TotalSeconds);

		// Check for records
		CheckAndSetRecord("LongestRunSeconds", (long)runDuration.TotalSeconds);
		CheckAndSetRecord("LongestRunDays", Get(MetricType.CurrentRunDays));
		CheckAndSetRecord("FurthestDistance", Get(MetricType.FurthestDistanceSingleRun));
		CheckAndSetRecord("MostEnemiesDefeatedSingleRun", GetRun(MetricType.EnemiesDefeated));
		CheckAndSetRecord("MostGoldEarnedSingleRun", GetRun(MetricType.GoldEarned));
		CheckAndSetRecord("MostDamageDealtSingleRun", GetRun(MetricType.DamageDealt));

		if (isVictory) {
			Increment(MetricType.FogClears);

			// Check for shortest victory
			long currentDays = Get(MetricType.CurrentRunDays);
			long shortestVictory = GetRecord("ShortestVictoryDays");
			if (shortestVictory == 0 || currentDays < shortestVictory) {
				SetRecord("ShortestVictoryDays", currentDays);
			}
		} else {
			Increment(MetricType.TimesCaughtInTime);
		}

		// Merge run breakdowns into lifetime
		foreach (var (key, value) in k_runBreakdowns) {
			if (!k_lifetimeBreakdowns.ContainsKey(key)) {
				k_lifetimeBreakdowns[key] = 0;
			}
			k_lifetimeBreakdowns[key] += value;
		}

		// Update lifetime total days survived
		Add(MetricType.TotalDaysSurvived, Get(MetricType.CurrentRunDays));

		UnityEngine.Debug.Log($"MetricsManager: Run ended (Victory: {isVictory})");
	}

	/// <summary>
	/// Called each game tick to update time-based metrics.
	/// </summary>
	public void Tick(float deltaSeconds) {
		if (!k_isInRun) {
			return;
		}

		// Track current run time
		k_runMetrics[MetricType.CurrentRunTimeSeconds] = 
			(long)(DateTime.Now - k_runStartTime).TotalSeconds;
	}

	#endregion

	#region Metric Operations

	/// <summary>
	/// Increments a metric by 1.
	/// </summary>
	public void Increment(MetricType metric) {
		Add(metric, 1);
	}

	/// <summary>
	/// Adds a value to a metric.
	/// </summary>
	public void Add(MetricType metric, long amount) {
		long oldLifetime = k_lifetimeMetrics[metric];
		long oldRun = k_runMetrics[metric];

		k_lifetimeMetrics[metric] += amount;

		if (k_isInRun) {
			k_runMetrics[metric] += amount;
		}

		OnMetricChanged?.Invoke(metric, oldLifetime, k_lifetimeMetrics[metric]);
		CheckMilestones(metric, k_lifetimeMetrics[metric]);
	}

	/// <summary>
	/// Sets a metric to a specific value (lifetime only).
	/// </summary>
	public void Set(MetricType metric, long value) {
		long oldValue = k_lifetimeMetrics[metric];
		k_lifetimeMetrics[metric] = value;

		OnMetricChanged?.Invoke(metric, oldValue, value);
		CheckMilestones(metric, value);
	}

	/// <summary>
	/// Sets the run metric to a specific value.
	/// </summary>
	public void SetRun(MetricType metric, long value) {
		k_runMetrics[metric] = value;
	}

	/// <summary>
	/// Sets a metric to the maximum of current value and new value.
	/// Useful for "highest" type metrics.
	/// </summary>
	public void SetMax(MetricType metric, long value) {
		if (value > k_lifetimeMetrics[metric]) {
			Set(metric, value);
		}
		if (k_isInRun && value > k_runMetrics[metric]) {
			k_runMetrics[metric] = value;
		}
	}

	/// <summary>
	/// Sets a metric to the minimum of current value and new value (if current > 0).
	/// Useful for "fastest" or "shortest" type metrics.
	/// </summary>
	public void SetMin(MetricType metric, long value) {
		if (k_lifetimeMetrics[metric] == 0 || value < k_lifetimeMetrics[metric]) {
			Set(metric, value);
		}
	}

	/// <summary>
	/// Gets the lifetime value of a metric.
	/// </summary>
	public long Get(MetricType metric) {
		return k_lifetimeMetrics.GetValueOrDefault(metric, 0);
	}

	/// <summary>
	/// Gets the current run value of a metric.
	/// </summary>
	public long GetRun(MetricType metric) {
		return k_runMetrics.GetValueOrDefault(metric, 0);
	}

	#endregion

	#region Detailed Breakdowns

	/// <summary>
	/// Increments a breakdown metric (e.g., enemies defeated by type).
	/// </summary>
	public void IncrementBreakdown(MetricType metric, string subKey, long amount = 1) {
		string key = $"{metric}:{subKey}";

		if (!k_lifetimeBreakdowns.ContainsKey(key)) {
			k_lifetimeBreakdowns[key] = 0;
		}
		k_lifetimeBreakdowns[key] += amount;

		if (k_isInRun) {
			if (!k_runBreakdowns.ContainsKey(key)) {
				k_runBreakdowns[key] = 0;
			}
			k_runBreakdowns[key] += amount;
		}

		// Also increment the main metric
		Add(metric, amount);
	}

	/// <summary>
	/// Gets a breakdown value.
	/// </summary>
	public long GetBreakdown(MetricType metric, string subKey, bool runOnly = false) {
		string key = $"{metric}:{subKey}";
		var dict = runOnly ? k_runBreakdowns : k_lifetimeBreakdowns;
		return dict.GetValueOrDefault(key, 0);
	}

	/// <summary>
	/// Gets all breakdowns for a metric.
	/// </summary>
	public Dictionary<string, long> GetAllBreakdowns(MetricType metric, bool runOnly = false) {
		var dict = runOnly ? k_runBreakdowns : k_lifetimeBreakdowns;
		string prefix = $"{metric}:";

		return dict
			.Where(kvp => kvp.Key.StartsWith(prefix))
			.ToDictionary(
				kvp => kvp.Key[prefix.Length..],
				kvp => kvp.Value
			);
	}

	#endregion

	#region Records

	/// <summary>
	/// Checks if value is a new record and sets it if so.
	/// </summary>
	public bool CheckAndSetRecord(string recordKey, long value) {
		long currentRecord = k_records.GetValueOrDefault(recordKey, 0);

		// For most records, higher is better
		bool isNewRecord = value > currentRecord;

		// Handle "lowest" records (those containing "Shortest", "Fastest", "Lowest")
		if (recordKey.Contains("Shortest") || recordKey.Contains("Fastest") || recordKey.Contains("Lowest")) {
			isNewRecord = currentRecord == 0 || value < currentRecord;
		}

		if (isNewRecord) {
			SetRecord(recordKey, value);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Sets a record value.
	/// </summary>
	public void SetRecord(string recordKey, long value) {
		long oldValue = k_records.GetValueOrDefault(recordKey, 0);
		k_records[recordKey] = value;
		OnRecordSet?.Invoke(recordKey, oldValue, value);
	}

	/// <summary>
	/// Gets a record value.
	/// </summary>
	public long GetRecord(string recordKey) {
		return k_records.GetValueOrDefault(recordKey, 0);
	}

	/// <summary>
	/// Gets all records.
	/// </summary>
	public IReadOnlyDictionary<string, long> GetAllRecords() {
		return k_records;
	}

	#endregion

	#region Milestones

	private static readonly Dictionary<MetricType, long[]> s_milestones = new() {
		[MetricType.EnemiesDefeated] = [10, 50, 100, 500, 1000, 5000, 10000],
		[MetricType.TilesTraveled] = [100, 500, 1000, 5000, 10000, 50000],
		[MetricType.GoldEarned] = [100, 500, 1000, 5000, 10000, 50000, 100000],
		[MetricType.TotalRuns] = [1, 5, 10, 25, 50, 100],
		[MetricType.FogClears] = [1, 3, 5, 10],
		[MetricType.CriticalHits] = [10, 50, 100, 500, 1000],
		[MetricType.ItemsCrafted] = [10, 50, 100, 500],
		[MetricType.CampsSetUp] = [10, 50, 100, 500],
		[MetricType.LoreEntriesDiscovered] = [5, 10, 25, 50, 100],
	};

	private readonly HashSet<string> k_reachedMilestones = [];

	private void CheckMilestones(MetricType metric, long value) {
		if (!s_milestones.TryGetValue(metric, out var milestones)) {
			return;
		}

		foreach (var milestone in milestones) {
			string key = $"{metric}:{milestone}";
			if (value >= milestone && !k_reachedMilestones.Contains(key)) {
				k_reachedMilestones.Add(key);
				OnMilestoneReached?.Invoke(metric, milestone);
				UnityEngine.Debug.Log($"Milestone reached: {metric} = {milestone}");
			}
		}
	}

	#endregion

	#region Convenience Methods

	/// <summary>
	/// Records an attack attempt and result.
	/// </summary>
	public void RecordAttack(bool hit, bool critical, long damage, string? enemyType = null) {
		Increment(MetricType.AttacksAttempted);

		if (hit) {
			Increment(MetricType.AttacksHit);
			Add(MetricType.DamageDealt, damage);

			if (critical) {
				Increment(MetricType.CriticalHits);
			}
		} else {
			Increment(MetricType.AttacksMissed);
		}

		if (!string.IsNullOrEmpty(enemyType)) {
			IncrementBreakdown(MetricType.AttacksAttempted, enemyType);
			if (hit) {
				IncrementBreakdown(MetricType.DamageDealt, enemyType, damage);
			}
		}
	}

	/// <summary>
	/// Records an enemy defeat.
	/// </summary>
	public void RecordEnemyDefeated(string enemyType, bool isBoss = false) {
		IncrementBreakdown(MetricType.EnemiesDefeated, enemyType);

		if (isBoss) {
			Increment(MetricType.BossesDefeated);
		}
	}

	/// <summary>
	/// Records gold transaction.
	/// </summary>
	public void RecordGoldTransaction(long amount, string category = "General") {
		if (amount > 0) {
			Add(MetricType.GoldEarned, amount);
			IncrementBreakdown(MetricType.GoldEarned, category, amount);
		} else if (amount < 0) {
			long spent = Math.Abs(amount);
			Add(MetricType.GoldSpent, spent);
			IncrementBreakdown(MetricType.GoldSpent, category, spent);

			// Track specific spending categories
			switch (category.ToLower()) {
				case "equipment":
					Add(MetricType.GoldSpentOnEquipment, spent);
					break;
				case "consumable":
				case "consumables":
					Add(MetricType.GoldSpentOnConsumables, spent);
					break;
				case "upgrade":
				case "upgrades":
					Add(MetricType.GoldSpentOnUpgrades, spent);
					break;
			}
		}
	}

	/// <summary>
	/// Records resource gathering.
	/// </summary>
	public void RecordGathering(string resourceType, long amount) {
		IncrementBreakdown(MetricType.ResourcesGathered, resourceType, amount);

		// Track specific resource types
		switch (resourceType.ToLower()) {
			case "wood":
				Add(MetricType.WoodGathered, amount);
				break;
			case "stone":
				Add(MetricType.StoneGathered, amount);
				break;
			case "ore":
				Add(MetricType.OreGathered, amount);
				break;
			case "herb":
			case "herbs":
				Add(MetricType.HerbsGathered, amount);
				break;
			case "fish":
				Add(MetricType.FishCaught, amount);
				break;
		}
	}

	/// <summary>
	/// Records a skill check.
	/// </summary>
	public void RecordSkillCheck(string skillName, bool passed) {
		Increment(MetricType.SkillChecksAttempted);
		IncrementBreakdown(MetricType.SkillChecksAttempted, skillName);

		if (passed) {
			Increment(MetricType.SkillChecksPassed);
			IncrementBreakdown(MetricType.SkillChecksPassed, skillName);
		} else {
			Increment(MetricType.SkillChecksFailed);
			IncrementBreakdown(MetricType.SkillChecksFailed, skillName);
		}
	}

	/// <summary>
	/// Records a dice roll result.
	/// </summary>
	public void RecordDiceRoll(int result, int dieSize) {
		Increment(MetricType.DiceRolled);

		if (dieSize == 20) {
			if (result == 20) {
				Increment(MetricType.Natural20sRolled);
			} else if (result == 1) {
				Increment(MetricType.Natural1sRolled);
			}
		}
	}

	/// <summary>
	/// Records tile movement.
	/// </summary>
	public void RecordTileMove(float distanceFromVillage, bool isNewTile) {
		Increment(MetricType.TilesTraveled);

		if (isNewTile) {
			Increment(MetricType.TilesRevealed);
		}

		// Update furthest distance
		SetMax(MetricType.FurthestDistanceSingleRun, (long)distanceFromVillage);
		SetMax(MetricType.FurthestDistanceEver, (long)distanceFromVillage);
	}

	#endregion

	#region Formatting

	/// <summary>
	/// Formats a metric value for display.
	/// </summary>
	public string FormatMetric(MetricType metric, long value) {
		string format = metric.GetFormatString();

		if (format == "time") {
			return FormatTime(value);
		}

		return string.Format(format, value);
	}

	/// <summary>
	/// Formats seconds into a readable time string.
	/// </summary>
	public static string FormatTime(long totalSeconds) {
		var span = TimeSpan.FromSeconds(totalSeconds);

		if (span.TotalHours >= 1) {
			return $"{(int)span.TotalHours}h {span.Minutes}m";
		} else if (span.TotalMinutes >= 1) {
			return $"{span.Minutes}m {span.Seconds}s";
		} else {
			return $"{span.Seconds}s";
		}
	}

	#endregion

	#region Serialization

	/// <summary>
	/// Gets all data needed for saving.
	/// </summary>
	public MetricsData ToData() {
		return new MetricsData {
			LifetimeMetrics = new Dictionary<MetricType, long>(k_lifetimeMetrics),
			LifetimeBreakdowns = new Dictionary<string, long>(k_lifetimeBreakdowns),
			Records = new Dictionary<string, long>(k_records),
			ReachedMilestones = [.. k_reachedMilestones],
			RunMetrics = k_isInRun ? new Dictionary<MetricType, long>(k_runMetrics) : null,
			RunBreakdowns = k_isInRun ? new Dictionary<string, long>(k_runBreakdowns) : null,
			RunStartTime = k_isInRun ? k_runStartTime : null,
			IsInRun = k_isInRun
		};
	}

	/// <summary>
	/// Loads data from a save.
	/// </summary>
	public void FromData(MetricsData data) {
		k_lifetimeMetrics.Clear();
		foreach (var (metric, value) in data.LifetimeMetrics) {
			k_lifetimeMetrics[metric] = value;
		}

		k_lifetimeBreakdowns.Clear();
		foreach (var (key, value) in data.LifetimeBreakdowns) {
			k_lifetimeBreakdowns[key] = value;
		}

		k_records.Clear();
		foreach (var (key, value) in data.Records) {
			k_records[key] = value;
		}

		k_reachedMilestones.Clear();
		foreach (var milestone in data.ReachedMilestones) {
			k_reachedMilestones.Add(milestone);
		}

		k_isInRun = data.IsInRun;

		if (data.IsInRun && data.RunMetrics != null) {
			k_runMetrics.Clear();
			foreach (var (metric, value) in data.RunMetrics) {
				k_runMetrics[metric] = value;
			}

			if (data.RunBreakdowns != null) {
				k_runBreakdowns.Clear();
				foreach (var (key, value) in data.RunBreakdowns) {
					k_runBreakdowns[key] = value;
				}
			}

			if (data.RunStartTime.HasValue) {
				k_runStartTime = data.RunStartTime.Value;
			}
		}
	}


	#endregion

	#region Debug

	/// <summary>
	/// Gets a structured summary of lifetime metrics.
	/// </summary>
	public MetricsSummary GetMetricsSummary() {
		return new MetricsSummary {
			TotalRuns = (int)Get(MetricType.TotalRuns),
			SuccessfulRuns = (int)Get(MetricType.FogClears),
			TotalDeaths = (int)Get(MetricType.TimesCaughtInTime),
			TotalDamageDealt = Get(MetricType.DamageDealt),
			TotalDamageReceived = Get(MetricType.DamageReceived),
			TotalHealingDone = Get(MetricType.HealingDone),
			TotalEnemiesDefeated = (int)Get(MetricType.EnemiesDefeated),
			TotalBossesDefeated = (int)Get(MetricType.BossesDefeated),
			TotalDistanceTraveled = Get(MetricType.TilesTraveled),
			TotalGoldEarned = Get(MetricType.GoldEarned),
			TotalGoldSpent = Get(MetricType.GoldSpent),
			TotalItemsCollected = (int)Get(MetricType.ItemsPickedUp),
			TotalLoreDiscovered = (int)Get(MetricType.LoreEntriesDiscovered),
			TotalPlaytimeSeconds = Get(MetricType.TotalPlayTimeSeconds)
		};
	}

	/// <summary>
	/// Returns a summary of all metrics.
	/// </summary>
	public string GetSummary(MetricCategory? category = null) {
		var sb = new System.Text.StringBuilder();
		sb.AppendLine("=== METRICS SUMMARY ===\n");

		var grouped = Enum.GetValues(typeof(MetricType)).Cast<MetricType>()
			.Where(m => category == null || m.GetCategory() == category)
			.GroupBy(m => m.GetCategory());

		foreach (var group in grouped) {
			sb.AppendLine($"-- {group.Key} --");

			foreach (var metric in group.Where(m => Get(m) > 0)) {
				long lifetime = Get(metric);
				long run = GetRun(metric);

				string display = FormatMetric(metric, lifetime);
				string runDisplay = k_isInRun && run > 0 ? $" (run: {FormatMetric(metric, run)})" : "";

				sb.AppendLine($"  {metric.GetDisplayName()}: {display}{runDisplay}");
			}

			sb.AppendLine();
		}

		if (k_records.Count > 0) {
			sb.AppendLine("-- Records --");
			foreach (var (key, value) in k_records) {
				sb.AppendLine($"  {key}: {value:N0}");
			}
		}

		return sb.ToString();
	}

	#endregion
}

#region Serialization Data

/// <summary>
/// Serializable metrics data.
/// </summary>
public class MetricsData {
	public Dictionary<MetricType, long> LifetimeMetrics { get; set; } = [];
	public Dictionary<string, long> LifetimeBreakdowns { get; set; } = [];
	public Dictionary<string, long> Records { get; set; } = [];
	public List<string> ReachedMilestones { get; set; } = [];
	public Dictionary<MetricType, long>? RunMetrics { get; set; }
	public Dictionary<string, long>? RunBreakdowns { get; set; }
	public DateTime? RunStartTime { get; set; }
	public bool IsInRun { get; set; }
}

/// <summary>
/// Summary of metrics for display.
/// </summary>
public class MetricsSummary {
	public int TotalRuns { get; set; }
	public int SuccessfulRuns { get; set; }
	public int TotalDeaths { get; set; }
	public long TotalDamageDealt { get; set; }
	public long TotalDamageReceived { get; set; }
	public long TotalHealingDone { get; set; }
	public int TotalEnemiesDefeated { get; set; }
	public int TotalBossesDefeated { get; set; }
	public long TotalDistanceTraveled { get; set; }
	public long TotalGoldEarned { get; set; }
	public long TotalGoldSpent { get; set; }
	public int TotalItemsCollected { get; set; }
	public int TotalLoreDiscovered { get; set; }
	public double TotalPlaytimeSeconds { get; set; }

	public string FormattedPlaytime {
		get {
			var ts = TimeSpan.FromSeconds(TotalPlaytimeSeconds);
			return ts.TotalHours >= 1
				? $"{(int)ts.TotalHours}h {ts.Minutes}m"
				: $"{ts.Minutes}m {ts.Seconds}s";
		}
	}

	public float SuccessRate => TotalRuns > 0 ? (float)SuccessfulRuns / TotalRuns * 100 : 0;
}

#endregion