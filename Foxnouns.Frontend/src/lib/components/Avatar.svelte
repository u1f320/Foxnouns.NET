<script lang="ts">
	import { DEFAULT_AVATAR } from "$lib";

	type Props = { url: string | null; alt: string; lazyLoad?: boolean; width?: number };
	let { url, alt, lazyLoad, width }: Props = $props();
</script>

<img
	class="rounded-circle img-fluid"
	src={url || DEFAULT_AVATAR}
	{alt}
	width={width || 200}
	loading={lazyLoad ? "lazy" : "eager"}
/>
