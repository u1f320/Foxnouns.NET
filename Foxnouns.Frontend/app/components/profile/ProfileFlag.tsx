import type { PrideFlag } from "~/lib/api/user";
import { OverlayTrigger, Tooltip } from "react-bootstrap";

export default function ProfileFlag({ flag }: { flag: PrideFlag }) {
	return (
		<span className="mx-2 my-1">
			<OverlayTrigger
				key={flag.id}
				placement="top"
				overlay={
					<Tooltip id={flag.id} aria-hidden={true}>
						{flag.description ?? flag.name}
					</Tooltip>
				}
			>
				<span>
					<img
						className="pride-flag"
						src={flag.image_url}
						alt={flag.description ?? flag.name}
						style={{ pointerEvents: "none" }}
					/>
				</span>
			</OverlayTrigger>{" "}
			{flag.name}
		</span>
	);
}
