# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/ProposalService/API/ProposalService.API.csproj", "ProposalService/API/"]
COPY ["src/ProposalService/Application/ProposalService.Application.csproj", "ProposalService/Application/"]
COPY ["src/ProposalService/Domain/ProposalService.Domain.csproj", "ProposalService/Domain/"]
COPY ["src/ProposalService/Infrastructure/ProposalService.Infrastructure.csproj", "ProposalService/Infrastructure/"]

RUN dotnet restore "ProposalService/API/ProposalService.API.csproj"

# Copy everything else and build
COPY src/ProposalService/ ProposalService/
WORKDIR "/src/ProposalService/API"
RUN dotnet build "ProposalService.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProposalService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProposalService.API.dll"]
