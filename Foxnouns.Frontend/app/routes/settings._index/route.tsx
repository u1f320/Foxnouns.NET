import { Table } from "react-bootstrap";
import { useTranslation } from "react-i18next";
import { useRouteLoaderData } from "@remix-run/react";
import { loader as settingsLoader } from "../settings/route";

export default function SettingsIndex() {
	const { user } = useRouteLoaderData<typeof settingsLoader>("routes/settings")!;
	const { t } = useTranslation();
	
	return <>
		<Table striped bordered>
			<tr>
				<th scope="row">{t("settings.general.id")}</th>
				<td><code>{user.id}</code></td>
			</tr>
		</Table>
	</>
}
