import { apiRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error.js";
import type { Member } from "$api/models/member";
import log from "$lib/log.js";
import { error } from "@sveltejs/kit";

export const load = async ({ params, fetch, cookies }) => {
	try {
		const member = await apiRequest<Member>(
			"GET",
			`/users/${params.username}/members/${params.memberName}`,
			{
				fetch,
				cookies,
			},
		);

		return { member };
	} catch (e) {
		if (e instanceof ApiError) {
			if (e.code === ErrorCode.UserNotFound) error(404, "User not found");
			if (e.code === ErrorCode.MemberNotFound) error(404, "Member not found");
		}

		log.error("Error fetching user %s/member %s:", params.username, params.memberName, e);
		throw e;
	}
};
