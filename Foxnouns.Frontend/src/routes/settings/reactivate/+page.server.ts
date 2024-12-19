import { fastRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error";
import { redirect } from "@sveltejs/kit";

export const load = async ({ parent, fetch, cookies }) => {
	const { meUser } = await parent();
	if (!meUser) redirect(303, "/");

	if (meUser.suspended || !meUser.deleted)
		throw new ApiError({
			message: "You cannot use this page.",
			status: 403,
			code: ErrorCode.Forbidden,
		});

	await fastRequest("POST", "/self-delete/undelete", {
		fetch,
		cookies,
		isInternal: true,
	});

	return { user: meUser! };
};
