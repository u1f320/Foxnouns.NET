import { json, LoaderFunctionArgs, MetaFunction } from "@remix-run/node";
import { Link, redirect, useLoaderData, useRouteLoaderData } from "@remix-run/react";
import serverRequest from "~/lib/request.server";
import { Member } from "~/lib/api/member";
import BaseProfile from "~/components/profile/BaseProfile";
import { loader as rootLoader } from "~/root";
import { Alert, Button } from "react-bootstrap";
import { Trans, useTranslation } from "react-i18next";
import { ArrowLeft } from "react-bootstrap-icons";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	const { member } = data!;

	return [
		{ title: `${member.display_name ?? member.name} • @${member.user.username} • pronouns.cc` },
	];
};

export const loader = async ({ params }: LoaderFunctionArgs) => {
	let username = params.username!;
	const memberName = params.member!;
	if (!username.startsWith("@")) throw redirect(`/@${username}/${memberName}`);
	username = username.substring("@".length);

	const member = await serverRequest<Member>("GET", `/users/${username}/members/${memberName}`);
	return json({ member });
};

export default function MemberPage() {
	const { t } = useTranslation();
	const { member } = useLoaderData<typeof loader>();
	const { meUser } = useRouteLoaderData<typeof rootLoader>("root") || { meUser: undefined };
	const isMeUser = meUser && meUser.id === member.user.id;

	const memberName = member.name;

	return (
		<>
			{isMeUser && (
				<Alert variant="secondary">
					<Trans t={t} i18nKey="member.own-profile-alert" values={{ memberName: member.name }}>
						You are currently viewing the <strong>public</strong> profile of {{ memberName }}.
						<br />
						<Link to={`/settings/profile/${member.id}`}>Edit profile</Link>
					</Trans>
				</Alert>
			)}
			<div className="m-3">
				{/* @ts-expect-error using as=Link causes an error here, even though it runs completely fine */}
				<Button variant="secondary" as={Link} to={`/@${member.user.username}`}>
					<ArrowLeft />{" "}
					{t("member.back", { name: member.user.display_name ?? member.user.username })}
				</Button>
			</div>
			<BaseProfile
				name={member.name}
				fullName={`${member.name} (@${member.user.username})`}
				userI18nKeys={false}
				profile={member}
				customPreferences={member.user.custom_preferences}
			/>
		</>
	);
}
