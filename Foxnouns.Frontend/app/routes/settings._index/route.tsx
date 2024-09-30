import { Table } from "react-bootstrap";
import { useTranslation } from "react-i18next";
import { useLoaderData, useRouteLoaderData } from "@remix-run/react";
import { loader as settingsLoader } from "../settings/route";
import { LoaderFunctionArgs, json } from "@remix-run/node";
import serverRequest, { getToken } from "~/lib/request.server";
import { PartialMember } from "~/lib/api/user";
import { limits } from "~/env.server";
import { DateTime } from "luxon";
import { idTimestamp } from "~/lib/utils";

export const loader = async ({ request }: LoaderFunctionArgs) => {
	const token = getToken(request);
	const members = await serverRequest<PartialMember[]>("GET", "/users/@me/members", { token });
	return json({ members, maxMemberCount: limits.member_count });
};

export default function SettingsIndex() {
	const { members, maxMemberCount } = useLoaderData<typeof loader>();
	const { user } = useRouteLoaderData<typeof settingsLoader>("routes/settings")!;
	const { t } = useTranslation();

	const createdAt = idTimestamp(user.id);

	return (
		<>
			<Table striped bordered>
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
							{members.length}/{maxMemberCount}
						</td>
					</tr>
				</tbody>
			</Table>
		</>
	);
}
