import { apiRequest } from "$api";
import ApiError from "$api/error";
import type { Member } from "$api/models";
import log from "$lib/log";
import { redirect } from "@sveltejs/kit";

export const load = async ({ parent, params, fetch, cookies }) => {
	const { meUser, token } = await parent();
	if (!meUser) redirect(303, "/");

	try {
		const member = await apiRequest<Member>("GET", `/users/@me/members/${params.id}`, {
			fetch,
			cookies,
		});
		return { user: meUser, token: token!, member };
	} catch (e) {
		if (e instanceof ApiError) throw e.obj;
		log.error("Error trying to fetch member %s:", params.id, e);
		throw e;
	}
};
