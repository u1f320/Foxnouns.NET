import { fastRequest } from "$api";
import ApiError from "$api/error";
import log from "$lib/log.js";

export const actions = {
	default: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const bio = body.get("bio") as string | null;

		try {
			await fastRequest("PATCH", "/users/@me", { body: { bio }, fetch, cookies });
			return { error: null, ok: true };
		} catch (e) {
			if (e instanceof ApiError) return { error: e.obj, ok: false };
			log.error("Error updating bio:", e);
			throw e;
		}
	},
};
