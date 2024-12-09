FROM docker.io/node:22-slim

ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
RUN corepack enable

COPY ./Foxnouns.Frontend /app
COPY ./docker/frontend.env /app/.env.local
WORKDIR /app

ENV PRIVATE_API_HOST=http://rate:5003/api
ENV PRIVATE_INTERNAL_API_HOST=http://backend:5000/api

RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --frozen-lockfile
RUN pnpm run build

CMD ["pnpm", "node", "build/index.js"]
