import createCallbackLoader from "$lib/actions/callback";
import createRegisterAction from "$lib/actions/register";

export const load = createCallbackLoader("discord");

export const actions = {
	default: createRegisterAction("/auth/discord/register"),
};
