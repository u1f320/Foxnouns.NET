/* eslint-disable @typescript-eslint/no-explicit-any */
export type ApiError = {
	status: number;
	message: string;
	code: ErrorCode;
	errors?: ValidationError[];
};

export enum ErrorCode {
	InternalServerError = "INTERNAL_SERVER_ERROR",
	Forbidden = "FORBIDDEN",
	BadRequest = "BAD_REQUEST",
	AuthenticationError = "AUTHENTICATION_ERROR",
	AuthenticationRequired = "AUTHENTICATION_REQUIRED",
	MissingScopes = "MISSING_SCOPES",
	GenericApiError = "GENERIC_API_ERROR",
	UserNotFound = "USER_NOT_FOUND",
	MemberNotFound = "MEMBER_NOT_FOUND",
}

export type ValidationError = {
	message: string;
	min_length?: number;
	max_length?: number;
	actual_length?: number;
	allowed_values?: any[];
	actual_value?: any;
};
