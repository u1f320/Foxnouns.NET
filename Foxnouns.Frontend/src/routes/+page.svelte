<script lang="ts">
	import GlobalNotice from "$components/GlobalNotice.svelte";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();
</script>

<svelte:head>
	<title>pronouns.cc</title>
</svelte:head>

<div class="container">
	{#if data.meta.notice}
		<GlobalNotice
			id={data.meta.notice.id}
			message={data.meta.notice.message}
			settings={data.meUser?.settings}
			token={data.token}
		/>
	{/if}

	<h1>pronouns.cc</h1>

	<p>
		{data.meta.repository}
		{data.meta.version}
		{data.meta.users.total}
		{data.meta.limits.bio_length}
	</p>
</div>
