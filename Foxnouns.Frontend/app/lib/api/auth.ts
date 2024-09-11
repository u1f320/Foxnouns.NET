import { User } from "~/lib/api/user";

export type AuthResponse = {
	user: User;
	token: string;
	expires_at: string;
};

export type AuthUrls = {
	discord?: string;
	google?: string;
	tumblr?: string;
};
