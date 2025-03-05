<script lang="ts">
	import Avatar from "$components/Avatar.svelte";
	import { t } from "$lib/i18n";
	import { Icon, InputGroup, Modal } from "@sveltestrap/sveltestrap";
	import Cropper, { type CropArea, type OnCropCompleteEvent } from "svelte-easy-crop";
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
	let cropperOpen = $state(false);

	$effect(() => {
		getAvatar(avatarFiles);
	});

	const getAvatar = async (list: FileList | null) => {
		if (!list || list.length === 0) {
			uncroppedAvatar = "";
			return;
		}

		const buffer = await list[0].arrayBuffer();
		const base64 = encode(buffer);

		const uri = `data:${list[0].type};base64,${base64}`;
		uncroppedAvatar = uri;
		cropperOpen = true;
	};

	let uncroppedAvatar: string = $state("");
	let crop = $state({ x: 0, y: 0 });
	let zoom = $state(1);
	let croppedArea = $state({ x: 0, y: 0, height: 0, width: 0 } satisfies CropArea);

	const onCropComplete = (e: OnCropCompleteEvent) => {
		croppedArea = e.pixels;
	};

	const doCrop = () => {
		cropperOpen = false;
		if (!uncroppedAvatar) {
			return;
		}

		const canvas = document.createElement("canvas");
		const ctx = canvas.getContext("2d");
		const img = new Image();
		img.onload = () => {
			canvas.width = croppedArea.width;
			canvas.height = croppedArea.height;

			ctx?.drawImage(
				img,
				croppedArea.x,
				croppedArea.y,
				croppedArea.width,
				croppedArea.height,
				0,
				0,
				croppedArea.width,
				croppedArea.height,
			);

			avatar = canvas.toDataURL("image/webp", 1);
		};
		img.src = uncroppedAvatar;
	};
</script>

<p class="text-center">
	<Avatar {name} url={avatarExists ? avatar : current} {alt} />
</p>

<Modal
	isOpen={cropperOpen}
	autoFocus
	backdrop
	fade
	keyboard
	returnFocusAfterClose
	toggle={() => (cropperOpen = !cropperOpen)}
>
	<div class="modal-header">
		<h1 class="modal-title fs-5">{$t("editor.crop-avatar-header")}</h1>
	</div>
	<div class="modal-body cropper-wrapper">
		{#if uncroppedAvatar}
			<Cropper
				image={uncroppedAvatar}
				{crop}
				{zoom}
				minZoom={1}
				maxZoom={4}
				aspect={1 / 1}
				oncropcomplete={onCropComplete}
			/>
		{/if}
	</div>
	<div class="modal-footer">
		<button type="button" class="btn btn-primary" onclick={() => doCrop()}>
			{$t("editor.crop-avatar-button")}
		</button>
		<button type="button" class="btn btn-outline-secondary" onclick={() => (cropperOpen = false)}>
			{$t("cancel")}
		</button>
	</div>
</Modal>

<InputGroup class="mb-2">
	<input
		class="form-control"
		id="avatar"
		type="file"
		bind:files={avatarFiles}
		accept="image/png, image/jpeg, image/gif, image/webp"
	/>
	<button
		class="btn btn-primary"
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

<style>
	.cropper-wrapper {
		min-height: 30em;
	}
</style>
