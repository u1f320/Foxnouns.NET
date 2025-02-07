<script lang="ts">
	import Error from "$components/Error.svelte";
	import NewAuthMethod from "$components/settings/NewAuthMethod.svelte";
	import { t } from "$lib/i18n";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();
</script>

<svelte:head>
	<title>{$t("auth.register-with-tumblr")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	{#if data.error}
		<h1>{$t("auth.register-with-tumblr")}</h1>
		<Error error={data.error} />
	{:else}
		<NewAuthMethod method={data.newAuthMethod!} user={data.meUser!} />
	{/if}
</div>
