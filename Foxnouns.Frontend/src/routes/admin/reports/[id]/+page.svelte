<script lang="ts">
	import type { Member } from "$api/models/member";
	import type { User } from "$api/models/user";
	import ActionForm from "$components/admin/ActionForm.svelte";
	import ClosedReportAuditLog from "$components/admin/ClosedReportAuditLog.svelte";
	import PartialProfileCard from "$components/admin/PartialProfileCard.svelte";
	import ProfileHeader from "$components/profile/ProfileHeader.svelte";
	import MemberCard from "$components/profile/user/MemberCard.svelte";
	import { renderMarkdown } from "$lib/markdown";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
	let { report, user, auditLogEntry } = $derived(data);
</script>

<svelte:head>
	<title>Report on @{user.username} • pronouns.cc</title>
</svelte:head>

{#if report.status === "CLOSED"}
	<div class="alert alert-secondary">
		This report has already been handled. <a href="#report-status" class="alert-link"
			>See audit log entry</a
		>
	</div>
{/if}

<div class="row">
	<div class="col-md">
		<h3>Target user</h3>
		<PartialProfileCard user={report.target_user} />
	</div>
	{#if report.target_member}
		<div class="col-md">
			<h3>Target member</h3>
			<MemberCard
				username={report.target_user.username}
				member={report.target_member}
				allPreferences={{}}
			/>
		</div>
	{/if}
	<div class="col-md">
		<h3>Reporter</h3>
		<PartialProfileCard user={report.reporter} />
	</div>
</div>

<div class="row">
	<div class="col-md">
		<h3>Reason</h3>
		<p><code>{report.reason}</code></p>
	</div>
	<div class="col-md">
		<h3>Context</h3>
		<p>
			{#if report.context}
				{@html renderMarkdown(report.context)}
			{:else}
				<em>(no context given)</em>
			{/if}
		</p>
	</div>
</div>

{#if report.status === "OPEN"}
	<div class="row">
		<h3>Take action</h3>
		<ActionForm
			userId={report.target_user.id}
			reportId={report.id}
			memberId={report.target_member?.id}
			{form}
		/>
	</div>
{:else if report.status === "CLOSED" && auditLogEntry}
	<ClosedReportAuditLog entry={auditLogEntry} />
{:else}
	<div class="row">
		<h3>Closed by an unknown moderator</h3>
		<p>
			<em>This should not happen!</em>
		</p>
	</div>
{/if}

{#if report.snapshot}
	<h3>Profile at time of report</h3>
	<hr />
	{#if report.target_type === "USER"}
		{@const snapshot = report.snapshot as User}
		<ProfileHeader profile={snapshot} name="@{snapshot.username}" />
	{:else}
		{@const snapshot = report.snapshot as Member}
		<ProfileHeader profile={snapshot} name="{snapshot.name} (@{snapshot.user.username})" />
	{/if}
{/if}
