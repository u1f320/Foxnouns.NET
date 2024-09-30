import "dotenv/config";
import { env } from "node:process";
import { Limits } from "~/lib/api/meta";

export const API_BASE = env.API_BASE || "https://pronouns.localhost/api";
export const LANGUAGE = env.LANGUAGE || "en";

const apiLimits: Limits = await fetch(`${API_BASE}/v2/meta`)
	.then((resp) => resp.json())
	.then((m) => m.limits);
export const limits: Limits = Object.freeze(apiLimits);
