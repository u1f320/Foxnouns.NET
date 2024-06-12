import { fastRequest } from "$lib/request";

export const load = async ({ fetch, url }) => {
	await fastRequest(fetch, "POST", "/auth/discord/callback", {
		body: {
			code: url.searchParams.get("code"),
			state: url.searchParams.get("state"),
		},
	});
};
