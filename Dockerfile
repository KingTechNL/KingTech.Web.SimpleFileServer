FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
RUN apk --no-cache add curl icu-libs libcap bash

WORKDIR /app
COPY KingTech.Web.SimpleFileServer/bin/Release/net8.0 .
COPY KingTech.Web.SimpleFileServer.BasicPlugins/bin/Release/net8.0 ./basicplugins
COPY entrypoint.sh .
RUN mkdir -p /plugins
RUN chmod +x entrypoint.sh
RUN dos2unix entrypoint.sh

EXPOSE 80
EXPOSE 443

#do not run as root (but allow ports below 1024)
RUN setcap 'cap_net_bind_service=+ep' /app/KingTech.Web.SimpleFileServer.dll
RUN addgroup --system appgroup && adduser --ingroup appgroup --system appuser && chown -R appuser /app /tmp /usr/share /plugins
USER appuser

# Set dotnet environment to production
ENV ASPNETCORE_ENVIRONMENT Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["./entrypoint.sh"]
CMD ["dotnet", "KingTech.Web.SimpleFileServer.dll"]