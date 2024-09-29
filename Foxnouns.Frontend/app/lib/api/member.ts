import { Field, PartialMember, PrideFlag } from "~/lib/api/user";

export type Member = PartialMember & {
	fields: Field[];
	flags: PrideFlag[];
	links: string[];
};
