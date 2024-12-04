<script lang="ts">
	import type { RawApiError } from "$api/error";
	import ErrorAlert from "$components/ErrorAlert.svelte";
	import { t } from "$lib/i18n";
	import { Button, Input, Label } from "@sveltestrap/sveltestrap";

	type Props = {
		title: string;
		remoteLabel: string;
		remoteUser: string;
		ticket: string;
		error?: RawApiError;
	};
	let { title, remoteLabel, remoteUser, ticket, error }: Props = $props();
</script>

<h1>{title}</h1>

{#if error}
	<ErrorAlert {error} />
{/if}

<form method="POST">
	<div class="mb-3">
		<Label>{remoteLabel}</Label>
		<Input type="text" readonly value={remoteUser} />
	</div>
	<div class="mb-3">
		<Label>{$t("auth.register-username-label")}</Label>
		<Input type="text" name="username" required />
	</div>
	<input type="hidden" name="ticket" value={ticket} />
	<Button color="primary" type="submit">{$t("auth.register-button")}</Button>
</form>
