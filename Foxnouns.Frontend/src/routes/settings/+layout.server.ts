import { redirect } from "@sveltejs/kit";

export const load = async ({ parent }) => {
	const data = await parent();
	if (!data.meUser) redirect(303, "/auth/log-in?alert=auth-required");

	return { user: data.meUser!, token: data.token! };
};
