using System;
using Unity.Properties;
using UnityEngine;

namespace RPGGame.Core;

/// <summary>
/// Singleton service managing game time progression.
/// Provides the central tick counter, time unit conversions, and time advancement.
/// 
/// Integrates with:
/// - <see cref="Duration"/> for time spans (cooldowns, buffs, travel time)
/// - <see cref="TimestampRPG"/> for moments in time (when events occurred)
/// 
/// <example>
/// <code>
/// // Advance time
/// GameTime.Instance.FastForward(8.Hours());
/// GameTime.Instance.AdvanceTurn();
/// 
/// // Get current time
/// TimestampRPG now = GameTime.Now;
/// Debug.Log(GameTime.Instance.FormattedDateTime);
/// 
/// // Check elapsed time
/// Duration elapsed = GameTime.Instance.ElapsedDuration;
/// </code>
/// </example>
/// </summary>
[Dependency(registrationType: RegistrationType.Singleton)]
public class GameTime {
    #region Constants

    /// <summary>Ticks per second/turn. One turn = one second = 10 ticks.</summary>
    public const int TICKS_PER_SECOND = 10;

	/// <summary>Seconds per minute.</summary>
    public const int SECONDS_PER_MINUTE = 60;

    /// <summary>Minutes per hour.</summary>
    public const int MINUTES_PER_HOUR = 60;

    /// <summary>Hours per day.</summary>
    public const int HOURS_PER_DAY = 24;

    /// <summary>Days per month.</summary>
    public const int DAYS_PER_MONTH = 30;

    /// <summary>Months per year.</summary>
    public const int MONTHS_PER_YEAR = 12;

	/// <summary>Default starting hour for new games.</summary>
	public const int DEFAULT_STARTING_HOUR = 8;

    // Pre-calculated tick multipliers for performance
    internal static readonly long TICKS_PER_MINUTE = TICKS_PER_SECOND * SECONDS_PER_MINUTE;
    internal static readonly long TICKS_PER_HOUR = TICKS_PER_MINUTE * MINUTES_PER_HOUR;
    internal static readonly long TICKS_PER_DAY = TICKS_PER_HOUR * HOURS_PER_DAY;
    internal static readonly long TICKS_PER_MONTH = TICKS_PER_DAY * DAYS_PER_MONTH;
    internal static readonly long TICKS_PER_YEAR = TICKS_PER_MONTH * MONTHS_PER_YEAR;
	public static readonly long SECONDS_PER_HOUR = SECONDS_PER_MINUTE * MINUTES_PER_HOUR;
    
	/// <summary>Default starting ticks (8AM on day 1).</summary>
	internal static readonly long DEFAULT_STARTING_TICKS = TICKS_PER_HOUR * DEFAULT_STARTING_HOUR;

    #endregion

    #region Singleton

    private static GameTime? s_instance;

    /// <summary>
    /// Gets the singleton instance of GameTime.
    /// </summary>
    /// <remarks>
    /// This is set by the dependency injection system. 
    /// If accessed before initialization, creates a temporary instance.
    /// </remarks>
    public static GameTime Instance {
        get {
            if (s_instance == null) {
                Debug.LogWarning("GameTime.Instance accessed before initialization. Creating temporary instance.");
                s_instance = new GameTime();
            }
            return s_instance;
        }
        internal set => s_instance = value;
    }

    #endregion

    #region Fields

    private long k_totalTicks;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new GameTime instance.
    /// </summary>
    public GameTime() {
        k_totalTicks = DEFAULT_STARTING_TICKS;

		_lastHour = DEFAULT_STARTING_HOUR;
		_lastDay = 1;
		_lastTimeOfDay = TimeOfDayPeriod.Midday;

        s_instance = this;
        Debug.Log("GameTime Initialized");
    }

    #endregion

    #region Properties - Raw Ticks (Backward Compatible)

    /// <summary>
    /// Gets the total ticks elapsed since game start.
    /// </summary>
    [CreateProperty]
    public long TotalTicks => k_totalTicks;

    #endregion

    #region Properties - Time Components (Backward Compatible)

    /// <summary>Gets the current tick within the second (0-9).</summary>
    [CreateProperty]
    public int Tick => (int)(k_totalTicks % TICKS_PER_SECOND);

