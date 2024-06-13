import type Meta from "$lib/api/meta";
import type { User } from "$lib/api/user";
import request from "$lib/request";

export async function load({ fetch, locals }) {
	const meta = await request<Meta>(fetch, "GET", "/meta");
	let user: User | undefined;
	try {
		user = await request<User>(fetch, "GET", "/users/@me");
	} catch {}

	return { meta, user, token: locals.token };
}
