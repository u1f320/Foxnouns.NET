import { apiRequest, fastRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import type { AuthUrls } from "$api/models";
import log from "$lib/log";
import { redirect } from "@sveltejs/kit";

export const load = async ({ params, parent, fetch }) => {
	const { meUser } = await parent();
	if (meUser) redirect(303, `/@${meUser.username}`);

	const urls = await apiRequest<AuthUrls>("POST", "/auth/urls", { fetch, isInternal: true });
	if (!urls.email_enabled) redirect(303, "/");

	return { state: params.code };
};

export const actions = {
	default: async ({ request, fetch }) => {
		const data = await request.formData();
		const state = data.get("state") as string;
		const password = data.get("password") as string;
		const password2 = data.get("confirm-password") as string;
		if (password !== password2) {
			return {
				ok: false,
				error: {
					status: 400,
					message: "Passwords don't match",
					code: ErrorCode.BadRequest,
				} as RawApiError,
			};
		}

		try {
			await fastRequest("POST", "/auth/email/reset-password", {
				body: { state, password },
				isInternal: true,
				fetch,
			});

			return { ok: true, error: null };
		} catch (e) {
			if (e instanceof ApiError) return { ok: false, error: e.obj };
			log.error("error resetting password:", e);
			throw e;
		}
	},
};
