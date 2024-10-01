import {
	defaultPreferences,
	mergePreferences,
	PartialMember,
	PartialUser,
	Pronoun,
} from "~/lib/api/user";
import { Link } from "@remix-run/react";
import { defaultAvatarUrl } from "~/lib/utils";
import { useTranslation } from "react-i18next";
import { OverlayTrigger, Tooltip } from "react-bootstrap";
import { Lock } from "react-bootstrap-icons";
import AvatarImage from "~/components/profile/AvatarImage";

export default function MemberCard({ user, member }: { user: PartialUser; member: PartialMember }) {
	const { t } = useTranslation();

	const mergedPrefs = mergePreferences(user.custom_preferences);
	const pronouns: Pronoun[] = [];
	for (const pronoun of member.pronouns) {
		const pref =
			pronoun.status in mergedPrefs ? mergedPrefs[pronoun.status] : defaultPreferences.missing;
		if (pref.favourite) pronouns.push(pronoun);
	}

	const displayedPronouns = pronouns
		.map((pronoun) => {
			if (pronoun.display_text) {
				return pronoun.display_text;
			} else {
				const split = pronoun.value.split("/");
				if (split.length === 5) return split.splice(0, 2).join("/");
				return pronoun.value;
			}
		})
		.join(", ");

	return (
		<div className="col">
			<Link to={`/@${user.username}/${member.name}`}>
				<AvatarImage
					src={member.avatar_url || defaultAvatarUrl}
					width={200}
					alt={t("user.member-avatar-alt", { name: member.name })}
					lazyLoad
				/>
			</Link>
			<p className="m-2">
				<Link to={`/@${user.username}/${member.name}`} className="text-reset fs-5 text-break">
					{member.display_name ?? member.name}
					{member.unlisted === true && (
						<>
							<OverlayTrigger
								key={member.id}
								placement="top"
								overlay={
									<Tooltip id={member.id} aria-hidden={true}>
										{t("user.member-hidden")}
									</Tooltip>
								}
							>
								<span className="d-inline-block">
									<Lock aria-hidden={true} style={{ pointerEvents: "none" }} />
								</span>
							</OverlayTrigger>
						</>
					)}
				</Link>
				{displayedPronouns && <>{displayedPronouns}</>}
			</p>
		</div>
	);
}
