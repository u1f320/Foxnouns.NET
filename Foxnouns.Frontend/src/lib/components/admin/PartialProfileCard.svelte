<script lang="ts">
	import type { PartialUser } from "$api/models";
	import Avatar from "$components/Avatar.svelte";
	import { idTimestamp } from "$lib";
	import { t } from "$lib/i18n";
	import { DateTime } from "luxon";

	type Props = { user: PartialUser };
	let { user }: Props = $props();

	let createdAt = $derived(idTimestamp(user.id).toLocaleString(DateTime.DATETIME_SHORT));
</script>

<div class="text-center">
	<a href="/@{user.username}">
		<Avatar
			name={user.username}
			url={user.avatar_url}
			lazyLoad
			alt={$t("avatar-tooltip", { name: "@" + user.username })}
		/>
	</a>
	<p class="m-2">
		<a class="text-reset fs-5 text-break" href="/@{user.username}">
			@{user.username}
		</a>
	</p>
	<p>Created {createdAt}</p>
</div>
