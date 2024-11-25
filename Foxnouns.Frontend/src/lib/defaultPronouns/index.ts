import enPronouns from "./en";

const defaultPronouns = {
	en: enPronouns,
} as Record<string, Record<string, { pronouns: string[]; display?: string }>>;

export default defaultPronouns;
