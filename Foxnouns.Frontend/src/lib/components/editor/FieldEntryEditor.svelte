<script lang="ts">
	import { defaultPreferences, type CustomPreference, type FieldEntry } from "$api/models";
	import IconButton from "$components/IconButton.svelte";
	import { t } from "$lib/i18n";
	import tippy from "$lib/tippy";
	import {
		ButtonDropdown,
		DropdownItem,
		DropdownMenu,
		DropdownToggle,
		Icon,
	} from "@sveltestrap/sveltestrap";

	type Props = {
		index: number;
		value: FieldEntry;
		allPreferences: Record<string, CustomPreference>;
		moveValue: (index: number, up: boolean) => void;
		removeValue: (index: number) => void;
	};
	let { index, value = $bindable(), allPreferences, moveValue, removeValue }: Props = $props();

	let status = $derived(
		value.status in allPreferences ? allPreferences[value.status] : defaultPreferences.missing,
	);

	let prefIds = $derived(Object.keys(allPreferences).filter((s) => s !== "missing"));
</script>

<div class="input-group m-1">
	<IconButton
		icon="chevron-up"
		color="secondary"
		tooltip={$t("editor.move-entry-up")}
		onclick={() => moveValue(index, true)}
	/>
	<IconButton
		icon="chevron-down"
		color="secondary"
		tooltip={$t("editor.move-entry-down")}
		onclick={() => moveValue(index, true)}
	/>
	<input type="text" class="form-control" bind:value={value.value} />
	<ButtonDropdown>
		<span use:tippy={{ content: status.tooltip }}>
			<DropdownToggle color="secondary" caret>
				<Icon name={status.icon} />
			</DropdownToggle>
		</span>
		<DropdownMenu>
			{#each prefIds as id}
				<DropdownItem on:click={() => (value.status = id)} active={value.status === id}>
					<Icon name={allPreferences[id].icon} aria-hidden />
					{allPreferences[id].tooltip}
				</DropdownItem>
			{/each}
		</DropdownMenu>
	</ButtonDropdown>
	<IconButton
		color="danger"
		icon="trash3"
		tooltip={$t("editor.remove-entry")}
		onclick={() => removeValue(index)}
	/>
</div>
