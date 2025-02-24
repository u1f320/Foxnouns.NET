<script lang="ts">
	import { fastRequest } from "$api";
	import type { RawApiError } from "$api/error";
	import ApiError from "$api/error";
	import LinksEditor from "$components/editor/LinksEditor.svelte";
	import ProfileFlagsEditor from "$components/editor/ProfileFlagsEditor.svelte";
	import log from "$lib/log";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();

	let flagForm: { ok: boolean; error: RawApiError | null } | null = $state(null);
	let linksForm: { ok: boolean; error: RawApiError | null } | null = $state(null);

	const flagSave = async (flags: string[]) => {
		try {
			await fastRequest("PATCH", `/users/@me/members/${data.member.id}`, {
				body: { flags },
				token: data.token,
			});
			flagForm = { ok: true, error: null };
		} catch (e) {
			log.error("Could not update profile flags for member %s:", data.member.id, e);
			if (e instanceof ApiError) flagForm = { ok: false, error: e.obj };
		}
	};

	const linksSave = async (links: string[]) => {
		try {
			await fastRequest("PATCH", "/users/@me", {
				body: { links },
				token: data.token,
			});
			linksForm = { ok: true, error: null };
		} catch (e) {
			log.error("Could not update profile links:", e);
			if (e instanceof ApiError) linksForm = { ok: false, error: e.obj };
		}
	};
</script>

<ProfileFlagsEditor
	stateKey="member-{data.member.id}-flags"
	profileFlags={data.member.flags}
	allFlags={data.flags}
	save={flagSave}
	form={flagForm}
/>

<LinksEditor
	stateKey="member-{data.member.id}-links"
	currentLinks={data.member.links}
	save={linksSave}
	form={linksForm}
/>
