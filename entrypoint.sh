#!/bin/sh

# Copy basic plugins to plugin directory.
cp /app/basicplugins/* /plugins
# Start application
exec "$@"