# Runtime image for Sonarr, built by .github/workflows/docker.yml.
# The build context is prepared by the workflow and contains one self-contained
# build per platform (<TARGETPLATFORM>/Sonarr), the package_info file and entrypoint.sh.
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0

ARG TARGETPLATFORM

# PUID/PGID: user and group Sonarr runs as (owner of /config), nobody:nogroup by default.
# TZ: timezone, as a tz database name (e.g. Europe/Paris).
ENV PUID=65534 \
    PGID=65534 \
    TZ=Etc/UTC \
    HOME=/config

COPY package_info /app/package_info
COPY ${TARGETPLATFORM}/Sonarr /app/bin
COPY --chmod=755 entrypoint.sh /app/entrypoint.sh

EXPOSE 8989
VOLUME ["/config"]

ENTRYPOINT ["/app/entrypoint.sh", "/app/bin/Sonarr", "-nobrowser", "-data=/config"]
