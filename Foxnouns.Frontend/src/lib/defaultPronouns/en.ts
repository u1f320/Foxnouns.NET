const enPronouns = {
	"they/them": { pronouns: ["they", "them", "their", "theirs", "themself"] },
	"they/them (singular)": {
		pronouns: ["they", "them", "their", "theirs", "themself"],
		display: "they/them (singular)",
	},
	"they/them (plural)": {
		pronouns: ["they", "them", "their", "theirs", "themselves"],
		display: "they/them (plural)",
	},
	"he/him": { pronouns: ["he", "him", "his", "his", "himself"] },
	"she/her": { pronouns: ["she", "her", "her", "hers", "herself"] },
	"it/its": { pronouns: ["it", "it", "its", "its", "itself"], display: "it/its" },
} as Record<string, { pronouns: string[]; display?: string }>;

export default enPronouns;
