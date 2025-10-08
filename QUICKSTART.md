# Быстрый старт

## 1️⃣ Запуск Backend (API)

### Вариант А: Docker (рекомендуется)

```bash
# В корне проекта
docker-compose up -d

# Проверка
curl http://localhost:8080/api/detection/health
```

### Вариант Б: Локально

```bash
cd BlackJackCamera/BlackJackCamera.Api
dotnet restore
dotnet run

# API доступен на http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

## 2️⃣ Настройка Frontend

Откройте `BlackJackCamera/BlackJackCamera/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8080"  // или ваш VPS URL
  }
}
```

### Для Android эмулятора:
```json
{
  "ApiSettings": {
    "BaseUrl": "http://10.0.2.2:8080"
  }
}
```

### Для физического устройства:
```json
{
  "ApiSettings": {
    "BaseUrl": "http://192.168.x.x:8080"  // IP вашего компьютера
  }
}
```

## 3️⃣ Запуск Frontend

```bash
cd BlackJackCamera/BlackJackCamera

# Android
dotnet build -f net9.0-android
dotnet build -t:Run -f net9.0-android

# iOS (только на macOS)
dotnet build -f net9.0-ios -t:Run

# Windows
dotnet build -f net9.0-windows10.0.19041.0 -t:Run
```

## 4️⃣ Тестирование

1. Откройте приложение
2. Нажмите на кнопку сканера
3. Сделайте фото объекта
4. Приложение отправит изображение на backend
5. Получите результаты распознавания

## 🚀 Развертывание на VPS

```bash
# На сервере
git clone <your-repo>
cd BlackJackCamera
docker-compose up -d

# Проверка
curl http://localhost:8080/api/detection/health
```

Затем обновите `BaseUrl` в мобильном приложении на URL вашего сервера.

## 📊 Мониторинг

```bash
# Логи backend
docker-compose logs -f

# Статус контейнера
docker-compose ps

# Остановка
docker-compose down
```

## ⚠️ Требования

- **.NET 9.0 SDK**
- **Docker** (для backend в контейнере)
- **Android SDK** или **Xcode** (для мобильной разработки)
- **Модель YOLOv8**: `yolov8x-oiv7.onnx` (275MB) должна быть в `BlackJackCamera/Resources/Raw/`

## 🔧 Troubleshooting

**Проблема:** Backend не находит модель
**Решение:** Убедитесь, что `yolov8x-oiv7.onnx` находится в `BlackJackCamera/BlackJackCamera/Resources/Raw/`

**Проблема:** Frontend не подключается к API
**Решение:**
- Проверьте URL в `appsettings.json`
- Для эмулятора используйте `10.0.2.2:8080` вместо `localhost:8080`
- Убедитесь, что backend запущен

**Проблема:** "Connection refused"
**Решение:**
- Проверьте firewall
- Убедитесь, что порт 8080 открыт
- Для физического устройства используйте IP компьютера в одной сети

## 📝 Дополнительная информация

Полная документация: [README.md](README.md)
Backend API: [BlackJackCamera.Api/README.md](BlackJackCamera/BlackJackCamera.Api/README.md)
