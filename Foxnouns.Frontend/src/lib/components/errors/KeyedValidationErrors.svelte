<script lang="ts">
	import type { ValidationError } from "$api/error";
	import RequestValidationError from "./RequestValidationError.svelte";

	type Props = { key: string; errors: ValidationError[] };
	let { key, errors }: Props = $props();
</script>

<li>
	<code>{key}</code>:
	<ul>
		{#each errors as error}
			<RequestValidationError {error} />
		{/each}
	</ul>
</li>
