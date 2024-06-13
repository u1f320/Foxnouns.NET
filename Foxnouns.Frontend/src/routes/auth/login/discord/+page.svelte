<script lang="ts">
	import { onMount } from "svelte";
	import { goto } from "$app/navigation";
	import { enhance } from "$app/forms";
	import type { PageData, ActionData } from "./$types";
	export let data: PageData;

	export let form: ActionData;

	onMount(async () => {
		if (data.user) {
			await new Promise((r) => setTimeout(r, 3000));
			await goto(`/@${data.user.username}`);
		}
	});

	const redirectOnForm = async (action: ActionData) => {
		if (form?.user) {
			await new Promise((r) => setTimeout(r, 3000));
			await goto(`/@${form.user.username}`);
		}
	};

	$: redirectOnForm(form);
</script>

<div class="container">
	{#if form?.user}
		<h1 class="title">Successfully created account!</h1>
		<p>Welcome, <strong>@{form.user.username}</strong>!</p>
		<p>
			You should automatically be redirected to your profile in a few seconds. If you're not
			redirected, please press the link above.
		</p>
	{:else if data.loggedIn}
		<h1 class="title">Successfully logged in!</h1>
		<p>You are now logged in as <a href="/@{data.user?.username}">@{data.user?.username}</a>.</p>
		<p>
			You should automatically be redirected to your profile in a few seconds. If you're not
			redirected, please press the link above.
		</p>
	{:else}
		<h1 class="title">Finish signing up with a Discord account</h1>
		<form method="POST" action="?/register" use:enhance>
			<div class="field">
				<label for="remote_username" class="label">Discord username</label>
				<div class="control">
					<input
						type="text"
						name="remote_username"
						id="remote_username"
						class="input"
						value={data.remoteUsername}
						disabled
					/>
				</div>
			</div>
			<div class="field">
				<label for="username" class="label">Username</label>
				<div class="control">
					<input type="text" name="username" id="username" class="input" placeholder="Username" />
				</div>
			</div>
			<input
				type="text"
				name="ticket"
				id="ticket"
				class="hidden"
				style="display: hidden;"
				value={data.ticket}
			/>
			<div class="field">
				<div class="control">
					<button class="button is-primary">Sign up</button>
				</div>
			</div>
		</form>
	{/if}
</div>
