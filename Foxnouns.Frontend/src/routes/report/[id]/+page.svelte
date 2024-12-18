<script lang="ts">
	import { ReportReason } from "$api/models/moderation";
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import RequiredFieldMarker from "$components/RequiredFieldMarker.svelte";
	import { t } from "$lib/i18n";
	import type { ActionData, PageData } from "./$types";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

	let name = $derived(
		data.member ? `${data.member.name} (@${data.user.username})` : "@" + data.user.username,
	);

	let link = $derived(
		data.member ? `/@${data.user.username}/${data.member.name}` : `/@${data.user.username}`,
	);

	console.log(data.user, !!data.member);

	let reasons = $derived.by(() => {
		const reasons = [];
		for (const value of Object.values(ReportReason)) {
			const key = "report." + value.toLowerCase().replaceAll("_", "-");
			reasons.push({ key, value });
		}
		return reasons;
	});
</script>

<svelte:head>
	<title>{$t("report.title", { name })} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<form method="POST" class="w-lg-75 mx-auto">
		<h3>{$t("report.title", { name })}</h3>
		<FormStatusMarker {form} successMessage={$t("report.success")} />
		<input type="hidden" name="target-type" value={data.member ? "member" : "user"} />
		<input type="hidden" name="target-id" value={data.member ? data.member.id : data.user.id} />

		<h4 class="mt-3">{$t("report.reason-label")} <RequiredFieldMarker required /></h4>
		<div class="row row-cols-1 row-cols-lg-2">
			{#each reasons as reason}
				<div class="col">
					<div class="form-check">
						<input
							class="form-check-input"
							type="radio"
							name="reason"
							value={reason.value}
							id="reason-{reason.value}"
							required
						/>
						<label class="form-check-label" for="reason-{reason.value}">{$t(reason.key)}</label>
					</div>
				</div>
			{/each}
		</div>

		<h4 class="mt-3">
			{$t("report.context-label")}
			<RequiredFieldMarker />
		</h4>
		<textarea class="form-control" name="context" style="height: 100px;" maxlength={512}></textarea>

		<div class="mt-3">
			<button type="submit" class="btn btn-danger">{$t("report.submit-button")}</button>
			<a href={link} class="btn btn-secondary">{$t("cancel")}</a>
		</div>
	</form>
</div>
