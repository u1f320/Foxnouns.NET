<script lang="ts">
	import { DEFAULT_AVATAR } from "$lib";

	type Props = { url: string | null; alt: string; lazyLoad?: boolean };
	let { url, alt, lazyLoad }: Props = $props();
</script>

<img
	class="rounded-circle img-fluid"
	src={url || DEFAULT_AVATAR}
	{alt}
	width={200}
	loading={lazyLoad ? "lazy" : "eager"}
/>

<style>
	img {
		object-fit: cover;
		height: 200px;
		width: 200px;
	}
</style>
