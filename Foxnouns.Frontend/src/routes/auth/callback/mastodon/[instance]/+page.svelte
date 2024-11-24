<script lang="ts">
	import { Button, Input, Label } from "@sveltestrap/sveltestrap";
	import type { ActionData, PageData } from "./$types";
	import { t } from "$lib/i18n";
	import { enhance } from "$app/forms";
	import ErrorAlert from "$components/ErrorAlert.svelte";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<svelte:head>
	<title>{$t("auth.register-with-mastodon")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<h1>{$t("auth.register-with-mastodon")}</h1>

	{#if form?.error}
		<ErrorAlert error={form?.error} />
	{/if}

	<form method="POST" use:enhance>
		<div class="mb-3">
			<Label>{$t("auth.remote-fediverse-account-label")}</Label>
			<Input type="text" readonly value={data.remoteUser} />
		</div>
		<div class="mb-3">
			<Label>{$t("auth.register-username-label")}</Label>
			<Input type="text" name="username" required />
		</div>
		<input type="hidden" name="ticket" value={data.ticket} />
		<Button color="primary" type="submit">{$t("auth.register-button")}</Button>
	</form>
</div>
