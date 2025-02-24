import { onMount, onDestroy } from "svelte";
import { browser } from "$app/environment";
import log from "./log";
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import type { Snapshot } from "@sveltejs/kit";

/**
 * Store ephemeral state in sessionStorage to persist between navigations.
 * Similar to {@link Snapshot}, but doesn't attach it to a history entry.
 * @param key Unique key to use for this state.
 * @param capture Function that returns the state to store.
 * @param restore Function that takes the state that was stored previously and assigns it back to component variables.
 */
export default function ephemeralState<T>(
	key: string,
	capture: () => T,
	restore: (data: T) => void,
): void {
	if (!browser) return;

	onMount(() => {
		if (!("sessionStorage" in window)) return;
		const rawData = sessionStorage.getItem("ephemeral-" + key);
		if (!rawData) return;

		log.debug("Restoring data %s from session storage", key);
		const data = JSON.parse(rawData) as T;
		restore(data);
	});

	onDestroy(() => {
		if (!("sessionStorage" in window)) return;

		log.debug("Saving data %s to session storage", key);
		sessionStorage.setItem("ephemeral-" + key, JSON.stringify(capture()));
	});
}
