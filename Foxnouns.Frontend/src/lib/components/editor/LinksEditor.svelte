<script lang="ts">
	import type { RawApiError } from "$api/error";
	import IconButton from "$components/IconButton.svelte";
	import { t } from "$lib/i18n";
	import ephemeralState from "$lib/state.svelte";
	import FormStatusMarker from "./FormStatusMarker.svelte";

	type Props = {
		stateKey: string;
		currentLinks: string[];
		save(links: string[]): Promise<void>;
		form: { ok: boolean; error: RawApiError | null } | null;
	};
	let { stateKey, currentLinks, save, form }: Props = $props();

	let links = $state(currentLinks);
	let newEntry = $state("");

	const moveValue = (index: number, up: boolean) => {
		if (up && index == 0) return;
		if (!up && index == links.length - 1) return;

		const newIndex = up ? index - 1 : index + 1;
		const temp = links[index];
		links[index] = links[newIndex];
		links[newIndex] = temp;
		links = [...links];
	};

	const removeValue = (index: number) => {
		links.splice(index, 1);
		links = [...links];
	};

	const addEntry = (event: Event) => {
		event.preventDefault();
		if (!newEntry) return;

		links = [...links, newEntry];
		newEntry = "";
	};

	ephemeralState(
		stateKey,
		() => links,
		(data) => (links = data),
	);
</script>

<h4>
	{$t("editor.links-header")}
	<button type="button" class="btn btn-primary" onclick={() => save(links)}>
		{$t("save-changes")}
	</button>
</h4>

<FormStatusMarker {form} />

{#each links as _, index}
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
			onclick={() => moveValue(index, false)}
		/>
		<input type="text" class="form-control" bind:value={links[index]} autocomplete="off" />
		<IconButton
			color="danger"
			icon="trash3"
			tooltip={$t("editor.remove-entry")}
			onclick={() => removeValue(index)}
		/>
	</div>
{/each}

<form class="input-group m-1" onsubmit={addEntry}>
	<input
		type="text"
		class="form-control"
		bind:value={newEntry}
		placeholder={$t("editor.new-entry")}
		autocomplete="off"
	/>
	<IconButton type="submit" color="success" icon="plus" tooltip={$t("editor.add-entry")} />
</form>
