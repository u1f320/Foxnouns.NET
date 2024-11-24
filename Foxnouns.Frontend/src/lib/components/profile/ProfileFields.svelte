<script lang="ts">
	import type { CustomPreference, Member, User } from "$api/models";
	import ProfileField from "./field/ProfileField.svelte";
	import { t } from "$lib/i18n";

	type Props = { profile: User | Member; allPreferences: Record<string, CustomPreference> };
	let { profile, allPreferences }: Props = $props();
</script>

<div class="row row-cols-1 row-cols-sm-2 row-cols-md-3">
	{#if profile.names.length > 0}
		<ProfileField name={$t("profile.names-header")} entries={profile.names} {allPreferences} />
	{/if}
	{#if profile.pronouns.length > 0}
		<ProfileField
			name={$t("profile.pronouns-header")}
			entries={profile.pronouns}
			{allPreferences}
		/>
	{/if}
	{#each profile.fields as field}
		{#if field.entries.length > 0}
			<ProfileField name={field.name} entries={field.entries} {allPreferences} />
		{/if}
	{/each}
</div>
