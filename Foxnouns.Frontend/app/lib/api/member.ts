import { PartialMember, PrideFlag } from "~/lib/api/user";

export type Member = PartialMember & {
	flags: PrideFlag[];
};
