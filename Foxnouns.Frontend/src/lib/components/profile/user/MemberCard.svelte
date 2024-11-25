<script lang="ts">
	import type { CustomPreference, PartialMember } from "$api/models";
	import Avatar from "$components/Avatar.svelte";
	import { t } from "$lib/i18n";

	type Props = {
		username: string;
		member: PartialMember;
		allPreferences: Record<string, CustomPreference>;
	};
	let { username, member, allPreferences }: Props = $props();

	const getPronouns = (member: PartialMember) => {
		const filteredPronouns = member.pronouns.filter(
			(entry) => (allPreferences[entry.status] || { favourite: false }).favourite,
		);
		if (filteredPronouns.length === 0) {
			return undefined;
		}
		return filteredPronouns
			.map((pronouns) => {
				if (pronouns.display_text) {
					return pronouns.display_text;
				} else {
					const split = pronouns.value.split("/");
					if (split.length === 5) return split.splice(0, 2).join("/");
					return pronouns.value;
				}
			})
			.join(", ");
	};

	let pronouns = $derived(getPronouns(member));
</script>

<div>
	<a href="/@{username}/{member.name}">
		<Avatar
			url={member.avatar_url}
			lazyLoad
			alt={$t("avatar-tooltip", { name: member.display_name })}
		/>
	</a>
	<p class="m-2">
		<a class="text-reset fs-5 text-break" href="/@{username}/{member.name}">
			{member.display_name}
		</a>
		{#if pronouns}
			<br />
			{pronouns}
		{/if}
	</p>
</div>
