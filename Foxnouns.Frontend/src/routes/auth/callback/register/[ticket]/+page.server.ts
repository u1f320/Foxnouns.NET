import { apiRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import type { AuthResponse } from "$api/models/auth";
import { setToken } from "$lib";
import log from "$lib/log";
import { isRedirect, redirect } from "@sveltejs/kit";

export type TicketData = {
	type: string;
	ticket: string;
	remoteUsername: string;
};

export const load = async ({ params }) => {
	const data = JSON.parse(atob(params.ticket)) as TicketData;
	return data;
};

export const actions = {
	default: async ({ request, fetch, cookies, params }) => {
		const type = (JSON.parse(atob(params.ticket)) as TicketData).type;

		const data = await request.formData();
		const username = data.get("username") as string | null;
		const ticket = data.get("ticket") as string | null;

		if (!username || !ticket)
			return {
				error: { message: "Bad request", code: ErrorCode.BadRequest, status: 403 } as RawApiError,
			};

		try {
			const resp = await apiRequest<AuthResponse>("POST", `/auth/${type}/register`, {
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
