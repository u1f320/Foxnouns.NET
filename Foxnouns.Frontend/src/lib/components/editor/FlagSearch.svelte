<script lang="ts">
	import type { PrideFlag } from "$api/models";
	import InfoCircleFill from "svelte-bootstrap-icons/lib/InfoCircleFill.svelte";
	import Search from "svelte-bootstrap-icons/lib/Search.svelte";
	import FlagButton from "./FlagButton.svelte";
	import { t } from "$lib/i18n";

	type Props = { flags: PrideFlag[]; select(flag: PrideFlag): void };
	let { flags, select }: Props = $props();

	let query = $state("");
	let filteredFlags = $derived(search(query));

	function search(q: string) {
		if (!q) return flags.slice(0, 20);
		return flags.filter((f) => f.name.toLowerCase().indexOf(q.toLowerCase()) !== -1).slice(0, 20);
	}
</script>

<input class="form-control" placeholder={$t("editor.flag-search-placeholder")} bind:value={query} />

<div class="mt-3">
	{#each filteredFlags as flag (flag.id)}
		<FlagButton {flag} onclick={() => select(flag)} padding />
	{:else}
		<div class="text-secondary text-center">
			<p>
				<Search class="no-flags-icon" height={64} width={64} aria-hidden />
			</p>
			<p>
				{#if query}
					{$t("editor.flag-search-no-flags")}
				{:else}
					{$t("editor.flag-search-no-account-flags")}
				{/if}
			</p>
		</div>
	{/each}
	{#if flags.length > 0}
		<p class="text-secondary mt-2">
			<InfoCircleFill aria-hidden />
			{$t("editor.flag-search-hint")}
			<a href="/settings/flags">{$t("editor.flag-manage-your-flags")}</a>
		</p>
	{:else}
		<p><a href="/settings/flags">{$t("editor.flag-manage-your-flags")}</a></p>
	{/if}
</div>
