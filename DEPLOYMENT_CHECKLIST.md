# ✅ Чеклист развертывания на Ubuntu 24.04

Используйте этот чеклист для быстрого развертывания BlackJackCamera API.

---

## 📋 Предварительные требования

- [ ] VPS с Ubuntu 24.04 LTS
- [ ] SSH доступ с правами sudo
- [ ] Домен направлен на IP сервера
- [ ] Минимум 20GB свободного места

**Характеристики вашего сервера:**
- ✅ CPU: 8 ядер, 5.1 ГГц
- ✅ RAM: 12 GB DDR4
- ✅ Storage: 20 GB SSD NVMe
- ✅ OS: Ubuntu 24.04 LTS

---

## 🚀 Быстрое развертывание (5 команд)

```bash
# 1. Подключитесь к серверу
ssh your-user@your-server-ip

# 2. Клонируйте quick-deploy скрипт и запустите
wget https://raw.githubusercontent.com/your-repo/BlackJackCamera/main/deploy-vps.sh
chmod +x deploy-vps.sh
./deploy-vps.sh

# 3. Настройте Nginx и SSL (следуйте инструкциям скрипта)

# 4. Проверьте работоспособность
curl https://your-domain.com/api/detection/health

# 5. Обновите URL в мобильном приложении
# Измените appsettings.json: "BaseUrl": "https://your-domain.com"
```

---

## 📝 Пошаговый чеклист

### Этап 1: Подготовка сервера (10 минут)

- [ ] **1.1** Подключитесь к серверу через SSH
  ```bash
  ssh root@your-server-ip
  ```

- [ ] **1.2** Обновите систему
  ```bash
  apt update && apt upgrade -y
  ```

- [ ] **1.3** Установите базовые утилиты
  ```bash
  apt install -y curl wget git vim htop jq
  ```

- [ ] **1.4** Настройте timezone
  ```bash
  timedatectl set-timezone Europe/Moscow
  ```

### Этап 2: Установка Docker (5 минут)

- [ ] **2.1** Установите Docker
  ```bash
  curl -fsSL https://get.docker.com -o get-docker.sh
  sh get-docker.sh
  ```

- [ ] **2.2** Добавьте пользователя в группу docker
  ```bash
  usermod -aG docker $USER
  newgrp docker
  ```

- [ ] **2.3** Проверьте установку
  ```bash
  docker --version
  docker compose version
  ```

### Этап 3: Клонирование проекта (2 минуты)

- [ ] **3.1** Перейдите в домашнюю директорию
  ```bash
  cd ~
  ```

- [ ] **3.2** Клонируйте репозиторий
  ```bash
  git clone https://github.com/your-username/BlackJackCamera.git
  cd BlackJackCamera
  ```

- [ ] **3.3** Проверьте наличие ML модели
  ```bash
  ls -lh BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx
  # Должен быть файл ~275MB
  ```

### Этап 4: Настройка Firewall (2 минуты)

- [ ] **4.1** Установите UFW
  ```bash
  apt install -y ufw
  ```

- [ ] **4.2** Разрешите необходимые порты
  ```bash
  ufw allow 22/tcp   # SSH
  ufw allow 80/tcp   # HTTP
  ufw allow 443/tcp  # HTTPS
  ```

- [ ] **4.3** Включите firewall
  ```bash
  ufw enable
  ufw status
  ```

### Этап 5: Запуск API (3 минуты)

- [ ] **5.1** Соберите Docker образ
  ```bash
  docker compose build
  ```

- [ ] **5.2** Запустите контейнер
  ```bash
  docker compose up -d
  ```

- [ ] **5.3** Дождитесь инициализации (45 сек)
  ```bash
  sleep 45
  ```

- [ ] **5.4** Проверьте health endpoint
  ```bash
  curl http://localhost:8080/api/detection/health
  ```

  **Ожидаемый ответ:**
  ```json
  {
    "status": "healthy",
    "message": "Object detection service is ready",
    "classesCount": 601
  }
  ```

### Этап 6: Настройка Nginx (5 минут)

- [ ] **6.1** Установите Nginx
  ```bash
  apt install -y nginx
  ```

- [ ] **6.2** Скопируйте конфигурацию
  ```bash
  cp nginx/blackjackcamera.conf /etc/nginx/sites-available/blackjackcamera
  ```

- [ ] **6.3** Отредактируйте конфигурацию
  ```bash
  vim /etc/nginx/sites-available/blackjackcamera
  # Замените your-domain.com на ваш домен
  ```

- [ ] **6.4** Включите сайт
  ```bash
  ln -s /etc/nginx/sites-available/blackjackcamera /etc/nginx/sites-enabled/
  nginx -t
  systemctl reload nginx
  ```

### Этап 7: Настройка SSL (3 минуты)

- [ ] **7.1** Установите Certbot
  ```bash
  apt install -y certbot python3-certbot-nginx
  ```

- [ ] **7.2** Получите SSL сертификат
  ```bash
  certbot --nginx -d your-domain.com -d www.your-domain.com
  ```

- [ ] **7.3** Проверьте HTTPS
  ```bash
  curl https://your-domain.com/api/detection/health
  ```

### Этап 8: Тестирование (5 минут)

