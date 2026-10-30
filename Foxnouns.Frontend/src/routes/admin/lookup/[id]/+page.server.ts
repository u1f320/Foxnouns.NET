import { apiRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import type { QueriedUser } from "$api/models/moderation";
import { createModactions } from "$lib/actions/modaction.js";
import log from "$lib/log.js";
import type { Actions } from "./$types.js";

export const load = async ({ params, fetch, cookies }) => {
	const user = await apiRequest<QueriedUser>("GET", `/moderation/lookup/${params.id}`, {
		fetch,
		cookies,
	});

	return { user };
};

export const actions = {
	requestData: async ({ request, fetch, cookies, params }) => {
		const body = await request.formData();
		const reason = body.get("reason") as string | null;
		if (!reason) {
			return {
				ok: false,
				error: {
					status: 400,
					message: "Bad request",
					code: ErrorCode.BadRequest,
					errors: [
						{ key: "reason", errors: [{ message: "A reason is required for this action" }] },
					],
				} satisfies RawApiError,
			};
		}

		try {
			log.info(params.id);
			await apiRequest("POST", `/moderation/lookup/${params.id}/sensitive`, {
				fetch,
				cookies,
				body: {
					reason: "meow",
				},
			});
			return { ok: true, error: null };
		} catch (e) {
			if (e instanceof ApiError) return { ok: false, error: e.obj };
			log.error("could not request sensitive data for user %s:", params.id, e);
			throw e;
		}
	},
	...createModactions(),
} satisfies Actions;
