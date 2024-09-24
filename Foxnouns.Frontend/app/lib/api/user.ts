export type PartialUser = {
	id: string;
	username: string;
	display_name?: string | null;
	avatar_url?: string | null;
};

export type User = PartialUser & {
	bio: string | null;
	member_title: string | null;
	links: string[];
	names: FieldEntry[];
	pronouns: Pronoun[];
	fields: Field[];
	custom_preferences: Record<string, CustomPreference>;
};

export type UserWithMembers = User & { members: PartialMember[] };

export type UserWithHiddenFields = User & {
	auth_methods?: unknown[];
	member_list_hidden: boolean;
	last_active: string;
};

export type UserSettings = {
	dark_mode: boolean | null;
};

export type PartialMember = {
	id: string;
	name: string;
	display_name: string | null;
	bio: string | null;
	avatar_url: string | null;
	names: FieldEntry[];
	pronouns: Pronoun[];
};

export type FieldEntry = {
	value: string;
	status: string;
};

export type Pronoun = FieldEntry & { display_text: string | null };

export type Field = {
	name: string;
	entries: FieldEntry[];
};

export type CustomPreference = {
	icon: string;
	tooltip: string;
	muted: boolean;
	favourite: boolean;
	size: PreferenceSize;
};

export enum PreferenceSize {
	Large = "LARGE",
	Normal = "NORMAL",
	Small = "SMALL",
}

export const defaultPreferences = Object.freeze({
	favourite: {
		icon: "heart-fill",
		tooltip: "Favourite",
		size: PreferenceSize.Large,
		muted: false,
		favourite: true,
	},
	okay: {
		icon: "hand-thumbs-up",
		tooltip: "Okay",
		size: PreferenceSize.Normal,
		muted: false,
		favourite: false,
	},
	jokingly: {
		icon: "emoji-laughing",
		tooltip: "Jokingly",
		size: PreferenceSize.Normal,
		muted: false,
		favourite: false,
	},
	friends_only: {
		icon: "people",
		tooltip: "Friends only",
		size: PreferenceSize.Normal,
		muted: false,
		favourite: false,
	},
	avoid: {
		icon: "hand-thumbs-down",
		tooltip: "Avoid",
		size: PreferenceSize.Small,
		muted: true,
		favourite: false,
	},
	missing: {
		icon: "question-lg",
		tooltip: "Unknown (missing)",
		size: PreferenceSize.Normal,
		muted: false,
		favourite: false,
	},
});
