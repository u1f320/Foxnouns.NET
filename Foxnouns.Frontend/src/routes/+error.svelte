<script lang="ts">
	import { page } from "$app/stores";
	import { t } from "$lib/i18n";
	import type { LayoutData } from "./$types";

	let error = $derived($page.error!);

	type Props = { data: LayoutData };
	let { data }: Props = $props();
</script>

<svelte:head>
	<title>{$t("title.an-error-occurred")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<h3>{$t("title.an-error-occurred")}</h3>
	<p>
		<strong>{$page.status}</strong>: {error.message}
	</p>
	<p>
		{#if $page.status === 400}
			{$t("error.400-description")}
		{:else if $page.status === 404}
			{$t("error.404-description")}
		{:else if $page.status === 500}
			{$t("error.500-description")}
		{:else}
			{$t("error.unknown-status-description")}
		{/if}
	</p>
	<div class="btn-group">
		{#if data.meUser}
			<a class="btn btn-primary" href="/@{data.meUser.username}">
				{$t("error.back-to-profile-button")}
			</a>
		{:else}
			<a class="btn btn-primary" href="/">{$t("error.back-to-main-page-button")}</a>
		{/if}
		<button class="btn btn-secondary" type="button" onclick={() => history.back()}>
			{$t("error.back-to-prev-page-button")}
		</button>
	</div>
</div>
