import { fastRequest } from "$api";
import { clearToken } from "$lib";
import { redirect } from "@sveltejs/kit";

export const actions = {
	default: async ({ fetch, cookies }) => {
		await fastRequest("POST", "/auth/force-log-out", { isInternal: true, fetch, cookies }, true);
		clearToken(cookies);
		redirect(303, "/");
	},
};
