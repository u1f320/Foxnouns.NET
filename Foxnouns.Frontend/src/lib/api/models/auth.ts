import type { AuthType, User } from "./user";

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
	email_enabled: boolean;
	discord?: string;
	google?: string;
	tumblr?: string;
};

export type AddAccountResponse = {
	id: string;
	type: AuthType;
	remote_id: string;
	remote_username?: string;
};
