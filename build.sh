#!/usr/bin/env bash
set -euxo pipefail

ROOT_DIR=$(pwd)

echo "Cleaning output directory ($ROOT_DIR/build)"

[ -d "$ROOT_DIR/build" ] && rm -r "$ROOT_DIR/build"
mkdir "$ROOT_DIR/build"

echo "Building .NET backend"

cd "$ROOT_DIR/Foxnouns.Backend"
[ -d "$ROOT_DIR/Foxnouns.Backend/out" ] && rm -r "$ROOT_DIR/Foxnouns.Backend/out"
dotnet publish --artifacts-path "$ROOT_DIR/Foxnouns.Backend/out"
mv "$ROOT_DIR/Foxnouns.Backend/out/publish/Foxnouns.Backend/"* "$ROOT_DIR/build/bin"

echo "Building Go rate limiter"

cd "$ROOT_DIR/Foxnouns.RateLimiter"
go build -o rate -v .
mv rate "$ROOT_DIR/build/rate"

echo "Building Node.js frontend"

cd "$ROOT_DIR/Foxnouns.Frontend"
[ -d "$ROOT_DIR/Foxnouns.Frontend/build" ] && rm -r "$ROOT_DIR/Foxnouns.Frontend/build"
pnpm install
pnpm build

mkdir "$ROOT_DIR/build/fe"
cp -r build .env* package.json pnpm-lock.yaml "$ROOT_DIR/build/fe"
cd "$ROOT_DIR/build/fe"
pnpm install -P

echo "Finished building Foxnouns.NET"
