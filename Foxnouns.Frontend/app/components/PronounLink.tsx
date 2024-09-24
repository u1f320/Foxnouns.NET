import { Pronoun } from "~/lib/api/user";
import { Link } from "@remix-run/react";

export default function PronounLink({ pronoun }: { pronoun: Pronoun }) {
	let displayText: string;
	if (pronoun.display_text) displayText = pronoun.display_text;
	else {
		const split = pronoun.value.split("/");
		if (split.length === 5) displayText = split.splice(0, 2).join("/");
		else displayText = pronoun.value;
	}

	let link: string;
	const linkBase = pronoun.value
		.split("/")
		.map((value) => encodeURIComponent(value))
		.join("/");

	if (pronoun.display_text) {
		link = `${linkBase},${encodeURIComponent(pronoun.display_text)}`;
	} else {
		link = linkBase;
	}

	return pronoun.value.split("/").length === 5 ? (
		<Link className="text-reset" to={`/pronouns/${link}`}>
			{displayText}
		</Link>
	) : (
		<>{displayText}</>
	);
}
