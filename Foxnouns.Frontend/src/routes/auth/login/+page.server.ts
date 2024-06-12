import request from "$lib/request.js";

type UrlsResponse = {
	discord: string | null;
	google: string | null;
	tumblr: string | null;
};

export const load = async ({ fetch }) => {
	const urls = await request<UrlsResponse>(fetch, "POST", "/auth/urls");
	return { urls };
};
