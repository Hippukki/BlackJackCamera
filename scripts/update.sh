#!/bin/bash

# Скрипт обновления BlackJackCamera API
# Использование: ./update.sh

set -e

echo "🔄 BlackJackCamera API Update Script"
echo "====================================="
echo ""

# Проверка Git
if [ ! -d ".git" ]; then
    echo "❌ Not a git repository!"
    exit 1
fi

echo "📥 Fetching latest changes..."
git fetch origin

# Показываем изменения
echo ""
echo "📋 Changes to be applied:"
git log HEAD..origin/main --oneline --graph --decorate
echo ""

read -p "Continue with update? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ Update cancelled"
    exit 0
fi

# Создаём бэкап перед обновлением
echo ""
echo "💾 Creating backup before update..."
./scripts/backup.sh

# Получаем изменения
echo ""
echo "📥 Pulling latest code..."
git pull origin main

# Пересобираем Docker образ
echo ""
echo "🔨 Rebuilding Docker image..."
docker-compose build --no-cache

# Перезапускаем контейнер
echo ""
echo "🔄 Restarting container..."
docker-compose down
docker-compose up -d

# Ждём запуска
echo ""
echo "⏳ Waiting for service to start (30 seconds)..."
sleep 30

# Проверяем health
echo ""
echo "🏥 Checking health..."
if curl -f http://localhost:8080/api/detection/health &> /dev/null; then
    echo "✅ Update successful! Service is healthy."
else
    echo "⚠️  Service is not responding. Check logs:"
    echo "   docker-compose logs -f"
fi

echo ""
echo "📊 Current status:"
docker-compose ps

echo ""
echo "✅ Update complete!"
