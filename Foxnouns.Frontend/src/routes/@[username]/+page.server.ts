import { apiRequest } from "$api";
import type { UserWithMembers } from "$api/models";

export const load = async ({ params, fetch, cookies, url }) => {
	const user = await apiRequest<UserWithMembers>("GET", `/users/${params.username}`, {
		fetch,
		cookies,
	});

	// Paginate members on the server side
	let currentPage = Number(url.searchParams.get("page") || "0");
	const pageCount = Math.ceil(user.members.length / 20);
	let members = user.members.slice(currentPage * 20, (currentPage + 1) * 20);
	if (members.length === 0) {
		members = user.members.slice(0, 20);
		currentPage = 0;
	}

	return { user, members, currentPage, pageCount };
};
