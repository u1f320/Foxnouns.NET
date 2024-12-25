import { baseRequest } from "$api";
import ApiError from "$api/error";

export const load = async ({ fetch, params }) => {
	const resp = await baseRequest("GET", `/meta/page/${params.page}`, { fetch });
	if (resp.status < 200 || resp.status > 299) {
		const err = await resp.json();
		if ("code" in err) throw new ApiError(err);
		else throw new ApiError();
	}

	const pageText = await resp.text();
	return { page: params.page, text: pageText };
};
