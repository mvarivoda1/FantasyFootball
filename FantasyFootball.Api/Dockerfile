FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FantasyFootball.Core/FantasyFootball.Core.csproj", "FantasyFootball.Core/"]
COPY ["FantasyFootball.Api/FantasyFootball.Api.csproj", "FantasyFootball.Api/"]
RUN dotnet restore "FantasyFootball.Api/FantasyFootball.Api.csproj"

COPY FantasyFootball.Core/ FantasyFootball.Core/
COPY FantasyFootball.Api/ FantasyFootball.Api/
RUN dotnet publish "FantasyFootball.Api/FantasyFootball.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FantasyFootball.Api.dll"]
