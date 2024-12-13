<script lang="ts">
	import { minidenticon } from "minidenticons";

	type Props = { url: string | null; alt: string; lazyLoad?: boolean; size?: number; name: string };
	let { url, alt, lazyLoad, size, name }: Props = $props();

	let width = $derived(size || 200);

	let identicon: string | null = $derived.by(() => {
		if (url) return null;
		return "data:image/svg+xml;utf8," + encodeURIComponent(minidenticon(name, 50, 80));
	});
</script>

<img
	class="rounded-circle img-fluid"
	style="height: {width}px; width: {width}px"
	class:identicon={!url}
	src={url || identicon}
	{alt}
	{width}
	loading={lazyLoad ? "lazy" : "eager"}
/>

<style>
	img {
		object-fit: cover;
	}
</style>
