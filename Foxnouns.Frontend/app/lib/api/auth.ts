import { User } from "~/lib/api/user";

export type AuthResponse = {
	user: User;
	token: string;
	expires_at: string;
};

export type CallbackResponse = {
	has_account: boolean;
	ticket?: string;
	remote_username?: string;
	user?: User;
	token?: string;
	expires_at?: string;
};

export type AuthUrls = {
	discord?: string;
	google?: string;
	tumblr?: string;
};