    /// <summary>Gets the current second within the minute (0-0, since SECONDS_PER_MINUTE=1).</summary>
    [CreateProperty]
	public int Second => (int)(k_totalTicks / TICKS_PER_SECOND % SECONDS_PER_MINUTE);

    /// <summary>Gets the current minute within the hour (0-59).</summary>
    [CreateProperty]
    public int Minute => (int)(k_totalTicks / TICKS_PER_MINUTE % MINUTES_PER_HOUR);

    /// <summary>Gets the current hour within the day (0-23).</summary>
    [CreateProperty]
    public int Hour => (int)(k_totalTicks / TICKS_PER_HOUR % HOURS_PER_DAY);

    /// <summary>Gets the current day within the month (1-30).</summary>
    [CreateProperty]
    public int Day => (int)(k_totalTicks / TICKS_PER_DAY % DAYS_PER_MONTH) + 1;

    /// <summary>Gets the current month within the year (1-12).</summary>
    [CreateProperty]
    public int Month => (int)(k_totalTicks / TICKS_PER_MONTH % MONTHS_PER_YEAR) + 1;

    /// <summary>Gets the current year (1+).</summary>
    [CreateProperty]
    public int Year => (int)(k_totalTicks / TICKS_PER_YEAR) + 1;

    #endregion

    #region Properties - Formatted Display (Backward Compatible)

    /// <summary>
    /// Gets formatted time string: "[HH:MM]"
    /// </summary>
    [CreateProperty]
    public string FormattedTime => $"[{Hour:D2}:{Minute:D2}]";

    /// <summary>
    /// Gets formatted date string: "Year X : Day (Y)"
    /// </summary>
    [CreateProperty]
    public string FormattedDate => $"Year {Year} : Day ({Day})";

    /// <summary>
    /// Gets formatted full date-time string.
    /// </summary>
    [CreateProperty]
    public string FormattedDateTime => $"{FormattedDate} {FormattedTime}";

    /// <summary>
    /// Gets compact date-time string: "Y1 M3 D15 12:30"
    /// </summary>
    [CreateProperty]
    public string FormattedCompact => $"Y{Year} M{Month} D{Day} {Hour:D2}:{Minute:D2}";

    #endregion

    #region Properties - Elapsed Time (Backward Compatible)

    /// <summary>
    /// Gets the total elapsed game time as a TimeSpan.
    /// </summary>
    public TimeSpan ElapsedTime {
        get {
            double totalSeconds = (double)k_totalTicks / TICKS_PER_SECOND;
            return TimeSpan.FromSeconds(totalSeconds);
        }
    }

    #endregion

    #region Properties - Duration & Timestamp Integration (New)

    /// <summary>
    /// Gets the total elapsed time as a Duration.
    /// </summary>
    [CreateProperty]
    public Duration ElapsedDuration => Duration.FromTicks(k_totalTicks);

    /// <summary>
    /// Gets the current moment as a TimestampRPG.
    /// Equivalent to TimestampRPG.Now.
    /// </summary>
    public TimestampRPG CurrentTimestamp => new TimestampRPG(k_totalTicks);

    /// <summary>
    /// Static shortcut to get the current timestamp.
    /// </summary>
    public static TimestampRPG Now => new TimestampRPG(Instance.k_totalTicks);

    #endregion

    #region Properties - Time of Day (New)

    /// <summary>
    /// Gets whether it's currently daytime (6:00 - 17:59).
    /// </summary>
    [CreateProperty]
    public bool IsDaytime => Hour >= 6 && Hour < 18;

    /// <summary>
    /// Gets whether it's currently nighttime (18:00 - 5:59).
    /// </summary>
    [CreateProperty]
    public bool IsNighttime => !IsDaytime;

    /// <summary>
    /// Gets whether it's currently dawn (5:00 - 6:59).
    /// </summary>
    [CreateProperty]
    public bool IsDawn => Hour >= 5 && Hour < 7;

    /// <summary>
    /// Gets whether it's currently dusk (17:00 - 18:59).
    /// </summary>
    [CreateProperty]
    public bool IsDusk => Hour >= 17 && Hour < 19;

