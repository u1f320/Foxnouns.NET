<script lang="ts">
	import type { RawApiError } from "$api/error";
	import type { MeUser } from "$api/models";
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import { t } from "$lib/i18n";
	import AuthMethodRow from "./AuthMethodRow.svelte";
	import EnvelopePlusFill from "svelte-bootstrap-icons/lib/EnvelopePlusFill.svelte";

	type Props = {
		user: MeUser;
		canRemove: boolean;
		max: number;
		form: { error: RawApiError | null; ok: boolean } | null;
	};
	let { user, canRemove, max, form }: Props = $props();

	let emails = $derived(user.auth_methods.filter((a) => a.type === "EMAIL"));
</script>

<h3>{$t("auth.email-password-title")}</h3>

{#if emails.length > 0}
	<div class="row">
		<div class="col-md">
			<h4>Your email addresses</h4>
			<div class="list-group">
				{#each emails as method (method.id)}
					<AuthMethodRow {method} {canRemove} />
				{/each}
				{#if emails.length < max}
					<a class="list-group-item" href="/settings/auth/add-email">
						<EnvelopePlusFill /> <strong>{$t("auth.add-email-address")}</strong>
					</a>
				{/if}
			</div>
		</div>
		<div class="col-md">
			<FormStatusMarker {form} successMessage={$t("auth.password-changed-hint")} />
			<h4>Change password</h4>
			<form method="POST" action="?/password">
				<div class="mb-1">
					<label for="current" class="form-label">Current password</label>
					<input type="password" id="current" name="current" class="form-control" required />
				</div>
				<div class="mb-1">
					<label for="password" class="form-label">New password</label>
					<input type="password" id="password" name="password" class="form-control" required />
				</div>
				<div class="mb-1">
					<label for="confirm-password" class="form-label">Confirm new password</label>
					<input
						type="password"
						id="confirm-password"
						name="confirm-password"
						class="form-control"
						required
					/>
				</div>
				<div>
					<button type="submit" class="btn btn-secondary mt-2">Change password</button>
				</div>
			</form>
		</div>
	</div>
{:else}
	<p>{$t("auth.no-email-addresses")}</p>
	<p>
		<a class="btn btn-outline-secondary" href="/settings/auth/add-email">
			<EnvelopePlusFill />
			{$t("auth.add-email-address")}
		</a>
	</p>
{/if}
