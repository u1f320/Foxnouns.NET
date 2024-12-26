<script lang="ts">
	import type { AuditLogEntity } from "$api/models/moderation";
	import AuditLogEntryCard from "$components/admin/AuditLogEntryCard.svelte";
	import {
		ButtonDropdown,
		DropdownItem,
		DropdownMenu,
		DropdownToggle,
	} from "@sveltestrap/sveltestrap";
	import type { PageData } from "./$types";

	type Props = { data: PageData };
	let { data }: Props = $props();

	const addTypeFilter = (type: string | null) => {
		const url = new URL(data.url);
		if (type) url.searchParams.set("type", type);
		else url.searchParams.delete("type");

		return url.toString();
	};

	const addModerator = (mod: AuditLogEntity | null) => {
		const url = new URL(data.url);
		if (mod) url.searchParams.set("by-moderator", mod.id);
		else url.searchParams.delete("by-moderator");

		return url.toString();
	};

	const addBefore = (id: string) => {
		const url = new URL(data.url);
		url.searchParams.delete("after");
		url.searchParams.set("before", id);

		return url.toString();
	};

	const addAfter = (id: string) => {
		const url = new URL(data.url);
		url.searchParams.delete("before");
		url.searchParams.set("after", id);

		return url.toString();
	};
</script>

<h1>Audit log</h1>

<div class="btn-group">
	<ButtonDropdown>
		<DropdownToggle color="secondary" outline caret active={!!data.type}>
			Filter by type
		</DropdownToggle>
		<DropdownMenu>
			<DropdownItem href={addTypeFilter("IgnoreReport")} active={data.type === "IgnoreReport"}>
				Ignore report
			</DropdownItem>
			<DropdownItem href={addTypeFilter("WarnUser")} active={data.type === "WarnUser"}>
				Warn user
			</DropdownItem>
			<DropdownItem
				href={addTypeFilter("WarnUserAndClearProfile")}
				active={data.type === "WarnUserAndClearProfile"}
			>
				Warn user and clear profile
			</DropdownItem>
			<DropdownItem href={addTypeFilter("SuspendUser")} active={data.type === "SuspendUser"}>
				Suspend user
			</DropdownItem>
			{#if data.type}
				<DropdownItem href={addTypeFilter(null)}>Remove filter</DropdownItem>
			{/if}
		</DropdownMenu>
	</ButtonDropdown>
	<ButtonDropdown>
		<DropdownToggle color="secondary" outline caret active={!!data.modFilter}>
			Filter by moderator
		</DropdownToggle>
		<DropdownMenu>
			{#each data.moderators as mod (mod.id)}
				<DropdownItem href={addModerator(mod)} active={data.modFilter?.id === mod.id}>
					{mod.username}
				</DropdownItem>
			{/each}
			{#if data.modFilter}
				<DropdownItem href={addModerator(null)}>Remove filter</DropdownItem>
			{/if}
		</DropdownMenu>
	</ButtonDropdown>
</div>

{#if data.before}
	<a href={addAfter(data.before)}>Show newer entries</a>
{/if}

{#each data.entries as entry (entry.id)}
	<AuditLogEntryCard {entry} />
{:else}
	<p class="text-secondary m-3">There are no entries matching your filter</p>
{/each}

{#if data.entries.length === 100}
	<a href={addBefore(data.entries[data.entries.length - 1].id)}>Show older entries</a>
{/if}
