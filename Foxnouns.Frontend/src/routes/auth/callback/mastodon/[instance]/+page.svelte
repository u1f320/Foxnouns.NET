<script lang="ts">
	import type { ActionData, PageData } from "./$types";
	import { t } from "$lib/i18n";
	import OauthRegistrationForm from "$components/settings/OauthRegistrationForm.svelte";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<svelte:head>
	<title>{$t("auth.register-with-mastodon")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<OauthRegistrationForm
		title={$t("auth.register-with-mastodon")}
		remoteLabel={$t("auth.remote-fediverse-account-label")}
		remoteUser={data.remoteUser}
		ticket={data.ticket}
		error={form?.error}
	/>
</div>
