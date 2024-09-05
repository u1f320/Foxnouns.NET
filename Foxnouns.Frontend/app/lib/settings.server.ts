import { UserSettings } from "./api/user";
import { getCookie } from "./request.server";

export default function getLocalSettings(req: Request): UserSettings {
	const settings = { dark_mode: null } as UserSettings;
	const theme = getCookie(req, "pronounscc-theme");

	switch (theme) {
		case "auto":
			settings.dark_mode = null;
			break;
		case "light":
			settings.dark_mode = false;
			break;
		case "dark":
			settings.dark_mode = true;
			break;
		default:
			break;
	}

	return settings;
}
