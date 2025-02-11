import { fastRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error.js";
import log from "$lib/log.js";
import { error, isRedirect, redirect } from "@sveltejs/kit";

export const load = async ({ parent, params }) => {
	const data = await parent();
	if (data.user.auth_methods.length < 2) {
		error(403, {
			message: "You cannot remove your last authentication method.",
			status: 403,
			code: ErrorCode.LastAuthMethod,
		});
	}

	const authMethod = data.meUser!.auth_methods.find((m) => m.id === params.id);
	if (!authMethod) {
		error(404, {
			message: "No authentication method with that ID found.",
			status: 404,
			code: ErrorCode.GenericApiError,
		});
	}

	return { authMethod };
};

export const actions = {
	default: async ({ params, fetch, cookies }) => {
		try {
			fastRequest("DELETE", "/auth/methods/" + params.id, { fetch, cookies, isInternal: true });
			redirect(303, "/settings/auth?alert=auth-method-remove-success");
		} catch (e) {
			if (isRedirect(e)) throw e;
			if (e instanceof ApiError) return { error: e.obj };
			log.error("Could not remove auth method %s:", params.id, e);
			throw e;
		}
	},
};
