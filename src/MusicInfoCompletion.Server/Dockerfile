#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT Production
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["MusicInfoCompletion.Server/MusicInfoCompletion.Server.csproj", "MusicInfoCompletion.Server/"]
RUN dotnet restore "MusicInfoCompletion.Server/MusicInfoCompletion.Server.csproj"
COPY . .
WORKDIR "/src/MusicInfoCompletion.Server"
RUN dotnet build "MusicInfoCompletion.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MusicInfoCompletion.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicInfoCompletion.Server.dll"]