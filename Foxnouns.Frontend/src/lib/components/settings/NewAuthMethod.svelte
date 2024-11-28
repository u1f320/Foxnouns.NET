<script lang="ts">
	import type { AuthMethod, PartialUser } from "$api/models";
	import { t } from "$lib/i18n";

	type Props = { method: AuthMethod; user: PartialUser };
	let { method, user }: Props = $props();

	let name = $derived(
		method.type === "EMAIL" ? method.remote_id : (method.remote_username ?? method.remote_id),
	);

	let text = $derived.by(() => {
		switch (method.type) {
			case "DISCORD":
				return $t("auth.successful-link-discord");
			case "GOOGLE":
				return $t("auth.successful-link-google");
			case "TUMBLR":
				return $t("auth.successful-link-tumblr");
			case "FEDIVERSE":
				return $t("auth.successful-link-fedi");
			default:
				return "<you shouldn't see this!>";
		}
	});
</script>

<h1>{$t("auth.new-auth-method-added")}</h1>

<p>{text} <code>{name}</code></p>
<p>{$t("auth.successful-link-profile-hint")}</p>
<p>
	<a class="btn btn-primary" href="/@{user.username}">{$t("auth.successful-link-profile-link")}</a>
</p>
