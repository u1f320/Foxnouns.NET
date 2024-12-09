import { PRIVATE_API_HOST, PRIVATE_INTERNAL_API_HOST } from "$env/static/private";
import { PUBLIC_API_BASE } from "$env/static/public";
import type { HandleFetch } from "@sveltejs/kit";

export const handleFetch: HandleFetch = async ({ request, fetch }) => {
	console.log(PUBLIC_API_BASE, PRIVATE_INTERNAL_API_HOST, PRIVATE_API_HOST);
	if (request.url.startsWith(`${PUBLIC_API_BASE}/internal`)) {
		request = new Request(request.url.replace(PUBLIC_API_BASE, PRIVATE_INTERNAL_API_HOST), request);
	} else if (request.url.startsWith(PUBLIC_API_BASE)) {
		request = new Request(request.url.replace(PUBLIC_API_BASE, PRIVATE_API_HOST), request);
	}

	return await fetch(request);
};
