<script lang="ts">
	import { Icon } from "@sveltestrap/sveltestrap";
	import type { MouseEventHandler } from "svelte/elements";
	import tippy from "$lib/tippy";

	type Props = {
		icon: string;
		tooltip: string;
		color?: "primary" | "secondary" | "success" | "danger";
		type?: "submit" | "reset" | "button";
		id?: string;
		onclick?: MouseEventHandler<HTMLButtonElement>;
		outline?: boolean;
	};
	let { icon, tooltip, color = "primary", type, id, onclick, outline }: Props = $props();
</script>

<button
	{id}
	{type}
	use:tippy={{ content: tooltip }}
	class="btn {outline ? `btn-outline-${color}` : `btn-${color}`}"
	{onclick}
>
	<Icon name={icon} />
	<span class="visually-hidden">{tooltip}</span>
</button>
