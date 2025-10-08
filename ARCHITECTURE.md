# Архитектура BlackJackCamera

## Общая схема

```
┌──────────────────────────────────────────────────────────────────┐
│                        Mobile Application                         │
│                         (.NET MAUI 9.0)                          │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌─────────────┐    ┌──────────────┐    ┌─────────────────┐    │
│  │  MainPage   │───▶│  CameraPage  │───▶│ Image Capture   │    │
│  │   (Home)    │    │              │    │                 │    │
│  └─────────────┘    └──────────────┘    └─────────────────┘    │
│                            │                      │              │
│                            ▼                      ▼              │
│                   ┌─────────────────────────────────────┐       │
│                   │  ImageCompressionService            │       │
│                   │  • Resize to 1920x1080              │       │
│                   │  • JPEG compression (75% quality)   │       │
│                   │  • ~200KB output                    │       │
│                   └─────────────────────────────────────┘       │
│                            │                                     │
│                            ▼                                     │
│                   ┌─────────────────────────────────────┐       │
│                   │  DetectionApiService                 │       │
│                   │  • HttpClient wrapper                │       │
│                   │  • Multipart/form-data upload        │       │
│                   │  • JSON deserialization              │       │
│                   └─────────────────────────────────────┘       │
│                            │                                     │
└────────────────────────────┼─────────────────────────────────────┘
                             │
                             │ HTTP POST
                             │ /api/detection/detect
                             │ (Compressed JPEG ~200KB)
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│                         VPS Server                                │
│                      (Docker Container)                           │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              ASP.NET Core Web API                          │ │
│  │                 (Port 8080)                                │ │
│  └────────────────────────────────────────────────────────────┘ │
│                            │                                     │
│                            ▼                                     │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │          DetectionController                               │ │
│  │  • Validates image (size, format)                         │ │
│  │  • Handles multipart upload                               │ │
│  │  • Returns JSON response                                  │ │
│  └────────────────────────────────────────────────────────────┘ │
│                            │                                     │
│                            ▼                                     │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │          ImageProcessor                                    │ │
│  │  • Decode image                                            │ │
│  │  • Resize to 640x640                                       │ │
│  │  • Convert to tensor [1,3,640,640]                        │ │
│  │  • Normalize RGB [0-1]                                    │ │
│  └────────────────────────────────────────────────────────────┘ │
│                            │                                     │
│                            ▼                                     │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │        ObjectDetectionService                              │ │
│  │  • ONNX Runtime inference                                 │ │
│  │  • YOLOv8 model execution                                 │ │
│  │  • Parse detections                                       │ │
│  │  • Apply NMS (IoU < 0.45)                                 │ │
│  │  • Filter by confidence (> 0.25)                          │ │
│  └────────────────────────────────────────────────────────────┘ │
│                            │                                     │
│                            ▼                                     │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │          YOLOv8 ONNX Model                                 │ │
│  │  • Model: yolov8x-oiv7.onnx (275MB)                       │ │
│  │  • Classes: 601 objects                                   │ │
│  │  • Input: [1, 3, 640, 640] RGB tensor                     │ │
│  │  • Output: [1, 605, N] detections                         │ │
│  └────────────────────────────────────────────────────────────┘ │
│                            │                                     │
│                            ▼                                     │
│                   JSON Response                                  │
│                   ┌─────────────────────────────────┐           │
│                   │ {                                │           │
│                   │   "detections": [...],           │           │
│                   │   "processingTimeMs": 450,       │           │
│                   │   "success": true                │           │
│                   │ }                                │           │
│                   └─────────────────────────────────┘           │
│                            │                                     │
└────────────────────────────┼─────────────────────────────────────┘
                             │
                             │ HTTP Response
                             │ JSON (~5-50KB)
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│                        Mobile Application                         │
│                                                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │        CategoryBadgeMapper                                 │ │
│  │  • Maps detected classes to banking services              │ │
│  │  • Generates UI badges                                    │ │
│  │  • Shows relevant offers                                  │ │
│  └────────────────────────────────────────────────────────────┘ │
│                            │                                     │
│                            ▼                                     │
│                   Display Bottom Sheet                           │
│                   with Banking Services                          │
└──────────────────────────────────────────────────────────────────┘
```

## Поток данных

### 1. Захват изображения (Frontend)

```
User Action
    │
    ├─► Camera Capture (CommunityToolkit.Maui.Camera)
    │
    └─► Original Image (2-5MB, raw camera output)
```

### 2. Предобработка (Frontend)

```
Original Image
    │
    ├─► ImageCompressionService
    │   ├─ Decode with SkiaSharp
    │   ├─ Calculate proportions
    │   ├─ Resize to max 1920x1080
    │   └─ Encode to JPEG (75% quality)
    │
    └─► Compressed Image (~200KB, JPEG)
```

### 3. Отправка на Backend (Frontend)

```
Compressed Image
    │
    ├─► DetectionApiService
    │   ├─ Create MultipartFormDataContent
    │   ├─ HTTP POST to /api/detection/detect
    │   └─ Set timeout 30s
    │
    └─► Network Transfer (~200KB)
```

### 4. Обработка на Backend

```
HTTP Request
    │
    ├─► DetectionController
    │   ├─ Validate file size (max 10MB)
    │   ├─ Validate content type (JPEG/PNG)
    │   └─ Extract image stream
    │
    ├─► ImageProcessor
    │   ├─ Decode image (SkiaSharp)
    │   ├─ Resize to 640x640
    │   ├─ Convert to tensor [1,3,640,640]
    │   └─ Normalize RGB to [0,1]
    │
    ├─► ObjectDetectionService
    │   ├─ Create ONNX input
    │   ├─ Run inference
    │   ├─ Parse output tensor
    │   ├─ Filter by confidence > 0.25
    │   └─ Apply NMS (IoU < 0.45)
    │
    └─► JSON Response
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
          "success": true
        }
```

