<script lang="ts">
	import type { PageData } from "./$types";
	export let data: PageData;
</script>

<svelte:head>
	<title>pronouns.cc</title>
</svelte:head>

<h1 class="title">Welcome to SvelteKit</h1>
<p>Visit <a href="https://kit.svelte.dev">kit.svelte.dev</a> to read the documentation</p>

<p>
	are you logged in? {data.currentUser !== undefined}
	{#if data.currentUser}
		<br />hello, {data.currentUser.username}!
		<br />your ID: {data.currentUser.id}
	{/if}
</p>

<p>
	<strong>stats:</strong>
	{data.meta.users.total} users, {data.meta.members} members
</p>
