import type { Member } from "./member";
import type { AuthMethod, PartialMember, PartialUser, User } from "./user";

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
	report?: PartialReport;
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
	QuerySensitiveUserData = "QUERY_SENSITIVE_USER_DATA",
}

export type PartialReport = {
	id: string;
	reporter_id: string;
	target_user_id: string;
	target_member_id?: string;
	reason: ReportReason;
	context: string | null;
	target_type: "USER" | "MEMBER";
};

export type QueriedUser = {
	user: User;
	member_list_hidden: boolean;
	last_active: string;
	last_sid_reroll: string;
	suspended: boolean;
	deleted: boolean;
	auth_methods?: AuthMethod[];
};
