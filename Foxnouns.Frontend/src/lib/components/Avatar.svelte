<script lang="ts">
	import { DEFAULT_AVATAR } from "$lib";

	type Props = { url: string | null; alt: string; lazyLoad?: boolean; size?: number };
	let { url, alt, lazyLoad, size }: Props = $props();

	let width = $derived(size || 200);
</script>

<img
	class="rounded-circle img-fluid"
	style="height: {width}px; width: {width}px"
	src={url || DEFAULT_AVATAR}
	{alt}
	{width}
	loading={lazyLoad ? "lazy" : "eager"}
/>

<style>
	img {
		object-fit: cover;
	}
</style>
