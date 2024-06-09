<script lang="ts">
	import Navbar from "$lib/nav/Navbar.svelte";
	import "bootstrap/dist/css/bootstrap.min.css";
	import type { LayoutData } from "./$types";

	export let data: LayoutData;
</script>

<Navbar user={data.user} />

<slot />
