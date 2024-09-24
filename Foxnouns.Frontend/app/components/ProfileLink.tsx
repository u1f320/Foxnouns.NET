import { Globe } from "react-bootstrap-icons";

export default function ProfileLink({ link }: { link: string }) {
	const isLink = link.startsWith("http://") || link.startsWith("https://");

	let displayLink = link;
	if (link.startsWith("http://")) displayLink = link.substring("http://".length);
	else if (link.startsWith("https://")) displayLink = link.substring("https://".length);
	if (displayLink.endsWith("/")) displayLink = displayLink.substring(0, displayLink.length - 1);

	if (isLink) {
		return (
			<a href={link} className="text-decoration-none" rel="me nofollow noreferrer" target="_blank">
				<li className="py-2 py-lg-0">
					<Globe className="text-body" aria-hidden={true} />{" "}
					<span className="text-decoration-underline">{displayLink}</span>
				</li>
			</a>
		);
	}

	return (
		<li className="py-2 py-lg-0">
			<Globe aria-hidden={true} /> {displayLink}
		</li>
	);
}
