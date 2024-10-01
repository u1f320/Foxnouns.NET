export default function AvatarImage({
	src,
	width,
	alt,
	lazyLoad,
}: {
	src: string;
	width: number;
	alt: string;
	lazyLoad?: boolean;
}) {
	return (
		<img
			src={src}
			alt={alt}
			width={width}
			height={width}
			className="rounded-circle img-fluid"
			loading={lazyLoad ? "lazy" : "eager"}
		/>
	);
}
