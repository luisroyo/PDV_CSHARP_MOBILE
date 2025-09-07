# Dockerfile para PDV Multi-Vertical API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Pos.Api/Pos.Api.csproj", "Pos.Api/"]
COPY ["Pos.Domain/Pos.Domain.csproj", "Pos.Domain/"]
COPY ["Pos.Application/Pos.Application.csproj", "Pos.Application/"]
COPY ["Pos.Infrastructure/Pos.Infrastructure.csproj", "Pos.Infrastructure/"]
RUN dotnet restore "Pos.Api/Pos.Api.csproj"
COPY . .
WORKDIR "/src/Pos.Api"
RUN dotnet build "Pos.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pos.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pos.Api.dll"]
