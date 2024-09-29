import { json, LoaderFunctionArgs, MetaFunction } from "@remix-run/node";
import { Link, redirect, useLoaderData, useRouteLoaderData } from "@remix-run/react";
import { UserWithMembers } from "~/lib/api/user";
import serverRequest from "~/lib/request.server";
import { loader as rootLoader } from "~/root";
import { Alert, Button, Pagination } from "react-bootstrap";
import { Trans, useTranslation } from "react-i18next";
import { renderMarkdown } from "~/lib/markdown";
import ProfileLink from "~/components/profile/ProfileLink";
import ProfileField from "~/components/profile/ProfileField";
import { PersonPlusFill } from "react-bootstrap-icons";
import { defaultAvatarUrl } from "~/lib/utils";
import MemberCard from "~/routes/$username/MemberCard";
import { ReactNode } from "react";
import ProfileFlag from "~/components/profile/ProfileFlag";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	const { user } = data!;

	return [{ title: `@${user.username} • pronouns.cc` }];
};

export const loader = async ({ request, params }: LoaderFunctionArgs) => {
	const url = new URL(request.url);
	let memberPage = parseInt(url.searchParams.get("page") ?? "0", 10);

	let username = params.username!;
	if (!username.startsWith("@")) throw redirect(`/@${username}`);
	username = username.substring("@".length);

	const user = await serverRequest<UserWithMembers>("GET", `/users/${username}`);
	const pageCount = Math.ceil(user.members.length / 20);
	let members = user.members.slice(memberPage * 20, (memberPage + 1) * 20);
	if (members.length === 0) {
		members = user.members.slice(0, 20);
		memberPage = 0;
	}

	return json({ user, members, currentPage: memberPage, pageCount });
};

export default function UserPage() {
	const { t } = useTranslation();
	const { user, members, currentPage, pageCount } = useLoaderData<typeof loader>();
	const { meUser } = useRouteLoaderData<typeof rootLoader>("root") || { meUser: undefined };

	const isMeUser = meUser && meUser.id === user.id;
	const bio = renderMarkdown(user.bio);

	const paginationItems: ReactNode[] = [];
	for (let i = 0; i < pageCount; i++) {
		paginationItems.push(
			<Pagination.Item key={i} as={Link} to={`/@${user.username}?page=${i}`}>
				{i + 1}
			</Pagination.Item>,
		);
	}

	const pagination = (
		<Pagination className="justify-content-center">
			<Pagination.Prev
				as={Link}
				to={`/@${user.username}?page=${currentPage - 1}`}
				disabled={currentPage === 0}
			/>
			{paginationItems}
			<Pagination.Next
				as={Link}
				to={`/@${user.username}?page=${currentPage + 1}`}
				disabled={currentPage === pageCount - 1}
			/>
		</Pagination>
	);

	return (
		<>
			{isMeUser && (
				<Alert variant="secondary">
					<Trans t={t} i18nKey="user.own-profile-alert">
						You are currently viewing your <strong>public</strong> profile.
						<br />
						<Link to="/edit/profile">Edit your profile</Link>
					</Trans>
				</Alert>
			)}
			<div className="grid row-gap-3">
				<div className="row">
					<div className="col-md-4 text-center">
						<img
							src={user.avatar_url || defaultAvatarUrl}
							alt={t("user.avatar-alt", { username: user.username })}
							width={200}
							height={200}
							className="rounded-circle img-fluid"
						/>
						{user.flags && user.bio && (
							<div className="d-flex flex-wrap m-4">
								{user.flags.map((f, i) => (
									<ProfileFlag flag={f} key={i} />
								))}
							</div>
						)}
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
						<ProfileField
							name={t("user.heading.names")}
							entries={user.names}
							preferences={user.custom_preferences}
						/>
					)}
					{user.pronouns.length > 0 && (
						<ProfileField
							name={t("user.heading.pronouns")}
							entries={user.pronouns}
							preferences={user.custom_preferences}
						/>
					)}
					{user.fields.map((f, i) => (
						<ProfileField
							name={f.name}
							entries={f.entries}
							preferences={user.custom_preferences}
							key={i}
						/>
					))}
				</div>
			</div>
			{/* If a user or member has no bio, flags are displayed in a row below the other profile info, rather than just below the avatar */}
			{user.flags && !user.bio && (
				<div className="d-flex flex-wrap m-4">
					{user.flags.map((f, i) => (
						<ProfileFlag flag={f} key={i} />
					))}
				</div>
			)}
			{(members.length > 0 || isMeUser) && (
				<>
					<hr />
					<h2>
						{user.member_title || t("user.heading.members")}{" "}
						{isMeUser && (
							// @ts-expect-error using as=Link causes an error here, even though it runs completely fine
							<Button as={Link} to="/settings/members/create" variant="success">
								<PersonPlusFill /> {t("user.create-member-button")}
							</Button>
						)}
					</h2>
					{pageCount > 1 && pagination}
					<div className="grid">
						{members.length === 0 ? (
							<div>
								<Trans t={t} i18nKey="user.no-members-blurb">
									You don&apos;t have any members yet.
									<br />
									Members are sub-profiles that can have their own avatar, names, pronouns, and
									preferred terms.
									<br />
									You can create a new member with the &quot;Create member&quot; button above.{" "}
									<span className="text-muted">(only you can see this)</span>
								</Trans>
							</div>
						) : (
							<div className="row row-cols-2 row-cols-md-3 row-cols-lg-4 row-cols-xl-5 text-center">
								{members.map((member, i) => (
									<MemberCard user={user} member={member} key={i} />
								))}
							</div>
						)}
					</div>
					{pageCount > 1 && pagination}
				</>
			)}
		</>
	);
}
