import { isRedirect, redirect } from "@sveltejs/kit";

import { apiRequest } from "$api";
import type { AuthResponse, AuthUrls } from "$api/models/auth";
import { setToken } from "$lib";
import ApiError, { ErrorCode } from "$api/error";

export const load = async ({ fetch, parent }) => {
	const parentData = await parent();
	if (parentData.meUser) redirect(303, `/@${parentData.meUser.username}`);

	const urls = await apiRequest<AuthUrls>("POST", "/auth/urls", { fetch, isInternal: true });
	return { urls };
};

export const actions = {
	login: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const email = body.get("email") as string | null;
		const password = body.get("password") as string | null;

		try {
			const resp = await apiRequest<AuthResponse>("POST", "/auth/email/login", {
				body: { email, password },
				fetch,
				isInternal: true,
			});

			setToken(cookies, resp.token);
			redirect(303, `/@${resp.user.username}`);
		} catch (e) {
			if (isRedirect(e)) throw e;

			if (e instanceof ApiError) return { error: e.obj };
			throw e;
		}
	},
	fediToggle: () => {
		return { error: null, showFediBox: true };
	},
	fedi: async ({ request, fetch }) => {
		const body = await request.formData();
		const instance = body.get("instance") as string | null;
		if (!instance) return { error: new ApiError(undefined, ErrorCode.BadRequest).obj };

		try {
			const resp = await apiRequest<{ url: string }>(
				"GET",
				`/auth/fediverse?instance=${encodeURIComponent(instance)}`,
				{ fetch, isInternal: true },
			);
			redirect(303, resp.url);
		} catch (e) {
			if (isRedirect(e)) throw e;

			if (e instanceof ApiError) return { error: e.obj };
			throw e;
		}
	},
	fediForceRefresh: async ({ request, fetch }) => {
		const body = await request.formData();
		const instance = body.get("instance") as string | null;
		if (!instance) return { error: new ApiError(undefined, ErrorCode.BadRequest).obj };

		try {
			const resp = await apiRequest<{ url: string }>(
				"GET",
				`/auth/fediverse?instance=${encodeURIComponent(instance)}&forceRefresh=true`,
				{ fetch, isInternal: true },
			);
			redirect(303, resp.url);
		} catch (e) {
			if (isRedirect(e)) throw e;

			if (e instanceof ApiError) return { error: e.obj };
			throw e;
		}
	},
};
