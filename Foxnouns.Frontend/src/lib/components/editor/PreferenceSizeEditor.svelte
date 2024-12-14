<script lang="ts">
	import { PreferenceSize } from "$api/models";
	import { t } from "$lib/i18n";
	import tippy from "$lib/tippy";
	import {
		ButtonDropdown,
		DropdownItem,
		DropdownMenu,
		DropdownToggle,
	} from "@sveltestrap/sveltestrap";
	import Type from "svelte-bootstrap-icons/lib/Type.svelte";

	type Props = { size: PreferenceSize };
	let { size = $bindable() }: Props = $props();
</script>

<ButtonDropdown>
	<span use:tippy={{ content: $t("editor.custom-preference-size") }}>
		<DropdownToggle color="secondary" caret>
			<Type /> <span class="visually-hidden">{$t("editor.custom-preference-size")}</span>
		</DropdownToggle>
	</span>
	<DropdownMenu>
		<DropdownItem
			active={size === PreferenceSize.Large}
			on:click={() => (size = PreferenceSize.Large)}
		>
			{$t("editor.custom-preference-size-large")}
		</DropdownItem>
		<DropdownItem
			active={size === PreferenceSize.Normal}
			on:click={() => (size = PreferenceSize.Normal)}
		>
			{$t("editor.custom-preference-size-medium")}
		</DropdownItem>
		<DropdownItem
			active={size === PreferenceSize.Small}
			on:click={() => (size = PreferenceSize.Small)}
		>
			{$t("editor.custom-preference-size-small")}
		</DropdownItem>
	</DropdownMenu>
</ButtonDropdown>
