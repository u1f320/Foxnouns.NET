<script lang="ts">
	import { t } from "$lib/i18n";
	import type { AuthMethod } from "$api/models";

	type Props = { method: AuthMethod; canRemove: boolean; showType?: boolean };
	let { method, canRemove, showType }: Props = $props();

	let name = $derived(
		method.type === "EMAIL" ? method.remote_id : (method.remote_username ?? method.remote_id),
	);
	let showId = $derived(method.type !== "EMAIL");
</script>

<div class="list-group-item">
	<div class="row">
		<div class="col">
			{#if showType}
				<code>{method.type}</code>:
			{/if}
			{name}
			{#if showId}({method.remote_id}){/if}
		</div>
		{#if canRemove}
			<div class="col text-end">
				<a href="/settings/auth/remove-method/{method.id}">{$t("settings.auth-remove-method")}</a>
			</div>
		{/if}
	</div>
</div>
