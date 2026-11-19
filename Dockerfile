# syntax=docker/dockerfile:1

# ---- Stage 1: build the React frontend ----
FROM node:22-alpine AS frontend
WORKDIR /src/frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
# Public by design (demo account); empty disables the demo button.
ARG VITE_DEMO_EMAIL=""
ARG VITE_DEMO_PASSWORD=""
ENV VITE_DEMO_EMAIL=$VITE_DEMO_EMAIL \
    VITE_DEMO_PASSWORD=$VITE_DEMO_PASSWORD
# vite outDir is ../Finance.API/wwwroot -> /src/Finance.API/wwwroot
RUN npm run build

# ---- Stage 2: restore and publish the API ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Finance.API/Finance.API.csproj Finance.API/
COPY Finance.Application/Finance.Application.csproj Finance.Application/
COPY Finance.Domain/Finance.Domain.csproj Finance.Domain/
COPY Finance.Infrastructure/Finance.Infrastructure.csproj Finance.Infrastructure/
RUN dotnet restore Finance.API/Finance.API.csproj
COPY Finance.API/ Finance.API/
COPY Finance.Application/ Finance.Application/
COPY Finance.Domain/ Finance.Domain/
COPY Finance.Infrastructure/ Finance.Infrastructure/
COPY --from=frontend /src/Finance.API/wwwroot Finance.API/wwwroot
RUN dotnet publish Finance.API/Finance.API.csproj -c Release -o /app/publish --no-restore

# ---- Stage 3: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Finance.API.dll"]
