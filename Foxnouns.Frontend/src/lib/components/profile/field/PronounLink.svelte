<script lang="ts">
	import type { Pronoun } from "$api/models/user";

	type Props = { pronouns: Pronoun };
	let { pronouns }: Props = $props();

	// TODO: this entire component is only made with English pronouns in mind.
	// It's gonna need a major rework to work with other languages.
	const updatePronouns = (pronouns: Pronoun) => {
		if (pronouns.display_text) {
			return pronouns.display_text;
		} else {
			const split = pronouns.value.split("/");
			if (split.length === 5) return split.splice(0, 2).join("/");
			return pronouns.value;
		}
	};

	const linkPronouns = (pronouns: Pronoun) => {
		const linkBase = pronouns.value
			.split("/")
			.map((snippet) => encodeURIComponent(snippet))
			.join("/");

		if (pronouns.display_text) {
			return `${linkBase},${encodeURIComponent(pronouns.display_text)}`;
		}
		return linkBase;
	};

	let pronounText = $derived(updatePronouns(pronouns));

	let link = $derived(linkPronouns(pronouns));
	let shouldLink = $derived(pronouns.value.split("/").length === 5);
</script>

{#if shouldLink}
	<a class="text-reset" href="/pronouns/{link}">{pronounText}</a>
{:else}
	{pronounText}
{/if}
