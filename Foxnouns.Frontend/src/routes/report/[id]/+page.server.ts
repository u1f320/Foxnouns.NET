import { apiRequest, fastRequest } from "$api";
import ApiError from "$api/error.js";
import type { Member } from "$api/models/member.js";
import { type CreateReportRequest, ReportReason } from "$api/models/moderation.js";
import type { PartialUser, User } from "$api/models/user.js";
import log from "$lib/log.js";
import { redirect } from "@sveltejs/kit";

export const load = async ({ parent, params, fetch, cookies, url }) => {
	const { meUser } = await parent();
	if (!meUser) redirect(303, "/");

	let user: PartialUser;
	let member: Member | null = null;
	if (url.searchParams.has("member")) {
		const resp = await apiRequest<Member>(
			"GET",
			`/users/${params.id}/members/${url.searchParams.get("member")}`,
			{ fetch, cookies },
		);

		user = resp.user;
		member = resp;
	} else {
		user = await apiRequest<User>("GET", `/users/${params.id}`, { fetch, cookies });
	}

	if (meUser.id === user.id) redirect(303, "/");

	return { user, member };
};

export const actions = {
	default: async ({ request, fetch, cookies }) => {
		const body = await request.formData();

		const targetIsMember = body.get("target-type") === "member";
		const target = body.get("target-id") as string;
		const reason = body.get("reason") as ReportReason;
		const context = body.get("context") as string | null;

		const url = targetIsMember
			? `/moderation/report-member/${target}`
			: `/moderation/report-user/${target}`;

		try {
			await fastRequest<CreateReportRequest>("POST", url, {
				body: { reason, context },
				fetch,
				cookies,
			});

			return { ok: true, error: null };
		} catch (e) {
			if (e instanceof ApiError) return { ok: false, error: e.obj };
			log.error("error reporting user or member %s:", target, e);
			throw e;
		}
	},
};
