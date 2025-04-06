<script lang="ts">
	import type { PageData } from "./$types";
	import Notification from "$components/settings/Notification.svelte";
	import UrlAlert from "$components/URLAlert.svelte";
	import { t } from "$lib/i18n";

	type Props = { data: PageData };
	let { data }: Props = $props();
</script>

<UrlAlert {data} />

{#each data.notifications as notification (notification.id)}
	<Notification {notification} />
{:else}
	{$t("notification.no-notifications")}
{/each}
