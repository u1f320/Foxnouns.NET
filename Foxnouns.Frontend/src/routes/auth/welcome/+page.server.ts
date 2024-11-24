import { redirect } from "@sveltejs/kit";

export const load = async ({ parent }) => {
	const { meUser } = await parent();
	if (!meUser) redirect(303, "/auth/log-in");
};
