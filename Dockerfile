# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["BSD.Api/BSD.Api.csproj", "BSD.Api/"]
COPY ["BSD.Core/BSD.Core.csproj", "BSD.Core/"]
COPY ["BSD.Services/BSD.Services.csproj", "BSD.Services/"]

# Restore dependencies
RUN dotnet restore "BSD.Api/BSD.Api.csproj"

# Copy everything else
COPY . .

# Publish the application
FROM build AS publish
WORKDIR "/src/BSD.Api"
RUN dotnet publish "BSD.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

# Set environment to Development for Swagger
ENV ASPNETCORE_ENVIRONMENT=Development

# Copy published files
COPY --from=publish /app/publish .

# Copy data files
COPY BSD.Api/Data ./Data

ENTRYPOINT ["dotnet", "BSD.Api.dll"]
