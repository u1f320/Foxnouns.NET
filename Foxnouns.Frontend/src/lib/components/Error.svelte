<script lang="ts">
	import { ErrorCode, type RawApiError } from "$api/error";
	import errorDescription from "$lib/errorCodes";
	import { t } from "$lib/i18n";
	import KeyedValidationErrors from "./errors/KeyedValidationErrors.svelte";

	type Props = { showHeader?: boolean; headerElem?: string; error: RawApiError };
	let { showHeader, headerElem, error }: Props = $props();
</script>

{#if showHeader !== false}
	<svelte:element this={headerElem ?? "h4"}>
		{#if error.code === ErrorCode.BadRequest}
			{$t("error.bad-request-header")}
		{:else if error.status === 404}
			{$t("error.not-found-header")}
		{:else}
			{$t("error.generic-header")}
		{/if}
	</svelte:element>
{/if}
<p>{errorDescription($t, error.code)}</p>
{#if error.error_id}
	<p>
		{$t("error.error-id")}
		<code>
			{error.error_id}
		</code>
	</p>
{/if}
{#if error.errors}
	<details>
		<summary>{$t("error.extra-info-header")}</summary>
		<ul>
			{#each error.errors as val}
				<KeyedValidationErrors key={val.key} errors={val.errors} />
			{/each}
		</ul>
	</details>
{/if}
<details>
	<summary>{$t("error.raw-header")}</summary>
	<pre>{JSON.stringify(error, undefined, "  ")}</pre>
</details>
