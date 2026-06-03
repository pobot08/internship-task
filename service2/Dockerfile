# Этап 1 — сборка
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем файл проекта и восстанавливаем пакеты
COPY *.csproj .
RUN dotnet restore

# Копируем весь код и собираем
COPY . .
RUN dotnet publish -c Release -o /out

# Этап 2 — финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Копируем собранное приложение из этапа 1
COPY --from=build /out .

# Создаём папку для Excel файлов
RUN mkdir -p /data/excel

# Порт на котором работает сервис
EXPOSE 8080

# Запускаем
ENTRYPOINT ["dotnet", "service2.dll"]