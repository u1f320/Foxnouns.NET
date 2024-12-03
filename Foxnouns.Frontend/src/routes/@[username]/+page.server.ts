import { apiRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error.js";
import type { PartialMember, User, UserWithMembers } from "$api/models";
import log from "$lib/log.js";
import { error } from "@sveltejs/kit";

const MEMBERS_PER_PAGE = 20;

export const load = async ({ params, fetch, cookies, url }) => {
	let user: UserWithMembers;

	try {
		user = await apiRequest<UserWithMembers>("GET", `/users/${params.username}`, {
			fetch,
			cookies,
		});
	} catch (e) {
		if (e instanceof ApiError && e.code === ErrorCode.UserNotFound) error(404, "User not found");
		log.error("Error fetching user %s:", params.username, e);
		throw e;
	}

	// Paginate members on the server side
	let currentPage = 0;
	let pageCount = 0;
	let members: PartialMember[] = [];
	if (user.members) {
		currentPage = Number(url.searchParams.get("page") || "0");
		pageCount = Math.ceil(user.members.length / MEMBERS_PER_PAGE);
		members = user.members.slice(
			currentPage * MEMBERS_PER_PAGE,
			(currentPage + 1) * MEMBERS_PER_PAGE,
		);
		if (members.length === 0) {
			members = user.members.slice(0, MEMBERS_PER_PAGE);
			currentPage = 0;
		}
	}

	return { user, members, currentPage, pageCount };
};
