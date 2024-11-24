<script lang="ts">
	import { Icon, Tooltip } from "@sveltestrap/sveltestrap";

	import type { CustomPreference } from "$api/models/user";

	type Props = { preference: CustomPreference };
	let { preference }: Props = $props();

	// svelte-ignore non_reactive_update
	let elem: HTMLSpanElement;
</script>

<span bind:this={elem} aria-hidden={true}>
	<Icon name={preference.icon} />
</span>
<span class="visually-hidden">{preference.tooltip}:</span>
<Tooltip aria-hidden target={elem} placement="top">{preference.tooltip}</Tooltip>
