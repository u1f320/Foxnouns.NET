import { apiRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error";
import type { AddAccountResponse, CallbackResponse } from "$api/models/auth.js";
import { setToken } from "$lib";
import createRegisterAction from "$lib/actions/register.js";
import log from "$lib/log";
import { isRedirect, redirect } from "@sveltejs/kit";

export const load = async ({ parent, params, url, fetch, cookies }) => {
	const code = url.searchParams.get("code") as string | null;
	const state = url.searchParams.get("state") as string | null;
	if (!code || !state) throw new ApiError(undefined, ErrorCode.BadRequest).obj;

	const { meUser } = await parent();
	if (meUser) {
		try {
			const resp = await apiRequest<AddAccountResponse>(
				"POST",
				"/auth/fediverse/add-account/callback",
				{
					isInternal: true,
					body: { code, state, instance: params.instance },
					fetch,
					cookies,
				},
			);

			return { hasAccount: true, isLinkRequest: true, newAuthMethod: resp };
		} catch (e) {
			if (e instanceof ApiError) return { isLinkRequest: true, error: e.obj };
			log.error("error linking new fediverse account to user %s:", meUser.id, e);
			throw e;
		}
	}

	try {
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
			isLinkRequest: false,
			ticket: resp.ticket!,
			remoteUser: resp.remote_username!,
		};
	} catch (e) {
		if (isRedirect(e)) throw e;
		if (e instanceof ApiError) return { isLinkRequest: false, error: e.obj };
		log.error("error while requesting fediverse callback:", e);
		throw e;
	}
};

export const actions = {
	default: createRegisterAction("/auth/fediverse/register"),
};
