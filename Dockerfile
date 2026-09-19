# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, as its own layer, so code edits don't re-download packages.
COPY TechNova.csproj ./
RUN dotnet restore TechNova.csproj

COPY . .
RUN dotnet publish TechNova.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    # Uploads and pitch decks are written here. Mount a persistent disk at
    # this path, otherwise everything users upload is lost on redeploy.
    Storage__Root=/var/data

COPY --from=build /app/publish ./

# Run unprivileged. The mount point is created and handed over up front so
# the app can write to the disk without needing root at runtime.
RUN useradd -u 10001 -m -s /usr/sbin/nologin technova \
    && mkdir -p /var/data \
    && chown -R technova:technova /app /var/data
USER technova

# Render supplies PORT and the app binds to it; this is the local default.
EXPOSE 8080

ENTRYPOINT ["dotnet", "TechNova.dll"]