- [ ] **8.1** Проверьте статус контейнера
  ```bash
  docker ps
  ```

- [ ] **8.2** Проверьте логи
  ```bash
  docker compose logs --tail 50
  ```

- [ ] **8.3** Запустите мониторинг
  ```bash
  chmod +x scripts/monitor.sh
  ./scripts/monitor.sh
  ```

- [ ] **8.4** Тестовый запрос с изображением
  ```bash
  wget https://via.placeholder.com/640 -O test.jpg
  curl -X POST https://your-domain.com/api/detection/detect \
    -F "file=@test.jpg"
  ```

### Этап 9: Настройка автозапуска (2 минуты)

- [ ] **9.1** Проверьте политику перезапуска
  ```bash
  docker inspect blackjackcamera-api | grep RestartPolicy
  ```

- [ ] **9.2** Протестируйте перезагрузку
  ```bash
  reboot
  # После перезагрузки:
  docker ps
  curl https://your-domain.com/api/detection/health
  ```

### Этап 10: Обновление Frontend (5 минут)

- [ ] **10.1** Откройте `appsettings.json` в проекте MAUI
  ```json
  {
    "ApiSettings": {
      "BaseUrl": "https://your-domain.com"
    }
  }
  ```

- [ ] **10.2** Пересоберите приложение
  ```bash
  dotnet build -f net9.0-android
  ```

- [ ] **10.3** Запустите и протестируйте
  - Сделайте фото
  - Проверьте распознавание

---

## ✅ Финальная проверка

После выполнения всех шагов проверьте:

### Доступность API

```bash
# Health check
curl -I https://your-domain.com/api/detection/health
# Ожидается: HTTP/2 200

# Тест распознавания
curl -X POST https://your-domain.com/api/detection/detect \
  -F "file=@test-image.jpg" | jq '.success'
# Ожидается: true
```

### Производительность

```bash
# Использование ресурсов
docker stats --no-stream blackjackcamera-api

# Ожидается:
# CPU: 10-30% (в idle)
# RAM: 4-6 GB
```

### Безопасность

```bash
# Firewall
ufw status
# Ожидается: Status: active

# SSL сертификат
openssl s_client -connect your-domain.com:443 -servername your-domain.com < /dev/null 2>/dev/null | grep 'Verify return code'
# Ожидается: Verify return code: 0 (ok)
```

### Логи

```bash
# Нет критических ошибок
docker compose logs --tail 100 | grep -i error | wc -l
# Ожидается: 0 или минимальное количество
```

---

## 📊 Ожидаемые показатели

С вашими характеристиками сервера:

| Параметр | Значение |
|----------|----------|
| **Время инициализации** | ~45 секунд |
| **Время обработки (1 запрос)** | 200-400 ms |
| **Одновременные запросы** | До 50 |
| **Throughput** | 150-250 изображений/мин |
| **CPU в idle** | 10-20% |
| **CPU под нагрузкой** | 40-60% |
| **RAM** | 4-6 GB |
| **Размер Docker образа** | ~500 MB |

---

## 🔧 Полезные команды

### Управление

```bash
# Перезапуск
docker compose restart

# Остановка
docker compose down

# Обновление
./scripts/update.sh

# Бэкап
./scripts/backup.sh

# Мониторинг
./scripts/monitor.sh
```

### Логи

```bash
# В реальном времени
docker compose logs -f

# Последние 100 строк
docker compose logs --tail 100

# Только ошибки
docker compose logs | grep -i error
```

### Диагностика

```bash
# Статистика контейнера
docker stats blackjackcamera-api

# Системные ресурсы
htop

# Сетевые подключения
ss -tn state established '( dport = :8080 )'
```

---

## ⚠️ Troubleshooting

### Проблема: API не отвечает

```bash
# 1. Проверьте статус
docker ps

# 2. Проверьте логи
docker compose logs --tail 50

# 3. Проверьте health изнутри контейнера
docker exec blackjackcamera-api curl http://localhost:8080/api/detection/health

# 4. Перезапустите
docker compose restart
```

### Проблема: SSL не работает

```bash
# 1. Проверьте сертификат
certbot certificates

# 2. Проверьте конфигурацию Nginx
nginx -t

# 3. Обновите сертификат
certbot renew

# 4. Перезагрузите Nginx
systemctl reload nginx
```

### Проблема: Медленная обработка

```bash
# 1. Проверьте нагрузку
docker stats blackjackcamera-api

# 2. Увеличьте ресурсы в docker-compose.yml
cpus: '6' → cpus: '7'
memory: 8G → memory: 10G

# 3. Перезапустите
docker compose down
docker compose up -d
```

---

## 🎉 Готово!

После выполнения всех шагов у вас должен быть:

- ✅ Работающий API на `https://your-domain.com`
- ✅ Автоматический SSL сертификат
- ✅ Автозапуск при перезагрузке
- ✅ Настроенный firewall
- ✅ Мониторинг и бэкапы

**Общее время развертывания:** ~40 минут

---

## 📚 Дополнительная документация

- [Полная инструкция](DEPLOYMENT_UBUNTU_24.04.md)
- [Быстрый старт](QUICKSTART.md)
- [Архитектура](ARCHITECTURE.md)
- [README](README.md)