    /// <summary>
    /// Gets the current time of day period.
    /// </summary>
    [CreateProperty]
    public TimeOfDayPeriod TimeOfDay {
        get {
            return Hour switch {
                >= 5 and < 7 => TimeOfDayPeriod.Dawn,
                >= 7 and < 12 => TimeOfDayPeriod.Morning,
                >= 12 and < 14 => TimeOfDayPeriod.Midday,
                >= 14 and < 17 => TimeOfDayPeriod.Afternoon,
                >= 17 and < 19 => TimeOfDayPeriod.Dusk,
                >= 19 and < 22 => TimeOfDayPeriod.Evening,
                _ => TimeOfDayPeriod.Night
            };
        }
    }

    #endregion

    #region Methods - Time Advancement (Backward Compatible + Enhanced)

    /// <summary>
    /// Advances time by a certain number of ticks.
    /// </summary>
    /// <param name="ticks">Number of ticks to advance.</param>
    public void AdvanceTicks(int ticks) {
        if (ticks <= 0) return;
        
        long previousTicks = k_totalTicks;
        k_totalTicks += ticks;
        
        OnTimeAdvanced(previousTicks, k_totalTicks);
    }

    /// <summary>
    /// Advances time by a certain number of ticks (long overload).
    /// </summary>
    /// <param name="ticks">Number of ticks to advance.</param>
    public void AdvanceTicks(long ticks) {
        if (ticks <= 0) return;
        
        long previousTicks = k_totalTicks;
        k_totalTicks += ticks;
        
        OnTimeAdvanced(previousTicks, k_totalTicks);
    }

    /// <summary>
    /// Advances time by a Duration. Use for resting, traveling, crafting, etc.
    /// </summary>
    /// <param name="duration">The duration to advance.</param>
    public void FastForward(Duration duration) {
        if (duration.IsZero) return;
        
        long previousTicks = k_totalTicks;
        k_totalTicks += duration.TotalTicks;
        
        OnTimeAdvanced(previousTicks, k_totalTicks);
    }

    /// <summary>
    /// Advances time by one turn (one second = TICKS_PER_SECOND ticks).
    /// </summary>
    public void AdvanceTurn() {
        AdvanceTicks(TICKS_PER_SECOND);
    }

    /// <summary>
    /// Advances time by multiple turns.
    /// </summary>
    /// <param name="turns">Number of turns to advance.</param>
    public void AdvanceTurns(int turns) {
        if (turns <= 0) return;
        AdvanceTicks(turns * TICKS_PER_SECOND);
    }

    /// <summary>
    /// Advances time by one hour.
    /// </summary>
    public void AdvanceHour() {
        FastForward(Duration.OneHour);
    }

    /// <summary>
    /// Advances time by one day.
    /// </summary>
    public void AdvanceDay() {
        FastForward(Duration.OneDay);
    }

    #endregion

    #region Methods - Time Queries (New)

    /// <summary>
    /// Gets the duration elapsed since a specific timestamp.
    /// </summary>
    /// <param name="since">The earlier timestamp.</param>
    /// <returns>Duration elapsed since that time.</returns>
    public Duration TimeSince(TimestampRPG since) {
        long diff = k_totalTicks - since.TotalTicks;
        return Duration.FromTicks(Math.Max(0, diff));
    }

    /// <summary>
    /// Gets the duration until a future timestamp.
    /// Returns Zero if the timestamp is in the past.
    /// </summary>
    /// <param name="until">The future timestamp.</param>
    /// <returns>Duration until that time.</returns>
    public Duration TimeUntil(TimestampRPG until) {
        long diff = until.TotalTicks - k_totalTicks;
        return Duration.FromTicks(Math.Max(0, diff));
    }

    /// <summary>
    /// Checks if a specific duration has passed since a timestamp.
    /// </summary>
    /// <param name="since">The starting timestamp.</param>
    /// <param name="duration">The duration to check.</param>
    /// <returns>True if the duration has elapsed.</returns>
    public bool HasPassed(TimestampRPG since, Duration duration) {
        return (k_totalTicks - since.TotalTicks) >= duration.TotalTicks;
    }

    /// <summary>
    /// Gets a future timestamp by adding a duration to now.
    /// </summary>
    /// <param name="fromNow">Duration from now.</param>
    /// <returns>The future timestamp.</returns>
    public TimestampRPG GetFutureTime(Duration fromNow) {
        return new TimestampRPG(k_totalTicks + fromNow.TotalTicks);
    }

