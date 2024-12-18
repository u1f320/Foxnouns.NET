<script lang="ts">
	import { type User, type Member, type CustomPreference } from "$api/models";
	import StatusIcon from "$components/StatusIcon.svelte";

	type Props = { profile: User | Member; allPreferences: Record<string, CustomPreference> };
	let { profile, allPreferences }: Props = $props();

	let preferences = $derived.by(() => {
		let preferenceKeys = Object.keys(allPreferences).filter(
			(pref) =>
				profile.names.some((entry) => entry.status === pref) ||
				profile.pronouns.some((entry) => entry.status === pref) ||
				profile.fields.some((field) => field.entries.some((entry) => entry.status === pref)),
		);

		return preferenceKeys.map((pref) => allPreferences[pref]);
	});
</script>

<div class="text-center text-body-secondary">
	<ul class="list-inline">
		{#each preferences as preference}
			<li class="list-inline-item mx-2">
				<StatusIcon {preference} />
				{preference.tooltip}
			</li>
		{/each}
	</ul>
</div>
