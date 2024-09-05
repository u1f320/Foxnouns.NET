export type User = {
	id: string;
	username: string;
	display_name: string | null;
	bio: string | null;
	member_title: string | null;
	avatar_url: string | null;
	links: string[];
};

export type UserSettings = {
	dark_mode: boolean | null;
};
