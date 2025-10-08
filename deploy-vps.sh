#!/bin/bash

# Скрипт развертывания BlackJackCamera API на VPS
# Использование: ./deploy-vps.sh

set -e

echo "🚀 BlackJackCamera API Deployment Script"
echo "========================================"

# Проверка Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker не установлен. Устанавливаем..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    echo "✅ Docker установлен"
else
    echo "✅ Docker уже установлен"
fi

# Проверка Docker Compose
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose не установлен. Устанавливаем..."
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
    echo "✅ Docker Compose установлен"
else
    echo "✅ Docker Compose уже установлен"
fi

# Проверка наличия модели
if [ ! -f "BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx" ]; then
    echo "❌ ОШИБКА: Модель yolov8x-oiv7.onnx не найдена!"
    echo "Пожалуйста, поместите файл модели в BlackJackCamera/BlackJackCamera/Resources/Raw/"
    exit 1
else
    echo "✅ Модель YOLOv8 найдена"
fi

# Остановка существующих контейнеров
echo ""
echo "🛑 Остановка существующих контейнеров..."
docker-compose down 2>/dev/null || true

# Сборка и запуск
echo ""
echo "🔨 Сборка и запуск контейнеров..."
docker-compose up -d --build

# Ожидание запуска
echo ""
echo "⏳ Ожидание запуска сервиса (60 секунд)..."
sleep 60

# Проверка health
echo ""
echo "🏥 Проверка состояния сервиса..."
if curl -f http://localhost:8080/api/detection/health &> /dev/null; then
    echo "✅ API успешно запущен!"
    echo ""
    echo "📊 Информация о развертывании:"
    echo "   - API URL: http://$(hostname -I | awk '{print $1}'):8080"
    echo "   - Swagger UI: http://$(hostname -I | awk '{print $1}'):8080/swagger"
    echo "   - Health Check: http://$(hostname -I | awk '{print $1}'):8080/api/detection/health"
    echo ""
    echo "📝 Логи:"
    echo "   docker-compose logs -f"
    echo ""
    echo "⚠️  Не забудьте:"
    echo "   1. Настроить firewall (открыть порт 8080)"
    echo "   2. Настроить Nginx для HTTPS (опционально)"
    echo "   3. Обновить appsettings.json в мобильном приложении"
else
    echo "❌ Сервис не отвечает. Проверьте логи:"
    echo "   docker-compose logs"
    exit 1
fi

echo ""
echo "🎉 Развертывание завершено!"