### 5. Отображение результатов (Frontend)

```
JSON Response
    │
    ├─► DetectionApiService
    │   └─ Deserialize JSON to DetectionResponseDto
    │
    ├─► CameraPage
    │   └─ Convert DTOs to local Detection objects
    │
    ├─► CategoryBadgeMapper
    │   ├─ Map ClassId → Banking Category
    │   ├─ Generate Badge objects
    │   └─ Filter duplicates
    │
    └─► UI Display
        ├─ Show Bottom Sheet
        ├─ Render badges with FlexLayout
        └─ Animate slide-up
```

## Компоненты

### Backend (ASP.NET Core Web API)

| Компонент | Ответственность | Технология |
|-----------|-----------------|------------|
| **DetectionController** | HTTP endpoints, validation | ASP.NET Core MVC |
| **ModelLoaderService** | Load ONNX model & labels | ONNX Runtime |
| **ImageProcessor** | Resize, tensor conversion | SkiaSharp |
| **ObjectDetectionService** | YOLOv8 inference, NMS | ONNX Runtime |
| **Program.cs** | DI, CORS, startup init | ASP.NET Core |

### Frontend (.NET MAUI)

| Компонент | Ответственность | Технология |
|-----------|-----------------|------------|
| **MainPage** | Home screen, navigation | XAML, MAUI |
| **CameraPage** | Camera UI, capture | CommunityToolkit.Maui.Camera |
| **DetectionApiService** | HTTP client for API | HttpClient |
| **ImageCompressionService** | Image optimization | SkiaSharp |
| **CategoryBadgeMapper** | Business logic mapping | C# |
| **MauiProgram** | DI, configuration | .NET MAUI |

## Оптимизации

### Сетевой трафик

- ✅ **Сжатие изображений**: 2-5MB → ~200KB (10-25x уменьшение)
- ✅ **JPEG формат**: Optimal для фотографий
- ✅ **Качество 75%**: Баланс между размером и качеством
- ✅ **Responsive resize**: Сохранение пропорций

### Производительность Backend

- ✅ **Singleton ONNX Session**: Модель загружается 1 раз
- ✅ **Scoped ImageProcessor**: Переиспользование для каждого запроса
- ✅ **NMS оптимизация**: Удаление дубликатов детекций
- ✅ **Async/await**: Неблокирующие операции

### Производительность Frontend

- ✅ **HttpClient pooling**: Переиспользование соединений
- ✅ **Async image processing**: Не блокирует UI
- ✅ **Local caching**: CategoryBadgeMapper stateless
- ✅ **Lazy initialization**: Сервисы инициализируются по требованию

## Масштабирование

### Горизонтальное масштабирование

```
┌──────────┐      ┌──────────────────┐      ┌──────────┐
│  Mobile  │─────▶│  Load Balancer   │─────▶│  API #1  │
│   Apps   │      │   (Nginx/HAProxy)│      ├──────────┤
└──────────┘      └──────────────────┘      │  API #2  │
                                             ├──────────┤
                                             │  API #3  │
                                             └──────────┘
```

### Вертикальное масштабирование

- Увеличить CPU/RAM для Docker контейнера
- Использовать GPU для ONNX Runtime (опционально)
- Оптимизировать размер модели (YOLOv8s/m вместо YOLOv8x)

## Безопасность

### Текущие меры

- ✅ CORS настроен для мобильных приложений
- ✅ Валидация размера файла (max 10MB)
- ✅ Валидация типа файла (JPEG/PNG only)
- ✅ Timeout запросов (30s)

### Рекомендации для Production

- 🔒 JWT авторизация
- 🔒 API Keys
- 🔒 Rate Limiting (X запросов/минуту)
- 🔒 HTTPS обязателен
- 🔒 Input sanitization
- 🔒 Firewall rules

## Мониторинг

### Метрики

- **processingTimeMs**: Время обработки каждого запроса
- **Success rate**: Процент успешных запросов
- **Error types**: Категоризация ошибок
- **Network traffic**: Входящий/исходящий трафик

### Логирование

- **Backend**: Structured logging (Serilog рекомендуется)
- **Frontend**: Debug logging через ILogger
- **Docker**: Centralized logs через docker-compose logs

## Развертывание

### Development

```bash
# Backend
cd BlackJackCamera/BlackJackCamera.Api
dotnet run

# Frontend
cd BlackJackCamera/BlackJackCamera
dotnet build -t:Run -f net9.0-android
```

### Production

```bash
# Build Docker image
docker-compose build

# Deploy
docker-compose up -d

# Monitor
docker-compose logs -f
```

## Будущие улучшения

### Backend
- [ ] Redis кэширование результатов
- [ ] GPU поддержка для ONNX Runtime
- [ ] WebSocket для real-time detection
- [ ] Batch processing для нескольких изображений

### Frontend
- [ ] Offline mode с локальной моделью (опционально)
- [ ] Прогрессивное сжатие изображений
- [ ] Кэширование результатов
- [ ] Retry механизм с exponential backoff

### DevOps
- [ ] CI/CD pipeline
- [ ] Kubernetes deployment
- [ ] Prometheus + Grafana мониторинг
- [ ] Automated testing
