FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем все файлы
COPY . .

# Восстанавливаем зависимости
RUN dotnet restore "FountainBistro.Web/FountainBistro.Web.csproj"

# Публикуем проект
RUN dotnet publish "FountainBistro.Web/FountainBistro.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

# Создаем папки
RUN mkdir -p /app/Data /app/logs

# Копируем все из publish
COPY --from=build /app/publish .

# Проверяем наличие файлов
RUN echo "=== Файлы в /app ===" && ls -la /app/

# Проверяем наличие dll
RUN test -f /app/FountainBistro.Web.dll || (echo "❌ DLL не найдена!" && exit 1)

# Переменные окружения
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Запускаем приложение
CMD ["dotnet", "FountainBistro.Web.dll"]
