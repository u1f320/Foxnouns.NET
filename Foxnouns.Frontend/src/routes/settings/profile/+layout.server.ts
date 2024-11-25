import { redirect } from "@sveltejs/kit";

export const load = async ({ parent }) => {
	const { meUser, token } = await parent();
	if (!meUser) redirect(303, "/");
	return { user: meUser!, token: token! };
};
