package main

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"log"
	"net/http"
	"os"
	"strconv"

	"github.com/go-chi/chi/v5"
	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"
)

type Proxy struct {
	OldAvatarBase string `json:"old_avatar_base"`
	NewAvatarBase string `json:"new_avatar_base"`
	FlagBase      string `json:"flag_base"`

	Port     int    `json:"port"`
	Database string `json:"database"`

	db *pgxpool.Pool
}

type EntityWithAvatar struct {
	ID             uint64
	LegacyID       string
	Avatar         string
	AvatarMigrated bool
}

func main() {
	b, err := os.ReadFile("config.json")
	if err != nil {
		log.Fatalln("error reading config:", err)
	}

	p := &Proxy{}
	err = json.Unmarshal(b, p)
	if err != nil {
		log.Fatalln("error parsing config:", err)
	}

	p.db, err = pgxpool.New(context.Background(), p.Database)
	if err != nil {
		log.Fatalln("error connecting to database:", err)
	}

	r := chi.NewRouter()

	r.HandleFunc(`/flags/{hash:[\da-f]+}.webp`, func(w http.ResponseWriter, r *http.Request) {
		http.Redirect(w, r, fmt.Sprintf("%s%s", p.FlagBase, r.URL.Path), http.StatusTemporaryRedirect)
	})
	r.Get(`/members/{id:[\d]+}/avatars/{hash:[\da-f]+}.webp`, p.proxyHandler("member"))
	r.Get(`/users/{id:[\d]+}/avatars/{hash:[\da-f]+}.webp`, p.proxyHandler("user"))
	r.NotFound(func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusNotFound)
		w.Write([]byte("file not found"))
	})

	log.Printf("serving on port %v", p.Port)

	err = http.ListenAndServe(":"+strconv.Itoa(p.Port), r)
	if err != nil {
		log.Fatalf("listening on port %v: %v", p.Port, err)
	}
}

func (p *Proxy) proxyHandler(avatarType string) func(http.ResponseWriter, *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		id := chi.URLParam(r, "id")
		hash := chi.URLParam(r, "hash")

		var e EntityWithAvatar
		// don't do this normally, kids. avatarType can only be "user" or "member" so it's fine here but this is a BAD idea otherwise.
		err := p.db.QueryRow(
			r.Context(), "SELECT id, legacy_id, avatar, avatar_migrated FROM "+avatarType+"s WHERE id = $1 AND avatar = $2",
			id, hash,
		).Scan(&e.ID, &e.LegacyID, &e.Avatar, &e.AvatarMigrated)
		if err != nil {
			if errors.Is(err, pgx.ErrNoRows) {
				w.WriteHeader(http.StatusNotFound)
				w.Write([]byte("avatar not found"))
				return
			}

			log.Printf("error getting avatar for %s %s: %v", avatarType, id, err)
			w.WriteHeader(http.StatusInternalServerError)
			w.Write([]byte("internal server error"))
			return
		}

		if e.AvatarMigrated {
			http.Redirect(w, r, fmt.Sprintf("%s/%ss/%d/avatars/%s.webp", p.NewAvatarBase, avatarType, e.ID, e.Avatar), http.StatusTemporaryRedirect)
		} else {
			http.Redirect(w, r, fmt.Sprintf("%s/%ss/%s/%s.webp", p.OldAvatarBase, avatarType, e.LegacyID, e.Avatar), http.StatusTemporaryRedirect)
		}
	}
}
