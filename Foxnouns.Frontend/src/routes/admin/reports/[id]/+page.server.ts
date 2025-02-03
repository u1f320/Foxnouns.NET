import { apiRequest } from "$api";
import type { ReportDetails } from "$api/models/moderation";
import { createModactions } from "$lib/actions/modaction";

export const load = async ({ params, fetch, cookies }) => {
	const resp = await apiRequest<ReportDetails>("GET", `/moderation/reports/${params.id}`, {
		fetch,
		cookies,
	});
	return {
		report: resp.report,
		user: resp.user,
		member: resp.member,
		auditLogEntry: resp.audit_log_entry,
	};
};

export const actions = createModactions();
