import adapter from "@sveltejs/adapter-node";
import { vitePreprocess } from "@sveltejs/vite-plugin-svelte";

/** @type {import('@sveltejs/kit').Config} */
const config = {
	// Consult https://svelte.dev/docs/kit/integrations
	// for more information about preprocessors
	preprocess: vitePreprocess(),

	kit: {
		// adapter-auto only supports some environments, see https://svelte.dev/docs/kit/adapter-auto for a list.
		// If your environment is not supported, or you settled on a specific environment, switch out the adapter.
		// See https://svelte.dev/docs/kit/adapters for more information about adapters.
		adapter: adapter(),
		alias: {
			$api: "src/lib/api",
			$components: "src/lib/components",
		},
		csrf: {
			// using Caddy as a reverse proxy + CSRF protection breaks forms *in development*, not in production
			// we only disable it during development, during building NODE_ENV == production
			checkOrigin: process.env.NODE_ENV !== "development",
		},
	},
};

export default config;
