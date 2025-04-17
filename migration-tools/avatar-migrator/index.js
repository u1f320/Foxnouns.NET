// TODO: i'm not even sure if this code works. it's not easy to test either. woops

import postgres from "postgres";
import { config } from "dotenv";
import { Client } from "minio";
import { Logger } from "tslog";
import axios from "axios";
config();
const log = new Logger();

const env = (key) => {
  const value = process.env[key];
  if (value) return value;
  throw `No env variable with key $${key} found`;
};

const oldBaseUrl = env("OLD_BASE_URL");
const bucket = env("MINIO_BUCKET");

const sql = postgres(env("DATABASE_URL"));
const minio = new Client({
  endPoint: env("MINIO_ENDPOINT"),
  useSSL: true,
  accessKey: env("MINIO_ACCESS_KEY"),
  secretKey: env("MINIO_SECRET_KEY"),
});

const users =
  await sql`select id::text, username, legacy_id, avatar from users where  avatar is not null order by id asc`;
log.info("have to migrate %d users", users.length);

const migrate = async (user) => {
  log.debug(
    "copying /users/%s/%s.webp to /users/%s/avatars/%s.webp",
    user.legacy_id,
    user.avatar,
    user.id,
    user.avatar
  );

  try {
    const file = await axios.get(
      `${oldBaseUrl}/users/${user.legacy_id}/${user.avatar}.webp`,
      { responseType: "stream" }
    );
    await minio.putObject(
      bucket,
      `users/${user.id}/avatars/${user.avatar}.webp`,
      file.data,
      file.headers["Content-Length"]
    );

    log.info("copied avatar for user %s", user.id);

    await sql`update users set avatar_migrated = true where id = ${user.id}::bigint`;
  } catch (e) {
    if ("status" in e && e.status === 404) {
      log.warn(
        "avatar for user %s/%s is not found. marking it as migrated.",
        user.id,
        user.username
      );
      await sql`update users set avatar_migrated = true where id = ${user.id}::bigint`;
      return;
    }

    log.error(
      "could not migrate avatar for user %s/%s:",
      user.id,
      user.username,
      e
    );
  }
};

for (let index = 0; index < users.length; index++) {
  const user = users[index];
  await migrate(user);
}

log.info("all users migrated!");
process.exit();
