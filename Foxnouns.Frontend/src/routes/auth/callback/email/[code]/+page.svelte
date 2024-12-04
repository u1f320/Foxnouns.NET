<script lang="ts">
	import Error from "$components/Error.svelte";
	import ErrorAlert from "$components/ErrorAlert.svelte";
	import NewAuthMethod from "$components/settings/NewAuthMethod.svelte";
	import { t } from "$lib/i18n";
	import { Label, Input, Button } from "@sveltestrap/sveltestrap";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<svelte:head>
	<title>{$t("auth.register-with-email")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	{#if data.error}
		<h1>{$t("auth.register-with-email")}</h1>
		<Error error={data.error} />
	{:else if data.isLinkRequest}
		<NewAuthMethod method={data.newAuthMethod!} user={data.meUser!} />
	{:else}
		<h1>{$t("auth.register-with-email")}</h1>

		{#if form?.error}
			<ErrorAlert error={form.error} />
		{/if}

		<form method="POST">
			<div class="mb-3">
				<Label>{$t("auth.email-label")}</Label>
				<Input type="text" readonly value={data.remoteUser} />
			</div>
			<div class="mb-3">
				<Label>{$t("auth.register-username-label")}</Label>
				<Input type="text" name="username" required />
			</div>
			<div class="mb-3">
				<Label>{$t("auth.log-in-form-password-label")}</Label>
				<Input type="password" name="password" required />
			</div>
			<div class="mb-3">
				<Label>{$t("auth.confirm-password-label")}</Label>
				<Input type="password" name="confirm-password" required />
			</div>
			<input type="hidden" name="ticket" value={data.ticket!} />
			<Button color="primary" type="submit">{$t("auth.register-button")}</Button>
		</form>
	{/if}
</div>
