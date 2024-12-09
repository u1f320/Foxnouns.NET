import createCallbackLoader from "$lib/actions/callback";
import createRegisterAction from "$lib/actions/register";

export const load = createCallbackLoader("google");

export const actions = {
	default: createRegisterAction("/auth/google/register"),
};
