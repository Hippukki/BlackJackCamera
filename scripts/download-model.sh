#!/bin/bash

# Скрипт загрузки YOLOv8 модели для BlackJackCamera
# Использование: ./download-model.sh

set -e

echo "📦 YOLOv8 Model Download Script"
echo "================================"
echo ""

# Определяем путь к модели
MODEL_DIR="BlackJackCamera/BlackJackCamera/Resources/Raw"
MODEL_FILE="yolov8x-oiv7.onnx"
MODEL_PATH="${MODEL_DIR}/${MODEL_FILE}"

# Создаём директорию если не существует
mkdir -p "$MODEL_DIR"

# Проверяем наличие модели
if [ -f "$MODEL_PATH" ]; then
    echo "✅ Модель уже существует: $MODEL_PATH"
    echo "   Размер: $(du -h "$MODEL_PATH" | cut -f1)"
    echo ""
    read -p "Перезагрузить модель? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "❌ Загрузка отменена"
        exit 0
    fi
    rm "$MODEL_PATH"
fi

echo "📥 Загрузка YOLOv8x OIv7 модели..."
echo "   Размер: ~275MB"
echo "   Это может занять несколько минут..."
echo ""

# Вариант 1: Загрузка с GitHub Releases (если у вас есть)
# Раскомментируйте и укажите правильный URL
# GITHUB_URL="https://github.com/your-username/BlackJackCamera/releases/download/v1.0/yolov8x-oiv7.onnx"
# wget -O "$MODEL_PATH" "$GITHUB_URL" --progress=bar:force 2>&1

# Вариант 2: Загрузка с официального источника Ultralytics
# YOLOv8x с Open Images V7 датасетом
ULTRALYTICS_URL="https://github.com/ultralytics/assets/releases/download/v0.0.0/yolov8x-oiv7.onnx"

echo "🌐 Загрузка с Ultralytics GitHub..."
if wget -O "$MODEL_PATH" "$ULTRALYTICS_URL" --progress=bar:force 2>&1; then
    echo ""
    echo "✅ Модель успешно загружена!"
else
    echo ""
    echo "❌ Ошибка загрузки с Ultralytics"
    echo ""
    echo "📝 Альтернативные способы загрузки:"
    echo ""
    echo "1. Загрузите вручную с Ultralytics:"
    echo "   https://github.com/ultralytics/assets/releases"
    echo ""
    echo "2. Или используйте Python:"
    echo "   pip install ultralytics"
    echo "   python -c 'from ultralytics import YOLO; YOLO(\"yolov8x-oiv7.onnx\")'"
    echo ""
    echo "3. Или загрузите с Google Drive/Dropbox и скопируйте на сервер:"
    echo "   scp yolov8x-oiv7.onnx root@your-server:~/BlackJackCamera/$MODEL_DIR/"
    echo ""
    exit 1
fi

# Проверяем загруженный файл
if [ -f "$MODEL_PATH" ]; then
    FILE_SIZE=$(stat -f%z "$MODEL_PATH" 2>/dev/null || stat -c%s "$MODEL_PATH" 2>/dev/null)
    FILE_SIZE_MB=$((FILE_SIZE / 1024 / 1024))

    echo ""
    echo "📊 Информация о файле:"
    echo "   Путь: $MODEL_PATH"
    echo "   Размер: ${FILE_SIZE_MB}MB"
    echo ""

    # Проверяем размер (должен быть около 275MB)
    if [ $FILE_SIZE_MB -lt 200 ]; then
        echo "⚠️  ВНИМАНИЕ: Размер файла слишком мал ($FILE_SIZE_MB MB)"
        echo "   Ожидается: ~275MB"
        echo "   Возможно, файл загружен не полностью"
        echo ""
        read -p "Продолжить? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            rm "$MODEL_PATH"
            echo "❌ Файл удалён"
            exit 1
        fi
    fi

    echo "✅ Модель готова к использованию!"
    echo ""
    echo "📝 Следующие шаги:"
    echo "   1. Запустите: docker compose build"
    echo "   2. Запустите: docker compose up -d"
    echo "   3. Проверьте: curl http://localhost:8080/api/detection/health"
else
    echo "❌ Файл не найден после загрузки"
    exit 1
fi
