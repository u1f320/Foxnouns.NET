<script lang="ts">
	import type { PageData } from "./$types";
	import Notification from "$components/settings/Notification.svelte";
	import UrlAlert from "$components/URLAlert.svelte";

	type Props = { data: PageData };
	let { data }: Props = $props();
</script>

<UrlAlert {data} />

{#each data.notifications as notification (notification.id)}
	<Notification {notification} />
{:else}
	You have no notifications.
{/each}
