<script lang="ts">
	import type { ActionData, PageData } from "./$types";
	import { t } from "$lib/i18n";
	import { enhance } from "$app/forms";
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
			<div class="col-md mb-4">
				<h2>{$t("auth.log-in-form-title")}</h2>
				<form method="POST" action="?/login" use:enhance>
					<div class="mb-2">
						<label class="form-label" for="email">{$t("auth.log-in-form-email-label")}</label>
						<input
							class="form-control"
							type="email"
							id="email"
							name="email"
							placeholder="me@example.com"
						/>
					</div>
					<div class="mb-2">
						<label class="form-label" for="password">{$t("auth.log-in-form-password-label")}</label>
						<input class="form-control" type="password" id="password" name="password" />
					</div>
					<button class="btn btn-primary" type="submit">{$t("auth.log-in-button")}</button>
				</form>
				<p class="mt-2">
					<a href="/auth/register">{$t("auth.log-in-sign-up-link")}</a> •
					<a href="/auth/forgot-password">{$t("auth.log-in-forgot-password-link")}</a>
				</p>
			</div>
		{:else}
			<div class="col-lg-3"></div>
		{/if}
		<div class="col-md">
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
					<div class="input-group">
						<input
							class="form-control"
							name="instance"
							type="text"
							placeholder={$t("auth.log-in-with-fediverse-instance-placeholder")}
						/>
						<button class="btn btn-secondary" type="submit">{$t("auth.log-in-button")}</button>
					</div>
					<p>
						{$t("auth.log-in-with-fediverse-error-blurb")}
						<button class="btn btn-link" formaction="?/fediForceRefresh" type="submit">
							{$t("auth.log-in-with-fediverse-force-refresh-button")}
						</button>
					</p>
				</form>
			{/if}
		</div>
	</div>
</div>
