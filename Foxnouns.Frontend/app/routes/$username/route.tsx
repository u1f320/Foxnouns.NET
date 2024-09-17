import { json, LoaderFunctionArgs, MetaFunction } from "@remix-run/node";
import { redirect, useLoaderData } from "@remix-run/react";
import { UserWithMembers } from "~/lib/api/user";
import serverRequest from "~/lib/request.server";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	const { user } = data!;

	return [{ title: `@${user.username} - pronouns.cc` }];
};

export const loader = async ({ request, params }: LoaderFunctionArgs) => {
	const url = new URL(request.url);
	const memberPage = parseInt(url.searchParams.get("page") ?? "0", 10);

	let username = params.username!;
	if (!username.startsWith("@")) throw redirect(`/@${username}`);
	username = username.substring("@".length);

	const user = await serverRequest<UserWithMembers>("GET", `/users/${username}`);
	let members = user.members.slice(memberPage * 20, (memberPage + 1) * 20);
	if (members.length === 0) members = user.members.slice(0, 20);

	return json({ user, members });
};

export default function UserPage() {
	const { user } = useLoaderData<typeof loader>();

	return (
		<>
			hello! this is the user page for @{user.username}. their ID is {user.id}
		</>
	);
}
