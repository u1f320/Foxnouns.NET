export type CreateReportRequest = {
	reason: ReportReason;
	context: string | null;
};

export enum ReportReason {
	Totalitarianism = "TOTALITARIANISM",
	HateSpeech = "HATE_SPEECH",
	Racism = "RACISM",
	Homophobia = "HOMOPHOBIA",
	Transphobia = "TRANSPHOBIA",
	Queerphobia = "QUEERPHOBIA",
	Exclusionism = "EXCLUSIONISM",
	Sexism = "SEXISM",
	Ableism = "ABLEISM",
	ChildPornography = "CHILD_PORNOGRAPHY",
	PedophiliaAdvocacy = "PEDOPHILIA_ADVOCACY",
	Harassment = "HARASSMENT",
	Impersonation = "IMPERSONATION",
	Doxxing = "DOXXING",
	EncouragingSelfHarm = "ENCOURAGING_SELF_HARM",
	Spam = "SPAM",
	Trolling = "TROLLING",
	Advertisement = "ADVERTISEMENT",
	CopyrightViolation = "COPYRIGHT_VIOLATION",
}
