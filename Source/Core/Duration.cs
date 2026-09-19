using System;
using UnityEngine;

namespace RPGGame.Core;

/// <summary>
/// An immutable, self-documenting wrapper for game time durations.
/// Represents a span of game time in ticks, with seamless conversion between time units.
/// Used for buffs, cooldowns, travel time, resting, activity durations, and more.
/// Supports infinite/permanent durations for effects that never expire.
/// <example>
/// <code>
/// Duration buffTime = 3.Turns();
/// Duration restTime = 8.Hours();
/// Duration journey = 2.Days() + 4.Hours();
/// Duration permanentBuff = Duration.Infinite;
/// GameTime.FastForward(restTime);
/// </code>
/// </example>
/// </summary>
[Serializable]
public readonly struct Duration : IEquatable<Duration>, IComparable<Duration>, IFormattable {
    #region Fields

    private readonly long _ticks;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new Duration from raw ticks.
    /// </summary>
    /// <param name="ticks">Number of game ticks.</param>
    public Duration(long ticks) {
        _ticks = Math.Max(0, ticks);
    }

    #endregion

    #region Static Presets

    /// <summary>Zero duration.</summary>
    public static Duration Zero => new(0);
	public static Duration OneTick   => new(1);
	public static Duration OneSecond => new(GameTime.TICKS_PER_SECOND);
	public static Duration OneMinute => new(GameTime.TICKS_PER_MINUTE);
	public static Duration OneHour   => new(GameTime.TICKS_PER_HOUR);
	public static Duration OneDay    => new(GameTime.TICKS_PER_DAY);

	public static Duration OneCombatTurn => OneMinute;

    /// <summary>One month (30 days).</summary>
    public static Duration OneMonth => new(GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE * 
                                           GameTime.MINUTES_PER_HOUR * GameTime.HOURS_PER_DAY * GameTime.DAYS_PER_MONTH);

    /// <summary>One year (12 months).</summary>
    public static Duration OneYear => new(GameTime.TICKS_PER_SECOND * GameTime.SECONDS_PER_MINUTE * 
                                          GameTime.MINUTES_PER_HOUR * GameTime.HOURS_PER_DAY * 
                                          GameTime.DAYS_PER_MONTH * GameTime.MONTHS_PER_YEAR);

    /// <summary>
    /// An infinite/permanent duration that never expires.
    /// Use for permanent buffs, passive effects, infinite-use items, etc.
    /// </summary>
    /// <example>
    /// <code>
    /// // Permanent buff that never expires
    /// character.AddBuff(new Buff("Blessing", Duration.Infinite));
    /// 
    /// // Passive ability with no cooldown
    /// ability.Cooldown = Duration.Infinite;
    /// 
    /// // Item with unlimited uses
    /// item.UseDuration = Duration.Infinite;
    /// </code>
    /// </example>
    public static Duration Infinite => new(long.MaxValue);

    #endregion

    #region Static Factories (Backward Compatible)

    /// <summary>
    /// Creates a Duration from ticks.
    /// </summary>
    public static Duration FromTicks(long ticks) => new(ticks);

    /// <summary>
    /// Creates a Duration from seconds.
    /// </summary>
    public static Duration FromSeconds(double seconds) {
        long ticks = (long)(seconds * GameTime.TICKS_PER_SECOND);
        return new Duration(ticks);
    }

    /// <summary>
    /// Creates a Duration from minutes.
    /// </summary>
    public static Duration FromMinutes(double minutes) {
        return FromSeconds(minutes * GameTime.SECONDS_PER_MINUTE);
    }

    /// <summary>
    /// Creates a Duration from hours.
    /// </summary>
    public static Duration FromHours(double hours) {
        return FromMinutes(hours * GameTime.MINUTES_PER_HOUR);
    }

    /// <summary>
    /// Creates a Duration from days.
    /// </summary>
    public static Duration FromDays(double days) {
        return FromHours(days * GameTime.HOURS_PER_DAY);
    }

    /// <summary>
    /// Creates a Duration from months.
    /// </summary>
    public static Duration FromMonths(double months) {
        return FromDays(months * GameTime.DAYS_PER_MONTH);
    }

    /// <summary>
    /// Creates a Duration from years.
    /// </summary>
    public static Duration FromYears(double years) {
        return FromMonths(years * GameTime.MONTHS_PER_YEAR);
    }

    /// <summary>
    /// Creates a Duration from turns (1 turn = 1 second = TICKS_PER_SECOND ticks).
    /// </summary>
    public static Duration FromTurns(int turns) {
        return new Duration(turns * GameTime.TICKS_PER_SECOND);
    }

    /// <summary>
    /// Returns the minimum of two durations.
    /// Note: Infinite is always greater than any finite duration.
    /// </summary>
    public static Duration Min(Duration a, Duration b) => new(Math.Min(a._ticks, b._ticks));

    /// <summary>
    /// Returns the maximum of two durations.
    /// Note: Infinite is always greater than any finite duration.
    /// </summary>
    public static Duration Max(Duration a, Duration b) => new(Math.Max(a._ticks, b._ticks));

    /// <summary>
    /// Linearly interpolates between two durations.
    /// If either duration is Infinite, returns Infinite.
    /// </summary>
    public static Duration Lerp(Duration a, Duration b, float t) {
        if (a.IsInfinite || b.IsInfinite) return Infinite;
        t = Mathf.Clamp01(t);
        return new Duration((long)Mathf.Lerp(a._ticks, b._ticks, t));
    }

    #endregion

    #region Properties - Total (Backward Compatible)

    /// <summary>
    /// Gets the total duration in ticks (raw value).
    /// Returns long.MaxValue for infinite durations.
    /// </summary>
    public long TotalTicks => _ticks;

    /// <summary>
    /// Gets the total duration in seconds (as double for precision).
    /// Returns double.PositiveInfinity for infinite durations.
    /// </summary>
    public double TotalSeconds => IsInfinite ? double.PositiveInfinity : (double)_ticks / GameTime.TICKS_PER_SECOND;

    /// <summary>
    /// Gets the total duration in minutes.
    /// Returns double.PositiveInfinity for infinite durations.
    /// </summary>
    public double TotalMinutes => IsInfinite ? double.PositiveInfinity : TotalSeconds / GameTime.SECONDS_PER_MINUTE;

    /// <summary>
    /// Gets the total duration in hours.
    /// Returns double.PositiveInfinity for infinite durations.
    /// </summary>
    public double TotalHours => IsInfinite ? double.PositiveInfinity : TotalMinutes / GameTime.MINUTES_PER_HOUR;

    /// <summary>
    /// Gets the total duration in days.
    /// Returns double.PositiveInfinity for infinite durations.
    /// </summary>
    public double TotalDays => IsInfinite ? double.PositiveInfinity : TotalHours / GameTime.HOURS_PER_DAY;

    /// <summary>
    /// Gets the total duration in months.
    /// Returns double.PositiveInfinity for infinite durations.
    /// </summary>
    public double TotalMonths => IsInfinite ? double.PositiveInfinity : TotalDays / GameTime.DAYS_PER_MONTH;

    /// <summary>
    /// Gets the total duration in years.
    /// Returns double.PositiveInfinity for infinite durations.
    /// </summary>
    public double TotalYears => IsInfinite ? double.PositiveInfinity : TotalMonths / GameTime.MONTHS_PER_YEAR;

    #endregion

    #region Properties - Integer Conversions

    /// <summary>
    /// Gets the duration as whole turns (rounded down).
    /// Returns int.MaxValue for infinite durations.
    /// </summary>
    public int InTurns => IsInfinite ? int.MaxValue : (int)(_ticks / GameTime.TICKS_PER_SECOND);

    /// <summary>
    /// Gets the duration as whole seconds (rounded down).
    /// Returns int.MaxValue for infinite durations.
    /// </summary>
    public int InSeconds => InTurns; // 1 second = 1 turn

    /// <summary>
    /// Gets the duration as whole minutes (rounded down).
    /// Returns int.MaxValue for infinite durations.
    /// </summary>
    public int InMinutes => IsInfinite ? int.MaxValue : (int)TotalMinutes;

    /// <summary>
    /// Gets the duration as whole hours (rounded down).
    /// Returns int.MaxValue for infinite durations.
    /// </summary>
    public int InHours => IsInfinite ? int.MaxValue : (int)TotalHours;

    /// <summary>
    /// Gets the duration as whole days (rounded down).
    /// Returns int.MaxValue for infinite durations.
    /// </summary>
    public int InDays => IsInfinite ? int.MaxValue : (int)TotalDays;

    /// <summary>
    /// Gets the duration as whole months (rounded down).
    /// Returns int.MaxValue for infinite durations.
    /// </summary>
    public int InMonths => IsInfinite ? int.MaxValue : (int)TotalMonths;

    /// <summary>
    /// Gets the duration as whole years (rounded down).
    /// Returns int.MaxValue for infinite durations.
    /// </summary>
    public int InYears => IsInfinite ? int.MaxValue : (int)TotalYears;

    #endregion

    #region Properties - Component Extraction

    /// <summary>
    /// Gets the ticks component (0-9) after extracting larger units.
    /// Returns 0 for infinite durations.
    /// </summary>
    public int TicksComponent => IsInfinite ? 0 : (int)(_ticks % GameTime.TICKS_PER_SECOND);

    /// <summary>
    /// Gets the seconds/turns component (0-59) after extracting larger units.
    /// Returns 0 for infinite durations.
    /// </summary>
    public int SecondsComponent => IsInfinite ? 0 : InSeconds % GameTime.MINUTES_PER_HOUR;

    /// <summary>
    /// Gets the minutes component (0-59) after extracting larger units.
    /// Returns 0 for infinite durations.
    /// </summary>
    public int MinutesComponent => IsInfinite ? 0 : InMinutes % GameTime.MINUTES_PER_HOUR;

    /// <summary>
    /// Gets the hours component (0-23) after extracting larger units.
    /// Returns 0 for infinite durations.
    /// </summary>
    public int HoursComponent => IsInfinite ? 0 : InHours % GameTime.HOURS_PER_DAY;

    /// <summary>
    /// Gets the days component (0-29) after extracting larger units.
    /// Returns 0 for infinite durations.
    /// </summary>
    public int DaysComponent => IsInfinite ? 0 : InDays % GameTime.DAYS_PER_MONTH;

    /// <summary>
    /// Gets the months component (0-11) after extracting larger units.
    /// Returns 0 for infinite durations.
    /// </summary>
    public int MonthsComponent => IsInfinite ? 0 : InMonths % GameTime.MONTHS_PER_YEAR;

    /// <summary>
    /// Gets the years component (remaining years).
    /// Returns int.MaxValue for infinite durations.
    /// </summary>
    public int YearsComponent => IsInfinite ? int.MaxValue : InYears;

    #endregion

    #region Properties - State

    /// <summary>
    /// Returns true if this is a zero duration.
    /// </summary>
    public bool IsZero => _ticks == 0;

    /// <summary>
    /// Returns true if this is a positive duration (greater than zero).
    /// Note: Infinite durations are considered positive.
    /// </summary>
    public bool IsPositive => _ticks > 0;

    /// <summary>
    /// Returns true if this is an infinite/permanent duration.
    /// Use for checking if effects should never expire.
    /// </summary>
    /// <example>
    /// <code>
    /// if (buff.Duration.IsInfinite) {
    ///     // Don't tick down or expire this buff
    /// }
    /// </code>
    /// </example>
    public bool IsInfinite => _ticks == long.MaxValue;

    /// <summary>
    /// Returns true if this is a finite (non-infinite) duration.
    /// Includes zero and all positive finite durations.
    /// </summary>
    public bool IsFinite => _ticks != long.MaxValue;

    #endregion

    #region Instance Methods

    /// <summary>
    /// Returns a new Duration clamped to a minimum value.
    /// Infinite durations remain infinite.
    /// </summary>
    public Duration ClampedMin(Duration min) {
        if (IsInfinite) return this;
        return new(Math.Max(_ticks, min._ticks));
    }

    /// <summary>
    /// Returns a new Duration clamped to a maximum value.
    /// If max is Infinite, returns this unchanged.
    /// </summary>
    public Duration ClampedMax(Duration max) {
        if (max.IsInfinite) return this;
        if (IsInfinite) return max;
        return new(Math.Min(_ticks, max._ticks));
    }

    /// <summary>
    /// Returns a new Duration clamped between min and max.
    /// </summary>
    public Duration Clamped(Duration min, Duration max) {
        if (IsInfinite) return max.IsInfinite ? this : max;
        return new(Math.Clamp(_ticks, min._ticks, max.IsInfinite ? _ticks : max._ticks));
    }

    /// <summary>
    /// Returns this duration multiplied by a percentage.
    /// Infinite durations remain infinite.
    /// </summary>
    /// <param name="percent">The percentage to scale by (50% = half duration, 200% = double).</param>
    public Duration Scaled(Percent percent) {
        if (IsInfinite) return this;
        return new Duration((long)(_ticks * percent.Fraction));
    }

    /// <summary>
    /// Converts to a System.TimeSpan for interop.
    /// Infinite durations return TimeSpan.MaxValue.
    /// </summary>
    public TimeSpan ToTimeSpan() {
        if (IsInfinite) return TimeSpan.MaxValue;
        return TimeSpan.FromSeconds(TotalSeconds);
    }

    /// <summary>
    /// Returns a finite version of this duration, capped at the specified maximum.
    /// Useful for converting infinite durations to large finite values for display or calculation.
    /// </summary>
    /// <param name="maxDuration">The maximum duration to clamp to. Defaults to 100 years.</param>
    public Duration ToFinite(Duration? maxDuration = null) {
        if (IsFinite) return this;
        return maxDuration ?? 100.Years();
    }

    #endregion

    #region Display Methods

    /// <summary>
    /// Returns a smart, human-readable string representation.
    /// Automatically selects the most appropriate unit(s).
    /// Examples: "3 turns", "30 seconds", "2 hours", "1 day 5 hours", "7 days", "permanent"
    /// </summary>
    public string ToDisplay() {
        if (IsInfinite) return "permanent";
        if (_ticks == 0) return "instant";

        // Build components from largest to smallest
        int years = YearsComponent;
        int months = MonthsComponent;
        int days = DaysComponent;
        int hours = HoursComponent;
        int minutes = MinutesComponent;
        int seconds = SecondsComponent;
        int ticks = TicksComponent;

        var parts = new List<string>();

        // Add significant components (skip zeros, but show up to 2 levels)
        if (years > 0) {
            parts.Add($"{years} {Pluralize("year", years)}");
        }
        if (months > 0 && parts.Count < 2) {
            parts.Add($"{months} {Pluralize("month", months)}");
        }
        if (days > 0 && parts.Count < 2) {
            parts.Add($"{days} {Pluralize("day", days)}");
        }
        if (hours > 0 && parts.Count < 2) {
            parts.Add($"{hours} {Pluralize("hour", hours)}");
        }
        if (minutes > 0 && parts.Count < 2) {
            parts.Add($"{minutes} {Pluralize("minute", minutes)}");
        }
        if (seconds > 0 && parts.Count < 2) {
            // Use "turn" for combat contexts, "second" otherwise
            parts.Add($"{seconds} {Pluralize("turn", seconds)}");
        }
        if (ticks > 0 && parts.Count == 0) {
            parts.Add($"{ticks} {Pluralize("tick", ticks)}");
        }

        return parts.Count > 0 ? string.Join(" ", parts) : "instant";
    }

    /// <summary>
    /// Returns a compact display format.
    /// Examples: "3t", "30s", "2h", "1d 5h", "7d", "∞"
    /// </summary>
    public string ToDisplayCompact() {
        if (IsInfinite) return "∞";
        if (_ticks == 0) return "0";

        int years = YearsComponent;
        int months = MonthsComponent;
        int days = DaysComponent;
        int hours = HoursComponent;
        int minutes = MinutesComponent;
        int seconds = SecondsComponent;

        var parts = new List<string>();

        if (years > 0) parts.Add($"{years}y");
        if (months > 0 && parts.Count < 2) parts.Add($"{months}mo");
        if (days > 0 && parts.Count < 2) parts.Add($"{days}d");
        if (hours > 0 && parts.Count < 2) parts.Add($"{hours}h");
        if (minutes > 0 && parts.Count < 2) parts.Add($"{minutes}m");
        if (seconds > 0 && parts.Count < 2) parts.Add($"{seconds}t");

        return parts.Count > 0 ? string.Join(" ", parts) : $"{_ticks}tk";
    }

    private static string Pluralize(string word, int count) {
        return count == 1 ? word : word + "s";
    }

    #endregion

    #region Operators - Arithmetic (Backward Compatible + Enhanced)

    /// <summary>
    /// Adds two durations. If either is Infinite, returns Infinite.
    /// </summary>
    public static Duration operator +(Duration a, Duration b) {
        if (a.IsInfinite || b.IsInfinite) return Infinite;
        // Check for overflow
        if (a._ticks > long.MaxValue - b._ticks) return Infinite;
        return new(a._ticks + b._ticks);
    }

    /// <summary>
    /// Subtracts two durations.
    /// Infinite - finite = Infinite.
    /// Finite - Infinite = Zero (safe behavior).
    /// Infinite - Infinite = Zero (safe behavior).
    /// </summary>
    public static Duration operator -(Duration a, Duration b) {
        if (a.IsInfinite && b.IsInfinite) return Zero;
        if (a.IsInfinite) return Infinite;
        if (b.IsInfinite) return Zero;
        return new(a._ticks - b._ticks);
    }

    /// <summary>
    /// Multiplies a duration by an integer scalar.
    /// Infinite durations remain Infinite (unless multiplied by 0).
    /// </summary>
    public static Duration operator *(Duration d, int scalar) {
        if (scalar == 0) return Zero;
        if (d.IsInfinite) return Infinite;
        // Check for overflow
        if (scalar > 0 && d._ticks > long.MaxValue / scalar) return Infinite;
        return new(d._ticks * scalar);
    }

    /// <summary>Multiplies an integer scalar by a duration.</summary>
    public static Duration operator *(int scalar, Duration d) => d * scalar;

    /// <summary>
    /// Multiplies a duration by a float scalar.
    /// Infinite durations remain Infinite (unless multiplied by 0).
    /// </summary>
    public static Duration operator *(Duration d, float scalar) {
        if (scalar == 0) return Zero;
        if (d.IsInfinite) return Infinite;
        return new((long)(d._ticks * scalar));
    }

    /// <summary>Multiplies a float scalar by a duration.</summary>
    public static Duration operator *(float scalar, Duration d) => d * scalar;

    /// <summary>
    /// Divides a duration by an integer divisor.
    /// Infinite durations remain Infinite.
    /// </summary>
    public static Duration operator /(Duration d, int divisor) {
        if (divisor == 0) return Zero;
        if (d.IsInfinite) return Infinite;
        return new(d._ticks / divisor);
    }

    /// <summary>
    /// Divides a duration by a float divisor.
    /// Infinite durations remain Infinite.
    /// </summary>
    public static Duration operator /(Duration d, float divisor) {
        if (divisor == 0) return Zero;
        if (d.IsInfinite) return Infinite;
        return new((long)(d._ticks / divisor));
    }

    /// <summary>
    /// Divides two durations to get a ratio.
    /// Infinite / Infinite = 1.
    /// Infinite / finite = PositiveInfinity.
    /// Finite / Infinite = 0.
    /// </summary>
    public static double operator /(Duration a, Duration b) {
        if (b._ticks == 0) return 0;
        if (a.IsInfinite && b.IsInfinite) return 1.0;
        if (a.IsInfinite) return double.PositiveInfinity;
        if (b.IsInfinite) return 0.0;
        return (double)a._ticks / b._ticks;
    }

    /// <summary>
    /// Modulo operation on durations.
    /// Infinite % anything = Zero.
    /// </summary>
    public static Duration operator %(Duration a, Duration b) {
        if (b._ticks == 0) return Zero;
        if (a.IsInfinite) return Zero;
        if (b.IsInfinite) return a;
        return new(a._ticks % b._ticks);
    }

    /// <summary>
    /// Multiplies a duration by a percentage.
    /// Infinite durations remain Infinite (unless percentage is 0).
    /// </summary>
    public static Duration operator *(Duration d, Percent p) => d.Scaled(p);

    /// <summary>Multiplies a percentage by a duration.</summary>
    public static Duration operator *(Percent p, Duration d) => d.Scaled(p);

    #endregion

    #region Operators - Comparison

    /// <summary>
    /// Equality comparison. Infinite == Infinite is true.
    /// </summary>
    public static bool operator ==(Duration a, Duration b) => a._ticks == b._ticks;

    /// <summary>
    /// Inequality comparison. Infinite != finite is true.
    /// </summary>
    public static bool operator !=(Duration a, Duration b) => a._ticks != b._ticks;

    /// <summary>
    /// Less than comparison. Finite is always less than Infinite.
    /// </summary>
    public static bool operator <(Duration a, Duration b) => a._ticks < b._ticks;

    /// <summary>
    /// Greater than comparison. Infinite is always greater than finite.
    /// </summary>
    public static bool operator >(Duration a, Duration b) => a._ticks > b._ticks;

    /// <summary>
    /// Less than or equal comparison.
    /// </summary>
    public static bool operator <=(Duration a, Duration b) => a._ticks <= b._ticks;

    /// <summary>
    /// Greater than or equal comparison.
    /// </summary>
    public static bool operator >=(Duration a, Duration b) => a._ticks >= b._ticks;

    #endregion

    #region Operators - Implicit Conversions

    /// <summary>Implicitly converts Duration to long (ticks).</summary>
    public static implicit operator long(Duration d) => d._ticks;

    #endregion

    #region IEquatable / IComparable

    /// <inheritdoc />
    public bool Equals(Duration other) => _ticks == other._ticks;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Duration other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _ticks.GetHashCode();

    /// <inheritdoc />
    public int CompareTo(Duration other) => _ticks.CompareTo(other._ticks);

    #endregion

    #region IFormattable

    /// <summary>
    /// Returns a string representation: "2 hours 30 minutes" or "permanent"
    /// </summary>
    public override string ToString() => ToDisplay();

    /// <summary>
    /// Formats the duration.
    /// Formats:
    /// - null/G = "2 hours 30 minutes" or "permanent" (smart display)
    /// - "c" = "2h 30m" or "∞" (compact)
    /// - "t" = "1500" or "∞" (raw ticks)
    /// - "s" = "150" or "∞" (seconds/turns)
    /// - "full" = "0y 0mo 0d 2h 30m 0t" or "∞" (all components)
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider = null) {
        if (string.IsNullOrEmpty(format) || format == "G") {
            return ToDisplay();
        }

        if (IsInfinite) {
            return format.ToLowerInvariant() switch {
                "c" or "compact" => "∞",
                "t" or "ticks" => "∞",
                "s" or "seconds" or "turns" => "∞",
                "full" => "∞",
                _ => "permanent"
            };
        }

        return format.ToLowerInvariant() switch {
            "c" or "compact" => ToDisplayCompact(),
            "t" or "ticks" => _ticks.ToString(),
            "s" or "seconds" or "turns" => InTurns.ToString(),
            "full" => $"{YearsComponent}y {MonthsComponent}mo {DaysComponent}d {HoursComponent}h {MinutesComponent}m {SecondsComponent}t",
            _ => ToDisplay()
        };
    }

    #endregion
}

