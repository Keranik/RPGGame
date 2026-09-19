namespace RPGGame.Core.Metrics;

public static partial class MetricTypeExtensions {
	extension(MetricType metric) {
		/// <summary>
		/// Gets the category for this metric.
		/// </summary>
		public MetricCategory GetCategory() {
			return metric switch {
				>= MetricType.AttacksAttempted and <= MetricType.QuickestCombatTurns => MetricCategory.Combat,
				>= MetricType.GoldEarned and <= MetricType.AnimalsHunted => MetricCategory.Resources,
				>= MetricType.TilesTraveled and <= MetricType.ExpeditionsStarted => MetricCategory.Exploration,
				>= MetricType.EventsEncountered and <= MetricType.NPCsIgnored => MetricCategory.Choices,
				>= MetricType.SkillsUsed and <= MetricType.SkillPointsSpent => MetricCategory.Skills,
				>= MetricType.TotalRuns and <= MetricType.ClassesUnlocked => MetricCategory.Meta,
				>= MetricType.TimesSaved and <= MetricType.TimesLoaded => MetricCategory.SaveLoad,
				>= MetricType.TotalPlayTimeSeconds and <= MetricType.CurrentRunDays => MetricCategory.Time,
				_ => MetricCategory.Misc
			};
		}
		/// <summary>
		/// Gets a display-friendly name for this metric.
		/// </summary>
		public string GetDisplayName() {
			var name = metric.ToString();
			var result = new System.Text.StringBuilder();

			foreach (char c in name) {
				if (char.IsUpper(c) && result.Length > 0) {
					result.Append(' ');
				}
				result.Append(c);
			}

			return result.ToString();
		}
		/// <summary>
		/// Gets whether this metric should be tracked per-run only.
		/// </summary>
		public bool IsPerRunMetric() {
			return metric is
				MetricType.CurrentRunTimeSeconds or
				MetricType.CurrentRunDays or
				MetricType.FurthestDistanceSingleRun;
		}
		/// <summary>
		/// Gets whether higher values are "better" for this metric.
		/// </summary>
		public bool HigherIsBetter() {
			return metric is not (
				MetricType.AttacksMissed or
				MetricType.DamageReceived or
				MetricType.CriticalHitsReceived or
				MetricType.GoldLost or
				MetricType.GoldSpent or
				MetricType.GoldSpentOnEquipment or
				MetricType.GoldSpentOnConsumables or
				MetricType.GoldSpentOnUpgrades or
				MetricType.ItemsDiscarded or
				MetricType.TrapsTriggered or
				MetricType.SkillChecksFailed or
				MetricType.NPCsHarmed or
				MetricType.NPCsIgnored or
				MetricType.TimesCaughtInTime or
				MetricType.DeathsTotal or
				MetricType.Natural1sRolled or
				MetricType.StepsBackwards or
				MetricType.TimesGotLost or
				MetricType.MaxDaysWithoutSleep or
				MetricType.TimesMoraleHitZero or
				MetricType.TimesRanOutOfFood or
				MetricType.DeadEndsDiscovered
				);
		}
		/// <summary>
		/// Gets the format string for displaying this metric's value.
		/// </summary>
		public string GetFormatString() {
			return metric switch {
				MetricType.TotalPlayTimeSeconds or
					MetricType.TimeInCombatSeconds or
					MetricType.TimeTravelingSeconds or
					MetricType.TimeInMenusSeconds or
					MetricType.CurrentRunTimeSeconds => "time",

				MetricType.GoldEarned or
					MetricType.GoldSpent or
					MetricType.GoldSpentOnEquipment or
					MetricType.GoldSpentOnConsumables or
					MetricType.GoldSpentOnUpgrades or
					MetricType.GoldLost or
					MetricType.GoldGivenAway => "{0:N0}g",

				MetricType.TilesTraveled or
					MetricType.TilesExplored or
					MetricType.TilesRevealed or
					MetricType.DistanceTraveled or
					MetricType.FurthestDistanceSingleRun or
					MetricType.FurthestDistanceEver or
					MetricType.FurthestDistance => "{0:N0} tiles",

				MetricType.HoursRested => "{0:N1} hrs",

				MetricType.TotalDaysSurvived or
					MetricType.CurrentRunDays or
					MetricType.LongestRunDays or
					MetricType.ShortestVictoryDays or
					MetricType.MaxDaysWithoutSleep => "{0:N0} days",

				_ => "{0:N0}"
			};
		}
		/// <summary>
		/// Gets whether this metric represents a "high score" type value (max tracked, not sum).
		/// </summary>
		public bool IsHighScoreMetric() {
			return metric is
				MetricType.LongestCombatTurns or
				MetricType.FurthestDistanceSingleRun or
				MetricType.FurthestDistanceEver or
				MetricType.FurthestDistance or
				MetricType.LongestRunDays or
				MetricType.HighestLevel or
				MetricType.MaxDaysWithoutSleep;
		}
		/// <summary>
		/// Gets whether this metric represents a "low score" type value (min tracked, not sum).
		/// </summary>
		public bool IsLowScoreMetric() {
			return metric is
				MetricType.QuickestCombatTurns or
				MetricType.ShortestVictoryDays;
		}
	}
}
