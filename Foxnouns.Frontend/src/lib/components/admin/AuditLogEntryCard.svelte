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

<svelte:head>
	<title>Audit log</title>
</svelte:head>

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
	{#if reason}
		<details>
			<summary>Reason</summary>
			{@html reason}
		</details>
	{:else}
		<em>(no reason given)</em>
	{/if}
</div>
