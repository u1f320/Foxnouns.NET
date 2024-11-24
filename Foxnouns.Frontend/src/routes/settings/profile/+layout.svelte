<script lang="ts">
	import type { Snippet } from "svelte";
	import { page } from "$app/stores";
	import { t } from "$lib/i18n";

	type Props = { children: Snippet };
	let { children }: Props = $props();

	const isActive = (path: string) => $page.url.pathname === path;
</script>

<h3>{$t("edit-profile.user-header")}</h3>
<div class="row">
	<div class="col-md-3 mt-1 mb-3">
		<div class="list-group">
			<a
				href="/settings/profile"
				class="list-group-item list-group-item-action"
				class:active={isActive("/settings/profile")}
			>
				{$t("edit-profile.general-tab")}
			</a>
			<a
				href="/settings/profile/names-pronouns"
				class="list-group-item list-group-item-action"
				class:active={isActive("/settings/profile/names-pronouns")}
			>
				{$t("edit-profile.names-pronouns-tab")}
			</a>
			<a
				href="/settings/profile/bio"
				class="list-group-item list-group-item-action"
				class:active={isActive("/settings/profile/bio")}
			>
				{$t("edit-profile.bio-tab")}
			</a>
		</div>
	</div>
	<div class="col-md-9">
		{@render children?.()}
	</div>
</div>
