#!/bin/bash

# Скрипт резервного копирования BlackJackCamera
# Использование: ./backup.sh

BACKUP_DIR="/var/backups/blackjackcamera"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_NAME="blackjackcamera_${DATE}.tar.gz"

echo "💾 BlackJackCamera Backup Script"
echo "=================================="
echo ""

# Создаём директорию для бэкапов если её нет
mkdir -p "$BACKUP_DIR"

echo "📦 Creating backup: $BACKUP_NAME"
echo ""

# Остановка контейнера для консистентности (опционально)
read -p "Stop container during backup? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "🛑 Stopping container..."
    docker-compose stop blackjackcamera-api
    RESTART_NEEDED=true
fi

# Создание архива
echo "📁 Archiving..."
tar -czf "${BACKUP_DIR}/${BACKUP_NAME}" \
    --exclude='BlackJackCamera/*/bin' \
    --exclude='BlackJackCamera/*/obj' \
    --exclude='*.log' \
    -C "$(dirname $(pwd))" \
    "$(basename $(pwd))"

if [ $? -eq 0 ]; then
    echo "✅ Backup created successfully!"
    echo "   Location: ${BACKUP_DIR}/${BACKUP_NAME}"
    echo "   Size: $(du -h "${BACKUP_DIR}/${BACKUP_NAME}" | cut -f1)"
else
    echo "❌ Backup failed!"
fi

# Перезапуск контейнера если он был остановлен
if [ "$RESTART_NEEDED" = true ]; then
    echo "▶️  Starting container..."
    docker-compose start blackjackcamera-api
fi

echo ""
echo "📊 Backup Statistics:"
ls -lh "$BACKUP_DIR" | tail -n +2

# Удаление старых бэкапов (старше 30 дней)
echo ""
echo "🗑️  Cleaning old backups (>30 days)..."
find "$BACKUP_DIR" -name "blackjackcamera_*.tar.gz" -type f -mtime +30 -delete
echo "✅ Cleanup complete"

echo ""
echo "💡 Restore command:"
echo "   tar -xzf ${BACKUP_DIR}/${BACKUP_NAME} -C /path/to/restore"
