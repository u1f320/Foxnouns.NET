import ApiError, { ErrorCode } from "$api/error";
import createCallbackLoader from "$lib/actions/callback";

export const load = createCallbackLoader("fediverse", async ({ params, url }) => {
	const code = url.searchParams.get("code") as string | null;
	const state = url.searchParams.get("state") as string | null;
	const token = url.searchParams.get("token") as string | null;
	if ((!code || !state) && !token) throw new ApiError(undefined, ErrorCode.BadRequest).obj;

	return { code: code || token, state, instance: params.instance! };
});
