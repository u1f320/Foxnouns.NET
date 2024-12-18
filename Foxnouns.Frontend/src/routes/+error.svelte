<script lang="ts">
	import { page } from "$app/stores";
	import Error from "$components/Error.svelte";
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
	<Error {error} headerElem="h3" />
	<div class="btn-group mt-2">
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
