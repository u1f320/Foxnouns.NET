# Rate limiting proxy

This is a service that's meant to sit between nginx (or another reverse proxy) and Foxnouns.Backend.
To configure, copy `proxy-config.example.json` to your working directory, rename it to `proxy-config.json`,
and change any keys you need to.

Build with `go build -v .` and run with `./rate`
