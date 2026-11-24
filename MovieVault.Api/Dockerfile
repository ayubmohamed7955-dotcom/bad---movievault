# Build: kør fra roden af repoet (compose.yaml sætter context: .)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore først, så Docker kan cache pakkerne, når kun koden ændres
COPY MovieVault.Api/MovieVault.Api.csproj MovieVault.Api/
RUN dotnet restore MovieVault.Api/MovieVault.Api.csproj

COPY MovieVault.Api/ MovieVault.Api/
RUN dotnet publish MovieVault.Api/MovieVault.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime: kun ASP.NET Core runtime, ingen SDK -> mindre image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MovieVault.Api.dll"]
