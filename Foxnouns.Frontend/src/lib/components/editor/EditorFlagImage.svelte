<script lang="ts">
	import type { PrideFlag } from "$api/models/user";
	import { DEFAULT_FLAG } from "$lib";

	type Props = { flag: PrideFlag };
	let { flag }: Props = $props();
</script>

<img class="flag" src={flag.image_url ?? DEFAULT_FLAG} alt={flag.description ?? flag.name} />

<style>
	.flag {
		height: 1.5rem;
		max-width: 200px;
		border-radius: 3px;
	}
</style>
