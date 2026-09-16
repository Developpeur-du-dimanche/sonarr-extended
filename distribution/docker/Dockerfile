# Runtime image for Sonarr, built by .github/workflows/docker.yml.
# The build context is prepared by the workflow and contains one self-contained
# build per platform (<TARGETPLATFORM>/Sonarr) plus the package_info file.
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0

ARG TARGETPLATFORM

COPY package_info /app/package_info
COPY ${TARGETPLATFORM}/Sonarr /app/bin

EXPOSE 8989
VOLUME ["/config"]

ENTRYPOINT ["/app/bin/Sonarr", "-nobrowser", "-data=/config"]
