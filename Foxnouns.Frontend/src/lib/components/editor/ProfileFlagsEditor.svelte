<script lang="ts">
	import type { RawApiError } from "$api/error";
	import type { PrideFlag } from "$api/models";
	import FlagSearch from "$components/editor/FlagSearch.svelte";
	import IconButton from "$components/IconButton.svelte";
	import { t } from "$lib/i18n";
	import FlagButton from "./FlagButton.svelte";
	import FormStatusMarker from "./FormStatusMarker.svelte";

	type Props = {
		profileFlags: PrideFlag[];
		allFlags: PrideFlag[];
		save(flags: string[]): Promise<void>;
		form: { ok: boolean; error: RawApiError | null } | null;
	};
	let { profileFlags, allFlags, save, form }: Props = $props();

	let flags = $state(profileFlags);

	const select = (flag: PrideFlag) => {
		flags = [...flags, flag];
	};

	const removeFlag = (flag: PrideFlag) => {
		const idx = flags.indexOf(flag);
		if (idx === -1) return;
		flags.splice(idx, 1);
		flags = [...flags];
	};

	const moveFlag = (index: number, up: boolean) => {
		if (up && index == 0) return;
		if (!up && index == flags.length - 1) return;

		const newIndex = up ? index - 1 : index + 1;
		const temp = flags[index];
		flags[index] = flags[newIndex];
		flags[newIndex] = temp;
		flags = [...flags];
	};

	const saveChanges = () => save(flags.map((f) => f.id));
</script>

<div class="row">
	<div class="col-md">
		<h4>
			{$t("settings.flag-title")}
			<button type="button" class="btn btn-primary" onclick={() => saveChanges()}>
				{$t("save-changes")}
			</button>
		</h4>
		<FormStatusMarker {form} />
		{#each flags as flag, i}
			<div class="d-block">
				<div class="btn-group flag-group">
					<IconButton
						icon="chevron-up"
						tooltip={$t("editor.move-flag-up")}
						color="secondary"
						outline
						onclick={() => moveFlag(i, true)}
					/>
					<IconButton
						icon="chevron-down"
						tooltip={$t("editor.move-flag-down")}
						color="secondary"
						outline
						onclick={() => moveFlag(i, false)}
					/>
					<FlagButton
						{flag}
						onclick={() => removeFlag(flag)}
						tooltip={$t("editor.remove-this-flag")}
					/>
				</div>
			</div>
		{:else}
			<p class="text-secondary">
				{$t("editor.no-flags-hint")}
			</p>
		{/each}
	</div>
	<div class="col-md">
		<h4>{$t("editor.add-flags-header")}</h4>
		<FlagSearch flags={allFlags} {select} />
	</div>
</div>

<style>
	.flag-group {
		margin-right: 5px;
		margin-bottom: 5px;
	}
</style>
