<script module lang="ts">
	export type FormError = { error: RawApiError | null; ok: boolean } | null;
</script>

<script lang="ts">
	import { Icon } from "@sveltestrap/sveltestrap";
	import { t } from "$lib/i18n";
	import type { RawApiError } from "$api/error";
	import ErrorAlert from "$components/ErrorAlert.svelte";

	type Props = { form: FormError | null; successMessage?: string };
	let { form, successMessage }: Props = $props();
</script>

{#if form?.error}
	<ErrorAlert error={form.error} />
{:else if form?.ok}
	<p class="text-success-emphasis">
		<Icon name="check-circle-fill" />
		{successMessage ?? $t("edit-profile.saved-changes")}
	</p>
{/if}
