# Этап 1: Сборка
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем solution и csproj файлы
COPY FountainBistro.sln .
COPY FountainBistro.Web/*.csproj FountainBistro.Web/
COPY FountainBistro.Tests/*.csproj FountainBistro.Tests/

# Восстанавливаем зависимости
RUN dotnet restore

# Копируем весь код
COPY FountainBistro.Web/ FountainBistro.Web/
COPY FountainBistro.Tests/ FountainBistro.Tests/

# Собираем проект
WORKDIR /src/FountainBistro.Web
RUN dotnet build -c Release --no-restore

# Публикуем приложение
RUN dotnet publish -c Release -o /app/publish --no-restore

# Этап 2: Запуск
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Устанавливаем необходимые пакеты для работы с SQLite
RUN apt-get update && apt-get install -y \
    sqlite3 \
    && rm -rf /var/lib/apt/lists/*

# Создаем папки для логов и данных
RUN mkdir -p /app/logs /app/Data && chmod 755 /app/logs /app/Data

# Копируем опубликованное приложение
COPY --from=build /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_USE_POLLING_FILE_WATCHER=1

# Открываем порт
EXPOSE 8080

# Точка входа - запускаем DLL
ENTRYPOINT ["dotnet", "FountainBistro.Web.dll"]
