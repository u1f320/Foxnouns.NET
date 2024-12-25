<script lang="ts">
	import type { Snippet } from "svelte";
	import "../app.scss";
	import type { LayoutData } from "./$types";
	import Navbar from "$components/Navbar.svelte";
	import Footer from "$components/Footer.svelte";

	type Props = { children: Snippet; data: LayoutData };
	let { children, data }: Props = $props();
</script>

<div class="d-flex flex-column min-vh-100">
	<div class="flex-grow-1">
		<Navbar user={data.meUser} meta={data.meta} />
		{@render children?.()}
	</div>
	<Footer meta={data.meta} />
</div>
