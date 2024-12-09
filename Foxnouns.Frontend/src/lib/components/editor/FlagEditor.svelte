<script lang="ts">
	import type { PrideFlag } from "$api/models";
	import { DEFAULT_FLAG } from "$lib";
	import { t } from "$lib/i18n";

	type UpdateParams = { name: string; description: string | null };

	type Props = {
		flag: PrideFlag;
		update(id: string, params: UpdateParams): Promise<void>;
		deleteFlag(id: string): Promise<void>;
	};
	let { flag, update, deleteFlag }: Props = $props();

	let name = $state(flag.name);
	let description = $state(flag.description);

	const saveChanges = () =>
		update(flag.id, { name, description: description ? description : null });
</script>

<div class="d-flex">
	<span class="me-3">
		<img class="flag" src={flag.image_url ?? DEFAULT_FLAG} alt={flag.description ?? flag.name} />
	</span>
	<div class="w-lg-50">
		<input
			class="mb-2 form-control"
			placeholder={$t("settings.flag-name-placeholder")}
			bind:value={name}
			autocomplete="off"
		/>
		<textarea
			class="mb-2 form-control"
			style="height: 5rem;"
			placeholder={$t("settings.flag-description-placeholder")}
			bind:value={description}
			autocomplete="off"
		></textarea>
		<div class="btn-group">
			<button type="button" class="btn btn-primary" onclick={saveChanges}>
				{$t("save-changes")}
			</button>
			<button type="button" class="btn btn-outline-danger" onclick={() => deleteFlag(flag.id)}>
				{$t("settings.flag-delete-button")}
			</button>
		</div>
	</div>
</div>

<style>
	.flag {
		max-height: 6rem;
		max-width: 256px;
		border-radius: 3px;
	}
</style>
