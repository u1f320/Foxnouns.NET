import * as icons from "react-bootstrap-icons";
import { IconProps as BaseIconProps } from "react-bootstrap-icons";
import { pascalCase } from "change-case";

const startsWithNumberRegex = /^\d/;

export default function Icon({ iconName, ...props }: BaseIconProps & { iconName: string }) {
	let icon = pascalCase(iconName);
	if (startsWithNumberRegex.test(icon)) {
		icon = `Icon${icon}`;
	}

	// eslint-disable-next-line import/namespace
	const BootstrapIcon = icons[icon as keyof typeof icons];
	return <BootstrapIcon {...props} />;
}
