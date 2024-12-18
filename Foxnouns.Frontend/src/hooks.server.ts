import ApiError, { ErrorCode } from "$api/error";
import { PRIVATE_API_HOST, PRIVATE_INTERNAL_API_HOST } from "$env/static/private";
import { PUBLIC_API_BASE } from "$env/static/public";
import log from "$lib/log";
import type { HandleFetch, HandleServerError } from "@sveltejs/kit";

export const handleFetch: HandleFetch = async ({ request, fetch }) => {
	if (request.url.startsWith(`${PUBLIC_API_BASE}/internal`)) {
		request = new Request(request.url.replace(PUBLIC_API_BASE, PRIVATE_INTERNAL_API_HOST), request);
	} else if (request.url.startsWith(PUBLIC_API_BASE)) {
		request = new Request(request.url.replace(PUBLIC_API_BASE, PRIVATE_API_HOST), request);
	}

	return await fetch(request);
};

export const handleError: HandleServerError = async ({ error, status, message }) => {
	const id = crypto.randomUUID();

	if (error instanceof ApiError) {
		return {
			id,
			status: error.raw?.status || status,
			message: error.raw?.message || "Unknown error",
			code: error.code,
		};
	}

	if (status >= 400 && status <= 499) {
		return { id, status, message, code: ErrorCode.GenericApiError };
	}

	log.error("[%s] error in handler:", id, error);

	return { id, status, message, code: ErrorCode.InternalServerError };
};
