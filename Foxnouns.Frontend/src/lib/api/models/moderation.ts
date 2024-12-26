import type { Member } from "./member";
import type { PartialMember, PartialUser, User } from "./user";

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

export type Report = {
	id: string;
	reporter: PartialUser;
	target_user: PartialUser;
	target_member?: PartialMember;
	status: "OPEN" | "CLOSED";
	reason: ReportReason;
	context: string | null;
	target_type: "USER" | "MEMBER";
	snapshot: User | Member | null;
};

export type AuditLogEntry = {
	id: string;
	moderator: AuditLogEntity;
	target_user?: AuditLogEntity;
	target_member?: AuditLogEntity;
	report_id?: string;
	type: AuditLogEntryType;
	reason: string | null;
	cleared_fields?: string[];
};

export type AuditLogEntity = { id: string; username: string };

export enum AuditLogEntryType {
	IgnoreReport = "IGNORE_REPORT",
	WarnUser = "WARN_USER",
	WarnUserAndClearProfile = "WARN_USER_AND_CLEAR_PROFILE",
	SuspendUser = "SUSPEND_USER",
}
