FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FountainBistro.Web.csproj", "."]
RUN dotnet restore "./FountainBistro.Web.csproj"
COPY . .
RUN dotnet publish "FountainBistro.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FountainBistro.Web.dll"]
