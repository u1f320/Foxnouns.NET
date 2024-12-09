import { apiRequest, fastRequest } from "$api";
import ApiError from "$api/error";
import type { PrideFlag } from "$api/models/user";
import log from "$lib/log";
import { encode } from "base64-arraybuffer";

export const load = async ({ url, fetch, cookies }) => {
	const resp = await apiRequest<PrideFlag[]>("GET", "/users/@me/flags", { fetch, cookies });

	return {
		flags: resp,
	};
};

export const actions = {
	upload: async ({ request, fetch, cookies }) => {
		const body = await request.formData();
		const name = body.get("name") as string;
		const description = body.get("desc") as string;
		const image = body.get("image") as File;

		const buffer = await image.arrayBuffer();
		const base64 = encode(buffer);

		try {
			await fastRequest("POST", "/users/@me/flags", {
				body: {
					name,
					description: description ? description : null,
					image: `data:${image.type};base64,${base64}`,
				},
				fetch,
				cookies,
			});
			return { ok: true, error: null };
		} catch (e) {
			if (e instanceof ApiError) return { ok: false, error: e.obj };
			log.error("error uploading flag:", e);
			throw e;
		}
	},
};
