import { redirect } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { fastRequest } from "$api";
import log from "$lib/log";

export const GET: RequestHandler = async ({ params, fetch, cookies }) => {
	try {
		await fastRequest("PUT", `/notifications/${params.id}/ack`, { fetch, cookies });
	} catch (e) {
		log.error("error acking notification %s:", params.id, e);
		redirect(303, "/settings/notifications?alert=notif-ack-fail");
	}
	redirect(303, "/settings/notifications?alert=notif-ack-successful");
};
