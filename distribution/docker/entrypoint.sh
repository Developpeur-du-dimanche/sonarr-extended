#!/bin/sh
set -e

PUID="${PUID:-65534}"
PGID="${PGID:-65534}"

case "$PUID$PGID" in
    ''|*[!0-9]*)
        echo "PUID and PGID must be numeric (got PUID='$PUID', PGID='$PGID')" >&2
        exit 1
        ;;
esac

if [ "$(id -u)" != "0" ]; then
    exec "$@"
fi

if [ -n "$TZ" ] && [ -f "/usr/share/zoneinfo/$TZ" ]; then
    ln -sf "/usr/share/zoneinfo/$TZ" /etc/localtime
    echo "$TZ" > /etc/timezone
elif [ -n "$TZ" ]; then
    echo "Unknown timezone '$TZ', keeping the system default" >&2
fi

mkdir -p /config

if [ "$(stat -c '%u:%g' /config)" != "$PUID:$PGID" ]; then
    echo "Setting ownership of /config to $PUID:$PGID"
    chown -R "$PUID:$PGID" /config
fi

echo "Starting Sonarr as $PUID:$PGID (TZ=${TZ:-unset})"
exec setpriv --reuid="$PUID" --regid="$PGID" --clear-groups -- "$@"
