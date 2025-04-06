<script lang="ts">
	import type { Notification } from "$api/models/moderation";
	import { t } from "$lib/i18n";
	import InfoCircleFill from "svelte-bootstrap-icons/lib/InfoCircleFill.svelte";
	import ExclamationTriangleFill from "svelte-bootstrap-icons/lib/ExclamationTriangleFill.svelte";
	import XOctagonFill from "svelte-bootstrap-icons/lib/XOctagonFill.svelte";
	import QuestionCircleFill from "svelte-bootstrap-icons/lib/QuestionCircleFill.svelte";
	import { idTimestamp } from "$lib";
	import { DateTime } from "luxon";

	type Props = { notification: Notification };
	let { notification }: Props = $props();

	let Icon = $derived.by(() => {
		if (notification.type === "NOTICE") return InfoCircleFill;
		if (notification.type === "WARNING") return ExclamationTriangleFill;
		if (notification.type === "SUSPENSION") return XOctagonFill;
		return QuestionCircleFill;
	});
</script>

<div class="card mb-2">
	<div class="card-body">
		<div class="d-flex">
			<div aria-hidden="true">
				<Icon width={48} height={48} />
			</div>
			<div class="mx-3">
				<p class="card-text text-has-newline">
					{#if notification.localization_key}
						{$t(notification.localization_key, notification.localization_params)}
					{:else}
						{notification.message}
					{/if}
				</p>
			</div>
		</div>
	</div>
	<div class="card-footer text-body-secondary">
		{idTimestamp(notification.id).toLocaleString(DateTime.DATETIME_MED)}
		• <a href="/settings/notifications/ack/{notification.id}">{$t("notification.mark-as-read")}</a>
	</div>
</div>
