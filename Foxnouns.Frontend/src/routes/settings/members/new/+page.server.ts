import { apiRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import type { Member } from "$api/models/member";
import log from "$lib/log.js";
import { isRedirect, redirect } from "@sveltejs/kit";

export const actions = {
	default: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const name = body.get("name") as string | null;
		if (!name)
			return {
				error: {
					message: "No name supplied.",
					status: 403,
					code: ErrorCode.BadRequest,
				} as RawApiError,
			};

		try {
			const member = await apiRequest<Member>("POST", "/users/@me/members", {
				body: { name },
				fetch,
				cookies,
			});
			redirect(303, `/settings/members/${member.id}`);
		} catch (e) {
			if (isRedirect(e)) throw e;
			if (e instanceof ApiError) return { error: e.obj };
			log.error("Could not create member:", e);
			throw e;
		}
	},
};
