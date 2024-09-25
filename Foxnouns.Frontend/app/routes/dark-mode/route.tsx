import { ActionFunction } from "@remix-run/node";
import { UserSettings } from "~/lib/api/user";
import serverRequest, { getCookie, writeCookie } from "~/lib/request.server";

// Handles theme switching
// Remix itself handles redirecting back to the original page after the setting is set
//
// Note: this function is currently unused. Bootstrap only lets us switch themes with either prefers-color-scheme
// *or* a programmatic switch using data-bs-theme, not both.
// If the Sec-CH-Prefers-Color-Scheme header (https://caniuse.com/mdn-http_headers_sec-ch-prefers-color-scheme)
// is added to Firefox and Safari, the dark mode setting should be reworked to use it instead.
// As it stands, using prefers-color-scheme is the only way
// to respect the operating system's dark mode setting without using JavaScript.
export const action: ActionFunction = async ({ request }) => {
	const body = await request.formData();
	const theme = (body.get("theme") as string | null) || "auto";

	const token = getCookie(request, "pronounscc-token");
	if (token) {
		await serverRequest<UserSettings>("PATCH", "/users/@me/settings", {
			token,
			body: {
				dark_mode: theme === "auto" ? null : theme === "dark",
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
