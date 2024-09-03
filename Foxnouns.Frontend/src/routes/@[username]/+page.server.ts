import { error } from "@sveltejs/kit";
import type { User } from "$lib/api/user";
import request from "$lib/request";

export const load = async ({ params, fetch }) => {
	try {
		const user = await request<User>(fetch, "GET", `/users/${params.username}`);
		return { user };
	} catch {
		error(404, { message: "User not found" });
	}
};