#region Extension Methods

/// <summary>
/// Extension methods for creating Duration from numeric types.
/// </summary>
public static class DurationExtensions {
    /// <summary>
    /// Creates a Duration from ticks.
    /// </summary>
    /// <example>50.Ticks()</example>
    public static Duration Ticks(this int n) => new(n);

    /// <summary>
    /// Creates a Duration from ticks.
    /// </summary>
    public static Duration Ticks(this long n) => new(n);

    /// <summary>
    /// Creates a Duration from turns (1 turn = 1 second = TICKS_PER_SECOND ticks).
    /// </summary>
    /// <example>3.Turns()</example>
    public static Duration Turns(this int n) => new(n * GameTime.TICKS_PER_SECOND);

    /// <summary>
    /// Creates a Duration from seconds (same as turns in this time system).
    /// </summary>
    /// <example>30.Seconds()</example>
    public static Duration Seconds(this int n) => new(n * GameTime.TICKS_PER_SECOND);

    /// <summary>
    /// Creates a Duration from seconds (fractional).
    /// </summary>
    public static Duration Seconds(this float n) => Duration.FromSeconds(n);

    /// <summary>
    /// Creates a Duration from minutes.
    /// </summary>
    /// <example>5.Minutes()</example>
    public static Duration Minutes(this int n) => Duration.FromMinutes(n);

