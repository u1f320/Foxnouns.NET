<script lang="ts">
	import { t } from "$lib/i18n";
</script>

<h3>{$t("settings.force-log-out-title")}</h3>

<p>
	{$t("settings.force-log-out-confirmation")}
	<strong>{$t("settings.force-log-out-warning")}</strong>
</p>

<form method="POST">
	<button type="submit" class="btn btn-danger">{$t("settings.force-log-out-button")}</button>
</form>
