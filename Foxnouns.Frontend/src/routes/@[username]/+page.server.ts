import { apiRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error.js";
import type { UserWithMembers } from "$api/models";
import log from "$lib/log.js";
import paginate from "$lib/paginate";
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

	const { data, currentPage, pageCount } = paginate(
		user.members,
		url.searchParams.get("page"),
		MEMBERS_PER_PAGE,
	);

	return { user, members: data, currentPage, pageCount };
};
