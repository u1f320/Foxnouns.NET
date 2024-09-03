<script lang="ts">
	import type { PageData } from "./$types";
	export let data: PageData;
</script>

<svelte:head>
	<title>@{data.user.username} • pronouns.cc</title>
</svelte:head>

<h1>this is the user page for @{data.user.username}</h1>
