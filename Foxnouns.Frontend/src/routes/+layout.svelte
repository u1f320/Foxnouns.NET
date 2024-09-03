<script lang="ts">
	import type { LayoutData } from "./$types";
	import "../app.scss";
	import Navbar from "$lib/nav/Navbar.svelte";

	export let data: LayoutData;
</script>

<Navbar user={data.currentUser} />
<slot />
