FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем всё
COPY . .

# Восстанавливаем зависимости
RUN dotnet restore "FountainBistro.Web/FountainBistro.Web.csproj"

# Собираем и публикуем в папку /app/publish
RUN dotnet publish "FountainBistro.Web/FountainBistro.Web.csproj" -c Release -o /app/publish

# Финальный образ с ASP.NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Создаем папки для данных и логов
RUN mkdir -p /app/Data /app/logs

# Копируем опубликованные файлы
COPY --from=build /app/publish .

# Проверяем наличие dll
RUN ls -la /app/ && test -f /app/FountainBistro.Web.dll && echo "✅ DLL found" || echo "❌ DLL NOT FOUND"

# Переменные окружения
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "FountainBistro.Web.dll"]
