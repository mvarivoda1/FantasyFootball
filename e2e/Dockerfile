FROM mcr.microsoft.com/playwright:v1.61.1-noble

WORKDIR /e2e

COPY package.json package-lock.json ./
RUN npm ci

COPY playwright.config.ts tsconfig.json ./
COPY tests/ tests/

CMD ["npx", "playwright", "test"]
