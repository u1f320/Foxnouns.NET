import { page } from "$app/state";

export const isActive = (path: string | string[], prefix: boolean = false) =>
	typeof path === "string"
		? prefix
			? page.url.pathname.startsWith(path)
			: page.url.pathname === path
		: prefix
			? path.some((p) => page.url.pathname.startsWith(p))
			: path.some((p) => page.url.pathname === p);
