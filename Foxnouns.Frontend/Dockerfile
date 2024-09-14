FROM docker.io/node:22

RUN mkdir -p /app/node_modules && chown -R node:node /app
WORKDIR /app
COPY package.json yarn.lock ./
USER node
RUN yarn
COPY --chown=node:node . .

RUN yarn build

CMD ["yarn", "start"]