    /// <summary>
    /// Gets a past timestamp by subtracting a duration from now.
    /// </summary>
    /// <param name="ago">Duration ago.</param>
    /// <returns>The past timestamp.</returns>
    public TimestampRPG GetPastTime(Duration ago) {
        return new TimestampRPG(Math.Max(0, k_totalTicks - ago.TotalTicks));
    }

    #endregion

    #region Methods - Set Time (New - for save/load)

    /// <summary>
    /// Sets the total ticks directly. Use for save/load operations.
    /// </summary>
    /// <param name="ticks">The tick count to set.</param>
    public void SetTotalTicks(long ticks) {
        k_totalTicks = Math.Max(0, ticks);
    }

    /// <summary>
    /// Sets the time from a timestamp. Use for save/load operations.
    /// </summary>
    /// <param name="timestamp">The timestamp to restore to.</param>
    public void SetTime(TimestampRPG timestamp) {
        k_totalTicks = Math.Max(0, timestamp.TotalTicks);
    }

    /// <summary>
    /// Resets time to zero (new game).
    /// </summary>
    public void Reset() {
        k_totalTicks = 0;
        Debug.Log("GameTime Reset");
    }

    #endregion

    #region Events (New)

    /// <summary>
    /// Fired when time advances. Provides previous and new tick counts.
    /// </summary>
    public event Action<long, long>? OnTimeChanged;

    /// <summary>
    /// Fired when a new hour begins.
    /// </summary>
    public event Action<int>? OnHourChanged;

    /// <summary>
    /// Fired when a new day begins.
    /// </summary>
    public event Action<int>? OnDayChanged;

    /// <summary>
    /// Fired when time of day period changes (dawn, day, dusk, night).
    /// </summary>
    public event Action<TimeOfDayPeriod>? OnTimeOfDayChanged;

    private int _lastHour = -1;
    private int _lastDay = -1;
    private TimeOfDayPeriod _lastTimeOfDay;

    private void OnTimeAdvanced(long previousTicks, long newTicks) {
        OnTimeChanged?.Invoke(previousTicks, newTicks);

        // Check for hour change
        int currentHour = Hour;
        if (_lastHour != currentHour) {
            _lastHour = currentHour;
            OnHourChanged?.Invoke(currentHour);
        }

        // Check for day change
        int currentDay = Day;
        if (_lastDay != currentDay) {
            _lastDay = currentDay;
            OnDayChanged?.Invoke(currentDay);
        }

        // Check for time of day change
        TimeOfDayPeriod currentPeriod = TimeOfDay;
        if (_lastTimeOfDay != currentPeriod) {
            _lastTimeOfDay = currentPeriod;
            OnTimeOfDayChanged?.Invoke(currentPeriod);
        }
    }

    #endregion
}

#region Time of Day Period

/// <summary>
/// Periods of the day for gameplay effects (visibility, encounters, etc.).
/// </summary>
public enum TimeOfDayPeriod {
    /// <summary>Early morning (5:00 - 6:59).</summary>
    Dawn,
    /// <summary>Morning (7:00 - 11:59).</summary>
    Morning,
    /// <summary>Midday (12:00 - 13:59).</summary>
    Midday,
    /// <summary>Afternoon (14:00 - 16:59).</summary>
    Afternoon,
    /// <summary>Evening transition (17:00 - 18:59).</summary>
    Dusk,
    /// <summary>Evening (19:00 - 21:59).</summary>
    Evening,
    /// <summary>Nighttime (22:00 - 4:59).</summary>
    Night
}

