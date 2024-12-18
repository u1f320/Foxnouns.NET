<script lang="ts">
	import { t } from "$lib/i18n";

	type Props = { required?: boolean };
	let { required }: Props = $props();
</script>

{#if required}
	<small class="text-danger"><abbr title={$t("form.required")}>*</abbr></small>
{:else}
	<small class="text-body-secondary">{$t("form.optional")}</small>
{/if}
