# Production - Quasar
FROM node:20-alpine AS build
WORKDIR /app

RUN npm install -g pnpm
COPY package*.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .
RUN pnpm run build

# Servir con Nginx
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
# Assuming nginx.conf is at the project root
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
