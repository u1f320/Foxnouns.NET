<script lang="ts">
	import type { ValidationError } from "$api/error";
	import { t } from "$lib/i18n";

	type Props = { error: ValidationError };
	let { error }: Props = $props();

	let isLengthError = $derived(error.min_length && error.max_length && error.actual_length);
	let isDisallowedValueError = $derived(error.allowed_values && error.actual_value);
</script>

{#if isLengthError}
	{#if error.actual_length! > error.max_length!}
		<li>
			{$t("error.validation-max-length-error", {
				max: error.max_length,
				actual: error.actual_length,
			})}
		</li>
	{:else}
		<li>
			{$t("error.validation-min-length-error", {
				min: error.min_length,
				actual: error.actual_length,
			})}
		</li>
	{/if}
{:else if isDisallowedValueError}
	<li>
		{$t("error.validation-disallowed-value-1")}: <code>{error.actual_value}</code><br />
		{$t("error.validation-disallowed-value-2")}:
		<code>{error.allowed_values!.map((v) => v.toString()).join(", ")}</code>
	</li>
{:else if error.actual_value}
	<li>
		{$t("error.validation-disallowed-value-1")}: <code>{error.actual_value}</code><br />
		{$t("error.validation-reason")}: {error.message}
	</li>
{:else}
	<li>{$t("error.validation-generic")}: {error.message}</li>
{/if}
