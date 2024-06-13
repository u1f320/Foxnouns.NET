import request from "$lib/request";
import type { AuthResponse, CallbackResponse } from "$lib/api/auth";

export const load = async ({ fetch, url, cookies, parent }) => {
	const data = await parent();
	if (data.user) {
		return { loggedIn: true, token: data.token, user: data.user };
	}

	const resp = await request<AuthResponse | CallbackResponse>(
		fetch,
		"POST",
		"/auth/discord/callback",
		{
			body: {
				code: url.searchParams.get("code"),
				state: url.searchParams.get("state"),
			},
		},
	);

	console.log(JSON.stringify(resp));

	if ("token" in resp) {
		const authResp = resp as AuthResponse;
		cookies.set("pronounscc-token", authResp.token, { path: "/" });
		return { loggedIn: true, token: authResp.token, user: authResp.user };
	}

	const callbackResp = resp as CallbackResponse;
	return {
		loggedIn: false,
		hasAccount: callbackResp.has_account,
		ticket: resp.ticket,
		remoteUsername: resp.remote_username,
	};
};

export const actions = {
	register: async ({ cookies, request: req, fetch, locals }) => {
		const data = await req.formData();
		const username = data.get("username");
		const ticket = data.get("ticket");

		console.log(JSON.stringify({ username, ticket }));

		const resp = await request<AuthResponse>(fetch, "POST", "/auth/discord/register", {
			body: { username, ticket },
		});
		cookies.set("pronounscc-token", resp.token, { path: "/" });
		locals.token = resp.token;

		return { token: resp.token, user: resp.user };
	},
};
