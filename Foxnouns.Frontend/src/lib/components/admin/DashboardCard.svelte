<script lang="ts">
	import type { Snippet } from "svelte";

	type Props = { title: string; onlyNumber?: boolean; children: Snippet };
	let { title, onlyNumber = true, children }: Props = $props();
</script>

<div class="col-md">
	<div class="card">
		<div class="card-body">
			<h5 class="card-title">{title}</h5>
			<p class="card-text text-center" class:fs-1={onlyNumber}>
				{@render children()}
			</p>
		</div>
	</div>
</div>
