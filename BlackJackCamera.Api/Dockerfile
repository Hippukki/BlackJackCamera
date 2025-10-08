# Используем образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем csproj и nuget.config
COPY ["BlackJackCamera.Api.csproj", "./"]
COPY ["nuget.config", "./"]
# Увеличиваем таймаут NuGet для медленных соединений
ENV NUGET_HTTPCONNECTION_TIMEOUT=300
RUN dotnet restore "BlackJackCamera.Api.csproj" --disable-parallel

# Копируем остальные файлы и собираем проект
COPY . .
RUN dotnet build "BlackJackCamera.Api.csproj" -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish "BlackJackCamera.Api.csproj" \
    -c Release \
    -o /app/publish \
    -r linux-x64 \
    --self-contained false \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=false

# Создаем финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Устанавливаем зависимости для SkiaSharp и ONNX Runtime
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    libx11-6 \
    libxcb1 \
    libxext6 \
    libxrender1 \
    libc6-dev \
    libgdiplus \
    && rm -rf /var/lib/apt/lists/*

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV LD_LIBRARY_PATH=/app/runtimes/linux-x64/native:$LD_LIBRARY_PATH

# Открываем порт
EXPOSE 8080

# Копируем опубликованное приложение
COPY --from=publish /app/publish .

# ВАЖНО: Модель yolov8x-oiv7.onnx должна быть размещена в Resources/Models
# Можно загрузить через volume или скопировать при сборке образа
# Для production используйте volume mount для модели (275MB)

ENTRYPOINT ["dotnet", "BlackJackCamera.Api.dll"]
