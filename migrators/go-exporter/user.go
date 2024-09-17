package main

import (
	"context"
	"time"

	"codeberg.org/pronounscc/pronouns.cc/backend/db"
	"emperror.dev/errors"
	"github.com/georgysavva/scany/v2/pgxscan"
)

func getUsers() (u []db.User, err error) {
	q := sq.Select("*", "(SELECT instance FROM fediverse_apps WHERE id = users.fediverse_app_id) AS fediverse_instance").
		From("users").OrderBy("snowflake_id ASC")

	if *exportLimit != 0 {
		q = q.Limit(uint64(*exportLimit))
	}

	sql, args, err := q.ToSql()
	if err != nil {
		return u, errors.Wrap(err, "building sql")
	}

	err = pgxscan.Select(context.Background(), oldDB, &u, sql, args...)
	if err != nil {
		return u, errors.Wrap(err, "getting users from db")
	}

	return u, nil
}

func userToNewUser(u db.User) (NewUser, error) {
	fields, err := oldDB.UserFields(context.Background(), u.ID)
	if err != nil {
		return NewUser{}, errors.Wrap(err, "getting fields")
	}

	new := NewUser{
		ID:          u.SnowflakeID.String(),
		SID:         u.SID,
		Username:    u.Username,
		DisplayName: u.DisplayName,
		Bio:         u.Bio,
		MemberTitle: u.MemberTitle,
		LastActive:  u.LastActive,
		Avatar:      u.Avatar,
		Links:       u.Links,
		Names:       u.Names,

		MemberListHidden: u.ListPrivate,
		Timezone:         u.Timezone,
		Role:             "USER",

		Deleted:      u.DeletedAt != nil,
		DeletedAt:    u.DeletedAt,
		DeleteReason: u.DeleteReason,

		CustomPreferences: u.CustomPreferences,
	}

	if u.IsAdmin {
		new.Role = "ADMIN"
	}

	for _, p := range u.Pronouns {
		new.Pronouns = append(new.Pronouns, NewPronoun{Value: p.Pronouns, Status: string(p.Status), DisplayText: p.DisplayText})
	}

	for _, f := range fields {
		new.Fields = append(new.Fields, NewField{Name: f.Name, Entries: f.Entries})
	}

	return new, nil
}

type NewUser struct {
	ID          string          `json:"id"`
	SID         string          `json:"sid"`
	Username    string          `json:"username"`
	DisplayName *string         `json:"display_name"`
	Bio         *string         `json:"bio"`
	MemberTitle *string         `json:"member_title"`
	LastActive  time.Time       `json:"last_active"`
	Avatar      *string         `json:"avatar_hash"`
	Links       []string        `json:"links"`
	Names       []db.FieldEntry `json:"names"`
	Pronouns    []NewPronoun    `json:"pronouns"`
	Fields      []NewField      `json:"fields"`

	MemberListHidden bool    `json:"member_list_hidden"`
	Timezone         *string `json:"timezone"`
	Role             string  `json:"role"` // one of USER or ADMIN

	Deleted      bool       `json:"deleted"`
	DeletedAt    *time.Time `json:"deleted_at"`
	DeleteReason *string    `json:"delete_reason"` // TODO: this should be imported as a warning

	CustomPreferences db.CustomPreferences `json:"custom_preferences"`
}

type NewPronoun struct {
	Value       string  `json:"value"`
	Status      string  `json:"status"`
	DisplayText *string `json:"display_text"`
}

type NewField struct {
	Name    string          `json:"name"`
	Entries []db.FieldEntry `json:"entries"`
}
