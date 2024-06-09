import { PUBLIC_API_BASE } from "$env/static/public";

export type RequestParams = {
	token?: string;
	body?: any;
	headers?: Record<string, string>;
};

/**
 * Fetch a path from the API and parse the response.
 * To make sure the request is authenticated in load functions,
 * pass `fetch` from the request object into opts.
 *
 * @param fetchFn A function like `fetch`, such as from the `load` function
 * @param method The HTTP method, i.e. GET, POST, PATCH
 * @param path The path to request, minus the leading `/api/v2`
 * @param params Extra options for this request
 * @returns T
 * @throws APIError
 */
export default async function request<T>(
	fetchFn: typeof fetch,
	method: string,
	path: string,
	params: RequestParams = {},
) {
	const url = `${PUBLIC_API_BASE}/v2${path}`;
	const resp = await fetchFn(url, {
		method,
		body: params.body ? JSON.stringify(params.body) : undefined,
		headers: {
			...params.headers,
			...(params.token ? { Authorization: params.token } : {}),
			"Content-Type": "application/json",
		},
	});

	if (resp.status < 200 || resp.status >= 400) throw await resp.json();
	return (await resp.json()) as T;
}

/**
 * Fetch a path from the API and discard the response.
 * To make sure the request is authenticated in load functions,
 * pass `fetch` from the request object into opts.
 *
 * @param fetchFn A function like `fetch`, such as from the `load` function
 * @param method The HTTP method, i.e. GET, POST, PATCH
 * @param path The path to request, minus the leading `/api/v2`
 * @param params Extra options for this request
 * @returns T
 * @throws APIError
 */
export async function fastRequest(
	fetchFn: typeof fetch,
	method: string,
	path: string,
	params: RequestParams = {},
): Promise<void> {
	const url = `${PUBLIC_API_BASE}/v2${path}`;
	const resp = await fetchFn(url, {
		method,
		body: params.body ? JSON.stringify(params.body) : undefined,
		headers: {
			...params.headers,
			...(params.token ? { Authorization: params.token } : {}),
			"Content-Type": "application/json",
		},
	});

	if (resp.status < 200 || resp.status >= 400) throw await resp.json();
}
