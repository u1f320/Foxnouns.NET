import { ActionFunction } from "@remix-run/node";
import { writeCookie } from "~/lib/request.server";
import { tokenCookieName } from "~/lib/utils";

export const action: ActionFunction = async () => {
	return new Response(null, {
		headers: {
			"Set-Cookie": writeCookie(tokenCookieName, "token", 0),
		},
		status: 204,
	});
};
