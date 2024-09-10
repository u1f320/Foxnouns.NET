import { User } from "~/lib/api/user";

export type AuthResponse = {
	user: User;
	token: string;
	expires_at: string;
};
