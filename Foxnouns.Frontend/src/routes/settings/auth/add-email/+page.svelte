<script lang="ts">
	import { t } from "$lib/i18n";
	import type { ActionData, PageData } from "./$types";
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<div class="mx-auto w-lg-75">
	<h3>{$t("auth.link-email-header")}</h3>

	<FormStatusMarker {form} successMessage={$t("auth.check-inbox-for-link-hint")} />

	<form method="POST" action="?/add">
		<div class="mb-1">
			<label for="email" class="form-label">{$t("auth.log-in-form-email-label")}</label>
			<input
				type="email"
				id="email"
				name="email"
				placeholder="me@example.com"
				class="form-control"
				required
			/>
		</div>
		<div class="mb-1">
			<label for="password" class="form-label">{$t("auth.log-in-form-password-label")}</label>
			<input type="password" id="password" name="password" class="form-control" required />
		</div>
		{#if data.firstEmail}
			<div class="mb-1">
				<label for="confirm-password" class="form-label">
					{$t("auth.confirm-password-label")}
				</label>
				<input
					type="password"
					id="confirm-password"
					name="confirm-password"
					class="form-control"
					required
				/>
			</div>
		{/if}

		<button type="submit" class="btn btn-secondary mt-2">{$t("auth.add-email-address")}</button>
	</form>
</div>
