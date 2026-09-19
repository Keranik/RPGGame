using System;

namespace RPGGame.Core;

/// <summary>
/// An immutable wrapper representing a specific moment in game time (a snapshot of TotalTicks).
/// Used for tracking when events occurred, cooldown start times, buff application times, etc.
/// <example>
/// <code>
/// TimestampRPG abilityUsed = TimestampRPG.Now;
/// // ... later ...
/// if (TimestampRPG.Now - abilityUsed >= 30.Seconds()) {
///     AllowAbilityUse();
/// }
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct TimestampRPG : IEquatable<TimestampRPG>, IComparable<TimestampRPG>, IFormattable {
    #region Fields

    private readonly long _ticks;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new TimestampRPG from raw ticks.
    /// </summary>
    /// <param name="ticks">The tick count representing this moment in time.</param>
    public TimestampRPG(long ticks) {
        _ticks = ticks;
    }

    #endregion

    #region Static Properties

    /// <summary>
    /// Gets the current game time as a TimestampRPG.
    /// </summary>
    /// <remarks>
    /// Requires GameTime.Instance to be initialized.
    /// </remarks>
    public static TimestampRPG Now => new(GameTime.Instance.TotalTicks);

    /// <summary>
    /// The epoch (tick 0) - the start of game time.
    /// </summary>
    public static TimestampRPG Epoch => new(0);

    /// <summary>
    /// An invalid/unset timestamp marker. Use for "no timestamp" scenarios.
    /// </summary>
    public static TimestampRPG Invalid => new(long.MinValue);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the raw tick count for this timestamp.
    /// </summary>
    public long TotalTicks => _ticks;

    /// <summary>
    /// Returns true if this is the epoch (tick 0).
    /// </summary>
    public bool IsEpoch => _ticks == 0;

    /// <summary>
    /// Returns true if this is an invalid/unset timestamp.
    /// </summary>
    public bool IsInvalid => _ticks == long.MinValue;

    /// <summary>
    /// Returns true if this is a valid timestamp.
    /// </summary>
    public bool IsValid => !IsInvalid;

    /// <summary>
    /// Gets the tick component (0-9) of this timestamp.
    /// </summary>
    public int Tick => (int)(_ticks % GameTime.TICKS_PER_SECOND);

    /// <summary>
    /// Gets the second/turn component (0-59) of this timestamp.
    /// </summary>
    public int Second => (int)(_ticks / GameTime.TICKS_PER_SECOND % GameTime.SECONDS_PER_MINUTE);

    /// <summary>
    /// Gets the minute component (0-59) of this timestamp.
    /// </summary>
    public int Minute => (int)(_ticks / (GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE) % GameTime.MINUTES_PER_HOUR);

    /// <summary>
    /// Gets the hour component (0-23) of this timestamp.
    /// </summary>
    public int Hour => (int)(_ticks / (GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE * GameTime.MINUTES_PER_HOUR) % GameTime.HOURS_PER_DAY);

    /// <summary>
    /// Gets the day component (1-30) of this timestamp.
    /// </summary>
    public int Day => (int)(_ticks / (GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE * GameTime.MINUTES_PER_HOUR * GameTime.HOURS_PER_DAY) % GameTime.DAYS_PER_MONTH) + 1;

    /// <summary>
    /// Gets the month component (1-12) of this timestamp.
    /// </summary>
    public int Month => (int)(_ticks / (GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE * GameTime.MINUTES_PER_HOUR * GameTime.HOURS_PER_DAY * GameTime.DAYS_PER_MONTH) % GameTime.MONTHS_PER_YEAR) + 1;

    /// <summary>
    /// Gets the year component (1+) of this timestamp.
    /// </summary>
    public int Year => (int)(_ticks / (GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE * GameTime.MINUTES_PER_HOUR * GameTime.HOURS_PER_DAY * GameTime.DAYS_PER_MONTH * GameTime.MONTHS_PER_YEAR)) + 1;

    #endregion

    #region Static Factories

    /// <summary>
    /// Creates a TimestampRPG from raw ticks.
    /// </summary>
    public static TimestampRPG FromTicks(long ticks) => new(ticks);

    /// <summary>
    /// Creates a TimestampRPG from date/time components.
    /// </summary>
    /// <param name="year">Year (1+).</param>
    /// <param name="month">Month (1-12).</param>
    /// <param name="day">Day (1-30).</param>
    /// <param name="hour">Hour (0-23).</param>
    /// <param name="minute">Minute (0-59).</param>
    /// <param name="second">Second/Turn (0-59).</param>
    public static TimestampRPG FromDateTime(int year = 1, int month = 1, int day = 1, int hour = 0, int minute = 0, int second = 0) {
        long ticks = 0;
        ticks += (year - 1) * GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE * GameTime.MINUTES_PER_HOUR * GameTime.HOURS_PER_DAY * GameTime.DAYS_PER_MONTH * GameTime.MONTHS_PER_YEAR;
        ticks += (month - 1) * GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE * GameTime.MINUTES_PER_HOUR * GameTime.HOURS_PER_DAY * GameTime.DAYS_PER_MONTH;
        ticks += (day - 1) * GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE * GameTime.MINUTES_PER_HOUR * GameTime.HOURS_PER_DAY;
        ticks += hour * GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE * GameTime.MINUTES_PER_HOUR;
        ticks += minute * GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE;
        ticks += second * GameTime.TICKS_PER_SECOND;
        return new TimestampRPG(ticks);
    }

    /// <summary>
    /// Returns the minimum of two timestamps.
    /// </summary>
    public static TimestampRPG Min(TimestampRPG a, TimestampRPG b) => new(Math.Min(a._ticks, b._ticks));

    /// <summary>
    /// Returns the maximum of two timestamps.
    /// </summary>
    public static TimestampRPG Max(TimestampRPG a, TimestampRPG b) => new(Math.Max(a._ticks, b._ticks));

    #endregion

    #region Instance Methods - Duration Calculations

    /// <summary>
    /// Gets the duration elapsed since an earlier timestamp.
    /// </summary>
    /// <param name="earlier">The earlier timestamp to measure from.</param>
    /// <returns>The duration between the timestamps (always non-negative).</returns>
    public Duration ElapsedSince(TimestampRPG earlier) {
        long diff = _ticks - earlier._ticks;
        return new Duration(Math.Max(0, diff));
    }

    /// <summary>
    /// Gets the duration until a later timestamp.
    /// </summary>
    /// <param name="later">The later timestamp to measure to.</param>
    /// <returns>The duration between the timestamps (always non-negative).</returns>
    public Duration DurationUntil(TimestampRPG later) {
        long diff = later._ticks - _ticks;
        return new Duration(Math.Max(0, diff));
    }

    /// <summary>
    /// Gets the duration from this timestamp until now.
    /// </summary>
    /// <returns>The duration since this timestamp.</returns>
    public Duration TimeSinceNow() {
        return Now.ElapsedSince(this);
    }

    /// <summary>
    /// Gets the duration from now until this timestamp.
    /// Returns Zero if this timestamp is in the past.
    /// </summary>
    public Duration TimeUntilNow() {
        return DurationUntil(Now);
    }

    /// <summary>
    /// Checks if the specified duration has passed since this timestamp.
    /// </summary>
    /// <param name="duration">The duration to check.</param>
    /// <returns>True if the duration has elapsed since this timestamp.</returns>
    public bool HasPassed(Duration duration) {
        return (GameTime.Instance.TotalTicks - _ticks) >= duration.TotalTicks;
    }

    /// <summary>
    /// Checks if this timestamp is in the past (before now).
    /// </summary>
    public bool IsInPast => _ticks < GameTime.Instance.TotalTicks;

    /// <summary>
    /// Checks if this timestamp is in the future (after now).
    /// </summary>
    public bool IsInFuture => _ticks > GameTime.Instance.TotalTicks;

    /// <summary>
    /// Checks if this timestamp is the current moment.
    /// </summary>
    public bool IsNow => _ticks == GameTime.Instance.TotalTicks;

    #endregion

    #region Instance Methods - Modification

    /// <summary>
    /// Returns a new timestamp offset by the specified duration.
    /// </summary>
    /// <param name="offset">The duration to add.</param>
    public TimestampRPG Add(Duration offset) => new(_ticks + offset.TotalTicks);

    /// <summary>
    /// Returns a new timestamp offset backward by the specified duration.
    /// </summary>
    /// <param name="offset">The duration to subtract.</param>
    public TimestampRPG Subtract(Duration offset) => new(_ticks - offset.TotalTicks);

    #endregion

    #region Display Methods

    /// <summary>
    /// Returns a formatted time string: "[HH:MM]"
    /// </summary>
    public string FormattedTime => $"[{Hour:D2}:{Minute:D2}]";

    /// <summary>
    /// Returns a formatted date string: "Year X : Day (Y)"
    /// </summary>
    public string FormattedDate => $"Year {Year} : Day ({Day})";

    /// <summary>
    /// Returns a full formatted date-time string.
    /// </summary>
    public string FormattedDateTime => $"{FormattedDate} {FormattedTime}";

    /// <summary>
    /// Returns a compact date-time string: "Y1 M3 D15 12:30"
    /// </summary>
    public string ToDisplayCompact() {
        if (IsInvalid) return "Invalid";
        return $"Y{Year} M{Month} D{Day} {Hour:D2}:{Minute:D2}";
    }

    /// <summary>
    /// Returns a smart, human-readable representation.
    /// If within the last day, shows relative time ("2 hours ago").
    /// Otherwise shows the date.
    /// </summary>
    public string ToDisplay() {
        if (IsInvalid) return "Invalid";
        if (IsEpoch) return "The Beginning";

        Duration elapsed = TimeSinceNow();
        
        // If in the future
        if (_ticks > GameTime.Instance.TotalTicks) {
            Duration until = TimeUntilNow();
            if (until.InHours < 24) {
                return $"in {until.ToDisplay()}";
            }
            return FormattedDateTime;
        }

        // If in the past
        if (elapsed.InHours < 1) {
            if (elapsed.InTurns < 1) return "just now";
            return $"{elapsed.InTurns} {Pluralize("turn", elapsed.InTurns)} ago";
        }
        if (elapsed.InHours < 24) {
            return $"{elapsed.InHours} {Pluralize("hour", elapsed.InHours)} ago";
        }
        if (elapsed.InDays < 7) {
            return $"{elapsed.InDays} {Pluralize("day", elapsed.InDays)} ago";
        }

        return FormattedDateTime;
    }

    private static string Pluralize(string word, int count) {
        return count == 1 ? word : word + "s";
    }

    #endregion

    #region Operators - Arithmetic with Duration

    /// <summary>
    /// Subtracts two timestamps to get the duration between them.
    /// </summary>
    public static Duration operator -(TimestampRPG a, TimestampRPG b) {
        return new Duration(a._ticks - b._ticks);
    }

    /// <summary>
    /// Adds a duration to a timestamp.
    /// </summary>
    public static TimestampRPG operator +(TimestampRPG t, Duration d) {
        return new TimestampRPG(t._ticks + d.TotalTicks);
    }

    /// <summary>
    /// Adds a duration to a timestamp.
    /// </summary>
    public static TimestampRPG operator +(Duration d, TimestampRPG t) {
        return new TimestampRPG(t._ticks + d.TotalTicks);
    }

    /// <summary>
    /// Subtracts a duration from a timestamp.
    /// </summary>
    public static TimestampRPG operator -(TimestampRPG t, Duration d) {
        return new TimestampRPG(t._ticks - d.TotalTicks);
    }

    #endregion

    #region Operators - Comparison

    /// <summary>Equality comparison.</summary>
    public static bool operator ==(TimestampRPG a, TimestampRPG b) => a._ticks == b._ticks;

    /// <summary>Inequality comparison.</summary>
    public static bool operator !=(TimestampRPG a, TimestampRPG b) => a._ticks != b._ticks;

    /// <summary>Less than comparison (earlier in time).</summary>
    public static bool operator <(TimestampRPG a, TimestampRPG b) => a._ticks < b._ticks;

    /// <summary>Greater than comparison (later in time).</summary>
    public static bool operator >(TimestampRPG a, TimestampRPG b) => a._ticks > b._ticks;

    /// <summary>Less than or equal comparison.</summary>
    public static bool operator <=(TimestampRPG a, TimestampRPG b) => a._ticks <= b._ticks;

    /// <summary>Greater than or equal comparison.</summary>
    public static bool operator >=(TimestampRPG a, TimestampRPG b) => a._ticks >= b._ticks;

    #endregion

    #region Operators - Implicit Conversions

    /// <summary>Implicitly converts TimestampRPG to long (ticks).</summary>
    public static implicit operator long(TimestampRPG t) => t._ticks;

    #endregion

    #region IEquatable / IComparable

    /// <inheritdoc />
    public bool Equals(TimestampRPG other) => _ticks == other._ticks;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TimestampRPG other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _ticks.GetHashCode();

    /// <inheritdoc />
    public int CompareTo(TimestampRPG other) => _ticks.CompareTo(other._ticks);

    #endregion

    #region IFormattable

    /// <summary>
    /// Returns a string representation of this timestamp.
    /// </summary>
    public override string ToString() => ToDisplay();

    /// <summary>
    /// Formats the timestamp.
    /// Formats:
    /// - null/G = Smart display ("2 hours ago", "Year 1 : Day (15) [12:30]")
    /// - "t" = Time only "[12:30]"
    /// - "d" = Date only "Year 1 : Day (15)"
    /// - "dt" = Full datetime "Year 1 : Day (15) [12:30]"
    /// - "c" = Compact "Y1 M3 D15 12:30"
    /// - "ticks" = Raw ticks
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") {
            return ToDisplay();
        }

        return format.ToLowerInvariant() switch {
            "t" or "time" => FormattedTime,
            "d" or "date" => FormattedDate,
            "dt" or "datetime" => FormattedDateTime,
            "c" or "compact" => ToDisplayCompact(),
            "ticks" => _ticks.ToString(),
            _ => ToDisplay()
        };
    }

    #endregion
}

