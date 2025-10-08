# BlackJackCamera API

Backend Web API для распознавания объектов с использованием YOLOv8.

## Установка и запуск

### Локальный запуск

1. Убедитесь, что файл модели `yolov8x-oiv7.onnx` (275MB) находится в `Resources/Models/`
2. Восстановите зависимости:
```bash
dotnet restore
```

3. Запустите проект:
```bash
dotnet run
```

API будет доступен по адресу: `http://localhost:5000`

Swagger UI: `http://localhost:5000/swagger`

### Docker

#### Сборка образа

```bash
cd BlackJackCamera/BlackJackCamera.Api
docker build -t blackjackcamera-api .
```

#### Запуск контейнера

```bash
docker run -d \
  -p 8080:8080 \
  -v /path/to/models:/app/Resources/Models:ro \
  --name blackjackcamera-api \
  blackjackcamera-api
```

#### Docker Compose (рекомендуется)

Из корневой директории проекта:

```bash
docker-compose up -d
```

API будет доступен по адресу: `http://localhost:8080`

## API Endpoints

### POST /api/detection/detect

Распознает объекты на загруженном изображении.

**Параметры:**
- `file` (multipart/form-data) - Изображение в формате JPEG/PNG (макс. 10MB)

**Рекомендации по передаче изображений:**
- Формат: JPEG
- Качество: 70-80%
- Максимальное разрешение: 1920x1080
- Клиент должен сжимать изображение перед отправкой

**Ответ:**
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

### GET /api/detection/health

Проверка состояния сервиса.

**Ответ:**
```json
{
  "status": "healthy",
  "message": "Object detection service is ready",
  "classesCount": 601,
  "timestamp": "2025-10-08T12:00:00Z"
}
```

## Требования к системе

- **CPU:** Минимум 2 ядра (рекомендуется 4+)
- **RAM:** Минимум 2GB (рекомендуется 4GB+)
- **Диск:** 500MB + место для модели (275MB)

## Оптимизация производительности

1. **Размер изображения:** Клиент должен отправлять изображения размером не более 1920x1080
2. **Сжатие:** Используйте JPEG с качеством 70-80%
3. **Кэширование:** Модель загружается один раз при старте приложения
4. **Масштабирование:** Для высоких нагрузок используйте несколько экземпляров API с load balancer

## Развертывание на VPS

### Требования
- Ubuntu 20.04+ / Debian 11+
- Docker и Docker Compose установлены
- Открыт порт 8080

### Установка Docker

```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
```

### Установка Docker Compose

```bash
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

### Развертывание

1. Клонируйте репозиторий:
```bash
git clone <your-repo-url>
cd BlackJackCamera
```

2. Убедитесь, что модель находится в `BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx`

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

Для проксирования и SSL:

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

## Мониторинг

Используйте `/api/detection/health` для health checks в Kubernetes, Docker Swarm или мониторинговых системах.

## Лицензия

Проприетарное ПО
