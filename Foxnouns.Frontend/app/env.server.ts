import { env } from "node:process";

export const API_BASE = env.API_BASE || "https://pronouns.localhost/api";
export const LANGUAGE = env.LANGUAGE || "en";
