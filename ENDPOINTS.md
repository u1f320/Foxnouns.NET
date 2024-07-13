# List of API endpoints and scopes

## Scopes

- `identify`: `@me` will refer to token user (always granted)
- `user.read_hidden`: can read non-privileged hidden information such as timezone,
  whether the member list is hidden, and whether a member is unlisted.
- `user.read_privileged`: can read privileged information such as authentication methods
- `user.update`: can update the user's profile.
  **cannot** update anything locked behind `user.read_privileged`
- `member.read`: can view member list if it's hidden and enumerate unlisted members
- `member.create`: can create new members
- `member.update`: can edit and delete members

## Meta

- [x] GET `/meta`: gets stats and server information

## Users

- [x] GET `/users/{userRef}`: views current user.
  `identify` required to use `@me` as user reference.
  `user.read_hidden` required to view timezone and other hidden non-privileged data.
  `user.read_privileged` required to view authentication methods.
  `member.read` required to view unlisted members.
- [x] PATCH `/users/@me`: updates current user. `user.update` required.
- [ ] DELETE `/users/@me`: deletes current user. `*` required
- [ ] POST `/users/@me/export`: queues new data export. `*` required
- [ ] GET `/users/@me/export`: gets latest data export. `*` required
- [ ] GET `/users/@me/flags`: get all the user's flags. `identify` required
- [ ] POST `/users/@me/flags`: creates a new flag. `user.update` required
- [ ] PATCH `/users/@me/flags/{id}`: updates an existing flag. `user.update` required
- [ ] DELETE `/users/@me/flags/{id}`: deletes a user flag. `user.update` required
- [ ] POST `/users/@me/reroll`: rerolls a user's short ID. `user.update` required

## Members

- [x] GET `/users/{userRef}/members`: gets list of a user's members.
  if the user's member list is hidden, 
  and it is not the authenticated user (or the token doesn't have the `member.read` scope)
  returns an empty array.
- [x] GET `/users/{userRef}/members/{memberRef}`: gets a single member.
  will always return a member if it exists, even if the member is unlisted.
- [ ] POST `/users/@me/members`: creates a new member. `member.create` required
- [ ] PATCH `/users/@me/members/{memberRef}`: edits a member. `member.update` required
- [ ] DELETE `/users/@me/members/{memberRef}`: deletes a member. `member.update` required
- [ ] POST `/users/@me/members/{memberRef}/reroll`: rerolls a member's short ID. `member.update` required.
