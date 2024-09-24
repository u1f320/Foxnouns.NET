import { CustomPreference, defaultPreferences } from "~/lib/api/user";
import { OverlayTrigger, Tooltip } from "react-bootstrap";
import Icon from "~/components/KeyedIcon";

export default function StatusIcon({
	preferences,
	status,
}: {
	preferences: Record<string, CustomPreference>;
	status: string;
}) {
	const mergedPrefs = Object.assign({}, defaultPreferences, preferences);
	const currentPref = status in mergedPrefs ? mergedPrefs[status] : defaultPreferences.missing;

	const id = crypto.randomUUID();
	return (
		<>
			<OverlayTrigger
				key={id}
				placement="top"
				overlay={
					<Tooltip id={id} aria-hidden={true}>
						{currentPref.tooltip}
					</Tooltip>
				}
			>
				<span className="d-inline-block">
					<Icon iconName={currentPref.icon} aria-hidden={true} style={{ pointerEvents: "none" }} />
				</span>
			</OverlayTrigger>
			<span className="visually-hidden">{currentPref.tooltip}:</span>
		</>
	);
}
