<script lang="ts">
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();
</script>

<svelte:head>
	<title>pronouns.cc</title>
</svelte:head>

<div class="container">
	<h1>pronouns.cc</h1>

	<p>
		{data.meta.repository}
		{data.meta.version}
		{data.meta.users.total}
		{data.meta.limits.bio_length}
	</p>
</div>
