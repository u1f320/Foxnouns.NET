import { PUBLIC_LANGUAGE } from "$env/static/public";
import i18n, { type Config } from "sveltekit-i18n";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const config: Config<any> = {
	initLocale: PUBLIC_LANGUAGE,
	fallbackLocale: "en",
	loaders: [
		{
			locale: "en",
			key: "",
			loader: async () => (await import("./locales/en.json")).default,
		},
		{
			locale: "en-PR",
			key: "",
			loader: async () => (await import("./locales/en-PR.json")).default,
		},
	],
};

export const { t, locales, locale, translations, loadTranslations, setLocale } = new i18n(config);

loadTranslations(PUBLIC_LANGUAGE);
setLocale(PUBLIC_LANGUAGE);
