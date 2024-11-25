const MEMBERS_PER_PAGE = 15;

export const load = async ({ url, parent }) => {
	const { user } = await parent();

	let currentPage = Number(url.searchParams.get("page") || "0");
	let pageCount = Math.ceil(user.members.length / MEMBERS_PER_PAGE);
	let members = user.members.slice(
		currentPage * MEMBERS_PER_PAGE,
		(currentPage + 1) * MEMBERS_PER_PAGE,
	);
	if (members.length === 0) {
		members = user.members.slice(0, MEMBERS_PER_PAGE);
		currentPage = 0;
	}

	return { members, currentPage, pageCount };
};
