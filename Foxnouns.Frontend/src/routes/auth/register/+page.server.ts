import { apiRequest, fastRequest } from "$api";
import ApiError from "$api/error.js";
import type { AuthUrls } from "$api/models/auth";
import log from "$lib/log.js";
import { redirect } from "@sveltejs/kit";

export const load = async ({ fetch, parent }) => {
	const parentData = await parent();
	if (parentData.meUser) redirect(303, `/@${parentData.meUser.username}`);

	const urls = await apiRequest<AuthUrls>("POST", "/auth/urls", { fetch, isInternal: true });
	if (!urls.email_enabled) redirect(303, "/");
};

export const actions = {
	default: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const email = body.get("email") as string;

		try {
			await fastRequest("POST", `/auth/email/register/init`, {
				body: { email },
				isInternal: true,
				fetch,
				cookies,
			});

			return { ok: true, error: null };
		} catch (e) {
			if (e instanceof ApiError) return { ok: false, error: e.obj };
			log.error("error initiating registration for email %s:", email, e);
			throw e;
		}
	},
};
