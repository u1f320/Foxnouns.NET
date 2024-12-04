import { apiRequest } from "$api";
import { redirect } from "@sveltejs/kit";

export const actions = {
	add: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const instance = body.get("instance") as string;

		const { url } = await apiRequest<{ url: string }>(
			"GET",
			`/auth/fediverse/add-account?instance=${encodeURIComponent(instance)}`,
			{
				isInternal: true,
				fetch,
				cookies,
			},
		);

		redirect(303, url);
	},
	forceRefresh: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const instance = body.get("instance") as string;

		const { url } = await apiRequest<{ url: string }>(
			"GET",
			`/auth/fediverse/add-account?instance=${encodeURIComponent(instance)}&forceRefresh=true`,
			{
				isInternal: true,
				fetch,
				cookies,
			},
		);

		redirect(303, url);
	},
};
