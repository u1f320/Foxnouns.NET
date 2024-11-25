<script lang="ts">
	import Paginator from "$components/Paginator.svelte";
	import { Icon, ListGroup, ListGroupItem } from "@sveltestrap/sveltestrap";
	import { t } from "$lib/i18n";
	import type { PageData } from "./$types";
	import Avatar from "$components/Avatar.svelte";

	type Props = { data: PageData };
	let { data }: Props = $props();

	let canCreateMember = $derived(data.user.members.length < data.meta.limits.member_count);
</script>

<h3 class="mb-3">{$t("settings.members-tab")} ({data.user.members.length})</h3>

<Paginator
	center
	currentPage={data.currentPage}
	pageCount={data.pageCount}
	href="/settings/members"
/>

<ListGroup class="mb-3">
	{#if canCreateMember}
		<ListGroupItem tag="a" href="/settings/members/new">
			<Icon name="person-fill-add" aria-hidden />
			<strong>{$t("profile.create-member-button")}</strong>
		</ListGroupItem>
	{/if}
	{#each data.members as member (member.id)}
		<ListGroupItem tag="a" href="/settings/members/{member.id}" data-sveltekit-preload-data="tap">
			<Avatar
				url={member.avatar_url}
				alt={$t("avatar-tooltip", { name: member.display_name })}
				size={20}
			/>
			{member.display_name}
			{#if member.display_name !== member.name}({member.name}){/if}
		</ListGroupItem>
	{/each}
</ListGroup>

<Paginator
	center
	currentPage={data.currentPage}
	pageCount={data.pageCount}
	href="/settings/members"
/>
