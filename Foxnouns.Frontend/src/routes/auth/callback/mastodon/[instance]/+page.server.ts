import { apiRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import type { AuthResponse, CallbackResponse } from "$api/models/auth.js";
import { setToken } from "$lib";
import log from "$lib/log.js";
import { isRedirect, redirect } from "@sveltejs/kit";

export const load = async ({ parent, params, url, fetch, cookies }) => {
	const { meUser } = await parent();
	if (meUser) redirect(303, `/@${meUser.username}`);

	const code = url.searchParams.get("code") as string | null;
	const state = url.searchParams.get("state") as string | null;
	if (!code || !state) throw new ApiError(undefined, ErrorCode.BadRequest).obj;

	const resp = await apiRequest<CallbackResponse>("POST", "/auth/fediverse/callback", {
		body: { code, state, instance: params.instance },
		isInternal: true,
		fetch,
	});

	if (resp.has_account) {
		setToken(cookies, resp.token!);
		redirect(303, `/@${resp.user!.username}`);
	}

	return {
		hasAccount: false,
		instance: params.instance,
		ticket: resp.ticket!,
		remoteUser: resp.remote_username!,
	};
};

export const actions = {
	default: async ({ request, fetch, cookies }) => {
		const data = await request.formData();
		const username = data.get("username") as string | null;
		const ticket = data.get("ticket") as string | null;

		if (!username || !ticket)
			return {
				error: { message: "Bad request", code: ErrorCode.BadRequest, status: 403 } as RawApiError,
			};

		try {
			const resp = await apiRequest<AuthResponse>("POST", "/auth/fediverse/register", {
				body: { username, ticket },
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
