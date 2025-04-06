<script lang="ts">
	import { fastRequest } from "$api";
	import type { UserSettings } from "$api/models";
	import { idTimestamp } from "$lib";
	import { t } from "$lib/i18n";
	import log from "$lib/log";
	import { renderUnsafeMarkdown } from "$lib/markdown";
	import { DateTime } from "luxon";

	type Props = { id: string; message: string; settings?: UserSettings; token: string | null };
	let { id, message, settings, token }: Props = $props();

	let lastReadNotice = $state(settings?.last_read_notice || null);

	// Render the notice if:
	// - user is not logged in (no settings object)
	// - last read notice is null (never marked any notice as read)
	// - last read notice ID is smaller than the current one (has not marked the current notice as read)
	let renderNotice = $derived(!lastReadNotice || lastReadNotice < id);
	let canDismiss = $derived(!!token);
	let renderedMessage = $derived(renderUnsafeMarkdown(message));

	let dismiss = async () => {
		if (!token) return;
		try {
			await fastRequest("PATCH", "/users/@me/settings", { token, body: { last_read_notice: id } });
			lastReadNotice = id;
		} catch (e) {
			log.error("error updating last read notice ID:", e);
		}
	};
</script>

{#if renderNotice}
	<div class="alert alert-light" role="alert">
		<div>
			{@html renderedMessage}
		</div>
		{#if canDismiss}
			<div>
				<!-- svelte-ignore a11y_invalid_attribute -->
				<a href="#" tabindex="0" role="button" onclick={() => dismiss()} onkeyup={() => dismiss()}>
					{$t("notification.mark-as-read")}
				</a>
				• {idTimestamp(id).toLocaleString(DateTime.DATETIME_MED)}
			</div>
		{/if}
	</div>
{/if}
