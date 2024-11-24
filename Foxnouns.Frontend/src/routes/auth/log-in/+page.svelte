<script lang="ts">
	import type { ActionData, PageData } from "./$types";
	import { t } from "$lib/i18n";
	import { enhance } from "$app/forms";
	import { Button, ButtonGroup, Input, InputGroup } from "@sveltestrap/sveltestrap";
	import ErrorAlert from "$components/ErrorAlert.svelte";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<svelte:head>
	<title>{$t("title.log-in")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<div class="row">
		{#if form?.error}
			<ErrorAlert error={form.error} />
		{/if}
	</div>
	<div class="row">
		{#if data.urls.email_enabled}
			<div class="col col-md mb-4">
				<h2>{$t("auth.log-in-form-title")}</h2>
				<form method="POST" action="?/login" use:enhance>
					<div class="mb-2">
						<label class="form-label" for="email">{$t("auth.log-in-form-email-label")}</label>
						<Input type="email" id="email" name="email" placeholder="me@example.com" />
					</div>
					<div class="mb-2">
						<label class="form-label" for="password">{$t("auth.log-in-form-password-label")}</label>
						<Input type="password" id="password" name="password" />
					</div>
					<ButtonGroup>
						<Button type="submit" color="primary">{$t("auth.log-in-button")}</Button>
						<a class="btn btn-secondary" href="/auth/register">
							{$t("auth.register-with-email-button")}
						</a>
					</ButtonGroup>
				</form>
			</div>
		{:else}
			<div class="col-lg-3"></div>
		{/if}
		<div class="col col-md">
			<h3>{$t("auth.log-in-3rd-party-header")}</h3>
			<p>{$t("auth.log-in-3rd-party-desc")}</p>
			<form method="POST" action="?/fediToggle" use:enhance>
				<div class="list-group">
					{#if data.urls.discord}
						<a href={data.urls.discord} class="list-group-item list-group-item-action">
							{$t("auth.log-in-with-discord")}
						</a>
					{/if}
					{#if data.urls.google}
						<a href={data.urls.google} class="list-group-item list-group-item-action">
							{$t("auth.log-in-with-google")}
						</a>
					{/if}
					{#if data.urls.tumblr}
						<a href={data.urls.tumblr} class="list-group-item list-group-item-action">
							{$t("auth.log-in-with-tumblr")}
						</a>
					{/if}
					<button type="submit" class="list-group-item list-group-item-action">
						{$t("auth.log-in-with-the-fediverse")}
					</button>
				</div>
			</form>
			{#if form?.showFediBox}
				<h4 class="mt-4">{$t("auth.log-in-with-the-fediverse")}</h4>
				<form method="POST" action="?/fedi" use:enhance>
					<InputGroup>
						<Input name="instance" type="text" placeholder="Your instance (i.e. mastodon.social)" />
						<Button type="submit" color="secondary">{$t("auth.log-in-button")}</Button>
					</InputGroup>
					<p>
						{$t("auth.log-in-with-fediverse-error-blurb")}
						<Button formaction="?/fediForceRefresh" type="submit" color="link">
							{$t("auth.log-in-with-fediverse-force-refresh-button")}
						</Button>
					</p>
				</form>
			{/if}
		</div>
	</div>
</div>
