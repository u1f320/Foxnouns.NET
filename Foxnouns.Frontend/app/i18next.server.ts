import Backend from "i18next-fs-backend";
import { resolve } from "node:path";
import { RemixI18Next } from "remix-i18next/server";
import i18n from "~/i18n";
import { LANGUAGE } from "~/env.server";

const i18next = new RemixI18Next({
	detection: {
		supportedLanguages: [LANGUAGE],
		fallbackLanguage: LANGUAGE,
	},
	// This is the configuration for i18next used
	// when translating messages server-side only
	i18next: {
		...i18n,
		fallbackLng: LANGUAGE,
		lng: LANGUAGE,
		backend: {
			loadPath: resolve("./public/locales/{{lng}}.json"),
		},
	},
	// The i18next plugins you want RemixI18next to use for `i18n.getFixedT` inside loaders and actions.
	// E.g. The Backend plugin for loading translations from the file system
	// Tip: You could pass `resources` to the `i18next` configuration and avoid a backend here
	plugins: [Backend],
});

export default i18next;
