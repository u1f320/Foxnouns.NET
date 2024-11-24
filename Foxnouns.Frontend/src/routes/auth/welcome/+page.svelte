<script lang="ts">
	import { t } from "$lib/i18n";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();
	let user = $derived(data.meUser!);
</script>

<svelte:head>
	<title>{$t("title.welcome")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<h2>Welcome to pronouns.cc!</h2>

	<div class="alert alert-secondary">
		<a href="/@{user.username}">If you just want to go to your profile, click here</a>
	</div>

	<h3>Customize your profile</h3>
	<!-- TODO: this needs actual content, obviously -->
	<p>(todo)</p>

	<h3>Create members</h3>
	<p>(todo)</p>

	<h3>Create custom preferences</h3>
	<p>(todo)</p>

	<p>
		<a class="btn btn-primary btn-lg" href="/@{user.username}">Check out your profile</a>
	</p>
</div>
