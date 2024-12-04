<script lang="ts">
	import { t } from "$lib/i18n";
	import { Button, Input, InputGroup } from "@sveltestrap/sveltestrap";
</script>

<div class="mx-auto w-lg-75">
	<h3>Link a new Fediverse account</h3>

	<form method="POST" action="?/add">
		<InputGroup>
			<Input
				name="instance"
				type="text"
				placeholder={$t("auth.log-in-with-fediverse-instance-placeholder")}
			/>
			<Button type="submit" color="secondary">{$t("auth.log-in-button")}</Button>
		</InputGroup>
		<p>
			{$t("auth.log-in-with-fediverse-error-blurb")}
			<Button formaction="?/forceRefresh" type="submit" color="link">
				{$t("auth.log-in-with-fediverse-force-refresh-button")}
			</Button>
		</p>
	</form>
</div>
