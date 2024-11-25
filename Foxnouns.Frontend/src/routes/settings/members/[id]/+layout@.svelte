<script lang="ts">
	import type { Snippet } from "svelte";
	import { page } from "$app/stores";
	import { t } from "$lib/i18n";
	import type { LayoutData } from "./$types";

	type Props = { data: LayoutData; children: Snippet };
	let { data, children }: Props = $props();

	const isActive = (path: string) => $page.url.pathname === path;
</script>

<svelte:head>
	<title>{$t("edit-profile.member-header", { name: data.member.name })} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<h3>{$t("edit-profile.member-header", { name: data.member.name })}</h3>
	<div class="row">
		<div class="col-md-3 mt-1 mb-3">
			<div class="list-group">
				<a
					href="/settings/members/{data.member.id}"
					class="list-group-item list-group-item-action"
					class:active={isActive(`/settings/members/${data.member.id}`)}
				>
					{$t("edit-profile.general-tab")}
				</a>
				<a
					href="/settings/members/{data.member.id}/names-pronouns"
					class="list-group-item list-group-item-action"
					class:active={isActive(`/settings/members/${data.member.id}/names-pronouns`)}
				>
					{$t("edit-profile.names-pronouns-tab")}
				</a>
				<a
					href="/settings/members/{data.member.id}/fields"
					class="list-group-item list-group-item-action"
					class:active={isActive(`/settings/members/${data.member.id}/fields`)}
				>
					{$t("edit-profile.fields-tab")}
				</a>
				<a
					href="/settings/members/{data.member.id}/flags-links"
					class="list-group-item list-group-item-action"
					class:active={isActive(`/settings/members/${data.member.id}/flags-links`)}
				>
					{$t("edit-profile.flags-links-tab")}
				</a>
				<a
					href="/@{data.user.username}/{data.member.name}"
					class="list-group-item list-group-item-action text-danger"
				>
					Back to member
				</a>
				<a href="/settings/members" class="list-group-item list-group-item-action text-danger">
					{$t("edit-profile.back-to-settings-tab")}
				</a>
			</div>
		</div>
		<div class="col-md-9">
			{@render children?.()}
		</div>
	</div>
</div>
