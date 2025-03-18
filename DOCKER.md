# Running with Docker (pre-built backend and rate limiter) *(linux/arm64 only)*

Because SvelteKit is a pain in the ass to build in a container, and processes secrets at build time,
there is no pre-built frontend image available.
If you don't want to build images on your server, I recommend running the frontend outside of Docker.
This is preconfigured in `docker-compose.prebuilt.yml`: the backend, database, and rate limiter will run in Docker,
while the frontend is run as a normal, non-containerized service.

1. Copy `docker/config.example.ini` to `docker/config.ini`, and change the settings to your liking.
2. Copy `docker/proxy-config.example.json` to `docker/proxy-config.json`, and do the same.
3. Run with `docker compose up -f docker-compose.prebuilt.yml`

The backend will listen on port 5001 and metrics will be available on port 5002.
The rate limiter (which is what should be exposed to the outside) will listen on port 5003.
You can use `docker/Caddyfile` as an example for your reverse proxy. If you use nginx, good luck.

# Running with Docker (local builds)

In order to run *everything* in Docker, you'll have to build every container yourself.
The advantage of this is that it's an all-in-one solution, where you only have to point your reverse proxy at a single container.
The disadvantage is that you'll likely have to build the images on the server you'll be running them on. 

1. Configure the backend and rate limiter as in the section above.
2. Copy `docker/frontend.example.env` to `docker/frontend.env`, and configure it.
3. Build with `docker compose build -f docker-compose.local.yml`
4. Run with `docker compose up -f docker-compose.local.yml`

The Caddy server will listen on `localhost:5004` for the frontend and API,
and on `localhost:5005` for the profile URL shortener.

The backend server listens on `localhost:5006` for unproxied API access,
and `localhost:5007` for metrics.