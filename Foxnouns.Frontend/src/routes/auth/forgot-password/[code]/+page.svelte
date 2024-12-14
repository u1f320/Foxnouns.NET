<script lang="ts">
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import { t } from "$lib/i18n";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<svelte:head>
	<title>{$t("auth.reset-password-title")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<div class="mx-auto w-lg-50">
		<h3>{$t("auth.reset-password-title")}</h3>

		<FormStatusMarker {form} successMessage={$t("auth.password-changed-hint")} />

		<form method="POST">
			<input type="hidden" name="state" readonly value={data.state} />
			<div class="mb-2">
				<label for="password" class="form-label">{$t("auth.log-in-form-password-label")}</label>
				<input required type="password" id="password" name="password" class="form-control" />
			</div>
			<div class="mb-2">
				<label for="confirm-password" class="form-label">{$t("auth.confirm-password-label")}</label>
				<input
					required
					type="password"
					id="confirm-password"
					name="confirm-password"
					class="form-control"
				/>
			</div>
			<div class="d-grid">
				<button type="submit" class="btn btn-primary">{$t("auth.reset-password-button")}</button>
			</div>
		</form>
	</div>
</div>
