<script lang="ts">
	import type { Meta } from "$api/models";
	import Git from "svelte-bootstrap-icons/lib/Git.svelte";
	import Reception4 from "svelte-bootstrap-icons/lib/Reception4.svelte";
	import Newspaper from "svelte-bootstrap-icons/lib/Newspaper.svelte";
	import CardText from "svelte-bootstrap-icons/lib/CardText.svelte";
	import Shield from "svelte-bootstrap-icons/lib/Shield.svelte";
	import Envelope from "svelte-bootstrap-icons/lib/Envelope.svelte";
	import CashCoin from "svelte-bootstrap-icons/lib/CashCoin.svelte";
	import Logo from "./Logo.svelte";

	type Props = { meta: Meta };
	let { meta }: Props = $props();
</script>

<footer class="big-footer mt-3 pt-3 pb-1 px-5">
	<div class="d-flex flex-column flex-md-row mb-2">
		<div class="align-start flex-grow-1">
			<Logo />
			<ul class="mt-2 list-unstyled">
				<li><strong>Version</strong> {meta.version}</li>
			</ul>
		</div>
		<div class="align-end">
			<ul class="list-unstyled">
				<li>{meta.users.total} <strong>users</strong></li>
				<li>{meta.members} <strong>members</strong></li>
			</ul>
		</div>
	</div>
	<ul class="list-inline">
		<a
			class="list-inline-item link-underline link-underline-opacity-0"
			target="_blank"
			href={meta.repository}
		>
			<li class="list-inline-item">
				<Git />
				Source code
			</li>
		</a>
		<a
			class="list-inline-item link-underline link-underline-opacity-0"
			target="_blank"
			href="https://status.pronouns.cc"
		>
			<li class="list-inline-item">
				<Reception4 />
				Status
			</li>
		</a>
		<a class="list-inline-item link-underline link-underline-opacity-0" href="/page/about">
			<li class="list-inline-item">
				<Envelope />
				About and contact
			</li>
		</a>
		<a class="list-inline-item link-underline link-underline-opacity-0" href="/page/tos">
			<li class="list-inline-item">
				<CardText />
				Terms of service
			</li>
		</a>
		<a class="list-inline-item link-underline link-underline-opacity-0" href="/page/privacy">
			<li class="list-inline-item">
				<Shield />
				Privacy policy
			</li>
		</a>
		<a class="list-inline-item link-underline link-underline-opacity-0" href="/page/changelog">
			<li class="list-inline-item">
				<Newspaper />
				Changelog
			</li>
		</a>
		<a class="list-inline-item link-underline link-underline-opacity-0" href="/page/donate">
			<li class="list-inline-item">
				<CashCoin />
				Donate
			</li>
		</a>
	</ul>
</footer>
