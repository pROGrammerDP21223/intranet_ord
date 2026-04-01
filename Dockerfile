# Single container: Nginx (80) routes two subdomains — React dashboard + PHP ordpanel; Kestrel (.NET API) on 127.0.0.1:5000
# Build from repository root: docker build -t app-all-in-one .

FROM node:20-bookworm AS web-build
WORKDIR /src
COPY new-dashboard/package.json new-dashboard/package-lock.json ./
RUN npm ci
COPY new-dashboard/ ./
# Same-origin API via Nginx /api -> Kestrel (do not use localhost:8080 in production bundle)
ENV VITE_API_URL=
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src
COPY backend_net/backend_net.csproj backend_net/
RUN dotnet restore backend_net/backend_net.csproj
COPY backend_net/ backend_net/
RUN dotnet publish backend_net/backend_net.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates curl gnupg gettext-base supervisor nginx \
    php8.2-fpm php8.2-cli php8.2-curl php8.2-mbstring php8.2-xml php8.2-odbc unixodbc \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends msodbcsql18 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=api-build /app/publish .
COPY --from=web-build /src/dist /var/www/dashboard
COPY ordpanel /var/www/ordpanel

COPY docker/nginx.conf.template /etc/nginx/templates/app.conf.template
COPY docker/supervisord.conf /etc/supervisord.conf
COPY docker/entrypoint.sh /entrypoint.sh
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/entrypoint.sh"]
