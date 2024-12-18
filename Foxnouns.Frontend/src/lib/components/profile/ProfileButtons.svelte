<script lang="ts">
	import type { MeUser } from "$api/models";
	import { PUBLIC_BASE_URL, PUBLIC_SHORT_URL } from "$env/static/public";
	import { t } from "$lib/i18n";

	type Props = {
		user: string;
		member?: string;
		sid: string;
		reportUrl: string;
		meUser: MeUser | null;
	};

	let { user, member, sid, reportUrl, meUser }: Props = $props();

	let profileUrl = $derived(
		member ? `${PUBLIC_BASE_URL}/@${user}/${member}` : `${PUBLIC_BASE_URL}/@${user}`,
	);
	let shortUrl = $derived(`${PUBLIC_SHORT_URL}/${sid}`);

	const copyUrl = async (url: string) => {
		await navigator.clipboard.writeText(url);
	};
</script>

<div class="btn-group">
	<button type="button" class="btn btn-outline-secondary" onclick={() => copyUrl(profileUrl)}>
		{$t("profile.copy-link-button")}
	</button>
	<button type="button" class="btn btn-outline-secondary" onclick={() => copyUrl(shortUrl)}>
		{$t("profile.copy-short-link-button")}
	</button>
	{#if meUser && meUser.username !== user}
		<a class="btn btn-outline-danger" href={reportUrl}>{$t("profile.report-button")}</a>
	{/if}
</div>
