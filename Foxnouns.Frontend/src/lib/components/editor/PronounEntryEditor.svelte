<script lang="ts">
	import { defaultPreferences, type CustomPreference, type Pronoun } from "$api/models";
	import IconButton from "$components/IconButton.svelte";
	import { t } from "$lib/i18n";
	import tippy from "$lib/tippy";
	import {
		ButtonDropdown,
		Collapse,
		DropdownItem,
		DropdownMenu,
		DropdownToggle,
		Icon,
		InputGroupText,
		Popover,
	} from "@sveltestrap/sveltestrap";

	type Props = {
		index: number;
		value: Pronoun;
		allPreferences: Record<string, CustomPreference>;
		moveValue: (index: number, up: boolean) => void;
		removeValue: (index: number) => void;
	};
	let { index, value = $bindable(), allPreferences, moveValue, removeValue }: Props = $props();

	$effect(() => {
		if (!value.display_text) value.display_text = null;
	});

	let status = $derived(
		value.status in allPreferences ? allPreferences[value.status] : defaultPreferences.missing,
	);

	let prefIds = $derived(Object.keys(allPreferences).filter((s) => s !== "missing"));

	let displayOpen = $state(false);
</script>

<div class="m-1">
	<div class="input-group">
		<IconButton
			icon="chevron-up"
			color="secondary"
			tooltip={$t("editor.move-entry-up")}
			onclick={() => moveValue(index, true)}
		/>
		<IconButton
			icon="chevron-down"
			color="secondary"
			tooltip={$t("editor.move-entry-down")}
			onclick={() => moveValue(index, false)}
		/>
		<input type="text" class="form-control" bind:value={value.value} autocomplete="off" />
		<ButtonDropdown>
			<span use:tippy={{ content: status.tooltip }}>
				<DropdownToggle color="secondary" caret>
					<Icon name={status.icon} />
				</DropdownToggle>
			</span>
			<DropdownMenu>
				{#each prefIds as id}
					<DropdownItem on:click={() => (value.status = id)} active={value.status === id}>
						<Icon name={allPreferences[id].icon} aria-hidden />
						{allPreferences[id].tooltip}
					</DropdownItem>
				{/each}
			</DropdownMenu>
		</ButtonDropdown>
		<IconButton
			color="secondary"
			icon={value.display_text ? "tag-fill" : "tag"}
			tooltip={$t("editor.change-display-text")}
			onclick={() => (displayOpen = !displayOpen)}
		/>
		<IconButton
			color="danger"
			icon="trash3"
			tooltip={$t("editor.remove-entry")}
			onclick={() => removeValue(index)}
		/>
	</div>

	<Collapse class="mt-1" isOpen={displayOpen}>
		<div class="input-group">
			<InputGroupText>{$t("editor.display-text-label")}</InputGroupText>
			<input
				placeholder={$t("editor.display-text-example")}
				type="text"
				class="form-control"
				bind:value={value.display_text}
				autocomplete="off"
			/>
			<IconButton id="display-help" icon="question" tooltip="Help" color="secondary" />
			<!-- TODO: remove children={false} once sveltestrap is updated
			 This component is too complex to write manually, sadly -->
			<Popover target="display-help" placement="bottom" children={false}>
				{$t("editor.display-text-info")}
			</Popover>
		</div>
	</Collapse>
</div>
