<script lang="ts">
	export let divider = false;
	export let href: string | undefined = undefined;
</script>

{#if divider}
	<hr class="navbar-divider" />
{:else}
	<a {href} class="navbar-item"><slot /></a>
{/if}
