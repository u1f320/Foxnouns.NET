using Prometheus;

namespace Foxnouns.Backend;

public static class FoxnounsMetrics
{
    public static readonly Gauge UsersCount = Metrics.CreateGauge(
        "foxnouns_user_count",
        "Number of total users"
    );

    public static readonly Gauge UsersActiveMonthCount = Metrics.CreateGauge(
        "foxnouns_user_count_active_month",
        "Number of users active in the last month"
    );

    public static readonly Gauge UsersActiveWeekCount = Metrics.CreateGauge(
        "foxnouns_user_count_active_week",
        "Number of users active in the last week"
    );

    public static readonly Gauge UsersActiveDayCount = Metrics.CreateGauge(
        "foxnouns_user_count_active_day",
        "Number of users active in the last day"
    );

    public static readonly Gauge MemberCount = Metrics.CreateGauge(
        "foxnouns_member_count",
        "Number of total members"
    );

    public static readonly Summary MetricsCollectionTime = Metrics.CreateSummary(
        "foxnouns_time_metrics",
        "Time it took to collect metrics"
    );

    public static Gauge ProcessPhysicalMemory =>
        Metrics.CreateGauge("foxnouns_process_physical_memory", "Process physical memory");

    public static Gauge ProcessVirtualMemory =>
        Metrics.CreateGauge("foxnouns_process_virtual_memory", "Process virtual memory");

    public static Gauge ProcessPrivateMemory =>
        Metrics.CreateGauge("foxnouns_process_private_memory", "Process private memory");

    public static Gauge ProcessThreads =>
        Metrics.CreateGauge("foxnouns_process_threads", "Process thread count");

    public static Gauge ProcessHandles =>
        Metrics.CreateGauge("foxnouns_process_handles", "Process handle count");
}
