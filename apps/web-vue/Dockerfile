FROM node:20-alpine AS builder
WORKDIR /app

COPY apps/web-vue/package.json apps/web-vue/package-lock.json ./
RUN npm ci

COPY apps/web-vue/ ./
RUN npm run build

FROM nginx:1.27-alpine AS runner

COPY apps/web-vue/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
