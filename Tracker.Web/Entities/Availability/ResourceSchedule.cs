namespace Tracker.Web.Entities.Availability;

/// <summary>
/// Defines a resource's standard work schedule.
/// Supports part-time schedules and schedule changes over time.
/// </summary>
public class ResourceSchedule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The resource this schedule belongs to
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;
    public virtual Resource? Resource { get; set; }

    /// <summary>
    /// When this schedule takes effect
    /// </summary>
    public DateOnly EffectiveFrom { get; set; }

    /// <summary>
    /// When this schedule ends (null = still active)
    /// </summary>
    public DateOnly? EffectiveTo { get; set; }

    /// <summary>
    /// Hours scheduled for Monday (null = not working)
    /// </summary>
    public decimal? MondayHours { get; set; } = 8m;

    /// <summary>
    /// Hours scheduled for Tuesday (null = not working)
    /// </summary>
    public decimal? TuesdayHours { get; set; } = 8m;

    /// <summary>
    /// Hours scheduled for Wednesday (null = not working)
    /// </summary>
    public decimal? WednesdayHours { get; set; } = 8m;

    /// <summary>
    /// Hours scheduled for Thursday (null = not working)
    /// </summary>
    public decimal? ThursdayHours { get; set; } = 8m;

    /// <summary>
    /// Hours scheduled for Friday (null = not working)
    /// </summary>
    public decimal? FridayHours { get; set; } = 8m;

    /// <summary>
    /// Hours scheduled for Saturday (null = not working)
    /// </summary>
    public decimal? SaturdayHours { get; set; }

    /// <summary>
    /// Hours scheduled for Sunday (null = not working)
    /// </summary>
    public decimal? SundayHours { get; set; }

    /// <summary>
    /// Optional notes about this schedule
    /// </summary>
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // === COMPUTED PROPERTIES ===

    /// <summary>
    /// Total weekly hours for this schedule
    /// </summary>
    public decimal TotalWeeklyHours =>
        (MondayHours ?? 0) + (TuesdayHours ?? 0) + (WednesdayHours ?? 0) +
        (ThursdayHours ?? 0) + (FridayHours ?? 0) + (SaturdayHours ?? 0) +
        (SundayHours ?? 0);

    /// <summary>
    /// Number of working days per week
    /// </summary>
    public int WorkingDaysPerWeek
    {
        get
        {
            var count = 0;
            if (MondayHours.HasValue && MondayHours > 0) count++;
            if (TuesdayHours.HasValue && TuesdayHours > 0) count++;
            if (WednesdayHours.HasValue && WednesdayHours > 0) count++;
            if (ThursdayHours.HasValue && ThursdayHours > 0) count++;
            if (FridayHours.HasValue && FridayHours > 0) count++;
            if (SaturdayHours.HasValue && SaturdayHours > 0) count++;
            if (SundayHours.HasValue && SundayHours > 0) count++;
            return count;
        }
    }

    /// <summary>
    /// Whether this schedule is currently active
    /// </summary>
    public bool IsActive
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return EffectiveFrom <= today && (!EffectiveTo.HasValue || EffectiveTo >= today);
        }
    }

    /// <summary>
    /// Get hours for a specific day of week
    /// </summary>
    public decimal GetHoursForDay(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => MondayHours ?? 0,
        DayOfWeek.Tuesday => TuesdayHours ?? 0,
        DayOfWeek.Wednesday => WednesdayHours ?? 0,
        DayOfWeek.Thursday => ThursdayHours ?? 0,
        DayOfWeek.Friday => FridayHours ?? 0,
        DayOfWeek.Saturday => SaturdayHours ?? 0,
        DayOfWeek.Sunday => SundayHours ?? 0,
        _ => 0
    };

    /// <summary>
    /// Check if a specific day is a working day
    /// </summary>
    public bool IsWorkingDay(DayOfWeek day) => GetHoursForDay(day) > 0;

    /// <summary>
    /// Schedule summary (e.g., "Mon-Fri 8h" or "Mon-Wed 8h, Thu 4h")
    /// </summary>
    public string ScheduleSummary
    {
        get
        {
            var parts = new List<string>();

            // Check for standard Mon-Fri 8h pattern
            if (MondayHours == 8 && TuesdayHours == 8 && WednesdayHours == 8 &&
                ThursdayHours == 8 && FridayHours == 8 &&
                (SaturdayHours ?? 0) == 0 && (SundayHours ?? 0) == 0)
            {
                return "Mon-Fri 8h (40h/week)";
            }

            if ((MondayHours ?? 0) > 0) parts.Add($"Mon {MondayHours}h");
            if ((TuesdayHours ?? 0) > 0) parts.Add($"Tue {TuesdayHours}h");
            if ((WednesdayHours ?? 0) > 0) parts.Add($"Wed {WednesdayHours}h");
            if ((ThursdayHours ?? 0) > 0) parts.Add($"Thu {ThursdayHours}h");
            if ((FridayHours ?? 0) > 0) parts.Add($"Fri {FridayHours}h");
            if ((SaturdayHours ?? 0) > 0) parts.Add($"Sat {SaturdayHours}h");
            if ((SundayHours ?? 0) > 0) parts.Add($"Sun {SundayHours}h");

            return parts.Any()
                ? $"{string.Join(", ", parts)} ({TotalWeeklyHours}h/week)"
                : "No schedule";
        }
    }

    /// <summary>
    /// Create a default full-time schedule (Mon-Fri 8h)
    /// </summary>
    public static ResourceSchedule CreateDefault(string resourceId) => new()
    {
        ResourceId = resourceId,
        EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
        MondayHours = 8m,
        TuesdayHours = 8m,
        WednesdayHours = 8m,
        ThursdayHours = 8m,
        FridayHours = 8m,
        SaturdayHours = null,
        SundayHours = null
    };

    /// <summary>
    /// Create a part-time schedule (e.g., 3 days/week)
    /// </summary>
    public static ResourceSchedule CreatePartTime(string resourceId, decimal hoursPerDay, params DayOfWeek[] workingDays)
    {
        var schedule = new ResourceSchedule
        {
            ResourceId = resourceId,
            EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
            MondayHours = null,
            TuesdayHours = null,
            WednesdayHours = null,
            ThursdayHours = null,
            FridayHours = null,
            SaturdayHours = null,
            SundayHours = null
        };

        foreach (var day in workingDays)
        {
            switch (day)
            {
                case DayOfWeek.Monday: schedule.MondayHours = hoursPerDay; break;
                case DayOfWeek.Tuesday: schedule.TuesdayHours = hoursPerDay; break;
                case DayOfWeek.Wednesday: schedule.WednesdayHours = hoursPerDay; break;
                case DayOfWeek.Thursday: schedule.ThursdayHours = hoursPerDay; break;
                case DayOfWeek.Friday: schedule.FridayHours = hoursPerDay; break;
                case DayOfWeek.Saturday: schedule.SaturdayHours = hoursPerDay; break;
                case DayOfWeek.Sunday: schedule.SundayHours = hoursPerDay; break;
            }
        }

        return schedule;
    }
}
