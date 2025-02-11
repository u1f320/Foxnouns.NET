<script lang="ts">
	import AuthMethodList from "$components/settings/AuthMethodList.svelte";
	import EmailSettings from "$components/settings/EmailSettings.svelte";
	import UrlAlert from "$components/URLAlert.svelte";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

	let max = $derived(data.meta.limits.max_auth_methods);
	let canRemove = $derived(data.user.auth_methods.length > 1);
	let discordAccounts = $derived(data.user.auth_methods.filter((m) => m.type === "DISCORD"));
	let googleAccounts = $derived(data.user.auth_methods.filter((m) => m.type === "GOOGLE"));
	let tumblrAccounts = $derived(data.user.auth_methods.filter((m) => m.type === "TUMBLR"));
	let fediAccounts = $derived(data.user.auth_methods.filter((m) => m.type === "FEDIVERSE"));
</script>

<UrlAlert {data} />

{#if data.urls.email_enabled}
	<EmailSettings user={data.user} {canRemove} {max} {form} />
{/if}
{#if data.urls.discord}
	<h3>Discord accounts</h3>
	<AuthMethodList
		methods={discordAccounts}
		{canRemove}
		{max}
		buttonLink="/settings/auth/add-discord"
		buttonText="Link Discord account"
	/>
{/if}
{#if data.urls.google}
	<h3>Google accounts</h3>
	<AuthMethodList
		methods={googleAccounts}
		{canRemove}
		{max}
		buttonLink="/settings/auth/add-google"
		buttonText="Link Google account"
	/>
{/if}
{#if data.urls.tumblr}
	<h3>Tumblr accounts</h3>
	<AuthMethodList
		methods={tumblrAccounts}
		{canRemove}
		{max}
		buttonLink="/settings/auth/add-tumblr"
		buttonText="Link Tumblr account"
	/>
{/if}
<h3>Fediverse accounts</h3>
<AuthMethodList
	methods={fediAccounts}
	{canRemove}
	{max}
	buttonLink="/settings/auth/add-fediverse"
	buttonText="Link Fediverse account"
/>
