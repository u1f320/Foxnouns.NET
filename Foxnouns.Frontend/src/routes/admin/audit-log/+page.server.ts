import { apiRequest } from "$api";
import { type AuditLogEntity, type AuditLogEntry } from "$api/models/moderation.js";

export const load = async ({ url, fetch, cookies }) => {
	const type = url.searchParams.get("type");
	const before = url.searchParams.get("before");
	const after = url.searchParams.get("after");
	const byModerator = url.searchParams.get("by-moderator");
	let limit: number = 100;
	if (url.searchParams.has("limit")) limit = parseInt(url.searchParams.get("limit")!);

	const params = new URLSearchParams();
	params.set("limit", limit.toString());
	if (type) params.set("type", type);
	if (before) params.set("before", before);
	if (after) params.set("after", after);
	if (byModerator) params.set("by-moderator", byModerator);

	const entries = await apiRequest<AuditLogEntry[]>(
		"GET",
		`/moderation/audit-log?${params.toString()}`,
		{
			fetch,
			cookies,
		},
	);

	const moderators = await apiRequest<AuditLogEntity[]>("GET", "/moderation/audit-log/moderators", {
		fetch,
		cookies,
	});

	let modFilter: AuditLogEntity | null = null;
	if (byModerator)
		modFilter = entries.find((e) => e.moderator.id === byModerator)?.moderator || null;

	return { entries, type, before, after, modFilter, url: url.toString(), moderators };
};
