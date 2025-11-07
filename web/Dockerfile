# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY package.json package-lock.json* yarn.lock* pnpm-lock.yaml* ./
RUN npm ci || npm i
COPY . .
RUN npm run build

# Run dev server for quick testing (optional) or serve built assets
# Here we'll run vite dev server to keep hot reload in docker-compose
FROM node:20-alpine AS dev
WORKDIR /app
COPY --from=build /app /app
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
EXPOSE 5173
CMD ["npm", "run", "dev"]