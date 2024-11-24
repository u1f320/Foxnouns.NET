<script lang="ts">
	import type { User, Member } from "$api/models";
	import { t } from "$lib/i18n";
	import { renderMarkdown } from "$lib/markdown";
	import ProfileLink from "./ProfileLink.svelte";
	import ProfileFlag from "./ProfileFlag.svelte";
	import Avatar from "$components/Avatar.svelte";

	type Props = {
		name: string;
		profile: User | Member;
		lazyLoadAvatar?: boolean;
	};

	let { name, profile, lazyLoadAvatar }: Props = $props();

	// renderMarkdown sanitizes the output HTML for us
	let bio = $derived(renderMarkdown(profile.bio));
</script>

<div class="grid row-gap-3">
	<div class="row">
		<div class="col-md-4 text-center">
			<Avatar
				url={profile.avatar_url}
				alt={$t("avatar-tooltip", { name })}
				lazyLoad={lazyLoadAvatar}
			/>
			<!-- Flags show up below the avatar if the profile has a bio, otherwise they show up below the row entirely -->
			{#if profile.flags && profile.bio}
				<div class="d-flex flex-wrap m-4">
					{#each profile.flags as flag}
						<ProfileFlag {flag} />
					{/each}
				</div>
			{/if}
		</div>
		<div class="col-md">
			{#if profile.display_name}
				<div>
					<h2>{profile.display_name}</h2>
					<p class="fs-5 text-body-secondary">{name}</p>
				</div>
			{:else}
				<h2>{name}</h2>
			{/if}
			{#if bio}
				<hr />
				<p>{@html bio}</p>
			{/if}
		</div>
		{#if profile.links.length > 0}
			<div class="col-md d-flex align-items-center">
				<ul class="list-unstyled">
					{#each profile.links as link}
						<ProfileLink {link} />
					{/each}
				</ul>
			</div>
		{/if}
	</div>
</div>
{#if profile.flags && !profile.bio}
	<div class="d-flex flex-wrap m-4">
		{#each profile.flags as flag}
			<ProfileFlag {flag} />
		{/each}
	</div>
{/if}
