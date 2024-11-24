<script lang="ts">
	import type { Snippet } from "svelte";
	import { page } from "$app/stores";
	import { t } from "$lib/i18n";
	import { Nav, NavLink } from "@sveltestrap/sveltestrap";

	type Props = { children: Snippet };
	let { children }: Props = $props();

	const isActive = (path: string | string[], prefix: boolean = false) =>
		typeof path === "string"
			? prefix
				? $page.url.pathname.startsWith(path)
				: $page.url.pathname === path
			: prefix
				? path.some((p) => $page.url.pathname.startsWith(p))
				: path.some((p) => $page.url.pathname === p);
</script>

<svelte:head>
	<title>{$t("title.settings")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<Nav pills justified fill class="flex-column flex-md-row mb-2">
		<NavLink active={isActive(["/settings", "/settings/force-log-out"])} href="/settings">
			{$t("settings.general-information-tab")}
		</NavLink>
		<NavLink active={isActive("/settings/profile", true)} href="/settings/profile">
			{$t("settings.your-profile-tab")}
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
