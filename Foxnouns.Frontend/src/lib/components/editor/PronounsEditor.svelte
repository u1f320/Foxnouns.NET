<script lang="ts">
	import type { CustomPreference, Pronoun } from "$api/models";
	import IconButton from "$components/IconButton.svelte";
	import { PUBLIC_LANGUAGE } from "$env/static/public";
	import defaultPronouns from "$lib/defaultPronouns";
	import { t } from "$lib/i18n";
	import PronounEntryEditor from "./PronounEntryEditor.svelte";

	type Props = {
		entries: Pronoun[];
		allPreferences: Record<string, CustomPreference>;
	};
	let { entries = $bindable(), allPreferences }: Props = $props();

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

		// Some pronouns are commonly abbreviated and should be expanded by the editor, or people will get confused
		let entry = { value: newEntry, display_text: null as string | null, status: "okay" };
		if (PUBLIC_LANGUAGE in defaultPronouns && newEntry in defaultPronouns[PUBLIC_LANGUAGE]) {
			const def = defaultPronouns[PUBLIC_LANGUAGE][newEntry];
			if (def.pronouns) entry.value = def.pronouns.join("/");
			if (def.display) entry.display_text = def.display;
		}

		entries = [...entries, entry];
		newEntry = "";
	};
</script>

<h4>{$t("profile.pronouns-header")}</h4>

{#each entries as _, index}
	<PronounEntryEditor
		{index}
		bind:value={entries[index]}
		{allPreferences}
		{moveValue}
		{removeValue}
	/>
{/each}

<form class="input-group m-1" onsubmit={addEntry}>
	<input type="text" class="form-control" bind:value={newEntry} autocomplete="off" />
	<IconButton type="submit" color="success" icon="plus" tooltip={$t("editor.add-entry")} />
</form>
