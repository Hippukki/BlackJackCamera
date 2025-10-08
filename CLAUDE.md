# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Project Overview

BlackJackCamera is a **separated Backend/Frontend architecture**:
- **Backend:** ASP.NET Core Web API with YOLOv8 ML model (deployed on VPS)
- **Frontend:** .NET MAUI 9.0 cross-platform mobile application

## Architecture

### Backend (BlackJackCamera.Api)
- **ASP.NET Core Web API** - REST API for object detection
- **ONNX Runtime** - YOLOv8 model inference
- **Docker** - Containerized deployment
- **Location:** Deployed on Ubuntu 24.04 VPS (8 cores, 12GB RAM)

### Frontend (BlackJackCamera)
- **.NET MAUI 9.0** - Cross-platform mobile app (Android, iOS, Windows, macOS)
- **CommunityToolkit.Maui.Camera** - Camera functionality
- **HttpClient** - Communication with Backend API
- **SkiaSharp** - Image compression before upload

## Project Structure

```
/
├── docker-compose.yml              # Docker deployment configuration
├── deploy-vps.sh                   # Automated deployment script
├── README.md                       # Main documentation
├── QUICKSTART.md                   # Quick start guide
├── DEPLOYMENT_UBUNTU_24.04.md      # Full deployment guide
├── DEPLOYMENT_CHECKLIST.md         # Deployment checklist
├── ARCHITECTURE.md                 # Architecture documentation
├── MIGRATION_GUIDE.md              # Migration from monolith to separated
├── DOWNLOAD_MODEL.md               # Guide for downloading YOLOv8 model
│
├── scripts/
│   ├── monitor.sh                  # Monitoring dashboard
│   ├── backup.sh                   # Backup script
│   ├── update.sh                   # Update script
│   └── download-model.sh           # Model download script
│
├── nginx/
│   └── blackjackcamera.conf        # Nginx configuration with SSL
│
├── BlackJackCamera.Api/            # Backend Web API
│   ├── Controllers/
│   │   └── DetectionController.cs  # API endpoints
│   ├── Services/
│   │   ├── ModelLoaderService.cs
│   │   ├── ObjectDetectionService.cs
│   │   └── ImageProcessor.cs
│   ├── Models/
│   │   ├── Detection.cs
│   │   └── DetectionResponse.cs
│   ├── Resources/Models/
│   │   ├── yolov8x-oiv7.onnx      # YOLOv8 model (275MB, Git LFS)
│   │   └── labels.txt              # 601 class labels
│   ├── Dockerfile
│   └── Program.cs
│
└── BlackJackCamera/                # Frontend MAUI App
    ├── Services/
    │   ├── DetectionApiService.cs      # HTTP client for API
    │   ├── ImageCompressionService.cs  # Image optimization
    │   └── CategoryBadgeMapper.cs      # Business logic
    ├── Models/
    │   ├── DetectionDto.cs
    │   └── DetectionResponseDto.cs
    ├── appsettings.json                # API URL configuration
    ├── MainPage.xaml                   # Home screen
    ├── CameraPage.xaml                 # Camera screen
    └── MauiProgram.cs
```

## Development Commands

### Backend (API)

```bash
# Local development
cd BlackJackCamera.Api
dotnet run

# Docker deployment
docker compose build
docker compose up -d

# View logs
docker compose logs -f

# Health check
curl http://localhost:8080/api/detection/health
```

### Frontend (MAUI)

```bash
# Build for Android
dotnet build BlackJackCamera/BlackJackCamera.csproj -f net9.0-android

# Run on Android
dotnet build BlackJackCamera/BlackJackCamera.csproj -t:Run -f net9.0-android

# Build for iOS (macOS only)
dotnet build BlackJackCamera/BlackJackCamera.csproj -f net9.0-ios -t:Run

# Build for Windows
dotnet build BlackJackCamera/BlackJackCamera.csproj -f net9.0-windows10.0.19041.0 -t:Run
```

## API Endpoints

### POST /api/detection/detect
Detects objects in uploaded image

**Request:** multipart/form-data with image file (max 10MB)
**Response:** JSON with detections

### GET /api/detection/health
Health check endpoint

## Configuration

### Frontend (appsettings.json)

```json
{
  "ApiSettings": {
    "BaseUrl": "https://your-domain.com",
    "Timeout": 30
  },
  "ImageCompression": {
    "MaxWidth": 1920,
    "MaxHeight": 1080,
    "Quality": 75
  }
}
```

### Backend (docker-compose.yml)

Optimized for 8 cores / 12GB RAM server:
- CPU: 6 cores limit
- RAM: 8GB limit
- Auto-restart enabled
- Health checks configured

## Deployment

### VPS Deployment (Ubuntu 24.04)

```bash
# Clone repository
git clone <repo-url>
cd BlackJackCamera

# Install Git LFS and download model
sudo apt install git-lfs
git lfs install
git lfs pull

# Run deployment
chmod +x deploy-vps.sh
./deploy-vps.sh
```

See [DEPLOYMENT_UBUNTU_24.04.md](DEPLOYMENT_UBUNTU_24.04.md) for detailed instructions.

## Important Notes

### Image Processing Pipeline

**Frontend:**
1. Capture image from camera (2-5MB raw)
2. Compress to JPEG 75% quality (~200KB)
3. Resize to max 1920x1080
4. Upload to API via HTTP

**Backend:**
1. Receive compressed image
2. Resize to 640x640
3. Convert to tensor [1,3,640,640]
4. Run YOLOv8 inference
5. Apply NMS filtering
6. Return JSON response (~5-50KB)

### Performance

On 8-core/12GB server:
- Processing time: 200-400ms per image
- Concurrent requests: up to 50
- Throughput: 150-250 images/minute

### Security

- HTTPS/SSL via Let's Encrypt
- CORS configured for mobile apps
- File size limits (10MB max)
- Docker security constraints
- UFW firewall enabled

### Git LFS

YOLOv8 model (`yolov8x-oiv7.onnx`, 275MB) is stored in Git LFS.

```bash
# Download LFS files
git lfs pull
```

## Monitoring

```bash
# Real-time monitoring
./scripts/monitor.sh

# View logs
docker compose logs -f

# Resource usage
docker stats blackjackcamera-api
```

## Troubleshooting

See documentation:
- [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md) - Common issues
- [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) - Migration details
- [README.md](README.md) - Full documentation

## Development Tips

### Adding New Endpoints
1. Add method in `DetectionController.cs`
2. Update Swagger documentation
3. Add corresponding method in Frontend `DetectionApiService.cs`

### Updating ML Model
1. Replace `yolov8x-oiv7.onnx` in `Resources/Models/`
2. Update `labels.txt` if classes changed
3. Rebuild Docker image: `docker compose build`
4. Restart: `docker compose up -d`

### Testing API Locally
Use `BlackJackCamera.Api/test-api.http` file with REST Client extension

## Links

- API Swagger: `https://your-domain.com/swagger`
- Health Check: `https://your-domain.com/api/detection/health`
