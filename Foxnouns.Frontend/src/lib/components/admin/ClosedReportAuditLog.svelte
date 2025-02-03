<script lang="ts">
	import { AuditLogEntryType, type AuditLogEntry } from "$api/models/moderation";
	import { idTimestamp } from "$lib";
	import { renderMarkdown } from "$lib/markdown";
	import { DateTime } from "luxon";
	import AuditLogEntity from "./AuditLogEntity.svelte";

	type Props = { entry: AuditLogEntry };
	let { entry }: Props = $props();
</script>

<div class="row">
	<h3 id="report-status">
		Closed by <AuditLogEntity entity={entry.moderator} /> at
		{idTimestamp(entry.id).toLocaleString(DateTime.DATETIME_MED)}
	</h3>
	<p>
		{#if entry.type === AuditLogEntryType.IgnoreReport}
			Report was ignored
		{:else if entry.type === AuditLogEntryType.WarnUser || entry.type === AuditLogEntryType.WarnUserAndClearProfile}
			User was warned
			{#if entry.cleared_fields && entry.cleared_fields.length > 0}
				<br />Cleared fields: {entry.cleared_fields.join(", ")}
			{/if}
		{:else if entry.type === AuditLogEntryType.SuspendUser}
			User was suspended
		{/if}
	</p>
	<h4>Reason</h4>
	<p>
		{#if entry.reason}
			{@html renderMarkdown(entry.reason)}
		{:else}
			<em class="text-secondary">(no reason given)</em>
		{/if}
	</p>
</div>
