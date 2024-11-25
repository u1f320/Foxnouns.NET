<script lang="ts">
	import type { PageData } from "./$types";
	import ProfileHeader from "$components/profile/ProfileHeader.svelte";
	import OwnProfileNotice from "$components/profile/OwnProfileNotice.svelte";
	import { mergePreferences } from "$api/models";
	import ProfileFields from "$components/profile/ProfileFields.svelte";
	import { Icon } from "@sveltestrap/sveltestrap";
	import { t } from "$lib/i18n";

	type Props = { data: PageData };
	let { data }: Props = $props();

	let allPreferences = $derived(mergePreferences(data.member.user.custom_preferences));
	let isMeUser = $derived(data.meUser && data.meUser.id === data.member.user.id);
</script>

<svelte:head>
	<title>{data.member.display_name} • @{data.member.user.username} • pronouns.cc</title>
</svelte:head>

<div class="container">
	{#if isMeUser}
		<OwnProfileNotice
			memberName={data.member.display_name}
			editLink="/settings/members/{data.member.id}"
		/>
	{/if}

	<div class="m-3">
		<a class="btn btn-secondary" href="/@{data.member.user.username}">
			<Icon name="arrow-left" aria-hidden />
			{$t("profile.back-to-user", {
				name: data.member.user.display_name ?? data.member.user.username,
			})}
		</a>
	</div>

	<ProfileHeader name="{data.member.name} (@{data.member.user.username})" profile={data.member} />
	<ProfileFields profile={data.member} {allPreferences} />
</div>
