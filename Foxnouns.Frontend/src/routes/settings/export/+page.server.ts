import { apiRequest, fastRequest } from "$api";
import ApiError from "$api/error.js";
import log from "$lib/log.js";
import { DateTime, Duration } from "luxon";

type Export = { url: string | null; expires_at: string | null };

export const load = async ({ fetch, cookies }) => {
	const resp = await apiRequest<Export>("GET", "/data-exports", {
		fetch,
		cookies,
		isInternal: true,
	});

	let canExport = true;
	if (resp.expires_at) {
		const created = DateTime.fromISO(resp.expires_at).minus(Duration.fromObject({ days: 15 }));
		canExport = DateTime.now().diff(created, "seconds").seconds >= 86400;
	}

	return { url: resp.url, expiresAt: resp.expires_at, canExport };
};

export const actions = {
	default: async ({ fetch, cookies }) => {
		try {
			fastRequest("POST", "/data-exports", { fetch, cookies, isInternal: true });
			return { ok: true, error: null };
		} catch (e) {
			if (e instanceof ApiError) return { ok: false, error: e.obj };
			log.error("Error requesting data export:", e);
			throw e;
		}
	},
};
