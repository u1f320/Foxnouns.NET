<script lang="ts">
	import type { CustomPreference, FieldEntry } from "$api/models";
	import IconButton from "$components/IconButton.svelte";
	import { t } from "$lib/i18n";
	import FieldEntryEditor from "./FieldEntryEditor.svelte";

	type Props = {
		name: string;
		entries: FieldEntry[];
		allPreferences: Record<string, CustomPreference>;
	};
	let { name, entries = $bindable(), allPreferences }: Props = $props();

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

<h4>{name}</h4>

{#each entries as _, index}
	<FieldEntryEditor
		{index}
		bind:value={entries[index]}
		{allPreferences}
		{moveValue}
		{removeValue}
	/>
{/each}

<form class="input-group m-1" onsubmit={addEntry}>
	<input type="text" class="form-control" bind:value={newEntry} />
	<IconButton type="submit" color="success" icon="plus" tooltip={$t("editor.add-entry")} />
</form>
