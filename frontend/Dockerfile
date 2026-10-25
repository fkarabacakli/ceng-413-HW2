# ---------- Stage 1: build the static files with Node ----------
FROM node:22-alpine AS build
WORKDIR /app

# Dependencies first (cached until package*.json changes).
COPY package.json package-lock.json ./
RUN npm ci

# Then the source code.
COPY . .
RUN npm run build

# ---------- Stage 2: serve with nginx (no Node, no node_modules, no source) ----------
# nginx-unprivileged runs as a non-root user (uid 101) and listens on 8080.
FROM nginxinc/nginx-unprivileged:1.27-alpine AS final

COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

# Where nginx forwards /api requests. Override at run time if needed.
ENV API_UPSTREAM=http://app:8080

USER 101
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=3s --start-period=5s --retries=3 \
  CMD wget -qO- http://127.0.0.1:8080/healthz || exit 1
