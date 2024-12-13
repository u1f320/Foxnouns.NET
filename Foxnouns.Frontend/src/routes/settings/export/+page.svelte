<script lang="ts">
	import { DateTime } from "luxon";
	import type { ActionData, PageData } from "./$types";
	import { t } from "$lib/i18n";
	import { enhance } from "$app/forms";
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

	let expiresAt = $derived.by(() => {
		if (!data.expiresAt) return null;
		return DateTime.fromISO(data.expiresAt);
	});
</script>

<div class="mx-auto w-lg-75">
	<h3>{$t("settings.export-title")}</h3>

	<FormStatusMarker {form} successMessage={$t("settings.export-request-success")} />

	<p>
		{$t("settings.export-info")}
	</p>

	<form method="POST" use:enhance>
		<div class="btn-group">
			<button type="submit" class="btn btn-primary" disabled={!data.canExport}>
				{$t("settings.export-request-button")}
			</button>
			{#if data.url}
				<a href={data.url} target="_blank" class="btn btn-success">
					{$t("settings.export-download")}
					{#if expiresAt}
						{$t("settings.export-expires-at", { expiresAt: expiresAt.toRelative() })}
					{/if}
				</a>
			{/if}
		</div>
	</form>
</div>
