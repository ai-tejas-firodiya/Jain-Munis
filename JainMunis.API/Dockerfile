FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["JainMunis.API.csproj", "."]
RUN dotnet restore "./JainMunis.API.csproj"
COPY . .
WORKDIR "/src/JainMunis.API"
RUN dotnet build "JainMunis.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JainMunis.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Create uploads directory
RUN mkdir -p /app/uploads
ENTRYPOINT ["dotnet", "JainMunis.API.dll"]