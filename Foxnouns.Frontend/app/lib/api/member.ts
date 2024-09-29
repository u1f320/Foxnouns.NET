import { Field, PartialMember, PartialUser, PrideFlag } from "~/lib/api/user";

export type Member = PartialMember & {
	fields: Field[];
	flags: PrideFlag[];
	links: string[];

	user: PartialUser;
};
