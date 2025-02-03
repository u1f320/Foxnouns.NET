import { apiRequest } from "$api";
import ApiError, { ErrorCode, type RawApiError } from "$api/error";
import type { AuditLogEntry, ClearableField } from "$api/models/moderation";
import log from "$lib/log";
import { type RequestEvent } from "@sveltejs/kit";

type ModactionResponse = { ok: boolean; resp: AuditLogEntry | null; error: RawApiError | null };
type ModactionFunction = (evt: RequestEvent) => Promise<ModactionResponse>;

export default function createModactionAction(
	type: "ignore" | "warn" | "suspend",
	requireReason: boolean,
): ModactionFunction {
	return async function ({ request, fetch, cookies }) {
		const body = await request.formData();
		const userId = body.get("user") as string;
		const memberId = body.get("member") as string | null;
		const reportId = body.get("report") as string | null;
		const reason = body.get("reason") as string | null;

		if (!reportId && type === "ignore") {
			return {
				ok: false,
				resp: null,
				error: {
					status: 400,
					message: "Bad request",
					code: ErrorCode.BadRequest,
					errors: [
						{ key: "report", errors: [{ message: "Ignoring a report requires a report ID" }] },
					],
				} satisfies RawApiError,
			};
		}

		if (!reason && requireReason) {
			return {
				ok: false,
				resp: null,
				error: {
					status: 400,
					message: "Bad request",
					code: ErrorCode.BadRequest,
					errors: [{ key: "reason", errors: [{ message: "You must give a reason" }] }],
				} satisfies RawApiError,
			};
		}

		let clearFields: ClearableField[] | undefined = undefined;
		if (type === "warn") {
			clearFields = body.getAll("clear-fields") as ClearableField[];
		}

		let path: string;
		if (type === "warn") path = `/moderation/warnings/${userId}`;
		else if (type === "suspend") path = `/moderation/suspensions/${userId}`;
		else path = `/moderation/reports/${reportId}/ignore`;

		try {
			const resp = await apiRequest<AuditLogEntry>("POST", path, {
				fetch,
				cookies,
				body: {
					reason: reason,
					// These are ignored by POST /reports/{id}/ignore
					member_id: memberId,
					report_id: reportId,
					// This is ignored by everything but POST /warnings/{id}
					clear_fields: clearFields,
					// This is ignored by everything but POST /suspensions/{id}
					clear_profile: !!body.get("clear-profile"),
				},
			});

			return { ok: true, resp, error: null };
		} catch (e) {
			if (e instanceof ApiError) return { ok: false, error: e.obj, resp: null };
			log.error("could not take action on %s:", path, e);
			throw e;
		}
	};
}

export function createModactions() {
	return {
		ignore: createModactionAction("ignore", false),
		warn: createModactionAction("warn", true),
		suspend: createModactionAction("suspend", true),
	};
}
