#!/bin/bash

# Скрипт мониторинга BlackJackCamera API
# Использование: ./monitor.sh

echo "📊 BlackJackCamera API Monitoring Dashboard"
echo "============================================"
echo ""

# Проверка статуса контейнера
echo "🐳 Docker Container Status:"
docker ps --filter "name=blackjackcamera-api" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""

# Использование ресурсов
echo "💻 Resource Usage:"
docker stats --no-stream blackjackcamera-api --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}"
echo ""

# Health check
echo "🏥 Health Check:"
HEALTH_RESPONSE=$(curl -s http://localhost:8080/api/detection/health)
if [ $? -eq 0 ]; then
    echo "✅ API is healthy"
    echo "$HEALTH_RESPONSE" | jq '.'
else
    echo "❌ API is not responding"
fi
echo ""

# Последние логи
echo "📝 Last 20 log entries:"
docker logs --tail 20 blackjackcamera-api
echo ""

# Статистика запросов (если есть логи)
echo "📈 Request Statistics (last 100 lines):"
docker logs --tail 100 blackjackcamera-api 2>&1 | grep -i "detection" | wc -l | xargs echo "Total detection requests:"
docker logs --tail 100 blackjackcamera-api 2>&1 | grep -i "error" | wc -l | xargs echo "Errors:"
echo ""

# Размер логов
echo "📦 Log Size:"
docker inspect blackjackcamera-api --format='{{.LogPath}}' | xargs du -h
echo ""

# Uptime
echo "⏱️  Container Uptime:"
docker inspect blackjackcamera-api --format='{{.State.StartedAt}}' | xargs -I {} date -d {} +'Started: %Y-%m-%d %H:%M:%S'
echo "Current time: $(date +'%Y-%m-%d %H:%M:%S')"
echo ""

# Network connections
echo "🌐 Active Connections:"
docker exec blackjackcamera-api ss -tn state established 2>/dev/null | grep :8080 | wc -l | xargs echo "Established connections:"
echo ""

echo "============================================"
echo "💡 Tips:"
echo "  - View live logs: docker logs -f blackjackcamera-api"
echo "  - Restart container: docker-compose restart"
echo "  - View full stats: docker stats blackjackcamera-api"
