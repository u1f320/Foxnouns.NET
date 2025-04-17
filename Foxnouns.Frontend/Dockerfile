FROM docker.io/node:23-slim

COPY ./Foxnouns.Frontend /app
COPY ./docker/frontend.env /app/.env.local
WORKDIR /app

ENV PRIVATE_API_HOST=http://rate:5003/api
ENV PRIVATE_INTERNAL_API_HOST=http://backend:5000/api

RUN npm ci
RUN npm run build

CMD ["node", "build/index.js"]
