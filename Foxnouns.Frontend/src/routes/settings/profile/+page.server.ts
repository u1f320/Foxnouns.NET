import { fastRequest } from "$api";
import ApiError from "$api/error";
import log from "$lib/log.js";

export const actions = {
	options: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		let memberTitle = body.get("member-title") as string | null;
		if (!memberTitle || memberTitle === "") memberTitle = null;

		let timezone = body.get("timezone") as string | null;
		if (!timezone || timezone === "") timezone = null;

		const hideMemberList = !!body.get("hide-member-list");

		try {
			await fastRequest("PATCH", "/users/@me", {
				body: { timezone, member_title: memberTitle, member_list_hidden: hideMemberList },
				fetch,
				cookies,
			});
			return { error: null, ok: true };
		} catch (e) {
			if (e instanceof ApiError) return { error: e.obj, ok: false };
			log.error("Error patching user:", e);
			throw e;
		}
	},
	changeDisplayName: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		let displayName = body.get("display-name") as string | null;
		if (!displayName || displayName === "") displayName = null;

		try {
			await fastRequest("PATCH", "/users/@me", {
				body: { display_name: displayName },
				fetch,
				cookies,
			});
			return { error: null, ok: true };
		} catch (e) {
			if (e instanceof ApiError) return { error: e.obj, ok: false };
			log.error("Error patching user:", e);
			throw e;
		}
	},
};
