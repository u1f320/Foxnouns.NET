export type Meta = {
	repository: string;
	version: string;
	hash: string;
	users: {
		total: number;
		active_month: number;
		active_week: number;
		active_day: number;
	};
	members: number;
	limits: Limits;
	notice: { id: string; message: string } | null;
};

export type Limits = {
	member_count: number;
	bio_length: number;
	custom_preferences: number;
	max_auth_methods: number;
	max_flags: number;
};
