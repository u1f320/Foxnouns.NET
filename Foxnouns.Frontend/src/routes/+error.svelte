<script>
	import { page } from "$app/stores";
	import neofox from "./neofox_confused_2048.png";
</script>

{#if $page.status === 404}
	<div class="has-text-centered">
		<img src={neofox} alt="A very confused-looking fox" width="25%" />
		<h1 class="title">Not found</h1>
		<p>Our foxes can't find the page you're looking for, sorry!</p>
	</div>
{:else}
	div.has-text-centered
{/if}
