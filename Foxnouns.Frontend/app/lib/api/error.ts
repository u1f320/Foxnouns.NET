/* eslint-disable @typescript-eslint/no-explicit-any */
export type ApiError = {
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
export const firstErrorFor = (error: ApiError, key: string): ValidationError | undefined => {
	if (!error.errors) return undefined;
	const field = error.errors.find((e) => e.key == key);
	if (!field?.errors) return undefined;
	return field.errors.length != 0 ? field.errors[0] : undefined;
};

export enum ValidationErrorType {
	LengthError = 0,
	DisallowedValueError = 1,
	GenericValidationError = 2,
}

export const validationErrorType = (error: ValidationError) => {
	if (error.min_length && error.max_length && error.actual_length) {
		return ValidationErrorType.LengthError;
	}
	if (error.allowed_values && error.actual_value) {
		return ValidationErrorType.DisallowedValueError;
	}
	return ValidationErrorType.GenericValidationError;
};
