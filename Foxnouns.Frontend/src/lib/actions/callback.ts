import { apiRequest } from "$api";
import ApiError, { ErrorCode } from "$api/error";
import type { AddAccountResponse, CallbackResponse } from "$api/models";
import { setToken } from "$lib";
import log from "$lib/log";
import { isRedirect, redirect, type ServerLoadEvent } from "@sveltejs/kit";
import type { TicketData } from "../../routes/auth/callback/register/[ticket]/+page.server";

export default function createCallbackLoader(
	callbackType: string,
	bodyFn?: (event: ServerLoadEvent) => Promise<unknown>,
	returnData?: boolean,
) {
	return async (event: ServerLoadEvent) => {
		const { parent, fetch, cookies } = event;

		bodyFn ??= async ({ url }) => {
			const code = url.searchParams.get("code") as string | null;
			const state = url.searchParams.get("state") as string | null;
			if (!code || !state) throw new ApiError(undefined, ErrorCode.BadRequest).obj;
			return { code, state };
		};

		const { meUser } = await parent();
		if (meUser) {
			try {
				const resp = await apiRequest<AddAccountResponse>(
					"POST",
					`/auth/${callbackType}/add-account/callback`,
					{
						isInternal: true,
						body: await bodyFn(event),
						fetch,
						cookies,
					},
				);

				return { hasAccount: true, isLinkRequest: true, newAuthMethod: resp };
			} catch (e) {
				if (e instanceof ApiError) return { isLinkRequest: true, error: e.obj };
				log.error("error linking new %s account to user %s:", callbackType, meUser.id, e);
				throw e;
			}
		}

		try {
			const resp = await apiRequest<CallbackResponse>("POST", `/auth/${callbackType}/callback`, {
				body: await bodyFn(event),
				isInternal: true,
				fetch,
			});

			if (resp.has_account) {
				setToken(cookies, resp.token!);
				redirect(303, `/@${resp.user!.username}`);
			}

			if (returnData)
				return {
					ticket: resp.ticket!,
					remoteUser: resp.remote_username!,
				};

			const ticket = btoa(
				JSON.stringify({
					type: callbackType,
					ticket: resp.ticket!,
					remoteUsername: resp.remote_username!,
				} satisfies TicketData),
			)
				.replaceAll("+", "-")
				.replaceAll("/", "_");

			redirect(303, "/auth/callback/register/" + ticket);
		} catch (e) {
			if (isRedirect(e)) throw e;
			if (e instanceof ApiError) return { isLinkRequest: false, error: e.obj };
			log.error("error while requesting %s callback:", callbackType, e);
			throw e;
		}
	};
}