#region Extension Methods

/// <summary>
/// Extension methods for TimestampRPG.
/// </summary>
public static class TimestampRPGExtensions {
    /// <summary>
    /// Converts a tick count to a TimestampRPG.
    /// </summary>
    public static TimestampRPG AsTimestamp(this long ticks) => new(ticks);

    /// <summary>
    /// Checks if a duration has passed since the given timestamp.
    /// </summary>
    /// <param name="since">The starting timestamp.</param>
    /// <param name="duration">The duration to check.</param>
    /// <returns>True if the duration has elapsed.</returns>
    public static bool HasElapsed(this TimestampRPG since, Duration duration) {
        return since.HasPassed(duration);
    }

    /// <summary>
    /// Gets the remaining time until a duration expires from a starting timestamp.
    /// Returns Zero if already expired.
    /// </summary>
    /// <param name="startTime">When the duration started.</param>
    /// <param name="totalDuration">The total duration.</param>
    public static Duration RemainingTime(this TimestampRPG startTime, Duration totalDuration) {
        TimestampRPG expiresAt = startTime + totalDuration;
        Duration remaining = expiresAt - TimestampRPG.Now;
        return remaining.IsPositive ? remaining : Duration.Zero;
    }

    /// <summary>
    /// Gets the progress (0.0 - 1.0) through a duration from a starting timestamp.
    /// Returns 1.0 if the duration has fully elapsed.
    /// </summary>
    /// <param name="startTime">When the duration started.</param>
    /// <param name="totalDuration">The total duration.</param>
    public static float Progress(this TimestampRPG startTime, Duration totalDuration) {
        if (totalDuration.IsZero) return 1f;
        Duration elapsed = TimestampRPG.Now - startTime;
        float progress = (float)(elapsed.TotalTicks / (double)totalDuration.TotalTicks);
        return Math.Clamp(progress, 0f, 1f);
    }
}

