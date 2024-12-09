// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
