<script lang="ts">
	import type { RawApiError } from "$api/error";
	import type { CustomPreference, Field } from "$api/models";
	import IconButton from "$components/IconButton.svelte";
	import { t } from "$lib/i18n";
	import FieldEditor from "./FieldEditor.svelte";
	import FormStatusMarker from "./FormStatusMarker.svelte";
	import NoscriptWarning from "./NoscriptWarning.svelte";
	import CustomPreferencesNotice from "$components/editor/CustomPreferencesNotice.svelte";

	type Props = {
		fields: Field[];
		ok: { ok: boolean; error: RawApiError | null } | null;
		allPreferences: Record<string, CustomPreference>;
		update: () => Promise<void>;
	};

	let { fields = $bindable(), ok, allPreferences, update }: Props = $props();

	let newFieldName = $state("");

	const moveField = (index: number, up: boolean) => {
		if (up && index == 0) return;
		if (!up && index == fields.length - 1) return;

		const newIndex = up ? index - 1 : index + 1;
		const temp = fields[index];
		fields[index] = fields[newIndex];
		fields[newIndex] = temp;
		fields = [...fields];
	};

	const removeField = (index: number) => {
		fields.splice(index, 1);
		fields = [...fields];
	};

	const addField = (event: Event) => {
		event.preventDefault();
		if (!newFieldName) return;

		fields = [...fields, { name: newFieldName, entries: [] }];
		newFieldName = "";
	};
</script>

<NoscriptWarning />
<FormStatusMarker form={ok} />
<CustomPreferencesNotice />

<h4>{$t("edit-profile.editing-fields-header")}</h4>

<div>
	<h5>{$t("editor.add-field")}</h5>
	<form class="input-group m-1" onsubmit={addField}>
		<input
			type="text"
			class="form-control"
			bind:value={newFieldName}
			placeholder={$t("editor.field-name")}
			autocomplete="off"
		/>
		<IconButton type="submit" color="success" icon="plus" tooltip={$t("editor.add-field")} />
	</form>
</div>

{#if fields.length > 0}
	<hr />
	{#each fields as field, index}
		<FieldEditor
			{index}
			bind:name={field.name}
			bind:entries={field.entries}
			{allPreferences}
			move={moveField}
			remove={removeField}
		/>
	{/each}
{/if}
<div>
	<button class="btn btn-primary" onclick={() => update()}>{$t("save-changes")}</button>
</div>
