import { json, LoaderFunctionArgs, MetaFunction } from "@remix-run/node";
import { Link, redirect, useLoaderData, useRouteLoaderData } from "@remix-run/react";
import { UserWithMembers } from "~/lib/api/user";
import serverRequest from "~/lib/request.server";
import { loader as rootLoader } from "~/root";
import { Alert } from "react-bootstrap";
import { Trans, useTranslation } from "react-i18next";
import { renderMarkdown } from "~/lib/markdown";
import ProfileLink from "~/components/ProfileLink";
import StatusLine from "~/components/StatusLine";
import PronounLink from "~/components/PronounLink";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	const { user } = data!;

	return [{ title: `@${user.username} • pronouns.cc` }];
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
	const { t } = useTranslation();
	const { user } = useLoaderData<typeof loader>();
	const { meUser } = useRouteLoaderData<typeof rootLoader>("root") || { meUser: undefined };

	const bio = renderMarkdown(user.bio);

	return (
		<>
			{meUser && meUser.id === user.id && (
				<Alert variant="secondary">
					<Trans t={t} i18nKey="user.own-profile-alert">
						You are currently viewing your <strong>public</strong> profile.
						<br />
						<Link to={`/@${user.username}/edit`}>Edit your profile</Link>
					</Trans>
				</Alert>
			)}
			<div className="grid row-gap-3">
				<div className="row">
					<div className="col-md-4 text-center">
						<img
							src={user.avatar_url || "https://pronouns.cc/default/512.webp"}
							alt={t("user.avatar-alt", { username: user.username })}
							width={200}
							height={200}
							className="rounded-circle img-fluid"
						/>
					</div>
					<div className="col-md">
						{user.display_name ? (
							<>
								<h2>{user.display_name}</h2>
								<p className="fs-5 text-body-secondary">@{user.username}</p>
							</>
						) : (
							<>
								<h2>@{user.username}</h2>
							</>
						)}
						{bio && (
							<>
								<hr />
								<p dangerouslySetInnerHTML={{ __html: bio }}></p>
							</>
						)}
					</div>
					{user.links.length > 0 && (
						<div className="col-md d-flex align-items-center">
							<ul className="list-unstyled">
								{user.links.map((l, i) => (
									<ProfileLink link={l} key={i} />
								))}
							</ul>
						</div>
					)}
				</div>
				<div className="row row-cols-1 row-cols-sm-2 row-cols-md-3">
					{user.names.length > 0 && (
						<div className="col-md">
							<h3>{t("user.heading.names")}</h3>
							<ul className="list-unstyled fs-5">
								{user.names.map((n, i) => (
									<StatusLine entry={n} preferences={user.custom_preferences} key={i}>
										{n.value}
									</StatusLine>
								))}
							</ul>
						</div>
					)}
					{user.pronouns.length > 0 && (
						<div className="col-md">
							<h3>{t("user.heading.pronouns")}</h3>
							{user.pronouns.map((p, i) => (
								<StatusLine entry={p} preferences={user.custom_preferences} key={i}>
									<PronounLink pronoun={p} />
								</StatusLine>
							))}
						</div>
					)}
				</div>
			</div>
		</>
	);
}
