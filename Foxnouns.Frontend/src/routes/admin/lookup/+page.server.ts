import { apiRequest } from "$api";
import { redirect } from "@sveltejs/kit";

export const actions = {
	default: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const query = body.get("query") as string;
		const fuzzy = body.get("fuzzy") === "yes";

		const users = await apiRequest<Array<{ id: string; username: string }>>(
			"POST",
			"/moderation/lookup",
			{
				fetch,
				cookies,
				body: {
					query,
					fuzzy,
				},
			},
		);

		if (!fuzzy && users.length > 0) redirect(303, `/admin/lookup/${users[0].id}`);

		return { users };
	},
};
