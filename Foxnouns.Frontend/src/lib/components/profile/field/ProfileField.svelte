<script lang="ts">
	import type { CustomPreference, FieldEntry, Pronoun } from "$api/models";
	import ProfileFieldEntry from "./ProfileFieldEntry.svelte";
	import PronounLink from "./PronounLink.svelte";

	type Props = {
		name: string;
		entries: Array<FieldEntry | Pronoun>;
		allPreferences: Record<string, CustomPreference>;
		isCol?: boolean;
	};
	let { name, entries, allPreferences, isCol }: Props = $props();
</script>

<div class:col={isCol === false}>
	<h3>{name}</h3>
	<ul class="list-unstyled fs-5">
		{#each entries as entry}
			<li>
				<ProfileFieldEntry status={entry.status} {allPreferences}>
					{#if "display_text" in entry}
						<PronounLink pronouns={entry} />
					{:else}
						{entry.value}
					{/if}
				</ProfileFieldEntry>
			</li>
		{/each}
	</ul>
</div>
