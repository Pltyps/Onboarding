# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

# Runtime stage with LibreOffice
FROM libreoffice/officebase:stable as runtime
WORKDIR /app
COPY --from=build /app/out .

# expose HTTPS or HTTP depending on your app
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "MOAI.API.dll"]
