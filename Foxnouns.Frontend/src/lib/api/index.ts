import { PUBLIC_API_BASE } from "$env/static/public";
import type { Cookies } from "@sveltejs/kit";
import ApiError, { ErrorCode } from "./error";
import { TOKEN_COOKIE_NAME } from "$lib";
import log from "$lib/log";

export type Method = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

export type RequestArgs = {
	token?: string;
	isInternal?: boolean;
	body?: any;
	fetch?: typeof fetch;
	cookies?: Cookies;
};

/**
 * Makes a raw request to the API.
 * @param method The HTTP method for this request
 * @param path The path for this request, without the /api/v2 prefix, starting with a slash.
 * @param args Optional arguments to the request function.
 * @returns A Promise object.
 */
export async function baseRequest(
	method: Method,
	path: string,
	args: RequestArgs = {},
): Promise<Response> {
	const token = args.token ?? args.cookies?.get(TOKEN_COOKIE_NAME);

	const fetchFn = args.fetch ?? fetch;
	const url = `${PUBLIC_API_BASE}/${args.isInternal ? "internal" : "v2"}${path}`;

	log.debug("Sending request to %s %s", method, url);

	const headers = {
		...(args.body ? { "Content-Type": "application/json; charset=utf-8" } : {}),
		...(token ? { Authorization: token } : {}),
	};

	return await fetchFn(url, {
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
