# Развертывание через Docker Hub

Этот метод используется когда на сервере нет доступа к NuGet.org для сборки образа.

## Процесс развертывания

### 1. Локальная сборка на Windows (разработчик)

```bash
# Перейдите в папку проекта
cd E:\BlackJackCamera\BlackJackCamera

# Соберите Docker образ локально
docker build -t YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:latest -f BlackJackCamera.Api/Dockerfile BlackJackCamera.Api

# Войдите в Docker Hub
docker login

# Опубликуйте образ
docker push YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:latest
```

**Замените** `YOUR_DOCKERHUB_USERNAME` на ваш логин в Docker Hub.

### 2. Настройка на сервере (Ubuntu 24.04)

#### 2.1 Обновите docker-compose.yml

Отредактируйте файл `docker-compose.yml` на сервере:

```bash
nano docker-compose.yml
```

Замените секцию `build:` на `image:`:

```yaml
services:
  blackjackcamera-api:
    # Используем pre-built образ из Docker Hub
    image: YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:latest

    # Закомментируйте или удалите секцию build:
    # build:
    #   context: ./BlackJackCamera.Api
    #   dockerfile: Dockerfile

    container_name: blackjackcamera-api
    ports:
      - "8080:8080"
    # ... остальная конфигурация без изменений
```

#### 2.2 Загрузите образ и запустите

```bash
# Загрузите образ из Docker Hub
docker-compose pull

# Запустите контейнер
docker-compose up -d

# Проверьте статус
docker-compose ps

# Проверьте логи
docker-compose logs -f
```

#### 2.3 Проверка работоспособности

```bash
# Health check
curl http://localhost:8080/api/detection/health

# Должен вернуть:
# {"status":"Healthy","timestamp":"2025-10-09T..."}
```

### 3. Обновление образа

Когда вы обновили код и собрали новый образ:

**На Windows:**
```bash
# Пересоберите и опубликуйте
docker build -t YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:latest -f BlackJackCamera.Api/Dockerfile BlackJackCamera.Api
docker push YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:latest
```

**На сервере:**
```bash
# Загрузите новый образ и перезапустите
docker-compose pull
docker-compose up -d

# Проверьте обновление
docker-compose logs -f
```

### 4. Использование тегов версий (рекомендуется)

Вместо `latest` используйте версии:

**На Windows:**
```bash
# Соберите с тегом версии
docker build -t YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:1.0.0 -f BlackJackCamera.Api/Dockerfile BlackJackCamera.Api
docker build -t YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:latest -f BlackJackCamera.Api/Dockerfile BlackJackCamera.Api

# Опубликуйте обе версии
docker push YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:1.0.0
docker push YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:latest
```

**docker-compose.yml:**
```yaml
services:
  blackjackcamera-api:
    image: YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:1.0.0
```

## Преимущества этого метода

✅ Не требуется доступ к NuGet.org на сервере
✅ Быстрое развертывание (образ уже собран)
✅ Контроль версий через теги
✅ Можно откатиться на предыдущую версию

## Размер образа

Ожидаемый размер: ~450-600 MB (ASP.NET Core runtime + ONNX Runtime + зависимости)

Модель YOLOv8 (275 MB) **не включена** в образ - она монтируется через volume.

## Безопасность

**Публичный образ:**
```yaml
image: YOUR_DOCKERHUB_USERNAME/blackjackcamera-api:latest
```

**Приватный образ (требует docker login на сервере):**
```bash
# На сервере войдите в Docker Hub
docker login

# Затем pull будет работать с приватными образами
docker-compose pull
```

## Troubleshooting

### Ошибка: "pull access denied"

Образ приватный, нужно войти:
```bash
docker login
docker-compose pull
```

### Ошибка: "manifest unknown"

Образ не опубликован или неправильное имя:
```bash
# Проверьте имя образа
docker images | grep blackjackcamera
```

### Медленная загрузка образа

Зависит от скорости интернета на сервере. Образ ~500MB может загружаться 5-15 минут.

## Полная последовательность действий

1. **Windows (локально):**
   ```bash
   cd E:\BlackJackCamera\BlackJackCamera
   docker build -t fedoseevpv/blackjackcamera-api:latest -f BlackJackCamera.Api/Dockerfile BlackJackCamera.Api
   docker login
   docker push fedoseevpv/blackjackcamera-api:latest
   ```

2. **Коммит в Git:**
   ```bash
   git add docker-compose.yml
   git commit -m "Update docker-compose to use Docker Hub image"
   git push
   ```

3. **Сервер:**
   ```bash
   cd /root/BlackJackCamera
   git pull

   # Отредактируйте docker-compose.yml: замените YOUR_DOCKERHUB_USERNAME
   nano docker-compose.yml

   docker-compose pull
   docker-compose up -d

   # Проверка
   curl http://localhost:8080/api/detection/health
   ./scripts/monitor.sh
   ```

Готово! 🚀
