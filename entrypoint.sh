#!/bin/sh

# Copy basic plugins to plugin directory.
cp /app/plugins/* /plugins
# Start application
exec "$@"