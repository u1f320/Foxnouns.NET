<script lang="ts">
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import { t } from "$lib/i18n";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<svelte:head>
	<title>{$t("settings.force-delete-page-header")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<div class="w-lg-75 mx-auto">
		<h3>{$t("settings.force-delete-page-header")}</h3>

		<p>
			{$t("settings.force-delete-page-explanation")}
		</p>

		<ul>
			<li>{$t("settings.force-delete-immediate-delete")}</li>
			<li>{$t("settings.force-delete-username-available")}</li>
			<li><strong>{$t("settings.force-delete-irreversible")}</strong></li>
		</ul>

		<p>
			{$t("settings.force-delete-export-hint")}
			<a href="/settings/export">{$t("settings.force-delete-export-link")}</a>
		</p>

		<form method="POST">
			<FormStatusMarker {form} />
			<p>
				{$t("settings.force-delete-input-label", { username: data.user.username })}
				<input
					class="form-control mt-2"
					type="text"
					name="username"
					required
					placeholder="@{data.user.username}"
					autocomplete="off"
				/>
				<input type="hidden" value="@{data.user.username}" readonly name="current-username" />
			</p>
			<div class="form-check">
				<input
					class="form-check-input"
					type="checkbox"
					value="yes"
					name="confirm"
					id="confirm"
					required
				/>
				<label class="form-check-label" for="confirm">
					{$t("settings.force-delete-checkbox-label")}
				</label>
			</div>
			<div class="btn-group mt-3 mb-2">
				<button type="submit" class="btn btn-danger">
					{$t("settings.force-delete-page-button")}
				</button>
				<a href="/settings" class="btn btn-secondary">{$t("settings.force-delete-page-cancel")}</a>
			</div>
		</form>
	</div>
</div>
