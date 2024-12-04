<script lang="ts">
	import type { ActionData, PageData } from "./$types";
	import { t } from "$lib/i18n";
	import { enhance } from "$app/forms";
	import { Button, Input, InputGroup } from "@sveltestrap/sveltestrap";
	import FormStatusMarker from "$components/editor/FormStatusMarker.svelte";

	type Props = { data: PageData; form: ActionData };
	let { data, form }: Props = $props();
</script>

<svelte:head>
	<title>{$t("auth.register-with-email")} • pronouns.cc</title>
</svelte:head>

<div class="container">
	<div class="mx-auto w-lg-75">
		<h2>{$t("auth.register-with-email")}</h2>

		<FormStatusMarker {form} successMessage={$t("auth.register-with-email-init-success")} />

		<form method="POST" use:enhance>
			<InputGroup>
				<Input name="email" type="email" placeholder={$t("auth.email-label")} />
				<Button type="submit" color="secondary">{$t("auth.register-with-email-button")}</Button>
			</InputGroup>
		</form>
	</div>
</div>
