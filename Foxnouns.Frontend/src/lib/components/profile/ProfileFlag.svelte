<script lang="ts">
	import type { PrideFlag } from "$api/models/user";
	import { DEFAULT_FLAG } from "$lib";
	import tippy from "$lib/tippy";

	type Props = { flag: PrideFlag };
	let { flag }: Props = $props();
</script>

<span class="mx-2 my-1">
	<img
		use:tippy={{ content: flag.description ?? flag.name }}
		class="profile-flag"
		src={flag.image_url ?? DEFAULT_FLAG}
		alt={flag.description ?? flag.name}
	/>
	{flag.name}
</span>
