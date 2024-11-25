<script lang="ts">
	import type { PrideFlag } from "$api/models/user";
	import tippy from "$lib/tippy";

	type Props = { flag: PrideFlag };
	let { flag }: Props = $props();
</script>

<span class="mx-2 my-1">
	<img
		use:tippy={{ content: flag.description ?? flag.name }}
		class="flag"
		src={flag.image_url}
		alt={flag.description ?? flag.name}
	/>
	{flag.name}
</span>

<style>
	.flag {
		height: 1.5rem;
		max-width: 200px;
		border-radius: 3px;
	}
</style>