    /// <summary>
    /// Creates a Duration from hours.
    /// </summary>
    /// <example>8.Hours()</example>
    public static Duration Hours(this int n) => Duration.FromHours(n);

    /// <summary>
    /// Creates a Duration from days.
    /// </summary>
    /// <example>2.Days()</example>
    public static Duration Days(this int n) => Duration.FromDays(n);

    /// <summary>
    /// Creates a Duration from months.
    /// </summary>
    /// <example>3.Months()</example>
    public static Duration Months(this int n) => Duration.FromMonths(n);

    /// <summary>
    /// Creates a Duration from years.
    /// </summary>
    /// <example>1.Years()</example>
    public static Duration Years(this int n) => Duration.FromYears(n);

    /// <summary>
    /// Converts a TimeSpan to a Duration.
    /// TimeSpan.MaxValue is converted to Duration.Infinite.
    /// </summary>
    public static Duration ToDuration(this TimeSpan timeSpan) {
        if (timeSpan == TimeSpan.MaxValue) return Duration.Infinite;
        return Duration.FromSeconds(timeSpan.TotalSeconds);
    }

    /// <summary>
    /// Returns Duration.Infinite. Fluent way to create infinite durations.
    /// </summary>
    /// <example>
    /// <code>
    /// Duration permanentBuff = 0.Infinite(); // Same as Duration.Infinite
    /// </code>
    /// </example>
    public static Duration Infinite(this int _) => Duration.Infinite;
}

