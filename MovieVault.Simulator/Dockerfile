# Build: kør fra roden af repoet (compose.yaml sætter context: .)
# Forudsætter projektet MovieVault.Simulator/MovieVault.Simulator.csproj (Worker Service)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MovieVault.Simulator/MovieVault.Simulator.csproj MovieVault.Simulator/
RUN dotnet restore MovieVault.Simulator/MovieVault.Simulator.csproj

COPY MovieVault.Simulator/ MovieVault.Simulator/
RUN dotnet publish MovieVault.Simulator/MovieVault.Simulator.csproj -c Release -o /app/publish /p:UseAppHost=false

# Worker Service har ikke brug for ASP.NET Core -> runtime-image
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MovieVault.Simulator.dll"]
