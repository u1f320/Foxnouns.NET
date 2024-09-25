import {
	CustomPreference,
	defaultPreferences,
	FieldEntry,
	mergePreferences,
	PreferenceSize,
	Pronoun,
} from "~/lib/api/user";
import classNames from "classnames";
import StatusIcon from "~/components/StatusIcon";
import PronounLink from "~/components/PronounLink";

export default function StatusLine({
	entry,
	preferences,
}: {
	entry: FieldEntry | Pronoun;
	preferences: Record<string, CustomPreference>;
}) {
	const mergedPrefs = mergePreferences(preferences);
	const currentPref =
		entry.status in mergedPrefs ? mergedPrefs[entry.status] : defaultPreferences.missing;

	const classes = classNames({
		"text-muted": currentPref.muted,
		"fw-bold fs-5": currentPref.size == PreferenceSize.Large,
		"fs-6": currentPref.size == PreferenceSize.Small,
	});

	if ("display_text" in entry) {
		const pronoun = entry as Pronoun;
		return (
			<span className={classes}>
				<StatusIcon preferences={preferences} status={entry.status} />{" "}
				<PronounLink pronoun={pronoun} />
			</span>
		);
	}

	return (
		<span className={classes}>
			<StatusIcon preferences={preferences} status={entry.status} /> {entry.value}
		</span>
	);
}
