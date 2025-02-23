import { apiRequest } from "$api";
import type { Report } from "$api/models/moderation";

export const load = async ({ url, fetch, cookies }) => {
	const before = url.searchParams.get("before");
	const after = url.searchParams.get("after");
	const byReporter = url.searchParams.get("by-reporter");
	const byTarget = url.searchParams.get("by-target");
	const includeClosed = url.searchParams.get("include-closed") === "true";

	const params = new URLSearchParams();
	if (before) params.set("before", before);
	if (after) params.set("after", after);
	if (byReporter) params.set("by-reporter", byReporter);
	if (byTarget) params.set("by-target", byTarget);
	if (includeClosed) params.set("include-closed", "true");

	const reports = await apiRequest<Report[]>("GET", `/moderation/reports?${params.toString()}`, {
		fetch,
		cookies,
	});
	return { reports, url: url.toString(), includeClosed, byReporter, byTarget, before, after };
};
