<script lang="ts">
	import { Accordion, AccordionItem } from "@sveltestrap/sveltestrap";
	import type { ActionData, PageData } from "./$types";
	import EditorFlagImage from "$components/editor/EditorFlagImage.svelte";
	import { t } from "$lib/i18n";
	import type { PrideFlag } from "$api/models";
	import paginate from "$lib/paginate";
	import ClientPaginator from "$components/ClientPaginator.svelte";
	import FlagEditor from "$components/editor/FlagEditor.svelte";
	import NoscriptWarning from "$components/editor/NoscriptWarning.svelte";
	import { fastRequest } from "$api";
	import type { RawApiError } from "$api/error";
	import ApiError from "$api/error";
	import log from "$lib/log";
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

	let flags = $state(data.flags);

	let arr: PrideFlag[] = $state([]);
	let currentPage = $state(0);
	let pageCount = $state(0);

	const FLAGS_PER_PAGE = 50;

	$effect(() => {
		const pages = paginate(flags, currentPage, FLAGS_PER_PAGE);
		arr = pages.data;
		pageCount = pages.pageCount;
	});

	let lastEditedFlag: string | null = $state(null);
	let ok: { ok: boolean; error: RawApiError | null } | null = $state(null);

	const update = async (
		id: string,
		{ name, description }: { name: string; description: string | null },
	) => {
		lastEditedFlag = id;
		try {
			await fastRequest("PATCH", `/users/@me/flags/${id}`, {
				body: { name, description },
				token: data.token,
			});
			ok = { ok: true, error: null };

			const idx = flags.findIndex((f) => f.id === id);
			if (idx === -1) return;
			console.log("yippee");
			flags[idx] = { ...flags[idx], name, description };
		} catch (e) {
			log.error("Could not update flag %s:", id, e);
			if (e instanceof ApiError) ok = { ok: false, error: e.obj };
		}
	};

	const deleteFlag = async (id: string) => {
		lastEditedFlag = id;
		try {
			await fastRequest("DELETE", `/users/@me/flags/${id}`, { token: data.token });
			ok = { ok: true, error: null };

			const idx = flags.findIndex((f) => f.id === id);
			if (idx === -1) return;
			flags.splice(idx, 1);
			flags = [...flags];
		} catch (e) {
			log.error("Could not remove flag %s:", id, e);
			if (e instanceof ApiError) ok = { ok: false, error: e.obj };
		}
	};
</script>

<h3>{$t("settings.flag-title")}</h3>

<NoscriptWarning />

<form method="POST" action="?/upload" enctype="multipart/form-data">
	<FormStatusMarker {form} successMessage={$t("settings.flag-upload-success")} />
	<h4>{$t("settings.flag-upload-title")}</h4>
	<input
		type="file"
		name="image"
		accept="image/png, image/jpeg, image/gif, image/webp"
		class="mb-2 form-control"
		required
	/>
	<input
		class="mb-2 form-control"
		name="name"
		placeholder={$t("settings.flag-name-placeholder")}
		autocomplete="off"
		required
	/>
	<input
		class="mb-2 form-control"
		name="desc"
		placeholder={$t("settings.flag-description-placeholder")}
		autocomplete="off"
	/>
	<button type="submit" class="btn btn-primary">{$t("settings.flag-upload-button")}</button>
</form>

<h4 class="mt-3">
	{$t("settings.flag-current-flags-title", {
		count: data.flags.length,
		max: data.meta.limits.max_flags,
	})}
</h4>

<ClientPaginator center bind:currentPage {pageCount} />

<Accordion class="mb-3">
	{#each arr as flag (flag.id)}
		<AccordionItem>
			<span slot="header">
				<EditorFlagImage {flag} />
				{flag.name}
			</span>

			{#if lastEditedFlag === flag.id}<FormStatusMarker form={ok} />{/if}
			<FlagEditor {flag} {update} {deleteFlag} />
		</AccordionItem>
	{/each}
</Accordion>

<ClientPaginator center bind:currentPage {pageCount} />
