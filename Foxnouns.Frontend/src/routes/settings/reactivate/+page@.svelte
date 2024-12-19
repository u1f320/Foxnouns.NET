<script lang="ts">
	import { t } from "$lib/i18n";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();
</script>

<div class="w-lg-75 mx-auto">
	<h3>{$t("settings.reactivated-header")}</h3>

	<p>{$t("settings.reactivated-explanation")}</p>

	<div class="btn-group">
		<a href="/settings" class="btn btn-primary">{$t("edit-profile.back-to-settings-tab")}</a>
		<a href="/@{data.user.username}" class="btn btn-secondary">
			{$t("error.back-to-profile-button")}
		</a>
	</div>
</div>
