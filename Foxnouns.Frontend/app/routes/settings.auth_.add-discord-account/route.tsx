import { LoaderFunctionArgs, redirect, json } from "@remix-run/node";
import serverRequest, { getToken } from "~/lib/request.server";
import { ApiError } from "~/lib/api/error";
import { useLoaderData } from "@remix-run/react";
import ErrorAlert from "~/components/ErrorAlert";

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const token = getToken(request);

	try {
		const { url } = await serverRequest<{ url: string }>("GET", "/auth/discord/add-account", {
			isInternal: true,
			token,
		});

		return redirect(url, 303);
	} catch (e) {
		return json({ error: e as ApiError });
	}
};

export default function AddDiscordAccountPage() {
	const { error } = useLoaderData<typeof loader>();

	return <ErrorAlert error={error} />;
}
