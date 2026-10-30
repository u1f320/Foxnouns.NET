<script lang="ts">
	import { ClearableField } from "$api/models/moderation";
	import FormStatusMarker, { type FormError } from "$components/editor/FormStatusMarker.svelte";
	import { TabContent, TabPane } from "@sveltestrap/sveltestrap";

	let {
		userId,
		reportId,
		memberId,
		form,
	}: { userId: string; reportId?: string; memberId?: string; form: FormError } = $props();

	let fields = $derived.by(() => {
		const fields = [];
		for (const value of Object.values(ClearableField)) {
			fields.push({ value });
		}
		return fields;
	});
</script>

<form method="POST">
	<input type="hidden" name="user" value={userId} />
	{#if memberId}
		<input type="hidden" name="member" value={memberId} />
	{/if}
	{#if reportId}
		<input type="hidden" name="report" value={reportId} />
	{/if}
	<FormStatusMarker {form} />
	<textarea name="reason" class="form-control mb-2" style="height: 200px;"></textarea>
	<TabContent>
		{#if reportId}
			<TabPane tabId="ignore" tab="Ignore" class="p-2">
				<button type="submit" formaction="?/ignore" class="btn btn-secondary">Ignore report</button>
			</TabPane>
		{/if}
		<TabPane tabId="warn" tab="Warn" active class="p-2">
			<div class="row row-cols-1 row-cols-lg-2 mb-2">
				{#each fields as field}
					<div class="form-check">
						<input
							class="form-check-input"
							type="checkbox"
							name="clear-fields"
							value={field.value}
							id="reason-{field.value}"
						/>
						<label class="form-check-label" for="reason-{field.value}">
							<code>{field.value}</code>
						</label>
					</div>
				{/each}
			</div>
			<div>
				<button type="submit" formaction="?/warn" class="btn btn-danger">Warn user</button>
			</div>
		</TabPane>
		<TabPane tabId="suspend" tab="Suspend" class="p-2">
			<div class="form-check mb-2">
				<input
					class="form-check-input"
					type="checkbox"
					value="yes"
					name="clear-profile"
					id="clear-profile"
				/>
				<label class="form-check-label" for="clear-profile">Clear the user's profile?</label>
			</div>
			<button type="submit" formaction="?/suspend" class="btn btn-danger">Suspend user</button>
		</TabPane>
	</TabContent>
</form>
