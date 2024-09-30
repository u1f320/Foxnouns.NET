export default interface Meta {
	version: string;
	hash: string;
	users: {
		total: number;
		active_month: number;
		active_week: number;
		active_day: number;
	};
	members: number;
}

export type Limits = {
	member_count: number;
	bio_length: number;
};
