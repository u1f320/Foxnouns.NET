import { ActionFunction, redirect } from "@remix-run/node";
import { fastRequest, getToken, writeCookie } from "~/lib/request.server";
import { tokenCookieName } from "~/lib/utils";

export const action: ActionFunction = async ({ request }) => {
	const token = getToken(request);
	if (!token)
		return redirect("/", {
			status: 303,
			headers: { "Set-Cookie": writeCookie(tokenCookieName, "token", 0) },
		});

	await fastRequest("POST", "/auth/force-log-out", { token, isInternal: true });

	return redirect("/", {
		status: 303,
		headers: { "Set-Cookie": writeCookie(tokenCookieName, "token", 0) },
	});
};
