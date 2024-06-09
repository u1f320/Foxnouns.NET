<script lang="ts">
	let isOpen = false;
</script>

<div class={"navbar-item has-dropdown" + (isOpen ? " is-active" : "")}>
	<button class="navbar-link" on:click={() => (isOpen = !isOpen)}><slot name="label" /></button>
	<div class="navbar-dropdown">
		<slot />
	</div>
</div>
