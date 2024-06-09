import { writable } from "svelte/store";
import { browser } from "$app/environment";

const defaultThemeValue = "light";
const initialThemeValue = browser
	? window.localStorage.getItem("pronouns-theme") ?? defaultThemeValue
	: defaultThemeValue;

export const themeStore = writable<string>(initialThemeValue);
