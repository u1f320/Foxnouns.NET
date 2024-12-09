<script lang="ts">
	import { Icon } from "@sveltestrap/sveltestrap";

	type Props = { link: string };
	let { link }: Props = $props();

	const prettifyLink = (raw: string) => {
		let out = raw;
		if (raw.startsWith("https://")) out = raw.substring("https://".length);
		else if (raw.startsWith("http://")) out = raw.substring("http://".length);

		if (out.endsWith("/")) out = out.substring(0, out.length - 1);

		return out;
	};

	let isLink = $derived(link.startsWith("http://") || link.startsWith("https://"));
	let displayLink = $derived(prettifyLink(link));
</script>

{#if isLink}
	<a href={link} class="text-decoration-none" rel="me nofollow noreferrer" target="_blank">
		<li class="py-2 py-lg-0">
			<Icon name="globe" aria-hidden class="text-body" />
			<span class="text-decoration-underline">{displayLink}</span>
		</li>
	</a>
{:else}
	<li class="py-2 py-lg-0">
		<Icon name="globe" aria-hidden />
		<span>{displayLink}</span>
	</li>
{/if}
