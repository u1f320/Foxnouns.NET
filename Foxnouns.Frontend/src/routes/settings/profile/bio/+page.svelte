<script lang="ts">
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import { renderMarkdown } from "$lib/markdown";
	import { t } from "$lib/i18n";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

	let bio = $state(data.user.bio || "");
</script>

<h4>Bio</h4>

<FormStatusMarker {form} />

<form method="POST">
	<textarea name="bio" class="form-control" style="height: 200px;" bind:value={bio}></textarea>
	<button
		disabled={bio.length > data.meta.limits.bio_length}
		type="submit"
		class="btn btn-primary mt-2 my-1"
	>
		{$t("save-changes")}
	</button>
</form>

<p class="text-muted mt-1">
	{$t("edit-profile.bio-length-hint", {
		length: bio.length,
		maxLength: data.meta.limits.bio_length,
	})}
</p>

{#if bio !== ""}
	<div class="card">
		<div class="card-header">Preview</div>
		<div class="card-body">{@html renderMarkdown(bio)}</div>
	</div>
{/if}
