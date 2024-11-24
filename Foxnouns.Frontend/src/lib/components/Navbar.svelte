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
	import { page } from "$app/stores";
	import type { User, Meta } from "$api/models/index";
	import Logo from "$components/Logo.svelte";
	import { t } from "$lib/i18n";

	type Props = { user: User | null; meta: Meta };
	let { user, meta }: Props = $props();

	let isOpen = $state(true);
	const toggleMenu = () => (isOpen = !isOpen);
</script>

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
						active={$page.url.pathname.startsWith(`/@${user.username}`)}
					>
						@{user.username}
					</NavLink>
				</NavItem>
				<NavItem>
					<NavLink href="/settings" active={$page.url.pathname.startsWith("/settings")}>
						{$t("nav.settings")}
					</NavLink>
				</NavItem>
			{:else}
				<NavItem>
					<NavLink href="/auth/log-in" active={$page.url.pathname === "/auth/log-in"}>
						{$t("nav.log-in")}
					</NavLink>
				</NavItem>
			{/if}
		</Nav>
	</Collapse>
</Navbar>

<style>
	/* These exact values make it look almost identical to the SVG version, which is what we want */
	#beta-text {
		font-size: 0.7em;
		position: relative;
		font-style: italic;
		bottom: 12px;
		right: 3px;
	}
</style>
