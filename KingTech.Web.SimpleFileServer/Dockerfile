#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["KingTech.Web.SimpleFileServer.csproj", "KingTech.Web.SimpleFileServer/"]
COPY ["../KingTech.Web.SimpleFileServer.Abstract.NuGet/KingTech.Web.SimpleFileServer.Abstract.NuGet.csproj", "KingTech.Web.SimpleFileServer.Abstract.NuGet/"]
#COPY ["./", "KingTech.Web.SimpleFileServer/"]
RUN dotnet restore "KingTech.Web.SimpleFileServer/KingTech.Web.SimpleFileServer.csproj"
COPY . .
WORKDIR "/src/KingTech.Web.SimpleFileServer"
RUN dotnet build "KingTech.Web.SimpleFileServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KingTech.Web.SimpleFileServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KingTech.Web.SimpleFileServer.dll"]