#endregion

#region Usage Examples
/*
 * ═══════════════════════════════════════════════════════════════════════════
 * TIMESTAMPRPG USAGE EXAMPLES
 * ═══════════════════════════════════════════════════════════════════════════
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // CAPTURING MOMENTS IN TIME
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG gameStart = TimestampRPG.Now;
 * TimestampRPG lastSave = TimestampRPG.Now;
 * TimestampRPG buffApplied = TimestampRPG.Now;
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // COOLDOWN TRACKING
 * // ─────────────────────────────────────────────────────────────────────────
 * class Ability {
 *     public Duration Cooldown { get; } = 30.Seconds();
 *     private TimestampRPG _lastUsed = TimestampRPG.Epoch;
 *     
 *     public bool IsReady => _lastUsed.HasPassed(Cooldown);
 *     public Duration RemainingCooldown => _lastUsed.RemainingTime(Cooldown);
 *     
 *     public void Use() {
 *         if (!IsReady) return;
 *         _lastUsed = TimestampRPG.Now;
 *         // Execute ability...
 *     }
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // BUFF/DEBUFF EXPIRATION
 * // ─────────────────────────────────────────────────────────────────────────
 * class StatusEffect {
 *     public TimestampRPG AppliedAt { get; }
 *     public Duration TotalDuration { get; }
 *     
 *     public TimestampRPG ExpiresAt => AppliedAt + TotalDuration;
 *     public bool IsExpired => ExpiresAt < TimestampRPG.Now;
 *     public Duration TimeRemaining => AppliedAt.RemainingTime(TotalDuration);
 *     public float Progress => AppliedAt.Progress(TotalDuration);
 *     
 *     public StatusEffect(Duration duration) {
 *         AppliedAt = TimestampRPG.Now;
 *         TotalDuration = duration;
 *     }
 * }
 * 
 * var poison = new StatusEffect(5.Turns());
 * Debug.Log($"Poison expires in: {poison.TimeRemaining}"); // "5 turns"
 * Debug.Log($"Progress: {poison.Progress:P0}"); // "0%"
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // TIMED CHALLENGES
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG challengeStart = TimestampRPG.Now;
 * Duration timeLimit = 60.Seconds();
 * 
 * void Update() {
 *     Duration elapsed = TimestampRPG.Now - challengeStart;
 *     Duration remaining = challengeStart.RemainingTime(timeLimit);
 *     
 *     if (remaining.IsZero) {
 *         EndChallenge(success: false);
 *     }
 *     
 *     UpdateTimerUI(remaining.ToDisplayCompact()); // "45t", "30t", "15t"...
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // ACTIVITY SCHEDULING
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG craftingStarted = TimestampRPG.Now;
 * Duration craftingTime = 2.Hours();
 * 
 * TimestampRPG completionTime = craftingStarted + craftingTime;
 * Debug.Log($"Item will be ready at: {completionTime:t}"); // "[14:30]"
 * Debug.Log($"Time remaining: {craftingStarted.RemainingTime(craftingTime)}"); // "1 hour 45 minutes"
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // DURATION BETWEEN EVENTS
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG lastMeal = TimestampRPG.Now;
 * // ... time passes ...
 * Duration timeSinceMeal = TimestampRPG.Now - lastMeal;
 * 
 * if (timeSinceMeal >= 8.Hours()) {
 *     ApplyHungerDebuff();
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // TIME COMPARISONS
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG eventA = TimestampRPG.Now;
 * GameTime.Instance.FastForward(1.Hours());
 * TimestampRPG eventB = TimestampRPG.Now;
 * 
 * if (eventA < eventB) {
 *     Debug.Log("Event A happened before Event B");
 * }
 * 
 * Duration timeBetween = eventB - eventA; // 1 hour
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // DISPLAY FORMATTING
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG ts = TimestampRPG.Now;
 * Debug.Log(ts.FormattedTime);     // "[12:30]"
 * Debug.Log(ts.FormattedDate);     // "Year 1 : Day (15)"
 * Debug.Log(ts.FormattedDateTime); // "Year 1 : Day (15) [12:30]"
 * Debug.Log($"{ts:c}");            // "Y1 M3 D15 12:30"
 * Debug.Log(ts.ToDisplay());       // "2 hours ago" or full datetime
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // SCHEDULED EVENTS
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG shopRestock = TimestampRPG.Now + 1.Days();
 * TimestampRPG bossSpawn = TimestampRPG.FromDateTime(year: 1, month: 1, day: 10, hour: 0);
 * 
 * if (TimestampRPG.Now >= bossSpawn) {
 *     SpawnWorldBoss();
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // PROGRESS TRACKING
 * // ─────────────────────────────────────────────────────────────────────────
 * TimestampRPG gatheringStart = TimestampRPG.Now;
 * Duration gatheringDuration = 30.Minutes();
 * 
 * void UpdateProgressBar() {
 *     float progress = gatheringStart.Progress(gatheringDuration);
 *     progressBar.fillAmount = progress; // 0.0 to 1.0
 *     
 *     Duration remaining = gatheringStart.RemainingTime(gatheringDuration);
 *     timeLabel.text = remaining.ToDisplayCompact(); // "15m", "10m", "5m"...
 * }
 */
#endregion