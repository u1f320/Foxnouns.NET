// See https://svelte.dev/docs/kit/types#app.d.ts

import type { ErrorCode } from "$api/error";

// for information about these interfaces
declare global {
	namespace App {
		interface Error {
			message: string;
			status: number;
			code: ErrorCode;
			errors?: Array<{ key: string; errors: ValidationError[] }>;
			error_id?: string;
		}
		// interface Error {}
		// interface Locals {}
		// interface PageData {}
		// interface PageState {}
		// interface Platform {}
	}
}

export {};
