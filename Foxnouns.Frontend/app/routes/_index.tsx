import type { MetaFunction } from "@remix-run/node";

export const meta: MetaFunction = () => {
	return [{ title: "pronouns.cc" }];
};

export default function Index() {
	return (
		<div>
			<h1>pronouns.cc</h1>
		</div>
	);
}
