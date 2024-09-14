# Running with Docker

1. Copy `docker/config.example.ini` to `docker/config.ini`, and change the settings to your liking.
2. Copy `docker/proxy-config.example.json` to `docker/proxy-config.json`, and do the same.
3. Build with `docker compose build`
4. Run with `docker compose up`

The Caddy server will listen on `localhost:5004`.
