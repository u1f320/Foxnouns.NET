# List of API endpoints and scopes

## Scopes

- `identify`: `@me` will refer to token user (always granted)
- `user.read_privileged`: can read privileged information such as authentication methods
- `user.update`: can update the user's profile.
  **cannot** update anything locked behind `user.read_privileged`
- `member.read`: can view member list if it's hidden and enumerate unlisted members
- `member.create`: can create new members
- `member.update`: can edit and delete members

## Users

- GET `/users/{userRef}`: `identify` required to use `@me` as user reference.
  `user.read_privileged` required to view authentication methods.
  `member.read` required to view unlisted members.
- PATCH `/users/@me`: `user.update` required.