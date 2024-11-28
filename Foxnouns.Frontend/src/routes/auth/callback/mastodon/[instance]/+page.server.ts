import { apiRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error";
import type { CallbackResponse } from "$api/models/auth.js";
import { setToken } from "$lib";
import createRegisterAction from "$lib/actions/register.js";
import { redirect } from "@sveltejs/kit";

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
	default: createRegisterAction("/auth/fediverse/register"),
};
