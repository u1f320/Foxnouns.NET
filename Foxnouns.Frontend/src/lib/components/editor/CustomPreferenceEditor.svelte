<script lang="ts">
	import { type CustomPreference } from "$api/models";
	import IconButton from "$components/IconButton.svelte";
	import { t } from "$lib/i18n";
	import PreferenceIconSelector from "./PreferenceIconSelector.svelte";
	import PreferenceSizeEditor from "./PreferenceSizeEditor.svelte";

	type Props = { pref: CustomPreference & { id: string | undefined }; remove: () => void };
	let { pref = $bindable(), remove }: Props = $props();
</script>

<div class="input-group my-1">
	<PreferenceIconSelector bind:icon={pref.icon} />
	<input
		type="text"
		bind:value={pref.tooltip}
		class="form-control"
		placeholder={$t("editor.tooltip-hint")}
	/>
	<PreferenceSizeEditor bind:size={pref.size} />
	<IconButton
		color="secondary"
		icon={pref.favourite ? "star-fill" : "star"}
		onclick={() => (pref.favourite = !pref.favourite)}
		active={pref.favourite}
		tooltip={$t("editor.custom-preference-favourite")}
	/>
	<IconButton
		color="secondary"
		icon="fonts"
		onclick={() => (pref.muted = !pref.muted)}
		active={pref.muted}
		tooltip={$t("editor.custom-preference-muted")}
	/>
	<IconButton
		color="danger"
		icon="trash3"
		tooltip={$t("editor.remove-entry")}
		onclick={() => remove()}
	/>
</div>
