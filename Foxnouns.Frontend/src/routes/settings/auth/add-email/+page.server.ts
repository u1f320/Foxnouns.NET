import { fastRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error.js";
import log from "$lib/log.js";
import { redirect } from "@sveltejs/kit";

export const load = async ({ parent }) => {
	const { user } = await parent();
	return { firstEmail: user.auth_methods.filter((a) => a.type === "EMAIL").length === 0 };
};

export const actions = {
	add: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const email = body.get("email") as string;
		const password = body.get("password") as string | null;
		const password2 = body.get("confirm-password") as string | null;

		if (password2 && password !== password2) {
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
			await fastRequest("POST", "/auth/email/add-account", {
				body: { email, password },
				isInternal: true,
				fetch,
				cookies,
			});

			return { ok: true, error: null };
		} catch (e) {
			if (e instanceof ApiError) return { ok: false, error: e.obj };
			log.error("error adding email address to account:", e);
			throw e;
		}
	},
};
