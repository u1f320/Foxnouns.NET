<script lang="ts">
	import type { Snippet } from "svelte";
	import type { LayoutData } from "./$types";
	import { isActive } from "$lib/pageUtils.svelte";

	type Props = { data: LayoutData; children: Snippet };
	let { data, children }: Props = $props();
</script>

<div class="container">
	<div class="row">
		<div class="col-md-3 mt-1 mb-3">
			<div class="list-group">
				<a
					href="/admin"
					class="list-group-item list-group-item-action"
					class:active={isActive("/admin")}
				>
					Dashboard
				</a>
				<a
					href="/admin/reports"
					class="list-group-item list-group-item-action"
					class:active={isActive("/admin/reports", true)}
				>
					Reports
					{#if data.reportCount}
						<span
							class="badge"
							class:text-bg-danger={data.reportCount >= 10}
							class:text-bg-secondary={data.reportCount < 10}
						>
							{data.reportCount >= 100 ? "99+" : data.reportCount.toString()}
						</span>
					{/if}
				</a>
				<a
					href="/admin/audit-log"
					class="list-group-item list-group-item-action"
					class:active={isActive("/admin/audit-log", true)}
				>
					Audit log
				</a>
				<a
					href="/admin/lookup"
					class="list-group-item list-group-item-action"
					class:active={isActive("/admin/lookup", true)}
				>
					Lookup
				</a>
			</div>
		</div>
		<div class="col-md-9">
			{@render children?.()}
		</div>
	</div>
</div>
