FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "FountainBistro.Web/FountainBistro.Web.csproj"
RUN dotnet publish "FountainBistro.Web/FountainBistro.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN mkdir -p /app/Data /app/logs
COPY --from=build /app/publish .

# Диагностика
RUN ls -la /app/

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

# Используем полный путь
ENTRYPOINT ["dotnet", "/app/FountainBistro.Web.dll"]
