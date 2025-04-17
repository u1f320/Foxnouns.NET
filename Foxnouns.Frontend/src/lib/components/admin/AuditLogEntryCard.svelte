<script lang="ts">
	import type { AuditLogEntry } from "$api/models/moderation";
	import { idTimestamp } from "$lib";
	import { renderMarkdown } from "$lib/markdown";
	import { DateTime } from "luxon";
	import AuditLogEntity from "./AuditLogEntity.svelte";

	type Props = { entry: AuditLogEntry };
	let { entry }: Props = $props();

	let reason = $derived(renderMarkdown(entry.reason));
	let date = $derived(idTimestamp(entry.id).toLocaleString(DateTime.DATETIME_MED));
</script>

<div class="card my-1 p-2">
	<h6 class="d-flex">
		<span class="flex-grow-1">
			<AuditLogEntity entity={entry.moderator} />
			{#if entry.type === "IGNORE_REPORT"}
				ignored a report
			{:else if entry.type === "WARN_USER" || entry.type === "WARN_USER_AND_CLEAR_PROFILE"}
				warned
			{:else if entry.type === "SUSPEND_USER"}
				suspended
			{:else if entry.type === "QUERY_SENSITIVE_USER_DATA"}
				looked up sensitive data of
			{:else}
				(unknown action <code>{entry.type}</code>)
			{/if}
			{#if entry.target_user}
				<AuditLogEntity entity={entry.target_user} />
			{/if}
			{#if entry.target_member}
				for member <AuditLogEntity entity={entry.target_member} />
			{/if}
		</span>

		<small class="text-secondary">{date}</small>
	</h6>

	{#if entry.type === "IGNORE_REPORT"}
		{#if entry.report}
			<details>
				<summary>Report</summary>
				<ul>
					<li><strong>From:</strong> {entry.report.reporter_id}</li>
					<li><strong>Target:</strong> {entry.report.target_user_id}</li>
					<li><strong>Reason:</strong> {entry.report.reason}</li>
					{#if entry.report.context}
						<li><strong>Context:</strong> {entry.report.context}</li>
					{/if}
				</ul>
			</details>
		{:else}
			<p><em>(the ignored report has been deleted)</em></p>
		{/if}
	{/if}

	{#if reason}
		<details>
			<summary>Reason</summary>
			<!-- eslint-disable-next-line svelte/no-at-html-tags -->
			{@html reason}
		</details>
	{:else}
		<p><em>(no reason given)</em></p>
	{/if}
</div>
