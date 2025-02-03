import { apiRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error";
import type { Report } from "$api/models/moderation";
import { idTimestamp } from "$lib";
import { redirect } from "@sveltejs/kit";

export const load = async ({ parent, fetch, cookies }) => {
	const { meUser } = await parent();
	if (!meUser) redirect(303, "/");

	if (meUser.role !== "ADMIN" && meUser.role !== "MODERATOR") {
		throw new ApiError({
			status: 403,
			code: ErrorCode.Forbidden,
			message: "Only admins and moderators can use this page.",
		});
	}

	const reports = await apiRequest<Report[]>("GET", "/moderation/reports", { fetch, cookies });
	const staleReportCount = reports.filter(
		(r) => idTimestamp(r.id).diffNow(["days"]).days <= -7,
	).length;

	return {
		user: meUser,
		isAdmin: meUser.role === "ADMIN",
		reportCount: reports.length,
		staleReportCount,
	};
};
