import { json, LoaderFunctionArgs, MetaFunction } from "@remix-run/node";
import { Link, redirect, useLoaderData, useRouteLoaderData } from "@remix-run/react";
import { UserWithMembers } from "~/lib/api/user";
import serverRequest from "~/lib/request.server";
import { loader as rootLoader } from "~/root";
import { Alert, Button, Pagination } from "react-bootstrap";
import { Trans, useTranslation } from "react-i18next";
import { PersonPlusFill } from "react-bootstrap-icons";
import MemberCard from "~/routes/$username/MemberCard";
import { ReactNode } from "react";
import BaseProfile from "~/components/profile/BaseProfile";

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
				// @ts-expect-error using as=Link causes an error here, even though it runs completely fine
				to={`/@${user.username}?page=${currentPage - 1}`}
				disabled={currentPage === 0}
			/>
			{paginationItems}
			<Pagination.Next
				as={Link}
				// @ts-expect-error using as=Link causes an error here, even though it runs completely fine
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
						<Link to="/settings/profile">Edit your profile</Link>
					</Trans>
				</Alert>
			)}
			<BaseProfile
				name={user.username}
				userI18nKeys={true}
				profile={user}
				customPreferences={user.custom_preferences}
			/>
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
