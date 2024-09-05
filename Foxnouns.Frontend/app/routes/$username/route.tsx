import { json, LoaderFunction, MetaFunction } from "@remix-run/node";
import { redirect, useLoaderData } from "@remix-run/react";
import { User } from "~/lib/api/user";
import serverRequest from "~/lib/request.server";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	const { user } = data!;

	return [{ title: `@${user.username} - pronouns.cc` }];
};

export const loader: LoaderFunction = async ({ params }) => {
	let username = params.username!;
	if (!username.startsWith("@")) throw redirect(`/@${username}`);
	username = username.substring("@".length);

	const user = await serverRequest<User>("GET", `/users/${username}`);

	return json({ user });
};

export default function UserPage() {
	const { user } = useLoaderData<typeof loader>();

	return (
		<>
			hello! this is the user page for @{user.username}. their ID is {user.id}
		</>
	);
}
