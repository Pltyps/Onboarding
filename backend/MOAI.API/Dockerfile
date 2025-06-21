# ---- Base image with .NET Runtime ----
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
    WORKDIR /app
    
    # Install LibreOffice and wkhtmltopdf
    RUN apt-get update && apt-get install -y \
        libreoffice \
        wkhtmltopdf \
        && apt-get clean \
        && rm -rf /var/lib/apt/lists/*
    
    # ---- Build the app ----
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    
    # Copy csproj and restore first (faster caching)
    COPY *.csproj ./
    RUN dotnet restore
    
    # Copy the rest of the files and build
    COPY . . 
    RUN dotnet publish -c Release -o /app/publish
    
    # ---- Final image ----
    FROM base AS final
    WORKDIR /app
    COPY --from=build /app/publish ./
    
    # Expose port (optional but useful for local dev/test)
    EXPOSE 5000
    EXPOSE 4000
    
    # Run the app
    ENTRYPOINT ["dotnet", "MOAI.API.dll"]
    