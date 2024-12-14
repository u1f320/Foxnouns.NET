<script lang="ts">
	import { apiRequest } from "$api";
	import ApiError, { type RawApiError } from "$api/error";
	import { PreferenceSize, type CustomPreference } from "$api/models";
	import CustomPreferenceEditor from "$components/editor/CustomPreferenceEditor.svelte";
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import { t } from "$lib/i18n";
	import log from "$lib/log";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();

	type EditableCustomPreference = CustomPreference & {
		id: string | undefined;
	};

	let customPreferences = $state(
		Object.keys(data.user.custom_preferences)
			.sort()
			.map(
				(id) =>
					({
						...data.user.custom_preferences[id],
						id,
					}) as EditableCustomPreference,
			),
	);

	let canAdd = $derived(customPreferences.length < data.meta.limits.custom_preferences);
	// Used for form status
	let ok: { ok: boolean; error: RawApiError | null } | null = $state(null);

	const remove = (idx: number) => {
		customPreferences.splice(idx, 1);
		customPreferences = [...customPreferences];
	};

	const add = () => {
		customPreferences = [
			...customPreferences,
			{
				id: undefined,
				tooltip: "",
				icon: "question-lg",
				size: PreferenceSize.Normal,
				muted: false,
				favourite: false,
			},
		];
	};

	const save = async () => {
		try {
			const resp = await apiRequest<Record<string, CustomPreference>>(
				"PATCH",
				"/users/@me/custom-preferences",
				{
					body: customPreferences,
					token: data.token,
				},
			);

			customPreferences = Object.keys(resp)
				.sort()
				.map(
					(id) =>
						({
							...resp[id],
							id,
						}) as EditableCustomPreference,
				);

			ok = { ok: true, error: null };
		} catch (e) {
			log.error("error saving custom preferences:", e);
			if (e instanceof ApiError) ok = { ok: false, error: e.obj };
			else ok = null;
		}
	};
</script>

<h3>
	{$t("settings.custom-preferences-title")}
	<div class="btn-group">
		<button class="btn btn-primary" onclick={() => save()}>{$t("save-changes")}</button>
		<button class="btn btn-success" onclick={add}>{$t("editor.add-custom-preference")}</button>
	</div>
</h3>

<FormStatusMarker form={ok} />

<div>
	{#each customPreferences as _, idx}
		<CustomPreferenceEditor bind:pref={customPreferences[idx]} remove={() => remove(idx)} />
	{/each}
</div>

<form></form>
