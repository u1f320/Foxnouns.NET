<script lang="ts">
	import type { ActionData, PageData } from "./$types";
	import { t } from "$lib/i18n";
	import { Button, ButtonGroup, Icon, InputGroup } from "@sveltestrap/sveltestrap";
	import { PUBLIC_SHORT_URL } from "$env/static/public";
	import AvatarEditor from "$components/editor/AvatarEditor.svelte";
	import { apiRequest, fastRequest } from "$api";
	import ApiError from "$api/error";
	import ErrorAlert from "$components/ErrorAlert.svelte";
	import type { MeUser } from "$api/models/user";
	import log from "$lib/log";
	import { DateTime, FixedOffsetZone } from "luxon";
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();

	let error: ApiError | null = $state(null);

	const copySid = async () => {
		const url = `${PUBLIC_SHORT_URL}/${data.user.sid}`;
		await navigator.clipboard.writeText(url);
	};

	// Editable properties
	let sid = $state(data.user.sid);
	let lastSidReroll = $state(data.user.last_sid_reroll);
	let tz = $state(data.user.timezone === "<none>" ? null : data.user.timezone);

	// Timezone code
	const validTimezones = Intl.supportedValuesOf("timeZone");
	const detectTimezone = () => {
		tz = DateTime.local().zoneName;
	};

	// Timezone code
	let currentTime = $state("");
	let displayTimezone = $state("");
	$effect(() => {
		if (!tz || tz === "") {
			currentTime = "";
			displayTimezone = "";
			return;
		}

		const offset = DateTime.now().setZone(tz).offset;
		const zone = FixedOffsetZone.instance(offset);

		currentTime = DateTime.now().setZone(zone).toLocaleString(DateTime.TIME_SIMPLE);
		displayTimezone = zone.formatOffset(DateTime.now().toUnixInteger(), "narrow");
	});

	// SID reroll code
	// We compare the current time with the user's last SID reroll time. If it's more than an hour ago, it can be rerolled.
	let canRerollSid = $derived(
		DateTime.now().toLocal().diff(DateTime.fromISO(lastSidReroll).toLocal(), "hours").hours >= 1,
	);
	const rerollSid = async () => {
		try {
			const resp = await apiRequest<MeUser>("POST", "/users/@me/reroll-sid", { token: data.token });
			sid = resp.sid;
			lastSidReroll = resp.last_sid_reroll;
			error = null;
		} catch (e) {
			log.error("Could not reroll sid:", e);
			if (e instanceof ApiError) error = e;
		}
	};

	// Passed to AvatarEditor
	let updated = $state(false);
	const updateAvatar = async (avatar: string) => {
		try {
			await fastRequest("PATCH", "/users/@me", {
				body: { avatar },
				token: data.token,
			});
			updated = true;
			error = null;
		} catch (e) {
			log.error("Could not update avatar:", e);
			if (e instanceof ApiError) error = e;
		}
	};
</script>

{#if error}
	<ErrorAlert error={error.obj} />
{/if}

<div class="row">
	<div class="col-md">
		<h4>{$t("settings.avatar")}</h4>
		<AvatarEditor
			current={data.user.avatar_url}
			alt={$t("avatar-tooltip", { name: "@" + data.user.username })}
			onclick={updateAvatar}
			{updated}
		/>
	</div>
	<div class="col-md">
		<h4>{$t("edit-profile.sid")}</h4>
		{$t("edit-profile.sid-current")} <code>{sid}</code>
		<ButtonGroup class="mb-1">
			<Button color="secondary" onclick={() => rerollSid()} disabled={!canRerollSid}>
				{$t("edit-profile.sid-reroll")}
			</Button>
			<Button color="secondary" onclick={() => copySid()}>
				<Icon name="link-45deg" aria-hidden />
				<span class="visually-hidden">{$t("edit-profile.sid-copy")}</span>
			</Button>
		</ButtonGroup>
		<p class="text-muted">
			<Icon name="info-circle-fill" aria-hidden />
			{$t("edit-profile.sid-hint")}
		</p>
	</div>
</div>

<div class="mt-3">
	<h4>{$t("edit-profile.profile-options-header")}</h4>
	<FormStatusMarker {form} />
	<form method="POST" action="?/options">
		<div class="mb-3">
			<label class="form-label" for="member-title">{$t("edit-profile.member-header-label")}</label>
			<input
				type="text"
				id="member-title"
				name="member-title"
				class="form-control"
				value={data.user.member_title}
				placeholder={$t("profile.default-members-header")}
			/>
			<p class="text-muted mt-1">
				<Icon name="info-circle-fill" aria-hidden />
				{$t("edit-profile.member-header-info")}
			</p>
		</div>
		<div class="mb-3">
			<label class="form-label" for="timezone">{$t("edit-profile.timezone-label")}</label>
			<InputGroup>
				<input
					type="text"
					id="timezone"
					name="timezone"
					class="form-control"
					list="timezones"
					bind:value={tz}
				/>
				<datalist id="timezones">
					{#each validTimezones as timezone}<option value={timezone}></option>{/each}
				</datalist>
				<button type="button" class="btn btn-secondary" onclick={() => detectTimezone()}>
					Detect timezone
				</button>
			</InputGroup>
			{#if tz && tz !== "" && validTimezones.includes(tz)}
				<div class="mt-1">
					{$t("edit-profile.timezone-preview")}
					<Icon name="clock" aria-hidden />
					{currentTime} <span class="text-body-secondary">(UTC{displayTimezone})</span>
				</div>
			{/if}
			<p class="text-muted mt-1">
				<Icon name="info-circle-fill" aria-hidden />
				{$t("edit-profile.timezone-info")}
			</p>
		</div>
		<div class="form-check">
			<input
				class="form-check-input"
				type="checkbox"
				checked={data.user.member_list_hidden}
				value="true"
				name="hide-member-list"
				id="hide-member-list"
			/>
			<label class="form-check-label" for="hide-member-list">
				{$t("edit-profile.hide-member-list-label")}
			</label>
		</div>
		<p class="text-muted mt-1">
			<Icon name="info-circle-fill" aria-hidden />
			{$t("edit-profile.hide-member-list-info")}
		</p>
		<div class="mt-2">
			<button type="submit" class="btn btn-primary">{$t("save-changes")}</button>
		</div>
	</form>
</div>
