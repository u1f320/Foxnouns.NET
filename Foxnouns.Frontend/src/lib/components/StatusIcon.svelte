<script lang="ts">
	import { Icon } from "@sveltestrap/sveltestrap";

	import type { CustomPreference } from "$api/models/user";
	import tippy from "$lib/tippy";

	type Props = { preference: CustomPreference };
	let { preference }: Props = $props();
</script>

<span use:tippy={{ content: preference.tooltip }} aria-hidden={true}>
	<Icon name={preference.icon} />
</span>
<span class="visually-hidden">{preference.tooltip}:</span>
