# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY toritanulo.sln ./
COPY toritanulo/toritanulo.csproj ./toritanulo/
RUN dotnet restore ./toritanulo/toritanulo.csproj

COPY . .
RUN dotnet publish ./toritanulo/toritanulo.csproj -c Release -o /app/out --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "toritanulo.dll"]
