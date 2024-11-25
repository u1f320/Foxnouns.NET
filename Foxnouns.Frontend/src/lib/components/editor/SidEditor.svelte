<script lang="ts">
	import { PUBLIC_SHORT_URL } from "$env/static/public";
	import { t } from "$lib/i18n";
	import { ButtonGroup, Button, Icon } from "@sveltestrap/sveltestrap";
	import ShortNoscriptWarning from "./ShortNoscriptWarning.svelte";

	type Props = { sid: string; rerollSid: () => Promise<void>; canRerollSid: boolean };
	let { sid, rerollSid, canRerollSid }: Props = $props();

	const copySid = async () => {
		const url = `${PUBLIC_SHORT_URL}/${sid}`;
		await navigator.clipboard.writeText(url);
	};
</script>

{$t("edit-profile.sid-current")} <code>{sid}</code>
<ButtonGroup class="mb-1">
	<Button color="secondary" onclick={() => rerollSid()} disabled={!canRerollSid}>
		{$t("edit-profile.sid-reroll")}
	</Button>
	<Button color="secondary" onclick={() => copySid()}>
		<Icon name="link-45deg" aria-hidden />
		<span class="visually-hidden">{$t("edit-profile.sid-copy")}</span>
	</Button>
</ButtonGroup>
<ShortNoscriptWarning />
<p class="text-muted">
	<Icon name="info-circle-fill" aria-hidden />
	{$t("edit-profile.sid-hint")}
</p>
