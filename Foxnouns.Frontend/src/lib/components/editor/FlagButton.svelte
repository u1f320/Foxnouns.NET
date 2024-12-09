<script lang="ts">
	import type { PrideFlag } from "$api/models";
	import type { MouseEventHandler } from "svelte/elements";
	import EditorFlagImage from "./EditorFlagImage.svelte";
	import tippy from "$lib/tippy";

	type Props = {
		flag: PrideFlag;
		tooltip?: string;
		class?: string;
		onclick: MouseEventHandler<HTMLButtonElement>;
		padding?: boolean;
	};
	let { flag, tooltip, class: className, onclick, padding }: Props = $props();

	let tip = $derived(tooltip ? tippy : () => {});
</script>

<button
	type="button"
	class="btn btn-outline-secondary {className || ''}"
	class:padding
	{onclick}
	use:tip={{ content: tooltip }}
>
	<EditorFlagImage {flag} />
	{flag.name}
</button>

<style>
	.padding {
		margin-right: 5px;
		margin-bottom: 5px;
	}
</style>
