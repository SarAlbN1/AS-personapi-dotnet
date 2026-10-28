# Imagen del monolito personapi-dotnet en dos etapas: se compila con el SDK y se
# ejecuta con el runtime de ASP.NET Core, más liviano. La cadena de conexión no
# vive en la imagen: la inyecta docker-compose.yml con ConnectionStrings__PersonaDb.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY personapi-dotnet/personapi-dotnet.csproj personapi-dotnet/
RUN dotnet restore personapi-dotnet/personapi-dotnet.csproj
COPY personapi-dotnet/ personapi-dotnet/
RUN dotnet publish personapi-dotnet/personapi-dotnet.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# La imagen aspnet:10.0 ya escucha en 8080 (ASPNETCORE_HTTP_PORTS); se declara el puerto.
EXPOSE 8080
ENTRYPOINT ["dotnet", "personapi-dotnet.dll"]
