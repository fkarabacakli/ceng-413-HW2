# ---------- Stage 1: build (SDK image, large, never shipped) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the solution and project files first. The restore layer below is then
# cached and runs again only when a dependency changes, not on every code edit.
COPY Nalbur.slnx ./
COPY src/Nalbur.Domain/Nalbur.Domain.csproj                 src/Nalbur.Domain/
COPY src/Nalbur.Application/Nalbur.Application.csproj       src/Nalbur.Application/
COPY src/Nalbur.Infrastructure/Nalbur.Infrastructure.csproj src/Nalbur.Infrastructure/
COPY src/Nalbur.Api/Nalbur.Api.csproj                       src/Nalbur.Api/
COPY tests/Nalbur.Tests/Nalbur.Tests.csproj                 tests/Nalbur.Tests/
RUN dotnet restore Nalbur.slnx

# Now the source code, then compile.
COPY . .
RUN dotnet build Nalbur.slnx -c Release --no-restore

# ---------- Stage 2: test (the image build fails if a test fails) ----------
FROM build AS test
RUN dotnet test Nalbur.slnx -c Release --no-build

# ---------- Stage 3: publish (starts FROM test, so tests always run first) ----------
FROM test AS publish
RUN dotnet publish src/Nalbur.Api/Nalbur.Api.csproj -c Release -o /out --no-restore

# ---------- Stage 4: runtime (ASP.NET runtime only: no SDK, no source, no tests) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# curl is used only by HEALTHCHECK. Installed as root, before switching user.
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

COPY --from=publish /out .

# Non-root: the official image ships an "app" user; APP_UID holds its id.
USER $APP_UID

# Defaults only. Override with -e or docker compose. No secrets here.
ENV ASPNETCORE_HTTP_PORTS=8080 \
    SHOP_NAME="Nalbur Stok" \
    CRITICAL_STOCK_DEFAULT=5 \
    SEED_SAMPLE_DATA=false

EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Nalbur.Api.dll"]
