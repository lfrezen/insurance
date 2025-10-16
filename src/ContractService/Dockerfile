# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/ContractService/API/ContractService.API.csproj", "ContractService/API/"]
COPY ["src/ContractService/Application/ContractService.Application.csproj", "ContractService/Application/"]
COPY ["src/ContractService/Domain/ContractService.Domain.csproj", "ContractService/Domain/"]
COPY ["src/ContractService/Infrastructure/ContractService.Infrastructure.csproj", "ContractService/Infrastructure/"]

RUN dotnet restore "ContractService/API/ContractService.API.csproj"

# Copy everything else and build
COPY src/ContractService/ ContractService/
WORKDIR "/src/ContractService/API"
RUN dotnet build "ContractService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ContractService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContractService.API.dll"]
