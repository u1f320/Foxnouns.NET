import { apiRequest } from "$api";
import type { QueriedUser } from "$api/models/moderation";

export const load = async ({ params, fetch, cookies }) => {
	const user = await apiRequest<QueriedUser>("GET", `/moderation/lookup/${params.id}`, {
		fetch,
		cookies,
	});

	return { user };
};
