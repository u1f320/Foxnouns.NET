import { apiRequest } from "$api";
import type { AuthUrls } from "$api/models/auth";

export const load = async ({ fetch }) => {
	const urls = await apiRequest<AuthUrls>("POST", "/auth/urls", { fetch, isInternal: true });
	return { urls };
};
