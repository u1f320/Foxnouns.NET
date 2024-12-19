import { fastRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import { clearToken } from "$lib";
import { redirect } from "@sveltejs/kit";

export const load = async ({ parent }) => {
	const { meUser } = await parent();
	if (!meUser) redirect(303, "/");

	if (meUser.deleted)
		throw new ApiError({
			message: "You cannot use this page.",
			status: 403,
			code: ErrorCode.Forbidden,
		});

	return { user: meUser! };
};

export const actions = {
	default: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const username = body.get("username") as string;
		const currentUsername = body.get("current-username") as string;

		if (!username || username !== currentUsername) {
			return {
				ok: false,
				error: {
					message: "Username doesn't match your username.",
					status: 400,
					code: ErrorCode.BadRequest,
				} as RawApiError,
			};
		}

		await fastRequest("POST", "/self-delete/delete", { fetch, cookies, isInternal: true });
		clearToken(cookies);
		redirect(303, "/settings/delete/success");
	},
};
