# Загрузка YOLOv8 модели

Модель **yolov8x-oiv7.onnx** (275MB) не включена в Git репозиторий из-за её размера. Вам нужно загрузить её одним из способов ниже.

## 🚀 Быстрая загрузка (рекомендуется)

### Способ 1: Автоматический скрипт

```bash
cd ~/BlackJackCamera
chmod +x scripts/download-model.sh
./scripts/download-model.sh
```

Скрипт автоматически:
- Создаст нужную директорию
- Загрузит модель с Ultralytics GitHub
- Проверит размер файла
- Подготовит к использованию

---

## 📥 Альтернативные способы

### Способ 2: Прямая загрузка через wget

```bash
cd ~/BlackJackCamera
mkdir -p BlackJackCamera/BlackJackCamera/Resources/Raw

# Загрузка с Ultralytics
wget -O BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx \
  https://github.com/ultralytics/assets/releases/download/v0.0.0/yolov8x-oiv7.onnx

# Проверка размера
ls -lh BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx
```

**Ожидаемый размер:** ~275MB (288,000,000 bytes)

---

### Способ 3: Загрузка через Python (если установлен)

```bash
pip install ultralytics

python3 << EOF
from ultralytics import YOLO
import shutil
import os

# Загружаем модель
model = YOLO('yolov8x')

# Экспортируем в ONNX формат
model.export(format='onnx')

# Перемещаем в нужную директорию
target_dir = 'BlackJackCamera/BlackJackCamera/Resources/Raw'
os.makedirs(target_dir, exist_ok=True)
shutil.move('yolov8x.onnx', f'{target_dir}/yolov8x-oiv7.onnx')
print("✅ Модель готова!")
EOF
```

---

### Способ 4: Загрузка на локальную машину → загрузка на сервер

Если прямая загрузка на сервере не работает:

#### 4.1 На вашей локальной машине:

```bash
# Загрузите модель
wget https://github.com/ultralytics/assets/releases/download/v0.0.0/yolov8x-oiv7.onnx
```

#### 4.2 Загрузите на сервер через SCP:

```bash
# С локальной машины
scp yolov8x-oiv7.onnx root@your-server-ip:~/BlackJackCamera/BlackJackCamera/BlackJackCamera/Resources/Raw/
```

#### 4.3 На сервере проверьте:

```bash
ls -lh ~/BlackJackCamera/BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx
```

---

### Способ 5: Загрузка с Google Drive / Dropbox

Если вы загрузили модель на облачное хранилище:

#### Google Drive:

```bash
# Установите gdown
pip install gdown

# Загрузите (замените FILE_ID на ID вашего файла)
gdown https://drive.google.com/uc?id=FILE_ID -O BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx
```

#### Dropbox:

```bash
# Получите прямую ссылку (замените ?dl=0 на ?dl=1)
wget -O BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx \
  "https://www.dropbox.com/s/YOUR_FILE_LINK/yolov8x-oiv7.onnx?dl=1"
```

---

## ✅ Проверка загрузки

После загрузки любым способом проверьте:

```bash
cd ~/BlackJackCamera

# Проверка наличия файла
ls -lh BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx

# Должен вывести что-то вроде:
# -rw-r--r-- 1 root root 275M Oct 8 12:00 yolov8x-oiv7.onnx

# Проверка размера
FILE_SIZE=$(stat -c%s BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx)
echo "Размер файла: $((FILE_SIZE / 1024 / 1024)) MB"

# Должно быть около 275MB
```

---

## 🔍 Troubleshooting

### Проблема: wget не установлен

```bash
# Ubuntu/Debian
sudo apt install wget

# CentOS/RHEL
sudo yum install wget
```

### Проблема: Не хватает места

```bash
# Проверьте свободное место
df -h

# Освободите место
docker system prune -a  # Удалить неиспользуемые Docker образы
```

### Проблема: Медленная загрузка

```bash
# Используйте curl вместо wget
curl -L -o BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx \
  https://github.com/ultralytics/assets/releases/download/v0.0.0/yolov8x-oiv7.onnx

# Или загрузите с помощью aria2 (многопоточная загрузка)
sudo apt install aria2
aria2c -x 16 -s 16 \
  -d BlackJackCamera/BlackJackCamera/Resources/Raw \
  -o yolov8x-oiv7.onnx \
  https://github.com/ultralytics/assets/releases/download/v0.0.0/yolov8x-oiv7.onnx
```

### Проблема: Загрузка прерывается

```bash
# Используйте wget с возобновлением
wget -c -O BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx \
  https://github.com/ultralytics/assets/releases/download/v0.0.0/yolov8x-oiv7.onnx
```

---

## 📋 Альтернативные модели (если нужно меньше размер)

Если 275MB слишком много, можете использовать меньшие модели:

| Модель | Размер | Скорость | Точность |
|--------|--------|----------|----------|
| yolov8n-oiv7.onnx | ~6MB | Очень быстро | Средняя |
| yolov8s-oiv7.onnx | ~22MB | Быстро | Хорошая |
| yolov8m-oiv7.onnx | ~52MB | Средне | Хорошая |
| yolov8l-oiv7.onnx | ~88MB | Медленно | Отличная |
| yolov8x-oiv7.onnx | ~275MB | Очень медленно | Максимальная |

**Для вашего сервера (8 ядер, 12GB RAM) рекомендуется yolov8x для максимальной точности.**

---

## 🚀 После загрузки модели

```bash
# 1. Проверьте наличие
ls -lh BlackJackCamera/BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx

# 2. Соберите Docker образ
docker compose build

# 3. Запустите контейнер
docker compose up -d

# 4. Проверьте логи (модель загружается ~45 секунд)
docker compose logs -f

# 5. Проверьте health endpoint
curl http://localhost:8080/api/detection/health
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

---

## 📞 Нужна помощь?

Если ни один способ не работает:

1. Проверьте подключение к интернету: `ping github.com`
2. Проверьте DNS: `nslookup github.com`
3. Проверьте firewall: `sudo ufw status`
4. Попробуйте загрузить через VPN/прокси

Или свяжитесь с администратором репозитория для получения альтернативной ссылки на загрузку.
