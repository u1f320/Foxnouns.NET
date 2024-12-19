<script lang="ts">
	import {
		Navbar,
		NavbarBrand,
		NavbarToggler,
		Collapse,
		Nav,
		NavLink,
		NavItem,
	} from "@sveltestrap/sveltestrap";
	import { page } from "$app/state";
	import type { Meta, MeUser } from "$api/models/index";
	import Logo from "$components/Logo.svelte";
	import { t } from "$lib/i18n";

	type Props = { user: MeUser | null; meta: Meta };
	let { user, meta }: Props = $props();

	let isOpen = $state(true);
	const toggleMenu = () => (isOpen = !isOpen);
</script>

{#if user && user.deleted}
	<div class="deleted-alert text-center py-3 mb-2 px-2">
		{#if user.suspended}
			<strong>{$t("nav.suspended-account-hint")}</strong>
			<br />
			<a href="/settings">{$t("nav.delete-permanently-link")}</a> •
			<a href="/contact">{$t("nav.appeal-suspension-link")}</a> •
			<a href="/settings/export">{$t("nav.export-link")}</a>
		{:else}
			<strong>{$t("nav.deleted-account-hint")}</strong>
			<br />
			<a href="/settings">{$t("nav.reactivate-or-delete-link")}</a> •
			<a href="/settings/export">{$t("nav.export-link")}</a>
		{/if}
	</div>
{/if}

<Navbar expand="lg" class="mb-4 mx-2">
	<NavbarBrand href="/">
		<Logo />
		{#if meta.version.endsWith(".dirty")}
			<strong id="beta-text" class="text-danger">dev</strong>
		{:else}
			<span id="beta-text">beta</span>
		{/if}
	</NavbarBrand>
	<NavbarToggler onclick={toggleMenu} aria-label="Toggle menu" />
	<Collapse {isOpen} navbar expand="lg">
		<Nav navbar class="ms-auto">
			{#if user}
				<NavItem>
					<NavLink
						href="/@{user.username}"
						active={page.url.pathname.startsWith(`/@${user.username}`)}
					>
						@{user.username}
					</NavLink>
				</NavItem>
				<NavItem>
					<NavLink href="/settings" active={page.url.pathname.startsWith("/settings")}>
						{$t("nav.settings")}
					</NavLink>
				</NavItem>
			{:else}
				<NavItem>
					<NavLink href="/auth/log-in" active={page.url.pathname === "/auth/log-in"}>
						{$t("nav.log-in")}
					</NavLink>
				</NavItem>
			{/if}
		</Nav>
	</Collapse>
</Navbar>

<style>
	.deleted-alert {
		color: var(--bs-danger-text-emphasis);
		background-color: var(--bs-danger-bg-subtle);
	}

	/* These exact values make it look almost identical to the SVG version, which is what we want */
	#beta-text {
		font-size: 0.7em;
		position: relative;
		font-style: italic;
		bottom: 12px;
		right: 3px;
	}
</style>
