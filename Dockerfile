FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY *.sln .
COPY src/BatalhaNaval.API/*.csproj src/BatalhaNaval.API/
COPY src/BatalhaNaval.Application/*.csproj src/BatalhaNaval.Application/
COPY src/BatalhaNaval.Domain/*.csproj src/BatalhaNaval.Domain/
COPY src/BatalhaNaval.Infrastructure/*.csproj src/BatalhaNaval.Infrastructure/

COPY tests/BatalhaNaval.IntegrationTests/*.csproj tests/BatalhaNaval.IntegrationTests/
COPY tests/BatalhaNaval.UnitTests/*.csproj tests/BatalhaNaval.UnitTests/

RUN dotnet restore

COPY src/ src/

# Se desconetar a linha abaixo, prepare-se para um build mais lento, pois os testes não serão rápidos.
# COPY tests/ tests/

WORKDIR /app/src/BatalhaNaval.API
RUN dotnet publish "BatalhaNaval.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

ENV DB_HOST=localhost
ENV DB_PORT=5432
ENV DB_NAME=postgres
ENV DB_USER=postgres
ENV REDIS_HOST=localhost
ENV REDIS_PORT=6379
ENV ASPNETCORE_URLS=http://+:8080
ENV ALLOWED_ORIGINS=http://localhost:3000

# Copy published output from build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BatalhaNaval.API.dll"]