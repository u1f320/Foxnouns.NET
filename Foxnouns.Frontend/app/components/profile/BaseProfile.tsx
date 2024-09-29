import { CustomPreference, User } from "~/lib/api/user";
import { Member } from "~/lib/api/member";
import { defaultAvatarUrl } from "~/lib/utils";
import ProfileFlag from "~/components/profile/ProfileFlag";
import ProfileLink from "~/components/profile/ProfileLink";
import ProfileField from "~/components/profile/ProfileField";
import { useTranslation } from "react-i18next";
import { renderMarkdown } from "~/lib/markdown";

export type Props = {
	name: string;
	fullName?: string;
	avatarI18nKey: string;
	profile: User | Member;
	customPreferences: Record<string, CustomPreference>;
};

export default function BaseProfile({
	name,
	avatarI18nKey,
	fullName,
	profile,
	customPreferences,
}: Props) {
	const { t } = useTranslation();
	const bio = renderMarkdown(profile.bio);

	return (
		<>
			<div className="grid row-gap-3">
				<div className="row">
					<div className="col-md-4 text-center">
						<img
							src={profile.avatar_url || defaultAvatarUrl}
							alt={t(avatarI18nKey, { username: name })}
							width={200}
							height={200}
							className="rounded-circle img-fluid"
						/>
						{profile.flags && profile.bio && (
							<div className="d-flex flex-wrap m-4">
								{profile.flags.map((f, i) => (
									<ProfileFlag flag={f} key={i} />
								))}
							</div>
						)}
					</div>
					<div className="col-md">
						{profile.display_name ? (
							<>
								<h2>{profile.display_name}</h2>
								<p className="fs-5 text-body-secondary">{fullName || `@${name}`}</p>
							</>
						) : (
							<>
								<h2>{fullName || `@${name}`}</h2>
							</>
						)}
						{bio && (
							<>
								<hr />
								<p dangerouslySetInnerHTML={{ __html: bio }}></p>
							</>
						)}
					</div>
					{profile.links.length > 0 && (
						<div className="col-md d-flex align-items-center">
							<ul className="list-unstyled">
								{profile.links.map((l, i) => (
									<ProfileLink link={l} key={i} />
								))}
							</ul>
						</div>
					)}
				</div>
				<div className="row row-cols-1 row-cols-sm-2 row-cols-md-3">
					{profile.names.length > 0 && (
						<ProfileField
							name={t("user.heading.names")}
							entries={profile.names}
							preferences={customPreferences}
						/>
					)}
					{profile.pronouns.length > 0 && (
						<ProfileField
							name={t("user.heading.pronouns")}
							entries={profile.pronouns}
							preferences={customPreferences}
						/>
					)}
					{profile.fields.map((f, i) => (
						<ProfileField
							name={f.name}
							entries={f.entries}
							preferences={customPreferences}
							key={i}
						/>
					))}
				</div>
			</div>
			{/* If a user or member has no bio, flags are displayed in a row below the other profile info, rather than just below the avatar */}
			{profile.flags && !profile.bio && (
				<div className="d-flex flex-wrap m-4">
					{profile.flags.map((f, i) => (
						<ProfileFlag flag={f} key={i} />
					))}
				</div>
			)}
		</>
	);
}
