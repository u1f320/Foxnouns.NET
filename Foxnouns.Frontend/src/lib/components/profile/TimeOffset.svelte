<script lang="ts">
	import { DateTime, FixedOffsetZone } from "luxon";
	import Clock from "svelte-bootstrap-icons/lib/Clock.svelte";

	type Props = { offset: number };
	let { offset }: Props = $props();

	let { currentTime, timezone } = $derived.by(() => {
		const zone = FixedOffsetZone.instance(offset / 60);

		return {
			currentTime: DateTime.now().setZone(zone).toLocaleString(DateTime.TIME_SIMPLE),
			timezone: zone.formatOffset(DateTime.now().toUnixInteger(), "narrow"),
		};
	});
</script>

<Clock aria-hidden />
{currentTime} <span class="text-body-secondary">(UTC{timezone})</span>
