FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["UnifiPortAutomation/UnifiPortAutomation.csproj", "UnifiPortAutomation/"]
RUN dotnet restore "UnifiPortAutomation/UnifiPortAutomation.csproj"
COPY . .
WORKDIR "/src/UnifiPortAutomation"
RUN dotnet build "UnifiPortAutomation.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "UnifiPortAutomation.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UnifiPortAutomation.dll"]
