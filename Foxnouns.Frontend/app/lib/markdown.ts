import MarkdownIt from "markdown-it";
import sanitize from "sanitize-html";

const md = new MarkdownIt({
	html: false,
	breaks: true,
	linkify: true,
}).disable(["heading", "lheading", "link", "table", "blockquote"]);

const unsafeMd = new MarkdownIt({
	html: false,
	breaks: true,
	linkify: true,
});

export function renderMarkdown(src: string | null) {
	return src ? sanitize(md.render(src)) : null;
}

export function renderUnsafeMarkdown(src: string) {
	return sanitize(unsafeMd.render(src));
}
