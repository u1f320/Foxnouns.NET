<script lang="ts">
	let isOpen = false;
	export let right = false;
</script>

<div class={"navbar-item has-dropdown" + (isOpen ? " is-active" : "") + (right ? " is-right" : "")}>
	<button class="navbar-link" on:click={() => (isOpen = !isOpen)}><slot name="label" /></button>
	<div class="navbar-dropdown">
		<slot />
	</div>
</div>
