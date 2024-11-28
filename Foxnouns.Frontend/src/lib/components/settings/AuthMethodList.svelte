<script lang="ts">
	import type { AuthMethod } from "$api/models";
	import AuthMethodRow from "./AuthMethodRow.svelte";

	type Props = {
		methods: AuthMethod[];
		canRemove: boolean;
		max: number;
		buttonLink: string;
		buttonText: string;
	};
	let { methods, canRemove, max, buttonLink, buttonText }: Props = $props();
</script>

{#if methods.length > 0}
	<div class="list-group mb-3">
		{#each methods as method (method.id)}
			<AuthMethodRow {method} {canRemove} />
		{/each}
	</div>
{/if}
{#if methods.length < max}
	<a class="btn btn-primary mb-3" href={buttonLink}>{buttonText}</a>
{/if}
