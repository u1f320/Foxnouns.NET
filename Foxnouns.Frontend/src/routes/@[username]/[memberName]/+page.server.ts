import { apiRequest } from "$api";
import type { Member } from "$api/models/member";

export const load = async ({ params, fetch, cookies }) => {
	const member = await apiRequest<Member>(
		"GET",
		`/users/${params.username}/members/${params.memberName}`,
		{
			fetch,
			cookies,
		},
	);

	return { member };
};
