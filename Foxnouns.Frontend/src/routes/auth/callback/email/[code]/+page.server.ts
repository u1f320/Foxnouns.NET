import { apiRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import type { AuthResponse } from "$api/models/auth";
import { setToken } from "$lib";
import createCallbackLoader from "$lib/actions/callback";
import log from "$lib/log";
import { redirect, isRedirect } from "@sveltejs/kit";

export const load = createCallbackLoader("email", async ({ params }) => {
	log.info("params:", params, "code:", params.code);

	return { state: params.code! };
});

export const actions = {
	default: async ({ request, fetch, cookies }) => {
		const data = await request.formData();
		const username = data.get("username") as string | null;
		const ticket = data.get("ticket") as string | null;
		const password = data.get("password") as string | null;
		const password2 = data.get("confirm-password") as string | null;

		if (!username || !ticket || !password || !password2)
			return {
				error: { message: "Bad request", code: ErrorCode.BadRequest, status: 400 } as RawApiError,
			};

		if (password !== password2)
			return {
				error: {
					message: "Passwords do not match",
					code: ErrorCode.BadRequest,
					status: 400,
				} as RawApiError,
			};

		try {
			const resp = await apiRequest<AuthResponse>("POST", "/auth/email/register", {
				body: { username, ticket, password },
				isInternal: true,
				fetch,
			});

			setToken(cookies, resp.token);
			redirect(303, "/auth/welcome");
		} catch (e) {
			if (isRedirect(e)) throw e;
			log.error("Could not sign up user with username %s:", username, e);
			if (e instanceof ApiError) return { error: e.obj };
			throw e;
		}
	},
};
