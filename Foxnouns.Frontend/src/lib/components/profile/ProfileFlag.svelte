<script lang="ts">
	import type { PrideFlag } from "$api/models/user";
	import { Tooltip } from "@sveltestrap/sveltestrap";

	type Props = { flag: PrideFlag };
	let { flag }: Props = $props();

	// svelte-ignore non_reactive_update
	let elem: HTMLImageElement;
</script>

<span class="mx-2 my-1">
	<Tooltip target={elem} aria-hidden placement="top">{flag.description ?? flag.name}</Tooltip>
	<img bind:this={elem} class="flag" src={flag.image_url} alt={flag.description ?? flag.name} />
	{flag.name}
</span>

<style>
	.flag {
		height: 1.5rem;
		max-width: 200px;
		border-radius: 3px;
	}
</style>
