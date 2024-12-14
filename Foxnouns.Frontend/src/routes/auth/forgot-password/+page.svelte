<script lang="ts">
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import { t } from "$lib/i18n";
	import type { ActionData } from "./$types";

	type Props = { form: ActionData };
	let { form }: Props = $props();
</script>

<svelte:head>
	<title>{$t("auth.forgot-password-title")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<div class="mx-auto w-lg-50">
		<h3>{$t("auth.forgot-password-title")}</h3>

		<FormStatusMarker {form} successMessage={$t("auth.check-inbox-for-link-hint")} />

		<form method="POST">
			<label for="email" class="form-label">{$t("auth.log-in-form-email-label")}</label>
			<input
				required
				type="email"
				id="email"
				name="email"
				placeholder="me@example.com"
				class="form-control mb-2"
			/>
			<div class="d-grid">
				<button type="submit" class="btn btn-primary">{$t("auth.reset-password-button")}</button>
			</div>
		</form>
	</div>
</div>
