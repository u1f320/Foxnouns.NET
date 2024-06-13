import type { User } from "./user";

export type CallbackRequest = {
	code: string;
	state: string;
};

export type CallbackResponse = {
	has_account: boolean;
	ticket: string;
	remote_username: string | null;
};

export type AuthResponse = {
	user: User;
	token: string;
	expires_at: string;
};
