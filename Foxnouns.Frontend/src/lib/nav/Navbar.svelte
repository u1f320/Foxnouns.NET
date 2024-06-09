<script lang="ts">
	import {
		Navbar,
		NavbarBrand,
		NavbarToggler,
		Collapse,
		Nav,
		Dropdown,
		DropdownToggle,
		DropdownMenu,
		DropdownItem,
		NavItem,
	} from "@sveltestrap/sveltestrap";
	import Logo from "./Logo.svelte";
	import type { User } from "$lib/api/user";

	export let user: User | undefined;

	let showMenu = true;
	const toggleMenu = () => {
		showMenu = !showMenu;
	};
	let userMenuOpen = false;
</script>

<Navbar>
	<NavbarBrand href="/" aria-label="pronouns.cc landing page">
		<Logo />
	</NavbarBrand>
	<NavbarToggler on:click={toggleMenu} aria-label="Toggle menu" />
	<Collapse isOpen={showMenu} navbar expand="lg">
		<Nav class="ms-auto" navbar>
			{#if user}
				<NavItem>
					<Dropdown nav isOpen={userMenuOpen} toggle={() => (userMenuOpen = !userMenuOpen)}>
						<DropdownToggle nav caret>@{user.username}</DropdownToggle>
						<DropdownMenu>
							<DropdownItem href="/@{user.username}">View profile</DropdownItem>
							<DropdownItem href="/settings">Settings</DropdownItem>
							<DropdownItem divider />
							<DropdownItem href="/logout">Log out</DropdownItem>
						</DropdownMenu>
					</Dropdown>
				</NavItem>
			{:else}
				<NavItem>
					<Dropdown nav isOpen={userMenuOpen} toggle={() => (userMenuOpen = !userMenuOpen)}>
						<DropdownToggle nav caret>Not logged in</DropdownToggle>
						<DropdownMenu>
							<DropdownItem href="/login">Log in</DropdownItem>
						</DropdownMenu>
					</Dropdown>
				</NavItem>
			{/if}
		</Nav>
	</Collapse>
</Navbar>
