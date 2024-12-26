<script lang="ts">
	import type { AuditLogEntity } from "$api/models/moderation";

	type Props = { entity: AuditLogEntity };
	let { entity }: Props = $props();
</script>

<strong>{entity.username}</strong> <span class="text-secondary">({entity.id})</span>
