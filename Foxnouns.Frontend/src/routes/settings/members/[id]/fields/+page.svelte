<script lang="ts">
	import { apiRequest } from "$api";
	import ApiError, { type RawApiError } from "$api/error";
	import { mergePreferences, type User } from "$api/models/user";
	import FieldsEditor from "$components/editor/FieldsEditor.svelte";
	import log from "$lib/log";
	import ephemeralState from "$lib/state.svelte";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();

	let fields = $state(data.member.fields);
	let ok: { ok: boolean; error: RawApiError | null } | null = $state(null);
	let allPreferences = $derived(mergePreferences(data.user.custom_preferences));

	const update = async () => {
		try {
			const resp = await apiRequest<User>("PATCH", `/users/@me/members/${data.member.id}`, {
				body: { fields },
				token: data.token,
			});
			fields = resp.fields;
			ok = { ok: true, error: null };
		} catch (e) {
			ok = { ok: false, error: null };
			log.error("Could not update fields:", e);
			if (e instanceof ApiError) ok.error = e.obj;
		}
	};

	ephemeralState(
		"member-" + data.member.id + "-fields",
		() => fields,
		(data) => (fields = data),
	);
</script>

<FieldsEditor bind:fields {ok} {allPreferences} {update} />
