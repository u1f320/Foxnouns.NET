import i18n from "~/i18next.server";
import { LoaderFunctionArgs, MetaFunction } from "@remix-run/node";
import { Link, useRouteLoaderData } from "@remix-run/react";
import { Button, ListGroup } from "react-bootstrap";
import { loader as settingsLoader } from "~/routes/settings/route";
import { useTranslation } from "react-i18next";
import { AuthMethod, MeUser } from "~/lib/api/user";

export const meta: MetaFunction<typeof loader> = ({ data }) => {
	return [{ title: `${data?.meta.title || "Authentication"} • pronouns.cc` }];
};

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const t = await i18n.getFixedT(request);
	return { meta: { title: t("settings.auth.title") } };
};

export default function AuthSettings() {
	const { user } = useRouteLoaderData<typeof settingsLoader>("routes/settings")!;

	return (
		<div className="px-md-5">
			<EmailSettings user={user} />
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
