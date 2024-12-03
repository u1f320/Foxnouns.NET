<script lang="ts">
	import Avatar from "$components/Avatar.svelte";
	import { t } from "$lib/i18n";
	import { Icon, InputGroup } from "@sveltestrap/sveltestrap";
	import { encode } from "base64-arraybuffer";
	import prettyBytes from "pretty-bytes";
	import ShortNoscriptWarning from "./ShortNoscriptWarning.svelte";

	type Props = {
		name: string;
		current: string | null;
		alt: string;
		update: (avatar: string) => Promise<void>;
		updated: boolean;
	};
	let { name, current, alt, update: onclick, updated }: Props = $props();

	const MAX_AVATAR_BYTES = 1_000_000;

	let avatarFiles: FileList | null = $state(null);
	let avatar: string = $state("");
	let avatarExists = $derived(avatar !== "");
	let avatarTooLarge = $derived(avatar !== "" && avatar.length > MAX_AVATAR_BYTES);

	$effect(() => {
		getAvatar(avatarFiles);
	});

	const getAvatar = async (list: FileList | null) => {
		if (!list || list.length === 0) {
			avatar = "";
			return;
		}

		const buffer = await list[0].arrayBuffer();
		const base64 = encode(buffer);

		const uri = `data:${list[0].type};base64,${base64}`;
		avatar = uri;
	};
</script>

<p class="text-center">
	<Avatar {name} url={avatarExists ? avatar : current} {alt} />
</p>

<InputGroup class="mb-2">
	<input
		class="form-control"
		id="avatar"
		type="file"
		bind:files={avatarFiles}
		accept="image/png, image/jpeg, image/gif, image/webp"
	/>
	<button
		class="btn btn-secondary"
		disabled={!avatarExists || avatarTooLarge}
		onclick={() => onclick(avatar)}
	>
		{$t("edit-profile.update-avatar")}
	</button>
</InputGroup>

<ShortNoscriptWarning />

{#if updated}
	<p class="text-success-emphasis">
		<Icon name="check-circle-fill" />
		{$t("edit-profile.avatar-updated")}
	</p>
{/if}

{#if avatarTooLarge}
	<p class="text-danger-emphasis">
		<Icon name="exclamation-circle-fill" />
		{$t("edit-profile.file-too-large", {
			max: prettyBytes(MAX_AVATAR_BYTES),
			current: prettyBytes(avatar.length),
		})}
	</p>
{/if}
