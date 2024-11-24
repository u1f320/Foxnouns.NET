import type { Field, PartialMember, PartialUser, PrideFlag } from "./user";

export type Member = PartialMember & {
	fields: Field[];
	flags: PrideFlag[];
	links: string[];

	user: PartialUser;
};
