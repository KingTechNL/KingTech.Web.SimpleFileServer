#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Image to build web-application
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["KingTech.Web.SimpleFileServer.csproj", "KingTech.Web.SimpleFileServer/"]
COPY ["../KingTech.Web.SimpleFileServer.Abstract.NuGet/KingTech.Web.SimpleFileServer.Abstract.NuGet.csproj", "KingTech.Web.SimpleFileServer.Abstract.NuGet/"]
COPY ["../KingTech.Web.SimpleFileServer.BasicPlugins/KingTech.Web.SimpleFileServer.BasicPlugins.csproj", "KingTech.Web.SimpleFileServer.BasicPlugins/"]
RUN dotnet restore "KingTech.Web.SimpleFileServer/KingTech.Web.SimpleFileServer.csproj"
RUN dotnet restore "KingTech.Web.SimpleFileServer.BasicPlugins/KingTech.Web.SimpleFileServer.BasicPlugins.csproj"
COPY . .
WORKDIR "/src/KingTech.Web.SimpleFileServer"
RUN dotnet build "KingTech.Web.SimpleFileServer.csproj" -c Release -o /app/build
RUN dotnet build "KingTech.Web.SimpleFileServer.BasicPlugins/KingTech.Web.SimpleFileServer.BasicPlugins.csproj" -c Release -o /plugins

FROM build AS publish
RUN dotnet publish "KingTech.Web.SimpleFileServer.csproj" -c Release -o /app/publish



# The final docker image
FROM base AS final
WORKDIR /app
# Adding bash for easy debugging of docker container.
RUN apk add --no-cache bash 
# Add compiled & published web application
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KingTech.Web.SimpleFileServer.dll"]