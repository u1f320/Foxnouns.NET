package main

import (
	"context"
	"encoding/json"
	"flag"
	"fmt"
	"log"
	"os"
	"time"

	"codeberg.org/pronounscc/pronouns.cc/backend/db"
	"github.com/Masterminds/squirrel"
	"github.com/jackc/pgx/v5/pgxpool"
)

var exportWhat = flag.String("export", "", "What to export")
var exportLimit = flag.Int64("limit", 0, "Number of items to export for testing")

var sq = squirrel.StatementBuilder.PlaceholderFormat(squirrel.Dollar)
var oldDB *db.DB

type Output struct {
	Output  any      `json:"output"`  // The output array
	Skipped []string `json:"skipped"` // IDs of skipped items
}

func main() {
	flag.Parse()

	dbUrl := os.Getenv("DATABASE")

	pool, err := pgxpool.New(context.Background(), dbUrl)
	if err != nil {
		log.Fatalf("creating database pool: %v\n", err)
	}

	oldDB = &db.DB{
		Pool: pool,
	}

	switch *exportWhat {
	case "users":
		exportUsers()
	default:
		fmt.Printf("invalid export type %q\nvalid export types are: users\n", *exportWhat)
	}
}

func exportUsers() {
	filename := fmt.Sprintf("users-output-%v.json", time.Now().Unix())
	f, err := os.Create(filename)
	if err != nil {
		log.Fatalf("error opening output file %q: %v\n", filename, err)
	}
	defer f.Close()

	users, err := getUsers()
	if err != nil {
		log.Fatalf("getting users from database: %v\n", err)
	}

	log.Println("converting users")

	start := time.Now()

	var (
		newUsers []NewUser
		skipped  []string
	)
	for i, u := range users {
		newUser, err := userToNewUser(u)
		if err != nil {
			log.Printf("error converting user %v: %v\n", u.SnowflakeID, err)
			skipped = append(skipped, u.SnowflakeID.String())
			continue
		}

		newUsers = append(newUsers, newUser)
		log.Printf("converted user %7d (%v)\n", i+1, u.SnowflakeID)
	}

	log.Printf("converted users in %v (skipped: %v)\n", time.Since(start).Round(time.Millisecond), len(skipped))

	b, err := json.MarshalIndent(Output{Output: newUsers, Skipped: skipped}, "", "  ")
	if err != nil {
		log.Fatalf("error marshaling json: %v\n", err)
	}

	_, err = f.Write(b)
	if err != nil {
		log.Fatalf("writing file: %v\n", err)
	}

	err = f.Sync()
	if err != nil {
		log.Fatalf("syncing file: %v\n", err)
	}

	fmt.Printf("\n\nexported %v users! filename: %q\n", len(newUsers), filename)
}
