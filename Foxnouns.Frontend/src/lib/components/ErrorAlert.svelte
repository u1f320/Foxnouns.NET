<script lang="ts">
	import type { RawApiError } from "$api/error";
	import Error from "./Error.svelte";

	type Props = { error: RawApiError };
	let { error }: Props = $props();
</script>

<div class="alert alert-danger" role="alert">
	<Error {error} />
</div>
