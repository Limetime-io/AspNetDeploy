FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SatelliteLinuxGrpcHost/SatelliteLinuxGrpcHost.csproj", "SatelliteLinuxGrpcHost/"]
RUN dotnet restore "SatelliteLinuxGrpcHost/SatelliteLinuxGrpcHost.csproj"
COPY . .
WORKDIR "/src/SatelliteLinuxGrpcHost"
RUN dotnet build "SatelliteLinuxGrpcHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SatelliteLinuxGrpcHost.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install Docker CLI and PowerShell
RUN apt-get update && apt-get install -y \
    docker.io \
    wget \
    apt-transport-https \
    && wget https://github.com/PowerShell/PowerShell/releases/download/v7.4.0/powershell_7.4.0-1.deb_amd64.deb \
    && dpkg -i powershell_7.4.0-1.deb_amd64.deb \
    && apt-get install -f \
    && rm powershell_7.4.0-1.deb_amd64.deb \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "SatelliteLinuxGrpcHost.dll"]
