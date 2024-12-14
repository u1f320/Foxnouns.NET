import { apiRequest, fastRequest } from "$api";
import ApiError from "$api/error.js";
import type { AuthUrls } from "$api/models/auth";
import log from "$lib/log.js";
import { redirect } from "@sveltejs/kit";

export const load = async ({ parent, fetch }) => {
	const { meUser } = await parent();
	if (meUser) redirect(303, `/@${meUser.username}`);

	const urls = await apiRequest<AuthUrls>("POST", "/auth/urls", { fetch, isInternal: true });
	if (!urls.email_enabled) redirect(303, "/");
};

export const actions = {
	default: async ({ request, fetch }) => {
		const data = await request.formData();
		const email = data.get("email") as string;

		try {
			await fastRequest("POST", "/auth/email/forgot-password", {
				body: { email },
				isInternal: true,
				fetch,
			});

			return { ok: true, error: null };
		} catch (e) {
			if (e instanceof ApiError) return { ok: false, error: e.obj };
			log.error("error sending forget password email:", e);
			throw e;
		}
	},
};
