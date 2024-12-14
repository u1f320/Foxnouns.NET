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
		active?: boolean;
		class?: string;
	};
	let {
		icon,
		tooltip,
		color = "primary",
		type,
		id,
		onclick,
		outline,
		active,
		class: className,
	}: Props = $props();
</script>

<button
	{id}
	{type}
	use:tippy={{ content: tooltip }}
	class="btn {outline ? `btn-outline-${color}` : `btn-${color}`} {className || ''}"
	class:active
	aria-pressed={active}
	{onclick}
>
	<Icon name={icon} />
	<span class="visually-hidden">{tooltip}</span>
</button>
