# Build > 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY VideoCache.sln .
COPY src/VideoCache.Api/*.csproj src/VideoCache.Api/
COPY tests/VideoCache.UnitTests/*.csproj tests/VideoCache.UnitTests/
RUN dotnet restore src/VideoCache.Api/VideoCache.Api.csproj
COPY . .
RUN dotnet publish src/VideoCache.Api/VideoCache.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false
# Runtime > 
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
USER app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VideoCache.Api.dll"]