<script lang="ts">
	import IconButton from "$components/IconButton.svelte";
	import { t } from "$lib/i18n";
	import icons from "$lib/icons";
	import {
		ButtonDropdown,
		DropdownToggle,
		Icon,
		DropdownMenu,
		DropdownItem,
	} from "@sveltestrap/sveltestrap";
	import tippy from "tippy.js";

	const MAX_VISIBLE_ITEMS = 20;

	type Props = { icon: string };
	let { icon = $bindable() }: Props = $props();

	let search = $state("");
	let showAll = $state(false);
	let filteredIcons = $derived(
		icons
			.filter((icon) => icon.toLowerCase().includes(search.toLowerCase()))
			.sort()
			.slice(0, showAll ? undefined : MAX_VISIBLE_ITEMS),
	);

	let totalIcons = $derived(
		icons.filter((icon) => icon.toLowerCase().includes(search.toLowerCase())).length,
	);
</script>

<ButtonDropdown autoClose={false}>
	<span use:tippy={{ content: $t("editor.icons-change-icon") }}>
		<DropdownToggle color="secondary" caret>
			<Icon name={icon} />
			<span class="visually-hidden">{$t("editor.icons-change-icon")}</span>
		</DropdownToggle>
	</span>
	<DropdownMenu>
		<p class="px-2">
			<input
				type="text"
				class="form-control"
				bind:value={search}
				placeholder={$t("editor.flag-search-placeholder")}
			/>
		</p>
		<ul class="list-unstyled icon-list text-center">
			{#each filteredIcons as selectable}
				<IconButton
					icon={selectable}
					tooltip={selectable}
					type="button"
					color="secondary"
					outline
					class="border-0"
					onclick={() => (icon = selectable)}
				/>
			{/each}
		</ul>
		{#if totalIcons > MAX_VISIBLE_ITEMS || showAll}
			<DropdownItem divider />
			<DropdownItem toggle onclick={() => (showAll = !showAll)}>
				{#if showAll}
					{$t("editor.icons-stop-showing-all")}
				{:else}
					{$t("editor.icons-show-all", { count: totalIcons })}
				{/if}
			</DropdownItem>
		{/if}
	</DropdownMenu>
</ButtonDropdown>

<style>
	.icon-list {
		max-height: 200px;
		overflow-y: auto;
	}
</style>
