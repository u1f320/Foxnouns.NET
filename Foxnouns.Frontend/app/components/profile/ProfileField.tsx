import { CustomPreference, FieldEntry, Pronoun } from "~/lib/api/user";
import StatusLine from "~/components/profile/StatusLine";

export default function ProfileField({
	name,
	entries,
	preferences,
}: {
	name: string;
	entries: Array<FieldEntry | Pronoun>;
	preferences: Record<string, CustomPreference>;
}) {
	return (
		<div className="col">
			<h3>{name}</h3>
			<ul className="list-unstyled fs-5">
				{entries.map((e, i) => (
					<li key={i}>
						<StatusLine entry={e} preferences={preferences} />
					</li>
				))}
			</ul>
		</div>
	);
}
