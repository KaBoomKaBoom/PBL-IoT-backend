# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0.202 AS build
WORKDIR /App

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the files and build
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
COPY --from=build /App/out ./

# Set the entry point
ENTRYPOINT ["dotnet", "PBL-IoT-backend.dll"]

# Expose ports if needed (uncomment and adjust as necessary)
# EXPOSE 80
# EXPOSE 443