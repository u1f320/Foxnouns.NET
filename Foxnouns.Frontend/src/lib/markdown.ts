import MarkdownIt from "markdown-it";
import sanitize from "sanitize-html";

const md = new MarkdownIt({
	html: false,
	breaks: true,
	linkify: true,
}).disable(["heading", "lheading", "link", "table", "blockquote"]);

const unsafeMd = new MarkdownIt();

export const renderMarkdown = (src: string | null) => (src ? sanitize(md.render(src)) : null);

export const renderUnsafeMarkdown = (src: string) => sanitize(unsafeMd.render(src));
