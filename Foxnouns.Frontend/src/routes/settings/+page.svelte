<script lang="ts">
	import type { ActionData, PageData } from "./$types";
	import { t } from "$lib/i18n";
	import { Button, FormGroup, Icon, Input, InputGroup } from "@sveltestrap/sveltestrap";
	import Avatar from "$components/Avatar.svelte";
	import { firstErrorFor } from "$api/error";
	import Error from "$components/Error.svelte";
	import { idTimestamp } from "$lib";
	import { DateTime } from "luxon";
	import { enhance } from "$app/forms";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

	let usernameError = $derived(form?.error ? firstErrorFor(form.error, "username") : undefined);
	let createdAt = $derived(idTimestamp(data.user.id));
</script>

<h3>{$t("settings.general-information-tab")}</h3>

<div class="row mb-3">
	<div class="col-md-9">
		<h5>Change your username</h5>
		<form method="POST" action="?/changeUsername" use:enhance>
			<FormGroup class="mb-3">
				<InputGroup class="m-1 mt-3 w-md-75">
					<Input
						type="text"
						value={data.user.username}
						name="username"
						required
						autocomplete="off"
					/>
					<Button type="submit" color="secondary">{$t("settings.change-username-button")}</Button>
				</InputGroup>
			</FormGroup>
			{#if form?.ok}
				<p class="text-success-emphasis">
					<Icon name="check-circle-fill" />
					{$t("settings.username-update-success")}
				</p>
			{:else if usernameError}
				<p class="text-danger-emphasis text-has-newline">
					<Icon name="exclamation-triangle-fill" />
					{$t("settings.username-update-error", { message: usernameError.message })}
				</p>
			{:else if form?.error}
				<Error showHeader={false} error={form?.error} />
			{/if}
		</form>
		<p class="text-muted text-has-newline">
			<Icon name="info-circle-fill" aria-hidden />
			{$t("settings.username-change-hint")}
		</p>
	</div>
	<div class="col-md-3 text-center">
		<h5>{$t("settings.avatar")}</h5>
		<Avatar
			name={data.user.username}
			url={data.user.avatar_url}
			alt={$t("avatar-tooltip", { name: "@" + data.user.username })}
		/>
		<p class="mt-2">
			<a href="/settings/profile">{$t("settings.change-avatar-link")}</a>
		</p>
	</div>
</div>

<div class="mb-3">
	<h4>{$t("settings.log-out-title")}</h4>
	<p>{$t("settings.log-out-hint")}</p>
	<form method="POST" action="?/logout">
		<Button color="secondary" type="submit">{$t("settings.log-out-button")}</Button>
	</form>
</div>

<div class="mb-3">
	<h4>{$t("settings.force-log-out-title")}</h4>
	<p>{$t("settings.force-log-out-hint")}</p>
	<a class="btn btn-danger" href="/settings/force-log-out">{$t("settings.force-log-out-button")}</a>
</div>

<div>
	<h4>{$t("settings.table-title")}</h4>

	<table class="table table-striped table-hover table-bordered">
		<tbody>
			<tr>
				<th scope="row">{$t("settings.table-id")}</th>
				<td>
					<code>{data.user.id}</code>
				</td>
			</tr>
			<tr>
				<th scope="row">{$t("settings.table-created-at")}</th>
				<td>{createdAt.toLocaleString(DateTime.DATETIME_MED)}</td>
			</tr>
			<tr>
				<th scope="row">{$t("settings.table-member-count")}</th>
				<td>
					{data.user.members.length}/{data.meta.limits.member_count}
				</td>
			</tr>
			<tr>
				<th scope="row">{$t("settings.table-member-list-hidden")}</th>
				<td>{data.user.member_list_hidden ? $t("yes") : $t("no")}</td>
			</tr>
			<tr>
				<th scope="row">{$t("settings.table-custom-preferences")}</th>
				<td>
					{Object.keys(data.user.custom_preferences).length}/{data.meta.limits.custom_preferences}
				</td>
			</tr>
			<tr>
				<th scope="row">{$t("settings.table-role")}</th>
				<td>
					<code>{data.user.role}</code>
				</td>
			</tr>
		</tbody>
	</table>
</div>
