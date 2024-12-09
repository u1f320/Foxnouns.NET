import paginate from "$lib/paginate";

const MEMBERS_PER_PAGE = 15;

export const load = async ({ url, parent }) => {
	const { user } = await parent();

	const { data, currentPage, pageCount } = paginate(
		user.members,
		url.searchParams.get("page"),
		MEMBERS_PER_PAGE,
	);

	return { members: data, currentPage, pageCount };
};
