import { PRIVATE_API_BASE } from "$env/static/private";
import { PUBLIC_API_BASE } from "$env/static/public";

export async function handle({ event, resolve }) {
	event.locals.token = event.cookies.get("pronounscc-token");
	return await resolve(event);
}

export function handleFetch({ event, request, fetch }) {
	if (request.url.startsWith(PUBLIC_API_BASE))
		request = new Request(request.url.replace(PUBLIC_API_BASE, PRIVATE_API_BASE), request);
	if (event.locals.token) request.headers.set("Authorization", event.locals.token);

	return fetch(request);
}
