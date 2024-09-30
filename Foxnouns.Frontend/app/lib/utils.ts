import { DateTime } from "luxon";

export const defaultAvatarUrl = "https://pronouns.cc/default/512.webp";
export const tokenCookieName = "__Host-pronounscc-token";
export const idTimestamp = (id: string) =>
	DateTime.fromMillis(parseInt(id, 10) / (1 << 22) + 1_640_995_200_000);
