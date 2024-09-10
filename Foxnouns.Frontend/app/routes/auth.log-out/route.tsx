import { ActionFunction } from "@remix-run/node";
import { writeCookie } from "~/lib/request.server";

export const action: ActionFunction = async () => {
	return new Response(null, {
		headers: {
			"Set-Cookie": writeCookie("pronounscc-token", "token", 0),
		},
		status: 204,
	});
};
