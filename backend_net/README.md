# Backend API - .NET 8.0

## 🎯 Quick Start

### Local Development (Docker + Shared VPS Database)

1. **Start SSH Tunnel** (Terminal 1):
   ```powershell
   ssh -L 1433:localhost:1433 root@72.61.248.96 -N
   ```

2. **Start Docker Container** (Terminal 2):
   ```powershell
   docker-compose up -d
   ```

3. **Run Migrations** (First Time):
   ```powershell
   docker-compose exec backend_api dotnet ef database update
   ```

4. **Access API**: http://localhost:8080/swagger

### Production (Docker + Isolated Database)

```powershell
docker-compose -f docker-compose.prod.yml up -d
docker-compose -f docker-compose.prod.yml exec backend_api dotnet ef database update
```

---

## 📚 Documentation

- **`DEVELOPMENT_WORKFLOW.md`** - Complete development workflow guide
- **`SETUP_GUIDE.md`** - VPS SQL Server setup with SSH tunnel
- **`DOCKER_SETUP.md`** - Docker-only setup (alternative)
- **`TROUBLESHOOTING.md`** - Common issues and solutions

---

## 🏗️ Architecture

### Development
- **Backend API**: Docker container
- **Database**: Shared VPS database (via SSH tunnel)
- **Access**: All developers share same database

### Production
- **Backend API**: Docker container
- **Database**: Docker container (isolated)
- **Access**: Only production API can access

---

## ⚠️ Important Notes

- **No Auto-Sync**: Development and Production databases are completely separate
- **SSH Tunnel Required**: Must be running for local development
- **Docker Required**: Backend API runs in Docker for both dev and prod

---

## 🔧 Configuration

- **Development**: `appsettings.Development.json` + `docker-compose.yml`
- **Production**: `appsettings.Production.json` + `docker-compose.prod.yml`

---

## 📝 Environment Variables

### Development
- `ASPNETCORE_ENVIRONMENT=Development`
- Connection string points to shared VPS database

### Production
- `ASPNETCORE_ENVIRONMENT=Production`
- Connection string points to Docker SQL Server
- `SA_PASSWORD` must be set securely

---

## 🚀 API Endpoints

- **Swagger UI**: `/swagger`
- **Health Check**: `/health`
- **API Base**: `/api`

---

## 📦 Technologies

- .NET 8.0
- Entity Framework Core
- SQL Server
- Docker
- Serilog (Logging)
- FluentValidation
- JWT Authentication
