# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so the layer is cached until the .csproj changes
COPY RaceDay.MVC.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish RaceDay.MVC.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DisableHttpsRedirect=true \
    ApiSettings__BaseUrl=http://host.docker.internal:5276/

EXPOSE 8080

# Run as the non-root user that ships with the .NET 8 image
USER app

ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]