/// <summary>
/// Extension methods for TimeOfDayPeriod.
/// </summary>
public static class TimeOfDayPeriodExtensions {
    /// <summary>
    /// Gets a display name for the time period.
    /// </summary>
    public static string GetDisplayName(this TimeOfDayPeriod period) {
        return period switch {
            TimeOfDayPeriod.Dawn => "Dawn",
            TimeOfDayPeriod.Morning => "Morning",
            TimeOfDayPeriod.Midday => "Midday",
            TimeOfDayPeriod.Afternoon => "Afternoon",
            TimeOfDayPeriod.Dusk => "Dusk",
            TimeOfDayPeriod.Evening => "Evening",
            TimeOfDayPeriod.Night => "Night",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets whether this period is considered "day" for gameplay purposes.
    /// </summary>
    public static bool IsDay(this TimeOfDayPeriod period) {
        return period is TimeOfDayPeriod.Dawn or TimeOfDayPeriod.Morning 
            or TimeOfDayPeriod.Midday or TimeOfDayPeriod.Afternoon or TimeOfDayPeriod.Dusk;
    }

    /// <summary>
    /// Gets whether this period is considered "night" for gameplay purposes.
    /// </summary>
    public static bool IsNight(this TimeOfDayPeriod period) {
        return period is TimeOfDayPeriod.Evening or TimeOfDayPeriod.Night;
    }

    /// <summary>
    /// Gets a visibility modifier for this time period (1.0 = full visibility).
    /// </summary>
    public static float GetVisibilityModifier(this TimeOfDayPeriod period) {
        return period switch {
            TimeOfDayPeriod.Dawn => 0.7f,
            TimeOfDayPeriod.Morning => 1.0f,
            TimeOfDayPeriod.Midday => 1.0f,
            TimeOfDayPeriod.Afternoon => 1.0f,
            TimeOfDayPeriod.Dusk => 0.7f,
            TimeOfDayPeriod.Evening => 0.5f,
            TimeOfDayPeriod.Night => 0.3f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Gets an encounter rate modifier for this time period.
    /// </summary>
    public static float GetEncounterModifier(this TimeOfDayPeriod period) {
        return period switch {
            TimeOfDayPeriod.Dawn => 0.8f,
            TimeOfDayPeriod.Morning => 1.0f,
            TimeOfDayPeriod.Midday => 0.9f,
            TimeOfDayPeriod.Afternoon => 1.0f,
            TimeOfDayPeriod.Dusk => 1.2f,
            TimeOfDayPeriod.Evening => 1.3f,
            TimeOfDayPeriod.Night => 1.5f,
            _ => 1.0f
        };
    }
}

#endregion

#region GameTime Extensions

/// <summary>
/// Convenience extension methods for GameTime.
/// </summary>
public static class GameTimeExtensions {
    /// <summary>
    /// Fast-forwards time by the specified number of hours.
    /// </summary>
    public static void FastForwardHours(this GameTime gameTime, int hours) {
        gameTime.FastForward(hours.Hours());
    }

    /// <summary>
    /// Fast-forwards time by the specified number of days.
    /// </summary>
    public static void FastForwardDays(this GameTime gameTime, int days) {
        gameTime.FastForward(days.Days());
    }

    /// <summary>
    /// Fast-forwards time to the next dawn (5:00).
    /// </summary>
    public static void FastForwardToNextDawn(this GameTime gameTime) {
        int currentHour = gameTime.Hour;
        int hoursUntilDawn = currentHour < 5 ? (5 - currentHour) : (24 - currentHour + 5);
        int minutesToAdvance = hoursUntilDawn * GameTime.MINUTES_PER_HOUR - gameTime.Minute;
        gameTime.FastForward(minutesToAdvance.Minutes());
    }

    /// <summary>
    /// Fast-forwards time to the next night (22:00).
    /// </summary>
    public static void FastForwardToNextNight(this GameTime gameTime) {
        int currentHour = gameTime.Hour;
        int hoursUntilNight = currentHour < 22 ? (22 - currentHour) : (24 - currentHour + 22);
        int minutesToAdvance = hoursUntilNight * GameTime.MINUTES_PER_HOUR - gameTime.Minute;
        gameTime.FastForward(minutesToAdvance.Minutes());
    }

    /// <summary>
    /// Fast-forwards time to a specific hour today (or tomorrow if already past).
    /// </summary>
    public static void FastForwardToHour(this GameTime gameTime, int targetHour) {
        targetHour = Math.Clamp(targetHour, 0, 23);
        int currentHour = gameTime.Hour;
        int hoursToAdvance = currentHour < targetHour 
            ? (targetHour - currentHour) 
            : (24 - currentHour + targetHour);
        int minutesToAdvance = hoursToAdvance * GameTime.MINUTES_PER_HOUR - gameTime.Minute;
        gameTime.FastForward(minutesToAdvance.Minutes());
    }
}

#endregion

#region Usage Examples
/*
 * ═══════════════════════════════════════════════════════════════════════════
 * GAMETIME USAGE EXAMPLES
 * ═══════════════════════════════════════════════════════════════════════════
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // BASIC TIME ADVANCEMENT
 * // ─────────────────────────────────────────────────────────────────────────
 * GameTime.Instance.AdvanceTurn();              // One combat turn
 * GameTime.Instance.AdvanceTurns(5);            // Five turns
 * GameTime.Instance.AdvanceTicks(50);           // Precise tick control
 * GameTime.Instance.FastForward(2.Hours());     // Using Duration
 * GameTime.Instance.FastForward(8.Hours());     // Full rest
 * GameTime.Instance.FastForward(1.Days());      // Skip a day
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // GETTING CURRENT TIME
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG now = GameTime.Now;
 * TimestampRPG also = GameTime.Instance.CurrentTimestamp;
 * 
 * Debug.Log(GameTime.Instance.FormattedTime);     // "[14:30]"
 * Debug.Log(GameTime.Instance.FormattedDate);     // "Year 1 : Day (15)"
 * Debug.Log(GameTime.Instance.FormattedDateTime); // "Year 1 : Day (15) [14:30]"
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // TIME OF DAY EFFECTS
 * // ─────────────────────────────────────────────────────────────────────────
 * if (GameTime.Instance.IsNighttime) {
 *     ApplyNightPenalties();
 * }
 * 
 * var period = GameTime.Instance.TimeOfDay;
 * float visibility = period.GetVisibilityModifier();
 * float encounterRate = period.GetEncounterModifier();
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // COOLDOWN TRACKING WITH TIMESTAMPS
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG abilityUsed = GameTime.Now;
 * Duration cooldown = 30.Seconds();
 * 
 * // Later...
 * if (GameTime.Instance.HasPassed(abilityUsed, cooldown)) {
 *     // Ability is ready!
 * }
 * 
 * Duration remaining = abilityUsed.RemainingTime(cooldown);
 * Debug.Log($"Ready in: {remaining.ToDisplayCompact()}");
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // TIME QUERIES
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG lastMeal = ...; // Stored earlier
 * Duration timeSinceFood = GameTime.Instance.TimeSince(lastMeal);
 * 
 * if (timeSinceFood >= 8.Hours()) {
 *     ApplyHunger();
 * }
 * 
 * TimestampRPG shopOpens = GameTime.Instance.GetFutureTime(2.Hours());
 * Duration waitTime = GameTime.Instance.TimeUntil(shopOpens);
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // RESTING UNTIL A SPECIFIC TIME
 * // ─────────────────────────────────────────────────────────────────────────
 * GameTime.Instance.FastForwardToNextDawn();  // Rest until morning
 * GameTime.Instance.FastForwardToHour(6);     // Wake up at 6am
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // TIME EVENTS
 * // ─────────────────────────────────────────────────────────────────────────
 * GameTime.Instance.OnHourChanged += (hour) => {
 *     Debug.Log($"It's now {hour}:00");
 * };
 * 
 * GameTime.Instance.OnDayChanged += (day) => {
 *     Debug.Log($"A new day dawns: Day {day}");
 *     ProcessDailyEvents();
 * };
 * 
 * GameTime.Instance.OnTimeOfDayChanged += (period) => {
 *     UpdateLighting(period);
 *     UpdateAmbientSounds(period);
 * };
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // SAVE/LOAD
 * // ─────────────────────────────────────────────────────────────────────────
 * // Save
 * long savedTicks = GameTime.Instance.TotalTicks;
 * 
 * // Load
 * GameTime.Instance.SetTotalTicks(savedTicks);
 * // or
 * GameTime.Instance.SetTime(savedTimestamp);
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // COMBINED WORKFLOW: BUFF WITH EXPIRATION
 * // ─────────────────────────────────────────────────────────────────────────
 * class TimedBuff {
 *     public TimestampRPG AppliedAt { get; }
 *     public Duration TotalDuration { get; }
 *     
 *     public bool IsExpired => AppliedAt.HasPassed(TotalDuration);
 *     public Duration Remaining => AppliedAt.RemainingTime(TotalDuration);
 *     public float Progress => AppliedAt.Progress(TotalDuration);
 *     
 *     public TimedBuff(Duration duration) {
 *         AppliedAt = GameTime.Now;
 *         TotalDuration = duration;
 *     }
 * }
 * 
 * var strengthBuff = new TimedBuff(5.Turns());
 * var restingHeal = new TimedBuff(8.Hours());
 */
#endregion