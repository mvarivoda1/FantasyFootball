# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore (zaseban sloj radi cachiranja paketa)
COPY ["FantasyFootball.csproj", "./"]
RUN dotnet restore "FantasyFootball.csproj"

# Kopiraj ostatak izvornog koda i objavi web projekt (pod-projekti su isključeni
# iz web csproj-a pa se ne grade)
COPY . .
RUN dotnet publish "FantasyFootball.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cloud platforme (Azure App Service, Cloud Run) prosljeđuju port preko $PORT/8080.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FantasyFootball.dll"]
