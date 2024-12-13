<script lang="ts">
	import { t } from "$lib/i18n";
	import { renderMarkdown } from "$lib/markdown";

	type Props = { value: string; maxLength: number };
	let { value = $bindable(), maxLength }: Props = $props();
</script>

<textarea name="bio" class="form-control" style="height: 200px;" bind:value></textarea>
<button disabled={value.length > maxLength} type="submit" class="btn btn-primary mt-2 my-1">
	{$t("save-changes")}
</button>

<p class="text-muted mt-1">
	{$t("edit-profile.bio-length-hint", {
		length: value.length,
		maxLength,
	})}
</p>

{#if value !== ""}
	<div class="card">
		<div class="card-header">{$t("edit-profile.preview")}</div>
		<!-- bios are sanitized before being passed to @html and the allowed markdown is heavily restricted -->
		<!-- eslint-disable-next-line svelte/no-at-html-tags -->
		<div class="card-body">{@html renderMarkdown(value)}</div>
	</div>
{/if}
