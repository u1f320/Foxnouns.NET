<script lang="ts">
	import { IconSun, IconMoon } from "@tabler/icons-svelte";
	import { onMount } from "svelte";
	import Dropdown from "./Dropdown.svelte";
	import DropdownItem from "./DropdownItem.svelte";
	import Logo from "./Logo.svelte";
	import type { User } from "$lib/api/user";
	import { themeStore } from "$lib/store";

	export let user: User | undefined;
	let navIsOpen = false;

	onMount(() => {
		let theme = localStorage.getItem("pronounscc-theme");
		if (!theme)
			theme = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
		themeStore.set(theme);
	});

	const toggleTheme = () => {
		themeStore.set($themeStore === "dark" ? "light" : "dark");
		document.documentElement.setAttribute("data-theme", $themeStore);
		localStorage.setItem("pronounscc-theme", $themeStore);
	};
</script>

<nav class="navbar" aria-label="Main navigation">
	<div class="navbar-brand">
		<a href="/" class="navbar-item">
			<Logo />
		</a>
		<button
			class={"navbar-burger" + (navIsOpen ? " is-active" : "")}
			aria-label="menu"
			aria-expanded="false"
			on:click={() => (navIsOpen = !navIsOpen)}
		>
			<span aria-hidden="true"></span>
			<span aria-hidden="true"></span>
			<span aria-hidden="true"></span>
			<span aria-hidden="true"></span>
		</button>
	</div>
	<div class={"navbar-menu" + (navIsOpen ? " is-active" : "")}>
		<div class="navbar-end">
			{#if user}
				<Dropdown>
					<span slot="label">@{user.username}</span>
					<DropdownItem href="/@{user.username}">View profile</DropdownItem>
					<DropdownItem href="/settings">Settings</DropdownItem>
					<DropdownItem divider />
					<DropdownItem href="/auth/logout">Log out</DropdownItem>
				</Dropdown>
			{:else}
				<a href="/auth/login" class="navbar-item">Log in or sign up</a>
			{/if}
			<button class="navbar-item" on:click={() => toggleTheme()}>
				{#if $themeStore === "dark"}
					<IconSun size={20} />&nbsp;Light theme
				{:else}
					<IconMoon size={20} />&nbsp;Dark theme
				{/if}
			</button>
		</div>
	</div>
</nav>
