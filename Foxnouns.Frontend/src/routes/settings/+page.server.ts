import { fastRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import { clearToken } from "$lib";
import { redirect } from "@sveltejs/kit";

export const actions = {
	logout: async ({ cookies }) => {
		clearToken(cookies);
		redirect(303, "/");
	},
	changeUsername: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const username = body.get("username") as string | null;
		if (username == null)
			return {
				error: {
					status: 403,
					code: ErrorCode.BadRequest,
					message: "Invalid username",
				} as RawApiError,
				ok: false,
			};

		try {
			await fastRequest("PATCH", "/users/@me", {
				fetch,
				cookies,
				body: { username },
			});

			return { error: null, ok: true };
		} catch (e) {
			if (e instanceof ApiError) return { error: e.obj, ok: false };
			throw e;
		}
	},
};
