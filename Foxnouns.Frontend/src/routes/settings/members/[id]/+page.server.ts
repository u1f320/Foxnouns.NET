import { apiRequest, fastRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import type { Member } from "$api/models/member";
import log from "$lib/log.js";

export const load = async ({ params, fetch, cookies }) => {
	try {
		const member = await apiRequest<Member>("GET", `/users/@me/members/${params.id}`, {
			fetch,
			cookies,
		});
		return { member };
	} catch (e) {
		if (e instanceof ApiError) throw e.obj;
		log.error("Error trying to fetch member %s:", params.id, e);
		throw e;
	}
};

export const actions = {
	changeName: async ({ params, request, fetch, cookies }) => {
		const body = await request.formData();
		const name = body.get("name") as string | null;
		if (!name)
			return {
				error: {
					message: "You must pass a name.",
					status: 403,
					code: ErrorCode.BadRequest,
				} as RawApiError,
				ok: false,
			};

		try {
			await fastRequest("PATCH", `/users/@me/members/${params.id}`, {
				body: { name },
				fetch,
				cookies,
			});
			return { error: null, ok: true };
		} catch (e) {
			if (e instanceof ApiError) return { error: e.obj, ok: false };
			log.error("Error updating name for member %s:", params.id, e);
			throw e;
		}
	},
	changeDisplayName: async ({ params, request, fetch, cookies }) => {
		const body = await request.formData();
		let displayName = body.get("display-name") as string | null;
		if (!displayName || displayName === "") displayName = null;

		try {
			await fastRequest("PATCH", `/users/@me/members/${params.id}`, {
				body: { display_name: displayName },
				fetch,
				cookies,
			});
			return { error: null, ok: true };
		} catch (e) {
			if (e instanceof ApiError) return { error: e.obj, ok: false };
			log.error("Error updating name for member %s:", params.id, e);
			throw e;
		}
	},
	bio: async ({ params, request, fetch, cookies }) => {
		const body = await request.formData();
		let bio = body.get("bio") as string | null;
		if (!bio || bio === "") bio = null;

		try {
			await fastRequest("PATCH", `/users/@me/members/${params.id}`, {
				body: { bio },
				fetch,
				cookies,
			});
			return { error: null, ok: true };
		} catch (e) {
			if (e instanceof ApiError) return { error: e.obj, ok: false };
			log.error("Error updating bio for member %s:", params.id, e);
			throw e;
		}
	},
	options: async ({ params, request, fetch, cookies }) => {
		const body = await request.formData();
		const unlisted = !!body.get("unlisted");

		try {
			await fastRequest("PATCH", `/users/@me/members/${params.id}`, {
				body: { unlisted },
				fetch,
				cookies,
			});
			return { error: null, ok: true };
		} catch (e) {
			if (e instanceof ApiError) return { error: e.obj, ok: false };
			log.error("Error patching member %s:", params.id, e);
			throw e;
		}
	},
};
