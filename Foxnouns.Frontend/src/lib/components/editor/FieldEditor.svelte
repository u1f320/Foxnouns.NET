<script lang="ts">
	import type { CustomPreference, FieldEntry } from "$api/models";
	import IconButton from "$components/IconButton.svelte";
	import { t } from "$lib/i18n";
	import { InputGroup, InputGroupText } from "@sveltestrap/sveltestrap";
	import FieldEntryEditor from "./FieldEntryEditor.svelte";

	type Props = {
		name: string;
		entries: FieldEntry[];
		allPreferences: Record<string, CustomPreference>;
		index?: number;
		move?: (index: number, up: boolean) => void;
		remove?: (index: number) => void;
	};
	let {
		name = $bindable(),
		entries = $bindable(),
		allPreferences,
		index,
		move,
		remove,
	}: Props = $props();

	let newEntry = $state("");

	const moveValue = (index: number, up: boolean) => {
		if (up && index == 0) return;
		if (!up && index == entries.length - 1) return;

		const newIndex = up ? index - 1 : index + 1;
		const temp = entries[index];
		entries[index] = entries[newIndex];
		entries[newIndex] = temp;
		entries = [...entries];
	};

	const removeValue = (index: number) => {
		entries.splice(index, 1);
		entries = [...entries];
	};

	const addEntry = (event: Event) => {
		event.preventDefault();
		if (!newEntry) return;

		entries = [...entries, { value: newEntry, status: "okay" }];
		newEntry = "";
	};
</script>

{#if index !== undefined && move && remove}
	<div class="d-flex">
		<InputGroup>
			<IconButton
				icon="chevron-up"
				color="secondary"
				tooltip={$t("editor.move-field-up")}
				onclick={() => move(index, true)}
			/>
			<IconButton
				icon="chevron-down"
				color="secondary"
				tooltip={$t("editor.move-field-down")}
				onclick={() => move(index, false)}
			/>
			<InputGroupText>{$t("editor.field-name")}</InputGroupText>
			<input class="form-control" bind:value={name} />
			<IconButton
				color="danger"
				icon="trash3"
				tooltip={$t("editor.remove-field")}
				onclick={() => remove(index)}
			/>
		</InputGroup>
	</div>
{:else}
	<h4>{name}</h4>
{/if}

{#each entries as _, i}
	<FieldEntryEditor index={i} bind:value={entries[i]} {allPreferences} {moveValue} {removeValue} />
{/each}

<form class="input-group m-1" onsubmit={addEntry}>
	<input
		type="text"
		class="form-control"
		bind:value={newEntry}
		placeholder={$t("editor.new-entry")}
	/>
	<IconButton type="submit" color="success" icon="plus" tooltip={$t("editor.add-entry")} />
</form>