#endregion

#region Usage Examples
/*
 * ═══════════════════════════════════════════════════════════════════════════
 * DURATION USAGE EXAMPLES
 * ═══════════════════════════════════════════════════════════════════════════
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // CREATION STYLES
 * // ─────────────────────────────────────────────────────────────────────────
 * Duration d1 = 3.Turns();              // 3 combat turns (30 ticks)
 * Duration d2 = 30.Seconds();           // 30 seconds (300 ticks)
 * Duration d3 = 2.Hours();              // 2 hours
 * Duration d4 = 1.Days() + 4.Hours();   // 1 day and 4 hours
 * Duration d5 = Duration.FromTicks(100);// Explicit factory
 * Duration d6 = Duration.OneDay;        // Preset
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // INFINITE/PERMANENT DURATIONS
 * // ─────────────────────────────────────────────────────────────────────────
 * Duration permanent = Duration.Infinite;     // Preferred
 * Duration alsoPerm = 0.Infinite();           // Fluent alternative
 * 
 * // Permanent buffs
 * void ApplyPassiveAbility(Character target) {
 *     target.AddBuff(new Buff("Iron Will", Duration.Infinite, +5, StatType.Willpower));
 * }
 * 
 * // Check if effect should expire
 * void TickEffect(StatusEffect effect, Duration elapsed) {
 *     if (effect.Duration.IsInfinite) {
 *         // Never expires - skip countdown
 *         return;
 *     }
 *     effect.RemainingTime -= elapsed;
 *     if (effect.RemainingTime <= Duration.Zero) {
 *         effect.Expire();
 *     }
 * }
 * 
 * // Infinite-use items
 * class MagicWand {
 *     public Duration UseCooldown { get; } = Duration.Infinite; // No cooldown
 *     public bool CanUse => Cooldown.IsInfinite || RemainingCooldown.IsZero;
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // BUFF/DEBUFF DURATIONS
 * // ─────────────────────────────────────────────────────────────────────────
 * void ApplyPoison(Character target, int damage, Duration duration) {
 *     target.AddStatusEffect(new PoisonEffect(damage, duration));
 * }
 * ApplyPoison(enemy, 5, 3.Turns());
 * ApplyPoison(enemy, 2, 10.Seconds());
 * ApplyPoison(boss, 1, Duration.Infinite); // Permanent poison!
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // COOLDOWNS
 * // ─────────────────────────────────────────────────────────────────────────
 * class Ability {
 *     public Duration Cooldown { get; } = 5.Turns();
 *     public Duration RemainingCooldown { get; private set; }
 *     
 *     public void Use() {
 *         RemainingCooldown = Cooldown;
 *     }
 *     
 *     public void OnTurnEnd() {
 *         if (RemainingCooldown.IsInfinite) return; // Locked ability
 *         RemainingCooldown -= 1.Turns();
 *         if (RemainingCooldown < Duration.Zero) {
 *             RemainingCooldown = Duration.Zero;
 *         }
 *     }
 *     
 *     public bool IsReady => RemainingCooldown.IsZero;
 *     public bool IsLocked => Cooldown.IsInfinite;
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // TRAVEL & RESTING
 * // ─────────────────────────────────────────────────────────────────────────
 * Duration travelTime = 2.Days() + 4.Hours();
 * GameTime.Instance.FastForward(travelTime);
 * 
 * Duration restTime = 8.Hours();
 * GameTime.Instance.FastForward(restTime);
 * 
 * Debug.Log($"Journey will take: {travelTime.ToDisplay()}"); // "2 days 4 hours"
 * Debug.Log($"Permanent buff: {Duration.Infinite.ToDisplay()}"); // "permanent"
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // ACTIVITY TIMERS
 * // ─────────────────────────────────────────────────────────────────────────
 * class GatheringActivity {
 *     public Duration TotalDuration { get; } = 30.Minutes();
 *     public Duration Elapsed { get; private set; } = Duration.Zero;
 *     
 *     public float Progress => TotalDuration.IsInfinite ? 0f : (float)(Elapsed / TotalDuration);
 *     public Duration Remaining => TotalDuration - Elapsed;
 *     
 *     public void Tick(Duration delta) {
 *         Elapsed += delta;
 *         if (!TotalDuration.IsInfinite && Elapsed >= TotalDuration) {
 *             Complete();
 *         }
 *     }
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // ARITHMETIC WITH INFINITE
 * // ─────────────────────────────────────────────────────────────────────────
 * Duration inf = Duration.Infinite;
 * Duration finite = 5.Turns();
 * 
 * Duration r1 = inf + finite;    // Infinite
 * Duration r2 = finite + inf;    // Infinite
 * Duration r3 = inf - finite;    // Infinite
 * Duration r4 = finite - inf;    // Zero (safe)
 * Duration r5 = inf * 2;         // Infinite
 * Duration r6 = inf * 0;         // Zero
 * Duration r7 = inf / 2;         // Infinite
 * 
 * bool b1 = inf > finite;        // true
 * bool b2 = finite < inf;        // true
 * bool b3 = inf == inf;          // true
 * bool b4 = inf != finite;       // true
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // COMPARISONS
 * // ─────────────────────────────────────────────────────────────────────────
 * Duration timeout = 30.Seconds();
 * Duration elapsed = 25.Seconds();
 * 
 * if (elapsed < timeout) {
 *     Debug.Log($"Time remaining: {(timeout - elapsed).ToDisplay()}");
 * }
 * 
 * if (elapsed >= 20.Seconds()) {
 *     Debug.Log("Warning: running low on time!");
 * }
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // DISPLAY FORMATTING
 * // ─────────────────────────────────────────────────────────────────────────
 * Duration d = 1.Days() + 5.Hours() + 30.Minutes();
 * Debug.Log(d.ToDisplay());        // "1 day 5 hours"
 * Debug.Log(d.ToDisplayCompact()); // "1d 5h"
 * Debug.Log($"{d:c}");             // "1d 5h" (compact format)
 * Debug.Log($"{d:t}");             // "106200" (raw ticks)
 * Debug.Log($"{d:full}");          // "0y 0mo 1d 5h 30m 0t"
 * 
 * Duration perm = Duration.Infinite;
 * Debug.Log(perm.ToDisplay());        // "permanent"
 * Debug.Log(perm.ToDisplayCompact()); // "∞"
 * Debug.Log($"{perm:c}");             // "∞"
 *
 * // ─────────────────────────────────────────────────────────────────────────
 * // CONVERSION
 * // ─────────────────────────────────────────────────────────────────────────
 * Duration d = 2.Hours() + 30.Minutes();
 * int turns = d.InTurns;       // 150
 * int hours = d.InHours;       // 2
 * double exactHours = d.TotalHours; // 2.5
 * TimeSpan ts = d.ToTimeSpan();
 * 
 * Duration inf = Duration.Infinite;
 * int infTurns = inf.InTurns;        // int.MaxValue
 * double infHours = inf.TotalHours;  // double.PositiveInfinity
 * TimeSpan infTs = inf.ToTimeSpan(); // TimeSpan.MaxValue
 * Duration finite = inf.ToFinite();  // 100 years (default cap)
 */
#endregion