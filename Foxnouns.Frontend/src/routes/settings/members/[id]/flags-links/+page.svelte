<script lang="ts">
	import { fastRequest } from "$api";
	import type { RawApiError } from "$api/error";
	import ApiError from "$api/error";
	import ProfileFlagsEditor from "$components/editor/ProfileFlagsEditor.svelte";
	import log from "$lib/log";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();

	let form: { ok: boolean; error: RawApiError | null } | null = $state(null);

	const save = async (flags: string[]) => {
		try {
			await fastRequest("PATCH", `/users/@me/members/${data.member.id}`, {
				body: { flags },
				token: data.token,
			});
			form = { ok: true, error: null };
		} catch (e) {
			log.error("Could not update profile flags for member %s:", data.member.id, e);
			if (e instanceof ApiError) form = { ok: false, error: e.obj };
		}
	};
</script>

<ProfileFlagsEditor profileFlags={data.member.flags} allFlags={data.flags} {save} {form} />
