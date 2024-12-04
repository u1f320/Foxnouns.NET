import ApiError, { ErrorCode } from "$api/error";
import createCallbackLoader from "$lib/actions/callback";
import createRegisterAction from "$lib/actions/register";

export const load = createCallbackLoader("fediverse", async ({ params, url }) => {
	const code = url.searchParams.get("code") as string | null;
	const state = url.searchParams.get("state") as string | null;
	if (!code || !state) throw new ApiError(undefined, ErrorCode.BadRequest).obj;

	return { code, state, instance: params.instance! };
});

export const actions = {
	default: createRegisterAction("/auth/fediverse/register"),
};
