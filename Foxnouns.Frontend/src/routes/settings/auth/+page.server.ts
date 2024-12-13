import { apiRequest, fastRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error.js";
import type { AuthUrls } from "$api/models/auth";
import log from "$lib/log";

export const load = async ({ fetch }) => {
	const urls = await apiRequest<AuthUrls>("POST", "/auth/urls", { fetch, isInternal: true });
	return { urls };
};

export const actions = {
	password: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const current = body.get("current") as string | null;
		const password = body.get("password") as string | null;
		const password2 = body.get("confirm-password") as string | null;

		if (password !== password2) {
			return {
				ok: false,
				error: {
					status: 400,
					code: ErrorCode.BadRequest,
					message: "Passwords do not match",
				} as RawApiError,
			};
		}

		try {
			await fastRequest("POST", "/auth/email/change-password", {
				body: { current, new: password },
				isInternal: true,
				fetch,
				cookies,
			});

			return { ok: true, error: null };
		} catch (e) {
			if (e instanceof ApiError) return { ok: false, error: e.obj };
			log.error("error changing password:", e);
			throw e;
		}
	},
};
