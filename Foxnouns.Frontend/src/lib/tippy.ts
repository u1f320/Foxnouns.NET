import "tippy.js/animations/scale-subtle.css";
import { createTippy } from "svelte-tippy";

// use with use:tippy on elements
// temporary (probably) until sveltestrap works with svelte 5
const tippy = createTippy({
	animation: "scale-subtle",
	delay: [null, 0],
});

export default tippy;
