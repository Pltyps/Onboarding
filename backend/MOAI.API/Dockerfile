# ---- Base image with .NET Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Install LibreOffice and cleanup cache for smaller image
RUN apt-get update && apt-get install -y \
    libreoffice \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# ---- Build the app ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and publish
COPY . . 
RUN dotnet publish -c Release -o /app/publish

# ---- Final image ----
FROM base AS final
WORKDIR /app

# Copy published app from build
COPY --from=build /app/publish .

# Expose ports if needed for dev/testing
EXPOSE 5000
EXPOSE 4000

# Run the app
ENTRYPOINT ["dotnet", "MOAI.API.dll"]
