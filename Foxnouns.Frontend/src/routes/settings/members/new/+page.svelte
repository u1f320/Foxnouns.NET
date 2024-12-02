<script lang="ts">
	import ErrorAlert from "$components/ErrorAlert.svelte";
	import { t } from "$lib/i18n";
	import type { ActionData } from "./$types";

	type Props = { form: ActionData };
	let { form }: Props = $props();
</script>

<h3>{$t("settings.create-member-title")}</h3>

{#if form?.error}
	<ErrorAlert error={form.error} />
{/if}

<form method="POST">
	<div class="my-3">
		<label class="form-label" for="name">{$t("settings.create-member-name-label")}</label>
		<input class="form-control" type="text" id="name" name="name" required autocomplete="off" />
	</div>
	<button class="btn btn-primary" type="submit">{$t("profile.create-member-button")}</button>
</form>
