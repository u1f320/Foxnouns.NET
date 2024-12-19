<script lang="ts">
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import { t } from "$lib/i18n";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<svelte:head>
	<title>{$t("settings.soft-delete-page-header")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<div class="w-lg-75 mx-auto">
		<h3>{$t("settings.soft-delete-page-header")}</h3>

		<p>
			{$t("settings.soft-delete-page-explanation")}
		</p>

		<ul>
			<li>{$t("settings.soft-delete-90-days")}</li>
			<li>
				{$t("settings.soft-delete-can-reactivate")}
			</li>
			<li>{$t("settings.soft-delete-keep-username")}</li>
			<li>
				{$t("settings.soft-delete-can-delete-permanently")}
			</li>
		</ul>

		<form method="POST">
			<FormStatusMarker {form} />
			<p>
				{$t("settings.soft-delete-input-label", { username: data.user.username })}
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
			<div class="btn-group mb-2">
				<button type="submit" class="btn btn-danger">
					{$t("settings.soft-delete-page-button")}
				</button>
				<a href="/settings" class="btn btn-secondary">{$t("settings.force-delete-page-cancel")}</a>
			</div>
		</form>
	</div>
</div>
