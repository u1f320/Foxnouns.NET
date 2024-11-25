import { apiRequest } from "$api";
import type { PartialMember, UserWithMembers } from "$api/models";

const MEMBERS_PER_PAGE = 20;

export const load = async ({ params, fetch, cookies, url }) => {
	const user = await apiRequest<UserWithMembers>("GET", `/users/${params.username}`, {
		fetch,
		cookies,
	});

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
