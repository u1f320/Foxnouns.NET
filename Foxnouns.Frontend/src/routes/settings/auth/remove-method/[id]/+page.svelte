<script lang="ts">
	import type { AuthType } from "$api/models";
	import ErrorAlert from "$components/ErrorAlert.svelte";
	import { t } from "$lib/i18n";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

	const unlinkHeader = (type: AuthType): string => {
		switch (type) {
			case "DISCORD":
				return $t("auth.unlink-discord-header");
			case "GOOGLE":
				return $t("auth.unlink-google-header");
			case "TUMBLR":
				return $t("auth.unlink-tumblr-header");
			case "FEDIVERSE":
				return $t("auth.unlink-fediverse-header");
			case "EMAIL":
				return $t("auth.unlink-email-header");
		}
	};
</script>

<svelte:head>
	<title>{unlinkHeader(data.authMethod.type)} • pronouns.cc</title>
</svelte:head>

<div class="mx-auto w-lg-75">
	<h3>{unlinkHeader(data.authMethod.type)}</h3>
	{#if form?.error}
		<ErrorAlert error={form.error} />
	{/if}
	<p>
		{$t("auth.unlink-confirmation-1", {
			username: data.authMethod.remote_username || data.authMethod.remote_id,
		})}
		<strong>{$t("auth.unlink-confirmation-2")}</strong>
	</p>
	<form method="POST">
		<button type="submit" class="btn btn-danger">{$t("auth.unlink-button")}</button>
	</form>
</div>
