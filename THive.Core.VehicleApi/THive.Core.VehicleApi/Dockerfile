#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["THive.Core.VehicleApi/THive.Core.VehicleApi.csproj", "THive.Core.VehicleApi/"]
RUN dotnet restore "THive.Core.VehicleApi/THive.Core.VehicleApi.csproj"
COPY . .
WORKDIR "/src/THive.Core.VehicleApi"
RUN dotnet build "THive.Core.VehicleApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "THive.Core.VehicleApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "THive.Core.VehicleApi.dll"]