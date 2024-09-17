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
