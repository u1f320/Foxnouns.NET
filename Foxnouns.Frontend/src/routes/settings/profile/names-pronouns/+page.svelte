<script lang="ts">
	import { apiRequest } from "$api";
	import ApiError, { type RawApiError } from "$api/error";
	import { mergePreferences, type User } from "$api/models/user";
	import CustomPreferencesNotice from "$components/editor/CustomPreferencesNotice.svelte";
	import FieldEditor from "$components/editor/FieldEditor.svelte";
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import NoscriptWarning from "$components/editor/NoscriptWarning.svelte";
	import PronounsEditor from "$components/editor/PronounsEditor.svelte";
	import { t } from "$lib/i18n";
	import log from "$lib/log";
	import ephemeralState from "$lib/state.svelte";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();

	let names = $state(data.user.names);
	let pronouns = $state(data.user.pronouns);
	let ok: { ok: boolean; error: RawApiError | null } | null = $state(null);
	let allPreferences = $derived(mergePreferences(data.user.custom_preferences));

	ephemeralState(
		"user-names-pronouns",
		() => ({ names, pronouns }),
		(data) => {
			names = data.names;
			pronouns = data.pronouns;
		},
	);

	const update = async () => {
		try {
			const resp = await apiRequest<User>("PATCH", "/users/@me", {
				body: { names, pronouns },
				token: data.token,
			});
			names = resp.names;
			pronouns = resp.pronouns;
			ok = { ok: true, error: null };
		} catch (e) {
			ok = { ok: false, error: null };
			log.error("Could not update names/pronouns:", e);
			if (e instanceof ApiError) ok.error = e.obj;
		}
	};
</script>

<NoscriptWarning />
<FormStatusMarker form={ok} />
<CustomPreferencesNotice />

<div>
	<FieldEditor name={$t("profile.names-header")} bind:entries={names} {allPreferences} />
</div>
<div>
	<PronounsEditor bind:entries={pronouns} {allPreferences} />
</div>
<div>
	<button class="btn btn-primary" onclick={() => update()}>{$t("save-changes")}</button>
</div>
