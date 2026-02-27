# Build stage — compiles the AOT native binary inside the SDK image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# AOT compilation requires clang and native build tools
RUN apt-get update && apt-get install -y clang zlib1g-dev

WORKDIR /src
COPY . .

# Suppress the local AotOnBuild target — Cloud Build handles publishing
RUN dotnet publish -r linux-x64 -c Release \
    /p:PublishAot=true \
    /p:SelfContained=true \
    /p:AotOnBuild=false \
    -o /app/publish

# Runtime stage — minimal Debian image, no .NET runtime needed
FROM debian:bookworm-slim AS final

WORKDIR /app
COPY --from=build /app/publish/ChatApp .

# Cloud Run expects the container to listen on PORT (default 8080)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["./ChatApp"]
