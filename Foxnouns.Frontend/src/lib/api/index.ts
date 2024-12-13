import { PUBLIC_API_BASE } from "$env/static/public";
import type { Cookies } from "@sveltejs/kit";
import ApiError, { ErrorCode } from "./error";
import { TOKEN_COOKIE_NAME } from "$lib";
import log from "$lib/log";

export type Method = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

/**
 * Optional arguments for a request. `load` and `action` functions should always pass `fetch` and `cookies`.
 */
export type RequestArgs = {
	/**
	 * The token for this request. Where possible, `cookies` should be passed instead.
	 * Will override `cookies` if both are passed.
	 */
	token?: string;
	/**
	 * Whether this request is to an internal endpoint.
	 * Internal requests bypass the rate limiter and are prefixed with /api/internal/ rather than /api/v2/.
	 */
	isInternal?: boolean;
	/**
	 * The body for this request, which will be serialized to JSON. Should be a plain JS object.
	 */
	body?: unknown;
	/**
	 * The fetch function to use. Should be passed in loader and action functions, but can be safely ignored for client-side requests.
	 */
	fetch?: typeof fetch;
	/**
	 * The cookies object to try to get the token from. Can only be passed in loader and action functions.
	 */
	cookies?: Cookies;
};

/**
 * Makes a raw request to the API.
 * @param method The HTTP method for this request
 * @param path The path for this request, without the /api/v2 prefix, starting with a slash.
 * @param args Optional arguments to the request function.
 * @returns A Response object.
 */
export async function baseRequest(
	method: Method,
	path: string,
	args: RequestArgs = {},
): Promise<Response> {
	const token = args.token ?? args.cookies?.get(TOKEN_COOKIE_NAME);

	const fetchFn = args.fetch ?? fetch;
	const url = `/${args.isInternal ? "internal" : "v2"}${path}`;

	log.debug("Sending request to %s %s", method, url);

	const headers = {
		...(args.body ? { "Content-Type": "application/json; charset=utf-8" } : {}),
		...(token ? { Authorization: token } : {}),
	};

	return await fetchFn(PUBLIC_API_BASE + url, {
		method,
		headers,
		body: args.body ? JSON.stringify(args.body) : undefined,
	});
}

/**
 * Makes a request to the API and parses the returned object.
 * @param method The HTTP method for this request
 * @param path The path for this request, without the /api/v2 prefix, starting with a slash.
 * @param args Optional arguments to the request function.
 * @returns The response deserialized as `T`.
 */
export async function apiRequest<T>(
	method: Method,
	path: string,
	args: RequestArgs = {},
): Promise<T> {
	const resp = await baseRequest(method, path, args);

	if (resp.status < 200 || resp.status > 299) {
		const err = await resp.json();
		if ("code" in err) throw new ApiError(err);
		else throw new ApiError();
	}
	return (await resp.json()) as T;
}

/**
 * Makes a request without reading the body (unless the API returns an error).
 * @param method The HTTP method for this request
 * @param path The path for this request, without the /api/v2 prefix, starting with a slash.
 * @param args Optional arguments to the request function.
 * @param enforce204 Whether to throw an error on a non-204 status code.
 */
export async function fastRequest(
	method: Method,
	path: string,
	args: RequestArgs = {},
	enforce204: boolean = false,
): Promise<void> {
	const resp = await baseRequest(method, path, args);

	if (resp.status < 200 || resp.status > 299) {
		const err = await resp.json();
		if ("code" in err) throw new ApiError(err);
		else throw new ApiError();
	}

	if (enforce204 && resp.status !== 204) throw new ApiError(undefined, ErrorCode.Non204Response);
}
