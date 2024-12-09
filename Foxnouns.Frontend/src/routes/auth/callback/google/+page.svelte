<script lang="ts">
	import Error from "$components/Error.svelte";
	import NewAuthMethod from "$components/settings/NewAuthMethod.svelte";
	import OauthRegistrationForm from "$components/settings/OauthRegistrationForm.svelte";
	import { t } from "$lib/i18n";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<svelte:head>
	<title>{$t("auth.register-with-google")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	{#if data.error}
		<h1>{$t("auth.register-with-google")}</h1>
		<Error error={data.error} />
	{:else if data.isLinkRequest}
		<NewAuthMethod method={data.newAuthMethod!} user={data.meUser!} />
	{:else}
		<OauthRegistrationForm
			title={$t("auth.register-with-google")}
			remoteLabel={$t("auth.remote-google-account-label")}
			remoteUser={data.remoteUser!}
			ticket={data.ticket!}
			error={form?.error}
		/>
	{/if}
</div>
