<script lang="ts">
	import { defaultPreferences, PreferenceSize, type CustomPreference } from "$api/models";
	import StatusIcon from "$components/StatusIcon.svelte";
	import type { Snippet } from "svelte";

	type Props = {
		status: string;
		allPreferences: Record<string, CustomPreference>;
		children: Snippet;
	};
	let { status, allPreferences, children }: Props = $props();

	let preference = $derived(
		status in allPreferences ? allPreferences[status] : defaultPreferences.missing,
	);

	let elemType = $derived(preference.size === PreferenceSize.Large ? "strong" : "span");
</script>

<svelte:element
	this={elemType}
	class:text-muted={preference.muted}
	class:fs-5={preference.size === PreferenceSize.Large}
	class:fs-6={preference.size === PreferenceSize.Small}
>
	<StatusIcon {preference} />
	{@render children?.()}
</svelte:element>
