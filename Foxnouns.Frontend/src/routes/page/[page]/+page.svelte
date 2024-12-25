<script lang="ts">
	import { renderUnsafeMarkdown } from "$lib/markdown";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();

	let md = $derived(renderUnsafeMarkdown(data.text));
	let title = $derived.by(() => {
		let title = data.text.split("\n")[0];
		if (title.startsWith("# ")) title = title.substring("# ".length);
		return title;
	});
</script>

<svelte:head>
	<title>{title} • pronouns.cc</title>
</svelte:head>

<div class="container">
	{@html md}
</div>
