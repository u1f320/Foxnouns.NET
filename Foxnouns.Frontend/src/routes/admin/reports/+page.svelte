<script lang="ts">
	import Link45deg from "svelte-bootstrap-icons/lib/Link45deg.svelte";
	import Funnel from "svelte-bootstrap-icons/lib/Funnel.svelte";
	import FunnelFill from "svelte-bootstrap-icons/lib/FunnelFill.svelte";
	import type { PageData } from "./$types";
	import { t } from "$lib/i18n";
	import { DateTime } from "luxon";
	import { idTimestamp } from "$lib";

	type Props = { data: PageData };
	let { data }: Props = $props();

	const addReporter = (id: string | null) => {
		const url = new URL(data.url);
		if (id) url.searchParams.set("by-reporter", id);
		else url.searchParams.delete("by-reporter");

		return url.toString();
	};

	const addClosed = () => {
		const url = new URL(data.url);
		if (!data.includeClosed) url.searchParams.set("include-closed", "true");
		else url.searchParams.delete("include-closed");

		return url.toString();
	};

	const addTarget = (id: string | null) => {
		const url = new URL(data.url);
		if (id) url.searchParams.set("by-target", id);
		else url.searchParams.delete("by-target");

		return url.toString();
	};

	const addBefore = (id: string) => {
		const url = new URL(data.url);
		url.searchParams.delete("after");
		url.searchParams.set("before", id);

		return url.toString();
	};

	const addAfter = (id: string) => {
		const url = new URL(data.url);
		url.searchParams.delete("before");
		url.searchParams.set("after", id);

		return url.toString();
	};
</script>

<svelte:head>
	<title>Reports • pronouns.cc</title>
</svelte:head>

<h2>Reports</h2>

<ul>
	{#if data.byTarget}
		<li>Filtering by target (<a href={addTarget(null)}>clear</a>)</li>
	{/if}
	{#if data.byReporter}
		<li>Filtering by reporter (<a href={addReporter(null)}>clear</a>)</li>
	{/if}
	{#if data.includeClosed}
		<li>Showing all reports (<a href={addClosed()}>only show open reports</a>)</li>
	{:else}
		<li>Showing open reports (<a href={addClosed()}>show all reports</a>)</li>
	{/if}
</ul>

{#if data.before}
	<a href={addAfter(data.before)}>Show newer reports</a>
{/if}

<table class="table">
	<thead>
		<tr>
			<th scope="col"></th>
			<th scope="col">User</th>
			<th scope="col">Member</th>
			<th scope="col">Reporter</th>
			<th scope="col">Reason</th>
			<th scope="col">Context?</th>
			<th scope="col">Created at</th>
		</tr>
	</thead>
	<tbody>
		{#each data.reports as report (report.id)}
			<tr>
				<td>
					<a href="/admin/reports/{report.id}">
						<Link45deg />
						<span class="visually-hidden">Open report</span>
					</a>
				</td>
				<td>
					<a href="/@{report.target_user.username}">@{report.target_user.username}</a>
					(<a href={addTarget(report.target_user.id)}>
						{#if data.byTarget === report.target_user.id}<FunnelFill />{:else}<Funnel />{/if}
					</a>)
				</td>
				<td>
					{#if report.target_member}
						<a href="@/{report.target_user.username}/{report.target_member.name}">
							{report.target_member.name}
						</a>
					{:else}
						<em>(none)</em>
					{/if}
				</td>
				<td>
					<a href="/@{report.reporter.username}">{report.reporter.username}</a>
					(<a href={addReporter(report.reporter.id)}>
						{#if data.byReporter === report.reporter.id}<FunnelFill />{:else}<Funnel />{/if}
					</a>)
				</td>
				<td><code>{report.reason}</code></td>
				<td>
					{#if report.context}
						{$t("yes")}
					{:else}
						{$t("no")}
					{/if}
				</td>
				<td>
					{idTimestamp(report.id).toLocaleString(DateTime.DATETIME_SHORT)}
				</td>
			</tr>
		{/each}
	</tbody>
</table>

{#if data.reports.length === 100}
	<a href={addBefore(data.reports[data.reports.length - 1].id)}>Show older reports</a>
{/if}
