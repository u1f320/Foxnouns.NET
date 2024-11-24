<script lang="ts">
	import type { PageData } from "./$types";
	import ProfileHeader from "$components/profile/ProfileHeader.svelte";
	import OwnProfileNotice from "$components/profile/OwnProfileNotice.svelte";
	import { mergePreferences } from "$api/models";
	import ProfileFields from "$components/profile/ProfileFields.svelte";
	import { t } from "$lib/i18n";
	import { Icon } from "@sveltestrap/sveltestrap";
	import Paginator from "./Paginator.svelte";
	import MemberCard from "$components/profile/user/MemberCard.svelte";

	type Props = { data: PageData };
	let { data }: Props = $props();

	let allPreferences = $derived(mergePreferences(data.user.custom_preferences));
	let isMeUser = $derived(data.meUser && data.meUser.id === data.user.id);
</script>

<svelte:head>
	<title>@{data.user.username} • pronouns.cc</title>
</svelte:head>

<div class="container">
	{#if isMeUser}
		<OwnProfileNotice editLink="/settings/profile" />
	{/if}

	<ProfileHeader name="@{data.user.username}" profile={data.user} lazyLoadAvatar={true} />
	<ProfileFields profile={data.user} {allPreferences} />

	{#if data.members.length > 0}
		<hr />
		<h2>
			{data.user.member_title || $t("profile.default-members-header")}
			{#if isMeUser}
				<a class="btn btn-success" href="/settings/create-member">
					<Icon name="person-plus-fill" aria-hidden={true} />
					{$t("profile.create-member-button")}
				</a>
			{/if}
			<Paginator
				currentPage={data.currentPage}
				pageCount={data.pageCount}
				href="/@{data.user.username}"
			/>
		</h2>
		<div class="row row-cols-2 row-cols-md-3 row-cols-lg-4 row-cols-xl-5 text-center">
			{#each data.members as member (member.id)}
				<MemberCard username={data.user.username} {member} {allPreferences} />
			{/each}
		</div>
		<div class="text-center">
			<Paginator
				currentPage={data.currentPage}
				pageCount={data.pageCount}
				href="/@{data.user.username}"
			/>
		</div>
	{/if}
</div>
