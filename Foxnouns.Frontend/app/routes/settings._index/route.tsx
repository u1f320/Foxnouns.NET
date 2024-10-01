import { Button, Form, InputGroup, Table } from "react-bootstrap";
import { useTranslation } from "react-i18next";
import { Form as RemixForm, useActionData, useRouteLoaderData } from "@remix-run/react";
import { loader as settingsLoader } from "../settings/route";
import { loader as rootLoader } from "../../root";
import { DateTime } from "luxon";
import { defaultAvatarUrl, idTimestamp } from "~/lib/utils";
import { ExclamationTriangleFill, InfoCircleFill } from "react-bootstrap-icons";
import AvatarImage from "~/components/profile/AvatarImage";
import { ActionFunctionArgs, json } from "@remix-run/node";
import { type ApiError, ErrorCode, firstErrorFor } from "~/lib/api/error";
import serverRequest, { getToken } from "~/lib/request.server";
import { MeUser } from "~/lib/api/user";
import ErrorAlert from "~/components/ErrorAlert";

export const action = async ({ request }: ActionFunctionArgs) => {
	const data = await request.formData();
	const username = data.get("username") as string | null;
	const token = getToken(request);

	if (!username) {
		return json({
			error: {
				status: 403,
				code: ErrorCode.BadRequest,
				message: "Invalid username",
			} as ApiError,
			user: null,
		});
	}

	try {
		const resp = await serverRequest<MeUser>("PATCH", "/users/@me", { body: { username }, token });

		return json({ user: resp, error: null });
	} catch (e) {
		return json({ error: e as ApiError, user: null });
	}
};

export default function SettingsIndex() {
	const { user } = useRouteLoaderData<typeof settingsLoader>("routes/settings")!;
	const actionData = useActionData<typeof action>();
	const { meta } = useRouteLoaderData<typeof rootLoader>("root")!;
	const { t } = useTranslation();

	const createdAt = idTimestamp(user.id);

	return (
		<>
			<div className="row">
				<div className="col-md">
					<RemixForm method="POST">
						<Form as="div">
							<Form.Group className="mb-3" controlId="username">
								<Form.Label>{t("settings.general.username")}</Form.Label>
								<InputGroup className="m-1 w-75">
									<Form.Control
										defaultValue={user.username}
										id="username"
										name="username"
										type="text"
										required
									/>
									<Button variant="secondary" type="submit">
										{t("settings.general.change-username")}
									</Button>
								</InputGroup>
							</Form.Group>
						</Form>
					</RemixForm>

					<p className="text-muted text-has-newline">
						<InfoCircleFill /> {t("settings.general.username-change-hint")}
					</p>
					{actionData?.error && <UsernameUpdateError error={actionData.error} />}
				</div>
				<div className="col-md text-center">
					<AvatarImage
						src={user.avatar_url || defaultAvatarUrl}
						width={200}
						alt={t("user.avatar-alt", { username: user.username })}
					/>
				</div>
			</div>
			<div>
				<h4>{t("settings.general.log-out-everywhere")}</h4>
				<p></p>
			</div>
			<h4>{t("settings.general.table-header")}</h4>
			<Table striped bordered hover>
				<tbody>
					<tr>
						<th scope="row">{t("settings.general.id")}</th>
						<td>
							<code>{user.id}</code>
						</td>
					</tr>
					<tr>
						<th scope="row">{t("settings.general.created")}</th>
						<td>{createdAt.toLocaleString(DateTime.DATETIME_MED)}</td>
					</tr>
					<tr>
						<th scope="row">{t("settings.general.member-count")}</th>
						<td>
							{user.members.length}/{meta.limits.member_count}
						</td>
					</tr>
					<tr>
						<th scope="row">{t("settings.general.member-list-hidden")}</th>
						<td>{user.member_list_hidden ? t("yes") : t("no")}</td>
					</tr>
					<tr>
						<th scope="row">{t("settings.general.custom-preferences")}</th>
						<td>
							{Object.keys(user.custom_preferences).length}/{meta.limits.custom_preferences}
						</td>
					</tr>
					<tr>
						<th scope="row">{t("settings.general.role")}</th>
						<td>
							<code>{user.role}</code>
						</td>
					</tr>
				</tbody>
			</Table>
		</>
	);
}

function UsernameUpdateError({ error }: { error: ApiError }) {
	const { t } = useTranslation();
	const usernameError = firstErrorFor(error, "username");
	if (!usernameError) {
		return <ErrorAlert error={error} />;
	}

	return (
		<p className="text-danger-emphasis text-has-newline">
			<ExclamationTriangleFill />{" "}
			{t("settings.general.username-update-error", { message: usernameError.message })}
		</p>
	);
}
