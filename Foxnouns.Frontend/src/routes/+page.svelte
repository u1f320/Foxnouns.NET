<script lang="ts">
	import type { PageData } from "./$types";
	export let data: PageData;
</script>

<h1>Welcome to SvelteKit</h1>
<p>Visit <a href="https://kit.svelte.dev">kit.svelte.dev</a> to read the documentation</p>

<p>
    are you logged in? {data.user !== undefined}
    {#if data.user}
    <br />hello, {data.user.username}!
    <br />your ID: {data.user.id}
    {/if}
</p>

<p>
    stats: {data.meta.users.total} users, {data.meta.members} members
</p>
