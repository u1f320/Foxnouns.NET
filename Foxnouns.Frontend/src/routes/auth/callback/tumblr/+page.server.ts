import createCallbackLoader from "$lib/actions/callback";
import createRegisterAction from "$lib/actions/register";

export const load = createCallbackLoader("tumblr");

export const actions = {
	default: createRegisterAction("/auth/tumblr/register"),
};
