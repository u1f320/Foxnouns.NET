<script lang="ts">
	import { t } from "$lib/i18n";
</script>

<svelte:head>
	<title>{$t("settings.soft-delete-page-header")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<div class="w-lg-75 mx-auto">
		<h3>{$t("settings.account-is-deactivated-header")}</h3>
		<p>
			{$t("settings.account-is-deactivated-description")}
		</p>
		<p>{$t("settings.account-is-deleted-close-page")}</p>
		<p>
			<a href="/" class="btn btn-secondary">{$t("error.back-to-main-page-button")}</a>
		</p>
	</div>
</div>
