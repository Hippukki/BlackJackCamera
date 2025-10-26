# BlackJackCamera

> AI-Powered Visual Banking Assistant - Cross-platform mobile application with intelligent object detection for financial services

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MAUI](https://img.shields.io/badge/MAUI-Cross--Platform-512BD4)](https://dotnet.microsoft.com/apps/maui)
[![ML](https://img.shields.io/badge/YOLOv8-ONNX-00D4AA)](https://github.com/ultralytics/ultralytics)
[![License](https://img.shields.io/badge/License-Proprietary-red)](LICENSE)

## Overview

BlackJackCamera is an innovative mobile banking application that uses AI-powered computer vision to recognize objects through your camera and instantly suggest relevant banking products and services. Built with a modern separated architecture, it combines the power of YOLOv8 object detection with an intuitive mobile interface.

**Key Features:**
- Real-time object detection using YOLOv8 (601 object classes)
- Intelligent mapping of detected objects to banking services
- Special financial offers (credit pre-approval, installment plans)
- Cross-platform support (Android, iOS, Windows, macOS)
- Optimized client-server architecture
- Enterprise-grade deployment with Docker

## Table of Contents

- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Technology Stack](#technology-stack)
- [Performance](#performance)
- [Security](#security)
- [Development](#development)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

## Architecture

BlackJackCamera follows a **separated Frontend/Backend architecture** for optimal performance, scalability, and maintainability:

```
┌─────────────────────┐         HTTPS/HTTP          ┌─────────────────────┐
│                     │   ──────────────────────►   │                     │
│  MAUI Mobile App    │   Compressed JPEG (~200KB)  │   ASP.NET Core API  │
│  (Frontend)         │                             │   (Backend)         │
│                     │   ◄──────────────────────   │                     │
│  • Camera Capture   │      JSON Response (~5KB)   │  • YOLOv8 ML Model  │
│  • Image Compress   │                             │  • ONNX Runtime     │
│  • UI/UX            │                             │  • Object Detection │
└─────────────────────┘                             └─────────────────────┘
         │                                                    │
         │ Client Devices                                     │ VPS Server
         │ (Android/iOS/Windows)                              │ (Docker Container)
         └────────────────────────────────────────────────────┘
```

### Component Responsibilities

**Frontend (Mobile App)**
- Camera integration and image capture
- Image preprocessing and compression (JPEG 75%, max 1920x1080)
- Network communication via HTTP API
- Business logic mapping (objects → banking services)
- Rich UI with animations and interactive elements
- Special offer flows (credit calculator, installment plans)

**Backend (Web API)**
- Image validation and processing
- ML model inference (YOLOv8 on ONNX Runtime)
- Object detection with confidence filtering
- Non-Maximum Suppression (NMS) for duplicate removal
- RESTful API with Swagger documentation
- Health monitoring and logging

### Data Flow

```
1. User captures photo → 2. Image compression (2-5MB → ~200KB) →
3. HTTP POST to API → 4. ML inference on server →
5. JSON response → 6. UI display with banking services
```

**Processing Time:** ~200-400ms end-to-end on recommended hardware

## Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) (for backend deployment)
- Android SDK / Xcode (for mobile development)
- Git LFS (for ML model file)

### 1. Clone Repository

```bash
git clone https://github.com/your-org/BlackJackCamera.git
cd BlackJackCamera

# Download ML model via Git LFS
git lfs install
git lfs pull
```

### 2. Start Backend

**Option A: Docker (Recommended)**

```bash
cd BlackJackCamera
docker-compose up -d

# Verify health
curl http://localhost:8080/api/detection/health
```

**Option B: Local Development**

```bash
cd BlackJackCamera/BlackJackCamera.Api
dotnet restore
dotnet run

# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### 3. Configure Frontend

Edit `BlackJackCamera/BlackJackCamera/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8080"  // Change to your server URL
  }
}
```

**Special Cases:**
- **Android Emulator:** Use `http://10.0.2.2:8080`
- **Physical Device:** Use `http://YOUR_PC_IP:8080` (same Wi-Fi network)
- **Production:** Use `https://your-domain.com`

### 4. Run Mobile App

```bash
cd BlackJackCamera/BlackJackCamera

# Android
dotnet build -f net9.0-android
dotnet build -t:Run -f net9.0-android

# iOS (macOS only)
dotnet build -f net9.0-ios -t:Run

# Windows
dotnet build -f net9.0-windows10.0.19041.0 -t:Run
```

### 5. Test the App

1. Open the app and tap the scanner button
2. Capture a photo of an object (laptop, phone, car, etc.)
3. Wait for detection results (~2-5 seconds)
4. View suggested banking services

## System Requirements

### Backend Server

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| **CPU** | 2 cores | 8 cores |
| **RAM** | 4 GB | 12 GB |
| **Storage** | 2 GB | 10 GB SSD |
| **OS** | Ubuntu 20.04 | Ubuntu 24.04 LTS |
| **Network** | 10 Mbps | 100 Mbps |

**Recommended VPS Specs:** 8 vCPU / 12 GB RAM / 50 GB SSD

### Mobile Devices

| Platform | Minimum Version | Recommended |
|----------|----------------|-------------|
| **Android** | 7.0 (API 24) | Android 13+ |
| **iOS** | 15.0 | iOS 17+ |
| **Windows** | 10 (17763) | Windows 11 |
| **macOS** | Catalina 15.0 | Sonoma 15+ |

**Hardware:**
- Camera: Required
- RAM: 2 GB minimum, 4 GB recommended
- Storage: 100 MB free space

## Installation

### Backend Deployment

#### Docker Deployment (Production)

1. **Prepare Server**

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

2. **Clone and Deploy**

```bash
git clone https://github.com/your-org/BlackJackCamera.git
cd BlackJackCamera/BlackJackCamera

# Install Git LFS and download model
sudo apt install git-lfs
git lfs install
git lfs pull

# Verify model exists
ls -lh BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx

# Deploy with script
chmod +x deploy-vps.sh
./deploy-vps.sh
```

3. **Verify Deployment**

```bash
# Check container status
docker-compose ps

# View logs
docker-compose logs -f

# Test health endpoint
curl http://localhost:8080/api/detection/health
```

#### Optional: Nginx Reverse Proxy with SSL

```bash
# Install Nginx
sudo apt install nginx

# Copy configuration
sudo cp nginx/blackjackcamera.conf /etc/nginx/sites-available/
sudo ln -s /etc/nginx/sites-available/blackjackcamera.conf /etc/nginx/sites-enabled/

# Install SSL certificate
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d your-domain.com

# Test and reload
sudo nginx -t
sudo systemctl reload nginx
```

### Frontend Deployment

#### Building Release APK (Android)

```bash
cd BlackJackCamera/BlackJackCamera

# Build release APK
dotnet build -c Release -f net9.0-android

# Output: bin/Release/net9.0-android/BlackJackCamera.apk
```

**Note:** The project includes a keystore with credentials in `.csproj`. For production, generate your own:

```bash
keytool -genkeypair -v -keystore myapp.keystore -alias myalias -keyalg RSA -keysize 2048 -validity 10000
```

#### Building for iOS

```bash
cd BlackJackCamera/BlackJackCamera

# Build for iOS
dotnet build -c Release -f net9.0-ios

# Archive for App Store
dotnet publish -f net9.0-ios -c Release -p:RuntimeIdentifier=ios-arm64
```

## Configuration

### Backend Configuration

**Environment Variables** (docker-compose.yml)

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ASPNETCORE_URLS=http://+:8080
  - DOTNET_GCServer=1                    # Enable server GC
  - DOTNET_GCConcurrent=1                # Concurrent GC
```

**Resource Limits**

```yaml
deploy:
  resources:
    limits:
      cpus: '6'      # Use 6 of 8 cores
      memory: 8G     # 8GB of 12GB available
    reservations:
      cpus: '4'
      memory: 4G
```

### Frontend Configuration

**appsettings.json**

```json
{
  "ApiSettings": {
    "BaseUrl": "https://your-domain.com",  // API endpoint
    "Timeout": 30                           // Request timeout (seconds)
  },
  "ImageCompression": {
    "MaxWidth": 1920,      // Maximum image width
    "MaxHeight": 1080,     // Maximum image height
    "Quality": 75          // JPEG quality (1-100)
  }
}
```

**Optimization Settings:**

- **Quality 75:** Best balance between size and visual quality
- **Max 1920x1080:** Preserves Full HD detail while limiting file size
- **Average result:** 150-300 KB per image (10-25x compression)

### ML Model Configuration

**Model:** YOLOv8x trained on Open Images V7
**File:** `yolov8x-oiv7.onnx` (275 MB)
**Classes:** 601 object categories
**Input:** 640x640 RGB tensor
**Confidence Threshold:** 0.25
**NMS IoU Threshold:** 0.45

**To download/update the model:**

```bash
cd BlackJackCamera/scripts
./download-model.sh
```

See [DOWNLOAD_MODEL.md](DOWNLOAD_MODEL.md) for detailed instructions.

## Deployment

### Production Deployment Checklist

Use our comprehensive deployment checklist: [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md)

**Key Steps:**

- [ ] Verify ML model is present (275 MB)
- [ ] Configure firewall (open port 8080)
- [ ] Set up SSL certificate (Let's Encrypt)
- [ ] Update frontend `BaseUrl` to production domain
- [ ] Enable monitoring and logging
- [ ] Set up automated backups
- [ ] Configure rate limiting
- [ ] Test health endpoint
- [ ] Verify API response times

### Deployment Guides

- **[DEPLOYMENT_UBUNTU_24.04.md](DEPLOYMENT_UBUNTU_24.04.md)** - Complete Ubuntu 24.04 setup guide
- **[DEPLOYMENT_DOCKERHUB.md](DEPLOYMENT_DOCKERHUB.md)** - Using pre-built Docker images
- **[QUICKSTART.md](QUICKSTART.md)** - Rapid deployment guide

### Monitoring Tools

```bash
# Real-time monitoring dashboard
./scripts/monitor.sh

# View logs
docker-compose logs -f blackjackcamera-api

# Resource usage
docker stats blackjackcamera-api

# Health check
curl http://localhost:8080/api/detection/health
```

### Backup and Restore

```bash
# Backup
./scripts/backup.sh

# Update deployment
./scripts/update.sh
```

## API Documentation

### Base URL

- **Development:** `http://localhost:8080`
- **Production:** `https://your-domain.com`

### Endpoints

#### POST /api/detection/detect

Detects objects in an uploaded image using YOLOv8 ML model.

**Request:**

```http
POST /api/detection/detect HTTP/1.1
Content-Type: multipart/form-data

file: [binary image data]
```

**Supported Formats:** JPEG, PNG
**Maximum Size:** 10 MB
**Recommended Size:** 150-300 KB (compressed)

**Response:**

```json
{
  "detections": [
    {
      "x": 320.5,
      "y": 240.3,
      "width": 150.2,
      "height": 200.1,
      "confidence": 0.89,
      "classId": 381,
      "className": "Person"
    }
  ],
  "processingTimeMs": 450,
  "success": true,
  "errorMessage": null
}
```

**Status Codes:**
- `200 OK` - Successful detection
- `400 Bad Request` - Invalid file format or size
- `500 Internal Server Error` - Processing error

#### GET /api/detection/health

Health check endpoint for monitoring service availability.

**Response:**

```json
{
  "status": "Healthy",
  "timestamp": "2025-10-24T12:00:00Z",
  "modelLoaded": true
}
```

**Status Codes:**
- `200 OK` - Service is healthy
- `503 Service Unavailable` - Service is down or model not loaded

### Interactive API Documentation

**Swagger UI:** Available at `/swagger` endpoint

- **Development:** http://localhost:5000/swagger
- **Production:** https://your-domain.com/swagger

## Project Structure

```
BlackJackCamera/
├── 📄 README.md                          # This file
├── 📄 ARCHITECTURE.md                    # Detailed architecture documentation
├── 📄 QUICKSTART.md                      # Quick start guide
├── 📄 DEPLOYMENT_UBUNTU_24.04.md         # Ubuntu deployment guide
├── 📄 DEPLOYMENT_CHECKLIST.md            # Deployment verification
├── 📄 DEPLOYMENT_DOCKERHUB.md            # Docker Hub deployment
├── 📄 MIGRATION_GUIDE.md                 # Architecture migration guide
├── 📄 DOWNLOAD_MODEL.md                  # ML model download guide
├── 📄 CLAUDE.md                          # AI assistant guidance
│
├── 📁 BlackJackCamera.Api/               # Backend Web API
│   ├── 📁 Controllers/
│   │   └── DetectionController.cs        # API endpoints
│   ├── 📁 Services/
│   │   ├── ModelLoaderService.cs         # ONNX model loader
│   │   ├── ObjectDetectionService.cs     # YOLOv8 inference
│   │   └── ImageProcessor.cs             # Image preprocessing
│   ├── 📁 Interfaces/
│   │   ├── IModelLoaderService.cs
│   │   ├── IObjectDetectionService.cs
│   │   └── IImageProcessor.cs
│   ├── 📁 Models/
│   │   ├── Detection.cs                  # Detection model
│   │   └── DetectionResponse.cs          # API response model
│   ├── 📁 Resources/Models/
│   │   ├── yolov8x-oiv7.onnx            # YOLOv8 ML model (275 MB, Git LFS)
│   │   └── labels.txt                    # 601 class labels (JSON)
│   ├── 📄 Dockerfile                     # Docker image config
│   ├── 📄 Program.cs                     # API startup & DI
│   └── 📄 BlackJackCamera.Api.csproj     # Project file
│
├── 📁 BlackJackCamera/                   # Frontend MAUI App
│   ├── 📁 Models/
│   │   ├── DetectionDto.cs               # Detection DTO
│   │   └── DetectionResponseDto.cs       # API response DTO
│   ├── 📁 Services/
│   │   ├── DetectionApiService.cs        # HTTP client for API
│   │   ├── ImageCompressionService.cs    # Image optimization
│   │   ├── CategoryBadgeMapper.cs        # Object → Banking mapping
│   │   └── JsonContext.cs                # JSON serialization
│   ├── 📁 Interfaces/
│   │   ├── IDetectionApiService.cs
│   │   └── IImageCompressionService.cs
│   ├── 📁 Pages/
│   │   ├── MainPage.xaml                 # Home screen
│   │   ├── MainPage.xaml.cs
│   │   ├── CameraPage.xaml               # Camera & detection UI
│   │   └── CameraPage.xaml.cs
│   ├── 📁 Resources/
│   │   ├── 📁 Images/                    # UI images & icons
│   │   ├── 📁 Fonts/                     # Custom fonts
│   │   ├── 📁 AppIcon/                   # Application icon
│   │   ├── 📁 Splash/                    # Splash screen
│   │   └── 📁 Styles/                    # XAML styles
│   ├── 📁 Platforms/                     # Platform-specific code
│   │   ├── 📁 Android/
│   │   ├── 📁 iOS/
│   │   ├── 📁 MacCatalyst/
│   │   ├── 📁 Windows/
│   │   └── 📁 Tizen/
│   ├── 📄 App.xaml                       # App-level resources
│   ├── 📄 AppShell.xaml                  # Navigation shell
│   ├── 📄 MauiProgram.cs                 # DI container
│   ├── 📄 appsettings.json               # Configuration
│   └── 📄 BlackJackCamera.csproj         # Project file
│
├── 📁 nginx/
│   └── blackjackcamera.conf              # Nginx SSL/TLS config
│
├── 📁 scripts/
│   ├── monitor.sh                        # Monitoring dashboard
│   ├── backup.sh                         # Backup automation
│   ├── update.sh                         # Update deployment
│   └── download-model.sh                 # Download ML model
│
├── 📄 docker-compose.yml                 # Docker orchestration
├── 📄 deploy-vps.sh                      # VPS deployment script
├── 📄 .gitattributes                     # Git LFS configuration
└── 📄 BlackJackCamera.sln                # Visual Studio solution
```

## Technology Stack

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 9.0 | Web API framework |
| **ASP.NET Core** | 9.0 | RESTful API |
| **ONNX Runtime** | 1.22.0 | ML model inference |
| **SkiaSharp** | 3.119.0 | Image processing |
| **Swashbuckle** | 7.2.0 | API documentation |
| **Docker** | Latest | Containerization |
| **Ubuntu** | 24.04 LTS | Server OS |

### Frontend

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET MAUI** | 9.0 | Cross-platform UI |
| **CommunityToolkit.Maui** | 12.2.0 | UI components |
| **CommunityToolkit.Maui.Camera** | 3.0.2 | Camera integration |
| **SkiaSharp** | 3.119.0 | Image compression |
| **HttpClient** | 9.0 | Network communication |

### Machine Learning

| Component | Details |
|-----------|---------|
| **Model** | YOLOv8x |
| **Dataset** | Open Images V7 |
| **Format** | ONNX |
| **Size** | 275 MB |
| **Classes** | 601 objects |
| **Input Size** | 640x640 |
| **Framework** | ONNX Runtime |

### DevOps

| Tool | Purpose |
|------|---------|
| **Docker** | Containerization |
| **Docker Compose** | Multi-container orchestration |
| **Nginx** | Reverse proxy & SSL termination |
| **Let's Encrypt** | SSL certificates |
| **Git LFS** | Large file storage (ML model) |
| **GitHub Actions** | CI/CD (optional) |

## Performance

### Backend Performance

**Tested on: 8 vCPU / 12 GB RAM VPS**

| Metric | Value |
|--------|-------|
| **Processing Time** | 200-400 ms per image |
| **Concurrent Requests** | 50+ simultaneous |
| **Throughput** | 150-250 images/minute |
| **Memory Usage** | 2-4 GB (including ML model) |
| **CPU Usage** | 40-60% (6 cores allocated) |
| **Cold Start** | ~15 seconds (model loading) |

### Network Optimization

| Stage | Original | Optimized | Reduction |
|-------|----------|-----------|-----------|
| **Camera Output** | 2-5 MB | - | - |
| **After Compression** | - | 150-300 KB | **10-25x** |
| **API Response** | - | 5-50 KB | - |
| **Total Upload** | 2-5 MB | ~200 KB | **92-96%** |

### Optimization Strategies

**Frontend:**
- JPEG compression (75% quality)
- Automatic image resizing (max 1920x1080)
- HttpClient connection pooling
- Async/await for non-blocking operations
- Lazy service initialization

**Backend:**
- Singleton ONNX session (load model once)
- Scoped image processor (reuse per request)
- Non-Maximum Suppression (NMS) for deduplication
- Server GC mode for high throughput
- Docker resource limits

## Security

### Current Security Measures

✅ **Network Security**
- CORS configuration for mobile clients
- HTTPS/SSL support via Nginx
- Docker network isolation
- UFW firewall configuration

✅ **Input Validation**
- File size limits (10 MB maximum)
- File type validation (JPEG/PNG only)
- Request timeout (30 seconds)
- Image format verification

✅ **Container Security**
- No new privileges (`no-new-privileges:true`)
- Dropped capabilities (`CAP_DROP: ALL`)
- Minimal capabilities (`NET_BIND_SERVICE` only)
- Non-root user execution

✅ **Operational Security**
- Structured logging
- Health check monitoring
- Automated restart on failure
- Log rotation (100 MB, 5 files)

### Recommended for Production

🔒 **Authentication & Authorization**
- [ ] JWT token authentication
- [ ] API key management
- [ ] Rate limiting per user/IP
- [ ] OAuth 2.0 integration

🔒 **Enhanced Security**
- [ ] Input sanitization for file metadata
- [ ] Antivirus scanning for uploads
- [ ] DDoS protection (Cloudflare)
- [ ] Web Application Firewall (WAF)
- [ ] Security headers (CSP, HSTS, etc.)

🔒 **Monitoring & Auditing**
- [ ] Centralized logging (ELK stack)
- [ ] Security event monitoring
- [ ] Intrusion detection system (IDS)
- [ ] Regular security audits

## Development

### Setting Up Development Environment

1. **Install Prerequisites**

```bash
# .NET 9.0 SDK
winget install Microsoft.DotNet.SDK.9

# Visual Studio 2022 (Windows)
# OR Visual Studio Code with C# extension

# Android SDK (for mobile development)
# Via Visual Studio Installer
```

2. **Clone Repository**

```bash
git clone https://github.com/your-org/BlackJackCamera.git
cd BlackJackCamera
git lfs install
git lfs pull
```

3. **Open in IDE**

```bash
# Visual Studio
start BlackJackCamera/BlackJackCamera.sln

# VS Code
code BlackJackCamera
```

### Development Workflow

**Backend Development:**

```bash
cd BlackJackCamera/BlackJackCamera.Api
dotnet watch run  # Auto-reload on changes

# Run tests
dotnet test
```

**Frontend Development:**

```bash
cd BlackJackCamera/BlackJackCamera

# Run on Android emulator
dotnet build -t:Run -f net9.0-android

# Run on Windows (Hot Reload enabled)
dotnet watch -t:Run -f net9.0-windows10.0.19041.0
```

### Code Style

This project follows [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

**Key Guidelines:**
- Use `async`/`await` for I/O operations
- Dependency Injection for services
- Interface-based design
- XML documentation for public APIs
- Nullable reference types enabled

### Adding New Features

**New API Endpoint:**
1. Add method in `DetectionController.cs`
2. Update Swagger documentation
3. Add corresponding method in `DetectionApiService.cs` (frontend)
4. Update DTOs if needed

**New Banking Service Mapping:**
1. Edit `CategoryBadgeMapper.cs`
2. Add new category constant
3. Map YOLO class IDs to category
4. Update UI in `CameraPage.xaml.cs`

**Updating ML Model:**
1. Replace `yolov8x-oiv7.onnx` in `Resources/Models/`
2. Update `labels.txt` if classes changed
3. Rebuild Docker image: `docker-compose build`
4. Restart: `docker-compose up -d`

### Testing

**Unit Testing Backend:**

```bash
cd BlackJackCamera/BlackJackCamera.Api.Tests
dotnet test --verbosity normal
```

**Manual API Testing:**

Use `test-api.http` file with REST Client extension (VS Code):

```http
### Health Check
GET http://localhost:8080/api/detection/health

### Detect Objects
POST http://localhost:8080/api/detection/detect
Content-Type: multipart/form-data; boundary=boundary

--boundary
Content-Disposition: form-data; name="file"; filename="test.jpg"

< ./test-images/laptop.jpg
--boundary--
```

## Troubleshooting

### Common Issues

#### Backend doesn't start

**Symptom:** Docker container exits immediately

**Solutions:**
```bash
# Check model file exists
ls -lh BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx

# View container logs
docker-compose logs blackjackcamera-api

# Verify port not in use
netstat -tuln | grep 8080

# Check disk space
df -h
```

#### Frontend can't connect to API

**Symptom:** Network errors, connection refused

**Solutions:**
```json
// For Android Emulator (appsettings.json)
{
  "ApiSettings": {
    "BaseUrl": "http://10.0.2.2:8080"
  }
}

// For physical device (same network)
{
  "ApiSettings": {
    "BaseUrl": "http://192.168.1.100:8080"  // Your PC IP
  }
}
```

```bash
# Test API accessibility
curl http://YOUR_IP:8080/api/detection/health

# Check firewall
sudo ufw status
sudo ufw allow 8080/tcp
```

#### Slow processing times

**Symptom:** Detection takes >5 seconds

**Solutions:**
```bash
# Check image size in app logs (should be ~200KB)
# Verify compression settings in appsettings.json

# Increase Docker resources
# Edit docker-compose.yml:
deploy:
  resources:
    limits:
      cpus: '8'
      memory: 12G

# Monitor server resources
docker stats blackjackcamera-api
htop
```

#### Model not found

**Symptom:** "Model file not found" error

**Solutions:**
```bash
# Download model via Git LFS
git lfs install
git lfs pull

# Verify file
file BlackJackCamera/Resources/Raw/yolov8x-oiv7.onnx
# Output should show: data (binary file)

# Manual download
cd scripts
./download-model.sh
```

See [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md) for comprehensive troubleshooting.

## Contributing

We welcome contributions! Please follow these guidelines:

### Reporting Issues

1. Check existing issues first
2. Use issue templates
3. Provide detailed reproduction steps
4. Include logs and screenshots

### Pull Requests

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

### Code Review Process

- All PRs require review
- CI/CD checks must pass
- Follow code style guidelines
- Update documentation
- Add tests for new features

## Documentation

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Detailed system architecture
- **[DEPLOYMENT_UBUNTU_24.04.md](DEPLOYMENT_UBUNTU_24.04.md)** - Complete deployment guide
- **[DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md)** - Deployment verification
- **[QUICKSTART.md](QUICKSTART.md)** - Quick start guide
- **[MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)** - Architecture migration notes
- **[DOWNLOAD_MODEL.md](DOWNLOAD_MODEL.md)** - ML model management

## Roadmap

### Planned Features

**Backend:**
- [ ] Redis caching for frequent detections
- [ ] WebSocket support for real-time streaming
- [ ] Batch processing for multiple images
- [ ] GPU acceleration (CUDA/TensorRT)
- [ ] Model versioning and A/B testing

**Frontend:**
- [ ] Offline mode with TensorFlow Lite (optional)
- [ ] Progressive image compression
- [ ] Result caching
- [ ] Retry mechanism with exponential backoff
- [ ] Multi-language support (i18n)

**DevOps:**
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Kubernetes deployment manifests
- [ ] Prometheus + Grafana monitoring
- [ ] Automated testing suite
- [ ] Blue-green deployments

## License

This project is proprietary software owned by BlackJackCamera Team.

**Copyright © 2024 BlackJackCamera Team. All rights reserved.**

Unauthorized copying, distribution, or modification of this software is strictly prohibited.

## Support

- **Issues:** [GitHub Issues](https://github.com/your-org/BlackJackCamera/issues)
- **Discussions:** [GitHub Discussions](https://github.com/your-org/BlackJackCamera/discussions)
- **Email:** support@blackjackcamera.com

## Acknowledgments

- **YOLOv8** by [Ultralytics](https://github.com/ultralytics/ultralytics)
- **Open Images V7** by [Google Research](https://storage.googleapis.com/openimages/web/index.html)
- **.NET MAUI** by [Microsoft](https://dotnet.microsoft.com/apps/maui)
- **ONNX Runtime** by [Microsoft](https://onnxruntime.ai/)

---

**Built with ❤️ using .NET 9.0 and YOLOv8**

*Last Updated: October 2024*
