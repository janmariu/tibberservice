FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["tibberservice.csproj", "."]
RUN dotnet restore "tibberservice.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "tibberservice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "tibberservice.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app    
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "tibberservice.dll"]