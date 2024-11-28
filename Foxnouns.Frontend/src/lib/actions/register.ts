import { apiRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import type { AuthResponse } from "$api/models/auth";
import { setToken } from "$lib";
import log from "$lib/log";
import { isRedirect, redirect, type RequestEvent } from "@sveltejs/kit";

export default function createRegisterAction(callbackUrl: string) {
	return async function ({ request, fetch, cookies }: RequestEvent) {
		const data = await request.formData();
		const username = data.get("username") as string | null;
		const ticket = data.get("ticket") as string | null;

		if (!username || !ticket)
			return {
				error: { message: "Bad request", code: ErrorCode.BadRequest, status: 403 } as RawApiError,
			};

		try {
			const resp = await apiRequest<AuthResponse>("POST", callbackUrl, {
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
	};
}
