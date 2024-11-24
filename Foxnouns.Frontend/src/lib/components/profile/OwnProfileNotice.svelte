<script lang="ts">
	import { t } from "$lib/i18n";

	type Props = { memberName?: string; editLink: string };
	let { memberName, editLink }: Props = $props();
</script>

<div class="alert alert-secondary">
	{#if memberName}
		{$t("profile.edit-member-profile-notice", { memberName })}
	{:else}
		{$t("profile.edit-user-profile-notice")}
	{/if}
	<br />
	<a href={editLink}>{$t("profile.edit-profile-link")}</a>
</div>
