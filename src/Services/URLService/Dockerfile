# STAGE 1: Base Runtime (Optimized for production)
# This stage uses a small footprint image with only the ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
# Default ports for modern .NET applications
EXPOSE 8080
EXPOSE 8081

# STAGE 2: Build (Uses full SDK to compile)
# Multi-stage builds use this heavy image only during the build process 
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project file first to leverage Docker layer caching
COPY ["src/Services/URLService/URLService.csproj", "src/Services/URLService/"]
RUN dotnet restore "./src/Services/URLService/URLService.csproj"

# Copy the remaining source code and build the application
COPY . .
WORKDIR "/src/src/Services/URLService"
RUN dotnet build "./URLService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# STAGE 3: Publish (Create the final binaries)iip
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./URLService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "URLService.dll"]