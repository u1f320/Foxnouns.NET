<script lang="ts">
	import OauthRegistrationForm from "$components/settings/OauthRegistrationForm.svelte";
	import { t } from "$lib/i18n";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<svelte:head>
	<title>{$t(`auth.register-with-${data.type}`)} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<OauthRegistrationForm
		title={$t(`auth.register-with-${data.type}`)}
		remoteLabel={$t(`auth.remote-${data.type}-account-label`)}
		remoteUser={data.remoteUsername}
		ticket={data.ticket}
		error={form?.error}
	/>
</div>
