<script lang="ts">
	import type { ActionData } from "./$types";

	type Props = { form: ActionData };
	let { form }: Props = $props();
</script>

<svelte:head>
	<title>Look up a user • pronouns.cc</title>
</svelte:head>

<h1>Look up a user</h1>

<form method="POST">
	<div class="input-group w-lg-50 mb-2">
		<input type="text" class="form-control" name="query" placeholder="Query" required />
		<button class="btn btn-primary" type="submit">Search</button>
	</div>
	<div class="form-check mb-3">
		<input class="form-check-input" type="checkbox" value="yes" name="fuzzy" id="fuzzy" />
		<label class="form-check-label" for="fuzzy">Fuzzy?</label>
	</div>
</form>

<div class="list-group">
	{#each form?.users || [] as user (user.id)}
		<a href="/admin/lookup/{user.id}" class="list-group-item list-group-item-action">
			{user.username} <span class="text-secondary">({user.id})</span>
		</a>
	{:else}
		<div class="list-group-item">No results</div>
	{/each}
</div>
