import {
	CustomPreference,
	defaultPreferences,
	FieldEntry,
	PreferenceSize,
	Pronoun,
} from "~/lib/api/user";
import classNames from "classnames";
import { ReactNode } from "react";
import StatusIcon from "~/components/StatusIcon";

export default function StatusLine({
	entry,
	preferences,
	children,
}: {
	entry: FieldEntry | Pronoun;
	preferences: Record<string, CustomPreference>;
	children: ReactNode;
}) {
	const mergedPrefs = Object.assign({}, defaultPreferences, preferences);
	const currentPref =
		entry.status in mergedPrefs ? mergedPrefs[entry.status] : defaultPreferences.missing;

	const classes = classNames({
		"text-muted": currentPref.muted,
		"fw-bold fs-5": currentPref.size == PreferenceSize.Large,
		"fs-6": currentPref.size == PreferenceSize.Small,
	});

	return (
		<span className={classes}>
			<StatusIcon preferences={preferences} status={entry.status} /> {children}
		</span>
	);
}
