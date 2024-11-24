<script lang="ts">
	import type { Snippet } from "svelte";
	import "../app.scss";
	import type { LayoutData } from "./$types";
	import Navbar from "$components/Navbar.svelte";

	type Props = { children: Snippet; data: LayoutData };
	let { children, data }: Props = $props();
</script>

<Navbar user={data.meUser} meta={data.meta} />

{@render children?.()}
