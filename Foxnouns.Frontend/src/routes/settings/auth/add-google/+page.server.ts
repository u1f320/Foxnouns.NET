import { apiRequest } from "$api";
import { redirect } from "@sveltejs/kit";

export const load = async ({ fetch, cookies }) => {
	const { url } = await apiRequest<{ url: string }>("GET", "/auth/google/add-account", {
		isInternal: true,
		fetch,
		cookies,
	});

	redirect(303, url);
};
