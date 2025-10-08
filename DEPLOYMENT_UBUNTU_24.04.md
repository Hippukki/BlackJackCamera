# Развертывание BlackJackCamera API на Ubuntu 24.04 LTS

Полное руководство по установке и настройке для сервера с характеристиками:
- **CPU:** 8 ядер, 5.1 ГГц
- **RAM:** 12 GB DDR4
- **Storage:** 20 GB SSD NVMe
- **OS:** Ubuntu 24.04 LTS

## ⚠️ Важно: Проблема с NuGet.org

Если на сервере нет доступа к NuGet.org (таймауты при `dotnet restore`), используйте **Docker Hub** метод развертывания: [DEPLOYMENT_DOCKERHUB.md](DEPLOYMENT_DOCKERHUB.md)

Этот документ описывает полное развертывание с локальной сборкой Docker образа на сервере.

## Содержание

1. [Подготовка сервера](#1-подготовка-сервера)
2. [Установка зависимостей](#2-установка-зависимостей)
3. [Клонирование проекта](#3-клонирование-проекта)
4. [Настройка Docker](#4-настройка-docker)
5. [Настройка Nginx и SSL](#5-настройка-nginx-и-ssl)
6. [Настройка Firewall](#6-настройка-firewall)
7. [Запуск приложения](#7-запуск-приложения)
8. [Мониторинг и логи](#8-мониторинг-и-логи)
9. [Автозапуск при перезагрузке](#9-автозапуск-при-перезагрузке)
10. [Оптимизация производительности](#10-оптимизация-производительности)

---

## 1. Подготовка сервера

### 1.1 Обновление системы

```bash
# Обновляем список пакетов
sudo apt update

# Обновляем установленные пакеты
sudo apt upgrade -y

# Устанавливаем базовые утилиты
sudo apt install -y \
    curl \
    wget \
    git \
    htop \
    vim \
    build-essential \
    software-properties-common \
    apt-transport-https \
    ca-certificates \
    gnupg \
    lsb-release \
    jq
```

### 1.2 Настройка часового пояса

```bash
# Настраиваем timezone
sudo timedatectl set-timezone Europe/Moscow  # Замените на ваш timezone

# Проверяем
timedatectl
```

### 1.3 Создание пользователя для приложения (опционально)

```bash
# Создаём пользователя
sudo useradd -m -s /bin/bash blackjack

# Добавляем в группы
sudo usermod -aG sudo blackjack
sudo usermod -aG docker blackjack  # Будет добавлено после установки Docker

# Переключаемся на пользователя
sudo su - blackjack
```

---

## 2. Установка зависимостей

### 2.1 Установка Docker

```bash
# Удаляем старые версии Docker (если есть)
sudo apt remove docker docker-engine docker.io containerd runc 2>/dev/null || true

# Добавляем официальный GPG ключ Docker
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

# Добавляем репозиторий Docker
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Устанавливаем Docker
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Проверяем установку
docker --version
docker compose version
```

### 2.2 Настройка Docker для текущего пользователя

```bash
# Добавляем пользователя в группу docker
sudo usermod -aG docker $USER

# Применяем изменения (или перелогиньтесь)
newgrp docker

# Проверяем (должно работать без sudo)
docker ps
```

### 2.3 Оптимизация Docker для производительности

```bash
# Создаём конфигурацию Docker daemon
sudo mkdir -p /etc/docker

sudo tee /etc/docker/daemon.json > /dev/null <<EOF
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "100m",
    "max-file": "5"
  },
  "storage-driver": "overlay2",
  "default-ulimits": {
    "nofile": {
      "Name": "nofile",
      "Hard": 65536,
      "Soft": 65536
    }
  }
}
EOF

# Перезапускаем Docker
sudo systemctl restart docker
sudo systemctl enable docker
```

### 2.4 Установка Nginx

```bash
# Устанавливаем Nginx
sudo apt install -y nginx

# Проверяем версию
nginx -v

# Запускаем и включаем автозапуск
sudo systemctl start nginx
sudo systemctl enable nginx
```

### 2.5 Установка Certbot (для SSL сертификатов)

```bash
# Устанавливаем Certbot для Nginx
sudo apt install -y certbot python3-certbot-nginx

# Проверяем версию
certbot --version
```

---

## 3. Клонирование проекта

### 3.1 Настройка SSH ключа (рекомендуется)

```bash
# Генерируем SSH ключ
ssh-keygen -t ed25519 -C "your-email@example.com"

# Выводим публичный ключ (добавьте его в GitHub)
cat ~/.ssh/id_ed25519.pub
```

### 3.2 Клонирование репозитория

```bash
# Переходим в домашнюю директорию
cd ~

# Клонируем проект (замените на ваш URL)
git clone git@github.com:your-username/BlackJackCamera.git

# Переходим в директорию проекта
cd BlackJackCamera
```

### 3.3 Проверка наличия ML модели

```bash
# Проверяем наличие модели (должна быть 275MB)
ls -lh BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx

# Если модели нет, загрузите её отдельно
# wget https://your-storage-url/yolov8x-oiv7.onnx -O BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx
```

---

## 4. Настройка Docker

### 4.1 Проверка docker-compose.yml

Убедитесь, что `docker-compose.yml` оптимизирован для вашего сервера:

```yaml
deploy:
  resources:
    limits:
      cpus: '6'        # 6 из 8 ядер
      memory: 8G       # 8GB из 12GB
    reservations:
      cpus: '4'
      memory: 4G
```

### 4.2 Сборка образа

```bash
# Собираем Docker образ
docker compose build

# Проверяем созданный образ
docker images | grep blackjackcamera
```

---

## 5. Настройка Nginx и SSL

### 5.1 Создание конфигурации Nginx

```bash
# Копируем конфигурацию
sudo cp nginx/blackjackcamera.conf /etc/nginx/sites-available/blackjackcamera

# Редактируем конфигурацию (замените your-domain.com на ваш домен)
sudo vim /etc/nginx/sites-available/blackjackcamera
```

**Важные изменения:**
- Замените `your-domain.com` на ваш домен
- Закомментируйте SSL секцию (настроим позже через Certbot)

### 5.2 Включение сайта

```bash
# Создаём symlink
sudo ln -s /etc/nginx/sites-available/blackjackcamera /etc/nginx/sites-enabled/

# Удаляем дефолтный сайт (опционально)
sudo rm /etc/nginx/sites-enabled/default

# Проверяем конфигурацию
sudo nginx -t

# Перезагружаем Nginx
sudo systemctl reload nginx
```

### 5.3 Получение SSL сертификата

```bash
# Останавливаем Nginx временно
sudo systemctl stop nginx

# Получаем сертификат
sudo certbot certonly --standalone -d your-domain.com -d www.your-domain.com

# Запускаем Nginx
sudo systemctl start nginx

# Раскомментируем SSL секцию в конфигурации
sudo vim /etc/nginx/sites-available/blackjackcamera

# Проверяем и перезагружаем
sudo nginx -t
sudo systemctl reload nginx
```

### 5.4 Автообновление SSL сертификата

```bash
# Проверяем автообновление
sudo certbot renew --dry-run

# Certbot автоматически настроит cron для обновления
```

---

## 6. Настройка Firewall

### 6.1 Установка и настройка UFW

```bash
# Устанавливаем UFW (если не установлен)
sudo apt install -y ufw

# Разрешаем SSH (ВАЖНО! Сделайте это первым)
sudo ufw allow 22/tcp

# Разрешаем HTTP и HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Включаем firewall
sudo ufw enable

# Проверяем статус
sudo ufw status verbose
```

### 6.2 Опционально: Rate limiting для SSH

```bash
# Ограничиваем количество попыток подключения SSH
sudo ufw limit 22/tcp
```

---

## 7. Запуск приложения

### 7.1 Первый запуск

```bash
# Переходим в директорию проекта
cd ~/BlackJackCamera

# Запускаем в фоновом режиме
docker compose up -d

# Смотрим логи
docker compose logs -f
```

Нажмите `Ctrl+C` для выхода из логов.

### 7.2 Проверка работоспособности

```bash
# Ждём 45 секунд для инициализации модели
sleep 45

# Проверяем health endpoint
curl http://localhost:8080/api/detection/health

# Проверяем через внешний адрес (замените на ваш домен)
curl https://your-domain.com/api/detection/health
```

**Ожидаемый ответ:**
```json
{
  "status": "healthy",
  "message": "Object detection service is ready",
  "classesCount": 601,
  "timestamp": "2025-10-08T12:00:00Z"
}
```

### 7.3 Тестовый запрос с изображением

```bash
# Загрузите тестовое изображение
wget https://via.placeholder.com/640 -O test.jpg

# Отправьте запрос
curl -X POST https://your-domain.com/api/detection/detect \
  -F "file=@test.jpg" \
  -H "Content-Type: multipart/form-data"
```

---

## 8. Мониторинг и логи

### 8.1 Использование скриптов мониторинга

```bash
# Делаем скрипты исполняемыми
chmod +x scripts/*.sh

# Запускаем мониторинг
./scripts/monitor.sh
```

### 8.2 Просмотр логов

```bash
# В реальном времени
docker compose logs -f

# Последние 100 строк
docker compose logs --tail 100

# Только ошибки
docker compose logs | grep -i error

# Логи за последний час
docker compose logs --since 1h
```

### 8.3 Статистика ресурсов

```bash
# Использование CPU/RAM в реальном времени
docker stats blackjackcamera-api

# Однократный вывод
docker stats --no-stream blackjackcamera-api
```

### 8.4 Системные ресурсы

```bash
# Общая загрузка системы
htop

# Использование диска
df -h

# Использование памяти
free -h

# Нагрузка CPU
uptime
```

---

## 9. Автозапуск при перезагрузке

### 9.1 Настройка Docker Compose для автозапуска

Docker Compose уже настроен с `restart: unless-stopped`, но убедимся:

```bash
# Проверяем политику перезапуска
docker inspect blackjackcamera-api | grep RestartPolicy -A 3
```

### 9.2 Создание systemd сервиса (альтернатива)

```bash
# Создаём systemd unit file
sudo tee /etc/systemd/system/blackjackcamera.service > /dev/null <<EOF
[Unit]
Description=BlackJackCamera API
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/home/$(whoami)/BlackJackCamera
ExecStart=/usr/bin/docker compose up -d
ExecStop=/usr/bin/docker compose down
User=$(whoami)
Group=$(whoami)

[Install]
WantedBy=multi-user.target
EOF

# Перезагружаем systemd
sudo systemctl daemon-reload

# Включаем автозапуск
sudo systemctl enable blackjackcamera.service

# Проверяем статус
sudo systemctl status blackjackcamera.service
```

### 9.3 Тест перезагрузки

```bash
# Перезагружаем сервер
sudo reboot

# После перезагрузки проверяем
docker ps
curl http://localhost:8080/api/detection/health
```

---

## 10. Оптимизация производительности

### 10.1 Настройка системных лимитов

```bash
# Увеличиваем лимиты для файловых дескрипторов
sudo tee -a /etc/security/limits.conf > /dev/null <<EOF
* soft nofile 65536
* hard nofile 65536
* soft nproc 65536
* hard nproc 65536
EOF

# Применяем изменения (или перелогиньтесь)
ulimit -n 65536
```

### 10.2 Оптимизация сетевых параметров

```bash
# Оптимизируем TCP параметры
sudo tee -a /etc/sysctl.conf > /dev/null <<EOF
# Network optimization for BlackJackCamera API
net.core.somaxconn = 4096
net.ipv4.tcp_max_syn_backlog = 4096
net.ipv4.tcp_fin_timeout = 30
net.ipv4.tcp_keepalive_time = 600
net.ipv4.tcp_keepalive_probes = 5
net.ipv4.tcp_keepalive_intvl = 15
EOF

# Применяем изменения
sudo sysctl -p
```

### 10.3 Настройка swap (для безопасности)

```bash
# Проверяем наличие swap
free -h

# Если swap нет, создаём (4GB)
sudo fallocate -l 4G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile

# Добавляем в fstab для автомонтирования
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab

# Настраиваем swappiness (минимальное использование swap)
echo 'vm.swappiness=10' | sudo tee -a /etc/sysctl.conf
sudo sysctl -p
```

### 10.4 Мониторинг производительности

```bash
# Установка дополнительных инструментов
sudo apt install -y sysstat iotop nethogs

# Включаем сбор статистики
sudo systemctl enable sysstat
sudo systemctl start sysstat

# Смотрим статистику CPU
sar -u 1 10

# Смотрим статистику диска
iostat -x 1 10

# Мониторинг сети по процессам
sudo nethogs
```

---

## Обслуживание и управление

### Резервное копирование

```bash
# Ручное создание бэкапа
./scripts/backup.sh

# Настройка автоматического бэкапа (cron)
crontab -e

# Добавьте строку для ежедневного бэкапа в 3 утра:
0 3 * * * /home/$(whoami)/BlackJackCamera/scripts/backup.sh >> /var/log/blackjackcamera-backup.log 2>&1
```

### Обновление приложения

```bash
# Используйте скрипт обновления
./scripts/update.sh
```

### Перезапуск сервиса

```bash
# Перезапуск контейнера
docker compose restart

# Полная пересборка и перезапуск
docker compose down
docker compose build --no-cache
docker compose up -d
```

### Просмотр активных подключений

```bash
# Активные подключения к API
ss -tn state established '( dport = :8080 or sport = :8080 )'

# Количество подключений
ss -tn state established '( dport = :8080 or sport = :8080 )' | wc -l
```

---

## Troubleshooting

### Проблема: Контейнер не запускается

```bash
# Проверяем логи
docker compose logs

# Проверяем наличие модели
ls -lh BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx

# Проверяем ресурсы
free -h
df -h
```

### Проблема: Высокая нагрузка на CPU

```bash
# Смотрим статистику
docker stats blackjackcamera-api

# Уменьшаем лимиты в docker-compose.yml
# cpus: '4' вместо '6'

# Перезапускаем
docker compose down
docker compose up -d
```

### Проблема: Медленные ответы API

```bash
# Проверяем processingTimeMs в ответах
curl https://your-domain.com/api/detection/health

# Проверяем нагрузку на диск
iostat -x 1 5

# Проверяем сеть
nethogs

# Увеличиваем ресурсы в docker-compose.yml
```

---

## Безопасность

### Рекомендации

1. **Регулярные обновления**
```bash
sudo apt update && sudo apt upgrade -y
```

2. **Fail2Ban для защиты SSH**
```bash
sudo apt install -y fail2ban
sudo systemctl enable fail2ban
sudo systemctl start fail2ban
```

3. **Отключение root login через SSH**
```bash
sudo vim /etc/ssh/sshd_config
# Установите: PermitRootLogin no
sudo systemctl restart sshd
```

4. **Мониторинг логов**
```bash
# Установка Logwatch
sudo apt install -y logwatch
sudo logwatch --detail High --mailto your-email@example.com
```

---

## Ожидаемая производительность

С вашими характеристиками сервера:

| Метрика | Значение |
|---------|----------|
| **Время обработки (1 изображение)** | 200-400ms |
| **Concurrent requests** | До 50 одновременных запросов |
| **Throughput** | ~150-250 изображений/минуту |
| **CPU Usage** | 40-60% при средней нагрузке |
| **RAM Usage** | 4-6GB |

---

## Контакты и поддержка

Если возникли проблемы:
1. Проверьте логи: `docker compose logs -f`
2. Запустите мониторинг: `./scripts/monitor.sh`
3. Проверьте документацию: [README.md](README.md)

**Готово!** Ваш BlackJackCamera API развернут и готов к работе! 🚀
