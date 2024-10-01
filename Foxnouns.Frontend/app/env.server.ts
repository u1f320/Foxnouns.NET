import "dotenv/config";
import { env } from "node:process";

export const API_BASE = env.API_BASE || "https://pronouns.localhost/api";
export const INTERNAL_API_BASE = env.INTERNAL_API_BASE || "https://localhost:5000/api";
export const LANGUAGE = env.LANGUAGE || "en";
