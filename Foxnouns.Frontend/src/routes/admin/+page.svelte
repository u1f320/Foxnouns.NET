<script lang="ts">
	import DashboardCard from "$components/admin/DashboardCard.svelte";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();
</script>

<h1>Dashboard</h1>

<div class="row gx-3 gy-3">
	<DashboardCard title="Users" onlyNumber={false}>
		<span class="fs-1">{data.meta.users.total.toLocaleString("en")}</span>
		<br />
		<small>({data.meta.users.active_month.toLocaleString("en")} active in the last month)</small>
	</DashboardCard>
	<DashboardCard title="Members">{data.meta.members.toLocaleString("en")}</DashboardCard>
	<DashboardCard title="Open reports" onlyNumber={false}>
		<span class="fs-1">{data.reportCount.toLocaleString("en")}</span>
		<br />
		<small>({data.staleReportCount} older than 1 week)</small>
	</DashboardCard>
</div>
