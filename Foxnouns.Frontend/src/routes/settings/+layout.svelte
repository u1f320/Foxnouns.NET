<script lang="ts">
	import type { Snippet } from "svelte";
	import { t } from "$lib/i18n";
	import { Nav, NavLink } from "@sveltestrap/sveltestrap";
	import { isActive } from "$lib/pageUtils.svelte";
	import type { LayoutData } from "./$types";
	import GlobalNotice from "$components/GlobalNotice.svelte";

	type Props = { data: LayoutData; children: Snippet };
	let { data, children }: Props = $props();
</script>

<svelte:head>
	<title>{$t("title.settings")} • pronouns.cc</title>
</svelte:head>

{#if data.meta.notice}
	<div class="container">
		<GlobalNotice
			id={data.meta.notice.id}
			message={data.meta.notice.message}
			settings={data.meUser?.settings}
			token={data.token}
		/>
	</div>
{/if}

<div class="container">
	<Nav pills justified fill class="flex-column flex-md-row mb-2">
		<NavLink active={isActive(["/settings", "/settings/force-log-out"])} href="/settings">
			{$t("settings.general-information-tab")}
		</NavLink>
		<NavLink active={isActive("/settings/profile", true)} href="/settings/profile">
			{$t("settings.your-profile-tab")}
		</NavLink>
		<NavLink active={isActive("/settings/flags")} href="/settings/flags">
			{$t("settings.flag-title")}
		</NavLink>
		<NavLink active={isActive("/settings/prefs")} href="/settings/prefs">
			{$t("settings.custom-preferences-title")}
		</NavLink>
		<NavLink active={isActive("/settings/members", true)} href="/settings/members">
			{$t("settings.members-tab")}
		</NavLink>
		<NavLink active={isActive("/settings/auth", true)} href="/settings/auth">
			{$t("settings.authentication-tab")}
		</NavLink>
		<NavLink active={isActive("/settings/export")} href="/settings/export">
			{$t("settings.export-tab")}
		</NavLink>
	</Nav>

	{@render children?.()}
</div>
