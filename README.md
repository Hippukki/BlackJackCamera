# BlackJackCamera - Разделенная архитектура Frontend/Backend

Проект разделен на два независимых компонента:
- **Backend (API)**: ASP.NET Core Web API с YOLOv8 моделью для детекции объектов
- **Frontend (Mobile)**: .NET MAUI мобильное приложение

## Архитектура

```
┌─────────────────────┐         HTTP/HTTPS          ┌─────────────────────┐
│                     │   ──────────────────────►   │                     │
│  MAUI Mobile App    │                             │   ASP.NET Core API  │
│  (Frontend)         │   ◄──────────────────────   │   (Backend)         │
│                     │      JSON Response          │                     │
└─────────────────────┘                             └─────────────────────┘
         │                                                    │
         │ Сжимает изображение                               │ YOLOv8 ML Model
         │ (JPEG, 75%, 1920x1080)                           │ (ONNX Runtime)
         │                                                    │
         └─ Отправляет через multipart/form-data            └─ Детекция объектов
```

## Backend API

### Структура проекта

```
BlackJackCamera.Api/
├── Controllers/
│   └── DetectionController.cs      # API endpoints
├── Services/
│   ├── ModelLoaderService.cs       # Загрузка ONNX модели
│   ├── ObjectDetectionService.cs   # YOLOv8 детекция
│   └── ImageProcessor.cs           # Обработка изображений
├── Interfaces/
│   ├── IModelLoaderService.cs
│   ├── IObjectDetectionService.cs
│   └── IImageProcessor.cs
├── Models/
│   ├── Detection.cs
│   └── DetectionResponse.cs
├── Resources/Models/
│   ├── yolov8x-oiv7.onnx          # ML модель (275MB)
│   └── labels.txt                  # 601 класс объектов
├── Dockerfile
└── Program.cs
```

### API Endpoints

#### POST /api/detection/detect
Распознает объекты на изображении

**Request:**
- Content-Type: multipart/form-data
- Body: file (JPEG/PNG, макс 10MB)

**Response:**
```json
{
  "detections": [
    {
      "x": 320.5,
      "y": 240.3,
      "width": 150.2,
      "height": 200.1,
      "confidence": 0.89,
      "classId": 381,
      "className": "Person"
    }
  ],
  "processingTimeMs": 450,
  "success": true,
  "errorMessage": null
}
```

#### GET /api/detection/health
Проверка состояния сервиса

### Запуск Backend

#### Локально

```bash
cd BlackJackCamera/BlackJackCamera.Api
dotnet restore
dotnet run
```

API: `http://localhost:5000`
Swagger: `http://localhost:5000/swagger`

#### Docker

```bash
cd BlackJackCamera
docker-compose up -d
```

API: `http://localhost:8080`

### Требования к системе (Backend)
- CPU: 2+ ядра
- RAM: 2GB+ (рекомендуется 4GB)
- Диск: 500MB + модель (275MB)

## Frontend (MAUI Mobile)

### Структура проекта

```
BlackJackCamera/
├── Models/
│   ├── DetectionDto.cs
│   └── DetectionResponseDto.cs
├── Services/
│   ├── DetectionApiService.cs          # HTTP клиент для API
│   ├── ImageCompressionService.cs      # Сжатие изображений
│   └── CategoryBadgeMapper.cs          # Маппинг на банковские услуги
├── Interfaces/
│   ├── IDetectionApiService.cs
│   └── IImageCompressionService.cs
├── Pages/
│   ├── MainPage.xaml                   # Главная страница
│   └── CameraPage.xaml                 # Страница камеры
├── appsettings.json                    # Конфигурация API URL
└── MauiProgram.cs
```

### Конфигурация

Файл `appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8080",
    "Timeout": 30
  },
  "ImageCompression": {
    "MaxWidth": 1920,
    "MaxHeight": 1080,
    "Quality": 75
  }
}
```

**Важно:** Для production замените `BaseUrl` на URL вашего VPS сервера.

