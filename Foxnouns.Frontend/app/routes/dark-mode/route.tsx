import { ActionFunction } from "@remix-run/node";
import { UserSettings } from "~/lib/api/user";
import serverRequest, { getCookie, writeCookie } from "~/lib/request.server";

// Handles theme switching
// Remix itself handles redirecting back to the original page after the setting is set
export const action: ActionFunction = async ({ request }) => {
	const body = await request.formData();
	const theme = (body.get("theme") as string | null) || "auto";

	const token = getCookie(request, "pronounscc-token");
	if (token) {
		await serverRequest<UserSettings>("PATCH", "/users/@me/settings", {
			token,
			body: {
				dark_mode: theme === "auto" ? null : theme === "dark" ? true : false,
			},
		});

		return new Response(null, {
			status: 204,
		});
	}

	return new Response(null, {
		headers: {
			"Set-Cookie": writeCookie("pronounscc-theme", theme),
		},
		status: 204,
	});
};
