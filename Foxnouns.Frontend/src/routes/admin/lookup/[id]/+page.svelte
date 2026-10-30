<script lang="ts">
	import { idTimestamp } from "$lib";
	import { DateTime } from "luxon";
	import type { ActionData, PageData } from "./$types";
	import ProfileHeader from "$components/profile/ProfileHeader.svelte";
	import ProfileFields from "$components/profile/ProfileFields.svelte";
	import { mergePreferences } from "$api/models";
	import AuthMethodRow from "$components/settings/AuthMethodRow.svelte";
	import { enhance } from "$app/forms";
	import ActionForm from "$components/admin/ActionForm.svelte";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

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

<div class="row">
	<h3>Take action on user</h3>
	<ActionForm userId={data.user.user.id} {form} />
</div>

{#if !data.user.show_sensitive_data}
	<h2>Query sensitive data</h2>
	<p>Only use this if required. Requests will be logged to the audit log.</p>
	<div class="list-group">
		<form method="POST" action="?/requestData" use:enhance>
			<div class="form-field mb-3">
				<textarea
					name="reason"
					class="form-control"
					style="height: 200px;"
					required
					placeholder="Reason"
				></textarea>
			</div>
			<div class="form-field">
				<button type="submit" class="btn btn-secondary"> Query sensitive data </button>
			</div>
		</form>
	</div>
{/if}

{#if authMethods}
	<h2>Authentication methods</h2>
	<div class="list-group">
		{#each authMethods as method (method.id)}
			<AuthMethodRow {method} canRemove={false} showType />
		{/each}
	</div>
{/if}
