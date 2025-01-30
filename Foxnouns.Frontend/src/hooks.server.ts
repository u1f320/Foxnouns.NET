import ApiError, { ErrorCode } from "$api/error";
import { PRIVATE_API_HOST, PRIVATE_INTERNAL_API_HOST } from "$env/static/private";
import { env } from "$env/dynamic/private";
import { PUBLIC_API_BASE } from "$env/static/public";
import log from "$lib/log";
import type { HandleFetch, HandleServerError } from "@sveltejs/kit";
import * as Sentry from "@sentry/sveltekit";

export const handleFetch: HandleFetch = async ({ request, fetch }) => {
	if (request.url.startsWith(`${PUBLIC_API_BASE}/internal`)) {
		request = new Request(request.url.replace(PUBLIC_API_BASE, PRIVATE_INTERNAL_API_HOST), request);
	} else if (request.url.startsWith(PUBLIC_API_BASE)) {
		request = new Request(request.url.replace(PUBLIC_API_BASE, PRIVATE_API_HOST), request);
	}

	return await fetch(request);
};

Sentry.init({
	dsn: env.PRIVATE_SENTRY_DSN,
});

export const handleError: HandleServerError = async ({ error, status, message }) => {
	// as far as i know, sentry IDs are just UUIDs with the dashes removed. use those here as well
	let id = crypto.randomUUID().replaceAll("-", "");

	if (error instanceof ApiError) {
		return {
			error_id: id,
			status: error.raw?.status || status,
			message: error.raw?.message || "Unknown error",
			code: error.code,
		};
	}

	if (status >= 400 && status <= 499) {
		return { error_id: id, status, message, code: ErrorCode.GenericApiError };
	}

	// client errors and backend API errors just clog up sentry, so we don't send those.
	id = Sentry.captureException(error, {
		mechanism: {
			type: "sveltekit",
			handled: false,
		},
	});

	log.error("[%s] error in handler:", id, error);

	return { error_id: id, status, message, code: ErrorCode.InternalServerError };
};
