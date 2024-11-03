import i18n from "~/i18next.server";
import { LoaderFunctionArgs, MetaFunction } from "@remix-run/node";
import { Link, useLoaderData, useRouteLoaderData } from "@remix-run/react";
import { Button, ListGroup } from "react-bootstrap";
import { loader as settingsLoader } from "~/routes/settings/route";
import { useTranslation } from "react-i18next";
import { AuthMethod, MeUser } from "~/lib/api/user";
import serverRequest from "~/lib/request.server";
import { AuthUrls } from "~/lib/api/auth";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	return [{ title: `${data?.meta.title || "Authentication"} • pronouns.cc` }];
};

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);
	const urls = await serverRequest<AuthUrls>("POST", "/auth/urls", { isInternal: true });

	return { urls, meta: { title: t("settings.auth.title") } };
};

export default function AuthSettings() {
	const { urls } = useLoaderData<typeof loader>();
	const { user } = useRouteLoaderData<typeof settingsLoader>("routes/settings")!;

	return (
		<div className="px-md-5">
			{urls.email_enabled && <EmailSettings user={user} />}
			{urls.discord && <DiscordSettings user={user} />}
			<FediverseSettings user={user} />
		</div>
	);
}

function EmailSettings({ user }: { user: MeUser }) {
	const { t } = useTranslation();
	const oneAuthMethod = user.auth_methods.length === 1;
	const emails = user.auth_methods.filter((m) => m.type === "EMAIL");

	return (
		<>
			<h3>{t("settings.auth.email-addresses")}</h3>
			{emails.length > 0 && (
				<>
					<ListGroup className="pt-2 pb-3">
						{emails.map((e) => (
							<EmailRow email={e} key={e.id} disabled={oneAuthMethod} />
						))}
					</ListGroup>
				</>
			)}
			{emails.length < 3 && (
				<p>
					{/* @ts-expect-error as=Link */}
					<Button variant="primary" as={Link} to="/settings/auth/add-email">
						{emails.length === 0
							? t("settings.auth.form.add-first-email")
							: t("settings.auth.form.add-extra-email")}
					</Button>
				</p>
			)}
		</>
	);
}

function EmailRow({ email, disabled }: { email: AuthMethod; disabled: boolean }) {
	const { t } = useTranslation();

	return (
		<ListGroup.Item>
			<div className="row">
				<div className="col">{email.remote_id}</div>
				{!disabled && (
					<div className="col text-end">
						<Link to={`/settings/auth/remove-method/${email.id}`}>
							{t("settings.auth.remove-auth-method")}
						</Link>
					</div>
				)}
			</div>
		</ListGroup.Item>
	);
}

function DiscordSettings({ user }: { user: MeUser }) {
	const { t } = useTranslation();
	const oneAuthMethod = user.auth_methods.length === 1;
	const discordAccounts = user.auth_methods.filter((m) => m.type === "DISCORD");

	return (
		<>
			<h3>{t("settings.auth.discord-accounts")}</h3>
			{discordAccounts.length > 0 && (
				<>
					<ListGroup className="pt-2 pb-3">
						{discordAccounts.map((a) => (
							<NonEmailRow account={a} key={a.id} disabled={oneAuthMethod} />
						))}
					</ListGroup>
				</>
			)}
			{discordAccounts.length < 3 && (
				<p>
					{/* @ts-expect-error as=Link */}
					<Button variant="primary" as={Link} to="/settings/auth/add-discord-account">
						{discordAccounts.length === 0
							? t("settings.auth.form.add-first-discord-account")
							: t("settings.auth.form.add-extra-discord-account")}
					</Button>
				</p>
			)}
		</>
	);
}

function FediverseSettings({ user }: { user: MeUser }) {
	const { t } = useTranslation();
	const oneAuthMethod = user.auth_methods.length === 1;
	const fediAccounts = user.auth_methods.filter((m) => m.type === "FEDIVERSE");

	return (
		<>
			<h3>{t("settings.auth.fediverse-accounts")}</h3>
			{fediAccounts.length > 0 && (
				<>
					<ListGroup className="pt-2 pb-3">
						{fediAccounts.map((a) => (
							<NonEmailRow account={a} key={a.id} disabled={oneAuthMethod} />
						))}
					</ListGroup>
				</>
			)}
			{fediAccounts.length < 3 && (
				<p>
					{/* @ts-expect-error as=Link */}
					<Button variant="primary" as={Link} to="/settings/auth/add-fediverse-account">
						{fediAccounts.length === 0
							? t("settings.auth.form.add-first-fediverse-account")
							: t("settings.auth.form.add-extra-fediverse-account")}
					</Button>
				</p>
			)}
		</>
	);
}

function NonEmailRow({ account, disabled }: { account: AuthMethod; disabled: boolean }) {
	const { t } = useTranslation();

	return (
		<ListGroup.Item>
			<div className="row">
				<div className="col">
					{account.remote_username} {account.type !== "FEDIVERSE" && <>({account.remote_id})</>}
				</div>
				{!disabled && (
					<div className="col text-end">
						<Link to={`/settings/auth/remove-method/${account.id}`}>
							{t("settings.auth.remove-auth-method")}
						</Link>
					</div>
				)}
			</div>
		</ListGroup.Item>
	);
}
