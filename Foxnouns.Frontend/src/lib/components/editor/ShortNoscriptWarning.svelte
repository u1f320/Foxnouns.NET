<script lang="ts">
	import { t } from "$lib/i18n";
	import { Icon } from "@sveltestrap/sveltestrap";
</script>

<noscript>
	<p class="text-danger-emphasis">
		<Icon name="exclamation-circle-fill" aria-hidden />
		{$t("error.noscript-short")}
	</p>
</noscript>
