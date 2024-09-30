import { parse as parseCookie, serialize as serializeCookie } from "cookie";
import { API_BASE } from "~/env.server";
import { ApiError, ErrorCode } from "./api/error";
import { tokenCookieName } from "~/lib/utils";

export type RequestParams = {
	token?: string;
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	body?: any;
	headers?: Record<string, string>;
};

export default async function serverRequest<T>(
	method: string,
	path: string,
	params: RequestParams = {},
) {
	const url = `${API_BASE}/v2${path}`;
	const resp = await fetch(url, {
		method,
		body: params.body ? JSON.stringify(params.body) : undefined,
		headers: {
			...params.headers,
			...(params.token ? { Authorization: params.token } : {}),
			"Content-Type": "application/json",
		},
	});

	if (resp.headers.get("Content-Type")?.indexOf("application/json") === -1) {
		// If we don't get a JSON response, the server almost certainly encountered an internal error it couldn't recover from
		// (that, or the reverse proxy, which should also be treated as a 500 error)
		throw {
			status: 500,
			code: ErrorCode.InternalServerError,
			message: "Internal server error",
		} as ApiError;
	}

	if (resp.status < 200 || resp.status >= 400) throw (await resp.json()) as ApiError;
	return (await resp.json()) as T;
}

export const getToken = (req: Request) => getCookie(req, tokenCookieName);

export function getCookie(req: Request, cookieName: string): string | undefined {
	const header = req.headers.get("Cookie");
	if (!header) return undefined;

	const cookie = parseCookie(header);
	return cookieName in cookie ? cookie[cookieName] : undefined;
}

const YEAR = 365 * 86400;

export const writeCookie = (cookieName: string, value: string, maxAge: number | undefined = YEAR) =>
	serializeCookie(cookieName, value, {
		maxAge,
		path: "/",
		sameSite: "lax",
		httpOnly: true,
		secure: true,
	});
