import { apiRequest } from "$api";
import type { UserWithMembers } from "$api/models";
import paginate from "$lib/paginate";

const MEMBERS_PER_PAGE = 20;

export const load = async ({ params, fetch, cookies, url }) => {
	const user = await apiRequest<UserWithMembers>("GET", `/users/${params.username}`, {
		fetch,
		cookies,
	});

	const { data, currentPage, pageCount } = paginate(
		user.members,
		url.searchParams.get("page"),
		MEMBERS_PER_PAGE,
	);

	return { user, members: data, currentPage, pageCount };
};
