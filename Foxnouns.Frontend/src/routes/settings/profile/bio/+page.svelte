<script lang="ts">
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import type { ActionData, PageData } from "./$types";
	import BioEditor from "$components/editor/BioEditor.svelte";
	import { t } from "$lib/i18n";
	import { enhance } from "$app/forms";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

	let bio = $state(data.user.bio || "");
</script>

<h4>{$t("edit-profile.bio-tab")}</h4>
<FormStatusMarker {form} />
<form method="POST" use:enhance>
	<BioEditor bind:value={bio} maxLength={data.meta.limits.bio_length} />
</form>
