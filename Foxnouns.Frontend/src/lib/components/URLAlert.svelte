<script lang="ts">
	import { t } from "$lib/i18n";

	type Props = { data?: { alertKey?: string }; key?: string };

	let props: Props = $props();
	let key = $derived(props.key ?? props.data?.alertKey);
</script>

{#if key}
	<div class="alert alert-light">
		{$t(key)}
	</div>
{/if}
