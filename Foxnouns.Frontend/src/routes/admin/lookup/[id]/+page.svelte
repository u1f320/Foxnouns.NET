<script lang="ts">
	import { idTimestamp } from "$lib";
	import { DateTime } from "luxon";
	import type { PageData } from "./$types";
	import ProfileHeader from "$components/profile/ProfileHeader.svelte";
	import ProfileFields from "$components/profile/ProfileFields.svelte";
	import { mergePreferences } from "$api/models";
	import AuthMethodRow from "$components/settings/AuthMethodRow.svelte";

	type Props = { data: PageData };
	let { data }: Props = $props();

	let createdAt = $derived(idTimestamp(data.user.user.id));
	let lastActive = $derived(DateTime.fromISO(data.user.last_active));
	let lastSidReroll = $derived(DateTime.fromISO(data.user.last_sid_reroll));

	let authMethods = $derived.by(() => {
		if (!data.user.auth_methods) return undefined;

		return data.user.auth_methods.sort((a, b) => a.type.localeCompare(b.type));
	});
</script>

<svelte:head>
	<title>Looking up @{data.user.user.username} • pronouns.cc</title>
</svelte:head>

<h2>Basic profile</h2>

<ProfileHeader name="@{data.user.user.username}" profile={data.user.user} />

<ProfileFields
	profile={data.user.user}
	allPreferences={mergePreferences(data.user.user.custom_preferences)}
/>

<h2>Extra information</h2>

<table class="table table-striped table-hover table-bordered">
	<tbody>
		<tr>
			<th scope="row">Created at</th>
			<td>{createdAt.toLocaleString(DateTime.DATETIME_MED)}</td>
		</tr>
		<tr>
			<th scope="row">Last active</th>
			<td>{lastActive.toLocaleString(DateTime.DATETIME_MED)}</td>
		</tr>
		<tr>
			<th scope="row">Last SID reroll</th>
			<td>{lastSidReroll.toLocaleString(DateTime.DATETIME_MED)}</td>
		</tr>
	</tbody>
</table>

{#if authMethods}
	<h2>Authentication methods</h2>
	<div class="list-group">
		{#each authMethods as method (method.id)}
			<AuthMethodRow {method} canRemove={false} showType />
		{/each}
	</div>
{/if}
