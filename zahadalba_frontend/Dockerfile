# --- Этап 1: Сборка ---
FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json* yarn.lock* pnpm-lock.yaml* ./

RUN \
  if [ -f pnpm-lock.yaml ]; then corepack enable pnpm && pnpm i; \
  elif [ -f yarn.lock ]; then yarn install; \
  elif [ -f package-lock.json ]; then npm ci; \
  else npm install; fi

COPY . .

RUN \
  if [ -f pnpm-lock.yaml ]; then pnpm run build; \
  elif [ -f yarn.lock ]; then yarn run build; \
  else npm run build; fi

# --- Этап 2: Запуск ---
FROM node:24-alpine AS runtime
WORKDIR /app

COPY --from=build /app/.output ./.output
COPY --from=build /app/package.json ./package.json

EXPOSE 3000
ENV HOST=0.0.0.0
ENV PORT=3000

CMD [ "node", ".output/server/index.mjs" ]