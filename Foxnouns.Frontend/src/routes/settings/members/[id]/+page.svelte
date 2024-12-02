<script lang="ts">
	import { DateTime } from "luxon";
	import type { ActionData, PageData } from "./$types";
	import type { Member } from "$api/models";
	import { apiRequest, fastRequest } from "$api";
	import ApiError from "$api/error";
	import log from "$lib/log";
	import { Icon, InputGroup } from "@sveltestrap/sveltestrap";
	import { t } from "$lib/i18n";
	import AvatarEditor from "$components/editor/AvatarEditor.svelte";
	import ErrorAlert from "$components/ErrorAlert.svelte";
	import NoscriptWarning from "$components/editor/NoscriptWarning.svelte";
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";
	import SidEditor from "$components/editor/SidEditor.svelte";
	import BioEditor from "$components/editor/BioEditor.svelte";
	import { PUBLIC_BASE_URL } from "$env/static/public";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

	// SID reroll code
	// We compare the current time with the user's last SID reroll time. If it's more than an hour ago, it can be rerolled.
	let error: ApiError | null = $state(null);
	let sid = $state(data.member.sid);
	let lastSidReroll = $state(data.user.last_sid_reroll);
	let canRerollSid = $derived(
		DateTime.now().toLocal().diff(DateTime.fromISO(lastSidReroll).toLocal(), "hours").hours >= 1,
	);
	const rerollSid = async () => {
		try {
			const resp = await apiRequest<Member>(
				"POST",
				`/users/@me/members/${data.member.id}/reroll-sid`,
				{ token: data.token },
			);
			sid = resp.sid;
			lastSidReroll = DateTime.now().toUTC().toISO();
			error = null;
		} catch (e) {
			log.error("Could not reroll sid:", e);
			if (e instanceof ApiError) error = e;
		}
	};

	// Passed to AvatarEditor
	let avatarUpdated = $state(false);
	const updateAvatar = async (avatar: string) => {
		try {
			await fastRequest("PATCH", `/users/@me/members/${data.member.id}`, {
				body: { avatar },
				token: data.token,
			});
			avatarUpdated = true;
			error = null;
		} catch (e) {
			log.error("Could not update avatar:", e);
			if (e instanceof ApiError) error = e;
		}
	};

	// Bio is stored in a $state() so we have a markdown preview
	let bio = $state(data.member.bio || "");
</script>

{#if error}
	<ErrorAlert error={error.obj} />
{/if}

{#if form}
	<FormStatusMarker {form} />
{/if}

<div class="row">
	<div class="col-md">
		<h4>{$t("settings.avatar")}</h4>
		<AvatarEditor
			current={data.member.avatar_url}
			alt={$t("avatar-tooltip", { name: data.member.name })}
			update={updateAvatar}
			updated={avatarUpdated}
		/>
	</div>
	<div class="col-md">
		<h4>{$t("edit-profile.member-name")}</h4>
		<form method="POST" action="?/changeName" class="mb-3">
			<InputGroup>
				<input
					name="name"
					class="form-control"
					type="text"
					value={data.member.name}
					autocomplete="off"
				/>
				<button type="submit" class="btn btn-primary">
					{$t("change")}
				</button>
			</InputGroup>
		</form>

		<h4>{$t("edit-profile.display-name")}</h4>
		<form class="mb-3" method="POST" action="?/changeDisplayName">
			<InputGroup>
				<input
					class="form-control"
					name="display-name"
					placeholder={data.member.name}
					value={data.member.display_name !== data.member.name ? data.member.display_name : null}
					autocomplete="off"
				/>
				<button class="btn btn-primary" type="submit">{$t("change")}</button>
			</InputGroup>
		</form>

		<h4>{$t("edit-profile.sid")}</h4>
		<SidEditor {rerollSid} {sid} {canRerollSid} />
	</div>
	<div class="row mb-3">
		<h4>{$t("edit-profile.profile-options-header")}</h4>
		<form method="POST" action="?/options">
			<div class="form-check">
				<input
					class="form-check-input"
					type="checkbox"
					checked={data.member.unlisted}
					value="true"
					name="unlisted"
					id="unlisted"
				/>
				<label class="form-check-label" for="unlisted">
					{$t("edit-profile.unlisted-label")}
				</label>
			</div>
			<p class="text-muted mt-1">
				<Icon name="info-circle-fill" aria-hidden />
				{$t("edit-profile.unlisted-note")}
				<code>
					{PUBLIC_BASE_URL.substring("https://".length)}/@{data.member.user.username}/{data.member
						.name}
				</code>
			</p>
			<div class="mt-2">
				<button type="submit" class="btn btn-primary">{$t("save-changes")}</button>
			</div>
		</form>
	</div>
	<div class="row mb-3">
		<h4>{$t("edit-profile.bio-tab")}</h4>
		<form method="POST" action="?/bio">
			<BioEditor bind:value={bio} maxLength={data.meta.limits.bio_length} />
		</form>
	</div>
</div>
