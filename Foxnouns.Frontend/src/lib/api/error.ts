/* eslint-disable @typescript-eslint/no-explicit-any */
export default class ApiError {
	raw?: RawApiError;
	code: ErrorCode;

	constructor(err?: RawApiError, code?: ErrorCode) {
		this.raw = err;
		this.code = err?.code || code || ErrorCode.InternalServerError;
	}

	get obj(): RawApiError {
		return this.toObject();
	}

	toObject(): RawApiError {
		return {
			status: this.raw?.status || 500,
			code: this.code,
			message: this.raw?.message || "Internal server error",
			errors: this.raw?.errors,
		};
	}
}

export type RawApiError = {
	status: number;
	message: string;
	code: ErrorCode;
	errors?: Array<{ key: string; errors: ValidationError[] }>;
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
	AccountAlreadyLinked = "ACCOUNT_ALREADY_LINKED",
	LastAuthMethod = "LAST_AUTH_METHOD",
	// This code isn't actually returned by the API
	Non204Response = "(non 204 response)",
}

export type ValidationError = {
	message: string;
	min_length?: number;
	max_length?: number;
	actual_length?: number;
	allowed_values?: any[];
	actual_value?: any;
};

/**
 * Returns the first error for the value `key` in `error`.
 * @param error The error object to traverse.
 * @param key The JSON key to find.
 */
export const firstErrorFor = (error: RawApiError, key: string): ValidationError | undefined => {
	if (!error.errors) return undefined;
	const field = error.errors.find((e) => e.key == key);
	if (!field?.errors) return undefined;
	return field.errors.length != 0 ? field.errors[0] : undefined;
};