### Оптимизация передачи данных

Frontend автоматически сжимает изображения перед отправкой:
- **Формат:** JPEG
- **Качество:** 75% (настраивается)
- **Макс. разрешение:** 1920x1080 (настраивается)
- **Средний размер:** 150-300KB вместо 2-5MB

### Запуск Frontend

```bash
cd BlackJackCamera/BlackJackCamera
dotnet build -f net9.0-android
dotnet build -t:Run -f net9.0-android
```

## Развертывание на VPS

### Предварительные требования

- Ubuntu 20.04+ / Debian 11+
- Docker и Docker Compose
- Открытый порт 8080 (или настройте Nginx)

### Установка Docker

```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

### Развертывание

1. Клонируйте репозиторий:
```bash
git clone <your-repo-url>
cd BlackJackCamera
```

2. Убедитесь, что модель находится в правильной директории:
```bash
ls -lh BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx
```

3. Запустите:
```bash
docker-compose up -d
```

4. Проверьте логи:
```bash
docker-compose logs -f
```

5. Проверьте health endpoint:
```bash
curl http://localhost:8080/api/detection/health
```

### Настройка Nginx (опционально)

Для SSL и reverse proxy:

```nginx
server {
    listen 80;
    server_name your-domain.com;

    client_max_body_size 10M;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Затем настройте SSL с помощью Let's Encrypt:
```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d your-domain.com
```

### Обновление Frontend для production

В `appsettings.json` измените URL:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://your-domain.com",
    "Timeout": 30
  }
}
```

## Преимущества разделенной архитектуры

### Производительность
- ✅ ML модель работает на мощном сервере, а не на мобильном устройстве
- ✅ Оптимизированная передача данных (JPEG 75%, ~200KB вместо 5MB RAW)
- ✅ Клиент не тратит ресурсы на инференс

### Масштабируемость
- ✅ Можно запустить несколько экземпляров API с load balancer
- ✅ Централизованное обновление модели (не нужно обновлять все приложения)
- ✅ Кэширование результатов на backend

### Безопасность
- ✅ ML модель защищена на сервере
- ✅ Можно добавить авторизацию через JWT/API Keys
- ✅ Rate limiting для защиты от злоупотреблений

### Разработка
- ✅ Независимая разработка frontend и backend команд
- ✅ Проще тестирование (mock API для frontend)
- ✅ Возможность использовать API для веб-версии приложения

## Мониторинг и логирование

### Backend логи
```bash
docker-compose logs -f blackjackcamera-api
```

### Метрики производительности
API возвращает `processingTimeMs` в каждом ответе для мониторинга скорости обработки.

### Health checks
Настроен в docker-compose.yml с проверкой каждые 30 секунд.

## Безопасность

### Рекомендации для production:

1. **API Keys:** Добавьте авторизацию через API ключи
2. **Rate Limiting:** Ограничьте количество запросов с одного IP
3. **HTTPS:** Обязательно используйте SSL сертификат
4. **Firewall:** Настройте UFW для ограничения доступа к портам
5. **Secrets:** Используйте переменные окружения для секретов

## Troubleshooting

### Backend не запускается
- Проверьте наличие модели: `ls BlackJackCamera/BlackJackCamera/Resources/Raw/`
- Проверьте логи: `docker-compose logs`
- Убедитесь, что порт 8080 свободен: `netstat -tuln | grep 8080`

### Frontend не подключается к API
- Проверьте `appsettings.json` - правильный ли URL
- Проверьте доступность API: `curl http://your-api-url/api/detection/health`
- Для Android эмулятора используйте `http://10.0.2.2:8080` вместо `localhost`

### Медленная обработка
- Убедитесь, что изображения сжимаются (проверьте размер в Network Inspector)
- Увеличьте ресурсы Docker контейнера в docker-compose.yml
- Проверьте `processingTimeMs` в ответе API

## Лицензия

Проприетарное ПО

## Авторы

BlackJackCamera Team
